namespace TUBGWorldGenerator.BuildingGenerator.UI
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for SelectPaintWindow.xaml
    /// </summary>
    public partial class SelectPaintWindow : Window
    {
        public SelectPaintWindow()
        {
            InitializeComponent();
        }

        public string SelectedPaintName { get; set; }

        private void PaintButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                switch (button.ToolTip)
                {
                    case "Red Paint":
                        SelectedPaintName = "RedPaint";
                        break;
                    case "Orange Paint":
                        SelectedPaintName = "OrangePaint";
                        break;
                    case "Yellow Paint":
                        SelectedPaintName = "YellowPaint";
                        break;
                    case "Lime Paint":
                        SelectedPaintName = "LimePaint";
                        break;
                    case "Green Paint":
                        SelectedPaintName = "GreenPaint";
                        break;
                    case "Teal Paint":
                        SelectedPaintName = "TealPaint";
                        break;
                    case "Cyan Paint":
                        SelectedPaintName = "CyanPaint";
                        break;
                    case "Sky Blue Paint":
                        SelectedPaintName = "SkyBluePaint";
                        break;
                    case "Blue Paint":
                        SelectedPaintName = "BluePaint";
                        break;
                    case "Purple Paint":
                        SelectedPaintName = "PurplePaint";
                        break;
                    case "Violet Paint":
                        SelectedPaintName = "VioletPaint";
                        break;
                    case "Pink Paint":
                        SelectedPaintName = "PinkPaint";
                        break;
                    case "Deep Red Paint":
                        SelectedPaintName = "DeepRedPaint";
                        break;
                    case "Deep Orange Paint":
                        SelectedPaintName = "DeepOrangePaint";
                        break;
                    case "Deep Yellow Paint":
                        SelectedPaintName = "DeepYellowPaint";
                        break;
                    case "Deep Lime Paint":
                        SelectedPaintName = "DeepLimePaint";
                        break;
                    case "Deep Green Paint":
                        SelectedPaintName = "DeepGreenPaint";
                        break;
                    case "Deep Teal Paint":
                        SelectedPaintName = "DeepTealPaint";
                        break;
                    case "Deep Cyan Paint":
                        SelectedPaintName = "DeepCyanPaint";
                        break;
                    case "Deep Sky Blue Paint":
                        SelectedPaintName = "DeepSkyBluePaint";
                        break;
                    case "Deep Blue Paint":
                        SelectedPaintName = "DeepBluePaint";
                        break;
                    case "Deep Purple Paint":
                        SelectedPaintName = "DeepPurplePaint";
                        break;
                    case "Deep Violet Paint":
                        SelectedPaintName = "DeepVioletPaint";
                        break;
                    case "Deep Pink Paint":
                        SelectedPaintName = "DeepPinkPaint";
                        break;
                    case "Black Paint":
                        SelectedPaintName = "BlackPaint";
                        break;
                    case "White Paint":
                        SelectedPaintName = "WhitePaint";
                        break;
                    case "Gray Paint":
                        SelectedPaintName = "GrayPaint";
                        break;
                    case "Brown Paint":
                        SelectedPaintName = "BrownPaint";
                        break;
                    case "Shadow Paint":
                        SelectedPaintName = "ShadowPaint";
                        break;
                    case "Negative Paint":
                        SelectedPaintName = "NegativePaint";
                        break;
                    case "Illuminant Paint":
                        SelectedPaintName = "IlluminantPaint";
                        break;
                }

                Close();
            }
        }
    }
}
