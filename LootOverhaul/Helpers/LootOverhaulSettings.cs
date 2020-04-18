using ModLib;
using ModLib.Attributes;
using System.Xml;
using System.Xml.Serialization;

namespace LootOverhaul
{
    public class LootOverhaulSettings : SettingsBase
    {
        public const string InstanceID = "zLootOverhaul";

        #region Visual Menu
        public override string ModuleFolderName => "zLootOverhaul";
        public override string ModName => "Loot Overhaul";

        public static LootOverhaulSettings Instance
        {
            get
            {
                return (LootOverhaulSettings)SettingsDatabase.GetSettings(InstanceID);
            }
        }

        [XmlElement]
        public override string ID { get; set; } = InstanceID;

        [XmlElement]
        [SettingPropertyGroup("Developer options")]
        [SettingProperty("Enable debug", "Set debug on/off.")]
        public bool DebugEnabled { get; set; } = false;

        [XmlElement]
        [SettingPropertyGroup("Extra drop chances")]
        [SettingProperty("Min. Extra chance to loot an unit", 0f, 1f, "Sets the minimum extra chance to loot an unit on death.")]
        public float MinUnitLootChance { get; set; } = 0.03f;

        [XmlElement]
        [SettingPropertyGroup("Extra drop chances")]
        [SettingProperty("Max. Extra chance to loot an unit", 0f, 1f, "Sets the maximum extra chance to loot an unit on death.")]
        public float MaxUnitLootChance { get; set; } = 0.05f;

        [XmlElement]
        [SettingPropertyGroup("Extra drop chances")]
        [SettingProperty("Min. Extra chance to loot item", 0f, 1f, "Sets the minimum extra chance to loot a random dead unit equiped item.")]
        public float MinItemLootChance { get; set; } = 0.08f;

        [XmlElement]
        [SettingPropertyGroup("Extra drop chances")]
        [SettingProperty("Max. Extra chance to loot item", 0f, 1f, "Sets the maximum extra chance to loot a random dead unit equiped item.")]
        public float MaxItemLootChance { get; set; } = 0.20f;

        [XmlElement]
        [SettingPropertyGroup("Extra drop chances")]
        [SettingProperty("Allow loot allies", "Allows to loot our allies on death.")]
        public bool LootAlliesEnabled { get; set; } = false;

        [XmlElement]
        [SettingPropertyGroup("Extra drop chances")]
        [SettingProperty("Allow loot panicked soldiers", "Allows to loot the units just before these cowards flee.")]
        public bool LootPanickedEnabled { get; set; } = false;

        [XmlElement]
        [SettingPropertyGroup("Extra drop chances")]
        [SettingProperty("Max items per unit allowed", 0, 12, "Sets the maximum number of items to loot from an unit.")]
        public int MaxItemsPerUnit { get; set; } = 2;

        [XmlElement]
        [SettingPropertyGroup("Looting executed lords")]
        [SettingProperty("Loot executed lords", "Enables to loot the executed lords.")]
        public bool LootExecutedLords { get; set; } = true;

        [XmlElement]
        [SettingPropertyGroup("Looting executed lords")]
        [SettingProperty("Apply items per unit to lord executions", "Applies the max items looted per unit limitations to lord executions. Keep it disabled to loot ALL lord items on execution.")]
        public bool ApplyItemPerUnitToLords { get; set; } = false;
        #endregion
    }
}