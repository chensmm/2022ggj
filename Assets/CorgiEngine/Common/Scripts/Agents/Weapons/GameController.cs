using UnityEngine;

namespace MoreMountains.CorgiEngine
{
    public class GameController : MonoBehaviour
    {
        public static GameController Instance;

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

        }
    }
}