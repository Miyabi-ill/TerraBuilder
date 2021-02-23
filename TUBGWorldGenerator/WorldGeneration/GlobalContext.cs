namespace TUBGWorldGenerator.WorldGeneration
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// ワールド生成の全体を通じて使われる設定。
    /// </summary>
    public class GlobalContext : ActionContext
    {
        private int seed = 42;

        public int SurfaceLevel { get; set; } = 250;

        public int RespawnLevel { get; set; } = 100;

        public int Seed
        {
            get => seed;
            set
            {
                seed = value;
                Random = new Random(seed);
            }
        }

        [Browsable(false)]
        public Random Random { get; private set; } = new Random(42);
    }
}
