using System;

namespace LootOverhaul
{
    [HarmonyPatch(typeof(Mission), "OnAgentRemoved")]
    public class BattleLootOverhaul
    {
        public static void Postfix(Mission __instance, Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
        {
            Random rng = new Random(Guid.NewGuid().GetHashCode());
            DropChance dc = new DropChance();
            try
            {
                if (MapEvent.PlayerMapEvent == null)
                    return;

                if ((affectedAgent.Character == PartyBase.MainParty.Leader || affectedAgent.IsMount))
                    return;

                if (affectedAgent.Team.IsPlayerAlly)
                {
                    if (SubModule.settingsInstance.DebugEnabled && affectorAgent.Character == PartyBase.MainParty.Leader) { SubModule.WriteDebugMessage("You've killed an ally!"); }
                    if (!SubModule.settingsInstance.LootAlliesEnabled)
                        return;
                }


                //UNIT CHECK!
                if (rng.NextDouble() < dc.CalculateChanceForUnit())
                {
                    List<EquipmentIndex> lootedEquipmentIndexesOfUnit = new List<EquipmentIndex>();
                    List<ItemObject> lootedItemObjects = new List<ItemObject>();

                    //START LOOTING THAT JUICY GEAR!
                    for (int i = 0; i < 12; ++i)
                    {
                        EquipmentIndex equipmentIndex = (EquipmentIndex)(rng.Next(0, 12));
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
                                SubModule.WriteDebugMessage(affectedAgent.Team.IsPlayerAlly ? "Allied " + equipmentFromSlot.Item.Name.ToString() + " was looted!" : "Enemy " + equipmentFromSlot.Item.Name.ToString() + " was looted!");
                            }
                        }
                    }
                }
                else
                {
                    if (SubModule.settingsInstance.DebugEnabled)
                    {
                        if (affectedAgent.Team.IsPlayerAlly && SubModule.settingsInstance.LootAlliesEnabled)
                        {
                            SubModule.WriteDebugMessage("[Allied unit] No Luck! Will not be looted :(");
                        }
                        if (!affectedAgent.Team.IsPlayerAlly)
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
}
