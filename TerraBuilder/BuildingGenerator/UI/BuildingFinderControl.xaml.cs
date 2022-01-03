namespace TerraBuilder.BuildingGenerator.UI
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using Terraria;

    /// <summary>
    /// Interaction logic for BuildingFinderControl.xaml
    /// </summary>
    public partial class BuildingFinderControl : UserControl, INotifyPropertyChanged
    {
        private BuildingCache buildingCache;
        private SearchResult selectingResult;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        public BuildingCache BuildingCache
        {
            get => buildingCache;
            set
            {
                buildingCache = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BuildingCache)));
            }
        }

        public SearchResult SelectingResult
        {
            get => selectingResult;
            set
            {
                selectingResult = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectingResult)));
            }
        }

        public BuildingFinderControl()
        {
            InitializeComponent();
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string searchText = textBox.Text.Trim();
                var searchResults = await BuildingCache.Search(searchText);
                SearchResult.Dispatcher.Invoke(() => SearchResult.ItemsSource = searchResults);
            }
        }

        private void SearchResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResult.SelectedItem is SearchResult searchResult)
            {
                SelectingResult = searchResult;
            }
            else if (SearchResult.SelectedItem == null)
            {
                SelectingResult = null;
            }
        }

        private void FavoriteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SearchResult.SelectedItem is SearchResult searchResult)
            {
                BuildingCache.Favorites.SetFavorite(searchResult.OriginalName, searchResult.IsFavorite);
                BuildingCache.SaveFavorites();
            }
        }

        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SearchResult.SelectedItem is SearchResult searchResult)
            {
                if (!searchResult.IsEditable)
                {
                    return;
                }

                var window = new BuildingGeneratorWindow();
                window.TileEditor.ViewTiles = BuildingCache.GetTilesFromSearchResult(searchResult);
                window.BuildingMetaData.Name = searchResult.Name;
                window.BuildingMetaData.Tags = new ObservableCollection<string>(searchResult.Tags);
                window.BuildingMetaData.Size = new TerraBuilder.BuildingGenerator.Size(window.TileEditor.ViewTiles.GetLength(0), window.TileEditor.ViewTiles.GetLength(1));
                window.Show();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item
                && SearchResult.SelectedItem is SearchResult searchResult)
            {
                switch (item.Header)
                {
                    case "お気に入り":
                        BuildingCache.Favorites.SetFavorite(searchResult.OriginalName, searchResult.IsFavorite);
                        BuildingCache.SaveFavorites();
                        break;
                    case "編集":
                        {
                            if (!searchResult.IsEditable)
                            {
                                return;
                            }

                            var window = new BuildingGeneratorWindow();
                            window.TileEditor.ViewTiles = CloneTile(BuildingCache.GetTilesFromSearchResult(searchResult));
                            window.BuildingMetaData.Name = searchResult.Name;
                            window.BuildingMetaData.Tags = new ObservableCollection<string>(searchResult.Tags);
                            window.BuildingMetaData.Size = new TerraBuilder.BuildingGenerator.Size(window.TileEditor.ViewTiles.GetLength(0), window.TileEditor.ViewTiles.GetLength(1));
                            window.Show();
                            break;
                        }
                }
            }
        }

        private static Tile[,] CloneTile(Tile[,] from)
        {
            Tile[,] tiles = new Tile[from.GetLength(0), from.GetLength(1)];
            for (int x = 0; x < from.GetLength(0); x++)
            {
                for (int y = 0; y < from.GetLength(1); y++)
                {
                    tiles[x, y] = new Tile();
                    if (from[x, y] != null)
                    {
                        tiles[x, y].CopyFrom(from[x, y]);
                    }
                }
            }

            return tiles;
        }
    }
}
