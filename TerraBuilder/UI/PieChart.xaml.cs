namespace TerraBuilder.UI
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
    using Xceed.Wpf.Toolkit;

    /// <summary>
    /// Interaction logic for PieChart.xaml
    /// </summary>
    public partial class PieChart : UserControl
    {
        public class PieData
        {
            public double Percentage { get; set; }

            public string Title { get; set; }

            public string PieHeader => Title + (AddPercentageAfterTitle ? $" - {Percentage:P2}" : string.Empty);

            public string Description { get; set; }

            public bool AddPercentageAfterTitle { get; set; } = true;

            public Brush Color { get; set; }
        }

        public PieChart(string header, IDictionary<string, PieData> pieDatas)
        {
            Header = header;
            Width = 400;
            Height = 300;

            InitializeComponent();

            if (pieDatas != null)
            {
                double startAngle = 270;
                var datas = pieDatas.Values.OrderByDescending(x => x.Percentage);
                foreach (PieData data in datas)
                {
                    Pie pie = new Pie()
                    {
                        Fill = data.Color,
                        StartAngle = startAngle,
                        Slice = data.Percentage,
                        SweepDirection = SweepDirection.Clockwise,
                    };
                    startAngle = pie.EndAngle;

                    PieBaseGrid.Children.Add(pie);
                }

                LegendList.ItemsSource = datas;
            }
        }

        public string Header { get; }
    }
}
