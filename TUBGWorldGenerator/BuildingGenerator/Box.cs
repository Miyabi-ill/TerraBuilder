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
                int startY = Size.Height - (h + child.Y);
                for (int i = 0; i < builds.GetLength(0); i++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        tiles[i + child.X, startY + j] = builds[i, j];
                    }
                }
            }

            return tiles;
        }
    }
}
