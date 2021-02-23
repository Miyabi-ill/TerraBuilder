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
            var runner = new WorldGenerationRunner();
            runner.Run(Sandbox);
            UpdateMapView();
            Sandbox.Save(null);
        }

        /// <summary>
        /// マップを更新する。
        /// </summary>
        public void UpdateMapView()
        {
            MapImage.Source = Utils.WorldToImage.CreateMapImage(Sandbox);
        }
    }
}
