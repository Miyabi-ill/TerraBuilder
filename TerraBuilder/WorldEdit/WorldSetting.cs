// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TerraBuilder.WorldEdit
{
    using TerraBuilder.WorldEdit.Settings;
    using Terraria;

    /// <summary>
    /// サンドボックスに含まれる設定.
    /// サンドボックスリセット時にリセットされる.
    /// </summary>
    public class WorldSetting
    {
        /// <summary>
        /// ワールドの汚染.
        /// </summary>
        public enum WorldEvil
        {
            /// <summary>
            /// 紫不浄.
            /// </summary>
            Corruption = 0,

            /// <summary>
            /// クリムゾン.
            /// </summary>
            Crimson = 1,
        }

        /// <summary>
        /// 地表の高さ（空中/地下の境目）.
        /// </summary>
        public int SurfaceLevel { get; set; }

        /// <summary>
        /// シード値.
        /// </summary>
        public int Seed { get; set; } = 42;

        /// <summary>
        /// ワールドID.
        /// この値が同じワールドは、マップ情報が共有される.
        /// </summary>
        public int WorldID { get; set; }

        /// <summary>
        /// リスポーン地点の座標.
        /// </summary>
        public Coordinate SpawnCoordinate
        {
            get => new Coordinate(Main.spawnTileX, Main.spawnTileY);
            set
            {
                Main.spawnTileX = value.X;
                Main.spawnTileY = value.Y;
            }
        }

        /// <summary>
        /// ワールドの鉱石ティア.
        /// </summary>
        public OreTiers OreTiers { get; set; }

        /// <summary>
        /// ワールドの汚染.
        /// </summary>
        public WorldEvil Evil { get; set; }

        /// <summary>
        /// ダンジョンの設置サイド.
        /// </summary>
        public DungeonSide DungeonSide { get; set; }
    }
}
