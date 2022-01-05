namespace TerraBuilder.WorldGeneration.Layers.Biomes
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using TerraBuilder.Utils;
    using TerraBuilder.WorldEdit;

    /// <summary>
    /// 地下に水を生成する生成アクション.
    /// </summary>
    [Action]
    public class CavernWater : IWorldGenerationLayer<CavernWater.CavernWaterConfig>
    {
        /// <inheritdoc/>
        public string Name => nameof(CavernWater);

        /// <inheritdoc/>
        public string Description => "地下に水を生成する.洞窟の生成が必須.";

        /// <inheritdoc/>
        public CavernWaterConfig Config { get; set; } = new CavernWaterConfig();

        /// <inheritdoc/>
        public bool Apply(WorldGenerationRunner runner, WorldSandbox sandbox, out Dictionary<string, object> generatedValueDict)
        {
            GlobalConfig globalContext = runner.GlobalConfig;
            int cavernTop = (int)runner.GetGeneratedValue<Caverns, double[]>("CavernTop").Min();
            int cavernBottom = (int)runner.GetGeneratedValue<Caverns, double[]>("CavernBottom").Max();

            for (int i = 0; i < this.Config.RandomWaterBlockCount; i++)
            {
                int sizeX = globalContext.Random.Next(this.Config.RandomWaterBlockMinX, this.Config.RandomWaterBlockMaxX + 1);
                int sizeY = globalContext.Random.Next(this.Config.RandomWaterBlockMinY, this.Config.RandomWaterBlockMaxY + 1);
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
                        sandbox[new Coordinate(cx, cy)].liquid = 255;
                    }
                }
            }

            Liquids.Settle(sandbox);

            generatedValueDict = new Dictionary<string, object>();
            return true;
        }

        public class CavernWaterConfig : LayerConfig
        {
            /// <summary>
            /// 水の塊の設置数.
            /// </summary>
            [Category("水の塊")]
            [DisplayName("設置数")]
            public int RandomWaterBlockCount { get; set; } = 15;

            /// <summary>
            /// 水の塊の最小の幅.
            /// </summary>
            [Category("水の塊")]
            [DisplayName("水の塊の最小幅")]
            [Description("水の塊の最小の幅")]
            public int RandomWaterBlockMinX { get; set; } = 10;

            /// <summary>
            /// 水の塊の最大の幅.
            /// </summary>
            [Category("水の塊")]
            [DisplayName("水の塊の最大幅")]
            [Description("水の塊の最大の幅")]
            public int RandomWaterBlockMaxX { get; set; } = 50;

            /// <summary>
            /// 水の塊の最小の高さ.
            /// </summary>
            [Category("水の塊")]
            [DisplayName("水の塊の最小高さ")]
            [Description("水の塊の最小の高さ")]
            public int RandomWaterBlockMinY { get; set; } = 10;

            /// <summary>
            /// 水の塊の最大の高さ.
            /// </summary>
            [Category("水の塊")]
            [DisplayName("水の塊の最大高さ")]
            [Description("水の塊の最大の高さ")]
            public int RandomWaterBlockMaxY { get; set; } = 50;
        }
    }
}
