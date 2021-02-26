namespace TUBGWorldGenerator
{
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;
    using Terraria.ID;
    using TUBGWorldGenerator.WorldGeneration.Chests;

    public static class Configs
    {
        public static Dictionary<string, ChestProbably> ChestGroups { get; private set; } = new Dictionary<string, ChestProbably>();

        public static Dictionary<string, ChestContext> Chests { get; private set; } = new Dictionary<string, ChestContext>();

        public static Dictionary<string, ItemSlotContext> ItemSlots { get; private set; } = new Dictionary<string, ItemSlotContext>();

        public static Dictionary<string, ItemContext> Items { get; private set; } = new Dictionary<string, ItemContext>();

        public static void LoadAll(string dirName)
        {
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            if (File.Exists(Path.Combine(dirName, "Items.json")))
            {
                using (var sr = new StreamReader(Path.Combine(dirName, "Items.json")))
                {
                    Items = JsonConvert.DeserializeObject<Dictionary<string, ItemContext>>(sr.ReadToEnd());
                }
            }
            else
            {
                using (var sw = new StreamWriter(Path.Combine(dirName, "Items.json")))
                {
                    ItemContext itemContext = new ItemContext()
                    {
                        Name = "Magic Mirror",
                        ItemID = ItemID.MagicMirror,
                    };
                    Items.Add(itemContext.Name, itemContext);
                    sw.WriteLine(JsonConvert.SerializeObject(Items, Formatting.Indented));
                }
            }

            if (File.Exists(Path.Combine(dirName, "ItemSlots.json")))
            {
                using (var sr = new StreamReader(Path.Combine(dirName, "ItemSlots.json")))
                {
                    ItemSlots = JsonConvert.DeserializeObject<Dictionary<string, ItemSlotContext>>(sr.ReadToEnd());
                }
            }
            else
            {
                using (var sw = new StreamWriter(Path.Combine(dirName, "ItemSlots.json")))
                {
                    ItemSlotContext itemSlotContext = new ItemSlotContext
                    {
                        Name = "ExampleItemSlot",
                    };

                    ItemProbablyAndStack itemProbablyAndStack = new ItemProbablyAndStack
                    {
                        Name = "Magic Mirror",
                    };
                    itemSlotContext.Items.Add(itemProbablyAndStack);
                    ItemSlots.Add(itemSlotContext.Name, itemSlotContext);
                    sw.WriteLine(JsonConvert.SerializeObject(ItemSlots, Formatting.Indented));
                }
            }

            if (File.Exists(Path.Combine(dirName, "Chests.json")))
            {
                using (var sr = new StreamReader(Path.Combine(dirName, "Chests.json")))
                {
                    Chests = JsonConvert.DeserializeObject<Dictionary<string, ChestContext>>(sr.ReadToEnd());
                }
            }
            else
            {
                using (var sw = new StreamWriter(Path.Combine(dirName, "Chests.json")))
                {
                    ChestContext chestContext = new ChestContext
                    {
                        Name = "ExampleChest",
                    };
                    chestContext.ItemSlots.Add(new ItemSlotProbablyAndStack() { Name = "ExampleItemSlot" });
                    Chests.Add(chestContext.Name, chestContext);
                    sw.WriteLine(JsonConvert.SerializeObject(Chests, Formatting.Indented));
                }
            }

            if (File.Exists(Path.Combine(dirName, "ChestGroups.json")))
            {
                using (var sr = new StreamReader(Path.Combine(dirName, "ChestGroups.json")))
                {
                    ChestGroups = JsonConvert.DeserializeObject<Dictionary<string, ChestProbably>>(sr.ReadToEnd());
                }
            }
            else
            {
                using (var sw = new StreamWriter(Path.Combine(dirName, "ChestGroups.json")))
                {
                    ChestProbably chestProbably = new ChestProbably() { Name = "ExampleChest" };
                    ChestGroups.Add(chestProbably.Name, chestProbably);
                    sw.WriteLine(JsonConvert.SerializeObject(ChestGroups, Formatting.Indented));
                }
            }
        }
    }
}
