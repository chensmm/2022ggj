using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.CorgiEngine
{
    public class HitscanWeapon : Weapon
    {
        /// the possible modes this weapon laser sight can run on, 3D by default
        public enum Modes { TwoD, ThreeD }

        [MMInspectorGroup("Hitscan Spawn", true, 41)]

        /// the offset position at which the projectile will spawn
        [Tooltip("the offset position at which the projectile will spawn")]
        public Vector3 ProjectileSpawnOffset = Vector3.zero;
        /// the spread (in degrees) to apply randomly (or not) on each angle when spawning a projectile
        [Tooltip("the spread (in degrees) to apply randomly (or not) on each angle when spawning a projectile")]
        public Vector3 Spread = Vector3.zero;
        /// whether or not the weapon should rotate to align with the spread angle
        [Tooltip("whether or not the weapon should rotate to align with the spread angle")]
        public bool RotateWeaponOnSpread = false;
        /// whether or not the spread should be random (if not it'll be equally distributed)
        [Tooltip("whether or not the spread should be random (if not it'll be equally distributed)")]
        public bool RandomSpread = true;
        /// the projectile's spawn position
        [MMReadOnly]
        [Tooltip("the projectile's spawn position")]
        public Vector3 SpawnPosition = Vector3.zero;

        [MMInspectorGroup("Hitscan Damage", true, 42)]

        /// the layer(s) on which to hitscan ray should collide
        [Tooltip("the layer(s) on which to hitscan ray should collide")]
        public LayerMask HitscanTargetLayers;
        /// the maximum distance of this weapon, after that bullets will be considered lost
        [Tooltip("the maximum distance of this weapon, after that bullets will be considered lost")]
        public float HitscanMaxDistance = 100f;
        /// the amount of damage to apply to a damageable (something with a Health component) every time there's a hit
        [Tooltip("the amount of damage to apply to a damageable (something with a Health component) every time there's a hit")]
        public int DamageCaused = 5;
        /// the duration of the invincibility after a hit (to prevent insta death in the case of rapid fire)
        [Tooltip("the duration of the invincibility after a hit (to prevent insta death in the case of rapid fire)")]
        public float DamageCausedInvincibilityDuration = 0.2f;

        [MMInspectorGroup("Hitscan OnHit", true, 43)]

        /// a particle system to move to the position of the hit and to play when hitting something with a Health component
        [Tooltip("a particle system to move to the position of the hit and to play when hitting something with a Health component")]
        public ParticleSystem DamageableImpactParticles;

        /// a particle system to move to the position of the hit and to play when hitting something without a Health component
        [Tooltip("a particle system to move to the position of the hit and to play when hitting something without a Health component")]
        public ParticleSystem NonDamageableImpactParticles;

        protected Vector3 _damageDirection;
        protected Vector3 _flippedProjectileSpawnOffset;
        protected Vector3 _randomSpreadDirection;
        protected Transform _projectileSpawnTransform;
        public RaycastHit _hit { get; protected set; }
        public RaycastHit2D _hit2D { get; protected set; }
        public Vector3 _origin { get; protected set; }
        protected Vector3 _destination;
        protected Vector3 _direction;
        protected GameObject _hitObject = null;
        protected Vector3 _hitPoint;
        protected Health _health;

        [MMInspectorButton("TestShoot")]
        /// a button to test the shoot method
		public bool TestShootButton;

        public int direction = 1;//方向，1=X，-1=Y
        public int toUseScale = 0;
        public int curScale = 0;
        public int scaleMax = 100;
        public int scaleMin = 0;
        public int scaleAdd = 1;
        public int scaleReduce = -1;

        bool isPressing;
        Vector3 mouseOriginPosInWorld;
        Vector3 touchPos;
        Ray ray;
        RaycastHit2D hit1;

        private void Start()
        {
            if (GUIManager.Instance != null)
            {
                GUIManager.Instance.UpdateJetpackBar(toUseScale, 0f, scaleMax, "Player1", new Color(32, 214, 250, 221));
                GUIManager.Instance.UpdateHealthBar(curScale == 0 ? 0.1f : curScale, 0f, scaleMax, "Player1");
                GUIManager.Instance.ScaleToUse.text = "ToUse:" + toUseScale;
                GUIManager.Instance.MagazineUI.text = curScale + "/" + scaleMax;
            }
        }

        override protected void Update()
        {
            base.Update();
            //float scrollWheel = Input.mouseScrollDelta.y;
            //if (Input.GetKeyUp(KeyCode.Q))
            //{
            //    direction *= -1;

            //    if (direction == 1)
            //    {
            //        if (GUIManager.Instance != null)
            //        {
            //            GUIManager.Instance.ChangeDirection.text = "X";
            //        }
            //    }
            //    else if (direction == -1)
            //    {
            //        if (GUIManager.Instance != null)
            //        {
            //            GUIManager.Instance.ChangeDirection.text = "Y";
            //        }
            //    }
            //    Debug.Log("Direction=" + direction);
            //}

            //if (Input.GetKey(KeyCode.E))
            //{
            //    Time.timeScale = 0.3f;
            //}
            //else
            //{
            //    Time.timeScale = 1f;
            //}

            if (Input.GetMouseButtonDown(0))
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                hit1 = Physics2D.Raycast(new Vector2(ray.origin.x, ray.origin.y), Vector2.zero, Mathf.Infinity, HitscanTargetLayers);
                if (hit1.transform != null && hit1.transform.GetComponent<RHTransformController>())
                {
                    isPressing = true;
                    mouseOriginPosInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Time.timeScale = 0.3f;
                    //GameController.Instance.SetMsg(new MirageMsg(hit1.transform.gameObject, new Vector2(direction == 1 ? toUseScale : 0, direction == -1 ? toUseScale : 0), hit1.point));
                }
            }
            if (Input.GetMouseButton(0))
            {
                if (isPressing)
                {
                    var newMousePosInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    var direction2 = mouseOriginPosInWorld - newMousePosInWorld;
                    touchPos = hit1.transform.InverseTransformPoint(mouseOriginPosInWorld);
                    //aDebug.Log(direction2);
                    if (Mathf.Abs(direction2.x) > Mathf.Abs(direction2.y))
                    {
                        direction = 1;
                        bool isleft = touchPos.x < 0;
                        if (isleft)
                        {
                            toUseScale = (int)direction2.x / 1;
                        }
                        else
                        {
                            toUseScale = -(int)direction2.x / 1;
                        }
                    }
                    else
                    {
                        direction = -1;
                        bool isdown = touchPos.y < 0;
                        if (isdown)
                            toUseScale = (int)direction2.y / 1;
                        else
                            toUseScale = -(int)direction2.y / 1;
                    }
                    if (toUseScale > curScale)
                    {
                        toUseScale /= Mathf.Abs(toUseScale);
                        toUseScale *= curScale;
                    }
                    GameController.Instance.SetMsg(new MirageMsg(hit1.transform.gameObject, new Vector2(direction == 1 ? toUseScale : 0, direction == -1 ? toUseScale : 0), touchPos));
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (isPressing)
                {
                    isPressing = false;
                    Time.timeScale = 1f;

                    if (hit1.transform.GetComponent<RHTransformController>().TranformOrder(new Vector2(direction == 1 ? toUseScale : 0, direction == -1 ? toUseScale : 0), touchPos))
                    {
                        curScale -= toUseScale;

                        if (GUIManager.Instance != null)
                        {
                            GUIManager.Instance.UpdateHealthBar(curScale, 0f, scaleMax, "Player1");
                            GUIManager.Instance.MagazineUI.text = curScale + "/" + scaleMax;
                        }

                        if (toUseScale > curScale)
                        {
                            toUseScale = curScale;
                            GUIManager.Instance.ScaleToUse.text = "ToUse:" + toUseScale;
                            if (GUIManager.Instance != null)
                            {
                                if (toUseScale < 0)
                                {
                                    GUIManager.Instance.UpdateJetpackBar(-toUseScale, 0f, scaleMax, "Player1", Color.red);
                                }
                                else
                                {
                                    GUIManager.Instance.UpdateJetpackBar(toUseScale, 0f, scaleMax, "Player1", new Color(32, 214, 250, 221));
                                }

                            }

                            Debug.Log("Scale=" + toUseScale);
                        }
                        if (toUseScale < curScale - scaleMax)
                        {
                            toUseScale = curScale - scaleMax;
                            GUIManager.Instance.ScaleToUse.text = "ToUse:" + toUseScale;
                            if (GUIManager.Instance != null)
                            {
                                if (toUseScale < 0)
                                {
                                    GUIManager.Instance.UpdateJetpackBar(-toUseScale, 0f, scaleMax, "Player1", Color.red);
                                }
                                else
                                {
                                    GUIManager.Instance.UpdateJetpackBar(toUseScale, 0f, scaleMax, "Player1", new Color(32, 214, 250, 221));
                                }
                            }
                            Debug.Log("Scale=" + toUseScale);
                        }
                    }

                }
            }

            //if (scrollWheel > 0.25f)
            //{
            //    //向上滚滑轮
            //    if (toUseScale + scaleAdd <= curScale)
            //    {
            //        toUseScale += scaleAdd;
            //        GUIManager.Instance.ScaleToUse.text = "ToUse:" + toUseScale;
            //        if (GUIManager.Instance != null)
            //        {
            //            if (toUseScale < 0)
            //            {
            //                GUIManager.Instance.UpdateJetpackBar(-toUseScale, 0f, scaleMax, "Player1", Color.red);
            //            }
            //            else
            //            {
            //                GUIManager.Instance.UpdateJetpackBar(toUseScale, 0f, scaleMax, "Player1", new Color(32, 214, 250, 221));
            //            }
            //        }
            //        Debug.Log("Scale=" + toUseScale);
            //    }
            //}
            //else if (scrollWheel < -0.25f)
            //{
            //    //向下滚滑轮
            //    if (toUseScale + scaleReduce >= curScale - scaleMax)
            //    {
            //        toUseScale += scaleReduce;
            //        GUIManager.Instance.ScaleToUse.text = "ToUse:" + toUseScale;
            //        if (GUIManager.Instance != null)
            //        {
            //            if(toUseScale<0)
            //            {
            //                GUIManager.Instance.UpdateJetpackBar(-toUseScale, 0f, scaleMax, "Player1",Color.red);
            //            }
            //            else
            //            {
            //                GUIManager.Instance.UpdateJetpackBar(toUseScale, 0f, scaleMax, "Player1", new Color(32, 214, 250, 221));
            //            }
            //        }
            //        Debug.Log("Scale=" + toUseScale);
            //    }
            //}

            //Vector3 mousePos = Input.mousePosition;
            //Vector3 mousePosInWorld = Camera.main.ScreenToWorldPoint(mousePos);
            //DetermineSpawnPosition();
            //DetermineDirection();
            //RaycastHit2D hit;
            //hit = MMDebug.RayCast(SpawnPosition, _randomSpreadDirection, HitscanMaxDistance, HitscanTargetLayers, Color.red, true);
            ////Debug.Log(hit.transform.name);

            //if (hit.transform != null && hit.transform.GetComponent<RHTransformController>())
            //{
            //    GameController.Instance.SetMsg(new MirageMsg(hit.transform.gameObject, new Vector2(direction == 1 ? toUseScale : 0, direction == -1 ? toUseScale : 0), hit.point));
            //}
        }

        /// <summary>
        /// A test method that triggers the weapon
        /// </summary>
        protected virtual void TestShoot()
        {
            if (WeaponState.CurrentState == WeaponStates.WeaponIdle)
            {
                WeaponInputStart();
            }
            else
            {
                WeaponInputStop();
            }
        }

        /// <summary>
        /// Initialize this weapon
        /// </summary>
        public override void Initialization()
        {
            base.Initialization();
            _aimableWeapon = GetComponent<WeaponAim>();
            if (FlipWeaponOnCharacterFlip)
            {
                _flippedProjectileSpawnOffset = ProjectileSpawnOffset;
                _flippedProjectileSpawnOffset.y = -_flippedProjectileSpawnOffset.y;
            }
        }

        /// <summary>
		/// Called everytime the weapon is used
		/// </summary>
		protected override void WeaponUse()
        {
            base.WeaponUse();

            DetermineSpawnPosition();
            DetermineDirection();
            SpawnProjectile(SpawnPosition, true);
            HandleDamage();
        }

        /// <summary>
        /// Determines the direction of the ray we have to cast
        /// </summary>
        protected virtual void DetermineDirection()
        {
            _direction = Flipped ? -transform.right : transform.right;
            if (RandomSpread)
            {
                _randomSpreadDirection = MMMaths.RandomVector3(-Spread, Spread);
                Quaternion spread = Quaternion.Euler(_randomSpreadDirection);
                _randomSpreadDirection = spread * _direction;
                if (RotateWeaponOnSpread)
                {
                    this.transform.rotation = this.transform.rotation * spread;
                }
            }
            else
            {
                _randomSpreadDirection = _direction;
            }
        }

        /// <summary>
        /// Spawns a new object and positions/resizes it
        /// </summary>
        public virtual void SpawnProjectile(Vector3 spawnPosition, bool triggerObjectActivation = true)
        {
            if (curScale - toUseScale >= 0)
            {
                _hitObject = null;

                // we cast a ray in front of the weapon to detect an obstacle
                _origin = SpawnPosition;

                _hit2D = MMDebug.RayCast(_origin, _randomSpreadDirection, HitscanMaxDistance, HitscanTargetLayers, Color.red, true);
                if (_hit2D)
                {
                    _hitObject = _hit2D.collider.gameObject;
                    _hitPoint = _hit2D.point;

                    //if (_hitObject.GetComponent<RHTransformController>())
                    //{
                    //    if (_hitObject.GetComponent<RHTransformController>().TranformOrder(new Vector2(direction == 1 ? toUseScale : 0, direction == -1 ? toUseScale : 0), _hitPoint))
                    //    {
                    //        curScale -= toUseScale;

                    //        if (GUIManager.Instance != null)
                    //        {
                    //            GUIManager.Instance.UpdateHealthBar(curScale, 0f, scaleMax, "Player1");
                    //            GUIManager.Instance.MagazineUI.text = curScale + "/" + scaleMax;
                    //        }

                    //        if (toUseScale > curScale)
                    //        {
                    //            toUseScale = curScale;
                    //            GUIManager.Instance.ScaleToUse.text = "ToUse:" + toUseScale;
                    //            if (GUIManager.Instance != null)
                    //            {
                    //                if (toUseScale < 0)
                    //                {
                    //                    GUIManager.Instance.UpdateJetpackBar(-toUseScale, 0f, scaleMax, "Player1", Color.red);
                    //                }
                    //                else
                    //                {
                    //                    GUIManager.Instance.UpdateJetpackBar(toUseScale, 0f, scaleMax, "Player1", new Color(32, 214, 250, 221));
                    //                }

                    //            }

                    //            Debug.Log("Scale=" + toUseScale);
                    //        }
                    //        if (toUseScale < curScale - scaleMax)
                    //        {
                    //            toUseScale = curScale - scaleMax;
                    //            GUIManager.Instance.ScaleToUse.text = "ToUse:" + toUseScale;
                    //            if (GUIManager.Instance != null)
                    //            {
                    //                if (toUseScale < 0)
                    //                {
                    //                    GUIManager.Instance.UpdateJetpackBar(-toUseScale, 0f, scaleMax, "Player1", Color.red);
                    //                }
                    //                else
                    //                {
                    //                    GUIManager.Instance.UpdateJetpackBar(toUseScale, 0f, scaleMax, "Player1", new Color(32, 214, 250, 221));
                    //                }
                    //            }
                    //            Debug.Log("Scale=" + toUseScale);
                    //        }
                    //    }
                    //}
                }
                // otherwise we just draw our laser in front of our weapon 
                else
                {
                    _hitObject = null;
                    // we play the miss feedback
                    WeaponMiss();
                }
            }

        }

        /// <summary>
        /// Handles damage and the associated feedbacks
        /// </summary>
        protected virtual void HandleDamage()
        {
            if (_hitObject == null)
            {
                return;
            }

            WeaponHit();



            _health = _hitObject.MMGetComponentNoAlloc<Health>();

            if (_health == null)
            {
                // hit non damageable
                if (WeaponOnHitNonDamageableFeedback != null)
                {
                    WeaponOnHitNonDamageableFeedback.transform.position = _hitPoint;
                    WeaponOnHitNonDamageableFeedback.transform.LookAt(this.transform);
                }

                if (NonDamageableImpactParticles != null)
                {
                    NonDamageableImpactParticles.transform.position = _hitPoint;
                    NonDamageableImpactParticles.transform.LookAt(this.transform);
                    NonDamageableImpactParticles.Play();
                }

                WeaponHitNonDamageable();
            }
            else
            {
                // hit damageable
                _damageDirection = (_hitObject.transform.position - this.transform.position).normalized;
                _health.Damage(DamageCaused, this.gameObject, DamageCausedInvincibilityDuration, DamageCausedInvincibilityDuration, _damageDirection);
                if (_health.CurrentHealth <= 0)
                {
                    WeaponKill();
                }

                if (WeaponOnHitDamageableFeedback != null)
                {
                    WeaponOnHitDamageableFeedback.transform.position = _hitPoint;
                    WeaponOnHitDamageableFeedback.transform.LookAt(this.transform);
                }

                if (DamageableImpactParticles != null)
                {
                    DamageableImpactParticles.transform.position = _hitPoint;
                    DamageableImpactParticles.transform.LookAt(this.transform);
                    DamageableImpactParticles.Play();
                }

                WeaponHitDamageable();
            }

        }

        /// <summary>
        /// Determines the spawn position based on the spawn offset and whether or not the weapon is flipped
        /// </summary>
        public virtual void DetermineSpawnPosition()
        {
            if (Flipped)
            {
                if (FlipWeaponOnCharacterFlip)
                {
                    SpawnPosition = this.transform.position - this.transform.rotation * _flippedProjectileSpawnOffset;
                }
                else
                {
                    SpawnPosition = this.transform.position - this.transform.rotation * ProjectileSpawnOffset;
                }
            }
            else
            {
                SpawnPosition = this.transform.position + this.transform.rotation * ProjectileSpawnOffset;
            }
        }

        /// <summary>
        /// When the weapon is selected, draws a circle at the spawn's position
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            DetermineSpawnPosition();

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(SpawnPosition, 0.2f);
        }

    }
}
