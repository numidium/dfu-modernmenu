using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.UserInterfaceWindows;

namespace ModernMenu
{
    public class ModernMenu : MonoBehaviour
    {
        static Mod mod;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<ModernMenu>();
        }

        void Awake()
        {
            InitMod();
            mod.IsReady = true;
        }

        public static void InitMod()
        {
            // Register new inventory window
            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Inventory, typeof(ModernMenuWindow));
            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Trade, typeof(ModernMenuTradeWindow));

            Debug.Log("Modern Menu initialized.");
        }
    }
}
