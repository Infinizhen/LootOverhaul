using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;

namespace LootOverhaul
{
    [HarmonyPatch(typeof(MapEvent), "CalculateBattleResults")]
    public class MapEventLootOverhaul
    {
        public static void Postfix(MapEvent __instance, Boolean forScoreBoard = false)
        {
            if (__instance.IsPlayerMapEvent && (__instance.PlayerSide == __instance.WinningSide))
            {
                //IEnumerator<PartyBase> ieParty = __instance.PartiesOnSide(__instance.DefeatedSide).GetEnumerator();
                return;
                //TODO
            }
        }
    }
}