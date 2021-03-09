namespace TUBGWorldGenerator.WorldGeneration.Actions.Buildings
{
    using System.Collections.Generic;
    using Terraria;
    using Terraria.DataStructures;
    using Terraria.ID;
    using TUBGWorldGenerator.Utils;
    using TUBGWorldGenerator.WorldGeneration;
    using static TUBGWorldGenerator.WorldGeneration.Actions.Buildings.RandomSizeBlocks;

    /// <summary>
    /// ランダムなサイズのブロックをワールド地表に設置するクラス
    /// </summary>
    public class RandomSizeBlockWithArea : IWorldGenerationAction<RandomSizeBlockWithArea.RandomSizeBlockWithAreaContext>
    {
        private enum ActiveWallDirection : int
        {
            Top = 0,
            Left,
            Bottom,
            Right,
        }

        /// <inheritdoc/>
        public string Name => nameof(RandomSizeBlockWithArea);

        /// <inheritdoc/>
        public string Description => "ランダムな大きさ、形のブロックをワールド地表に配置する";

        /// <inheritdoc/>
        public RandomSizeBlockWithAreaContext Context { get; } = new RandomSizeBlockWithAreaContext();

        /// <inheritdoc/>
        public bool Run(WorldSandbox sandbox)
        {
            GlobalContext globalContext = WorldGenerationRunner.CurrentRunner.GlobalContext;
            var random = globalContext.Random;
            var area = new List<Point16>();
            for (int i = 0; i < Context.AreaCount; i++)
            {
                int width = random.Next(Context.AreaMinWidth, Context.AreaMaxWidth);
                int areaLeft = 0;
                bool success = false;
                for (int r = 0; r < Context.MaxAreaSelectRetry; r++)
                {
                    areaLeft = random.Next(100, sandbox.TileCountX - 100 - width);
                    success = true;
                    foreach (Point16 areaPos in area)
                    {
                        if (areaPos.Y < areaLeft - Context.MinDistanceFromNearbyArea
                            || areaLeft + width + Context.MinDistanceFromNearbyArea < areaPos.X)
                        {
                            // 重なっていない
                        }
                        else
                        {
                            // 重なっている
                            success = false;
                        }
                    }

                    if (success)
                    {
                        break;
                    }
                }

                if (success)
                {
                    area.Add(new Point16(areaLeft, areaLeft + width));
                    if (Context.PlaceBiome == PlaceBiome.Surface)
                    {
                        for (int c = 0; c < Context.BlockCount; c++)
                        {
                            for (int retry = 0; retry < Context.MaxPlaceRetry; retry++)
                            {
                                int x = random.Next(areaLeft, areaLeft + width);
                                bool result = PlaceBlockToSurface(
                                    sandbox,
                                    Context,
                                    sizeX: random.Next(Context.BlockMinX, Context.BlockMaxX),
                                    sizeY: random.Next(Context.BlockMinY, Context.BlockMaxY),
                                    x);
                                if (result)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else if (Context.PlaceBiome == PlaceBiome.Cavern)
                    {
                        double[] cavernTop = (double[])WorldGenerationRunner.CurrentRunner.GlobalContext["CavernTop"];
                        double[] cavernBottom = (double[])WorldGenerationRunner.CurrentRunner.GlobalContext["CavernBottom"];
                        for (int c = 0; c < Context.BlockCount; c++)
                        {
                            for (int retry = 0; retry < Context.MaxPlaceRetry; retry++)
                            {
                                int x = random.Next(areaLeft, areaLeft + width);
                                int y = random.Next((int)cavernTop[x], (int)cavernBottom[x]);
                                bool result = PlaceBlockToCavern(
                                    sandbox,
                                    Context,
                                    sizeX: random.Next(Context.BlockMinX, Context.BlockMaxX),
                                    sizeY: random.Next(Context.BlockMinY, Context.BlockMaxY),
                                    x,
                                    y,
                                    random.Next(2) == 0);
                                if (result)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        public class RandomSizeBlockWithAreaContext : RandomSizeBlocks.RandomSizeBlockContext
        {
            /// <summary>
            /// コンストラクタ。
            /// </summary>
            public RandomSizeBlockWithAreaContext()
            {
                BlockCount = 100;
                ChestProbably = 0.03;
            }

            public int AreaCount { get; set; } = 5;

            public int MinDistanceFromNearbyArea { get; set; } = 200;

            public int AreaMinWidth { get; set; } = 30;

            public int AreaMaxWidth { get; set; } = 150;

            public int MaxAreaSelectRetry { get; set; } = 10;
        }
    }
}
