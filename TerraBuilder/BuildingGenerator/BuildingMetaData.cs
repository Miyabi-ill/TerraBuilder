namespace TerraBuilder.BuildingGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

    public class BuildingMetaData : INotifyPropertyChanged
    {
        private string name;
        private string originalName;
        private ObservableCollection<string> tags = new ObservableCollection<string>();
        private Size size = new Size() { Width = new ConstantValue<int> { Value = 1 }, Height = new ConstantValue<int> { Value = 1 } };

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        [DisplayName("建築名")]
        public string Name
        {
            get => name;
            set
            {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        [Browsable(false)]
        public string OriginalName
        {
            get => originalName;
            set
            {
                originalName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OriginalName)));
            }
        }

        [DisplayName("タグ")]
        public ObservableCollection<string> Tags
        {
            get => tags;
            set
            {
                tags = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tags)));
            }
        }

        [DisplayName("サイズ")]
        [ExpandableObject]
        public Size Size
        {
            get => size;
            set
            {
                size = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Size)));
            }
        }
    }
}
