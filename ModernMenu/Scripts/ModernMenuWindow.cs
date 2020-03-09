using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Questing;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using System.Linq;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallConnect;
using DaggerfallWorkshop.Game.Formulas;

namespace ModernMenu
{
    public class ModernMenuWindow : DaggerfallInventoryWindow
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
        Rect rHandAttackRect = new Rect(225, 20, 37, 30);
        Rect lHandAttackRect = new Rect(225, 65, 37, 30);

        Panel rHandAttackPanel;
        Panel lHandAttackPanel;

        MultiFormatTextLabel rHandAttackLabel;
        MultiFormatTextLabel lHandAttackLabel;

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

        public ModernMenuWindow(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null)
            : base(uiManager, previous)
        {
        }

        #endregion

        #region Unity

        protected override void Setup()
        {
            base.Setup();

            RemoveOldButtons();
            LoadTextures();

            // Item tabs
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

            // Accessory buttons
            foreach (var button in accessoryButtons)
            {
                button.OnRightMouseClick += AccessoryItemsButton_OnMouseRightClick;
            }

            // Attack bonus indicators
            rHandAttackPanel = DaggerfallUI.AddPanel(rHandAttackRect, NativePanel);
            lHandAttackPanel = DaggerfallUI.AddPanel(lHandAttackRect, NativePanel);

            rHandAttackLabel = GetAttackInfoLabel();
            lHandAttackLabel = GetAttackInfoLabel();
            rHandAttackPanel.Components.Add(rHandAttackLabel);
            lHandAttackPanel.Components.Add(lHandAttackLabel);
            SetAttackInfo(rHandAttackLabel);
            SetAttackInfo(lHandAttackLabel, false);

            // Initialize
            FilterLocalItems();
            SelectTab(All, false);
        }

        public override void Update()
        {
            base.Update();

            // Handle info modifier
            if (Input.GetKey(KeyCode.LeftControl))
            {
                selectedActionMode = ActionModes.Info;
            }
            else
            {
                selectedActionMode = ActionModes.Equip;
            }
        }

