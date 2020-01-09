using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModernMenu
{
    public class ModernMenuWindow : DaggerfallPopupWindow
    {
        #region UI Rects

        #endregion

        #region UI Controls

        #endregion

        #region UI Textures

        #endregion

        #region Fields

        #endregion

        #region Properties

        #endregion

        #region Constructor

        public ModernMenuWindow(IUserInterfaceManager uiManager)
            : base(uiManager)
        {
        }

        #endregion

        #region Setup Methods

        protected override void Setup()
        {
            // Don't hide screen
            NativePanel.BackgroundColor = Color.clear;
            NativePanel.BackgroundTexture = null;
            ParentPanel.BackgroundColor = Color.clear;
            ParentPanel.BackgroundTexture = null;
        }

        #endregion
    }
}
