using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Drawing_App.Model;
using Drawing_App.VM;
namespace Drawing_App.View
{
    /// <summary>
    /// Interaction logic for ColorPalleteGenerator.xaml
    /// </summary>
    public partial class ColorPalleteGenerator : Window
    {
        public ObservableCollection<CustomPallete> pallete {  get; set; }
        public ColorPalleteGenerator()
        {
            InitializeComponent();
            Red.ValueChanged += Red_ValueChanged;
            Green.ValueChanged += Green_ValueChanged;
            Blue.ValueChanged += Blue_ValueChanged;
        }
        private void Red_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var vm = DataContext as ColorPalleteGeneratorVM;
            if (vm != null)
            {
                Color color = new Color();
                color=Color.FromRgb((byte)e.NewValue,vm.SelectedColor.Color.G,vm.SelectedColor.Color.B);
                vm.CurrentColor=vm.SelectedColor;
                vm.SelectedColor = new SolidColorBrush(color);
            }
        }

        private void Green_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var vm = DataContext as ColorPalleteGeneratorVM;
            if (vm != null)
            {
                Color color = new Color();
                color = Color.FromRgb(vm.SelectedColor.Color.R, (byte)e.NewValue, vm.SelectedColor.Color.B);
                vm.CurrentColor=vm.SelectedColor;
                vm.SelectedColor = new SolidColorBrush(color);
            }
        }

        private void Blue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var vm = DataContext as ColorPalleteGeneratorVM;
            if (vm != null)
            {
                Color color = new Color();
                color = Color.FromRgb(vm.SelectedColor.Color.R, vm.SelectedColor.Color.G, (byte)e.NewValue);
                vm.CurrentColor=vm.SelectedColor;
                vm.SelectedColor = new SolidColorBrush(color);
            }

        }
        private void OnSaturationValuePick(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as ColorPalleteGeneratorVM;
            if (vm != null)
            {
                Point point = e.GetPosition(svRectangle);
                vm.SelectSaturationValue(point.X, point.Y, svRectangle.Width, svRectangle.Height);
            }
        }

        private void OnSaturationValueDrag(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var vm = DataContext as ColorPalleteGeneratorVM;
                if (vm != null)
                {
                    Point point = e.GetPosition(svRectangle);
                    vm.SelectSaturationValue(point.X, point.Y, svRectangle.Width, svRectangle.Height);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            var vm= DataContext as ColorPalleteGeneratorVM;
            pallete = vm.SelectedPalette;
            this.Close();
        }
    }
}
