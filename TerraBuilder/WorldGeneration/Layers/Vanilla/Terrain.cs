// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TerraBuilder.WorldGeneration.Layers.Vanilla
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TerraBuilder.WorldEdit;
    using Terraria;
    using Terraria.ID;

    /// <summary>
    /// 大地を生成する.
    /// バニラのTerrain/TerrainPassに相当.
    /// </summary>
    public class Terrain : IWorldGenerationLayer<Terrain.TerrainConfig>
    {
        /// <summary>
        /// 地面の形状.起伏を決定する.
        /// </summary>
        private enum TerrainFeatureType
        {
            /// <summary>
            /// 平坦（若干の起伏）.
            /// </summary>
            Flat = 0,

            /// <summary>
            /// 上り坂（ゆるやか）.
            /// </summary>
            Hill = 1,

            /// <summary>
            /// 下り坂（ゆるやか）.
            /// </summary>
            DownHill = 2,

            /// <summary>
            /// 山（急な上り坂）.
            /// </summary>
            Mountain = 3,

            /// <summary>
            /// 谷（急な下り坂）.
            /// </summary>
            Valley = 4,
        }

        /// <inheritdoc/>
        public string Name => nameof(Terrain);

        /// <inheritdoc/>
        public string Description => "大地を生成する.";

        /// <inheritdoc/>
        public TerrainConfig Config { get; } = new TerrainConfig();

        /// <inheritdoc/>
        public bool Apply(WorldGenerationRunner runner, WorldSandbox sandbox, out Dictionary<string, object> generatedValueDict)
        {
            // TODO: この値を生成する場所を検討
            int leftBeachSize = runner.GetGeneratedValue<Setup, int>("LeftBeachEnd");
            int rightBeachSize = sandbox.TileCountX - runner.GetGeneratedValue<Setup, int>("RightBeachStart");

            int flatBeachPadding = this.Config.FlatBeachPadding;
            TerrainFeatureType currentTerrainFeature = TerrainFeatureType.Flat;

            double currentWorldSurface = sandbox.TileCountY * 0.3;
            currentWorldSurface *= runner.Random.Next(90, 110) * 0.005;
            double rockLayer = currentWorldSurface + (sandbox.TileCountY * 0.2);
            rockLayer *= runner.Random.Next(90, 110) * 0.01;
            double worldSurfaceLow = currentWorldSurface;
            double worldSurfaceHigh = currentWorldSurface;
            double rockLayerLow = rockLayer;
            double rockLayerHigh = rockLayer;
            double surfaceMaxHeight = sandbox.TileCountY * 0.23;
            SurfaceHistory history = new SurfaceHistory(500);

            // 最初はビーチぶんスキップしてから開始
            int currentTerrainCounter = leftBeachSize + flatBeachPadding;

            const float minSurfaceHeightPercentage = 0.17f;
            const float maxSurfaceHeightPercentage = 0.26f;
            for (int i = 0; i < sandbox.TileCountX; i++)
            {
                worldSurfaceLow = Math.Min(currentWorldSurface, worldSurfaceLow);
                worldSurfaceHigh = Math.Max(currentWorldSurface, worldSurfaceHigh);
                rockLayerLow = Math.Min(rockLayer, rockLayerLow);
                rockLayerHigh = Math.Max(rockLayer, rockLayerHigh);

                if (currentTerrainCounter <= 0)
                {
                    currentTerrainFeature = (TerrainFeatureType)runner.Random.Next(0, 5);
                    currentTerrainCounter = runner.Random.Next(5, 40);
                    if (currentTerrainFeature == TerrainFeatureType.Flat)
                    {
                        currentTerrainCounter *= (int)(runner.Random.Next(5, 30) * 0.2);
                    }
                }

                currentTerrainCounter--;

                // 中央付近は大きな山、谷は作らない
                if (i > sandbox.TileCountX * 0.45
                    && i < sandbox.TileCountX * 0.55
                    && (currentTerrainFeature == TerrainFeatureType.Mountain || currentTerrainFeature == TerrainFeatureType.Valley))
                {
                    currentTerrainFeature = (TerrainFeatureType)runner.Random.Next(3);
                }

                // 中央付近は平坦に
                if (i > sandbox.TileCountX * 0.48 && i < sandbox.TileCountX * 0.52)
                {
                    currentTerrainFeature = TerrainFeatureType.Flat;
                }

                currentWorldSurface += this.GenerateWorldSurfaceOffset(runner.Random, currentTerrainFeature);
                if (i < leftBeachSize + flatBeachPadding || i > sandbox.TileCountX - rightBeachSize - flatBeachPadding)
                {
                    currentWorldSurface = currentWorldSurface < sandbox.TileCountY * 0.17 ? sandbox.TileCountY * 0.17 : currentWorldSurface;
                    currentWorldSurface = currentWorldSurface > surfaceMaxHeight ? surfaceMaxHeight : currentWorldSurface;
                }
                else if (currentWorldSurface < sandbox.TileCountY * minSurfaceHeightPercentage)
                {
                    currentWorldSurface = sandbox.TileCountY * minSurfaceHeightPercentage;
                    currentTerrainCounter = 0;
                }
                else if (currentWorldSurface > sandbox.TileCountY * maxSurfaceHeightPercentage)
                {
                    currentWorldSurface = sandbox.TileCountY * maxSurfaceHeightPercentage;
                    currentTerrainCounter = 0;
                }

                while (runner.Random.Next(0, 3) == 0)
                {
                    rockLayer += runner.Random.Next(-2, 3);
                }

                if (rockLayer < currentWorldSurface + (sandbox.TileCountY * 0.06))
                {
                    rockLayer++;
                }

                if (rockLayer > currentWorldSurface + (sandbox.TileCountY * 0.35))
                {
                    rockLayer--;
                }

                history.Record(currentWorldSurface);
                this.FillColumn(sandbox, i, currentWorldSurface, rockLayer);

                // 右端まで到達
                if (i == sandbox.TileCountX - rightBeachSize - flatBeachPadding)
                {
                    // 地表が深く（低く）なりすぎている場合
                    if (currentWorldSurface > surfaceMaxHeight)
                    {
                        this.RetargetSurfaceHistory(sandbox, history, i, surfaceMaxHeight);
                    }

                    // 残り（ビーチ部分）は平坦に生成
                    currentTerrainFeature = TerrainFeatureType.Flat;
                    currentTerrainCounter = sandbox.TileCountX - i;
                }
            }

            int worldSurface = (int)(worldSurfaceHigh + 25.0);
            int waterLinePadding = (int)((rockLayerHigh - currentWorldSurface) / 6.0) * 6;
            int waterLine = (int)currentWorldSurface + waterLinePadding;
            waterLine = (waterLine + sandbox.TileCountY) / 2;
            waterLine += runner.Random.Next(-100, 20);
            int lavaLine = waterLine + runner.Random.Next(50, 80);
            const int rockLayerMinimumPadding = 20;

            // RockLayerの上限と地表の下限をRockLayyerMinimumPaddingぶん確保する.
            if (rockLayerLow < worldSurfaceHigh + rockLayerMinimumPadding)
            {
                double rockLayerCenter = (rockLayerLow + worldSurfaceHigh) / 2.0;
                double rockLayerDiff = Math.Abs(rockLayerLow - worldSurfaceHigh);
                if (rockLayerDiff < rockLayerMinimumPadding)
                {
                    rockLayerDiff = rockLayerMinimumPadding;
                }

                rockLayerLow = rockLayerCenter + (rockLayerDiff / 2.0);
                worldSurfaceHigh = rockLayerCenter - (rockLayerDiff / 2.0);
            }

            generatedValueDict = new Dictionary<string, object>()
            {
                ["RockLayer"] = rockLayer,
                ["RockLayerHigh"] = rockLayerHigh,
                ["RockLayerLow"] = rockLayerLow,
                ["CurrentWorldSurface"] = currentWorldSurface,
                ["WorldSurfaceHigh"] = worldSurfaceHigh,
                ["WorldSurfaceLow"] = worldSurfaceLow,
                ["WaterLine"] = waterLine,
                ["LavaLine"] = lavaLine,
            };
            return true;
        }

        private double GenerateWorldSurfaceOffset(Random random, TerrainFeatureType featureType)
        {
            double offset = 0.0;
            if (random.Next(2) == 0)
            {
                switch (featureType)
                {
                    case TerrainFeatureType.Flat:
                        while (random.Next(0, 6) == 0)
                        {
                            offset += random.Next(-1, 2);
                        }

                        break;
                    case TerrainFeatureType.Hill:
                        // 1/3の確率でループ
                        while (random.Next(0, 3) == 0)
                        {
                            offset--;
                        }

                        // 1/10の確率でループ
                        while (random.Next(0, 10) == 0)
                        {
                            offset++;
                        }

                        break;
                    case TerrainFeatureType.DownHill:
                        // 1/3の確率でループ
                        while (random.Next(0, 3) == 0)
                        {
                            offset++;
                        }

                        // 1/10の確率でループ
                        while (random.Next(0, 10) == 0)
                        {
                            offset--;
                        }

                        break;
                    case TerrainFeatureType.Mountain:
                        // 2/3の確率でループ
                        while (random.Next(0, 3) != 0)
                        {
                            offset--;
                        }

                        // 1/6の確率でループ
                        while (random.Next(0, 6) == 0)
                        {
                            offset++;
                        }

                        break;
                    case TerrainFeatureType.Valley:
                        // 2/3の確率でループ
                        while (random.Next(0, 3) != 0)
                        {
                            offset++;
                        }

                        // 1/5の確率でループ
                        while (random.Next(0, 5) == 0)
                        {
                            offset--;
                        }

                        break;
                }
            }
            else
            {
                switch (featureType)
                {
                    case TerrainFeatureType.Flat:
                        while (random.Next(0, 7) == 0)
                        {
                            offset += random.Next(-1, 2);
                        }

                        break;
                    case TerrainFeatureType.Hill:
                        while (random.Next(0, 4) == 0)
                        {
                            offset--;
                        }

                        while (random.Next(0, 10) == 0)
                        {
                            offset++;
                        }

                        break;
                    case TerrainFeatureType.DownHill:
                        while (random.Next(0, 4) == 0)
                        {
                            offset++;
                        }

                        while (random.Next(0, 10) == 0)
                        {
                            offset--;
                        }

                        break;
                    case TerrainFeatureType.Mountain:
                        while (random.Next(0, 2) == 0)
                        {
                            offset--;
                        }

                        while (random.Next(0, 6) == 0)
                        {
                            offset++;
                        }

                        break;
                    case TerrainFeatureType.Valley:
                        while (random.Next(0, 2) == 0)
                        {
                            offset++;
                        }

                        while (random.Next(0, 5) == 0)
                        {
                            offset--;
                        }

                        break;
                }
            }

            return offset;
        }

        private void RetargetSurfaceHistory(WorldSandbox sandbox, SurfaceHistory history, int targetX, double targetHeight)
        {
            for (int i = 0; i < history.Length / 2; i++)
            {
                if (history[history.Length - 1] <= targetHeight)
                {
                    break;
                }

                // 1つ飛ばしでチェックする
                for (int dimensions = 0; dimensions < history.Length - (i * 2); dimensions++)
                {
                    double height = history[history.Length - dimensions - 1];
                    height--;
                    history[history.Length - dimensions - 1] = height;
                    if (height <= targetHeight)
                    {
                        break;
                    }
                }
            }

            for (int i = 0; i < history.Length; i++)
            {
                double height = history[history.Length - i - 1];
                this.RetargetColumn(sandbox, targetX - i, height);
            }
        }

        private void RetargetColumn(WorldSandbox sandbox, int x, double worldSurface)
        {
            for (int y = 0; y < worldSurface; y++)
            {
                Coordinate coordinate = new Coordinate(x, y);
                Tile tile = (Tile)sandbox[coordinate].Clone();
                tile.active(false);
                tile.frameX = -1;
                tile.frameY = -1;
                _ = sandbox.PlaceTile(coordinate, tile);
            }

            for (int y = (int)worldSurface; y < sandbox.TileCountY; y++)
            {
                Coordinate coordinate = new Coordinate(x, y);
                if (sandbox[coordinate].type != 1 || !sandbox[coordinate].active())
                {
                    Tile tile = (Tile)sandbox[coordinate].Clone();
                    tile.active(true);
                    tile.type = TileID.Dirt;
                    tile.frameX = -1;
                    tile.frameY = -1;
                    _ = sandbox.PlaceTile(coordinate, tile);
                }
            }
        }

        private void FillColumn(WorldSandbox sandbox, int x, double worldSurface, double rockLayer)
        {
            for (int y = 0; y < worldSurface; y++)
            {
                Coordinate coordinate = new Coordinate(x, y);
                Tile tile = (Tile)sandbox[coordinate].Clone();
                tile.active(false);
                tile.frameX = -1;
                tile.frameY = -1;
                _ = sandbox.PlaceTile(coordinate, tile);
            }

            for (int y = (int)worldSurface; y < Main.maxTilesY; y++)
            {
                Coordinate coordinate = new Coordinate(x, y);
                if (coordinate.Y < rockLayer)
                {
                    Tile tile = (Tile)sandbox[coordinate].Clone();
                    tile.active(true);
                    tile.type = TileID.Dirt;
                    tile.frameX = -1;
                    tile.frameY = -1;
                    _ = sandbox.PlaceTile(coordinate, tile);
                }
                else
                {
                    Tile tile = (Tile)sandbox[coordinate].Clone();
                    tile.active(true);
                    tile.type = TileID.Stone;
                    tile.frameX = -1;
                    tile.frameY = -1;
                    _ = sandbox.PlaceTile(coordinate, tile);
                }
            }
        }

        /// <summary>
        /// 大地生成のコンフィグ.
        /// </summary>
        public class TerrainConfig : LayerConfig
        {
            // TODO? LeftBeachSize, RightBeachSizeはここでコンフィグにする？

            // TODO 説明文をわかりやすく
            /// <summary>
            /// ビーチからワールド中心部に向かうとき、この値ぶんだけ平らな地面を生成してから通常の地面生成を行う.
            /// </summary>
            public int FlatBeachPadding { get; set; } = 5;
        }

        // TODO 書き直し
        private class SurfaceHistory
        {
            private readonly double[] heights;

            private int index;

            public SurfaceHistory(int size)
            {
                this.heights = new double[size];
            }

            public int Length => this.heights.Length;

            public double this[int index]
            {
                get => this.heights[(index + this.index) % this.heights.Length];

                set => this.heights[(index + this.index) % this.heights.Length] = value;
            }

            public void Record(double height)
            {
                this.heights[this.index] = height;
                this.index = (this.index + 1) % this.heights.Length;
            }
        }
    }
}
