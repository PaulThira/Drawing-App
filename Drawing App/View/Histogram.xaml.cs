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
using Drawing_App.VM;

namespace Drawing_App.View
{
    /// <summary>
    /// Interaction logic for Histogram.xaml
    /// </summary>
    public partial class Histogram : Window
    {
        public Histogram(int[] redHistogram, int[] greenHistogram, int[] blueHistogram)
        {
            InitializeComponent();
            var viewModel = new HistogramVM(redHistogram, greenHistogram, blueHistogram);

            // Set the DataContext to the ViewModel
            DataContext = viewModel;
        }
    }
}
