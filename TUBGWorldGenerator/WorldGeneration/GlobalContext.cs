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
        [Category("ワールド設定")]
        [DisplayName("地表レベル")]
        [Description("地表の高さ(空中/地下の境目)を設定する")]
        public int SurfaceLevel { get; set; } = 250;

        /// <summary>
        /// リスポーン地点の高さ
        /// </summary>
        [Category("ワールド設定")]
        [DisplayName("リスポーン地点高さ")]
        [Description("リスポーン地点の高さを設定する")]
        public int RespawnLevel { get; set; } = 100;

        /// <summary>
        /// シード値
        /// </summary>
        [Category("ワールド設定")]
        [DisplayName("シード値")]
        [Description("ワールド生成に使われるシード値")]
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
