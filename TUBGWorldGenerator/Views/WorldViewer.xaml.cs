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
    using Terraria;
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
        }

        public void UpdateMap()
        {
            WorldMapImage.Source = WorldToImage.CreateMapImage(Sandbox);
        }

        private void ZoomControl_CurrentViewChanged(object sender, Xceed.Wpf.Toolkit.Zoombox.ZoomboxViewChangedEventArgs e)
        {
            if (e.NewViewStackIndex != -1)
            {
                int minSize = (int)Math.Min(ZoomControl.Viewport.Width, ZoomControl.Viewport.Height);
                if (minSize <= 50)
                {
                    int minX = ZoomControl.Viewport.Left - 2 < 0 ? 0 : (int)ZoomControl.Viewport.Left - 2;
                    int minY = ZoomControl.Viewport.Top - 2 < 0 ? 0 : (int)ZoomControl.Viewport.Top - 2;
                    int maxX = ZoomControl.Viewport.Right + 2 > Sandbox.TileCountX ? Sandbox.TileCountX : (int)ZoomControl.Viewport.Right + 2;
                    int maxY = ZoomControl.Viewport.Bottom + 2 > Sandbox.TileCountY ? Sandbox.TileCountY : (int)ZoomControl.Viewport.Bottom + 2;

                    Tile[,] tiles = new Tile[maxX - minX, maxY - minY];
                    for (int x = minX; x < maxX; x++)
                    {
                        for (int y = minY; y < maxY; y++)
                        {
                            tiles[x - minX, y - minY] = (Tile)Sandbox.Tiles[x, y];
                        }
                    }

                    DetailImage.Source = BuildingGenerator.UI.TileToImage.CreateBitmap(tiles);
                    DetailImage.Width = maxX - minX;
                    DetailImage.Height = maxY - minY;
                    DetailImage.SetValue(Canvas.TopProperty, (double)minY);
                    DetailImage.SetValue(Canvas.LeftProperty, (double)minX);
                }
                else
                {
                    DetailImage.Source = null;
                }
            }
        }
    }
}
