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
            SelectTab(Tabs.All, false);
            FilterLocalItems();
        }

        public override void Update()
        {
            base.Update();

            if (Input.GetKey(KeyCode.LeftAlt))
            {
                selectedActionMode = ActionModes.Info;
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                selectedActionMode = ActionModes.Use;
            }
            // TODO: I really want to use right-click for this but the class doesn't currently allow it
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                selectedActionMode = ActionModes.Equip;
            }
            else
            {
                selectedActionMode = ActionModes.Remove;
            }
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

            // Remove tab buttons
            var tabShortcuts = new List<DaggerfallShortcut.Buttons>()
            {
                DaggerfallShortcut.Buttons.InventoryWeapons,
                DaggerfallShortcut.Buttons.InventoryMagic,
                DaggerfallShortcut.Buttons.InventoryClothing,
                DaggerfallShortcut.Buttons.InventoryIngredients
            };

            int lastIndex = 0;
            var componentsToRemove = new List<BaseScreenComponent>();
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is Button)
                {
                    Button button = components[i] as Button;
                    foreach (var binding in tabShortcuts)
                    {
                        if (button.Hotkey.Equals(DaggerfallShortcut.GetBinding(binding)))
                        {
                            componentsToRemove.Add(button);
                            /*
                             * Use this index because the action buttons are added directly
                             * after the tab buttons.
                             * Note:
                             * This is a very ugly, hacky way to find the action button indices.
                             * We should update the window class in the future to eliminate the
                             * need to find/remove old buttons altogether.
                            */
                            lastIndex = i;
                        }
                    }
                }
            }

            // Remove action buttons (info, equip, remove, use, gold)
            componentsToRemove.Add(components[lastIndex + 1]);
            componentsToRemove.Add(components[lastIndex + 2]);
            componentsToRemove.Add(components[lastIndex + 3]);
            componentsToRemove.Add(components[lastIndex + 4]);
            componentsToRemove.Add(components[lastIndex + 5]);

            foreach (var b in componentsToRemove)
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
            clothingButton.BackgroundTexture = (tab == Tabs.Clothing) ? clothingSelected : clothingNotSelected;
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
                        AddLocalItemModernMenu(item);
                }
            }
        }

        protected void AddLocalItemModernMenu(DaggerfallUnityItem item)
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

        void UseItem(DaggerfallUnityItem item, ItemCollection collection = null)
        {
            const int noSpellsTextId = 12;

            // Handle quest items on use clicks
            if (item.IsQuestItem)
            {
                // Get the quest this item belongs to
                Quest quest = QuestMachine.Instance.GetQuest(item.QuestUID);
                if (quest == null)
                    throw new Exception("DaggerfallUnityItem references a quest that could not be found.");

                // Get the Item resource from quest
                Item questItem = quest.GetItem(item.QuestItemSymbol);

                // Use quest item
                if (!questItem.UseClicked && questItem.ActionWatching)
                {
                    questItem.UseClicked = true;

                    // Non-parchment items pop back to HUD so quest system has first shot at a custom click action in game world
                    // This is usually the case when actioning most quest items (e.g. a painting, bell, holy item, etc.)
                    // But when clicking a parchment item this behaviour is usually incorrect (e.g. a letter to read)
                    if (!questItem.DaggerfallUnityItem.IsParchment)
                    {
                        DaggerfallUI.Instance.PopToHUD();
                        return;
                    }
                }

                // Check for an on use value
                if (questItem.UsedMessageID != 0)
                {
                    // Display the message popup
                    quest.ShowMessagePopup(questItem.UsedMessageID, true);
                }
            }

            // Try to handle use with a registered delegate
            ItemHelper.ItemUseHander itemUseHander;
            if (DaggerfallUnity.Instance.ItemHelper.GetItemUseHander(item.TemplateIndex, out itemUseHander))
            {
                if (itemUseHander(item, collection))
                    return;
            }

            // Handle normal items
            if (item.ItemGroup == ItemGroups.Books && !item.IsArtifact)
            {
                DaggerfallUI.Instance.BookReaderWindow.OpenBook(item);
                if (DaggerfallUI.Instance.BookReaderWindow.IsBookOpen)
                {
                    DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiOpenBookReaderWindow);
                }
                else
                {
                    var messageBox = new DaggerfallMessageBox(uiManager, this);
                    messageBox.SetText(TextManager.Instance.GetText(textDatabase, "bookUnavailable"));
                    messageBox.ClickAnywhereToClose = true;
                    uiManager.PushWindow(messageBox);
                }
            }
            else if (item.IsPotion)
            {   // Handle drinking magic potions
                GameManager.Instance.PlayerEffectManager.DrinkPotion(item);
                collection.RemoveOne(item);
            }
            else if (item.IsPotionRecipe)
            {
                // TODO: There may be other objects that result in this dialog box, but for now I'm sure this one says it.
                // -IC122016
                DaggerfallMessageBox cannotUse = new DaggerfallMessageBox(uiManager, this);
                cannotUse.SetText(TextManager.Instance.GetText(textDatabase, "cannotUseThis"));
                cannotUse.ClickAnywhereToClose = true;
                cannotUse.Show();
            }
            else if ((item.IsOfTemplate(ItemGroups.MiscItems, (int)MiscItems.Map) ||
                      item.IsOfTemplate(ItemGroups.Maps, (int)Maps.Map)) && collection != null)
            {   // Handle map items
                RecordLocationFromMap(item);
                collection.RemoveItem(item);
                Refresh(false);
            }
            else if (item.TemplateIndex == (int)MiscItems.Spellbook)
            {
                if (GameManager.Instance.PlayerEntity.SpellbookCount() == 0)
                {
                    // Player has no spells
                    TextFile.Token[] textTokens = DaggerfallUnity.Instance.TextProvider.GetRSCTokens(noSpellsTextId);
                    DaggerfallMessageBox noSpells = new DaggerfallMessageBox(uiManager, this);
                    noSpells.SetTextTokens(textTokens);
                    noSpells.ClickAnywhereToClose = true;
                    noSpells.Show();
                }
                else
                {
                    // Show spellbook
                    DaggerfallUI.UIManager.PostMessage(DaggerfallUIMessages.dfuiOpenSpellBookWindow);
                }
            }
            else if (item.ItemGroup == ItemGroups.Drugs && collection != null)
            {
                // Drug poison IDs are 136 through 139. Template indexes are 78 through 81, so add to that.
                FormulaHelper.InflictPoison(GameManager.Instance.PlayerEntity, (Poisons)item.TemplateIndex + 66, true);
                collection.RemoveItem(item);
            }
            else if (item.IsLightSource)
            {
                if (item.currentCondition > 0)
                {
                    if (GameManager.Instance.PlayerEntity.LightSource == item)
                    {
                        DaggerfallUI.MessageBox(TextManager.Instance.GetText(textDatabase, "lightDouse"), false, item);
                        GameManager.Instance.PlayerEntity.LightSource = null;
                    }
                    else
                    {
                        DaggerfallUI.MessageBox(TextManager.Instance.GetText(textDatabase, "lightLight"), false, item);
                        GameManager.Instance.PlayerEntity.LightSource = item;
                    }
                }
                else
                    DaggerfallUI.MessageBox(TextManager.Instance.GetText(textDatabase, "lightEmpty"), false, item);
            }
            else if (item.ItemGroup == ItemGroups.UselessItems2 && item.TemplateIndex == (int)UselessItems2.Oil && collection != null)
            {
                DaggerfallUnityItem lantern = localItems.GetItem(ItemGroups.UselessItems2, (int)UselessItems2.Lantern);
                if (lantern != null && lantern.currentCondition <= lantern.maxCondition - item.currentCondition)
                {   // Re-fuel lantern with the oil.
                    lantern.currentCondition += item.currentCondition;
                    collection.RemoveItem(item.IsAStack() ? collection.SplitStack(item, 1) : item);
                    DaggerfallUI.MessageBox(TextManager.Instance.GetText(textDatabase, "lightRefuel"), false, lantern);
                    Refresh(false);
                }
                else
                    DaggerfallUI.MessageBox(TextManager.Instance.GetText(textDatabase, "lightFull"), false, lantern);
            }
            else
            {
                NextVariant(item);
            }

            // Handle enchanted item on use clicks - setup spell and pop back to HUD
            // Classic does not close inventory window like this, but this way feels better to me
            // Will see what feedback is like and revert to classic behaviour if widely preferred
            if (item.IsEnchanted)
            {
                // Close the inventory window first. Some artifacts (Azura's Star, the Oghma Infinium) create windows on use and we don't want to close those.
                CloseWindow();
                GameManager.Instance.PlayerEffectManager.DoItemEnchantmentPayloads(DaggerfallWorkshop.Game.MagicAndEffects.EnchantmentPayloadFlags.Used, item, collection);
                return;
            }
        }

        void RecordLocationFromMap(DaggerfallUnityItem item)
        {
            const int mapTextId = 499;
            PlayerGPS playerGPS = GameManager.Instance.PlayerGPS;

            try
            {
                DFLocation revealedLocation = playerGPS.DiscoverRandomLocation();

                if (string.IsNullOrEmpty(revealedLocation.Name))
                    throw new Exception();

                playerGPS.LocationRevealedByMapItem = revealedLocation.Name;
                GameManager.Instance.PlayerEntity.Notebook.AddNote(
                    TextManager.Instance.GetText(textDatabase, "readMap").Replace("%map", revealedLocation.Name));

                DaggerfallMessageBox mapText = new DaggerfallMessageBox(uiManager, this);
                mapText.SetTextTokens(DaggerfallUnity.Instance.TextProvider.GetRandomTokens(mapTextId));
                mapText.ClickAnywhereToClose = true;
                mapText.Show();
            }
            catch (Exception)
            {
                // Player has already descovered all valid locations in this region!
                DaggerfallUI.MessageBox(TextManager.Instance.GetText(textDatabase, "readMapFail"));
            }
        }

        #endregion

        #region Event Handlers

        #endregion
    }
}
