using UnityEngine;
using DG.Tweening;
using System.Collections;

namespace MoreMountains.CorgiEngine
{
    enum toPos
    {
        none,
        toleft,
        toright,
        totop,
        tobottom,
    }

    internal class RHTransformController : MonoBehaviour
    {
        bool isTransforming;

        toPos cur2Pos;

        float forceNum;

        int c = 5;//向上

        int d = 20; //左右

        float transformTime = 0.3f; //变形时长

        /// <summary>
        /// 
        /// </summary>
        /// <param name="changeVec">正值是增长，负值缩短</param>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool TranformOrder(Vector3 changeVec, Vector2 point)
        {
            if (isTransforming)
            {
                return false;
            }

            var finalVec = transform.localScale + changeVec;
            if (transform.gameObject.layer != 15)
            {
                if (finalVec.x < 0.999 || finalVec.y < 0.999)
                {

                    return false;
                }
            }

            isTransforming = true;

            var localHitPoint = point;
            //var localHitPoint = transform.InverseTransformPoint(point);
            Vector2 parentPos = Vector2.zero;
            if (changeVec.x != 0)
            {

                bool isLeft = localHitPoint.x < 0;
                if (isLeft)
                {
                    parentPos = new Vector2(transform.position.x + transform.localScale.x / 2, transform.position.y);
                    if (changeVec.x > 0)
                    {
                        cur2Pos = toPos.toleft;
                        forceNum = changeVec.x;
                    }
                }
                else
                {
                    parentPos = new Vector2(transform.position.x - transform.localScale.x / 2, transform.position.y);
                    if (changeVec.x > 0)
                    {
                        cur2Pos = toPos.toright;
                        forceNum = changeVec.x;
                    }
                }

            }
            else if (changeVec.y != 0)
            {
                bool isdown = localHitPoint.y < 0;
                if (isdown)
                {
                    parentPos = new Vector2(transform.position.x, transform.position.y + transform.localScale.y / 2);
                    if (changeVec.y > 0)
                    {
                        cur2Pos = toPos.tobottom;
                        forceNum = changeVec.y;
                    }
                }
                else
                {
                    parentPos = new Vector2(transform.position.x, transform.position.y - transform.localScale.y / 2);
                    if (changeVec.y > 0)
                    {
                        cur2Pos = toPos.totop;
                        forceNum = changeVec.y;
                    }
                }
            }

            var parentPoint = new GameObject();
            parentPoint.transform.SetParent(GameController.Instance.transform);
            parentPoint.transform.position = parentPos;
            parentPoint.transform.localScale = transform.localScale;
            transform.SetParent(parentPoint.transform);

            parentPoint.transform.DOScale(parentPoint.transform.localScale + changeVec, transformTime).SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    if (isPlayer)
                    {
                        addForce = true;
                        ForcePlayer(player);
                    }
                    transform.SetParent(GameController.Instance.transform);
                    isTransforming = false;
                    cur2Pos = toPos.none;
                    forceNum = 0;
                    Destroy(parentPoint);
                });

