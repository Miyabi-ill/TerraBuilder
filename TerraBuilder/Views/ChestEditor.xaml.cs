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
using System.Windows.Shapes;

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
        }

        /// <summary>
        /// チェストグループ名を更新する。
        /// </summary>
        /// <param name="oldName">旧チェストグループ名</param>
        /// <param name="newName">新チェストグループ名</param>
        private void UpdateChestGroupName(string oldName, string newName)
        {
            // TODO: 重複チェック
            if (Configs.ChestGroups.ContainsKey(oldName))
            {
                var value = Configs.ChestGroups[oldName];
                Configs.ChestGroups.Remove(oldName);
                Configs.ChestGroups.Add(newName, value);
            }
        }

        /// <summary>
        /// チェスト名を更新する。
        /// </summary>
        /// <param name="oldName">旧チェスト名</param>
        /// <param name="newName">新チェスト名</param>
        private void UpdateChestName(string oldName, string newName)
        {
            // TODO: 重複チェック
            if (Configs.Chests.ContainsKey(oldName))
            {
                var value = Configs.Chests[oldName];
                Configs.Chests.Remove(oldName);
                Configs.Chests.Add(newName, value);

                // チェストグループはチェスト名を参照するので、全てのチェストグループを探索し、更新していく
                foreach (var group in Configs.ChestGroups)
                {
                    var list = group.Value;
                    foreach (var item in list)
                    {
                        if (item.Name == oldName)
                        {
                            item.Name = newName;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// アイテムスロット名を変更する。
        /// </summary>
        /// <param name="oldName">旧アイテムスロット名</param>
        /// <param name="newName">新アイテムスロット名</param>
        private void UpdateItemSlotName(string oldName, string newName)
        {
            // TODO: 重複チェック
            if (Configs.ItemSlots.ContainsKey(oldName))
            {
                var value = Configs.ItemSlots[oldName];
                Configs.ItemSlots.Remove(oldName);
                Configs.ItemSlots.Add(newName, value);

                // チェストとアイテムスロットはアイテムスロット名を参照するので、全てのチェストとアイテムスロットを探索し、更新していく
                // チェスト内アイテムスロット参照の変更
                foreach (var chest in Configs.Chests)
                {
                    var context = chest.Value;
                    foreach (var itemSlot in context.ItemSlots)
                    {
                        if (itemSlot.Name.Equals(oldName))
                        {
                            itemSlot.Name = newName;
                        }
                    }
                }

                // アイテムスロット内アイテムスロット参照の変更
                foreach (var itemSlot in Configs.ItemSlots)
                {
                    foreach (var itemsProb in itemSlot.Value.Items)
                    {
                        if (itemsProb.Name.Equals(oldName))
                        {
                            itemsProb.Name = newName;
                        }
                    }
                }

                // アイテムスロット参照の変更
                var tmp = Configs.ItemSlots[oldName];
                Configs.ItemSlots.Remove(oldName);
                Configs.ItemSlots.Add(newName, tmp);
            }
        }

        /// <summary>
        /// アイテム名を変更する。
        /// </summary>
        /// <param name="oldName">旧アイテム名</param>
        /// <param name="newName">新アイテム名</param>
        private void UpdateItemName(string oldName, string newName)
        {
            // TODO: 重複チェック
            if (Configs.Items.ContainsKey(oldName))
            {
                // アイテムスロット内アイテム名の変更
                foreach (var itemSlot in Configs.ItemSlots)
                {
                    foreach (var itemsProb in itemSlot.Value.Items)
                    {
                        if (itemsProb.Name.Equals(oldName))
                        {
                            itemsProb.Name = newName;
                        }
                    }
                }

                // アイテム名の変更
                var tmp = Configs.Items[oldName];
                Configs.Items.Remove(oldName);
                Configs.Items.Add(newName, tmp);
            }
        }

        /// <summary>
        /// コンボボックスの内容を更新する。
        /// </summary>
        private void UpdateComboBox()
        {
            ChestGroupSelectionComboBox.ItemsSource = Configs.ChestGroups.Keys.ToList();
            ChestSelectionComboBox.ItemsSource = Configs.Chests.Keys.ToList();
            ItemSlotSelectionComboBox.ItemsSource = Configs.ItemSlots.Keys.ToList();
            ItemSelectionComboBox.ItemsSource = Configs.Items.Keys.ToList();
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

        }
    }
}
