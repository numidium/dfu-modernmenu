using System;
using System.Collections.Generic;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Questing;
using DaggerfallWorkshop.Game.Banking;
using System.Linq;
using DaggerfallConnect;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;

namespace ModernMenu
{
    public class ModernMenuWindow : DaggerfallInventoryWindow
    {
        #region Enums

        public enum Tabs
        {
            All,
            Weapons,
            Armor,
            Clothing,
            Alchemy,
            Misc
        }

        #endregion

        #region Screen Components

        Rect allRect = new Rect(0, 0, 53, 10);
        Rect weaponsRect = new Rect(54, 0, 53, 10);
        Rect armorRect = new Rect(107, 0, 53, 10);
        Rect clothingRect = new Rect(160, 0, 53, 10);
        Rect alchemyRect = new Rect(213, 0, 53, 10);
        Rect miscRect = new Rect(266, 0, 53, 10);

        Button allButton;
        Button weaponsButton;
        Button armorButton;
        Button clothingButton;
        Button alchemyButton;
        Button miscButton;

        #endregion

        #region Textures

        protected Texture2D allNotSelected;
        protected Texture2D weaponsNotSelected;
        protected Texture2D armorNotSelected;
        protected Texture2D clothingNotSelected;
        protected Texture2D alchemyNotSelected;
        protected Texture2D miscNotSelected;

        protected Texture2D allSelected;
        protected Texture2D weaponsSelected;
        protected Texture2D armorSelected;
        protected Texture2D clothingSelected;
        protected Texture2D alchemySelected;
        protected Texture2D miscSelected;

        #endregion

        #region Fields

        Tabs selectedTab = Tabs.All;

        #endregion

        #region Public methods

        public ModernMenuWindow(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null)
            : base(uiManager, previous)
        {
        }

        public override void Refresh(bool refreshPaperDoll = true)
        {
            base.Refresh(refreshPaperDoll);

            FilterLocalItems();
        }

        #endregion

        protected override void Setup()
        {
            base.Setup();

            RemoveOldButtons();
            LoadTextures();
            allButton = DaggerfallUI.AddButton(allRect, NativePanel);
            allButton.OnMouseClick += All_OnMouseClick;
            weaponsButton = DaggerfallUI.AddButton(weaponsRect, NativePanel);
            weaponsButton.OnMouseClick += Weapons_OnMouseClick;
            armorButton = DaggerfallUI.AddButton(armorRect, NativePanel);
            armorButton.OnMouseClick += Armor_OnMouseClick;
            clothingButton = DaggerfallUI.AddButton(clothingRect, NativePanel);
            clothingButton.OnMouseClick += Clothing_OnMouseClick;
            alchemyButton = DaggerfallUI.AddButton(alchemyRect, NativePanel);
            alchemyButton.OnMouseClick += Alchemy_OnMouseClick;
            miscButton = DaggerfallUI.AddButton(miscRect, NativePanel);
            miscButton.OnMouseClick += Misc_OnMouseClick;

            SelectTab(Tabs.All, false);
            FilterLocalItems();
        }

        #region Event Handlers

        private void All_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectTab(Tabs.All);
        }

