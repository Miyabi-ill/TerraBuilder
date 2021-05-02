namespace TUBGWorldGenerator.ChestSimulator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TUBGWorldGenerator.Utils;
    using TUBGWorldGenerator.WorldGeneration;

    public class ChestAccumulateResult
    {
        public string ChestName { get; set; }

        public int Count { get; set; }

        public double Probably { get; set; }

        // アイテム名, (全体の出現数, アイテムが出現したチェスト数, チェストの中に含まれる平均個数, チェストに含まれる確率)
        public Dictionary<string, Tuple<int, int, double, double>> ItemsProbablys { get; set; } = new Dictionary<string, Tuple<int, int, double, double>>();

        public static (Dictionary<string, (int count, double probably)> chestProbably,
            Dictionary<string, Dictionary<string, (int count, double probably)>> itemSlotProbably,
            Dictionary<string, Dictionary<string, (int stack, int count, double probably)>> itemProbablyPerItemSlot,
            Dictionary<string, ChestAccumulateResult> chestAccResults)
            CreateResultFromChestGroupWithStep(string chestGroupName, int simulateChestCount = 10000)
        {
            Random random = new Random();
            Dictionary<string, (int count, double probably)> chestOverrollProbably = new Dictionary<string, (int count, double probably)>();
            Dictionary<string, Dictionary<string, (int count, double probably)>> itemSlotProbably = new Dictionary<string, Dictionary<string, (int count, double probably)>>();
            Dictionary<string, Dictionary<string, (int stack, int count, double probably)>> itemProbablyPerItemSlot = new Dictionary<string, Dictionary<string, (int stack, int count, double probably)>>();
            Dictionary<string, ChestAccumulateResult> chestAccResults = new Dictionary<string, ChestAccumulateResult>();
            for (int i = 0; i < simulateChestCount; i++)
            {
                var chestContext = GenerateChest.GetChestContextByRandom(random, chestGroupName);
                Dictionary<string, (int count, double probably)> itemSlotDict;
                ChestAccumulateResult chestAccumulateResult;

                // チェストに加算、アイテムスロット辞書を取得/作成
                if (chestOverrollProbably.ContainsKey(chestContext.Name))
                {
                    int currentCount = chestOverrollProbably[chestContext.Name].count;
                    chestOverrollProbably[chestContext.Name] = (currentCount + 1, (double)(currentCount + 1) / simulateChestCount);

                    itemSlotDict = itemSlotProbably[chestContext.Name];

                    chestAccumulateResult = chestAccResults[chestContext.Name];
                }
                else
                {
                    chestOverrollProbably.Add(chestContext.Name, (1, 1.0 / simulateChestCount));

                    itemSlotDict = new Dictionary<string, (int count, double probably)>();
                    itemSlotProbably.Add(chestContext.Name, itemSlotDict);

                    chestAccumulateResult = new ChestAccumulateResult() { ChestName = chestContext.Name };
                    chestAccResults.Add(chestContext.Name, chestAccumulateResult);
                }

                chestAccumulateResult.Count++;

                // アイテムスロット確率は後で計算
                // チェスト内アイテムの辞書
                var itemInChest = new Dictionary<string, int>();
                foreach (WorldGeneration.Chests.ItemSlotContext itemSlotContext in GenerateChest.GenerateItemSlotsByRandom(random, chestContext))
                {
                    // アイテムスロット毎アイテム数用辞書の準備と、アイテムスロットを数える
                    Dictionary<string, (int stack, int count, double probably)> itemDict;
                    if (itemSlotDict.ContainsKey(itemSlotContext.Name))
                    {
                        int currentCount = itemSlotDict[itemSlotContext.Name].count;
                        itemSlotDict[itemSlotContext.Name] = (currentCount + 1, 0);
                        itemDict = itemProbablyPerItemSlot[itemSlotContext.Name];
                    }
                    else
                    {
                        itemSlotDict.Add(itemSlotContext.Name, (1, 0));

                        if (itemProbablyPerItemSlot.ContainsKey(itemSlotContext.Name))
                        {
                            itemDict = itemProbablyPerItemSlot[itemSlotContext.Name];
                        }
                        else
                        {
                            itemDict = new Dictionary<string, (int stack, int count, double probably)>();
                            itemProbablyPerItemSlot.Add(itemSlotContext.Name, itemDict);
                        }
                    }

                    // アイテムスロット毎アイテムを数える
                    foreach (var item in GenerateChest.GenerateFromItemSlot(random, itemSlotContext))
                    {
                        if (item == null || string.IsNullOrEmpty(item.Name))
                        {
                            continue;
                        }

                        if (itemDict.ContainsKey(item.Name))
                        {
                            int currentCount = itemDict[item.Name].count;
                            int currentStack = itemDict[item.Name].stack;
                            itemDict[item.Name] = (currentStack + item.stack, currentCount + 1, 0);
                        }
                        else
                        {
                            itemDict.Add(item.Name, (item.stack, 1, 0));
                        }

                        if (itemInChest.ContainsKey(item.Name))
                        {
                            itemInChest[item.Name] += item.stack;
                        }
                        else
                        {
                            itemInChest.Add(item.Name, item.stack);
                        }
                    }
                }

                // ChestAccumulateResult用合算
                foreach (var itemProbably in itemInChest)
                {
                    if (chestAccumulateResult.ItemsProbablys.ContainsKey(itemProbably.Key))
                    {
                        chestAccumulateResult.ItemsProbablys[itemProbably.Key] = new Tuple<int, int, double, double>(
                            chestAccumulateResult.ItemsProbablys[itemProbably.Key].Item1 + itemProbably.Value,
                            chestAccumulateResult.ItemsProbablys[itemProbably.Key].Item2 + 1,
                            0,
                            0);
                    }
                    else
                    {
                        chestAccumulateResult.ItemsProbablys.Add(
                            itemProbably.Key,
                            new Tuple<int, int, double, double>(
                                itemProbably.Value,
                                1,
                                0,
                                0));
                    }
                }
            }

            // 確率計算
            // アイテムスロット
            foreach (string key in itemSlotProbably.Keys.ToArray())
            {
                // keyはチェスト名。
                int itemSlotSum = itemSlotProbably[key].Sum(x => x.Value.count);

                // アイテムスロットごとに確率を設定
                foreach (var itemSlotKey in itemSlotProbably[key].Keys.ToArray())
                {
                    int count = itemSlotProbably[key][itemSlotKey].count;
                    itemSlotProbably[key][itemSlotKey] = (count, (double)count / itemSlotSum);
                }
            }

            // アイテム
            foreach (string key in itemProbablyPerItemSlot.Keys.ToArray())
            {
                // keyはアイテムスロット名
                int itemsSum = itemProbablyPerItemSlot[key].Values.Sum(x => x.count);
                foreach (string itemKey in itemProbablyPerItemSlot[key].Keys.ToArray())
                {
                    var item = itemProbablyPerItemSlot[key][itemKey];
                    itemProbablyPerItemSlot[key][itemKey] = (item.stack, item.count, (double)item.count / itemsSum);
                }
            }

            // ChestAccumulateResult
            int resultSum = chestAccResults.Values.Sum(x => x.Count);
            foreach (ChestAccumulateResult accResult in chestAccResults.Values)
            {
                accResult.Probably = (double)accResult.Count / resultSum;
                foreach (string key in accResult.ItemsProbablys.Keys.ToList())
                {
                    int count = accResult.ItemsProbablys[key].Item1;
                    int itemChestCount = accResult.ItemsProbablys[key].Item2;
                    double avgCountInChest = count / (double)itemChestCount;
                    double chestContainsProb = itemChestCount / (double)accResult.Count;
                    accResult.ItemsProbablys[key] = new Tuple<int, int, double, double>(count, itemChestCount, avgCountInChest, chestContainsProb);
                }
            }

            return (chestOverrollProbably, itemSlotProbably, itemProbablyPerItemSlot, chestAccResults);
        }

        public static Dictionary<string, ChestAccumulateResult> CreateResultFromChestGroup(string chestGroupName, int simulateChestCount = 10000)
        {
            Random random = new Random();
            Dictionary<string, ChestAccumulateResult> chestAccumulateDict = new Dictionary<string, ChestAccumulateResult>();
            for (int i = 0; i < simulateChestCount; i++)
            {
                var chestContext = GenerateChest.GetChestContextByRandom(random, chestGroupName);
                var chest = GenerateChest.GenerateChestByRandom(random, chestContext);

                // 辞書にあるなら取得、なければ新規作成
                ChestAccumulateResult chestAccumulateResult;
                if (chestAccumulateDict.ContainsKey(chestContext.Name))
                {
                    chestAccumulateResult = chestAccumulateDict[chestContext.Name];
                }
                else
                {
                    chestAccumulateResult = new ChestAccumulateResult() { ChestName = chestContext.Name };
                    chestAccumulateDict.Add(chestContext.Name, chestAccumulateResult);
                }

                chestAccumulateResult.Count++;

                // アイテムのカウントを追加していく。確率は最後に計算する。
                var chestItemProbablys = new Dictionary<string, int>();
                foreach (var item in chest.item)
                {
                    if (item != null && !string.IsNullOrEmpty(item.Name))
                    {
                        if (chestItemProbablys.ContainsKey(item.Name))
                        {
                            chestItemProbablys[item.Name] += item.stack;
                        }
                        else
                        {
                            chestItemProbablys.Add(item.Name, item.stack);
                        }
                    }
                }

                // カウントしたチェスト内アイテムを全体と合算する
                foreach (var itemProbably in chestItemProbablys)
                {
                    if (chestAccumulateResult.ItemsProbablys.ContainsKey(itemProbably.Key))
                    {
                        chestAccumulateResult.ItemsProbablys[itemProbably.Key] = new Tuple<int, int, double, double>(
                            chestAccumulateResult.ItemsProbablys[itemProbably.Key].Item1 + itemProbably.Value,
                            chestAccumulateResult.ItemsProbablys[itemProbably.Key].Item2 + 1,
                            0,
                            0);
                    }
                    else
                    {
                        chestAccumulateResult.ItemsProbablys[itemProbably.Key] = new Tuple<int, int, double, double>(
                            itemProbably.Value,
                            1,
                            0,
                            0);
                    }
                }
            }

            int resultSum = chestAccumulateDict.Values.Sum(x => x.Count);

            // 確率を計算
            foreach (ChestAccumulateResult accResult in chestAccumulateDict.Values)
            {
                accResult.Probably = (double)accResult.Count / resultSum;
                foreach (string key in accResult.ItemsProbablys.Keys.ToList())
                {
                    int count = accResult.ItemsProbablys[key].Item1;
                    int itemChestCount = accResult.ItemsProbablys[key].Item2;
                    double avgCountInChest = count / (double)itemChestCount;
                    double chestContainsProb = itemChestCount / (double)accResult.Count;
                    accResult.ItemsProbablys[key] = new Tuple<int, int, double, double>(count, itemChestCount, avgCountInChest, chestContainsProb);
                }
            }

            return chestAccumulateDict;
        }

        public static Dictionary<string, ChestAccumulateResult> CreateResultFromWorld(WorldSandbox sandbox)
        {
            Random random = new Random(42);
            Dictionary<string, ChestAccumulateResult> chestAccumulateDict = new Dictionary<string, ChestAccumulateResult>();
            for (int i = 0; i < sandbox.Chests.Length; i++)
            {
                var chest = sandbox.Chests[i];
                if (chest == null)
                {
                    continue;
                }

                var chestTopLeftTile = sandbox.Tiles[chest.x, chest.y];
                string chestName = string.Format("Type:{0}, Style:{1}, Paint:{2}", chestTopLeftTile?.type, chestTopLeftTile?.frameX, chestTopLeftTile?.color());

                // 辞書にあるなら取得、なければ新規作成
                ChestAccumulateResult chestAccumulateResult;
                if (chestAccumulateDict.ContainsKey(chestName))
                {
                    chestAccumulateResult = chestAccumulateDict[chestName];
                }
                else
                {
                    chestAccumulateResult = new ChestAccumulateResult() { ChestName = chestName };
                    chestAccumulateDict.Add(chestName, chestAccumulateResult);
                }

                chestAccumulateResult.Count++;

                // アイテムのカウントを追加していく。確率は最後に計算する。
                var chestItemProbablys = new Dictionary<string, int>();
                foreach (var item in chest.item)
                {
                    if (item != null && !string.IsNullOrEmpty(item.Name))
                    {
                        if (chestItemProbablys.ContainsKey(item.Name))
                        {
                            chestItemProbablys[item.Name] += item.stack;
                        }
                        else
                        {
                            chestItemProbablys.Add(item.Name, item.stack);
                        }
                    }
                }

                // カウントしたチェスト内アイテムを全体と合算する
                foreach (var itemProbably in chestItemProbablys)
                {
                    if (chestAccumulateResult.ItemsProbablys.ContainsKey(itemProbably.Key))
                    {
                        chestAccumulateResult.ItemsProbablys[itemProbably.Key] = new Tuple<int, int, double, double>(
                            chestAccumulateResult.ItemsProbablys[itemProbably.Key].Item1 + itemProbably.Value,
                            chestAccumulateResult.ItemsProbablys[itemProbably.Key].Item2 + 1,
                            0,
                            0);
                    }
                    else
                    {
                        chestAccumulateResult.ItemsProbablys[itemProbably.Key] = new Tuple<int, int, double, double>(
                            itemProbably.Value,
                            1,
                            0,
                            0);
                    }
                }
            }

            int resultSum = chestAccumulateDict.Values.Sum(x => x.Count);

            // 確率を計算
            foreach (ChestAccumulateResult accResult in chestAccumulateDict.Values)
            {
                accResult.Probably = (double)accResult.Count / resultSum;
                foreach (string key in accResult.ItemsProbablys.Keys.ToList())
                {
                    int count = accResult.ItemsProbablys[key].Item1;
                    int itemChestCount = accResult.ItemsProbablys[key].Item2;
                    double avgCountInChest = count / (double)itemChestCount;
                    double chestContainsProb = itemChestCount / (double)accResult.Count;
                    accResult.ItemsProbablys[key] = new Tuple<int, int, double, double>(count, itemChestCount, avgCountInChest, chestContainsProb);
                }
            }

            return chestAccumulateDict;
        }

        public static ChestAccumulateResult CreateOverrollResult(IEnumerable<ChestAccumulateResult> chestAccumulateResults)
        {
            ChestAccumulateResult chestAccumulateResult = new ChestAccumulateResult()
            {
                ChestName = "総計",
                Probably = 1,
            };

            int chestCount = 0;

            foreach (ChestAccumulateResult result in chestAccumulateResults)
            {
                chestCount += result.Count;

                // アイテムのカウントを追加していく。確率は最後に計算する。
                foreach (var item in result.ItemsProbablys)
                {
                    if (chestAccumulateResult.ItemsProbablys.ContainsKey(item.Key))
                    {
                        chestAccumulateResult.ItemsProbablys[item.Key] = new Tuple<int, int, double, double>(
                            chestAccumulateResult.ItemsProbablys[item.Key].Item1 + item.Value.Item1,
                            chestAccumulateResult.ItemsProbablys[item.Key].Item2 + item.Value.Item2,
                            0,
                            0);
                    }
                    else
                    {
                        chestAccumulateResult.ItemsProbablys.Add(item.Key, new Tuple<int, int, double, double>(
                            item.Value.Item1,
                            item.Value.Item2,
                            0,
                            0));
                    }
                }
            }

            int resultSum = chestAccumulateResult.ItemsProbablys.Sum(x => x.Value.Item1);
            chestAccumulateResult.Count = chestCount;

            // 確率を計算
            foreach (string key in chestAccumulateResult.ItemsProbablys.Keys.ToList())
            {
                int count = chestAccumulateResult.ItemsProbablys[key].Item1;
                int itemChestCount = chestAccumulateResult.ItemsProbablys[key].Item2;
                double avgCountInChest = count / (double)itemChestCount;
                double chestContainsProb = itemChestCount / (double)chestCount;
                chestAccumulateResult.ItemsProbablys[key] = new Tuple<int, int, double, double>(count, itemChestCount, avgCountInChest, chestContainsProb);
            }

            return chestAccumulateResult;
        }
    }
}
