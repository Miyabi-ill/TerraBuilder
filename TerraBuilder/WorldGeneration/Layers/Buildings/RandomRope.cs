// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TerraBuilder.WorldGeneration.Layers.Buildings
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using TerraBuilder.WorldEdit;
    using Terraria;
    using Terraria.ID;

    /// <summary>
    /// ロープをランダムに配置する.
    /// </summary>
    [Action]
    public class RandomRope : IWorldGenerationLayer<RandomRope.RandomRopeConfig>
    {
        /// <inheritdoc/>
        public string Name => nameof(RandomRope);

        /// <inheritdoc/>
        public string Description => "地下にロープをランダムに配置する";

        /// <inheritdoc/>
        public RandomRopeConfig Config { get; set; } = new RandomRopeConfig();

        /// <inheritdoc/>
        public bool Apply(WorldGenerationRunner runner, WorldSandbox sandbox, out Dictionary<string, object> generatedValueDict)
        {
            double[] cavernTop = runner.GetGeneratedValue<Biomes.Caverns, double[]>("CavernTop");
            double[] cavernBottom = runner.GetGeneratedValue<Biomes.Caverns, double[]>("CavernBottom");
            for (int i = 0; i < this.Config.RopeCount; i++)
            {
                int x = runner.Random.Next(100, sandbox.TileCountX - 100);
                int ropeLength = runner.Random.Next(this.Config.RopeMinLength, this.Config.RopeMaxLength + 1);
                int y = runner.Random.Next((int)cavernTop[x], Math.Max((int)cavernBottom[x] - ropeLength - 1, (int)cavernTop[x] + 1));

                for (int cy = y; cy < y + ropeLength; cy++)
                {
                    Coordinate coordinate = new Coordinate(x, cy);
                    if (!sandbox[coordinate].active())
                    {
                        Tile tile = sandbox[coordinate];
                        tile = (Tile)tile.Clone();
                        tile.type = TileID.Rope;
                        tile.active(true);
                        _ = sandbox.PlaceTile(coordinate, tile);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            generatedValueDict = new Dictionary<string, object>();
            return true;
        }

        /// <summary>
        /// ランダムにロープを設置するクラスのコンフィグ.
        /// </summary>
        public class RandomRopeConfig : LayerConfig
        {
            /// <summary>
            /// ロープを設置する数.
            /// </summary>
            [Category("ロープ生成")]
            [DisplayName("ロープ設置数")]
            [Description("ロープを設置する数")]
            public int RopeCount { get; set; } = 300;

            /// <summary>
            /// ロープ最小長さ.
            /// </summary>
            [Category("ロープ生成")]
            [DisplayName("ロープ最小長さ")]
            [Description("ロープの最小長さ")]
            public int RopeMinLength { get; set; } = 15;

            /// <summary>
            /// ロープの最大長さ.
            /// </summary>
            [Category("ロープ生成")]
            [DisplayName("ロープ最大長さ")]
            [Description("ロープの最大長さ")]
            public int RopeMaxLength { get; set; } = 50;
        }
    }
}
