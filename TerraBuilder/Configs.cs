namespace TerraBuilder
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using Newtonsoft.Json;
    using TerraBuilder.WorldGeneration;
    using TerraBuilder.Chests;
    using Terraria.ID;

    public static class Configs
    {
        private static ObservableCollection<ItemContext> itemsCollection;

        public static Dictionary<string, ObservableCollection<ChestProbably>> ChestGroups { get; private set; } = new Dictionary<string, ObservableCollection<ChestProbably>>();

        public static Dictionary<string, ChestContext> Chests { get; private set; } = new Dictionary<string, ChestContext>();

        public static Dictionary<string, ItemSlotContext> ItemSlots { get; private set; } = new Dictionary<string, ItemSlotContext>();

        public static Dictionary<string, ItemContext> Items { get; private set; } = new Dictionary<string, ItemContext>();

        public static ObservableCollection<ItemContext> ItemsCollection
        {
            get => itemsCollection;
            set
            {
                itemsCollection = value;
                itemsCollection.CollectionChanged += ItemsCollection_CollectionChanged;
            }
        }

        /// <summary>
        /// ItemsCollectionとItemsを連携する.
        /// ItemsCollection -> Itemsの１方向のみ.
        /// </summary>
        /// <param name="sender">コレクション</param>
        /// <param name="e">イベント</param>
        private static void ItemsCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    {
                        foreach (ItemContext item in e.NewItems)
                        {
                            if (string.IsNullOrWhiteSpace(item?.Name))
                            {
                                continue;
                            }

                            if (!Items.ContainsKey(item.Name))
                            {
                                Items.Add(item.Name, item);
                            }
                        }

                        break;
                    }

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    {
                        foreach (ItemContext item in e.OldItems)
                        {
                            if (Items.ContainsKey(item.Name))
                            {
                                Items.Remove(item.Name);
                            }
                        }

                        break;
                    }

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    {
                        foreach (ItemContext item in e.OldItems)
                        {
                            if (Items.ContainsKey(item.Name))
                            {
                                Items.Remove(item.Name);
                            }
                        }

                        foreach (ItemContext item in e.NewItems)
                        {
                            if (!Items.ContainsKey(item.Name))
                            {
                                Items.Add(item.Name, item);
                            }
                        }

                        break;
                    }

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    {
                        Items.Clear();

                        break;
                    }
            }
        }

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
                runner.Load(LastActionConfigPath);
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

        public static void SaveAllChestConfigs(string dirName)
        {
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            using (var sw = new StreamWriter(Path.Combine(dirName, "Items.json")))
            {
                sw.WriteLine(JsonConvert.SerializeObject(Items));
            }

            using (var sw = new StreamWriter(Path.Combine(dirName, "ItemSlots.json")))
            {
                sw.WriteLine(JsonConvert.SerializeObject(ItemSlots));
            }

            using (var sw = new StreamWriter(Path.Combine(dirName, "Chests.json")))
            {
                sw.WriteLine(JsonConvert.SerializeObject(Chests));
            }

            using (var sw = new StreamWriter(Path.Combine(dirName, "ChestGroups.json")))
            {
                sw.WriteLine(JsonConvert.SerializeObject(ChestGroups));
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

                    ItemsCollection = new ObservableCollection<ItemContext>(Items.Values);
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
                    ChestGroups = JsonConvert.DeserializeObject<Dictionary<string, ObservableCollection<ChestProbably>>>(sr.ReadToEnd());
                }
            }
            else
            {
                using (var sw = new StreamWriter(Path.Combine(dirName, "ChestGroups.json")))
                {
                    var chests = new ObservableCollection<ChestProbably>();
                    ChestProbably chestProbably = new ChestProbably() { Name = "ExampleChest" };
                    chests.Add(chestProbably);
                    ChestGroups.Add("ExampleGroup", chests);
                    sw.WriteLine(JsonConvert.SerializeObject(ChestGroups, Formatting.Indented));
                }
            }
        }
    }
}
