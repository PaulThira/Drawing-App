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
using Drawing_App.Model;
namespace Drawing_App.View
{
    /// <summary>
    /// Interaction logic for CustomFilter.xaml
    /// </summary>
    public partial class CustomFilter : Window
    {
        public Model.CustomFilter c {  get; set; }
        public CustomFilter()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as CustomFilterVM; // Retrieve the existing VM
            if (vm == null)
                return;

            vm.CheckValidityOfTheFilter();

            // Only proceed if the filter is valid
            c = new Model.CustomFilter
            {
                Kernel = vm.FilterMatrix,
                BlurRadius = vm.BlurRadius,
                Sharpness = vm.Sharpness,
                Saturation = vm.Saturation,
                Contrast = vm.Contrast,
                Brightness = vm.Brightness,
                EdgeIntensity = vm.EdgeIntensity,
                Name = vm.FilterName
            };

            DialogResult = true; // Close the dialog with success
        }

    }
}
