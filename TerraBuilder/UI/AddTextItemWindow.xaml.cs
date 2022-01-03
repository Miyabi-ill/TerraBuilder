namespace TerraBuilder.UI
{
    using System.ComponentModel;
    using System.Windows;

    /// <summary>
    /// Interaction logic for AddTextItemWindow.xaml
    /// </summary>
    public partial class AddTextItemWindow : Window, INotifyPropertyChanged
    {
        private string description = string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Description
        {
            get => description;
            set
            {
                description = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
            }
        }

        public string Text
        {
            get;
            set;
        }

        public AddTextItemWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
