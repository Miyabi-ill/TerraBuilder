// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TerraBuilder.WorldGeneration
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// ワールド生成の全体を通じて使われる設定.
    /// TODO: これはサンドボックスによって提供されるべき？サンドボックスの肥大化を考慮してこの実装になっている.
    /// </summary>
    public class GlobalConfig : LayerConfig, INotifyPropertyChanged
    {
        private int seed = 42;
        private int surfaceLevel = 250;
        private int respawnLevel = 100;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 地表の高さ.
        /// </summary>
        [Category("ワールド設定")]
        [DisplayName("地表レベル")]
        [Description("地表の高さ(空中/地下の境目)を設定する")]
        public int SurfaceLevel
        {
            get => this.surfaceLevel;
            set
            {
                this.surfaceLevel = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.SurfaceLevel)));
            }
        }

        /// <summary>
        /// リスポーン地点の高さ.
        /// </summary>
        [Category("ワールド設定")]
        [DisplayName("リスポーン地点高さ")]
        [Description("リスポーン地点の高さを設定する")]
        public int RespawnLevel
        {
            get => this.respawnLevel;
            set
            {
                this.respawnLevel = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.RespawnLevel)));
            }
        }

        /// <summary>
        /// シード値.
        /// </summary>
        [Category("ワールド設定")]
        [DisplayName("シード値")]
        [Description("ワールド生成に使われるシード値")]
        public int Seed
        {
            get => this.seed;
            set
            {
                this.seed = value;
                this.Random = new Random(this.seed);
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Seed)));
            }
        }

        /// <summary>
        /// ワールド生成に使われるランダムインスタンス.
        /// </summary>
        [Browsable(false)]
        public Random Random { get; private set; } = new Random(42);
    }
}
