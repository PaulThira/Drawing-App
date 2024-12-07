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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private MainWindowVM _viewModel;
        private System.Windows.Point? _startPoint;
        private Polyline _currentPolyline;
        public Window1()
        {
            InitializeComponent();
            _viewModel = (MainWindowVM)DataContext;
            SizeBrush.ValueChanged += SizeBrush_ValueChanged;
           
            SaturationSlider.ValueChanged += SaturationSlider_ValueChanged;
            ValueSlider.ValueChanged += BrightnessSlider_ValueChanged;
            OpacitySlider.ValueChanged += OpacitySlider_ValueChanged;

        }
        private void SizeBrush_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_viewModel != null)
            {
                _viewModel.OnBrushSizeChanged(e.NewValue);
            }
        }
        private void Thresholding_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_viewModel != null)
            {
                _viewModel.Threshold = (int)(e.NewValue);
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

    }
}
