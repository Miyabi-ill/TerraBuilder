namespace TUBGWorldGenerator.BuildingGenerator.UI
{
    using System.ComponentModel;
    using System.Linq;
    using System.IO;
    using System.Text;
    using System.Windows;
    using Microsoft.Win32;
    using TUBGWorldGenerator.Utils;
    using System.Windows.Controls;
    using Newtonsoft.Json;

    /// <summary>
    /// Interaction logic for BuildingGeneratorWindow.xaml
    /// </summary>
    public partial class BuildingGeneratorWindow : Window, INotifyPropertyChanged
    {
        private string syncFileName;
        private string jsonText;
        private BuildingGenerator buildingGenerator;
        private FileSystemWatcher fileSystemWatcher;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        public BuildingGeneratorWindow()
        {
            InitializeComponent();

            BuildingGenerator = new BuildingGenerator();
        }

        public string SyncFileName
        {
            get => syncFileName;
            set
            {
                syncFileName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SyncFileName)));
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

        public BuildingGenerator BuildingGenerator
        {
            get => buildingGenerator;
            private set
            {
                buildingGenerator = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BuildingGenerator)));
            }
        }

        private FileSystemWatcher FileSystemWatcher
        {
            get => fileSystemWatcher;
            set
            {
                fileSystemWatcher = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileSystemWatcher)));
            }
        }

        private void ParseJson()
        {
            BuildingGenerator.ImportJson(JsonText);
            BuildingGenerator.Build();

            if (BuildingGenerator.Result != null)
            {
                GenerateFailedOverlay.Visibility = Visibility.Hidden;
                PreviewImage.Source = TileToImage.CreateBitmap(BuildingGenerator.Result);
            }
            else
            {
                GenerateFailedOverlay.Visibility = Visibility.Visible;
            }
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name == Path.GetFileName(SyncFileName))
            {
                using (var sr = new StreamReader(e.FullPath))
                {
                    JsonText = sr.ReadToEnd();
                }
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
                if (SyncFileName != fileSelectDialog.FileName)
                {
                    SyncFileName = fileSelectDialog.FileName;
                    FileSystemWatcher = new FileSystemWatcher(SyncFileName)
                    {
                        NotifyFilter = NotifyFilters.LastWrite,
                    };
                    FileSystemWatcher.Changed += FileSystemWatcher_Changed;
                }
            }
        }

        private void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog()
            {
                Filter = "Jsonファイル(*.json)|*.json|すべてのファイル(*.*)|*.*",
                RestoreDirectory = true,
                FileName = "Building.xml",
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                SyncFileName = saveFileDialog.FileName;
                using (var sw = new StreamWriter(SyncFileName))
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

            TileObject leftDoor = new TileObject()
            {
                ItemName = "WoodenDoor",
                X = 1,
                Y = 2,
            };

            TileObject rightDoor = new TileObject()
            {
                ItemName = "WoodenDoor",
                X = 8,
                Y = 2,
            };

            TileObject table = new TileObject()
            {
                ItemName = "WoodenTable",
                X = 3,
                Y = 2,
            };

            TileObject chair = new TileObject()
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
                TypeNameHandling = TypeNameHandling.All,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
            });
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JsonText)));
        }
    }
}
