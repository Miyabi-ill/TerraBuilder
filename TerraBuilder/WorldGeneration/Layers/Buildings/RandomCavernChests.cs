// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TerraBuilder.WorldGeneration.Layers.Buildings
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using TerraBuilder.Chests;
    using TerraBuilder.Utils;
    using TerraBuilder.WorldEdit;
    using TerraBuilder.WorldGeneration;

    /// <summary>
    /// 第二層にランダムにチェストを置くためのクラス.
    /// </summary>
    [Action]
    public class RandomCavernChests : IWorldGenerationLayer<RandomCavernChests.RandomCavernChestConfig>
    {
        /// <inheritdoc/>
        public string Name => nameof(RandomCavernChests);

        /// <inheritdoc/>
        public string Description => "第二層にランダムにチェストを配置する";

        /// <inheritdoc/>
        public RandomCavernChestConfig Config { get; set; } = new RandomCavernChestConfig();

        /// <inheritdoc/>
        public bool Apply(WorldGenerationRunner runner, WorldSandbox sandbox, out Dictionary<string, object> generatedValueDict)
        {
            if (string.IsNullOrEmpty(this.Config.ChestGroupName))
            {
                generatedValueDict = new Dictionary<string, object>();
                return false;
            }

            Random random = runner.GlobalConfig.Random;
            double[] cavernTop = runner.GetGeneratedValue<Biomes.Caverns, double[]>("CavernTop");
            const int worldEdgeTiles = 100;
            for (int i = 0; i < this.Config.ChestCount; i++)
            {
                ChestContext chestContext = GenerateChest.GetChestContextByRandom(random, this.Config.ChestGroupName);
                for (int retry = 0; retry < this.Config.MaxRetryForOneChest; retry++)
                {
                    int x = random.Next(worldEdgeTiles, sandbox.TileCountX - worldEdgeTiles);
                    if (this.PlaceChest(sandbox, x, (int)cavernTop[x] + 2, chestContext))
                    {
                        break;
                    }
                }
            }

            generatedValueDict = new Dictionary<string, object>();
            return true;
        }

        private bool PlaceChest(WorldSandbox sandbox, int x, int startY, ChestContext chestContext)
        {
            for (int y = startY; y < sandbox.TileCountY; y++)
            {
                if (sandbox[new Coordinate(x, y)]?.active() == true && sandbox[new Coordinate(x + 1, y)]?.active() == true)
                {
                    bool success = GenerateChest.PlaceChest(sandbox, x, y - 2, runner.GlobalConfig.Random, chestContext);
                    if (success)
                    {
                        sandbox.TileProtectionMap.AddProtection(new Coordinate(x, y - 2), TileProtectionMap.TileProtectionType.All);
                        sandbox.TileProtectionMap.AddProtection(new Coordinate(x, y - 1), TileProtectionMap.TileProtectionType.All);
                        sandbox.TileProtectionMap.AddProtection(new Coordinate(x + 1, y - 2), TileProtectionMap.TileProtectionType.All);
                        sandbox.TileProtectionMap.AddProtection(new Coordinate(x + 1, y - 1), TileProtectionMap.TileProtectionType.All);
                        return success;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 地下にランダムにチェストを置くクラスのコンフィグ.
        /// </summary>
        public class RandomCavernChestConfig : LayerConfig
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
            /// 地下に設置するチェストのチェストグループ名.
            /// </summary>
            [Category("地下チェスト生成")]
            [DisplayName("設置するチェストグループ名")]
            [Description("地下に設置するチェストのチェストグループ名")]
            public string ChestGroupName { get; set; }
        }
    }
}
