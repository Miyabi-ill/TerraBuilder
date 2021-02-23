namespace TUBGWorldGenerator
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;
    using TUBGWorldGenerator.WorldGeneration;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, UIElement> globalContextElementDict = new Dictionary<string, UIElement>();

        private Dictionary<string, UIElement> localContextElementDict = new Dictionary<string, UIElement>();

        public MainWindow()
        {
            Sandbox = new WorldSandbox();
            Runner = new WorldGenerationRunner();
            InitializeComponent();
            GlobalContextProperty.SelectedObject = Runner.GlobalContext;
            ActionList.ItemsSource = Runner.WorldGenerationActions;

            UpdateMapView();
            Sandbox.Save(null);
        }

        private WorldSandbox Sandbox { get; }

        private WorldGenerationRunner Runner { get; }

        /// <summary>
        /// マップを更新する。
        /// </summary>
        public void UpdateMapView()
        {
            MapImage.Source = Utils.WorldToImage.CreateMapImage(Sandbox);
        }

        private void Action_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // LocalContextを設定
            if (sender is Grid grid && grid.DataContext is IWorldGenerationAction<ActionContext> generationAction)
            {
                LocalContextProperty.SelectedObject = generationAction.Context;
                LocalContextExpander.Header = generationAction.Name;
            }
        }

        private void RunAllButton_Click(object sender, RoutedEventArgs e)
        {
            RunningOverlay.Visibility = Visibility.Visible;
            Runner.Run(Sandbox);
            UpdateMapView();
            RunningOverlay.Visibility = Visibility.Collapsed;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            RunningOverlay.Visibility = Visibility.Visible;
            Sandbox.Reset();
            UpdateMapView();
            RunningOverlay.Visibility = Visibility.Collapsed;
        }
    }
}
