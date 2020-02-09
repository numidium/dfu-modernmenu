using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Utility;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop.Game.UserInterfaceWindows;

namespace ModernMenu
{
    public class ModernMenu : MonoBehaviour
    {
        bool isVisible = false;

        void Start()
        {
        }

        void Update()
        {
        }

        [Invoke(StateManager.StateTypes.Game, 0)]
        public static void Init(InitParams initParams)
        {
            GameObject modernMenuGo = new GameObject("modernMenu");
            ModernMenu modernMenu = modernMenuGo.AddComponent<ModernMenu>();

            // Register new inventory window
            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Inventory, typeof(ModernMenuWindow));

            //after finishing, set the mod's IsReady flag to true.
            ModManager.Instance.GetMod(initParams.ModTitle).IsReady = true;

            Debug.Log("Modern Menu initialized.");
        }
    }
}
