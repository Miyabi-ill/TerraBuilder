namespace TerraBuilder.BuildingGenerator.UI
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

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
                            window.TileEditor.ViewTiles = BuildingCache.GetTilesFromSearchResult(searchResult);
                            window.BuildingMetaData.Name = searchResult.Name;
                            window.BuildingMetaData.Tags = new ObservableCollection<string>(searchResult.Tags);
                            window.BuildingMetaData.Size = new TerraBuilder.BuildingGenerator.Size(window.TileEditor.ViewTiles.GetLength(0), window.TileEditor.ViewTiles.GetLength(1));
                            window.Show();
                            break;
                        }
                }
            }
        }
    }
}
