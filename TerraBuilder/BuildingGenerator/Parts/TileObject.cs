namespace TerraBuilder.BuildingGenerator.Parts
{
    using System;
    using Newtonsoft.Json;
    using Terraria;
    using Terraria.ObjectData;

    public class TileObject : BuildBase
    {
        private RandomValue<string> itemName;
        private RandomValue<int> tileId;
        private RandomValue<int> style;
        private RandomValue<int> alternate;

        private RandomValue<string> paintName;
        private RandomValue<byte> paintType;

        /// <summary>
        /// アイテム名。設定すると<see cref="TileID"/>と<see cref="Style"/>が自動的に設定される。
        /// </summary>
        [JsonProperty]
        public RandomValue<string> ItemName
        {
            get => itemName;
            set
            {
                itemName = value;
                tileId = null;
                style = null;
            }
        }

        /// <summary>
        /// 設置するタイルID。基本的に<see cref="ItemName"/>を使い、それで設定できないものに使う。
        /// </summary>
        [JsonProperty]
        public RandomValue<int> TileID
        {
            get => tileId;
            set
            {
                tileId = value;
                itemName = null;
            }
        }

        /// <summary>
        /// 設置するスタイル。基本的に<see cref="ItemName"/>を使い、それで設定できないものに使う。
        /// </summary>
        [JsonProperty]
        public RandomValue<int> Style
        {
            get => style;
            set
            {
                style = value;
                itemName = null;
            }
        }

        /// <summary>
        /// 同じタイル、スタイルの中で違うテクスチャを選択する。ex.本、カボチャランタン、プレゼントなど
        /// </summary>
        [JsonProperty]
        public RandomValue<int> Alternate
        {
            get => alternate;
            set
            {
                alternate = value;
            }
        }

        /// <summary>
        /// ペンキの名前
        /// </summary>
        [JsonProperty]
        public RandomValue<string> Paint
        {
            get => paintName;
            set
            {
                paintName = value;
                paintType = null;
            }
        }

        /// <summary>
        /// ペンキID
        /// </summary>
        [JsonIgnore]
        public RandomValue<byte> PaintType
        {
            get => paintType;
            set
            {
                paintType = value;
                paintName = null;
            }
        }

        /// <inheritdoc/>
        public override Tile[,] Build(Random rand)
        {
            string itemName = this.itemName?.GetValue(rand);
            string paintName = this.paintName?.GetValue(rand);
            int tileId = 0;
            int style = 0;
            int alternate = Alternate?.GetValue(rand) ?? 0;
            byte paintType = 0;

            if (!string.IsNullOrEmpty(itemName) && TerrariaNameDict.ItemNameToItem.ContainsKey(itemName))
            {
                Item item = TerrariaNameDict.ItemNameToItem[itemName];
                tileId = item.createTile;
                style = item.placeStyle;
            }
            else
            {
                tileId = TileID?.GetValue(rand) ?? 0;
                style = Style?.GetValue(rand) ?? 0;
            }

            if (!string.IsNullOrEmpty(paintName) && TerrariaNameDict.PaintNameToID.ContainsKey(paintName))
            {
                paintType = TerrariaNameDict.PaintNameToID[paintName];
            }
            else
            {
                paintType = PaintType?.GetValue(rand) ?? 0;
            }

            if (tileId < 0)
            {
                return new Tile[0, 0];
            }

            var tileObjectData = TileObjectData.GetTileData(tileId, style, alternate);
            if (tileObjectData == null)
            {
                return new Tile[0, 0];
            }

            Tile[,] tiles = new Tile[tileObjectData.Width, tileObjectData.Height];

            ushort num = (ushort)tileId;
            int styleMultiplyX = tileObjectData.CalculatePlacementStyle(style, alternate, 0);
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
                    tiles[i, j].color(paintType);
                    frameY += tileObjectData.CoordinateHeights[j] + tileObjectData.CoordinatePadding;
                }
            }

            return tiles;
        }
    }
}
