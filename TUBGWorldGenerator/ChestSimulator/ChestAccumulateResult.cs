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
