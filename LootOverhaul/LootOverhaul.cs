using HarmonyLib;
using ModLib;
using ModLib.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace LootOverhaul
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            //MessageBox.Show("STOP");
            InitializeModLib();
            ApplyHarmonyPatches();
            //SetTestOptionInMainMenu();
        }

        private void InitializeModLib()
        {
            try
            {
                FileDatabase.Initialise("zLootOverhaul");
                SettingsDatabase.RegisterSettings((FileDatabase.Get<LootOverhaulSettings>("zLootOverhaul") ?? new LootOverhaulSettings()));
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR IN LOOT OVERHAUL! " + ex.Message, "ERROR IN LOOT OVERHAUL!");
            }
        }

        private void ApplyHarmonyPatches()
        {
            new Harmony("Infinizhen.LootOverhaul").PatchAll();
        }

        private void SetTestOptionInMainMenu()
        {
            SettingsBase s = LootOverhaulSettings.Instance;
            Module.CurrentModule.AddInitialStateOption(new InitialStateOption("LootOverhaulTestMainMenuOption",
            new TextObject("Loot Overhaul", null),
            9990,
            () =>
            {
                WriteMessageInChatLog("---------------------------------------");
                WriteMessageInChatLog("Current values loaded for Loot Overhaul:");
                WriteMessageInChatLog("Debug Enabled: " + (LootOverhaulSettings.Instance.DebugEnabled ? "Yes" : "No"));                
                WriteMessageInChatLog("Min/Max Item/Unit LootChance: " + (LootOverhaulSettings.Instance.MinItemLootChance*100).ToString() + "%/" + (LootOverhaulSettings.Instance.MaxItemLootChance * 100).ToString() + "% - " + (LootOverhaulSettings.Instance.MinUnitLootChance * 100).ToString() + "%/" + (LootOverhaulSettings.Instance.MaxUnitLootChance * 100).ToString() + "%");
                WriteMessageInChatLog("Loot Allies: " + (LootOverhaulSettings.Instance.LootAlliesEnabled ? "Yes" : "No"));
                WriteMessageInChatLog("Loot Panicked: " + (LootOverhaulSettings.Instance.LootPanickedEnabled ? "Yes" : "No"));
                WriteMessageInChatLog("Max Items Looted per Unit: " + LootOverhaulSettings.Instance.MaxItemsPerUnit.ToString());
                WriteMessageInChatLog("Loot executed lords: " + (LootOverhaulSettings.Instance.LootExecutedLords ? "Yes" : "No"));
                WriteMessageInChatLog("Apply Max Items Looted per Unit to Executions: " + (LootOverhaulSettings.Instance.ApplyItemPerUnitToLords ? "Yes" : "No"));
            },
            false)
            );
        }

        public static void WriteMessageInChatLog(string message, string title = "")
        {
            InformationManager.DisplayMessage(new InformationMessage(title + " " + message));
        }

        public static void WriteDebug(string message, string title = "LootOverhaulDebug:", Color? c = null)
        {
            if (!LootOverhaulSettings.Instance.DebugEnabled)
                return;

            var color = c ?? Color.White;
            InformationManager.DisplayMessage(new InformationMessage(title + " " + message, color));
            
        }

        public static void WriteLootMessage(EquipmentElement _equipmentFromSlot, bool _isEnemy = true)
        {
            string message = _equipmentFromSlot.Item.Name.ToString();
            string side = _isEnemy ? "enemy:" : "ally:";
            if(LootOverhaulSettings.Instance.ShowLootMessages || LootOverhaulSettings.Instance.DebugEnabled)
                InformationManager.DisplayMessage(new InformationMessage("Looted " + side+" "+message, Color.FromUint(4282569842U)));
        }

        public static void WriteException(string exceptionMessage, string title = "LootOverhaulException:")
        {
            WriteMessageInChatLog(exceptionMessage, title);
        }
    }

    public static class LootOverhaul
    {
        public static List<EquipmentIndex> allowedSlotsToLoot = new List<EquipmentIndex>{
            EquipmentIndex.Head,
            EquipmentIndex.Cape,
            EquipmentIndex.Body,
            EquipmentIndex.Gloves,
            EquipmentIndex.Leg,
            EquipmentIndex.Horse,
            EquipmentIndex.HorseHarness,
            EquipmentIndex.Weapon0,
            EquipmentIndex.Weapon1,
            EquipmentIndex.Weapon2,
            EquipmentIndex.Weapon3,
            EquipmentIndex.Weapon4
        };

        public static void Loot(ItemObject _item, int number=1, bool removeDepleted=true, bool IsExecution=false)
        {
            if (_item == null)
            {
                SubModule.WriteException("Tried to loot a null item.");
                return;
            }

            if (IsExecution)
            {
                PartyBase.MainParty.ItemRoster.AddToCounts(_item, 1, true);
                return;
            }

            if (MapEvent.PlayerMapEvent!=null)
                MapEvent.PlayerMapEvent.ItemRosterForPlayerLootShare(PartyBase.MainParty).AddToCounts(_item, 1, true);
            
        }

        public static void WriteLootMessage(EquipmentElement _equipmentFromSlot, bool _isEnemy = true)
        {
            string message = _equipmentFromSlot.Item.Name.ToString();
            string side = _isEnemy ? "enemy:" : "ally:";
            SubModule.WriteDebug(message, "Looted "+side, Color.FromUint(4282569842U));
        }

        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }
    }
}