        private void Weapons_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectTab(Tabs.Weapons);
        }

        private void Armor_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectTab(Tabs.Armor);
        }

        private void Clothing_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectTab(Tabs.Clothing);
        }

        private void Alchemy_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectTab(Tabs.Alchemy);
        }

        private void Misc_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectTab(Tabs.Misc);
        }

        #endregion

        #region Helpers

        protected void RemoveOldButtons()
        {
            var components = NativePanel.Components;
            var tabShortcuts = new List<DaggerfallShortcut.Buttons>()
            {
                DaggerfallShortcut.Buttons.InventoryWeapons,
                DaggerfallShortcut.Buttons.InventoryMagic,
                DaggerfallShortcut.Buttons.InventoryClothing,
                DaggerfallShortcut.Buttons.InventoryIngredients
            };

            var buttonsToRemove = new List<Button>();
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is Button)
                {
                    Button button = components[i] as Button;
                    foreach (var binding in tabShortcuts)
                    {
                        if (button.Hotkey.Equals(DaggerfallShortcut.GetBinding(binding)))
                            buttonsToRemove.Add(button);
                    }
                }
            }

            foreach (var b in buttonsToRemove)
                components.Remove(b);
        }

        protected override void LoadTextures()
        {
            base.LoadTextures();

            DFSize baseSize = new DFSize(320, 200);

            // Cut out non-selected tab textures
            allNotSelected = ImageReader.GetSubTexture(baseTexture, allRect, baseSize);
            weaponsNotSelected = ImageReader.GetSubTexture(baseTexture, weaponsRect, baseSize);
            armorNotSelected = ImageReader.GetSubTexture(baseTexture, armorRect, baseSize);
            clothingNotSelected = ImageReader.GetSubTexture(baseTexture, clothingRect, baseSize);
            alchemyNotSelected = ImageReader.GetSubTexture(baseTexture, alchemyRect, baseSize);
            miscNotSelected = ImageReader.GetSubTexture(baseTexture, miscRect, baseSize);

            // Cut out selected tab textures
            allSelected = ImageReader.GetSubTexture(goldTexture, allRect, baseSize);
            weaponsSelected = ImageReader.GetSubTexture(goldTexture, weaponsRect, baseSize);
            armorSelected = ImageReader.GetSubTexture(goldTexture, armorRect, baseSize);
            clothingSelected = ImageReader.GetSubTexture(goldTexture, clothingRect, baseSize);
            alchemySelected = ImageReader.GetSubTexture(goldTexture, alchemyRect, baseSize);
            miscSelected = ImageReader.GetSubTexture(goldTexture, miscRect, baseSize);
        }

        protected void SelectTab(Tabs tab, bool playSound = true)
        {
            // Select new tab
            selectedTab = tab;

            // Set all buttons to appropriate state
            allButton.BackgroundTexture = (tab == Tabs.All) ? allSelected : allNotSelected;
            weaponsButton.BackgroundTexture = (tab == Tabs.Weapons) ? weaponsSelected : weaponsNotSelected;
            armorButton.BackgroundTexture = (tab == Tabs.Armor) ? armorSelected : armorNotSelected;
            clothingButton.BackgroundTexture = (tab == Tabs.Clothing) ? armorSelected : armorNotSelected;
            alchemyButton.BackgroundTexture = (tab == Tabs.Alchemy) ? alchemySelected : alchemyNotSelected;
            miscButton.BackgroundTexture = (tab == Tabs.Misc) ? miscSelected : miscNotSelected;

            // Clear info panel
            if (itemInfoPanelLabel != null)
                itemInfoPanelLabel.SetText(new TextFile.Token[0]);

            // Update filtered list
            localItemListScroller.ResetScroll();
            FilterLocalItems();
            localItemListScroller.Items = localItemsFiltered;

            if (playSound)
                DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected override void FilterLocalItems()
        {
            localItemsFiltered.Clear();

            if (localItems != null)
            {
                // Add items to list
                for (int i = 0; i < localItems.Count; i++)
                {
                    DaggerfallUnityItem item = localItems.GetItem(i);
                    // Add if not equipped
                    if (!item.IsEquipped)
                        AddLocalItemMm(item);
                }
            }
        }

        protected void AddLocalItemMm(DaggerfallUnityItem item)
        {
            // Add based on view
            if (selectedTab == Tabs.All)
            {
                localItemsFiltered.Add(item);
            }
            else if (selectedTab == Tabs.Weapons)
            {
                if (item.ItemGroup == ItemGroups.Weapons)
                    localItemsFiltered.Add(item);
            }
            else if (selectedTab == Tabs.Armor)
            {
                if (item.ItemGroup == ItemGroups.Armor)
                    localItemsFiltered.Add(item);
            }
            else if (selectedTab == Tabs.Clothing)
            {
                if (item.ItemGroup == ItemGroups.MensClothing ||
                    item.ItemGroup == ItemGroups.WomensClothing ||
                    item.ItemGroup == ItemGroups.Jewellery)
                    localItemsFiltered.Add(item);
            }
            else if (selectedTab == Tabs.Alchemy)
            {
                if (item.IsPotion || item.IsPotionRecipe || item.IsIngredient)
                    localItemsFiltered.Add(item);
            }
            else if (selectedTab == Tabs.Misc)
            {
                if (item.ItemGroup == ItemGroups.Books ||
                    item.ItemGroup == ItemGroups.MiscItems)
                    localItemsFiltered.Add(item);
            }
            else
            {
                Debug.Log("Modern Menu: Failed to categorize item - " + item.ItemName);
            }
        }

        #endregion
    }
}
