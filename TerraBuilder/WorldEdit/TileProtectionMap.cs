// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TerraBuilder.WorldEdit
{
    using System;
    using Terraria;

    /// <summary>
    /// タイルを保護するマップ。このマップを参照して、タイルの設置可否が決まる。
    /// </summary>
    public class TileProtectionMap
    {
        /// <summary>
        /// タイル保護マップのコンストラクタ。
        /// </summary>
        /// <param name="sandbox">ワールドサンドボックス</param>
        public TileProtectionMap(WorldSandbox sandbox)
        {
            this.MapSizeX = sandbox.TileCountX;
            this.MapSizeY = sandbox.TileCountY;

            this.TileProtectionTypes = new TileProtectionType[this.MapSizeX, this.MapSizeY];
        }

        /// <summary>
        /// タイル保護の種類
        /// </summary>
        [Flags]
        public enum TileProtectionType
        {
            /// <summary>
            /// タイル保護なし
            /// </summary>
            None = 0x0,

            /// <summary>
            /// 上面設置可(Platformなど)なら置き換え可
            /// </summary>
            TopSolid = 0x1,

            /// <summary>
            /// 下面設置可なら置き換え可
            /// </summary>
            BottomSolid = 0x2,

            /// <summary>
            /// 左面設置可なら置き換え可
            /// </summary>
            LeftSolid = 0x4,

            /// <summary>
            /// 右面設置可なら置き換え可
            /// </summary>
            RightSolid = 0x8,

            /// <summary>
            /// 固形ブロック（土など）
            /// </summary>
            Solid = TopSolid | BottomSolid | LeftSolid | RightSolid,

            /// <summary>
            /// 壁紙置き換え可
            /// </summary>
            Wall = 0x10,

            /// <summary>
            /// 液体種類、量置き換え可
            /// </summary>
            Liquid = 0x20,

            /// <summary>
            /// ワイヤ全種類置き換え可
            /// </summary>
            Wire = 0x30,

            /// <summary>
            /// アクチュエーター置き換え可
            /// </summary>
            Actuator = 0x40,

            /// <summary>
            /// タイル形状（ハーフ、斜め）変更可
            /// </summary>
            TileShape = 0x80,

            /// <summary>
            /// タイルID一致なら変更可
            /// </summary>
            TileType = 0x100,

            /// <summary>
            /// タイルフレーム（スタイル）一致なら変更可
            /// </summary>
            TileFrame = 0x200,

            /// <summary>
            /// 設置するアイテムが一致（タイルIDとスタイル一致）なら変更可
            /// </summary>
            SameTileItem = TileType | TileFrame,

            /// <summary>
            /// 変更不可（完全一致＝同じもの同士の置き換えなら変更可）
            /// </summary>
            All = TopSolid | BottomSolid | LeftSolid | RightSolid | Wall | Liquid | Wire | Actuator | TileShape | TileType | TileFrame,
        }

        private int MapSizeX { get; }

        private int MapSizeY { get; }

        private TileProtectionType[,] TileProtectionTypes { get; }

        /// <summary>
        /// 指定した座標のタイル保護タイプを取得する。
        /// </summary>
        /// <param name="coordinate">タイル座標。</param>
        /// <returns>指定した座標のタイル保護タイプ。</returns>
        public TileProtectionType this[Coordinate coordinate]
        {
            get => this.TileProtectionTypes[coordinate.X, coordinate.Y];

            set => this.TileProtectionTypes[coordinate.X, coordinate.Y] = value;
        }

        /// <summary>
        /// タイル保護マップを全てクリアする（全てのタイルを保護されていない状態にする）。
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

        public Tile PlaceTile(WorldSandbox sandbox, Tile tile, int x, int y)
        {
            TileProtectionType type = this.TileProtectionTypes[x, y];
            if (type == TileProtectionType.None)
            {
                sandbox.Tiles[x, y] = tile;
                return tile;
            }

            if (sandbox.Tiles[x, y] == null)
            {
                sandbox.Tiles[x, y] = new Tile();
            }

            Tile sandboxTile = (Tile)sandbox.Tiles[x, y];

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

        public bool PlaceTiles(WorldSandbox sandbox, TileProtectionType[,] protectionMap, Tile[,] tiles, int x, int y)
        {
            if (protectionMap.GetLength(0) != tiles.GetLength(0)
                || protectionMap.GetLength(1) != tiles.GetLength(1))
            {
                return false;
            }

            int width = tiles.GetLength(0);
            int height = tiles.GetLength(1);

            for (int tx = x; tx < x + width; tx++)
            {
                for (int ty = y; ty < y + height; ty++)
                {
                    this.PlaceTile(sandbox, tiles[tx - x, ty - y], tx, ty);
                    this.TileProtectionTypes[tx, ty] |= protectionMap[tx - x, ty - y];
                }
            }

            return true;
        }

        public void SetProtection(TileProtectionType type, int minX, int minY, int maxX, int maxY)
        {
            if (minX > maxX)
            {
                throw new ArgumentException("minX must be less than maxX.");
            }

            if (minY > maxY)
            {
                throw new ArgumentException("minY must be less than maxY.");
            }

            for (int x = minX; x < maxX + 1; x++)
            {
                for (int y = minY; y < maxY + 1; y++)
                {
                    this.TileProtectionTypes[x, y] = type;
                }
            }
        }
    }
}
