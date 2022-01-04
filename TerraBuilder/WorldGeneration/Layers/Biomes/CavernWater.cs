namespace TerraBuilder.WorldGeneration.Layers.Biomes
{
    using System.ComponentModel;
    using System.Linq;
    using TerraBuilder.Utils;

    /// <summary>
    /// 地下に水を生成する生成アクション.
    /// </summary>
    [Action]
    public class CavernWater : IWorldGenerationLayer<CavernWater.CavernWaterContext>
    {
        public string Name => nameof(CavernWater);

        public string Description => "地下に水を生成する.洞窟の生成が必須.";

        public CavernWaterContext Context { get; set; } = new CavernWaterContext();

        public bool Run(WorldSandbox sandbox)
        {
            GlobalConfig globalContext = WorldGenerationRunner.CurrentRunner.GlobalConfig;
            int cavernTop = (int)((double[])WorldGenerationRunner.CurrentRunner.GlobalConfig["CavernTop"]).Min();
            int cavernBottom = (int)((double[])WorldGenerationRunner.CurrentRunner.GlobalConfig["CavernBottom"]).Max();

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

        public class CavernWaterContext : LayerConfig
        {
            /// <summary>
            /// 水の塊の設置数
            /// </summary>
            [Category("水の塊")]
            [DisplayName("設置数")]
            public int RandomWaterBlockCount { get; set; } = 15;

            /// <summary>
            /// 水の塊の最小の幅
            /// </summary>
            [Category("水の塊")]
            [DisplayName("水の塊の最小幅")]
            [Description("水の塊の最小の幅")]
            public int RandomWaterBlockMinX { get; set; } = 10;

            /// <summary>
            /// 水の塊の最大の幅
            /// </summary>
            [Category("水の塊")]
            [DisplayName("水の塊の最大幅")]
            [Description("水の塊の最大の幅")]
            public int RandomWaterBlockMaxX { get; set; } = 50;

            /// <summary>
            /// 水の塊の最小の高さ
            /// </summary>
            [Category("水の塊")]
            [DisplayName("水の塊の最小高さ")]
            [Description("水の塊の最小の高さ")]
            public int RandomWaterBlockMinY { get; set; } = 10;

            /// <summary>
            /// 水の塊の最大の高さ
            /// </summary>
            [Category("水の塊")]
            [DisplayName("水の塊の最大高さ")]
            [Description("水の塊の最大の高さ")]
            public int RandomWaterBlockMaxY { get; set; } = 50;
        }
    }
}
