namespace TUBGWorldGenerator.Utils
{
    using System;
    using System.Linq;
    using Terraria;
    using TUBGWorldGenerator.WorldGeneration.Chests;

    public static class GenerateChest
    {
        public static Chest GenerateChestByRandom(Random random, ChestContext chestContext)
        {
            Chest chest = new Chest();
            int itemIndex = 0;
            foreach (var itemSlot in chestContext.ItemSlots)
            {
                if (itemSlot.Probably < random.NextDouble())
                {
                    int stackCount = random.Next(itemSlot.Min, itemSlot.Max + 1);
                    for (int i = 0; i < stackCount; i++)
                    {
                        int selectedItemIndex = WeightedRandom.SelectIndex(random, itemSlot.Context.Items.Select(x => x.Probably));
                        if (selectedItemIndex == -1)
                        {
                            continue;
                        }

                        Item item = new Item();
                        var itemContext = itemSlot.Context.Items[selectedItemIndex];
                        item.SetDefaults(itemContext.Context.ItemID);
                        item.stack = random.Next(itemContext.Min, itemContext.Max);
                        item.prefix = (byte)itemContext.Context.PrefixID;
                        chest.item[itemIndex] = item;
                        itemIndex++;
                    }
                }
            }

            return chest;
        }
    }
}
