// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TerraBuilder.WorldGeneration.Layers.Biomes
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using TerraBuilder.WorldEdit;
    using Terraria;

    /// <summary>
    /// トンネルを生成するクラス.
    /// </summary>
    [Action]
    public class Tunnel : IWorldGenerationLayer<Tunnel.TunnelConfig>
    {
        /// <inheritdoc/>
        public string Name => nameof(Tunnel);

        /// <inheritdoc/>
        public string Description => "第二層と地表を繋ぐトンネルを生成する";

        /// <inheritdoc/>
        public TunnelConfig Config { get; set; } = new TunnelConfig();

        /// <inheritdoc/>
        public bool Apply(WorldGenerationRunner runner, WorldSandbox sandbox, out Dictionary<string, object> generatedValueDict)
        {
            Random random = runner.Random;

            List<int> createdTunnels = new List<int>();
            for (int i = 0; i < this.Config.TunnelCount; i++)
            {
                int x = 0;
                bool check = false;
                for (int retry = 0; retry < 100; retry++)
                {
                    x = random.Next(100, sandbox.TileCountX - 100);
                    foreach (int tx in createdTunnels)
                    {
                        if (x + this.Config.MinDistanceFromNearbyTunnel > tx
                            && tx > x - this.Config.MinDistanceFromNearbyTunnel)
                        {
                            check = true;
                            break;
                        }
                    }

                    if (!check)
                    {
                        break;
                    }
                }

                if (check)
                {
                    continue;
                }

                for (int y = 0; y < sandbox.TileCountY; y++)
                {
                    Coordinate coordinate = new Coordinate(x, y);
                    if (sandbox[coordinate]?.active() == true)
                    {
                        bool result = this.GenerateTunnel(runner, sandbox, coordinate);
                        if (!result)
                        {
                            // Cavernが生成されていないなら、return
                            generatedValueDict = new Dictionary<string, object>() { ["CreatedTunnels"] = createdTunnels };
                            return false;
                        }

                        createdTunnels.Add(x);
                        break;
                    }
                }
            }

            generatedValueDict = new Dictionary<string, object>() { ["CreatedTunnels"] = createdTunnels };
            return true;
        }

        private bool GenerateTunnel(WorldGenerationRunner runner, WorldSandbox sandbox, Coordinate coordinate)
        {
            Random random = runner.Random;
            double[] cavernTop;
            try
            {
                cavernTop = runner.GetGeneratedValue<Caverns, double[]>("CavernTop");
            }
            catch (KeyNotFoundException)
            {
                return false;
            }

            double tunnelWidth = random.Next(7, 15);
            int tunnelDirection = 1;
            if (random.Next(2) == 0)
            {
                tunnelDirection = -1;
            }

            Vector2 currentPosition;
            currentPosition.X = coordinate.X;
            currentPosition.Y = coordinate.Y;
            int k = random.Next(20, 40);
            Vector2 nextVector;
            nextVector.Y = random.Next(10, 20) * 0.01f;
            nextVector.X = tunnelDirection;
            while (k > 0)
            {
                k--;
                int minX = (int)(currentPosition.X - (tunnelWidth * 0.5));
                int maxX = (int)(currentPosition.X + (tunnelWidth * 0.5));
                int minY = (int)(currentPosition.Y - (tunnelWidth * 0.5));
                int maxY = (int)(currentPosition.Y + (tunnelWidth * 0.5));
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
                    return true;
                }

                double tunnelCircle = tunnelWidth * random.Next(80, 120) * 0.004;
                for (int l = minX; l < maxX; l++)
                {
                    for (int m = minY; m < maxY; m++)
                    {
                        float a = Math.Abs(l - currentPosition.X);
                        float b = Math.Abs(m - currentPosition.Y);
                        if (Math.Sqrt((a * a) + (b * b)) < tunnelCircle)
                        {
                            Coordinate tunnelCoordinate = new Coordinate(l, m);
                            Tile tile = sandbox[tunnelCoordinate];
                            if (tile != null)
                            {
                                Tile cloned = (Tile)tile.Clone();
                                cloned.type = 0;
                                cloned.active(false);
                                _ = sandbox.PlaceTile(tunnelCoordinate, cloned);
                            }
                        }
                    }
                }

                currentPosition += nextVector;
                nextVector.X += random.Next(-10, 11) * 0.05f;
                nextVector.Y += random.Next(-10, 11) * 0.05f;
                if (nextVector.X > tunnelDirection + 0.5f)
                {
                    nextVector.X = tunnelDirection + 0.5f;
                }

                if (nextVector.X < tunnelDirection - 0.5f)
                {
                    nextVector.X = tunnelDirection - 0.5f;
                }

                if (nextVector.Y > 2f)
                {
                    nextVector.Y = 2f;
                }

                if (nextVector.Y < 0f)
                {
                    nextVector.Y = 0f;
                }
            }

            if (currentPosition.Y < cavernTop[(int)currentPosition.X])
            {
                return this.GenerateTunnel(runner, sandbox, new Coordinate((int)currentPosition.X, (int)currentPosition.Y));
            }

            return true;
        }

        /// <summary>
        /// トンネル生成のコンフィグ.
        /// </summary>
        public class TunnelConfig : LayerConfig
        {
            /// <summary>
            /// トンネルの数.
            /// </summary>
            [Category("トンネル生成")]
            [DisplayName("トンネル数")]
            [Description("生成するトンネルの個数")]
            public int TunnelCount { get; set; } = 10;

            /// <summary>
            /// 近くのトンネルとの最小距離.
            /// </summary>
            [Category("トンネル生成")]
            [DisplayName("近くのトンネルからの最小距離")]
            [Description("近くのトンネルからどれだけ離れているかを設定する.この距離以内には追加のトンネルの入口は生成されない.")]
            public int MinDistanceFromNearbyTunnel { get; set; } = 100;
        }
    }
}
