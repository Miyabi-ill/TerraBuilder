namespace TUBGWorldGenerator.WorldGeneration.Actions.Biomes
{
    using System;
    using System.Linq;
    using TUBGWorldGenerator.Utils;


    public class Caverns : IWorldGenerationAction<Caverns.CavernContext>
    {
        public string Name => nameof(Caverns);

        public string Description => "Generate Caverns.";

        public CavernContext Context { get; private set; }

        public class CavernContext : ActionContext
        {
            public int CavernMinHeight { get; }

            public int CavernMaxHeight { get; }

            public int CavernMinDistanceFromSurface { get; }

            public int CavernMaxDistanceFromSurface { get; }

            public int CavernMaxDiffToNextTile { get; }
        }

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

            

            return true;
        }
    }
}
