namespace TUBGWorldGenerator.WorldGeneration.Actions.Biomes
{
    using System;
    using System.Linq;
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

            int tileLengthX = sandbox.TileCountX;

            // Surface: topPerlinの振幅
            int diffSurface = Context.CavernMaxDistanceFromSurface - Context.CavernMinDistanceFromSurface;

            var topPerlin = PerlinNoise.GenerateOctave1D(128, tileLengthX, 1, 8, 2);
            var bottomPerlin = PerlinNoise.GenerateOctave1D(128, tileLengthX, 1, 8, 2);

            int minIndex = 0;
            double minValue = double.MaxValue;
            double maxValue = double.MinValue;
            for (int i = 0; i < tileLengthX; i++)
            {
                if (bottomPerlin[i] > maxValue)
                {
                    maxValue = bottomPerlin[i];
                }

                if (bottomPerlin[i] < minValue)
                {
                    minValue = bottomPerlin[i];
                    minIndex = i;
                }
            }

            // 空間のminとmaxから波の増幅量を決定、適用
            double bottomAmplifier = maxValue - minValue;
            for (int i = 0; i < tileLengthX; i++)
            {
                topPerlin[i] *= diffSurface;
                bottomPerlin[i] *= bottomAmplifier;
            }

            int bottomPerlinBaseTopLine = (int)(topPerlin[minIndex] + minValue);

            // TODO:実際にタイルを置く

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
            public int CavernMinHeight { get; }

            /// <summary>
            /// 洞窟の最大の高さ。
            /// </summary>
            public int CavernMaxHeight { get; }

            /// <summary>
            /// 地表からの最小距離。
            /// </summary>
            public int CavernMinDistanceFromSurface { get; }

            /// <summary>
            /// 地表からの最大距離。
            /// </summary>
            public int CavernMaxDistanceFromSurface { get; }
        }
    }
}
