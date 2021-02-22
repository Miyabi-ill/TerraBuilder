using System.Windows;
using TUBGWorldGenerator.WorldGeneration;

namespace TUBGWorldGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var runner = new WorldGenerationRunner();
            runner.Run();
        }
    }
}
