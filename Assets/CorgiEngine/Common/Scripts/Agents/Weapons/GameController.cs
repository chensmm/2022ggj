using UnityEngine;

namespace MoreMountains.CorgiEngine
{
    public class MirageMsg 
    {
        public GameObject go;
        public Vector3 changeVec;
        public Vector2 point;

        public MirageMsg(GameObject go, Vector3 changeVec, Vector2 point)
        {
            this.go = go;
            this.changeVec = changeVec;
            this.point = point;
        }
    }

    public class GameController : MonoBehaviour
    {
        public static GameController Instance;

        private MirageMsg msg;

        private GameObject MirageGo;

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (MirageGo)
            {
                Destroy(MirageGo);
            }

            if (msg != null)
            {
                //显示新的虚像
                msg.go.GetComponent<RHTransformController>().ShowMirage(msg);
                msg = null;
            }
        }
    }
}