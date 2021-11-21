namespace TerraBuilder.ChestSimulator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using TerraBuilder.Views;
    using TerraBuilder.WorldGeneration;

    /// <summary>
    /// Interaction logic for ChestSimulatorWindow.xaml
    /// </summary>
    public partial class ChestSimulatorWindow : Window
    {
        public ChestSimulatorWindow(WorldSandbox sandbox)
        {
            InitializeComponent();
            Sandbox = sandbox;
            UpdateComboBox();
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
                    };

                    itemProbablys.Add(itemProbably.Key, pieData);
                }

                pieCharts.Add(new PieChart(result.ChestName, itemProbablys));
            }

            pieCharts.Insert(0, new PieChart("チェスト確率", chestProbablys));

            return pieCharts;
        }

        private static List<PieChart> CreatePieChartsFromDictionary(Dictionary<string, Dictionary<string, double>> itemDict)
        {
            List<PieChart> pieCharts = new List<PieChart>();
            var itemNameAndColor = new Dictionary<string, Color>();
            var random = new Random();

            foreach (var probablyDict in itemDict)
            {
                var pieDatas = new Dictionary<string, PieChart.PieData>();
                foreach (var probably in probablyDict.Value)
                {
                    Color color;
                    if (itemNameAndColor.ContainsKey(probably.Key))
                    {
                        color = itemNameAndColor[probably.Key];
                    }
                    else
                    {
                        double h = random.NextDouble() * 360;
                        double s = (random.NextDouble() * 0.5) + 0.5;
                        double v = (random.NextDouble() * 0.2) + 0.8;
                        var rgb = Utils.Colors.HSVtoRGB(h, s, v);
                        color = Color.FromRgb(rgb.r, rgb.g, rgb.b);
                        itemNameAndColor.Add(probably.Key, color);
                    }

                    var pieData = new PieChart.PieData()
                    {
                        Percentage = probably.Value,
                        Color = new SolidColorBrush(color),
                        Title = probably.Key,
                    };
                    pieDatas.Add(probably.Key, pieData);
                }

                pieCharts.Add(new PieChart(probablyDict.Key, pieDatas));
            }

            return pieCharts;
        }

        private void FromWorldButton_Click(object sender, RoutedEventArgs e)
        {
            Configs.LoadAllChestConfigs(Configs.LastChestConfigsDir);
            ErrorMessageBox.Text = string.Empty;

            var results = ChestAccumulateResult.CreateResultFromWorld(Sandbox);
            var accResult = ChestAccumulateResult.CreateOverrollResult(results.Values);
            var allResults = new List<ChestAccumulateResult>();
            allResults.Add(accResult);
            allResults.AddRange(results.Values);

            ChestAccResults.ItemsSource = allResults;
            CircleGraphList.ItemsSource = CreatePieChartsFromResult(allResults);
        }

        private void SimulateButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessageBox.Text = string.Empty;
            if (int.TryParse(ChestSimulateCountBox.Text, out int chestCount))
            {
                try
                {
                    //Configs.LoadAllChestConfigs(Configs.LastChestConfigsDir);

                    var results = ChestAccumulateResult.CreateResultFromChestGroupWithStep(ChestGroupComboBox.Text, chestCount);
                    var accResult = ChestAccumulateResult.CreateOverrollResult(results.chestAccResults.Values);
                    var allResults = new List<ChestAccumulateResult>();
                    allResults.Add(accResult);
                    allResults.AddRange(results.chestAccResults.Values);

                    ChestAccResults.ItemsSource = allResults;

                    var chestDict = new Dictionary<string, Dictionary<string, double>>()
                    {
                        ["チェスト総計"] = results.chestProbably.ToDictionary(x => x.Key, x => x.Value.probably),
                    };
                    var slotDict = results.itemSlotProbably.ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.Key, y => y.Value.probably));
                    var itemDict = results.itemProbablyPerItemSlot.ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.Key, y => y.Value.probably));

                    var resultsChart = CreatePieChartsFromDictionary(chestDict);
                    resultsChart.AddRange(CreatePieChartsFromDictionary(slotDict));
                    resultsChart.AddRange(CreatePieChartsFromDictionary(itemDict));
                    CircleGraphList.ItemsSource = resultsChart;
                }
                catch (Exception ex)
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

        private void ShowDataMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ShowDataMode.SelectedIndex == -1 || !(ShowDataMode.SelectedItem is ComboBoxItem item && item.Content is string selectedText))
            {
                return;
            }

            if (ListViewModeGrid == null || CircleGraphViewModeGrid == null)
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

        private void ChestGroupUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateComboBox();
        }

        private void UpdateComboBox()
        {
            ChestGroupComboBox.ItemsSource = Configs.ChestGroups.Keys;
        }
    }
}
