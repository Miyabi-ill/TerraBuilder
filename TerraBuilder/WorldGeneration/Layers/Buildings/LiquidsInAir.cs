// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TerraBuilder.WorldGeneration.Layers.Buildings
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using TerraBuilder.Utils;
    using TerraBuilder.WorldEdit;
    using Terraria;
    using Terraria.ID;

    /// <summary>
    /// 空中に水を生成する.
    /// </summary>
    [Action]
    public class LiquidsInAir : IWorldGenerationLayer<LiquidsInAir.LiquidsInAirConfig>
    {
        /// <inheritdoc/>
        public string Name => nameof(LiquidsInAir);

        /// <inheritdoc/>
        public string Description => "空中にバブルブロックで囲った液体を設置する.";

        /// <inheritdoc/>
        public LiquidsInAirConfig Config { get; set; } = new LiquidsInAirConfig();

        /// <inheritdoc/>
        public bool Apply(WorldGenerationRunner runner, WorldSandbox sandbox, out Dictionary<string, object> generatedValueDict)
        {
            var globalContext = runner.GlobalConfig;

            const int xPadding = 5;
            for (int i = 0; i < this.Config.LiquidCount; i++)
            {
                for (int retry = 0; retry < this.Config.MaxRetry; retry++)
                {
                    int width = globalContext.Random.Next(this.Config.LiquidMinWidth, this.Config.LiquidMaxWidth + 1);
                    int height = globalContext.Random.Next(this.Config.LiquidMinHeight, this.Config.LiquidMaxHeight + 1);

                    int x = RandGen.SelectPositionInRange(globalContext.Random, xPadding, sandbox.TileCountX - xPadding, width + 2);
                    int y = RandGen.SelectPositionInRange(globalContext.Random, this.Config.PlaceAreaMinY, this.Config.PlaceAreaMaxY + 1, height + 2);

                    if (x == -1 || y == -1)
                    {
                        continue;
                    }

                    // バブルブロックぶんだけずらしてから範囲に上書き不可ブロックがないか確認
                    bool canPlace = true;
                    for (int tx = x; tx < x + width + 2; tx++)
                    {
                        for (int ty = y; ty < y + height + 2; ty++)
                        {
                            Coordinate coordinate = new Coordinate(tx, ty);
                            if (sandbox[coordinate]?.active() == true
                                || sandbox.TileProtectionMap[coordinate].HasFlag(TileProtectionMap.TileProtectionType.Liquid))
                            {
                                canPlace = false;
                                break;
                            }
                        }

                        if (!canPlace)
                        {
                            break;
                        }
                    }

                    if (!canPlace)
                    {
                        continue;
                    }

                    // 設置してbreak
                    // 0: water, 1: lava, 2: honey
                    int liquidType = WeightedRandom.SelectIndex(globalContext.Random, new double[] { this.Config.WaterWeight, this.Config.LavaWeight, this.Config.HoneyWeight });
                    if (liquidType == -1)
                    {
                        liquidType = 0;
                    }

                    for (int tx = x; tx < x + width + 2; tx++)
                    {
                        for (int ty = y; ty < y + height + 2; ty++)
                        {
                            Coordinate coordinate = new Coordinate(tx, ty);
                            Tile tile = sandbox[coordinate];
                            if (tile == null)
                            {
                                tile = new Tile();
                                tile = sandbox.PlaceTile(coordinate, tile);
                            }

                            // 四辺はBubble Block
                            if (tx == x
                                || tx == x + width + 1
                                || ty == y
                                || ty == y + height + 1)
                            {
                                tile.type = TileID.Bubble;
                                tile.active(true);
                                _ = sandbox.PlaceTile(coordinate, tile);
                            }
                            else
                            {
                                tile.liquid = 255;
                                tile.liquidType(liquidType);
                                _ = sandbox.PlaceTile(coordinate, tile);
                            }
                        }
                    }

                    break;
                }
            }

            generatedValueDict = new Dictionary<string, object>();
            return true;
        }

        /// <summary>
        /// 空中に液体を設置するクラスのコンフィグ.
        /// </summary>
        public class LiquidsInAirConfig : LayerConfig
        {
            /// <summary>
            /// 空中に設置する液体の最小幅を設定する.実際に生成されるサイズは液体幅+2(左右にバブルブロックが設置されるため)となる.
            /// </summary>
            [Category("液体")]
            [DisplayName("液体最小幅")]
            [Description("空中に設置する液体の最小幅を設定する.実際に生成されるサイズは液体幅+2(左右にバブルブロックが設置されるため)となる")]
            public int LiquidMinWidth { get; set; } = 1;

            /// <summary>
            /// 空中に設置する液体の最大幅を設定する.実際に生成されるサイズは液体幅+2(左右にバブルブロックが設置されるため)となる.
            /// </summary>
            [Category("液体")]
            [DisplayName("液体最大幅")]
            [Description("空中に設置する液体の最大幅を設定する.実際に生成されるサイズは液体幅+2(左右にバブルブロックが設置されるため)となる")]
            public int LiquidMaxWidth { get; set; } = 5;

            /// <summary>
            /// 空中に設置する液体の最小高さを設定する.実際に生成されるサイズは液体高さ+2(上下にバブルブロックが設置されるため)となる.
            /// </summary>
            [Category("液体")]
            [DisplayName("液体最小高さ")]
            [Description("空中に設置する液体の最小高さを設定する.実際に生成されるサイズは液体高さ+2(上下にバブルブロックが設置されるため)となる")]
            public int LiquidMinHeight { get; set; } = 1;

            /// <summary>
            /// 空中に設置する液体の最大高さを設定する.実際に生成されるサイズは液体高さ+2(上下にバブルブロックが設置されるため)となる.
            /// </summary>
            [Category("液体")]
            [DisplayName("液体最大高さ")]
            [Description("空中に設置する液体の最大高さを設定する.実際に生成されるサイズは液体高さ+2(上下にバブルブロックが設置されるため)となる")]
            public int LiquidMaxHeight { get; set; } = 5;

            /// <summary>
            /// 水がどの程度生成されるかの重みづけを行う.水を1, 溶岩0, はちみつ1に設定した場合、水とはちみつが50%づつの確率で生成される.
            /// </summary>
            [Category("液体種類")]
            [DisplayName("水生成重み")]
            [Description("水がどの程度生成されるかの重みづけを行う.水を1, 溶岩0, はちみつ1に設定した場合、水とはちみつが50%づつの確率で生成される.")]
            public double WaterWeight { get; set; } = 1;

            /// <summary>
            /// 溶岩がどの程度生成されるかの重みづけを行う.水を0.5, 溶岩1, はちみつ0.5に設定した場合、水とはちみつが25%づつ、溶岩が50%の確率で生成される.
            /// </summary>
            [Category("液体種類")]
            [DisplayName("溶岩生成重み")]
            [Description("溶岩がどの程度生成されるかの重みづけを行う.水を0.5, 溶岩1, はちみつ0.5に設定した場合、水とはちみつが25%づつ、溶岩が50%の確率で生成される.")]
            public double LavaWeight { get; set; } = 1;

            /// <summary>
            /// はちみつがどの程度生成されるかの重みづけを行う.水を1, 溶岩1, はちみつ1に設定した場合、水と溶岩とはちみつが33.33%づつ生成される.
            /// </summary>
            [Category("液体種類")]
            [DisplayName("はちみつ生成重み")]
            [Description("はちみつがどの程度生成されるかの重みづけを行う.水を1, 溶岩1, はちみつ1に設定した場合、水と溶岩とはちみつが33.33%づつ生成される.")]
            public double HoneyWeight { get; set; } = 1;

            /// <summary>
            /// 液体が設置されるエリアの上限(宇宙側)を設定する.これと最大Yの間にのみ液体が設置される.
            /// </summary>
            [Category("液体設置")]
            [DisplayName("液体設置エリア最小Y")]
            [Description("液体が設置されるエリアの上限(宇宙側)を設定する.これと最大Yの間にのみ液体が設置される.")]
            public int PlaceAreaMinY { get; set; } = 150;

            /// <summary>
            /// 液体が設置されるエリアの下限(地獄側)を設定する.これと最小Yの間にのみ液体が設置される.
            /// </summary>
            [Category("液体設置")]
            [DisplayName("液体設置エリア最大Y")]
            [Description("液体が設置されるエリアの下限(地獄側)を設定する.これと最小Yの間にのみ液体が設置される.")]
            public int PlaceAreaMaxY { get; set; } = 250;

            /// <summary>
            /// 設置する液体塊の個数.
            /// </summary>
            [Category("液体設置")]
            [DisplayName("液体設置数")]
            [Description("設置する液体塊の個数")]
            public int LiquidCount { get; set; } = 200;

            /// <summary>
            /// 液体設置をリトライする回数.リトライする原因=すでに地形が生成されている、建物が生成されているなど.
            /// </summary>
            [Category("液体設置")]
            [DisplayName("液体設置リトライ数")]
            [Description("液体設置をリトライする回数.リトライする原因=すでに地形が生成されている、建物が生成されているなど")]
            public int MaxRetry { get; set; } = 100;
        }
    }
}
