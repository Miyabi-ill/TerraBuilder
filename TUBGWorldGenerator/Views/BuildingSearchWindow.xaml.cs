namespace TUBGWorldGenerator.Views
{
    using System.ComponentModel;
    using System.Windows;
    using Terraria;
    using TUBGWorldGenerator.BuildingGenerator;

    /// <summary>
    /// Interaction logic for BuildingSearchWindow.xaml
    /// </summary>
    public partial class BuildingSearchWindow : Window, INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        public SearchResult SelectedResult
        {
            get => BuildingFinder.SelectingResult;
        }

        public BuildingSearchWindow(BuildingCache buildingCache)
        {
            InitializeComponent();

            BuildingFinder.BuildingCache = buildingCache;
            BuildingFinder.PropertyChanged += BuildingFinder_PropertyChanged;
        }

        private void BuildingFinder_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
           if (e.PropertyName == nameof(BuildingFinder.SelectingResult))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedResult)));
            }
        }

        public Tile[,] GetSelectedBuildTiles()
        {
            if (SelectedResult == null)
            {
                return new Tile[0, 0];
            }

            return BuildingFinder.BuildingCache.GetTilesFromSearchResult(SelectedResult);
        }
    }
}