        public override void Refresh(bool refreshPaperDoll = true)
        {
            base.Refresh(refreshPaperDoll);
            if (IsSetup)
            {
                SetAttackInfo(rHandAttackLabel);
                SetAttackInfo(lHandAttackLabel, false);
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

        protected override void LocalItemListScroller_OnItemClick(DaggerfallUnityItem item)
        {
            // Transfer to remote items
            if (remoteItems != null && !chooseOne)
            {
                int? canHold = null;
                if (usingWagon)
                    canHold = WagonCanHoldAmount(item);
                TransferItem(item, localItems, remoteItems, canHold, true);
                if (theftBasket != null && lootTarget != null && lootTarget.houseOwned)
                    theftBasket.RemoveItem(item);
            }
        }

        protected override void LocalItemListScroller_OnItemRightClick(DaggerfallUnityItem item)
        {
            // Info
            if (selectedActionMode == ActionModes.Info)
                ShowInfoPopup(item);
            // Use light source or book/parchment
            else if (item.IsLightSource || item.IsParchment || item.IsPotionRecipe || item.ItemGroup == ItemGroups.Books)
            {
                UseItem(item);
                Refresh(false);
            }
            // Use potion
            else if (item.IsPotion)
            {
                if (!item.UseItem(localItems))
                    UseItem(item, localItems);
                Refresh(false);
            }
            // Equip apparel/weapon
            else
            {
                EquipItem(item);
            }
        }

        protected override void RemoteItemListScroller_OnItemClick(DaggerfallUnityItem item)
        {
            TransferItem(item, remoteItems, localItems, CanCarryAmount(item));
            if (theftBasket != null && lootTarget != null && lootTarget.houseOwned)
                theftBasket.AddItem(item);
        }


        protected override void RemoteItemListScroller_OnItemRightClick(DaggerfallUnityItem item)
        {
            // Send click to quest system
            if (item.IsQuestItem)
            {
                Quest quest = QuestMachine.Instance.GetQuest(item.QuestUID);
                if (quest != null)
                {
                    Item questItem = quest.GetItem(item.QuestItemSymbol);
                    if (quest != null)
                        questItem.SetPlayerClicked();
                }
            }

            // Info
            if (selectedActionMode == ActionModes.Info)
                ShowInfoPopup(item);
            // Use light source or book/parchment
            else if (item.IsLightSource || item.IsParchment || item.IsPotionRecipe || item.ItemGroup == ItemGroups.Books)
            {
                UseItem(item);
                Refresh(false);
            }
            // Use potion
            else if (item.IsPotion)
            {
                if (!item.UseItem(remoteItems))
                    UseItem(item, remoteItems);
                Refresh(false);
            }
            // Equip apparel/weapon
            else
            {
                // Transfer to local items
                if (localItems != null)
                    TransferItem(item, remoteItems, localItems, CanCarryAmount(item), equip: true);
                if (theftBasket != null && lootTarget != null && lootTarget.houseOwned)
                    theftBasket.AddItem(item);
            }
        }

        protected void AccessoryItemsButton_OnMouseRightClick(BaseScreenComponent sender, Vector2 position)
        {
            // Use equipped accessory
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            var slot = (EquipSlots)sender.Tag;
            var item = playerEntity.ItemEquipTable.GetItem(slot);
            if (item == null)
                return;
            UseItem(item);
        }

        #endregion

        #region Helpers

        protected void RemoveOldButtons()
        {
            var components = NativePanel.Components;

            components.Remove(weaponsAndArmorButton);
            components.Remove(magicItemsButton);
            components.Remove(clothingAndMiscButton);
            components.Remove(ingredientsButton);
            components.Remove(infoButton);
            components.Remove(equipButton);
            components.Remove(removeButton);
            components.Remove(useButton);
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

        protected MultiFormatTextLabel GetAttackInfoLabel()
        {
            return new MultiFormatTextLabel
            {
                Position = new Vector2(2, 0),
                VerticalAlignment = VerticalAlignment.Middle,
                MinTextureDimTextLabel = 16, // important to prevent scaling issues for single text lines
                TextScale = .7f,
                MaxTextWidth = 37,
                WrapText = true,
                WrapWords = true,
                ExtraLeading = 3, // spacing between info panel elements
                TextColor = new Color32(250, 250, 0, 255),
                ShadowPosition = new Vector2(0.5f, 0.5f),
                ShadowColor = DaggerfallUI.DaggerfallAlternateShadowColor1
            };
        }

        protected void SetAttackInfo(MultiFormatTextLabel label, bool rightHand = true)
        {
            var weapon = playerEntity.ItemEquipTable.GetItem(rightHand ? EquipSlots.RightHand : EquipSlots.LeftHand);
            short weaponSkill = 0;
            int chanceToHitMod = 0;
            int damageMod = 0;
            int minDamage = 0;
            int maxDamage = 0;

            // Note from Numidium: I copied snippets from FormulaHelper.cs to calculate the values used here
            if (weapon != null)
            {
                weaponSkill = weapon.GetWeaponSkillIDAsShort();
                short skillValue = PlayerEntity.Skills.GetLiveSkillValue(weaponSkill);
                chanceToHitMod = skillValue;

                // Apply weapon proficiency
                if (((int)playerEntity.Career.ExpertProficiencies & weapon.GetWeaponSkillUsed()) != 0)
                {
                    damageMod += (playerEntity.Level / 3) + 1;
                    chanceToHitMod += playerEntity.Level;
                }

                // Apply weapon material modifier
                if (weapon.GetWeaponMaterialModifier() > 0)
                {
                    chanceToHitMod += weapon.GetWeaponMaterialModifier() * 10;
                }

                // Apply racial bonuses
                if (playerEntity.RaceTemplate.ID == (int)Races.DarkElf)
                {
                    damageMod += playerEntity.Level / 4;
                    chanceToHitMod += playerEntity.Level / 4;
                }
                else if (weaponSkill == (short)DFCareer.Skills.Archery)
                {
                    if (playerEntity.RaceTemplate.ID == (int)Races.WoodElf)
                    {
                        damageMod += playerEntity.Level / 3;
                        chanceToHitMod += playerEntity.Level / 3;
                    }
                }
                else if (playerEntity.RaceTemplate.ID == (int)Races.Redguard)
                {
                    damageMod += playerEntity.Level / 3;
                    chanceToHitMod += playerEntity.Level / 3;
                }

                // Calculate min and max damage player can do with their current weapon
                damageMod += weapon.GetWeaponMaterialModifier() + FormulaHelper.DamageModifier(playerEntity.Stats.LiveStrength);
                minDamage = weapon.GetBaseDamageMin() + damageMod;
                maxDamage = weapon.GetBaseDamageMax() + damageMod;
            }
            // Apply hand-to-hand proficiency. Hand-to-hand proficiency is not applied in classic.
            else 
            {
                var handToHandSkill = playerEntity.Skills.GetLiveSkillValue((short)DFCareer.Skills.HandToHand);
                chanceToHitMod += handToHandSkill;
                if (((int)playerEntity.Career.ExpertProficiencies & (int)DFCareer.ProficiencyFlags.HandToHand) != 0)
                {
                    damageMod += (playerEntity.Level / 3) + 1;
                    chanceToHitMod += playerEntity.Level;
                }

                minDamage = FormulaHelper.CalculateHandToHandMinDamage(handToHandSkill);
                maxDamage = FormulaHelper.CalculateHandToHandMaxDamage(handToHandSkill);
            }

            label.Clear();
            var handText = rightHand ? "Right" : "Left";
            label.SetText(new TextAsset(handText + "\nAtk:\n" + chanceToHitMod.ToString() + "\nDmg:\n" + minDamage.ToString() + "-" + maxDamage.ToString()));
        }

        #endregion
    }
}
