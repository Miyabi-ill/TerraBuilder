namespace TUBGWorldGenerator.WorldGeneration.Actions.Biomes
{
    using System;
    using Terraria;
    using Terraria.ID;
    using TUBGWorldGenerator.Utils;

    /// <summary>
    /// 洞窟(第二層)の生成を行う。
    /// </summary>
    public class Caverns : IWorldGenerationAction<Caverns.CavernContext>
    {
        /// <inheritdoc/>
        public string Name => nameof(Caverns);

        /// <inheritdoc/>
        public string Description => "Generate Caverns.";

        /// <inheritdoc/>
        public CavernContext Context { get; private set; }

        /// <inheritdoc/>
        public bool Run(WorldSandbox sandbox)
        {
            if (Context == null)
            {
                Context = new CavernContext();
            }

            GlobalContext globalContext = WorldGenerationRunner.CurrentRunner.GlobalContext;

            int tileLengthX = sandbox.TileCountX;

            // Surface: topPerlinの振幅
            int diffSurface = Context.CavernMaxDistanceFromSurface - Context.CavernMinDistanceFromSurface;

            var rand = globalContext.Random;
            var topPerlin = PerlinNoise.GenerateOctave1D(128, tileLengthX, 1, 8, 2, rand);
            var bottomPerlin = PerlinNoise.GenerateOctave1D(128, tileLengthX, 1, 8, 2, rand);

            int minIndex = 0;
            double minValue = double.MaxValue;
            double maxValue = double.MinValue;
            for (int i = 0; i < tileLengthX; i++)
            {
                double diff = Math.Abs(topPerlin[i] - bottomPerlin[i]);
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

            // 空間のminとmaxから波の増幅量を決定、適用。タイルで使うために四捨五入しておく。
            double bottomAmplifier = (maxValue - minValue) * (Context.CavernMaxHeight - Context.CavernMinHeight);
            for (int i = 0; i < tileLengthX; i++)
            {
                topPerlin[i] = Math.Round(topPerlin[i] * diffSurface);
                bottomPerlin[i] = Math.Round(bottomPerlin[i] * bottomAmplifier);
            }

            int bottomPerlinBaseTopLine = (int)(topPerlin[minIndex] + (minValue * bottomAmplifier));
            var tiles = sandbox.Tiles;

            int cavernStart = globalContext.SurfaceLevel + Context.CavernMinDistanceFromSurface;

            // TODO:実際にタイルを置く
            for (int x = 0; x < sandbox.TileCountX; x++)
            {
                for (int y = globalContext.SurfaceLevel; y < sandbox.TileCountY; y++)
                {
                    if (y < cavernStart + topPerlin[x])
                    {
                        tiles[x, y] = new Tile()
                        {
                            type = TileID.Stone,
                        };
                        tiles[x, y].active(true);
                    }

                    if (y > cavernStart + bottomPerlinBaseTopLine + bottomPerlin[x])
                    {
                        tiles[x, y] = new Tile()
                        {
                            type = TileID.Stone,
                        };
                        tiles[x, y].active(true);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 洞窟の生成に使われるコンテキスト。
        /// </summary>
        public class CavernContext : ActionContext
        {
            /// <summary>
            /// 洞窟の最小の高さ。
            /// </summary>
            public int CavernMinHeight { get; } = 5;

            /// <summary>
            /// 洞窟の最大の高さ。
            /// </summary>
            public int CavernMaxHeight { get; } = 100;

            /// <summary>
            /// 地表からの最小距離。
            /// </summary>
            public int CavernMinDistanceFromSurface { get; } = 0;

            /// <summary>
            /// 地表からの最大距離。
            /// </summary>
            public int CavernMaxDistanceFromSurface { get; } = 100;
        }
    }
}
