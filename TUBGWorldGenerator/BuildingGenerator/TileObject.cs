﻿namespace TUBGWorldGenerator.BuildingGenerator
{
    using System;
    using Terraria;
    using Terraria.ObjectData;

    public class TileObject : BuildBase
    {
        private string itemName;
        private int tileId;
        private int style;
        private int alternate;

        /// <inheritdoc/>
        public override int X { get; set; }

        /// <inheritdoc/>
        public override int Y { get; set; }

        /// <summary>
        /// アイテム名。設定すると<see cref="TileID"/>と<see cref="Style"/>が自動的に設定される。
        /// </summary>
        public string ItemName
        {
            get => itemName;
            set
            {
                itemName = value;
                if (TerrariaNameDict.ItemNameToItem.ContainsKey(itemName))
                {
                    Item item = TerrariaNameDict.ItemNameToItem[itemName];
                    TileID = item.createTile;
                    Style = item.placeStyle;
                }
            }
        }

        /// <summary>
        /// 設置するタイルID。基本的に<see cref="ItemName"/>を使い、それで設定できないものに使う。
        /// </summary>
        public int TileID
        {
            get => tileId;
            set
            {
                tileId = value;
            }
        }

        /// <summary>
        /// 設置するスタイル。基本的に<see cref="ItemName"/>を使い、それで設定できないものに使う。
        /// </summary>
        public int Style
        {
            get => style;
            set
            {
                style = value;
            }
        }

        /// <summary>
        /// 同じタイル、スタイルの中で違うテクスチャを選択する。ex.本、カボチャランタン、プレゼントなど
        /// </summary>
        public int Alternate
        {
            get => alternate;
            set
            {
                alternate = value;
            }
        }

        /// <inheritdoc/>
        public override Tile[,] Build()
        {
            var tileObjectData = TileObjectData.GetTileData(TileID, Style, Alternate);
            if (tileObjectData == null)
            {
                return new Tile[0, 0];
            }

            Tile[,] tiles = new Tile[tileObjectData.Width, tileObjectData.Height];

            ushort num = (ushort)TileID;
            int styleMultiplyX = (Style * tileObjectData.StyleMultiplier) + Style;
            int styleMultiplyY = 0;

            // スタイルが複数あり、テクスチャの最大幅を超えてしまう場合、改行したテクスチャの位置を取得する
            if (tileObjectData.StyleWrapLimit > 0)
            {
                styleMultiplyY = styleMultiplyX / tileObjectData.StyleWrapLimit * tileObjectData.StyleLineSkip;
                styleMultiplyX %= tileObjectData.StyleWrapLimit;
            }

            int styleX;
            int styleY;
            if (tileObjectData.StyleHorizontal)
            {
                styleX = tileObjectData.CoordinateFullWidth * styleMultiplyX;
                styleY = tileObjectData.CoordinateFullHeight * styleMultiplyY;
            }
            else
            {
                styleX = tileObjectData.CoordinateFullWidth * styleMultiplyY;
                styleY = tileObjectData.CoordinateFullHeight * styleMultiplyX;
            }

            for (int i = 0; i < tileObjectData.Width; i++)
            {
                int frameX = styleX + (i * (tileObjectData.CoordinateWidth + tileObjectData.CoordinatePadding));
                int frameY = styleY;
                for (int j = 0; j < tileObjectData.Height; j++)
                {
                    tiles[i, j] = new Tile();
                    Tile tile = tiles[i, j];
                    tile.active(active: true);
                    tile.frameX = (short)frameX;
                    tile.frameY = (short)frameY;
                    tile.type = num;
                    frameY += tileObjectData.CoordinateHeights[j] + tileObjectData.CoordinatePadding;
                }
            }

            return tiles;
        }
    }
}
