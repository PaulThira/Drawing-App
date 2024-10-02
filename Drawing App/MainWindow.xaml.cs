using Drawing_App.Model;
using Drawing_App.VM;
using Microsoft.Win32;
using Syncfusion.Windows.Shared;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace Drawing_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowVM _viewModel;
        private System.Windows.Point? _startPoint;
        private Polyline _currentPolyline;
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = (MainWindowVM)DataContext;

            // Subscribe to the ValueChanged events
            SizeBrush.ValueChanged += SizeBrush_ValueChanged;
            HueSlider.ValueChanged += HueSlider_ValueChanged;
            SaturationSlider.ValueChanged += SaturationSlider_ValueChanged;
            BrightnessSlider.ValueChanged += BrightnessSlider_ValueChanged;
            OpacitySlider.ValueChanged += OpacitySlider_ValueChanged;
            
        }

        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Ellipse clickedEllipse = sender as Ellipse;
            if (clickedEllipse != null)
            {
                SolidColorBrush brush = clickedEllipse.Fill as SolidColorBrush;
                if (brush != null)
                {
                    var vm = (MainWindowVM)DataContext;
                    if (vm != null) { 
                        vm.CurrentColor = brush.Color;
                        
                        vm.UpdateColor(1);
                        vm.UpdateBrush();
                      
                    
                    }
                }
            }
        }

        private void SizeBrush_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_viewModel != null)
            {
                _viewModel.OnBrushSizeChanged(e.NewValue);
            }
        }

        private void HueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_viewModel != null)
            {
                _viewModel.OnHueChanged(e.NewValue);
            }
        }

        private void SaturationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_viewModel != null)
            {
                _viewModel.OnSaturationChanged(e.NewValue);
            }
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_viewModel != null)
            {
                _viewModel.OnBrightnessChanged(e.NewValue);
            }
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_viewModel != null)
            {
                _viewModel.OnOpacityChanged(e.NewValue);
            }
        }

      
        private void AddStrokeToCanvas(Polyline stroke)
        {
            
        }

        
    }
}