using System.Collections.Generic;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Guilds;
using System.Linq;

namespace ModernMenu
{
    public class ModernMenuTradeWindow : DaggerfallTradeWindow
    {
        #region Enums

        // Can't use enums because they crash the game
        /*
        public enum Tabs
        {
            All,
            Weapons,
            Armor,
            Clothing,
            Alchemy,
            Misc
        }
        */
        const int All = 0;
        const int Weapons = 1;
        const int Armor = 2;
        const int Clothing = 3;
        const int Alchemy = 4;
        const int Misc = 5;

        #endregion

        #region Constants

        const int releaseTimeout = 10;

        #endregion

        #region Screen Components

        Rect allRect = new Rect(0, 0, 53, 10);
        Rect weaponsRect = new Rect(54, 0, 53, 10);
        Rect armorRect = new Rect(107, 0, 53, 10);
        Rect clothingRect = new Rect(160, 0, 53, 10);
        Rect alchemyRect = new Rect(213, 0, 53, 10);
        Rect miscRect = new Rect(266, 0, 53, 10);

        // Tab Buttons
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

        int selectedTab = All;

        #endregion

        #region Public methods

        public ModernMenuTradeWindow(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null, WindowModes windowMode = WindowModes.Sell, IGuild guild = null)
            : base(uiManager, previous, windowMode, guild)
        {
        }

        #endregion

        #region Unity

        protected override void Setup()
        {
            base.Setup();

            RemoveOldButtons();
            LoadTextures();

            // Setup tabs
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

            // Initialize
            FilterLocalItems();
            SelectTab(All, false);
        }

        public override void Update()
        {
            base.Update();

            if (Input.GetKey(KeyCode.LeftAlt))
            {
                selectedActionMode = ActionModes.Info;
            }
            else
            {
                selectedActionMode = ActionModes.Select;
            }
        }

        #endregion

        #region Event Handlers

        private void All_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectTab(All);
        }

        private void Weapons_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectTab(Weapons);
        }

        private void Armor_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectTab(Armor);
        }

        private void Clothing_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectTab(Clothing);
        }

        private void Alchemy_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectTab(Alchemy);
        }

        private void Misc_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectTab(Misc);
        }

        #endregion

        #region Helpers

        protected void RemoveOldButtons()
        {
            var components = NativePanel.Components;

            // Remove tab buttons
            var tabShortcuts = new List<DaggerfallShortcut.Buttons>()
            {
                DaggerfallShortcut.Buttons.InventoryWeapons,
                DaggerfallShortcut.Buttons.InventoryMagic,
                DaggerfallShortcut.Buttons.InventoryClothing,
                DaggerfallShortcut.Buttons.InventoryIngredients
            };

            var componentsToRemove = new List<BaseScreenComponent>();
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is Button)
                {
                    Button button = components[i] as Button;
                    foreach (var binding in tabShortcuts)
                    {
                        if (button.Hotkey.Equals(DaggerfallShortcut.GetBinding(binding)))
                            componentsToRemove.Add(button);
                    }
                }
            }

            // Remove action buttons (info and select)
            var offset = GameManager.Instance.PlayerEnterExit.IsPlayerInsideDungeon ? -1 : 0;
            // This should get actionButtonsPanel's children (beware: this could easily break if someone modifies the component load order)
            var childComponents = (components[3] as Panel).Components;
            var childComponentsToRemove = new List<BaseScreenComponent>
            {
                childComponents[2 + offset], // Info
                childComponents[3 + offset]  // Select
            };

            foreach (var c in childComponentsToRemove)
                childComponents.Remove(c);
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

        protected void SelectTab(int tab, bool playSound = true)
        {
            // Select new tab
            selectedTab = tab;

            // Set all buttons to appropriate state
            allButton.BackgroundTexture = (tab == All) ? allSelected : allNotSelected;
            weaponsButton.BackgroundTexture = (tab == Weapons) ? weaponsSelected : weaponsNotSelected;
            armorButton.BackgroundTexture = (tab == Armor) ? armorSelected : armorNotSelected;
            clothingButton.BackgroundTexture = (tab == Clothing) ? clothingSelected : clothingNotSelected;
            alchemyButton.BackgroundTexture = (tab == Alchemy) ? alchemySelected : alchemyNotSelected;
            miscButton.BackgroundTexture = (tab == Misc) ? miscSelected : miscNotSelected;

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

            // Add basket items
            for (int i = 0; i < BasketItems.Count; i++)
            {
                DaggerfallUnityItem item = BasketItems.GetItem(i);
                // Add if not equipped
                if (!item.IsEquipped)
                    AddLocalItemModernMenu(item);
            }

            if (localItems != null)
            {
                // Add items to list
                for (int i = 0; i < localItems.Count; i++)
                {
                    DaggerfallUnityItem item = localItems.GetItem(i);
                    // Add if not equipped
                    if (!item.IsEquipped)
                        AddLocalItemModernMenu(item);
                }

                localItemsFiltered = localItemsFiltered.OrderBy(item => GetCategoryPrecedence(item)).ThenBy(item => item.LongName).ToList();
            }
        }

        protected void AddLocalItemModernMenu(DaggerfallUnityItem item)
        {
            // Add based on view
            if (selectedTab == All)
            {
                localItemsFiltered.Add(item);
            }
            else if (selectedTab == Weapons)
            {
                if (item.ItemGroup == ItemGroups.Weapons)
                    localItemsFiltered.Add(item);
            }
            else if (selectedTab == Armor)
            {
                if (item.ItemGroup == ItemGroups.Armor)
                    localItemsFiltered.Add(item);
            }
            else if (selectedTab == Clothing)
            {
                if (item.ItemGroup == ItemGroups.MensClothing ||
                    item.ItemGroup == ItemGroups.WomensClothing ||
                    item.ItemGroup == ItemGroups.Jewellery)
                    localItemsFiltered.Add(item);
            }
            else if (selectedTab == Alchemy)
            {
                if (item.IsPotion || item.IsPotionRecipe || item.IsIngredient)
                    localItemsFiltered.Add(item);
            }
            else if (selectedTab == Misc)
            {
                // Anything not covered by previous categories
                if (item.ItemGroup != ItemGroups.Weapons &&
                    item.ItemGroup != ItemGroups.Armor &&
                    item.ItemGroup != ItemGroups.MensClothing &&
                    item.ItemGroup != ItemGroups.WomensClothing &&
                    item.ItemGroup != ItemGroups.Jewellery &&
                    !item.IsPotion && !item.IsPotionRecipe && !item.IsIngredient)
                    localItemsFiltered.Add(item);
            }
        }

        protected int GetCategoryPrecedence(DaggerfallUnityItem item)
        {
            if (item.ItemGroup == ItemGroups.Weapons)
                return 0;
            else if (item.ItemGroup == ItemGroups.Armor)
                return 1;
            else if (item.ItemGroup == ItemGroups.MensClothing ||
                item.ItemGroup == ItemGroups.WomensClothing ||
                item.ItemGroup == ItemGroups.Jewellery)
                return 2;
            else if (item.IsPotion || item.IsPotionRecipe || item.IsIngredient)
                return 3;

            return 4;
        }

        #endregion
    }
}
