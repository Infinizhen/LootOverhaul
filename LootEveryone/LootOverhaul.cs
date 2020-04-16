using HarmonyLib;
using LootOverhaul;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace LootOverhaul
{
    public class Config
    {
        private static readonly string config_file = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/LootOverhaulConfig.xml";
        public XmlDocument config = new XmlDocument();

        public Config()
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true
            };
            using (XmlReader reader = XmlReader.Create(Config.config_file, settings))
                this.config.Load(reader);
        }
    }

    public class SubModule : MBSubModuleBase
    {
        private XmlNode allyLootSettingsNode;
        private XmlNode debugSettingsNode;
        public static bool LootAlliesEnabled;
        public static bool debugEnabled;

        public static Config config = new Config();

        protected override void OnSubModuleLoad()
        {
            //InformationManager.DisplayMessage(new InformationMessage("TEST")); // Display message on chatlog
            //TaleWorlds.MountAndBlade.Module.CurrentModule.AddInitialStateOption(new InitialStateOption("Message",
            //    new TextObject("Loot Overhaul Debug Test", null),
            //    9990,
            //    () =>
            //    {
            //        try
            //        {
            //            InformationManager.DisplayMessage(new InformationMessage("Is debug enabled? " + config.config.ChildNodes[1].SelectSingleNode("DebugSettings").SelectSingleNode("DebugEnabled").InnerText));
            //            // Display message on chatlog
            //        }
            //        catch (Exception ex)
            //        {
            //            InformationManager.DisplayMessage(new InformationMessage(ex.Message));
            //        }
            //    },
            //    false)
            //);
            new Harmony("Infinizhen.LootOverhaul").PatchAll();
        }

        public SubModule() : base()
        {
            allyLootSettingsNode = SubModule.config.config.ChildNodes[1].SelectSingleNode("AllyLootSettings");
            LootAlliesEnabled = bool.Parse(allyLootSettingsNode.SelectSingleNode("LootAlliesEnabled").InnerText);
            debugSettingsNode = SubModule.config.config.ChildNodes[1].SelectSingleNode("DebugSettings");
            debugEnabled = bool.Parse(debugSettingsNode.SelectSingleNode("DebugEnabled").InnerText);
        }

        public static Dictionary<string, string> SettingsDictionary { get; set; } = new Dictionary<string, string>()
        {
          {
            "LootAlliesEnabled",
            "0"
          },
          {
            "DebugEnabled",
            "0"
          },
          {
            "MinDropRate",
            "0.20"
          },
          {
            "MaxDropRate",
            "0.35"
          }
        };

        public static Dictionary<string, string> GetModSettingValue()
        {
            string path = Directory.GetCurrentDirectory().Replace("bin\\Win64_Shipping_Client", "") + "Modules\\LootOverhaul\\settings.json";
            if (File.Exists(path))
            {
                Dictionary<string, string> dictionary = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(File.ReadAllText(path));
                if (dictionary != null)
                    SettingsDictionary = dictionary;
            }
            return SettingsDictionary;
        }

        public static void WriteDebugMessage(string message, string title = "DEBUG:")
        {
            string str;
            //SettingsDictionary.TryGetValue("DebugEnabled", out str);
            
            if (!SubModule.debugEnabled)
                return;
            InformationManager.DisplayMessage(new InformationMessage(title + " " + message));
        }

        public static void SaveModSettingValue(Dictionary<string, string> newSettings)
        {
            string path = Directory.GetCurrentDirectory().Replace("bin\\Win64_Shipping_Client", "") + "Modules\\LootOverhaul\\settings.json";
            if (!File.Exists(path))
                File.Create(path).Dispose();
            File.WriteAllText(path, new JavaScriptSerializer().Serialize((object)newSettings));
            SettingsDictionary = newSettings;
        }
    }

    public class DropChance
    {
        public static XmlNode dropRateSettingsNode;
        private static double minDropRate;
        private static double maxDropRate;
        public double CalculateChance()
        {
            double min = minDropRate;
            double max = maxDropRate;

            if (min < (double)0)
            {
                min = 0;
            }

            if (max > (double)1.00)
            {
                max = 1.00;
            }

            if (min > max)
            {
                max = min;
            }

            //this is the actual drop rate returned: a random between min and max.
            return new Random().NextDouble() * (max - min) + min;
        }

        public DropChance()
        {
            dropRateSettingsNode = SubModule.config.config.ChildNodes[1].SelectSingleNode("DropRateSettings");
            minDropRate = double.Parse(dropRateSettingsNode.SelectSingleNode("MinDropChance").InnerText);
            maxDropRate = double.Parse(dropRateSettingsNode.SelectSingleNode("MaxDropChance").InnerText);
        }
    }

    [HarmonyPatch(typeof(Mission), "OnAgentRemoved")]
    public class BattleLootOverhaul
    {
        public static void Postfix(Mission __instance,Agent affectedAgent,Agent affectorAgent,AgentState agentState,KillingBlow killingBlow)
        {
            try
            {
                DropChance dc = new DropChance();

                if ((affectedAgent.IsMainAgent || affectedAgent.IsMount))
                    return;

                if (!affectedAgent.IsEnemyOf(Agent.Main))
                {
                    if (SubModule.debugEnabled) { SubModule.WriteDebugMessage("Killed an ally."); }
                    if (!SubModule.LootAlliesEnabled)
                    {
                        return;
                    }
                }

                for (int index = 0; (long)index < (long)12; ++index)
                {
                    EquipmentIndex equipmentIndex = (EquipmentIndex)index;
                    if (new Random().NextDouble() < dc.CalculateChance())
                    {
                        EquipmentElement equipmentFromSlot = affectedAgent.Character.Equipment.GetEquipmentFromSlot(equipmentIndex);
                        if (equipmentFromSlot.Item != null)
                        {
                            equipmentFromSlot = affectedAgent.Character.Equipment.GetEquipmentFromSlot(equipmentIndex);
                            if (SubModule.debugEnabled)
                            {
                                //SubModule.WriteDebugMessage("debugMessage");
                            }
                            MapEvent.PlayerMapEvent.ItemRosterForPlayerLootShare(PartyBase.MainParty).AddToCounts(equipmentFromSlot.Item, 1, true);
                            SubModule.WriteDebugMessage(equipmentFromSlot.Item.Name.ToString() + " was looted!");
                        }
                    }
                    else
                    {
                        if (SubModule.debugEnabled)
                        {
                            SubModule.WriteDebugMessage("No Luck! " + (new Random().NextDouble()*100).ToString("0.##") + "% vs. " + (dc.CalculateChance()*100).ToString("0.##")+"%");
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
            double dropChance = dc.CalculateChance();
            Random random = new Random();
            CharacterObject character = command.Character;
            for (int index = 0; index < 12; ++index)
            {
                try
                {
                    if (random.NextDouble() < dropChance)
                    {
                        EquipmentElement equipmentElement = character.Equipment.GetEquipmentFromSlot((EquipmentIndex)index);
                        if (equipmentElement.Item != null)
                        {
                            ItemRoster itemRoster = PartyBase.MainParty.ItemRoster;
                            equipmentElement = character.Equipment.GetEquipmentFromSlot((EquipmentIndex)index);

                            itemRoster.AddToCounts(equipmentElement.Item, 1, true);

                            SubModule.WriteDebugMessage(equipmentElement.Item.Name.ToString() + " was looted");
                        }
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
    
    
