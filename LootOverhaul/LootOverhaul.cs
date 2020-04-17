using HarmonyLib;
using LootOverhaul;
using ModLib;
using ModLib.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace LootOverhaul
{
    public class LootOverHaulSettings : SettingsBase
    {
        public const string InstanceID = "LootOverhaul";

        [XmlElement]        
        public override string ID { get; set; } = InstanceID;

        public override string ModuleFolderName => "LootOverhaul";

        public override string ModName => "Loot Overhaul";

        [XmlElement]
        [SettingPropertyGroup("Developer options")]
        [SettingProperty("Enable debug", "Set debug on/off.")]
        public bool DebugEnabled { get; set; } = false;

        //[XmlElement]
        //[SettingPropertyGroup("Extra drop chances")]
        //[SettingProperty("Replace original loot tables", "By enabling this, you will only receive the loot this mod generates!")]
        //public bool ReplaceOriginalLootTables { get; set; } = false;

        [XmlElement]
        [SettingPropertyGroup("Extra drop chances")]
        [SettingProperty("Min. Extra chance to loot an unit", 0f, 1f, "Sets the minimum extra chance to loot an unit on death.")]
        public float MinUnitLootChance { get; set; }

        [XmlElement]
        [SettingPropertyGroup("Extra drop chances")]
        [SettingProperty("Max. Extra chance to loot an unit", 0f, 1f, "Sets the maximum extra chance to loot an unit on death.")]
        public float MaxUnitLootChance { get; set; }

        [XmlElement]
        [SettingPropertyGroup("Extra drop chances")]
        [SettingProperty("Min. Extra chance to loot item", 0f, 1f, "Sets the minimum extra chance to loot a random dead unit equiped item.")]
        public float MinItemLootChance { get; set; }

        [XmlElement]
        [SettingPropertyGroup("Extra drop chances")]
        [SettingProperty("Max. Extra chance to loot item", 0f, 1f, "Sets the maximum extra chance to loot a random dead unit equiped item.")]
        public float MaxItemLootChance { get; set; }

        [XmlElement]
        [SettingPropertyGroup("Extra drop chances")]
        [SettingProperty("Allow loot allies", "Allows to loot our allies on death.")]
        public bool LootAlliesEnabled { get; set; } = false;

        [XmlElement]
        [SettingPropertyGroup("Extra drop chances")]
        [SettingProperty("Apply item per unit to lord executions", "Applies the max items looted per unit limitations to lord executions. Keep it disabled to loot ALL lord items on execution.")]
        public bool ApplyItemPerUnitToLords { get; set; } = false;

        [XmlElement]
        [SettingPropertyGroup("Extra drop chances")]
        [SettingProperty("Max items per unit allowed", 0, 12, "Sets the maximum number of items to loot from an unit.")]
        public int MaxItemsPerUnit { get; set; } = 2;
    }

    public class SubModule : MBSubModuleBase
    {
        public static bool LootAlliesEnabled;
        public static bool ApplyItemPerUnitToLords;
        //public static bool ReplaceOriginalLootTables;
        public static bool debugEnabled;
        public static int maxItemsPerUnitAllowed;

        public static LootOverHaulSettings settingsInstance
        {
            get
            {
                return (LootOverHaulSettings)SettingsDatabase.GetSettings(LootOverHaulSettings.InstanceID);
            }
        }        

        protected override void OnSubModuleLoad()
        {
            try
            {
                FileDatabase.Initialise("LootOverhaul");
                LootOverHaulSettings settings = FileDatabase.Get<LootOverHaulSettings>(LootOverHaulSettings.InstanceID);
                if (settings == null) {
                    settings = new LootOverHaulSettings(); }
                SettingsDatabase.RegisterSettings(settings);
                SettingsDatabase.SaveSettings(settingsInstance);
                FileDatabase.SaveToFile("LootOverhaul",settingsInstance);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR IN LOOT OVERHAUL! "+ex.Message,"ERROR IN LOOT OVERHAUL!");
            }

            try
            {
                LootAlliesEnabled = settingsInstance.LootAlliesEnabled;
                debugEnabled = settingsInstance.DebugEnabled;
                maxItemsPerUnitAllowed = SetMaxItemsPerUnitAllowed(settingsInstance.MaxItemsPerUnit);
                ApplyItemPerUnitToLords = settingsInstance.ApplyItemPerUnitToLords;
                //ReplaceOriginalLootTables = settingsInstance.ReplaceOriginalLootTables;
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage(ex.Message));
            }

            new Harmony("Infinizhen.LootOverhaul").PatchAll();
        }

        protected override void OnSubModuleUnloaded() // Called when exiting Bannerlord entirely
        {
            SettingsDatabase.SaveSettings(settingsInstance);
            FileDatabase.SaveToFile("LootOverhaul", settingsInstance);
            base.OnSubModuleUnloaded();
        }

        private int SetMaxItemsPerUnitAllowed(int value)
        {
            if (value>12)
            {
                return 12;
            }

            if (value<0)
            {
                return 0;
            }

            return value;
        }

        public static void WriteDebugMessage(string message, string title = "DEBUG:")
        {            
            if (!SubModule.settingsInstance.DebugEnabled)
                return;
            InformationManager.DisplayMessage(new InformationMessage(title + " " + message));
        }
    }

    public class DropChance
    {
        float minItemChance;
        float maxItemChance;
        float minUnitChance;
        float maxUnitChance;

        public double CalculateChanceForUnit()
        {           
            //this is the actual drop rate returned: a random between min and max.
            return new Random().NextDouble() * (maxUnitChance - minUnitChance) + minUnitChance;
        }
        public double CalculateChanceForItem()
        {
            //this is the actual drop rate returned: a random between min and max.
            return new Random().NextDouble() * (maxItemChance - minItemChance) + minItemChance;
        }

        private void SetChances()
        {
            minItemChance = SubModule.settingsInstance.MinItemLootChance;
            maxItemChance = SubModule.settingsInstance.MaxItemLootChance;
            minUnitChance = SubModule.settingsInstance.MinUnitLootChance;
            maxUnitChance = SubModule.settingsInstance.MaxUnitLootChance;

            SetItemChances();
            SetUnitChances();
        }
        private void SetItemChances()
        {
            if (minItemChance < 0f)
            {
                minItemChance = 0;
            }

            if (maxItemChance > 1.00f)
            {
                maxItemChance = 1.00f;
            }

            if (minItemChance > maxItemChance)
            {
                maxItemChance = minItemChance;
            }
        }
        private void SetUnitChances()
        {
            if (minUnitChance < 0)
            {
                minUnitChance = 0;
            }

            if (maxUnitChance > 1.00f)
            {
                maxUnitChance = 1.00f;
            }

            if (minUnitChance > maxUnitChance)
            {
                maxUnitChance = minUnitChance;
            }
        }
        public DropChance()
        {
            SetChances();            
        }
    }

    //Just a placeholder Postfix
    [HarmonyPatch(typeof(MapEvent), "CalculateBattleResults")]
    public class MapEventLootOverhaul
    {
        public static void Postfix(MapEvent __instance, Boolean forScoreBoard=false)
        {
            if (__instance.IsPlayerMapEvent && (__instance.PlayerSide == __instance.WinningSide))
            {
                //IEnumerator<PartyBase> ieParty = __instance.PartiesOnSide(__instance.DefeatedSide).GetEnumerator();
                return;
                //TODO
            }
        }
    }

    [HarmonyPatch(typeof(Mission), "OnAgentRemoved")]
    public class BattleLootOverhaul
    {
        public static void Postfix(Mission __instance,Agent affectedAgent,Agent affectorAgent,AgentState agentState,KillingBlow killingBlow)
        {
            Random rng = new Random(Guid.NewGuid().GetHashCode());
            DropChance dc = new DropChance();
            try
            {
                if(MapEvent.PlayerMapEvent==null)
                    return;

                if ((affectedAgent.Character==PartyBase.MainParty.Leader || affectedAgent.IsMount))
                    return;

                if (affectedAgent.Team.IsPlayerAlly)
                {
                    if (SubModule.settingsInstance.DebugEnabled && affectorAgent.Character== PartyBase.MainParty.Leader) { SubModule.WriteDebugMessage("You've killed an ally!"); }
                    if (!SubModule.settingsInstance.LootAlliesEnabled)
                        return;
                }


                //UNIT CHECK!
                if (rng.NextDouble() < dc.CalculateChanceForUnit())
                {
                    List<EquipmentIndex> lootedEquipmentIndexesOfUnit = new List<EquipmentIndex>();
                    List<ItemObject> lootedItemObjects = new List<ItemObject>();

                    //START LOOTING THAT JUICY GEAR!
                    for(int i=0;i<12;++i)
                    {                        
                        EquipmentIndex equipmentIndex = (EquipmentIndex)(rng.Next(0,12));
                        if (lootedEquipmentIndexesOfUnit.Contains(equipmentIndex))
                        {
                            continue;
                        }

                        //ITEM CHECK!
                        if (rng.NextDouble() < dc.CalculateChanceForItem())
                        {
                            EquipmentElement equipmentFromSlot = affectedAgent.Character.Equipment.GetEquipmentFromSlot(equipmentIndex);
                            if (equipmentFromSlot.Item != null)
                            {
                                equipmentFromSlot = affectedAgent.Character.Equipment.GetEquipmentFromSlot(equipmentIndex);
                                lootedEquipmentIndexesOfUnit.Add(equipmentIndex);
                                MapEvent.PlayerMapEvent.ItemRosterForPlayerLootShare(PartyBase.MainParty).AddToCounts(equipmentFromSlot.Item, 1, true);
                                SubModule.WriteDebugMessage(affectedAgent.Team.IsPlayerAlly?"Allied " + equipmentFromSlot.Item.Name.ToString() + " was looted!" : "Enemy "+equipmentFromSlot.Item.Name.ToString() + " was looted!");
                            }
                        }
                    }
                }
                else
                {
                    if (SubModule.settingsInstance.DebugEnabled)
                    {
                        if(affectedAgent.Team.IsPlayerAlly && SubModule.settingsInstance.LootAlliesEnabled){
                            SubModule.WriteDebugMessage("[Allied unit] No Luck! Will not be looted :(");
                        }
                        if(!affectedAgent.Team.IsPlayerAlly)
                        {
                            SubModule.WriteDebugMessage("[Enemy unit] No Luck! Will not be looted :(");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SubModule.WriteDebugMessage(ex.Message);
            }
        }
    }

    [HarmonyPatch(typeof(PartyScreenLogic), "ExecuteTroop")]
    public class LordLootOverhaul
    {
        public static void Postfix(PartyScreenLogic.PartyCommand command)
        {
            DropChance dc = new DropChance();
            Random rng = new Random(Guid.NewGuid().GetHashCode());
            List<EquipmentElement> lootedItemsList = new List<EquipmentElement>();
            CharacterObject character = command.Character;

            EquipmentIndex index;
            EquipmentElement equipmentElement;

            int maxItemsToLoot = 12;
            if (SubModule.settingsInstance.ApplyItemPerUnitToLords)
            {
                maxItemsToLoot = SubModule.settingsInstance.MaxItemsPerUnit;
            }

            for (int i = 0; i < 12; ++i)
            {
                try
                {
                    if (SubModule.settingsInstance.ApplyItemPerUnitToLords)
                    {
                        index = (EquipmentIndex)i;
                        equipmentElement = character.Equipment.GetEquipmentFromSlot(index);
                    }
                    else
                    {
                        index = (EquipmentIndex)rng.Next(0, 12);
                        equipmentElement = character.Equipment.GetEquipmentFromSlot(index);
                    }
                    
                    if (lootedItemsList.Contains(equipmentElement))
                    {
                        continue;
                    }      

                    if (equipmentElement.Item != null)
                    {
                        ItemRoster itemRoster = PartyBase.MainParty.ItemRoster;
                        equipmentElement = character.Equipment.GetEquipmentFromSlot(index);

                        itemRoster.AddToCounts(equipmentElement.Item, 1, true);
                        if (SubModule.settingsInstance.ApplyItemPerUnitToLords)
                        {
                            lootedItemsList.Add(equipmentElement);
                        }
                        SubModule.WriteDebugMessage(equipmentElement.Item.Name.ToString() + " was looted");
                    }

                    if (SubModule.settingsInstance.ApplyItemPerUnitToLords && lootedItemsList.Count>=maxItemsToLoot)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    SubModule.WriteDebugMessage(ex.Message);
                }
            }
        }
    }

}
    
    
