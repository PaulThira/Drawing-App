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
    /// Interaction logic for SplineTool.xaml
    /// </summary>
    public partial class SplineTool : Window
    {
        SplineToolVM vm;
        public int[] LUT;
        public SplineTool()
        {
            InitializeComponent();
            DataContext = new SplineToolVM(canvas);
            

        }
        private void OnCanvasClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Canvas c)
            {
                vm = (SplineToolVM)DataContext;
                if (vm != null && vm.points.Count <= 5)

                {
                    var point = e.GetPosition(c);
                   
                    vm.points.Add(new Point(point.X,point.Y));
                    Ellipse pointed = new Ellipse
                    {
                        Width = 2,  // 1 pixel radius, so width and height are 2 pixels
                        Height = 2,
                        Fill = Brushes.Black  // You can change this color as needed
                    };

                    // Set the position of the Ellipse on the Canvas
                    Canvas.SetLeft(pointed, point.X - 1);  // Subtract 1 to center the ellipse at the point
                    Canvas.SetTop(pointed, point.Y - 1);   // Subtract 1 to center the ellipse at the point

                    // Add the point to the Canvas
                    canvas.Children.Add(pointed);

                }
            }
            else { MessageBox.Show("Not from canvas"); }
           


        }
        private void SplineTool_Closed(object sender, EventArgs e)
        {
            // Code to handle when the window is closed
           
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            vm = (SplineToolVM)DataContext;
            if (vm != null)
            {
                vm.SplineToolCommand.Execute(null);
                LUT=vm.LUT;
                int g = LUT[0];
            }
            this.DialogResult = true;
        }
    }
}
