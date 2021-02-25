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

        /// <summary>
        /// 地表の高さ
        /// </summary>
        public int SurfaceLevel { get; set; } = 250;

        /// <summary>
        /// リスポーン地点の高さ
        /// </summary>
        public int RespawnLevel { get; set; } = 100;

        /// <summary>
        /// シード値
        /// </summary>
        public int Seed
        {
            get => seed;
            set
            {
                seed = value;
                Random = new Random(seed);
            }
        }

        /// <summary>
        /// ワールド生成に使われるランダムインスタンス
        /// </summary>
        [Browsable(false)]
        public Random Random { get; private set; } = new Random(42);
    }
}
