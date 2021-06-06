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
    using TUBGWorldGenerator.BuildingGenerator;
    using TUBGWorldGenerator.BuildingGenerator.UI;
    using TUBGWorldGenerator.Utils;
    using TUBGWorldGenerator.WorldGeneration;

    /// <summary>
    /// Interaction logic for WorldViewer.xaml
    /// </summary>
    public partial class TileEditor : UserControl
    {
        private Tile[,] viewTiles;
        private WorldSandbox sandbox;

        /// <summary>
        /// 背景をマップ画像にするか、それとも全体をタイル画像にするか
        /// </summary>
        public bool UseMapImageAsBackground
        {
            get
            {
                if (ViewTiles == null && Sandbox != null)
                {
                    return true;
                }

                return Math.Max(ViewTiles.GetLength(0), ViewTiles.GetLength(1)) <= 100;
            }
        }

        public Tile[,] ViewTiles
        {
            get => viewTiles;
            set
            {
                viewTiles = value;
                if (UseMapImageAsBackground)
                {
                    WorldMapImage.Source = WorldToImage.CreateMapImage(viewTiles);
                }
                else
                {
                    WorldMapImage.Source = TileToImage.CreateBitmap(viewTiles);
                }
            }
        }

        public Tile[,] ToolTile { get; set; } = new Tile[0, 0];

        private ToolState CurrentToolState { get; set; }

        private HammerType CurrentHammerType { get; set; }

        private string CurrentPaintName { get; set; } = "RedPaint";

        public WorldSandbox Sandbox
        {
            get => sandbox;
            set
            {
                sandbox = value;
                WorldMapImage.Source = WorldToImage.CreateMapImage(sandbox);
            }
        }

        public TileEditor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ハンマー形状。
        /// </summary>
        public enum HammerType
        {
            Cycle,
            HalfBrick,
            RightBottom,
            LeftBottom,
            RightTop,
            LeftTop,
        }

        [Flags]
        private enum ToolState
        {
            PlaceTile = 0x1,
            Hammer = 0x2,
            Eraser = 0x4,
            Paint = 0x8,
            SwitchInactive = 0x10,
        }

        /// <summary>
        /// マップ画像をアップデートする。タイルに変更を加えた時に呼ぶ。
        /// </summary>
        public void UpdateMap()
        {
            if (viewTiles != null)
            {
                if (UseMapImageAsBackground)
                {
                    WorldMapImage.Source = WorldToImage.CreateMapImage(viewTiles);
                }
                else
                {
                    WorldMapImage.Source = TileToImage.CreateBitmap(viewTiles);
                }
            }
            else
            {
                WorldMapImage.Source = WorldToImage.CreateMapImage(Sandbox);
            }
        }

        private void UpdateGrid()
        {
            TileGrid.ColumnDefinitions.Clear();
            TileGrid.RowDefinitions.Clear();

            int columnCount = GetTileCountX();
            int rowCount = GetTileCountY();

            for (int i = 0; i < columnCount; i++)
            {
                ColumnDefinition columnDefinition = new ColumnDefinition();
                TileGrid.ColumnDefinitions.Add(columnDefinition);
            }

            for (int i = 0; i < rowCount; i++)
            {
                RowDefinition rowDefinition = new RowDefinition();
                TileGrid.RowDefinitions.Add(rowDefinition);
            }
        }

        private Tile GetTile(int x, int y)
        {
            if (ViewTiles == null)
            {
                return (Tile)Sandbox.Tiles[x, y];
            }
            else
            {
                return ViewTiles[x, y];
            }
        }

        private void SetTile(int x, int y, Tile tile)
        {
            if (ViewTiles == null)
            {
                Sandbox.Tiles[x, y] = tile;
            }
            else
            {
                ViewTiles[x, y] = tile;
            }
        }

        private int GetTileCountX()
        {
            if (ViewTiles == null)
            {
                return Sandbox.TileCountX;
            }
            else
            {
                return ViewTiles.GetLength(0);
            }
        }

        private int GetTileCountY()
        {
            if (ViewTiles == null)
            {
                return Sandbox.TileCountY;
            }
            else
            {
                return ViewTiles.GetLength(1);
            }
        }

        private void ModifyTile(int tileX, int tileY, bool leftClick, bool rightClick, bool isDragging)
        {
            if (GetTileCountX() <= tileX || GetTileCountY() <= tileY)
            {
                return;
            }

            if (GetTile(tileX, tileY) == null)
            {
                SetTile(tileX, tileY, new Tile());
            }

            Tile tile = GetTile(tileX, tileY);
            if (leftClick)
            {
                if (CurrentToolState.HasFlag(ToolState.Eraser))
                {
                    if (CurrentToolState.HasFlag(ToolState.PlaceTile))
                    {
                        tile.active(false);
                        tile.type = 0;
                        tile.color(0);
                        tile.inActive(false);
                        tile.halfBrick(false);
                        tile.slope(0);
                        tile.frameX = 0;
                        tile.frameY = 0;
                    }

                    if (CurrentToolState.HasFlag(ToolState.Hammer))
                    {
                        tile.halfBrick(false);
                        tile.slope(0);
                    }

                    if (CurrentToolState.HasFlag(ToolState.Paint))
                    {
                        tile.color(0);
                    }

                    if (CurrentToolState.HasFlag(ToolState.SwitchInactive))
                    {
                        tile.inActive(false);
                    }
                }
                else
                {
                    if (CurrentToolState.HasFlag(ToolState.PlaceTile)
                        && ToolTile != null)
                    {
                        int width = ToolTile.GetLength(0);
                        int height = ToolTile.GetLength(1);

                        for (int x = tileX; x < GetTileCountX() && x < tileX + width; x++)
                        {
                            for (int y = tileY; y < GetTileCountY() && y < tileY + height; y++)
                            {
                                Tile toPlace;
                                if (GetTile(x, y) == null)
                                {
                                    toPlace = new Tile();
                                    SetTile(x, y, toPlace);
                                }
                                else
                                {
                                    toPlace = GetTile(x, y);
                                }

                                toPlace.active(ToolTile[x - tileX, y - tileY].active());
                                toPlace.type = ToolTile[x - tileX, y - tileY].type;
                                toPlace.frameX = ToolTile[x - tileX, y - tileY].frameX;
                                toPlace.frameY = ToolTile[x - tileX, y - tileY].frameY;
                                toPlace.halfBrick(ToolTile[x - tileX, y - tileY].halfBrick());
                                toPlace.slope(ToolTile[x - tileX, y - tileY].slope());
                                toPlace.color(ToolTile[x - tileX, y - tileY].color());
                                toPlace.inActive(ToolTile[x - tileX, y - tileY].inActive());
                            }
                        }
                    }

                    if (CurrentToolState.HasFlag(ToolState.Hammer))
                    {
                        if (tile.active())
                        {
                            if (CurrentHammerType == HammerType.Cycle)
                            {
                                if (tile.slope() == 0)
                                {
                                    if (tile.halfBrick())
                                    {
                                        tile.halfBrick(false);
                                        tile.slope(1);
                                    }
                                    else
                                    {
                                        tile.halfBrick(true);
                                    }
                                }
                                else
                                {
                                    int slope = tile.slope();
                                    if (slope + 1 >= 5)
                                    {
                                        tile.slope(0);
                                        tile.halfBrick(false);
                                    }
                                    else
                                    {
                                        tile.slope((byte)(slope + 1));
                                    }
                                }
                            }
                            else
                            {
                                switch (CurrentHammerType)
                                {
                                    case HammerType.HalfBrick:
                                        tile.slope(0);
                                        tile.halfBrick(true);
                                        break;
                                    case HammerType.RightBottom:
                                        tile.halfBrick(false);
                                        tile.slope(2);
                                        break;
                                    case HammerType.RightTop:
                                        tile.halfBrick(false);
                                        tile.slope(4);
                                        break;
                                    case HammerType.LeftBottom:
                                        tile.halfBrick(false);
                                        tile.slope(1);
                                        break;
                                    case HammerType.LeftTop:
                                        tile.halfBrick(false);
                                        tile.slope(3);
                                        break;
                                }
                            }
                        }
                    }

                    if (CurrentToolState.HasFlag(ToolState.Paint))
                    {
                        if (tile.active())
                        {
                            if (TerrariaNameDict.PaintNameToID.ContainsKey(CurrentPaintName))
                            {
                                tile.color(TerrariaNameDict.PaintNameToID[CurrentPaintName]);
                            }
                        }
                    }

                    if (CurrentToolState.HasFlag(ToolState.SwitchInactive))
                    {
                        if (tile.active())
                        {
                            tile.inActive(true);
                        }
                    }
                }
            }

            if (rightClick)
            {
                if (CurrentToolState.HasFlag(ToolState.Eraser))
                {
                    if (CurrentToolState.HasFlag(ToolState.PlaceTile))
                    {
                        tile.wall = 0;
                        tile.wallColor(0);
                        tile.wallFrameX(0);
                        tile.wallFrameY(0);
                    }

                    if (CurrentToolState.HasFlag(ToolState.Paint))
                    {
                        tile.wallColor(0);
                    }
                }
                else
                {
                    if (CurrentToolState.HasFlag(ToolState.PlaceTile)
                        && ToolTile != null)
                    {
                        int width = ToolTile.GetLength(0);
                        int height = ToolTile.GetLength(1);

                        for (int x = tileX; x < GetTileCountX() && x < tileX + width; x++)
                        {
                            for (int y = tileY; y < GetTileCountY() && y < tileY + height; y++)
                            {
                                Tile toPlace;
                                if (GetTile(x, y) == null)
                                {
                                    toPlace = new Tile();
                                    SetTile(x, y, toPlace);
                                }
                                else
                                {
                                    toPlace = GetTile(x, y);
                                }

                                toPlace.wall = ToolTile[x - tileX, y - tileY].wall;
                                toPlace.wallColor(ToolTile[x - tileX, y - tileY].wallColor());
                                toPlace.wallFrameX(ToolTile[x - tileX, y - tileY].wallFrameX());
                                toPlace.wallFrameY(ToolTile[x - tileX, y - tileY].wallFrameY());
                            }
                        }
                    }

                    if (CurrentToolState.HasFlag(ToolState.Paint))
                    {
                        if (TerrariaNameDict.PaintNameToID.ContainsKey(CurrentPaintName))
                        {
                            tile.wallColor(TerrariaNameDict.PaintNameToID[CurrentPaintName]);
                        }
                    }
                }
            }

            UpdateMap();
        }

        private void TileGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point point = Mouse.GetPosition(TileGrid);

            int columnCount = TileGrid.ColumnDefinitions.Count;
            int rowCount = TileGrid.RowDefinitions.Count;

            double width = TileGrid.ActualWidth;
            double height = TileGrid.ActualHeight;

            int positionX = (int)(point.X / width * columnCount) + 1;
            int positionY = rowCount - (int)(point.Y / height * rowCount) + 1;

            ModifyTile(positionX, positionY, e.LeftButton == MouseButtonState.Pressed, e.RightButton == MouseButtonState.Pressed, true);
        }

        private void TileGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point point = Mouse.GetPosition(TileGrid);

            int columnCount = TileGrid.ColumnDefinitions.Count;
            int rowCount = TileGrid.RowDefinitions.Count;

            double width = TileGrid.ActualWidth;
            double height = TileGrid.ActualHeight;

            int positionX = (int)(point.X / width * columnCount);
            int positionY = (int)(point.Y / height * rowCount);

            ModifyTile(positionX, positionY, e.LeftButton == MouseButtonState.Pressed, e.RightButton == MouseButtonState.Pressed, false);
        }

        private void TileButton_Checked(object sender, RoutedEventArgs e)
        {
            CurrentToolState |= ToolState.PlaceTile;
        }

        private void TileButton_Unchecked(object sender, RoutedEventArgs e)
        {
            CurrentToolState &= ~ToolState.PlaceTile;
        }

        private void HammerButton_Checked(object sender, RoutedEventArgs e)
        {
            CurrentToolState |= ToolState.Hammer;
        }

        private void HammerButton_Unchecked(object sender, RoutedEventArgs e)
        {
            CurrentToolState &= ~ToolState.Hammer;
        }

        private void EraserButton_Checked(object sender, RoutedEventArgs e)
        {
            CurrentToolState |= ToolState.Eraser;
        }

        private void EraserButton_Unchecked(object sender, RoutedEventArgs e)
        {
            CurrentToolState &= ~ToolState.Eraser;
        }

        private void PaintButton_Checked(object sender, RoutedEventArgs e)
        {
            CurrentToolState |= ToolState.Paint;
        }

        private void PaintButton_Unchecked(object sender, RoutedEventArgs e)
        {
            CurrentToolState &= ~ToolState.Paint;
        }

        private void InactiveButton_Checked(object sender, RoutedEventArgs e)
        {
            CurrentToolState |= ToolState.SwitchInactive;
        }

        private void InactiveButton_Unchecked(object sender, RoutedEventArgs e)
        {
            CurrentToolState &= ~ToolState.SwitchInactive;
        }

        private void HammerButton_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var window = new SelectHammerWindow() { SelectedHammerType = CurrentHammerType };
            window.ShowDialog();
            CurrentHammerType = window.SelectedHammerType;
        }

        private void PaintButton_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var window = new SelectPaintWindow();
            Point point = Mouse.GetPosition(this);
            window.ShowDialog();
            if (!string.IsNullOrEmpty(window.SelectedPaintName))
            {
                CurrentPaintName = window.SelectedPaintName;
            }
        }

        private void ZoomControl_CurrentViewChanged(object sender, Xceed.Wpf.Toolkit.Zoombox.ZoomboxViewChangedEventArgs e)
        {
            if (e.NewViewStackIndex != -1 && UseMapImageAsBackground)
            {
                int minSize = (int)Math.Min(ZoomControl.Viewport.Width, ZoomControl.Viewport.Height);
                if (minSize <= 50)
                {
                    if (ViewTiles != null)
                    {
                        int minX = ZoomControl.Viewport.Left - 2 < 0 ? 0 : (int)ZoomControl.Viewport.Left - 2;
                        int minY = ZoomControl.Viewport.Top - 2 < 0 ? 0 : (int)ZoomControl.Viewport.Top - 2;
                        int maxX = ZoomControl.Viewport.Right + 2 > ViewTiles.GetLength(0) ? ViewTiles.GetLength(0) : (int)ZoomControl.Viewport.Right + 2;
                        int maxY = ZoomControl.Viewport.Bottom + 2 > ViewTiles.GetLength(1) ? ViewTiles.GetLength(1) : (int)ZoomControl.Viewport.Bottom + 2;

                        Tile[,] tiles = new Tile[maxX - minX, maxY - minY];
                        for (int x = minX; x < maxX; x++)
                        {
                            for (int y = minY; y < maxY; y++)
                            {
                                tiles[x - minX, y - minY] = ViewTiles[x, y];
                            }
                        }

                        DetailImage.Source = TileToImage.CreateBitmap(tiles);
                        DetailImage.Width = maxX - minX;
                        DetailImage.Height = maxY - minY;

                        DetailImage.SetValue(Canvas.TopProperty, (double)minY);
                        DetailImage.SetValue(Canvas.LeftProperty, (double)minX);
                    }
                    else
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

                        DetailImage.Source = TileToImage.CreateBitmap(tiles);
                        DetailImage.Width = maxX - minX;
                        DetailImage.Height = maxY - minY;

                        DetailImage.SetValue(Canvas.TopProperty, (double)minY);
                        DetailImage.SetValue(Canvas.LeftProperty, (double)minX);
                    }
                }
                else
                {
                    DetailImage.Source = null;
                }
            }
        }
    }
}
