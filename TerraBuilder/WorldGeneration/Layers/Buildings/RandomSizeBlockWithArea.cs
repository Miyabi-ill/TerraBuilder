namespace TerraBuilder.WorldGeneration.Layers.Buildings
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using TerraBuilder.WorldGeneration;
    using Terraria;
    using Terraria.DataStructures;
    using static TerraBuilder.WorldGeneration.Layers.Buildings.RandomSizeBlocks;

    /// <summary>
    /// ランダムなサイズのブロックをある程度まとまった量ワールド地表に設置するクラス
    /// </summary>
    [Action]
    public class RandomSizeBlockWithArea : IWorldGenerationLayer<RandomSizeBlockWithArea.RandomSizeBlockWithAreaContext>
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
        public string Description => "ランダムな大きさ、形のブロックをワールド地表の一定のエリア内に配置する";

        /// <inheritdoc/>
        public RandomSizeBlockWithAreaContext Config { get; } = new RandomSizeBlockWithAreaContext();

        /// <inheritdoc/>
        public bool Apply(WorldGenerationRunner runner, WorldSandbox sandbox, out Dictionary<string, object> generatedValueDict)
        {
            GlobalConfig globalContext = runner.GlobalConfig;
            var random = globalContext.Random;
            var area = new List<Point16>();
            for (int i = 0; i < Config.AreaCount; i++)
            {
                int width = random.Next(Config.AreaMinWidth, Config.AreaMaxWidth);
                int areaLeft = 0;
                bool success = false;
                for (int r = 0; r < Config.MaxAreaSelectRetry; r++)
                {
                    areaLeft = random.Next(100, sandbox.TileCountX - 100 - width);
                    success = true;
                    foreach (Point16 areaPos in area)
                    {
                        if (areaPos.Y < areaLeft - Config.MinDistanceFromNearbyArea
                            || areaLeft + width + Config.MinDistanceFromNearbyArea < areaPos.X)
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
                    if (Config.PlaceBiome == PlaceBiome.Surface)
                    {
                        for (int c = 0; c < Config.BlockCount; c++)
                        {
                            for (int retry = 0; retry < Config.MaxPlaceRetry; retry++)
                            {
                                int x = random.Next(areaLeft, areaLeft + width);
                                bool result = PlaceBlockToSurface(
                                    sandbox,
                                    Config,
                                    sizeX: random.Next(Config.BlockMinX, Config.BlockMaxX),
                                    sizeY: random.Next(Config.BlockMinY, Config.BlockMaxY),
                                    x);
                                if (result)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else if (Config.PlaceBiome == PlaceBiome.Cavern)
                    {
                        double[] cavernTop = (double[])runner.GlobalConfig["CavernTop"];
                        double[] cavernBottom = (double[])runner.GlobalConfig["CavernBottom"];
                        for (int c = 0; c < Config.BlockCount; c++)
                        {
                            for (int retry = 0; retry < Config.MaxPlaceRetry; retry++)
                            {
                                int x = random.Next(areaLeft, areaLeft + width);
                                int y = random.Next((int)cavernTop[x], (int)cavernBottom[x]);
                                bool placeToTop = random.Next(2) == 0;
                                int? maxY = placeToTop ? (int?)((int)cavernTop[x] - 2) : null;
                                bool result = PlaceBlockToCavern(
                                    sandbox,
                                    Config,
                                    sizeX: random.Next(Config.BlockMinX, Config.BlockMaxX),
                                    sizeY: random.Next(Config.BlockMinY, Config.BlockMaxY),
                                    x,
                                    y,
                                    placeToTop,
                                    maxY);
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
            /// コンストラクタ.
            /// </summary>
            public RandomSizeBlockWithAreaContext()
            {
                BlockCount = 100;
                ChestProbably = 0.03;
            }

            [Category("部屋エリア設置")]
            [DisplayName("エリア内の部屋の個数")]
            [Description("エリア内に設置する部屋の個数")]
            public new int BlockCount { get; set; } = 200;

            [Category("部屋エリア設置")]
            [DisplayName("エリア数")]
            [Description("部屋を集中して設置するエリアの個数")]
            public int AreaCount { get; set; } = 5;

            [Category("部屋エリア設置")]
            [DisplayName("近くのエリアからの最小距離")]
            [Description("近くのエリアから最小でも離れている距離.この距離以内にはエリアは生成されない.")]
            public int MinDistanceFromNearbyArea { get; set; } = 200;

            [Category("部屋エリア設置")]
            [DisplayName("エリア最小幅")]
            [Description("部屋を集中して設置するエリアの最小幅")]
            public int AreaMinWidth { get; set; } = 30;

            [Category("部屋エリア設置")]
            [DisplayName("エリア最大幅")]
            [Description("部屋を集中して設置するエリアの最小幅")]
            public int AreaMaxWidth { get; set; } = 150;

            [Category("部屋エリア設置")]
            [DisplayName("エリア選択リトライ数")]
            [Description("エリア選択をやり直す回数.選択に失敗した場合生成されない.失敗する原因=近くのエリアからの最小距離が大きすぎるなど")]
            public int MaxAreaSelectRetry { get; set; } = 10;
        }
    }
}
