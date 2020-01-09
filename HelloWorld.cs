using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Utility;
using DaggerfallConnect.Utility;

namespace ModernMenu
{
    public class HelloWorld : MonoBehaviour
    {
        bool isVisible = false;

        void Start()
        {
            Debug.Log("Modern menu Start() called.");
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (!isVisible)
                {
                    GameManager.Instance.PauseGame(true);
                    isVisible = true;
                }
                else
                {
                    GameManager.Instance.PauseGame(false);
                    isVisible = false;
                }
            }
        }

        [Invoke(StateManager.StateTypes.Game, 0)]
        public static void Init(InitParams initParams)
        {
            Debug.Log("main init");

            //just an example of how to add a mono-behavior to a scene.
            GameObject helloGo = new GameObject("hello");
            HelloWorld hello = helloGo.AddComponent<HelloWorld>();

            //after finishing, set the mod's IsReady flag to true.
            ModManager.Instance.GetMod(initParams.ModTitle).IsReady = true;
        }
    }
}
