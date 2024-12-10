using Drawing_App.VM;
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

namespace Drawing_App.View
{
    /// <summary>
    /// Interaction logic for AffineTransformation.xaml
    /// </summary>
    public partial class AffineTransformation : Window
    {
        public float[,] Matrix {  get; set; } 
        public AffineTransformation()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = (AffineTransformationVM)DataContext;
            if (vm != null) { 
                float a00=float.Parse(vm.a00);
                float a01 = float.Parse(vm.a01);
                float a02 = float.Parse(vm.a02);
                float a10 = float.Parse(vm.a10);
                float a11 = float.Parse(vm.a11);
                float a12 = float.Parse(vm.a12);
              Matrix= new float[2, 3]
             {
                { a00, a01, a02 },
                { a10, a11, a12 }
             };
                DialogResult=true;


            }
        }
    }
}
