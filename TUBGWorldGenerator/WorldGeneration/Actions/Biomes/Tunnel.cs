using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TUBGWorldGenerator.WorldGeneration.Actions.Biomes
{
    public class Tunnel : IWorldGenerationAction<Tunnel.TunnelContext>
    {
        /// <inheritdoc/>
        public string Name => nameof(Tunnel);

        /// <inheritdoc/>
        public string Description => "第二層と地表を繋ぐトンネルを生成する";

        /// <inheritdoc/>
        public TunnelContext Context { get; set; } = new TunnelContext();

        /// <inheritdoc/>
        public bool Run(WorldSandbox sandbox)
        {
            GlobalContext globalContext = WorldGenerationRunner.CurrentRunner.GlobalContext;
            var random = globalContext.Random;

            var createdTunnels = new List<int>();
            for (int i = 0; i < Context.TunnelCount; i++)
            {
                int x = 0;
                bool check = false;
                for (int retry = 0; retry < 100; retry++)
                {
                    x = random.Next(100, sandbox.TileCountX - 100);
                    foreach (int tx in createdTunnels)
                    {
                        if (x + Context.MinDistanceFromNearbyTunnel > tx
                            && tx > x - Context.MinDistanceFromNearbyTunnel)
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
                    if (sandbox.Tiles[x, y] != null && sandbox.Tiles[x, y].active())
                    {
                        bool result = GenerateTunnel(sandbox, x, y, globalContext);
                        if (!result)
                        {
                            return false;
                        }

                        createdTunnels.Add(x);
                        break;
                    }
                }
            }

            return true;
        }

        private bool GenerateTunnel(WorldSandbox sandbox, int i, int j, GlobalContext context)
        {
            Random random = context.Random;
            double[] cavernTop;
            try
            {
                cavernTop = (double[])context["CavernTop"];
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

            Vector2 startPos;
            startPos.X = (float)i;
            startPos.Y = (float)j;
            int k = random.Next(20, 40);
            Vector2 endPos;
            endPos.Y = random.Next(10, 20) * 0.01f;
            endPos.X = (float)tunnelDirection;
            while (k > 0)
            {
                k--;
                int num3 = (int)((double)startPos.X - tunnelWidth * 0.5);
                int num4 = (int)((double)startPos.X + tunnelWidth * 0.5);
                int num5 = (int)((double)startPos.Y - tunnelWidth * 0.5);
                int num6 = (int)((double)startPos.Y + tunnelWidth * 0.5);
                if (num3 < 0)
                {
                    num3 = 0;
                }

                if (num4 > sandbox.TileCountX)
                {
                    num4 = sandbox.TileCountX;
                }

                if (num5 < 0)
                {
                    num5 = 0;
                }

                if (num6 > sandbox.TileCountY)
                {
                    num6 = sandbox.TileCountY;
                    return true;
                }

                double num7 = tunnelWidth * random.Next(80, 120) * 0.01;
                for (int l = num3; l < num4; l++)
                {
                    for (int m = num5; m < num6; m++)
                    {
                        float num8 = Math.Abs((float)l - startPos.X);
                        float num9 = Math.Abs((float)m - startPos.Y);
                        if (Math.Sqrt((double)(num8 * num8 + num9 * num9)) < num7 * 0.4)
                        {
                            sandbox.Tiles[l, m]?.active(false);
                        }
                    }
                }

                startPos += endPos;
                endPos.X += random.Next(-10, 11) * 0.05f;
                endPos.Y += random.Next(-10, 11) * 0.05f;
                if (endPos.X > (float)tunnelDirection + 0.5f)
                {
                    endPos.X = (float)tunnelDirection + 0.5f;
                }

                if (endPos.X < (float)tunnelDirection - 0.5f)
                {
                    endPos.X = (float)tunnelDirection - 0.5f;
                }

                if (endPos.Y > 2f)
                {
                    endPos.Y = 2f;
                }

                if (endPos.Y < 0f)
                {
                    endPos.Y = 0f;
                }
            }

            if (startPos.Y < cavernTop[(int)startPos.X])
            {
                return GenerateTunnel(sandbox, (int)startPos.X, (int)startPos.Y, context);
            }

            return true;
        }

        public class TunnelContext : ActionContext
        {
            /// <summary>
            /// トンネルの数
            /// </summary>
            public int TunnelCount { get; set; } = 10;

            /// <summary>
            /// 近くのトンネルとの最小距離
            /// </summary>
            public int MinDistanceFromNearbyTunnel { get; set; } = 100;
        }
    }
}
