namespace TerraBuilder.WorldGeneration
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// ワールド生成の全体を通じて使われる設定.
    /// </summary>
    public class GlobalContext : ActionContext, INotifyPropertyChanged
    {
        private int seed = 42;
        private int surfaceLevel = 250;
        private int respawnLevel = 100;
        private Random rand = new Random(42);

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 地表の高さ
        /// </summary>
        [Category("ワールド設定")]
        [DisplayName("地表レベル")]
        [Description("地表の高さ(空中/地下の境目)を設定する")]
        public int SurfaceLevel
        {
            get => surfaceLevel;
            set
            {
                surfaceLevel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SurfaceLevel)));
            }
        }

        /// <summary>
        /// リスポーン地点の高さ
        /// </summary>
        [Category("ワールド設定")]
        [DisplayName("リスポーン地点高さ")]
        [Description("リスポーン地点の高さを設定する")]
        public int RespawnLevel
        {
            get => respawnLevel;
            set
            {
                respawnLevel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RespawnLevel)));
            }
        }

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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Seed)));
            }
        }

        /// <summary>
        /// ワールド生成に使われるランダムインスタンス
        /// </summary>
        [Browsable(false)]
        public Random Random
        {
            get => rand;
            set
            {
                rand = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Random)));
            }
        }
    }
}
