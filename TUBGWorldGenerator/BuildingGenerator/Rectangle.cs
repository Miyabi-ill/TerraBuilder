namespace TUBGWorldGenerator.BuildingGenerator
{
    using System.Linq;
    using Newtonsoft.Json;
    using Terraria;

    public class Rectangle : BuildBase
    {
        private string fillTile;
        private string fillWall;

        private int fillTileType = -1;
        private ushort fillWallType;

        private string paintName;
        private byte paintType;

        private string tileShape;
        private bool halfBlock;
        private byte slopeType;

        /// <inheritdoc/>
        [JsonProperty]
        public override int X { get; set; }

        /// <inheritdoc/>
        [JsonProperty]
        public override int Y { get; set; }

        [JsonProperty]
        public Size Size { get; set; }

        /// <summary>
        /// 範囲内をタイルで埋めるときのタイル内部名。
        /// </summary>
        [JsonProperty]
        public string FillTile
        {
            get => fillTile;
            set
            {
                fillTile = value;
                fillTileType = TerrariaNameDict.TileNameToID[fillTile.ToLower()];
            }
        }

        /// <summary>
        /// 範囲内をタイルで埋めるときのタイルID。
        /// </summary>
        [JsonIgnore]
        public int FillTileType
        {
            get => fillTileType;
            set
            {
                fillTileType = value;
            }
        }

        /// <summary>
        /// 範囲内を壁で埋めるときの壁内部名。
        /// </summary>
        [JsonProperty]
        public string FillWall
        {
            get => fillWall;
            set
            {
                fillWall = value;
                fillWallType = TerrariaNameDict.WallNameToID[fillWall];
            }
        }

        /// <summary>
        /// 範囲内を壁で埋めるときの壁ID。
        /// </summary>
        [JsonIgnore]
        public ushort FillWallType
        {
            get => fillWallType;
            set
            {
                fillWallType = value;
            }
        }

        /// <summary>
        /// ペンキの名前
        /// </summary>
        [JsonProperty]
        public string Paint
        {
            get => paintName;
            set
            {
                paintName = value;
                paintType = TerrariaNameDict.PaintNameToID[paintName];
            }
        }

        /// <summary>
        /// ペンキID
        /// </summary>
        [JsonIgnore]
        public byte PaintType
        {
            get => paintType;
            set => paintType = value;
        }

        [JsonProperty]
        public string TileShape
        {
            get => tileShape;
            set
            {
                tileShape = value;
                switch (tileShape.ToLowerInvariant())
                {
                    case "half":
                        HalfBlock = true;
                        SlopeType = 0;
                        break;
                    case "leftbottom":
                        SlopeType = 1;
                        HalfBlock = false;
                        break;
                    case "lefttop":
                        SlopeType = 3;
                        HalfBlock = false;
                        break;
                    case "rightbottom":
                        SlopeType = 2;
                        HalfBlock = false;
                        break;
                    case "righttop":
                        SlopeType = 4;
                        HalfBlock = false;
                        break;
                    default:
                        HalfBlock = false;
                        SlopeType = 0;
                        break;
                }
            }
        }

        [JsonIgnore]
        public bool HalfBlock
        {
            get => halfBlock;
            set => halfBlock = value;
        }

        [JsonIgnore]
        public byte SlopeType
        {
            get => slopeType;
            set => slopeType = value;
        }

        /// <inheritdoc/>
        public override Tile[,] Build()
        {
            Tile[,] tiles = new Tile[Size.Width, Size.Height];

            for (int i = 0; i < Size.Width; i++)
            {
                for (int j = 0; j < Size.Height; j++)
                {
                    tiles[i, j] = new Tile();

                    if (FillWallType != 0)
                    {
                        tiles[i, j].wall = FillWallType;
                        tiles[i, j].wallColor(PaintType);
                    }

                    if (FillTileType != -1)
                    {
                        tiles[i, j].type = (ushort)FillTileType;
                        tiles[i, j].active(true);
                        tiles[i, j].color(PaintType);

                        if (HalfBlock)
                        {
                            tiles[i, j].halfBrick(true);
                        }
                        else if (SlopeType != 0)
                        {
                            tiles[i, j].slope(SlopeType);
                        }
                    }
                }
            }

            return tiles;
        }
    }
}