            return true;
        }
        public GameObject ShowMirage(MirageMsg msg)
        {
            if (isTransforming)
            {
                return null;
            }
            Vector3 changeVec = msg.changeVec;
            var localHitPoint = msg.point;
            //Vector2 point = msg.point;

            //var localHitPoint = transform.InverseTransformPoint(point);
            Vector2 pos = Vector2.zero;
            Vector3 finalScale = Vector3.zero;

            finalScale = transform.localScale + changeVec;

            if (finalScale.x < 0.999 || finalScale.y < 0.999)
            {
                return null;
            }

            if (changeVec.x != 0)
            {
                bool isLeft = localHitPoint.x < 0;
                if (isLeft)
                {
                    pos = new Vector2(transform.position.x - changeVec.x / 2, transform.position.y);
                }
                else
                {
                    pos = new Vector2(transform.position.x + changeVec.x / 2, transform.position.y);
                }

            }
            else if (changeVec.y != 0)
            {
                bool isdown = localHitPoint.y < 0;
                if (isdown)
                {

                    pos = new Vector2(transform.position.x, transform.position.y - changeVec.y / 2);

                }
                else
                {
                    pos = new Vector2(transform.position.x, transform.position.y + changeVec.y / 2);
                }
            }

            var go = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/MirageGo"));
            go.transform.SetParent(GameController.Instance.transform);
            go.transform.position = pos;
            go.transform.localScale = finalScale;

            return go;
        }

        private bool addForce = false;

        bool isPlayer=false;
        GameObject ChildPoint = new GameObject();
        GameObject player = new GameObject();

        private void OnTriggerEnter2D(Collider2D collision)
        {
            isPlayer = collision.gameObject.tag == "Player";
            if (isPlayer)
            {
                player = collision.gameObject;
                Debug.Log("enter>>>" + collision.gameObject.name);
                ChildPoint = new GameObject();
                ChildPoint.transform.SetParent(GameController.Instance.transform);
                ChildPoint.transform.position = transform.position;
                ChildPoint.transform.localScale = transform.localScale;
            }

            //ForcePlayer(collision);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (isPlayer)
            {
                Debug.Log("stay>>>" + collision.gameObject.name);
                ChildPoint.transform.SetParent(GameController.Instance.transform);
                ChildPoint.transform.position = collision.ClosestPoint(collision.transform.position);

                if(isTransforming&&!addForce)
                {
                    collision.transform.position = ChildPoint.transform.position;
                }
            }
            //ForcePlayer(collision);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            isPlayer = collision.gameObject.tag == "Player";
            if(isPlayer)
            {
                Debug.Log("exit>>>" + collision.gameObject.name);
                Destroy(ChildPoint);
                //if (isPlayer && addForce)
                //{
                //    addForce = false;
                //    StartCoroutine(SpeedZero(collision.GetComponent<Rigidbody2D>()));
                //}
            }
            
        }

        private void ForcePlayer(GameObject collision)
        {
            Vector2 point = collision.transform.position;
            bool isPlayer = collision.gameObject.tag == "Player";
            if (isPlayer)
            {
                if (addForce && isTransforming && cur2Pos != toPos.none)
                {
                    var vec = point - (Vector2)transform.position;

                    if (Mathf.Abs(vec.x / vec.y) > transform.localScale.x / transform.localScale.y)
                    {
                        if (vec.x > 0 && cur2Pos == toPos.toright)
                        {
                            //右边
                            collision.GetComponent<CorgiController>().SetForce(Vector2.zero);
                            collision.GetComponent<CorgiController>().SetForce(Vector2.right * forceNum * d);
                            addForce = false;
                        }
                        else if (vec.x < 0 && cur2Pos == toPos.toleft)
                        {
                            collision.GetComponent<CorgiController>().SetForce(Vector2.zero);
                            collision.GetComponent<CorgiController>().SetForce(Vector2.left * forceNum * d);
                            addForce = false;
                        }
                    }
                    else
                    {
                        if (vec.y > 0 && cur2Pos == toPos.totop)
                        {
                            //上边
                            collision.GetComponent<CorgiController>().SetForce(Vector2.zero);
                            collision.GetComponent<CorgiController>().SetForce(Vector2.up * forceNum * c);
                            addForce = false;
                        }
                        else if (vec.y < 0 && cur2Pos == toPos.tobottom)
                        {
                            //下边
                            collision.GetComponent<CorgiController>().SetForce(Vector2.zero);
                            collision.GetComponent<CorgiController>().SetForce(Vector2.down * forceNum * c);
                            addForce = false;
                        }
                    }
                }
            }
        }

        IEnumerator SpeedZero(Rigidbody2D rb)
        {
            yield return new WaitForSeconds(0.5f);
            rb.velocity = (Vector2.zero);
        }
    }
}