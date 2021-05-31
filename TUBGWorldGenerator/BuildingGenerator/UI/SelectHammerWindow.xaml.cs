namespace TUBGWorldGenerator.BuildingGenerator.UI
{
    using System.Windows;
    using static TUBGWorldGenerator.BuildingGenerator.UI.BuildingGeneratorWindow;

    /// <summary>
    /// Interaction logic for SelectHammerWindow.xaml
    /// </summary>
    public partial class SelectHammerWindow : Window
    {
        private HammerType selectedHammerType;

        public SelectHammerWindow()
        {
            InitializeComponent();
        }

        public HammerType SelectedHammerType
        {
            get => selectedHammerType;
            set
            {
                selectedHammerType = value;
            }
        }

        private void CycleButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedHammerType = HammerType.Cycle;
            Close();
        }

        private void HalfBrickButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedHammerType = HammerType.HalfBrick;
            Close();
        }

        private void RightBottomSlopeButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedHammerType = HammerType.RightBottom;
            Close();
        }

        private void LeftBottomSlopeButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedHammerType = HammerType.LeftBottom;
            Close();
        }

        private void RightTopSlopeButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedHammerType = HammerType.RightTop;
            Close();
        }

        private void LeftTopSlopeButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedHammerType = HammerType.LeftTop;
            Close();
        }
    }
}
