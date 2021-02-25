namespace TUBGWorldGenerator.WorldGeneration.Actions.Biomes
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// トンネルを生成するクラス。
    /// </summary>
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

            Vector2 currentPosition;
            currentPosition.X = (float)i;
            currentPosition.Y = (float)j;
            int k = random.Next(20, 40);
            Vector2 nextVector;
            nextVector.Y = random.Next(10, 20) * 0.01f;
            nextVector.X = (float)tunnelDirection;
            while (k > 0)
            {
                k--;
                int minX = (int)((double)currentPosition.X - (tunnelWidth * 0.5));
                int maxX = (int)((double)currentPosition.X + (tunnelWidth * 0.5));
                int minY = (int)((double)currentPosition.Y - (tunnelWidth * 0.5));
                int maxY = (int)((double)currentPosition.Y + (tunnelWidth * 0.5));
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
                    maxY = sandbox.TileCountY;
                    return true;
                }

                double tunnelCircle = tunnelWidth * random.Next(80, 120) * 0.004;
                for (int l = minX; l < maxX; l++)
                {
                    for (int m = minY; m < maxY; m++)
                    {
                        float a = Math.Abs((float)l - currentPosition.X);
                        float b = Math.Abs((float)m - currentPosition.Y);
                        if (Math.Sqrt((double)(a * a + b * b)) < tunnelCircle)
                        {
                            sandbox.Tiles[l, m]?.active(false);
                        }
                    }
                }

                currentPosition += nextVector;
                nextVector.X += random.Next(-10, 11) * 0.05f;
                nextVector.Y += random.Next(-10, 11) * 0.05f;
                if (nextVector.X > (float)tunnelDirection + 0.5f)
                {
                    nextVector.X = (float)tunnelDirection + 0.5f;
                }

                if (nextVector.X < (float)tunnelDirection - 0.5f)
                {
                    nextVector.X = (float)tunnelDirection - 0.5f;
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
                return GenerateTunnel(sandbox, (int)currentPosition.X, (int)currentPosition.Y, context);
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
