﻿namespace TUBGWorldGenerator.BuildingGenerator.UI
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

    /// <summary>
    /// Interaction logic for BuildingFinderControl.xaml
    /// </summary>
    public partial class BuildingFinderControl : UserControl
    {
        public BuildingCache BuildingCache { get; set; }

        public BuildingFinderControl()
        {
            InitializeComponent();
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string searchText = textBox.Text.Trim();
                var searchResults = await BuildingCache.Search(searchText);
                SearchResult.Dispatcher.Invoke(() => SearchResult.ItemsSource = searchResults);
            }
        }
    }
}
