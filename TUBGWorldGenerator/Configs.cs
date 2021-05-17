namespace TUBGWorldGenerator
{
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;
    using Terraria.ID;
    using TUBGWorldGenerator.WorldGeneration;
    using TUBGWorldGenerator.WorldGeneration.Chests;

    public static class Configs
    {
        public static Dictionary<string, Dictionary<string, ChestProbably>> ChestGroups { get; private set; } = new Dictionary<string, Dictionary<string, ChestProbably>>();

        public static Dictionary<string, ChestContext> Chests { get; private set; } = new Dictionary<string, ChestContext>();

        public static Dictionary<string, ItemSlotContext> ItemSlots { get; private set; } = new Dictionary<string, ItemSlotContext>();

        public static Dictionary<string, ItemContext> Items { get; private set; } = new Dictionary<string, ItemContext>();

        // TODO: クラス全体をリファクタ
        // このクラスはコンフィグをまとめて読み込む用(パスを保存して読み込む用)のクラスにする
        internal static string LastChestConfigsDir
        {
            get => lastChestConfigsDir;
            set
            {
                lastChestConfigsDir = value;
                SaveConfigsPath();
            }
        }

        internal static string LastActionConfigPath
        {
            get => lastChestConfigPath;
            set
            {
                lastChestConfigPath = value;
                SaveConfigsPath();
            }
        }

        internal static string LastBuildingsPath
        {
            get => lastBuildingsPath;
            set
            {
                lastBuildingsPath = value;
                SaveConfigsPath();
            }
        }

        private static string lastChestConfigsDir;

        private static string lastChestConfigPath;

        private static string lastBuildingsPath;

        private static bool isLoading = false;

        private const string SavedConfigPath = "Config.json";

        public static void RecoverConfigsFromSaved()
        {
            isLoading = true;
            if (File.Exists(SavedConfigPath))
            {
                using (var sr = new StreamReader(SavedConfigPath))
                {
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());

                    if (dict.TryGetValue(nameof(LastChestConfigsDir), out string lastChestConfigsDir))
                    {
                        LastChestConfigsDir = lastChestConfigsDir;
                    }

                    if (dict.TryGetValue(nameof(LastActionConfigPath), out string lastChestConfigsPath))
                    {
                        LastActionConfigPath = lastChestConfigsPath;
                    }

                    if (dict.TryGetValue(nameof(LastBuildingsPath), out string lastBuildingsDir))
                    {
                        LastBuildingsPath = lastBuildingsDir;
                    }
                }
            }

            if (!string.IsNullOrEmpty(LastChestConfigsDir))
            {
                LoadAllChestConfigs(LastChestConfigsDir);
            }

            if (!string.IsNullOrEmpty(LastActionConfigPath))
            {
                WorldGenerationRunner.CurrentRunner.Load(LastActionConfigPath);
            }

            isLoading = false;
        }

        private static void SaveConfigsPath()
        {
            if (isLoading)
            {
                return;
            }

            Dictionary<string, string> configValues = new Dictionary<string, string>()
            {
                [nameof(LastChestConfigsDir)] = LastChestConfigsDir,
                [nameof(LastActionConfigPath)] = LastActionConfigPath,
                [nameof(LastBuildingsPath)] = LastBuildingsPath,
            };

            using (var sw = new StreamWriter(SavedConfigPath))
            {
                sw.Write(JsonConvert.SerializeObject(configValues, Formatting.Indented));
            }
        }

        public static void LoadAllChestConfigs(string dirName)
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
                    foreach (var item in Items)
                    {
                        item.Value.Name = item.Key;
                    }
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
                    foreach (var itemSlot in ItemSlots)
                    {
                        itemSlot.Value.Name = itemSlot.Key;
                    }
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

                    ItemSlotOrItemProbablyAndStack itemProbablyAndStack = new ItemSlotOrItemProbablyAndStack
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
                    foreach (var chest in Chests)
                    {
                        chest.Value.Name = chest.Key;
                    }
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
                    chestContext.ItemSlots.Add(new ItemSlotOrItemProbablyAndStack() { Name = "ExampleItemSlot" });
                    Chests.Add(chestContext.Name, chestContext);
                    sw.WriteLine(JsonConvert.SerializeObject(Chests, Formatting.Indented));
                }
            }

            if (File.Exists(Path.Combine(dirName, "ChestGroups.json")))
            {
                using (var sr = new StreamReader(Path.Combine(dirName, "ChestGroups.json")))
                {
                    ChestGroups = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ChestProbably>>>(sr.ReadToEnd());
                    foreach (var chestGroup in ChestGroups)
                    {
                        foreach (var chestProb in chestGroup.Value)
                        {
                            chestProb.Value.Name = chestProb.Key;
                        }
                    }
                }
            }
            else
            {
                using (var sw = new StreamWriter(Path.Combine(dirName, "ChestGroups.json")))
                {
                    var chests = new Dictionary<string, ChestProbably>();
                    ChestProbably chestProbably = new ChestProbably() { Name = "ExampleChest" };
                    chests.Add(chestProbably.Name, chestProbably);
                    ChestGroups.Add("ExampleGroup", chests);
                    sw.WriteLine(JsonConvert.SerializeObject(ChestGroups, Formatting.Indented));
                }
            }
        }
    }
}
