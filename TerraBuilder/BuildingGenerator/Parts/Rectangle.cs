namespace TerraBuilder.BuildingGenerator.Parts
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using Terraria;

    [JsonConverter(typeof(PartsConverter))]
    public class Rectangle : BuildBase
    {
        [JsonProperty]
        public Size Size { get; set; }

        /// <summary>
        /// 範囲内をタイルで埋めるときのタイル内部名。
        /// </summary>
        [JsonProperty]
        public RandomValue FillTile { get; set; }

        /// <summary>
        /// 範囲内を壁で埋めるときの壁内部名。
        /// </summary>
        [JsonProperty]
        public RandomValue FillWall { get; set; }

        /// <summary>
        /// ペンキの名前
        /// </summary>
        [JsonProperty]
        public RandomValue Paint { get; set; }

        [JsonProperty]
        public RandomValue TileShape { get; set; }

        /// <inheritdoc/>
        public override Tile[,] Build(Random rand)
        {
            int width = (int)Size.Width.GetValue(rand);
            int height = (int)Size.Height.GetValue(rand);
            Tile[,] tiles = new Tile[width, height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int fillTileType = -1;
                    if (FillTile != null)
                    {
                        string fillTile = (string)FillTile.GetValue(rand);
                        fillTileType = TerrariaNameDict.TileNameToID[fillTile.ToLower()];
                    }

                    ushort fillWallType = 0;
                    if (FillWall != null)
                    {
                        string fillWall = (string)FillWall.GetValue(rand);
                        fillWallType = TerrariaNameDict.WallNameToID[fillWall.ToLower()];
                    }

                    byte paintType = 0;
                    if (Paint != null)
                    {
                        string paintName = (string)Paint.GetValue(rand);
                        paintType = TerrariaNameDict.PaintNameToID[paintName.ToLower()];
                    }

                    bool halfBlock = false;
                    byte slopeType = 0;
                    if (TileShape != null)
                    {
                        string tileShape = (string)TileShape.GetValue(rand);
                        switch (tileShape?.ToLowerInvariant())
                        {
                            case "half":
                                halfBlock = true;
                                slopeType = 0;
                                break;
                            case "leftbottom":
                                slopeType = 1;
                                halfBlock = false;
                                break;
                            case "lefttop":
                                slopeType = 3;
                                halfBlock = false;
                                break;
                            case "rightbottom":
                                slopeType = 2;
                                halfBlock = false;
                                break;
                            case "righttop":
                                slopeType = 4;
                                halfBlock = false;
                                break;
                            default:
                                halfBlock = false;
                                slopeType = 0;
                                break;
                        }
                    }

                    tiles[i, j] = new Tile();

                    if (fillWallType != 0)
                    {
                        tiles[i, j].wall = fillWallType;
                        tiles[i, j].wallColor(paintType);
                    }

                    if (fillTileType != -1)
                    {
                        tiles[i, j].type = (ushort)fillTileType;
                        tiles[i, j].active(true);
                        tiles[i, j].color(paintType);

                        if (halfBlock)
                        {
                            tiles[i, j].halfBrick(true);
                        }
                        else if (slopeType != 0)
                        {
                            tiles[i, j].slope(slopeType);
                        }
                    }
                }
            }

            return tiles;
        }
    }
}
