using UnityEngine;
using DG.Tweening;

namespace MoreMountains.CorgiEngine
{
    internal class RHTransformController:MonoBehaviour
    {
        bool isTransforming;

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

            isTransforming = true;

            var localHitPoint = transform.InverseTransformPoint(point);
            Vector2 parentPos = Vector2.zero;

            if (changeVec.x != 0)
            {
                bool isLeft = localHitPoint.x < 0;
                if (isLeft)
                {
                    parentPos = new Vector2(transform.position.x + transform.localScale.x / 2, transform.position.y);
                }
                else
                {
                    parentPos = new Vector2(transform.position.x - transform.localScale.x / 2, transform.position.y);
                }

            }
            else if (changeVec.y != 0)
            {
                bool isdown = localHitPoint.y < 0;
                if (isdown)
                {
                    parentPos = new Vector2(transform.position.x, transform.position.y + transform.localScale.y / 2);
                }
                else
                {
                    parentPos = new Vector2(transform.position.x, transform.position.y - transform.localScale.y / 2);
                }
            }

            var parentPoint = Instantiate<GameObject>(new GameObject(), GameController.Instance.transform);
            parentPoint.transform.position = parentPos;
            parentPoint.transform.localScale = transform.localScale;
            transform.SetParent(parentPoint.transform);

            parentPoint.transform.DOScale(parentPoint.transform.localScale + changeVec, 0.5f)
                .OnComplete(() =>
                {
                    transform.SetParent(GameController.Instance.transform);
                    isTransforming = false;
                });

            return true;
        }
    }

}