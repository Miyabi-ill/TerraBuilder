namespace TerraBuilder.WorldGeneration.Layers.Buildings
{
    using System;
    using System.ComponentModel;
    using Terraria.ID;

    /// <summary>
    /// ロープをランダムに配置する
    /// </summary>
    [Action]
    public class RandomRope : IWorldGenerationLayer<RandomRope.RandomRopeContext>
    {
        public string Name => nameof(RandomRope);

        public string Description => "地下にロープをランダムに配置する";

        public RandomRopeContext Context { get; set; } = new RandomRopeContext();

        public bool Run(WorldSandbox sandbox)
        {
            GlobalConfig globalContext = WorldGenerationRunner.CurrentRunner.GlobalConfig;

            double[] cavernTop = (double[])WorldGenerationRunner.CurrentRunner.GlobalConfig["CavernTop"];
            double[] cavernBottom = (double[])WorldGenerationRunner.CurrentRunner.GlobalConfig["CavernBottom"];
            for (int i = 0; i < Context.RopeCount; i++)
            {
                int x = globalContext.Random.Next(100, sandbox.TileCountX - 100);
                int length = globalContext.Random.Next(Context.RopeMinLength, Context.RopeMaxLength + 1);
                int y = globalContext.Random.Next((int)cavernTop[x], Math.Max((int)cavernBottom[x] - length - 1, (int)cavernTop[x] + 1));

                for (int cy = y; cy < y + length; cy++)
                {
                    if (!sandbox.Tiles[x, cy].active())
                    {
                        sandbox.Tiles[x, cy].type = TileID.Rope;
                        sandbox.Tiles[x, cy].active(true);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return true;
        }

        public class RandomRopeContext : LayerConfig
        {
            [Category("ロープ生成")]
            [DisplayName("ロープ設置数")]
            [Description("ロープを設置する数")]
            public int RopeCount { get; set; } = 300;

            [Category("ロープ生成")]
            [DisplayName("ロープ最小長さ")]
            [Description("ロープの最小長さ")]
            public int RopeMinLength { get; set; } = 15;

            [Category("ロープ生成")]
            [DisplayName("ロープ最大長さ")]
            [Description("ロープの最大長さ")]
            public int RopeMaxLength { get; set; } = 50;
        }
    }
}
