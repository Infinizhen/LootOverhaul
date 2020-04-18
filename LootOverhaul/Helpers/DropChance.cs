using System;

namespace LootOverhaul
{
    public class DropChance
    {
        float minItemChance;
        float maxItemChance;
        float minUnitChance;
        float maxUnitChance;

        private static readonly Random random = new Random(Guid.NewGuid().GetHashCode());

        public double CalculateChanceForUnit()
        {
            //this is the actual drop rate returned: a random between min and max.
            return RandomNumberBetween(minUnitChance, maxUnitChance);
        }
        public double CalculateChanceForItem()
        {
            //this is the actual drop rate returned: a random between min and max.
            return RandomNumberBetween(minItemChance, maxItemChance);
        }

        private static double RandomNumberBetween(double minValue, double maxValue)
        {
            var next = random.NextDouble();

            return minValue + (next * (maxValue - minValue));
        }

        private void SetChances()
        {
            minItemChance = LootOverhaulSettings.Instance.MinItemLootChance;
            maxItemChance = LootOverhaulSettings.Instance.MaxItemLootChance;
            minUnitChance = LootOverhaulSettings.Instance.MinUnitLootChance;
            maxUnitChance = LootOverhaulSettings.Instance.MaxUnitLootChance;

            SetItemChances();
            SetUnitChances();
        }

        private void SetItemChances()
        {
            if (minItemChance < 0f)
                minItemChance = 0;

            if (maxItemChance > 1.00f)
                maxItemChance = 1.00f;

            if (minItemChance > maxItemChance)
                maxItemChance = minItemChance;
        }

        private void SetUnitChances()
        {
            if (minUnitChance < 0)
                minUnitChance = 0;

            if (maxUnitChance > 1.00f)
                maxUnitChance = 1.00f;

            if (minUnitChance > maxUnitChance)
                maxUnitChance = minUnitChance;
        }

        public DropChance()
        {
            SetChances();
        }
    }
}