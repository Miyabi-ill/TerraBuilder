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

        public BuildingMetaData BuildingMetaData { get; set; } = new BuildingMetaData();

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
                //UpdateGrid();
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

        private void RegenerateFromMetaDataButton_Click(object sender, RoutedEventArgs e)
        {
            int width = BuildingMetaData.Size.Width;
            int height = BuildingMetaData.Size.Height;
            Tiles = new Tile[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tiles[x, y] = new Tile();
                }
            }

            PreviewImage.Source = TileToImage.CreateBitmap(Tiles);
            //UpdateGrid();
        }
    }
}
