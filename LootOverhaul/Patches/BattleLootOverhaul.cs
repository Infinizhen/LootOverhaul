using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace LootOverhaul
{
    [HarmonyPatch(typeof(Mission), "OnAgentRemoved")]
    public class BattleLootOverhaul
    {
        static readonly Random rng = new Random(Guid.NewGuid().GetHashCode());
        static readonly DropChance dc = new DropChance();

        public static void Postfix(Mission __instance, Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
        {
            try
            {
                if (MapEvent.PlayerMapEvent == null)
                    return;

                if ((affectedAgent.Character == PartyBase.MainParty.Leader || affectedAgent.IsMount))
                    return;

                if (affectorAgent == null && !LootOverhaulSettings.Instance.LootPanickedEnabled)
                {
                    SubModule.WriteDebug("Some coward has fleed...");
                    return;
                }

                if (affectedAgent.Team.IsPlayerAlly)
                {
                    if (affectorAgent.Character == PartyBase.MainParty.Leader)
                        SubModule.WriteDebug("You've killed an ally!", "Oops:"); 
                    if (!LootOverhaulSettings.Instance.LootAlliesEnabled)
                        return;
                }

                if (rng.NextDouble() < dc.CalculateChanceForUnit())
                {
                    int itemsLooted = 0;
                    foreach (EquipmentIndex ei in LootOverhaul.allowedSlotsToLoot.Shuffle())
                    {
                        EquipmentElement equipmentElement = affectedAgent.Character.Equipment.GetEquipmentFromSlot(ei);

                        if (equipmentElement.Item == null)
                            continue;

                        if (rng.NextDouble() > dc.CalculateChanceForItem())
                            continue;
                        
                        LootOverhaul.Loot(equipmentElement.Item);
                        LootOverhaul.WriteLootMessage(equipmentElement,!affectedAgent.Team.IsPlayerAlly);
                        itemsLooted++;

                        if (itemsLooted >= LootOverhaulSettings.Instance.MaxItemsPerUnit)
                            break;
                    }      
                }
                else
                {
                    string messageTitle = affectedAgent.Team.IsPlayerAlly ? "Allied unit:" : "Enemy unit:";
                    string message = "No Luck! Will not be looted :(";                        

                    if (affectedAgent.Team.IsPlayerAlly && LootOverhaulSettings.Instance.LootAlliesEnabled)
                        SubModule.WriteDebug(message, messageTitle);

                    if (!affectedAgent.Team.IsPlayerAlly)
                        SubModule.WriteDebug(message, messageTitle);
                }
            }
            catch (Exception ex)
            {
                SubModule.WriteException(ex.Message);
            }
        }
    }
}
