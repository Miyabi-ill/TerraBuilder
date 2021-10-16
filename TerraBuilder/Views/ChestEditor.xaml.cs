using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using TerraBuilder.WorldGeneration.Chests;

namespace TerraBuilder.Views
{
    /// <summary>
    /// Interaction logic for ChestEditor.xaml
    /// </summary>
    public partial class ChestEditor : Window
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ChestEditor()
        {
            InitializeComponent();
            UpdateComboBox();

            ItemDataGrid.ItemsSource = Configs.ItemsCollection;
            FileNameTextBox.Text = Configs.LastChestConfigsDir;
        }

        /// <summary>
        /// コンボボックスの内容を更新する。
        /// </summary>
        private void UpdateComboBox()
        {
            ChestGroupSelectionComboBox.ItemsSource = Configs.ChestGroups.Keys.ToList();
            ChestSelectionComboBox.ItemsSource = Configs.Chests.Keys.ToList();
            ItemSlotSelectionComboBox.ItemsSource = Configs.ItemSlots.Keys.ToList();
        }

        /// <summary>
        /// チェストグループのコンボボックスの選択変更イベントハンドラ
        /// </summary>
        /// <param name="sender">コンボボックス</param>
        /// <param name="e">イベント</param>
        private void ChestGroupSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                if (comboBox.SelectedItem is string chestGroupName && Configs.ChestGroups.ContainsKey(chestGroupName))
                {
                    ChestGroupDataGrid.ItemsSource = Configs.ChestGroups[chestGroupName];
                }
            }
        }

        private void ChestSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                if (comboBox.SelectedItem is string chestName && Configs.Chests.ContainsKey(chestName))
                {
                    ChestDataGrid.ItemsSource = Configs.Chests[chestName].ItemSlots;
                    TileIDIntegerUpDown.Value = Configs.Chests[chestName].TileType;
                    TileStyleIntegerUpDown.Value = Configs.Chests[chestName].TileStyle;
                    PaintIDIntegerUpDown.Value = Configs.Chests[chestName].Paint;
                }
            }
        }

        private void TileID_IntegerUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (ChestSelectionComboBox.SelectedItem is string chestName && Configs.Chests.ContainsKey(chestName))
            {
                // 21 = TileID.Containers
                Configs.Chests[chestName].TileType = TileIDIntegerUpDown.Value.GetValueOrDefault(21);
            }
        }

        private void TileStyle_IntegerUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (ChestSelectionComboBox.SelectedItem is string chestName && Configs.Chests.ContainsKey(chestName))
            {
                Configs.Chests[chestName].TileStyle = TileStyleIntegerUpDown.Value.GetValueOrDefault(0);
            }
        }

        private void PaintID_IntegerUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (ChestSelectionComboBox.SelectedItem is string chestName && Configs.Chests.ContainsKey(chestName))
            {
                Configs.Chests[chestName].Paint = PaintIDIntegerUpDown.Value.GetValueOrDefault(0);
            }
        }

        private void ItemSlotSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                if (comboBox.SelectedItem is string itemSlotName && Configs.ItemSlots.ContainsKey(itemSlotName))
                {
                    ItemSlotDataGrid.ItemsSource = Configs.ItemSlots[itemSlotName].Items;
                }
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            Configs.LastChestConfigsDir = FileNameTextBox.Text;
            Configs.LoadAllChestConfigs(FileNameTextBox.Text);
            ItemDataGrid.ItemsSource = Configs.ItemsCollection;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Configs.SaveAllChestConfigs(FileNameTextBox.Text);
        }

        private void AddChestGroupButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddTextItemWindow
            {
                Description = "チェストグループ名を入力してください",
                Title = "チェストグループを追加",
            };
            if (window.ShowDialog() == true && !string.IsNullOrWhiteSpace(window.Text) && !Configs.ChestGroups.ContainsKey(window.Text))
            {
                Configs.ChestGroups.Add(window.Text, new ObservableCollection<ChestProbably>());
                UpdateComboBox();
            }
        }

        private void DeleteChestGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChestGroupSelectionComboBox.SelectedItem is string chestGroupName && Configs.ChestGroups.ContainsKey(chestGroupName))
            {
                Configs.ChestGroups.Remove(chestGroupName);
                UpdateComboBox();
            }
        }

        private void AddChestButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddTextItemWindow
            {
                Description = "チェスト名を入力してください",
                Title = "チェストを追加",
            };
            if (window.ShowDialog() == true && !string.IsNullOrWhiteSpace(window.Text) && !Configs.Chests.ContainsKey(window.Text))
            {
                Configs.Chests.Add(window.Text, new ChestContext() { Name = window.Text });
                UpdateComboBox();
            }
        }

        private void DeleteChestButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChestSelectionComboBox.SelectedItem is string chestName && Configs.Chests.ContainsKey(chestName))
            {
                Configs.Chests.Remove(chestName);
                UpdateComboBox();
            }
        }

        private void AddItemSlotButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddTextItemWindow
            {
                Description = "アイテムスロット名を入力してください",
                Title = "アイテムスロットを追加",
            };
            if (window.ShowDialog() == true && !string.IsNullOrWhiteSpace(window.Text) && !Configs.ItemSlots.ContainsKey(window.Text))
            {
                Configs.ItemSlots.Add(window.Text, new ItemSlotContext() { Name = window.Text });
                UpdateComboBox();
            }
        }

        private void DeleteItemSlotButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemSlotSelectionComboBox.SelectedItem is string itemSlots && Configs.ItemSlots.ContainsKey(itemSlots))
            {
                Configs.ItemSlots.Remove(itemSlots);
                UpdateComboBox();
            }
        }

        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddTextItemWindow
            {
                Description = "アイテム名を入力してください",
                Title = "アイテムを追加",
            };
            if (window.ShowDialog() == true && !string.IsNullOrWhiteSpace(window.Text) && !Configs.Items.ContainsKey(window.Text))
            {
                Configs.ItemsCollection.Add(new ItemContext() { Name = window.Text });
            }
        }

        private void DeleteItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemDataGrid.SelectedItem is ItemContext context)
            {
                Configs.ItemsCollection.Remove(context);
            }
        }
    }
}
