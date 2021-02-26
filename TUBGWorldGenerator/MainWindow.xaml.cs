﻿namespace TUBGWorldGenerator
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;
    using TUBGWorldGenerator.Views;
    using TUBGWorldGenerator.WorldGeneration;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MainWindow()
        {
            Configs.LoadAll("Configs");

            Sandbox = new WorldSandbox();
            Runner = new WorldGenerationRunner();
            InitializeComponent();

            // グローバルコンテキストをプロパティグリッドに表示
            GlobalContextProperty.SelectedObject = Runner.GlobalContext;

            ActionList.ItemsSource = Runner.WorldGenerationActions;

            UpdateMapView();
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

        private async void RunAllButton_Click(object sender, RoutedEventArgs e)
        {
            RunningOverlay.Visibility = Visibility.Visible;
            bool success = await Task.Run(() => Runner.Run(Sandbox)).ConfigureAwait(true);
            UpdateMapView();
            RunningOverlay.Visibility = Visibility.Collapsed;
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            RunningOverlay.Visibility = Visibility.Visible;
            bool success = await Task.Run(() => Sandbox.Reset()).ConfigureAwait(true);
            UpdateMapView();
            RunningOverlay.Visibility = Visibility.Collapsed;
        }

        private async void RunActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is IWorldGenerationAction<ActionContext> generationAction)
            {
                RunningOverlay.Visibility = Visibility.Visible;
                bool success = await Task.Run(() => generationAction.Run(Sandbox)).ConfigureAwait(true);
                UpdateMapView();
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
                MessageTextBlock.Text = string.Format("{0}に保存しました。", path);
                UpdateMapView();
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
    }
}
