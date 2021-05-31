namespace TUBGWorldGenerator.BuildingGenerator.UI
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Microsoft.Win32;
    using Microsoft.WindowsAPICodePack.Dialogs;
    using Newtonsoft.Json;
    using Terraria;
    using TUBGWorldGenerator.BuildingGenerator.Parts;

    /// <summary>
    /// Interaction logic for BuildingGeneratorWindow.xaml
    /// </summary>
    public partial class BuildingGeneratorWindow : Window, INotifyPropertyChanged
    {
        private string fileName;
        private string jsonText;
        private BuildingGenerator buildingGenerator;
        private ObservableCollection<BuildNode> buildNodes = new ObservableCollection<BuildNode>();

        public BuildingGeneratorWindow()
        {
            InitializeComponent();

            BuildingGenerator = new BuildingGenerator()
            {
                BuildingsRootPath = Configs.LastBuildingsPath,
            };

            BuildingFinder.BuildingCache = new BuildingCache(new BuildingGenerator() { BuildingsRootPath = Configs.LastBuildingsPath });
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        [Flags]
        private enum ToolState
        {
            PlaceTile = 0x1,
            Hammer = 0x2,
            Eraser = 0x4,
            Paint = 0x8,
            SwitchInactive = 0x10,
        }

        public enum HammerType
        {
            Cycle,
            HalfBrick,
            RightBottom,
            LeftBottom,
            RightTop,
            LeftTop,
        }

        public string FileName
        {
            get => fileName;
            set
            {
                fileName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileName)));
            }
        }

        public string JsonText
        {
            get => jsonText;
            set
            {
                jsonText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JsonText)));
                ParseJson();
            }
        }

        private ToolState CurrentToolState { get; set; }

        private HammerType CurrentHammerType { get; set; }

        private string CurrentPaintName { get; set; } = "RedPaint";

        private Tile[,] Tiles { get; set; }

        public BuildingGenerator BuildingGenerator
        {
            get => buildingGenerator;
            private set
            {
                buildingGenerator = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BuildingGenerator)));
            }
        }

        public ObservableCollection<BuildNode> Builds
        {
            get => buildNodes;
            set
            {
                buildNodes = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Builds)));
            }
        }

        private void ModifyTile(int tileX, int tileY, bool leftClick, bool rightClick)
        {
            if (Tiles.GetLength(0) <= tileX || Tiles.GetLength(1) <= tileY)
            {
                return;
            }

            if (Tiles[tileX, tileY] == null)
            {
                Tiles[tileX, tileY] = new Tile();
            }

            Tile tile = Tiles[tileX, tileY];
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
                        && BuildingFinder.SelectingResult != null)
                    {
                        Tile[,] toPlaces = BuildingFinder.BuildingCache.GetTilesFromSearchResult(BuildingFinder.SelectingResult);
                        int width = toPlaces.GetLength(0);
                        int height = toPlaces.GetLength(1);

                        for (int x = tileX; x < Tiles.GetLength(0) && x < tileX + width; x++)
                        {
                            for (int y = tileY; y < Tiles.GetLength(1) && y < tileY + height; y++)
                            {
                                Tile toPlace;
                                if (Tiles[x, y] == null)
                                {
                                    toPlace = new Tile();
                                    Tiles[x, y] = toPlace;
                                }
                                else
                                {
                                    toPlace = Tiles[x, y];
                                }

                                toPlace.active(toPlaces[x - tileX, y - tileY].active());
                                toPlace.type = toPlaces[x - tileX, y - tileY].type;
                                toPlace.frameX = toPlaces[x - tileX, y - tileY].frameX;
                                toPlace.frameY = toPlaces[x - tileX, y - tileY].frameY;
                                toPlace.color(0);
                                toPlace.inActive(false);
                                toPlace.halfBrick(false);
                                toPlace.slope(0);
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
                        && BuildingFinder.SelectingResult != null)
                    {
                        Tile[,] toPlaces = BuildingFinder.BuildingCache.GetTilesFromSearchResult(BuildingFinder.SelectingResult);
                        int width = toPlaces.GetLength(0);
                        int height = toPlaces.GetLength(1);

                        for (int x = tileX; x < Tiles.GetLength(0) && x < tileX + width; x++)
                        {
                            for (int y = tileY; y < Tiles.GetLength(1) && y < tileY + height; y++)
                            {
                                Tile toPlace;
                                if (Tiles[x, y] == null)
                                {
                                    toPlace = new Tile();
                                    Tiles[x, y] = toPlace;
                                }
                                else
                                {
                                    toPlace = Tiles[x, y];
                                }

                                toPlace.wall = toPlaces[x - tileX, y - tileY].wall;
                                toPlace.wallColor(toPlaces[x - tileX, y - tileY].wallColor());
                                toPlace.wallFrameX(toPlaces[x - tileX, y - tileY].wallFrameX());
                                toPlace.wallFrameY(toPlaces[x - tileX, y - tileY].wallFrameY());
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

            PreviewImage.Source = TileToImage.CreateBitmap(Tiles);
        }

        private void UpdateGrid()
        {
            TileGrid.ColumnDefinitions.Clear();
            TileGrid.RowDefinitions.Clear();

            int columnCount = (int)Math.Round(PreviewImage.Source.Width) / 16;
            int rowCount = (int)Math.Round(PreviewImage.Source.Height) / 16;

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

        private void ParseJson()
        {
            BuildingGenerator.ImportJson(JsonText);
            BuildingGenerator.Build();
            Builds.Clear();
            if (BuildingGenerator.Root != null)
            {
                BuildNode node = new BuildNode(BuildingGenerator.Root);
                foreach (var build in node.Child)
                {
                    Builds.Add(build);
                }
            }

            if (BuildingGenerator.Result != null)
            {
                GenerateFailedOverlay.Visibility = Visibility.Hidden;
                Tiles = BuildingGenerator.Result;
                PreviewImage.Source = TileToImage.CreateBitmap(BuildingGenerator.Result);
                UpdateGrid();
            }
            else
            {
                GenerateFailedOverlay.Visibility = Visibility.Visible;
            }
        }

        private void ReadButton_Click(object sender, RoutedEventArgs e)
        {
            var fileSelectDialog = new OpenFileDialog()
            {
                Filter = "Jsonファイル(*.json)|*.json|すべてのファイル(*.*)|*.*",
                RestoreDirectory = true,
            };

            if (fileSelectDialog.ShowDialog() == true)
            {
                if (FileName != fileSelectDialog.FileName)
                {
                    FileName = fileSelectDialog.FileName;
                }
            }
        }

        private void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog()
            {
                Filter = "Jsonファイル(*.json)|*.json|すべてのファイル(*.*)|*.*",
                RestoreDirectory = true,
                FileName = "Building.json",
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                FileName = saveFileDialog.FileName;
                using (var sw = new StreamWriter(FileName))
                {
                    sw.Write(jsonText);
                }
            }
        }

        private void InputTextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            jsonText = ((TextBox)sender).Text;
            ParseJson();
        }

        private void OverwriteWithTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            Box root = new Box()
            {
                Size = new TUBGWorldGenerator.BuildingGenerator.Size() { Height = 6, Width = 8 },
                Name = "Basic-House",
            };

            Rectangle leftWall = new Rectangle()
            {
                X = 1,
                Y = 5,
                FillTile = nameof(Terraria.ID.TileID.WoodBlock),
                Size = new TUBGWorldGenerator.BuildingGenerator.Size() { Height = 2, Width = 1 },
            };

            Rectangle topWall = new Rectangle()
            {
                X = 1,
                Y = 6,
                FillTile = nameof(Terraria.ID.TileID.WoodBlock),
                Size = new TUBGWorldGenerator.BuildingGenerator.Size() { Height = 1, Width = 8 },
            };

            Rectangle rightWall = new Rectangle()
            {
                X = 8,
                Y = 5,
                FillTile = nameof(Terraria.ID.TileID.WoodBlock),
                Size = new TUBGWorldGenerator.BuildingGenerator.Size() { Height = 2, Width = 1 },
            };

            Rectangle bottomWall = new Rectangle()
            {
                X = 1,
                Y = 1,
                FillTile = nameof(Terraria.ID.TileID.WoodBlock),
                Size = new TUBGWorldGenerator.BuildingGenerator.Size() { Height = 1, Width = 8 },
            };

            root.Childs.Add(leftWall);
            root.Childs.Add(rightWall);
            root.Childs.Add(topWall);
            root.Childs.Add(bottomWall);

            Rectangle inner = new Rectangle()
            {
                X = 2,
                Y = 2,
                FillWall = nameof(Terraria.ID.WallID.Wood),
                Size = new TUBGWorldGenerator.BuildingGenerator.Size() { Height = 4, Width = 6 },
            };

            root.Childs.Add(inner);

            Parts.TileObject leftDoor = new Parts.TileObject()
            {
                ItemName = "WoodenDoor",
                X = 1,
                Y = 2,
            };

            Parts.TileObject rightDoor = new Parts.TileObject()
            {
                ItemName = "WoodenDoor",
                X = 8,
                Y = 2,
            };

            Parts.TileObject table = new Parts.TileObject()
            {
                ItemName = "WoodenTable",
                X = 3,
                Y = 2,
            };

            Parts.TileObject chair = new Parts.TileObject()
            {
                ItemName = "WoodenChair",
                X = 6,
                Y = 2,
            };

            root.Childs.Add(leftDoor);
            root.Childs.Add(rightDoor);
            root.Childs.Add(table);
            root.Childs.Add(chair);

            jsonText = JsonConvert.SerializeObject(root, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
            });
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JsonText)));
        }

        private void SelectDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                DefaultDirectory = BuildingGenerator.BuildingsRootPath,
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                BuildingGenerator.BuildingsRootPath = dialog.FileName;
                Configs.LastBuildingsPath = dialog.FileName;
            }
        }

        private void TileGrid_MouseMove(object sender, MouseEventArgs e)
        {
            Point point = Mouse.GetPosition(TileGrid);

            int columnCount = TileGrid.ColumnDefinitions.Count;
            int rowCount = TileGrid.RowDefinitions.Count;

            double width = TileGrid.ActualWidth;
            double height = TileGrid.ActualHeight;

            int positionX = (int)(point.X / width * columnCount) + 1;
            int positionY = rowCount - (int)(point.Y / height * rowCount) + 1;

            InformationText.Text = $"X: {positionX}, Y: {positionY}";
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

            ModifyTile(positionX, positionY, e.LeftButton == MouseButtonState.Pressed, e.RightButton == MouseButtonState.Pressed);
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
            Point point = Mouse.GetPosition(this);
            window.Top = this.Top + point.Y;
            window.Left = this.Left + point.X;
            window.ShowDialog();
            CurrentHammerType = window.SelectedHammerType;
        }

        private void PaintButton_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var window = new SelectPaintWindow();
            Point point = Mouse.GetPosition(this);
            window.Top = this.Top + point.Y;
            window.Left = this.Left + point.X;
            window.ShowDialog();
            if (!string.IsNullOrEmpty(window.SelectedPaintName))
            {
                CurrentPaintName = window.SelectedPaintName;
            }
        }
    }
}
