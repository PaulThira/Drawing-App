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
            var vm = new CustomFilterVM();
            DataContext= vm;
            c = new Model.CustomFilter();
            vm.CheckValidityOfTheFilter();
            c.Kernel = vm.FilterMatrix;
            c.BlurRadius = vm.BlurRadius;
            c.Sharpness = vm.Sharpness;
            c.Saturation = vm.Saturation;
            c.Contrast = vm.Contrast;
            c.Brightness = vm.Brightness;
            c.EdgeIntensity = vm.EdgeIntensity;
            c.Name=vm.FilterName;
            DialogResult=true;

           

        }
    }
}
