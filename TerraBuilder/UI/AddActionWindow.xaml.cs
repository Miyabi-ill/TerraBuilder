namespace TerraBuilder.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using TerraBuilder.WorldGeneration;

    /// <summary>
    /// Interaction logic for AddActionWindow.xaml
    /// </summary>
    public partial class AddActionWindow : Window
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public AddActionWindow()
        {
            InitializeComponent();
            ActionsComboBox.ItemsSource = WorldGenerationRunner.AvailableActions.OrderBy(x => x.Key);
        }

        /// <summary>
        /// 生成したアクション。
        /// </summary>
        public IWorldGenerationAction<ActionContext> Action { get; private set; }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActionsComboBox.SelectedItem is KeyValuePair<string, Func<IWorldGenerationAction<ActionContext>>> pair)
            {
                Action = pair.Value();
            }

            DialogResult = Action != null;
        }
    }
}
