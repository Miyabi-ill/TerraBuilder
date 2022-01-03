namespace TerraBuilder.BuildingGenerator.Parts
{
    using System;
    using Newtonsoft.Json;
    using Terraria;

    [JsonConverter(typeof(PartsConverter))]
    public class Repeat : BuildBase
    {
        private RandomValue repeatX = new ConstantValue(1);
        private RandomValue repeatY = new ConstantValue(1);

        private RandomValue spaceX;
        private RandomValue spaceY;

        public RandomValue RepeatX
        {
            get => repeatX;
            set
            {
                repeatX = value;
                RaisePropertyChanged(nameof(RepeatX));
            }
        }

        public RandomValue RepeatY
        {
            get => repeatY;
            set
            {
                repeatY = value;
                RaisePropertyChanged(nameof(RepeatY));
            }
        }

        public RandomValue SpaceX
        {
            get => spaceX;
            set
            {
                spaceX = value;
                RaisePropertyChanged(nameof(SpaceX));
            }
        }

        public RandomValue SpaceY
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
        public override Tile[,] Build(Random rand)
        {
            Tile[,] build = Building?.Build(rand);
            int repeatX = (int)RepeatX.GetValue(rand);
            int repeatY = (int)RepeatY.GetValue(rand);
            if (build == null
                || build.GetLength(0) == 0
                || build.GetLength(1) == 0
                || repeatX == 0
                || repeatY == 0)
            {
                return new Tile[0, 0];
            }

            int width = SpaceX != null ? (int)SpaceX.GetValue(rand) : build.GetLength(0);
            int height = SpaceY != null ? (int)SpaceY.GetValue(rand) : build.GetLength(1);
            Tile[,] tiles = new Tile[(width * (repeatX - 1)) + build.GetLength(0), (height * (repeatY - 1)) + build.GetLength(1)];
            for (int i = 0; i < repeatX; i++)
            {
                for (int j = 0; j < repeatY; j++)
                {
                    for (int x = 0; x < build.GetLength(0); x++)
                    {
                        if (i != repeatX - 1 && x >= width)
                        {
                            break;
                        }

                        for (int y = 0; y < build.GetLength(1); y++)
                        {
                            if (j != repeatY - 1 && y >= height)
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
