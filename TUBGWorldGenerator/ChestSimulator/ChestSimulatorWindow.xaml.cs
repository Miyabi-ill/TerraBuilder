namespace TUBGWorldGenerator.ChestSimulator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using TUBGWorldGenerator.Views;
    using TUBGWorldGenerator.WorldGeneration;

    /// <summary>
    /// Interaction logic for ChestSimulatorWindow.xaml
    /// </summary>
    public partial class ChestSimulatorWindow : Window
    {
        public ChestSimulatorWindow(WorldSandbox sandbox)
        {
            InitializeComponent();
            Sandbox = sandbox;
        }

        private WorldSandbox Sandbox { get; }

        private static List<PieChart> CreatePieChartsFromResult(IEnumerable<ChestAccumulateResult> results)
        {
            List<PieChart> pieCharts = new List<PieChart>();
            var chestNameAndColor = new Dictionary<string, Color>();
            var random = new Random();
            foreach (var result in results)
            {
                double h = random.NextDouble() * 360;
                double s = (random.NextDouble() * 0.5) + 0.5;
                double v = (random.NextDouble() * 0.2) + 0.8;
                var rgb = Utils.Colors.HSVtoRGB(h, s, v);
                chestNameAndColor.Add(result.ChestName, Color.FromRgb(rgb.r, rgb.g, rgb.b));
            }

            // チェスト全体の確率表示
            var chestProbablys = new Dictionary<string, PieChart.PieData>();
            foreach (var result in results)
            {
                if (result.ChestName == "総計")
                {
                    continue;
                }

                var data = new PieChart.PieData()
                {
                    Percentage = result.Probably,
                    Color = new SolidColorBrush(chestNameAndColor[result.ChestName]),
                    Title = result.ChestName,
                };
                chestProbablys.Add(result.ChestName, data);
            }

            // チェストごとアイテム確率表示
            var itemNameAndColor = new Dictionary<string, Color>();
            foreach (var result in results)
            {
                double probablySum = result.ItemsProbablys.Sum(x => x.Value.Item4);
                var itemProbablys = new Dictionary<string, PieChart.PieData>();
                foreach (var itemProbably in result.ItemsProbablys)
                {
                    Color color;
                    if (itemNameAndColor.ContainsKey(itemProbably.Key))
                    {
                        color = itemNameAndColor[itemProbably.Key];
                    }
                    else
                    {
                        double h = random.NextDouble() * 360;
                        double s = (random.NextDouble() * 0.5) + 0.5;
                        double v = (random.NextDouble() * 0.2) + 0.8;
                        var rgb = Utils.Colors.HSVtoRGB(h, s, v);
                        color = Color.FromRgb(rgb.r, rgb.g, rgb.b);
                        itemNameAndColor.Add(itemProbably.Key, color);
                    }

                    var pieData = new PieChart.PieData()
                    {
                        Percentage = itemProbably.Value.Item4 / probablySum,
                        Color = new SolidColorBrush(color),
                        Title = itemProbably.Key,
                        AddPercentageAfterTitle = false,
                    };

                    itemProbablys.Add(itemProbably.Key, pieData);
                }

                pieCharts.Add(new PieChart(result.ChestName, itemProbablys));
            }

            pieCharts.Insert(0, new PieChart("チェスト確率", chestProbablys));

            return pieCharts;
        }

        private void FromWorldButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessageBox.Text = string.Empty;

            var results = ChestAccumulateResult.CreateResultFromWorld(Sandbox);
            var accResult = ChestAccumulateResult.CreateOverrollResult(results.Values);
            var allResults = new List<ChestAccumulateResult>();
            allResults.Add(accResult);
            allResults.AddRange(results.Values);

            ChestAccResults.ItemsSource = allResults;
        }

        private void SimulateButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessageBox.Text = string.Empty;
            if (int.TryParse(ChestSimulateCountBox.Text, out int chestCount))
            {
                try
                {
                    var results = ChestAccumulateResult.CreateResultFromChestGroup(ChestGroupNameBox.Text, chestCount);
                    var accResult = ChestAccumulateResult.CreateOverrollResult(results.Values);
                    var allResults = new List<ChestAccumulateResult>();
                    allResults.Add(accResult);
                    allResults.AddRange(results.Values);

                    ChestAccResults.ItemsSource = allResults;

                    CircleGraphList.ItemsSource = CreatePieChartsFromResult(allResults);
                }
                catch
                {
                    ErrorMessageBox.Text = "チェストグループ名が正しくありません";
                    return;
                }
            }
            else
            {
                ErrorMessageBox.Text = "チェスト数は整数で指定します";
            }
        }

        private void ShowDataMode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ShowDataMode.SelectedIndex == -1 || !(ShowDataMode.SelectedItem is ComboBoxItem item && item.Content is string selectedText))
            {
                return;
            }

            switch (selectedText)
            {
                case "表":
                    ListViewModeGrid.Visibility = Visibility.Visible;
                    CircleGraphViewModeGrid.Visibility = Visibility.Collapsed;
                    break;
                case "円グラフ":
                    ListViewModeGrid.Visibility = Visibility.Collapsed;
                    CircleGraphViewModeGrid.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
