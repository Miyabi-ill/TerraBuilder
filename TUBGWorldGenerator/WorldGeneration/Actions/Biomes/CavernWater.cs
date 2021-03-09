namespace TUBGWorldGenerator.WorldGeneration.Actions.Biomes
{
    using System.Linq;
    using TUBGWorldGenerator.Utils;

    public class CavernWater : IWorldGenerationAction<CavernWater.CavernWaterContext>
    {
        public string Name => nameof(CavernWater);

        public string Description => "地下に水を生成する";

        public CavernWaterContext Context { get; set; } = new CavernWaterContext();

        public bool Run(WorldSandbox sandbox)
        {
            GlobalContext globalContext = WorldGenerationRunner.CurrentRunner.GlobalContext;
            int cavernTop = (int)((double[])WorldGenerationRunner.CurrentRunner.GlobalContext["CavernTop"]).Min();
            int cavernBottom = (int)((double[])WorldGenerationRunner.CurrentRunner.GlobalContext["CavernBottom"]).Max();

            for (int i = 0; i < Context.RandomWaterBlockCount; i++)
            {
                int sizeX = globalContext.Random.Next(Context.RandomWaterBlockMinX, Context.RandomWaterBlockMaxX + 1);
                int sizeY = globalContext.Random.Next(Context.RandomWaterBlockMinY, Context.RandomWaterBlockMaxY + 1);
                if (cavernTop >= cavernBottom - sizeY)
                {
                    sizeY = cavernBottom - cavernTop - 1;
                }

                int x = globalContext.Random.Next(100, sandbox.TileCountX - 100 - sizeX);
                int y = globalContext.Random.Next(cavernTop, cavernBottom - sizeY);
                for (int cx = x; cx < x + sizeX; cx++)
                {
                    for (int cy = y; cy < y + sizeY; cy++)
                    {
                        sandbox.Tiles[cx, cy].liquid = 255;
                    }
                }
            }

            Liquids.Settle(sandbox);

            return true;
        }

        public class CavernWaterContext : ActionContext
        {
            public int RandomWaterBlockCount { get; set; } = 15;

            public int RandomWaterBlockMinX { get; set; } = 10;

            public int RandomWaterBlockMaxX { get; set; } = 50;

            public int RandomWaterBlockMinY { get; set; } = 10;

            public int RandomWaterBlockMaxY { get; set; } = 50;
        }
    }
}
