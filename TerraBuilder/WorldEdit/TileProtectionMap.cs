// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TerraBuilder.WorldEdit
{
    using System;
    using Terraria;

    /// <summary>
    /// タイルを保護するマップ.このマップを参照して、タイルの設置可否が決まる.
    /// </summary>
    public class TileProtectionMap
    {
        /// <summary>
        /// タイル保護マップのコンストラクタ.
        /// </summary>
        /// <param name="sandbox">ワールドサンドボックス.</param>
        /// <exception cref="ArgumentNullException"><see cref="sandbox"/>がnullのとき.</exception>
        public TileProtectionMap(WorldSandbox sandbox)
        {
            if (sandbox == null)
            {
                throw new ArgumentNullException(nameof(sandbox));
            }

            this.MapSizeX = sandbox.TileCountX;
            this.MapSizeY = sandbox.TileCountY;

            this.TileProtectionTypes = new TileProtectionType[this.MapSizeX, this.MapSizeY];
            this.Sandbox = sandbox;
        }

        /// <summary>
        /// タイル保護の種類.
        /// </summary>
        [Flags]
        public enum TileProtectionType
        {
            /// <summary>
            /// タイル保護なし
            /// </summary>
            None = 0,

            /// <summary>
            /// 上面設置可(Platformなど)なら置き換え可
            /// </summary>
            TopSolid = 1,

            /// <summary>
            /// 下面設置可なら置き換え可
            /// </summary>
            BottomSolid = 1 << 1,

            /// <summary>
            /// 左面設置可なら置き換え可
            /// </summary>
            LeftSolid = 1 << 2,

            /// <summary>
            /// 右面設置可なら置き換え可
            /// </summary>
            RightSolid = 1 << 3,

            /// <summary>
            /// 固形ブロック（土など）
            /// </summary>
            Solid = TopSolid | BottomSolid | LeftSolid | RightSolid,

            /// <summary>
            /// 壁紙置き換え可
            /// </summary>
            Wall = 1 << 4,

            /// <summary>
            /// 液体種類、量置き換え可
            /// </summary>
            Liquid = 1 << 5,

            /// <summary>
            /// ワイヤ全種類置き換え可
            /// </summary>
            Wire = 1 << 6,

            /// <summary>
            /// アクチュエーター置き換え可
            /// </summary>
            Actuator = 1 << 7,

            /// <summary>
            /// タイル形状（ハーフ、斜め）変更可
            /// </summary>
            TileShape = 1 << 8,

            /// <summary>
            /// タイルID一致なら変更可
            /// </summary>
            TileType = 1 << 9,

            /// <summary>
            /// タイルフレーム（スタイル）一致なら変更可
            /// </summary>
            TileFrame = 1 << 10,

            /// <summary>
            /// 設置するアイテムが一致（タイルIDとスタイル一致）なら変更可
            /// </summary>
            SameTileItem = TileType | TileFrame,

            /// <summary>
            /// 変更不可（完全一致＝同じもの同士の置き換えなら変更可）
            /// </summary>
            All = TopSolid | BottomSolid | LeftSolid | RightSolid | Wall | Liquid | Wire | Actuator | TileShape | TileType | TileFrame,
        }

        private WorldSandbox Sandbox { get; }

        private int MapSizeX { get; }

        private int MapSizeY { get; }

        private TileProtectionType[,] TileProtectionTypes { get; }

        /// <summary>
        /// 指定した座標のタイル保護タイプを取得する.
        /// </summary>
        /// <param name="coordinate">タイル座標.</param>
        /// <returns>指定した座標のタイル保護タイプ.</returns>
        public TileProtectionType this[Coordinate coordinate]
        {
            get => this.TileProtectionTypes[coordinate.X, coordinate.Y];

            set => this.TileProtectionTypes[coordinate.X, coordinate.Y] = value;
        }

        /// <summary>
        /// タイル保護マップを全てクリアする（全てのタイルを保護されていない状態にする）.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < this.MapSizeX; i++)
            {
                for (int j = 0; j < this.MapSizeY; j++)
                {
                    this.TileProtectionTypes[i, j] = TileProtectionType.None;
                }
            }
        }

        /// <summary>
        /// タイルをサンドボックスに設置する.
        /// タイルが保護されていた場合、保護部分以外が設置できるなら設置する.
        /// </summary>
        /// <param name="coordinate">設置先座標.</param>
        /// <param name="tile">設置するタイル.</param>
        /// <returns>設置できたタイル（`sandbox.Tiles[coordinate.X, coordinate.Y]`と同一）.</returns>
        public Tile PlaceTile(Coordinate coordinate, Tile tile)
        {
            TileProtectionType type = this.TileProtectionTypes[coordinate.X, coordinate.Y];
            if (type == TileProtectionType.None)
            {
                this.Sandbox[coordinate] = tile;
                return tile;
            }

            if (this.Sandbox[coordinate] == null)
            {
                this.Sandbox[coordinate] = new Tile();
            }

            Tile sandboxTile = this.Sandbox[coordinate];

            bool rejectTilePlace = false;
            bool rejectWallPlace = false;
            bool rejectWirePlace = false;
            bool rejectLiquidPlace = false;
            bool rejectActuatorPlace = false;
            if (type.HasFlag(TileProtectionType.TopSolid))
            {
                if (!tile.active() || !Main.tileSolidTop[tile.type])
                {
                    rejectTilePlace = true;
                }
            }

            if (!rejectTilePlace
                && (type.HasFlag(TileProtectionType.BottomSolid)
                || type.HasFlag(TileProtectionType.LeftSolid)
                || type.HasFlag(TileProtectionType.RightSolid)))
            {
                if (!tile.active() || !Main.tileSolid[tile.type])
                {
                    rejectTilePlace = true;
                }
            }

            if (type.HasFlag(TileProtectionType.Wall))
            {
                rejectWallPlace = true;
            }

            if (type.HasFlag(TileProtectionType.Wire))
            {
                rejectWirePlace = true;
            }

            if (type.HasFlag(TileProtectionType.Liquid))
            {
                rejectLiquidPlace = true;
            }

            if (type.HasFlag(TileProtectionType.Actuator))
            {
                rejectActuatorPlace = true;
            }

            if (!rejectTilePlace
                && type.HasFlag(TileProtectionType.TileType))
            {
                if (!tile.active() || tile.type != sandboxTile.type)
                {
                    rejectTilePlace = true;
                }
            }

            if (!rejectTilePlace
                && type.HasFlag(TileProtectionType.TileShape))
            {
                if (!tile.active() || tile.halfBrick() != sandboxTile.halfBrick() || tile.slope() != sandboxTile.slope())
                {
                    rejectTilePlace = true;
                }
            }

            if (!rejectTilePlace
                && type.HasFlag(TileProtectionType.TileFrame))
            {
                if (!tile.active() || tile.frameX != sandboxTile.frameX || tile.frameY != sandboxTile.frameY)
                {
                    rejectTilePlace = true;
                }
            }

            if (!rejectTilePlace)
            {
                sandboxTile.type = tile.type;
                sandboxTile.active(tile.active());
                sandboxTile.frameX = tile.frameX;
                sandboxTile.frameY = tile.frameY;
                sandboxTile.slope(tile.slope());
                sandboxTile.halfBrick(tile.halfBrick());
            }

            if (!rejectWallPlace)
            {
                sandboxTile.wall = tile.wall;
            }

            if (!rejectLiquidPlace)
            {
                sandboxTile.liquid = tile.liquid;
                sandboxTile.liquidType(tile.liquidType());
            }

            if (!rejectWirePlace)
            {
                sandboxTile.wire(tile.wire());
                sandboxTile.wire2(tile.wire2());
                sandboxTile.wire3(tile.wire3());
                sandboxTile.wire4(tile.wire4());
            }

            if (!rejectActuatorPlace)
            {
                sandboxTile.actuator(tile.actuator());
            }

            return sandboxTile;
        }
    }
}
