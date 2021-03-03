namespace TUBGWorldGenerator.Utils
{
    using System;
    using System.Linq;
    using Terraria;
    using TUBGWorldGenerator.WorldGeneration;
    using TUBGWorldGenerator.WorldGeneration.Chests;

    public static class GenerateChest
    {
        public static bool PlaceChest(WorldSandbox sandbox, int x, int y, Random random, ChestContext chestContext)
        {
            int chestIndex = WorldGen.PlaceChest(x, y + 1, (ushort)chestContext.TileType, style: chestContext.TileStyle);
            if (chestIndex != -1)
            {
                sandbox.Chests[chestIndex] = GenerateChestByRandom(random, chestContext);
                sandbox.Chests[chestIndex].x = x;
                sandbox.Chests[chestIndex].y = y;
                sandbox.Tiles[x, y]?.color((byte)chestContext.Paint);
                sandbox.Tiles[x + 1, y]?.color((byte)chestContext.Paint);
                sandbox.Tiles[x, y + 1]?.color((byte)chestContext.Paint);
                sandbox.Tiles[x + 1, y + 1]?.color((byte)chestContext.Paint);
                return true;
            }
            else
            {
                return false;
            }

        }

        public static Chest GenerateChestByRandom(Random random, ChestContext chestContext)
        {
            Chest chest = new Chest();
            int itemIndex = 0;
            foreach (var itemSlot in chestContext.ItemSlots)
            {
                if (random.NextDouble() < itemSlot.Probably)
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

        public static ChestContext GetChestContextByRandom(Random random, string chestGroupName)
        {
            if (string.IsNullOrEmpty(chestGroupName))
            {
                return null;
            }

            var probablies = Configs.ChestGroups[chestGroupName].Values.ToArray();
            int selectedItemIndex = WeightedRandom.SelectIndex(random, probablies.Select(x => x.Probably));
            if (selectedItemIndex == -1)
            {
                return null;
            }

            return probablies[selectedItemIndex].ChestContext;
        }
    }
}
