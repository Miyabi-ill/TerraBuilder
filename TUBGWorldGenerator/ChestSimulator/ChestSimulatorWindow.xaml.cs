namespace TUBGWorldGenerator.ChestSimulator
{
    using System.Collections.Generic;
    using System.Windows;
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
    }
}
