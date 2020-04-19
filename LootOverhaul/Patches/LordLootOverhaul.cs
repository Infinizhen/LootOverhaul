using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace LootOverhaul
{
    [HarmonyPatch(typeof(PartyScreenLogic), "ExecuteTroop")]
    public class LordLootOverhaul
    {
        private static void LootExecuted(ItemObject _item)
        {
            LootOverhaul.Loot(_item, 1, true, true);
        }

        public static void Postfix(PartyScreenLogic.PartyCommand command)
        {
            if (!LootOverhaulSettings.Instance.LootExecutedLords)
                return;

            CharacterObject character = command.Character;
            EquipmentElement equipmentElement;

            int maxItemsToLoot = LootOverhaulSettings.Instance.ApplyItemPerUnitToLords? LootOverhaulSettings.Instance.MaxItemsPerUnit : 12;
            int itemsLooted = 0;
            foreach (EquipmentIndex ei in LootOverhaul.allowedSlotsToLoot.Shuffle())
            {
                try
                {
                    if (itemsLooted >= maxItemsToLoot)
                        break;

                    equipmentElement = character.Equipment.GetEquipmentFromSlot(ei);
                    if (equipmentElement.Item == null)
                        continue;
                    
                    LootExecuted(equipmentElement.Item);
                    LootOverhaul.WriteLootMessage(equipmentElement);
                    itemsLooted++;
                }
                catch (Exception ex)
                {
                    SubModule.WriteException(ex.Message);
                }
            }
        }
    }
}