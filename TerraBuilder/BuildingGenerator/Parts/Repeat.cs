namespace TerraBuilder.BuildingGenerator.Parts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Terraria;

    public class Repeat : BuildBase
    {
        private int repeatX = 1;
        private int repeatY = 1;

        private int? spaceX;
        private int? spaceY;

        public int RepeatX
        {
            get => repeatX;
            set
            {
                repeatX = value;
                RaisePropertyChanged(nameof(RepeatX));
            }
        }

        public int RepeatY
        {
            get => repeatY;
            set
            {
                repeatY = value;
                RaisePropertyChanged(nameof(RepeatY));
            }
        }

        public int? SpaceX
        {
            get => spaceX;
            set
            {
                spaceX = value;
                RaisePropertyChanged(nameof(SpaceX));
            }
        }

        public int? SpaceY
        {
            get => spaceY;
            set
            {
                spaceY = value;
                RaisePropertyChanged(nameof(SpaceY));
            }
        }

        public BuildBase Building { get; set; }

        /// <inheritdoc/>
        public override Tile[,] Build()
        {
            Tile[,] build = Building?.Build();
            if (build == null
                || build.GetLength(0) == 0
                || build.GetLength(1) == 0
                || RepeatX == 0
                || RepeatY == 0)
            {
                return new Tile[0, 0];
            }

            int width = SpaceX.HasValue ? SpaceX.Value : build.GetLength(0);
            int height = SpaceY.HasValue ? SpaceY.Value : build.GetLength(1);
            Tile[,] tiles = new Tile[(width * (RepeatX - 1)) + build.GetLength(0), (height * (RepeatY - 1)) + build.GetLength(1)];
            for (int i = 0; i < RepeatX; i++)
            {
                for (int j = 0; j < RepeatY; j++)
                {
                    for (int x = 0; x < build.GetLength(0); x++)
                    {
                        if (i != RepeatX - 1 && x >= width)
                        {
                            break;
                        }

                        for (int y = 0; y < build.GetLength(1); y++)
                        {
                            if (j != RepeatY - 1 && y >= height)
                            {
                                break;
                            }

                            Tile tile = new Tile();
                            tile.CopyFrom(build[x, y]);
                            tiles[(i * width) + x, (j * height) + y] = tile;
                        }
                    }
                }
            }

            for (int i = 0; i < tiles.GetLength(0); i++)
            {
                for (int j = 0; j < tiles.GetLength(1); j++)
                {
                    if (tiles[i, j] == null)
                    {
                        tiles[i, j] = new Tile();
                    }
                }
            }

            return tiles;
        }
    }
}
