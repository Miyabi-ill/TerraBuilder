// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TerraBuilder.WorldEdit
{
    using System.ComponentModel;
    using Terraria;

    /// <summary>
    /// サンドボックス全体に関わるコンフィグ.
    /// </summary>
    public class WorldConfig
    {
        /// <summary>
        /// 地表の高さ.
        /// </summary>
        [Category("ワールド設定")]
        [DisplayName("地表レベル")]
        [Description("地表の高さ(空中/地下の境目)を設定する")]
        public int SurfaceLevel { get; set; }

        /// <summary>
        /// リスポーン地点の高さ.
        /// </summary>
        [Category("ワールド設定")]
        [DisplayName("リスポーン地点高さ")]
        [Description("リスポーン地点の高さを設定する")]
        public int RespawnLevel { get; set; }

        /// <summary>
        /// シード値.
        /// </summary>
        [Category("ワールド設定")]
        [DisplayName("シード値")]
        [Description("ワールド生成に使われるシード値")]
        public int Seed { get; set; } = 42;

        /// <summary>
        /// リスポーン地点X.
        /// </summary>
        public int SpawnTileX
        {
            get => Main.spawnTileX;
            set => Main.spawnTileX = value;
        }

        /// <summary>
        /// リスポーン地点Y.
        /// </summary>
        public int SpawnTileY
        {
            get => Main.spawnTileY;
            set => Main.spawnTileY = value;
        }
    }
}
