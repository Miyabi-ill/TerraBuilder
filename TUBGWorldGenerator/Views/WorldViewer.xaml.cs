namespace TUBGWorldGenerator.Views
{
    using System;
    using System.Collections.Generic;
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
    using TUBGWorldGenerator.Utils;
    using TUBGWorldGenerator.WorldGeneration;

    /// <summary>
    /// Interaction logic for WorldViewer.xaml
    /// </summary>
    public partial class WorldViewer : UserControl
    {
        private WorldSandbox sandbox;

        public WorldSandbox Sandbox
        {
            get => sandbox;
            set
            {
                sandbox = value;
                WorldMapImage.Source = WorldToImage.CreateMapImage(sandbox);
            }
        }

        public WorldViewer()
        {
            InitializeComponent();

            ZoomControl.AnimationCompleted += ZoomControl_AnimationCompleted;
        }

        public void UpdateMap()
        {
            WorldMapImage.Source = WorldToImage.CreateMapImage(Sandbox);
        }

        private void ZoomControl_AnimationCompleted(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Animation Completed");
        }
    }
}
