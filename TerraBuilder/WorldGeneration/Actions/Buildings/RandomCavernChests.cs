namespace TerraBuilder.WorldGeneration.Actions.Buildings
{
    using System;
    using System.ComponentModel;
    using TerraBuilder.Utils;
    using TerraBuilder.WorldGeneration;
    using TerraBuilder.WorldGeneration.Chests;
    using Terraria;

    /// <summary>
    /// 第二層にランダムにチェストを置くためのクラス
    /// </summary>
    [Action]
    public class RandomCavernChests : IWorldGenerationAction<RandomCavernChests.RandomCavernChestContext>
    {
        /// <inheritdoc/>
        public string Name => nameof(RandomCavernChests);

        /// <inheritdoc/>
        public string Description => "第二層にランダムにチェストを配置する";

        /// <inheritdoc/>
        public RandomCavernChestContext Context { get; set; } = new RandomCavernChestContext();

        /// <inheritdoc/>
        public bool Run(WorldSandbox sandbox)
        {
            if (string.IsNullOrEmpty(Context.ChestGroupName))
            {
                return false;
            }

            Random random = WorldGenerationRunner.CurrentRunner.GlobalContext.Random;
            double[] cavernTop = (double[])WorldGenerationRunner.CurrentRunner.GlobalContext["CavernTop"];
            const int worldEdgeTiles = 100;
            for (int i = 0; i < Context.ChestCount; i++)
            {
                ChestContext chestContext = GenerateChest.GetChestContextByRandom(random, Context.ChestGroupName);
                for (int retry = 0; retry < Context.MaxRetryForOneChest; retry++)
                {
                    int x = random.Next(worldEdgeTiles, sandbox.TileCountX - worldEdgeTiles);
                    if (PlaceChest(sandbox, x, (int)cavernTop[x] + 2, chestContext))
                    {
                        break;
                    }
                }
            }

            return true;
        }

        private bool PlaceChest(WorldSandbox sandbox, int x, int startY, ChestContext chestContext)
        {
            for (int y = startY; y < sandbox.TileCountY; y++)
            {
                if (sandbox.Tiles[x, y]?.active() == true && sandbox.Tiles[x + 1, y]?.active() == true)
                {
                    bool success = GenerateChest.PlaceChest(sandbox, x, y - 2, WorldGenerationRunner.CurrentRunner.GlobalContext.Random, chestContext);
                    if (success)
                    {
                        sandbox.TileProtectionMap.SetProtection(TileProtectionMap.TileProtectionType.All, x, y - 2, x + 1, y - 1);
                        return success;
                    }
                }
            }

            return false;
        }

        public class RandomCavernChestContext : ActionContext
        {
            /// <summary>
            /// チェストを1つ設置するまでのリトライ回数.チェストが設置できないとき=下が斜めブロックなどのときにリトライされる.
            /// </summary>
            [Category("地下チェスト生成")]
            [DisplayName("1チェストあたり最大リトライ数")]
            [Description("チェストを1つ設置するまでのリトライ回数.チェストが設置できないとき=下が斜めブロックなどのときにリトライされる.")]
            public int MaxRetryForOneChest { get; set; } = 100;

            /// <summary>
            /// チェストを設置する数.もしリトライに失敗した場合、この数より少なく生成される.
            /// </summary>
            [Category("地下チェスト生成")]
            [DisplayName("チェスト設置数")]
            [Description("チェストを設置する数.もしリトライに失敗した場合、この数より少なく生成される.")]
            public int ChestCount { get; set; } = 50;

            /// <summary>
            /// 地下に設置するチェストのチェストグループ名
            /// </summary>
            [Category("地下チェスト生成")]
            [DisplayName("設置するチェストグループ名")]
            [Description("地下に設置するチェストのチェストグループ名")]
            public string ChestGroupName { get; set; }
        }
    }
}
