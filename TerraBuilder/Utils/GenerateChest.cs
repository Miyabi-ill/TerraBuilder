namespace TerraBuilder.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Terraria;
    using TerraBuilder.WorldGeneration;
    using TerraBuilder.WorldGeneration.Chests;

    public static class GenerateChest
    {
        public static bool PlaceChest(WorldSandbox sandbox, int x, int y, Random random, ChestContext chestContext)
        {
            if (chestContext == null)
            {
                return false;
            }

            // タイル保護をチェック
            for (int i = x; i < x + 2; i++)
            {
                for (int j = y; j < y + 2; j++)
                {
                    if (sandbox.TileProtectionMap[i, j].HasFlag(TileProtectionMap.TileProtectionType.TopSolid)
                        || sandbox.TileProtectionMap[i, j].HasFlag(TileProtectionMap.TileProtectionType.BottomSolid)
                        || sandbox.TileProtectionMap[i, j].HasFlag(TileProtectionMap.TileProtectionType.RightSolid)
                        || sandbox.TileProtectionMap[i, j].HasFlag(TileProtectionMap.TileProtectionType.LeftSolid)
                        || sandbox.TileProtectionMap[i, j].HasFlag(TileProtectionMap.TileProtectionType.TileType)
                        || sandbox.TileProtectionMap[i, j].HasFlag(TileProtectionMap.TileProtectionType.TileShape)
                        || sandbox.TileProtectionMap[i, j].HasFlag(TileProtectionMap.TileProtectionType.TileFrame))
                    {
                        return false;
                    }
                }
            }

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
            List<Item> items = new List<Item>();
            foreach (var itemSlotContext in GenerateItemSlotsByRandom(random, chestContext))
            {
                items.AddRange(GenerateFromItemSlot(random, itemSlotContext));
            }

            for (int i = 0; i < chest.item.Length; i++)
            {
                if (i >= items.Count)
                {
                    break;
                }

                chest.item[i] = items[i];
            }

            return chest;
        }

        public static IEnumerable<ItemSlotContext> GenerateItemSlotsByRandom(Random random, ChestContext chestContext)
        {
            foreach (ItemSlotOrItemProbablyAndStack itemSlot in chestContext.ItemSlots)
            {
                if (random.NextDouble() < itemSlot.Probably)
                {
                    int count = random.Next(itemSlot.Min, itemSlot.Max + 1);
                    for (int i = 0; i < count; i++)
                    {
                        yield return itemSlot.ItemSlotContext;
                    }
                }
            }
        }

        public static ChestContext GetChestContextByRandom(Random random, string chestGroupName)
        {
            if (string.IsNullOrEmpty(chestGroupName) || !Configs.ChestGroups.ContainsKey(chestGroupName))
            {
                return null;
            }

            var probablies = Configs.ChestGroups[chestGroupName];
            int selectedItemIndex = WeightedRandom.SelectIndex(random, probablies.Select(x => x.Probably));
            if (selectedItemIndex == -1)
            {
                return null;
            }

            return probablies[selectedItemIndex].ChestContext;
        }

        public static IEnumerable<Item> GenerateFromItemSlot(Random random, ItemSlotContext itemSlot)
        {
            int selectedItemIndex = WeightedRandom.SelectIndex(random, itemSlot.Items.Select(x => x.Probably));
            if (selectedItemIndex == -1)
            {
                return new List<Item>();
            }

            List<Item> items = new List<Item>();
            ItemSlotOrItemProbablyAndStack probablyAndStack = itemSlot.Items[selectedItemIndex];
            if (probablyAndStack.ItemContext != null)
            {
                Item item = new Item();
                ItemContext itemContext = probablyAndStack.ItemContext;
                item.SetDefaults(itemContext.ItemID);
                item.stack = random.Next(probablyAndStack.Min, probablyAndStack.Max);
                item.prefix = (byte)itemContext.PrefixID;
                items.Add(item);
                return items;
            }

            if (probablyAndStack.ItemSlotContext != null)
            {
                int slotCount = random.Next(probablyAndStack.Min, probablyAndStack.Max);
                for (int i = 0; i < slotCount; i++)
                {
                    items.AddRange(GenerateFromItemSlot(random, probablyAndStack.ItemSlotContext));
                }

                return items;
            }

            return items;
        }
    }
}
