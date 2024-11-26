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
using Drawing_App.Model;
using Drawing_App.VM;

namespace Drawing_App.View
{
    /// <summary>
    /// Interaction logic for CustomBrushes.xaml
    /// </summary>
    public partial class CustomBrushes : Window
    {
        public CustomBrushesVM _viewModel { get; set; }
        public CustomBrush _brush { get; set; }
        public CustomBrushes()
        {
            InitializeComponent();
            _viewModel = new CustomBrushesVM();
            DataContext = _viewModel;

            // Set up event handlers for real-time updates
           

            BrushNameTextBox.TextChanged += (s, e) => _viewModel.Name = BrushNameTextBox.Text;
        }
        private void TextureSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_viewModel != null)
                _viewModel.TextureScale = e.NewValue;
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_viewModel != null)
                _viewModel.Opacity = e.NewValue;
        }

        private void HardnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_viewModel != null)
                _viewModel.Hardness = e.NewValue;
        }

        private void SpacingSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_viewModel != null)
                _viewModel.Spacing = e.NewValue;
        }

        private void FlowSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_viewModel != null)
                _viewModel.Flow = e.NewValue;
        }

        private void BlendingSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_viewModel != null)
                _viewModel.Blending = e.NewValue;
        }


        private void SaveBrushButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SaveBrushCommand.CanExecute())
            {
                _viewModel.SaveBrushCommand.Execute();
            }

            // Retrieve the brush from the ViewModel if needed
            _brush = _viewModel.CustomBrushy;

            // Close the dialog
            this.DialogResult = true;
            this.Close();
        }
    }
}
