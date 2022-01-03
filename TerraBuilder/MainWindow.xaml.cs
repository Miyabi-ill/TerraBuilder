namespace TerraBuilder
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using Microsoft.Win32;
    using Microsoft.WindowsAPICodePack.Dialogs;
    using TerraBuilder.BuildingGenerator;
    using TerraBuilder.ChestSimulator;
    using TerraBuilder.UI;
    using TerraBuilder.WorldGeneration;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// コンストラクタ.
        /// </summary>
        public MainWindow()
        {
            AppDomain.CurrentDomain.FirstChanceException += FirstChanceException;

            Terraria.Localization.LanguageManager.Instance.SetLanguage("en-US");
            Terraria.Main.Map = new Terraria.Map.WorldMap(1, 1);
            Terraria.Map.MapHelper.Initialize();

            Sandbox = new WorldSandbox();
            Runner = new WorldGenerationRunner();
            InitializeComponent();

            var window = new WorldGenerationWindow();
            window.Show();

            this.Title = "TerraBuilder - v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();

            ActionList.ItemsSource = Runner.WorldGenerationActions;

            TileEditor.Sandbox = Sandbox;

            Window = this;

            Configs.RecoverConfigsFromSaved();

            // グローバルコンテキストをプロパティグリッドに表示
            GlobalContextProperty.SelectedObject = Runner.GlobalContext;

            BuildingCache = new BuildingCache(new BuildingGenerator.BuildingGenerator() { BuildingsRootPath = Configs.LastBuildingsPath });
        }

        private void FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            Utils.ErrorLogger.Log(e.Exception);
        }

        internal static MainWindow Window { get; private set; }

        private WorldSandbox Sandbox { get; }

        private WorldGenerationRunner Runner { get; }

        internal BuildingCache BuildingCache { get; private set; }

        /// <summary>
        /// ユーザーにメッセージを表示する.
        /// </summary>
        /// <param name="text">表示するメッセージ</param>
        public void ShowMessage(string text)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                MessageTextBlock.Text = text;
                MessageTextBlock.Foreground = Brushes.Black;
            }));
        }

        /// <summary>
        /// ユーザーにエラーメッセージを表示する.
        /// </summary>
        /// <param name="text">表示するエラーメッセージ</param>
        public void ShowErrorMessage(string text)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                MessageTextBlock.Text = text;
                MessageTextBlock.Foreground = Brushes.Red;
            }));
        }

        private async void RunAllButton_Click(object sender, RoutedEventArgs e)
        {
            RunningOverlay.Visibility = Visibility.Visible;
            bool success = await Task.Run(() => Runner.Run(Sandbox)).ConfigureAwait(true);
            if (success)
            {
                ShowMessage("生成が正常に終了しました.");
            }

            TileEditor.UpdateMap();
            RunningOverlay.Visibility = Visibility.Collapsed;
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            RunningOverlay.Visibility = Visibility.Visible;
            bool success = await Task.Run(() => Sandbox.Reset()).ConfigureAwait(true);
            TileEditor.UpdateMap();
            RunningOverlay.Visibility = Visibility.Collapsed;
        }

        private async void RunActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is IWorldGenerationAction<ActionContext> generationAction)
            {
                RunningOverlay.Visibility = Visibility.Visible;
                bool success = await Task.Run(() => generationAction.Run(Sandbox)).ConfigureAwait(true);
                TileEditor.UpdateMap();
                RunningOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is IWorldGenerationAction<ActionContext> generationAction)
            {
                LocalContextProperty.SelectedObject = generationAction.Context;
                LocalContextExpander.Header = generationAction.Name;
                LocalContextExpander.IsExpanded = true;
            }
        }

        private void SaveWorldButton_Click(object sender, RoutedEventArgs e)
        {
            lock (Sandbox)
            {
                string path = Sandbox.Save(null);
                ShowMessage(string.Format("{0}に保存しました.", path));
                TileEditor.UpdateMap();
            }
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddActionWindow();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                // ダイアログがtrueを返せば、dialog.Actionはnon-nullを保証する
                WorldGenerationRunner.CurrentRunner.WorldGenerationActions.Add(dialog.Action);
            }
        }

        private void RemoveActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActionList.SelectedIndex != -1)
            {
                WorldGenerationRunner.CurrentRunner.WorldGenerationActions.RemoveAt(ActionList.SelectedIndex);
            }
        }

        private void UpActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActionList.SelectedIndex >= 1)
            {
                WorldGenerationRunner.CurrentRunner.WorldGenerationActions.Move(ActionList.SelectedIndex, ActionList.SelectedIndex - 1);
            }
        }

        private void DownActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActionList.SelectedIndex != -1 && ActionList.SelectedIndex != WorldGenerationRunner.CurrentRunner.WorldGenerationActions.Count - 1)
            {
                WorldGenerationRunner.CurrentRunner.WorldGenerationActions.Move(ActionList.SelectedIndex, ActionList.SelectedIndex + 1);
            }
        }

        private void LoadActionButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = "Jsonファイル(*.json)|*.json|すべてのファイル(*.*)|*.*",
                RestoreDirectory = true,
            };

            if (dialog.ShowDialog() == true)
            {
                Runner.Load(dialog.FileName);
                Configs.LastActionConfigPath = dialog.FileName;
                ActionList.ItemsSource = Runner.WorldGenerationActions;
                GlobalContextProperty.SelectedObject = Runner.GlobalContext;
                LocalContextProperty.SelectedObject = null;
                LocalContextExpander.Header = "Local Config";
            }
        }

        private void SaveActionButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog()
            {
                Filter = "Jsonファイル(*.json)|*.json|すべてのファイル(*.*)|*.*",
                FileName = "WorldGenerationActions.json",
                RestoreDirectory = true,
            };

            if (dialog.ShowDialog() == true)
            {
                Runner.Save(dialog.FileName);
                Configs.LastActionConfigPath = dialog.FileName;
            }
        }

        private void ChestSimulatorMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new ChestSimulatorWindow(Sandbox);
            window.Show();
        }

        private void BuildingGeneratorMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new BuildingGenerator.UI.BuildingGeneratorWindow();
            window.Show();
        }

        private void LoadChestConfigButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                RestoreDirectory = true,
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Configs.LoadAllChestConfigs(dialog.FileName);
                Configs.LastChestConfigsDir = dialog.FileName;
            }
        }

        private void RandomSeedButton_Click(object sender, RoutedEventArgs e)
        {
            WorldGenerationRunner.CurrentRunner.GlobalContext.Seed = new Random().Next();
        }

        private void BuildingSearchWindowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new BuildingSearchWindow(BuildingCache);
            window.Height = this.ActualHeight;
            window.PropertyChanged += BuildingSearchWindow_PropertyChanged;
            window.Show();
        }

        private void BuildingSearchWindow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is BuildingSearchWindow window)
            {
                if (e.PropertyName == nameof(window.SelectedResult))
                {
                    TileEditor.ToolTile = window.GetSelectedBuildTiles();
                }
            }
        }

        private void LoadWorldMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                InitialDirectory = System.IO.Path.Combine(Terraria.Main.SavePath, "Worlds"),
                Title = "読み込むワールドを選択",
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    Sandbox.Load(dialog.FileName);
                    TileEditor.UpdateMap();
                }
                catch
                {
                    Sandbox.Reset();
                }
            }
        }

        private void ChestSettingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new ChestEditor();
            window.Show();
        }
    }
}
