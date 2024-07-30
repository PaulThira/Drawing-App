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

        private void drawingCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel != null)
            {
                _startPoint = e.GetPosition(draw);
                _viewModel.StartStrokeCommand.Execute(_startPoint);

            }
        }

        private void drawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_viewModel != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    // Add the current point to the Polyline's Points collection
                    var currentPoint = e.GetPosition(draw);
                    _viewModel.ContinueStrokeCommand.Execute(currentPoint);
                }

            }
        }

        private void drawingCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel != null)
            {
                if (_currentPolyline != null && e.LeftButton == MouseButtonState.Pressed)
                {
                    // Add the current point to the Polyline's Points collection
                    _viewModel.EndStrokeCommand.Execute(null);
                }

            }
        }
        private void AddStrokeToCanvas(Polyline stroke)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(_viewModel!= null) {
              _viewModel.SaveCommand.Execute(draw);
            }
        }
    }
}