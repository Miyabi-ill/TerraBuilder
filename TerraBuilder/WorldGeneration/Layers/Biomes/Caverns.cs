// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TerraBuilder.WorldGeneration.Layers.Biomes
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using TerraBuilder.Utils;
    using TerraBuilder.WorldEdit;
    using Terraria;
    using Terraria.ID;

    /// <summary>
    /// 洞窟(第二層)の生成を行う.
    /// </summary>
    [Action]
    public class Caverns : IWorldGenerationLayer<Caverns.CavernConfig>
    {
        /// <inheritdoc/>
        public string Name => nameof(Caverns);

        /// <inheritdoc/>
        public string Description => "地下に洞窟を生成する.";

        /// <inheritdoc/>
        public CavernConfig Config { get; } = new CavernConfig();

        /// <inheritdoc/>
        public bool Apply(WorldGenerationRunner runner, WorldSandbox sandbox, out Dictionary<string, object> generatedValueDict)
        {
            GlobalConfig globalContext = runner.GlobalConfig;

            int tileLengthX = sandbox.TileCountX;

            // Surface: topPerlinの振幅
            int diffSurface = this.Config.CavernMaxDistanceFromSurface - this.Config.CavernMinDistanceFromSurface;

            Random rand = globalContext.Random;
            double[] topPerlin = PerlinNoise.NormalizeOctave1D(128, tileLengthX, 8, 2, rand);
            double[] bottomPerlin = PerlinNoise.NormalizeOctave1D(128, tileLengthX, 8, 2, rand);

            int minIndex = 0;
            double minValue = double.MaxValue;
            double maxValue = double.MinValue;
            for (int i = 0; i < tileLengthX; i++)
            {
                double diff = Math.Abs(topPerlin[i] - 1) + bottomPerlin[i];
                if (diff > maxValue)
                {
                    maxValue = diff;
                }

                if (diff < minValue)
                {
                    minValue = diff;
                    minIndex = i;
                }
            }

            // 空間のminとmaxから波の増幅量を決定、適用.タイルで使うために四捨五入しておく.
            double bottomAmplifier = this.Config.CavernMaxHeight - diffSurface - this.Config.CavernMinHeight;
            if (bottomAmplifier < 0)
            {
                generatedValueDict = new Dictionary<string, object>();
                return false;
            }

            for (int i = 0; i < tileLengthX; i++)
            {
                topPerlin[i] = Math.Round(topPerlin[i] * diffSurface);
                bottomPerlin[i] = Math.Round(bottomPerlin[i] * bottomAmplifier);
            }

            int cavernStart = globalContext.SurfaceLevel + this.Config.CavernMinDistanceFromSurface;
            int bottomPerlinBaseTopLine = (int)topPerlin[minIndex] - (int)bottomPerlin[minIndex] + this.Config.CavernMinHeight;
            double[] cavernTop = new double[topPerlin.Length];
            double[] cavernBottom = new double[bottomPerlin.Length];

            for (int x = 0; x < sandbox.TileCountX; x++)
            {
                cavernTop[x] = cavernStart + topPerlin[x];
                cavernBottom[x] = cavernStart + bottomPerlinBaseTopLine + bottomPerlin[x];
                for (int y = globalContext.SurfaceLevel; y < sandbox.TileCountY; y++)
                {
                    Coordinate coordinate = new Coordinate(x, y);
                    if (y < cavernStart + topPerlin[x])
                    {
                        Tile tile = new Tile()
                        {
                            type = TileID.GrayBrick,
                            wall = WallID.GrayBrick,
                        };
                        tile.active(true);
                        _ = sandbox.PlaceTile(coordinate, tile);
                    }
                    else if (y > cavernStart + bottomPerlinBaseTopLine + bottomPerlin[x])
                    {
                        Tile tile = new Tile()
                        {
                            type = TileID.GrayBrick,
                            wall = WallID.GrayBrick,
                        };
                        tile.active(true);
                        _ = sandbox.PlaceTile(coordinate, tile);
                    }
                    else
                    {
                        Tile tile = new Tile()
                        {
                            wall = WallID.GrayBrick,
                        };
                        _ = sandbox.PlaceTile(coordinate, tile);
                    }
                }
            }

            int generatedBlockCount = 0;
            for (int i = 0; i < this.Config.BlockCount; i++)
            {
                int x = rand.Next(100, sandbox.TileCountX - 100);
                if ((int)cavernTop[x] + this.Config.MinDistanceFromCavernTopAndBottom < (int)cavernBottom[x] - this.Config.MinDistanceFromCavernTopAndBottom)
                {
                    int y = rand.Next((int)cavernTop[x] + this.Config.MinDistanceFromCavernTopAndBottom, (int)cavernBottom[x] - this.Config.MinDistanceFromCavernTopAndBottom);
                    int repeat = rand.Next(this.Config.MinRepeatForStroke, this.Config.MaxRepeatForStroke + 1);
                    this.GenerateBlock(sandbox, x, y, rand, repeat);
                    generatedBlockCount++;
                }
            }

            generatedValueDict = new Dictionary<string, object>()
            {
                ["CavernTop"] = cavernTop,
                ["CavernBottom"] = cavernBottom,
                ["GeneratedBlockCount"] = generatedBlockCount,
            };

            return true;
        }

        private void GenerateBlock(WorldSandbox sandbox, int x, int y, Random random, int repeat = 0)
        {
            if (repeat < 0)
            {
                return;
            }

            double blockDiameter = random.Next(this.Config.BlockStrokeMinDiameter, this.Config.BlockStrokeMaxDiameter + 1);
            int blockDirection = random.Next(2) == 0 ? 1 : -1;

            Vector2 currentPosition;
            currentPosition.X = x;
            currentPosition.Y = y;
            int k = random.Next(this.Config.MinRepeatPerStroke, this.Config.MaxRepeatPerStroke);
            Vector2 velocity;
            velocity.Y = random.Next(10, 20) * 0.01f;
            velocity.Y = random.Next(2) == 0 ? velocity.Y : -velocity.Y;
            velocity.X = blockDirection;
            while (k > 0)
            {
                k--;
                int minX = (int)(currentPosition.X - (blockDiameter * 0.5));
                int maxX = (int)(currentPosition.X + (blockDiameter * 0.5));
                int minY = (int)(currentPosition.Y - (blockDiameter * 0.5));
                int maxY = (int)(currentPosition.Y + (blockDiameter * 0.5));
                if (minX < 0)
                {
                    minX = 0;
                }

                if (maxX > sandbox.TileCountX)
                {
                    maxX = sandbox.TileCountX;
                }

                if (minY < 0)
                {
                    minY = 0;
                }

                if (maxY > sandbox.TileCountY)
                {
                    // maxY = sandbox.TileCountY;
                    return;
                }

                double realRadius = blockDiameter * random.Next(80, 120) * 0.004;
                for (int l = minX; l < maxX; l++)
                {
                    for (int m = minY; m < maxY; m++)
                    {
                        float a = Math.Abs(l - currentPosition.X);
                        float b = Math.Abs(m - currentPosition.Y);
                        if (Math.Sqrt((a * a) + (b * b)) < realRadius)
                        {
                            Coordinate coordinate = new Coordinate(l, m);
                            sandbox[coordinate].type = TileID.GrayBrick;
                            sandbox[coordinate].active(true);
                        }
                    }
                }

                currentPosition += velocity;
                velocity.X += random.Next(-10, 11) * 0.05f;
                velocity.Y += random.Next(-10, 11) * 0.05f;
                if (velocity.X > blockDirection + 0.5f)
                {
                    velocity.X = blockDirection + 0.5f;
                }

                if (velocity.X < blockDirection - 0.5f)
                {
                    velocity.X = blockDirection - 0.5f;
                }

                if (velocity.Y > 0.5f)
                {
                    velocity.Y = 0.5f;
                }

                if (velocity.Y < -0.5f)
                {
                    velocity.Y = -0.5f;
                }
            }

            this.GenerateBlock(sandbox, x, y, random, repeat - 1);
        }

        /// <summary>
        /// 洞窟の生成に使われるコンテキスト.
        /// </summary>
        public class CavernConfig : LayerConfig
        {
            /// <summary>
            /// 洞窟の最小の高さ.
            /// </summary>
            [Category("洞窟生成")]
            [DisplayName("最小高さ")]
            [Description("洞窟の最小の高さ")]
            public int CavernMinHeight { get; set; } = 5;

            /// <summary>
            /// 洞窟の最大の高さ.
            /// </summary>
            [Category("洞窟生成")]
            [DisplayName("最大高さ")]
            [Description("洞窟の最大の高さ")]
            public int CavernMaxHeight { get; set; } = 200;

            /// <summary>
            /// 地表からの最小距離.
            /// </summary>
            [Category("洞窟生成")]
            [DisplayName("最小地表距離")]
            [Description("地表から洞窟の天井までの最小距離")]
            public int CavernMinDistanceFromSurface { get; set; }

            /// <summary>
            /// 地表からの最大距離.
            /// </summary>
            [Category("洞窟生成")]
            [DisplayName("最大地表距離")]
            [Description("地表から洞窟の天井までの最大距離")]
            public int CavernMaxDistanceFromSurface { get; set; } = 100;

            /// <summary>
            /// 空中に設置されるブロック塊のブラシ最小サイズ.
            /// </summary>
            [Category("洞窟内空中ブロック")]
            [DisplayName("ブロック最小直径")]
            [Description("空中に設置されるブロック塊のブラシ最小サイズ")]
            public int BlockStrokeMinDiameter { get; set; } = 7;

            /// <summary>
            /// 空中に設置されるブロック塊のブラシ最大サイズ.
            /// </summary>
            [Category("洞窟内空中ブロック")]
            [DisplayName("ブロック最大直径")]
            [Description("空中に設置されるブロック塊のブラシ最大サイズ")]
            public int BlockStrokeMaxDiameter { get; set; } = 15;

            /// <summary>
            /// 空中に設置されるブロック塊のブラシの1ストロークあたりの最小移動距離.
            /// </summary>
            [Category("洞窟内空中ブロック")]
            [DisplayName("ブロック塊の最小移動距離")]
            [Description("空中に設置されるブロック塊のブラシの1ストロークあたりの最小移動距離")]
            public int MinRepeatPerStroke { get; set; } = 15;

            /// <summary>
            /// 空中に設置されるブロック塊のブラシの1ストロークあたりの最大移動距離.
            /// </summary>
            [Category("洞窟内空中ブロック")]
            [DisplayName("ブロック塊の最大移動距離")]
            [Description("空中に設置されるブロック塊のブラシの1ストロークあたりの最大移動距離")]
            public int MaxRepeatPerStroke { get; set; } = 25;

            /// <summary>
            /// 空中に設置されるブロック塊のブラシのストローク最小繰り返し回数.
            /// </summary>
            [Category("洞窟内空中ブロック")]
            [DisplayName("ブロック塊の移動最小繰り返し回数")]
            [Description("空中に設置されるブロック塊のブラシのストローク最小繰り返し回数")]
            public int MinRepeatForStroke { get; set; } = 3;

            /// <summary>
            /// 空中に設置されるブロック塊のブラシのストローク最大繰り返し回数.
            /// </summary>
            [Category("洞窟内空中ブロック")]
            [DisplayName("ブロック塊の移動最大繰り返し回数")]
            [Description("空中に設置されるブロック塊のブラシのストローク最大繰り返し回数")]
            public int MaxRepeatForStroke { get; set; } = 4;

            /// <summary>
            /// 洞窟の上下のタイルから、ブロック塊のブラシ開始地点までの最小距離.
            /// </summary>
            [Category("洞窟内空中ブロック")]
            [DisplayName("洞窟の上下からの最小距離")]
            [Description("洞窟の上下のタイルから、ブロック塊のブラシ開始地点までの最小距離")]
            public int MinDistanceFromCavernTopAndBottom { get; set; } = 15;

            /// <summary>
            /// 空中に設置されるブロック塊の設置個数.
            /// </summary>
            [Category("洞窟内空中ブロック")]
            [DisplayName("ブロック塊設置個数")]
            [Description("空中に設置されるブロック塊の設置個数")]
            public int BlockCount { get; set; } = 60;
        }
    }
}
