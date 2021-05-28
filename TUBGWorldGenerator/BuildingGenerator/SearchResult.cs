using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TUBGWorldGenerator.BuildingGenerator
{
    public class SearchResult : INotifyPropertyChanged
    {
        private BitmapImage image;
        private string name;
        private string originalName;
        private IEnumerable<string> tags;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        public BitmapImage Image
        {
            get => image;
            set
            {
                image = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
            }
        }

        public string Name
        {
            get => name;
            set
            {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public string OriginalName
        {
            get => originalName;
            set
            {
                originalName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OriginalName)));
            }
        }

        public IEnumerable<string> Tags
        {
            get => tags;
            set
            {
                tags = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tags)));
            }
        }
    }
}
