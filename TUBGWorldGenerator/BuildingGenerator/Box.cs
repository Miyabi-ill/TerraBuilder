namespace TUBGWorldGenerator.BuildingGenerator
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using Newtonsoft.Json;
    using Terraria;

    public class Box : BuildBase, INotifyPropertyChanged
    {
        private string name;
        private Size size;
        private int x;
        private int y;
        private ObservableCollection<BuildBase> childs = new ObservableCollection<BuildBase>();

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        [JsonProperty]
        public string Name
        {
            get => name;
            set
            {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        [JsonProperty]
        public Size Size
        {
            get => size;
            set
            {
                size = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Size)));
            }
        }

        [JsonProperty]
        public ObservableCollection<BuildBase> Childs
        {
            get => childs;
            set
            {
                childs = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Childs)));
            }
        }

        /// <inheritdoc/>
        [JsonProperty]
        public override int X
        {
            get => x;
            set
            {
                x = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(X)));
            }
        }

        /// <inheritdoc/>
        [JsonProperty]
        public override int Y
        {
            get => y;
            set
            {
                y = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Y)));
            }
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
                }
            }

            foreach (BuildBase child in Childs)
            {
                Tile[,] builds = child.Build();
                int h = builds.GetLength(1);
                int startY = Size.Height - (h + child.Y - 1);
                for (int i = 0; i < builds.GetLength(0); i++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        if (i + child.X - 1 < 0
                            || i + child.X - 1 >= tiles.GetLength(0)
                            || startY + j < 0
                            || startY + j >= tiles.GetLength(1))
                        {
                            continue;
                        }

                        if (tiles[i + child.X - 1, startY + j] == null)
                        {
                            tiles[i + child.X - 1, startY + j] = new Tile();
                        }

                        MergeTileNotOverwrite(tiles[i + child.X - 1, startY + j], builds[i, j]);
                    }
                }
            }

            return tiles;
        }

        private static Tile MergeTileNotOverwrite(Tile to, Tile from)
        {
            if (!to.active() && from.active())
            {
                to.type = from.type;
                to.active(true);
                to.frameX = from.frameX;
                to.frameY = from.frameY;
                to.halfBrick(from.halfBrick());
                to.slope(from.slope());

                if (to.color() == 0)
                {
                    to.color(from.color());
                }
            }

            if (to.liquid == 0 && from.liquid > 0)
            {
                to.liquid = from.liquid;
                to.liquidType(from.liquidType());
            }

            if (to.wall == 0 && from.wall > 0)
            {
                to.wall = from.wall;

                if (to.wallColor() == 0)
                {
                    to.wallColor(from.wallColor());
                }
            }

            return to;
        }
    }
}
