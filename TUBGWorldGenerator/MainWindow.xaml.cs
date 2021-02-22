namespace TUBGWorldGenerator
{
    using System.Windows;
    using System.Windows.Media.Imaging;
    using TUBGWorldGenerator.WorldGeneration;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WorldSandbox Sandbox { get; }

        public MainWindow()
        {
            InitializeComponent();
            Sandbox = new WorldSandbox();
            UpdateMapView();
            var runner = new WorldGenerationRunner();
            //runner.Run();
        }

        /// <summary>
        /// マップを更新する。
        /// </summary>
        public void UpdateMapView()
        {
            BitmapImage image = Utils.WorldToImage.CreateMapImage(Sandbox);
            MapImage.Source = image;
        }
    }
}
