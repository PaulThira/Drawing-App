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
    /// Interaction logic for CustomBrushes.xaml
    /// </summary>
    public partial class CustomBrushes : Window
    {
        private readonly CustomBrushesVM _viewModel;
        public CustomBrushes()
        {
            InitializeComponent();
            _viewModel = new CustomBrushesVM();
            DataContext = _viewModel;

            // Set up event handlers for real-time updates
            TextureSlider.ValueChanged += (s, e) => _viewModel.TextureScale = TextureSlider.Value;
            OpacitySlider.ValueChanged += (s, e) => _viewModel.Opacity = OpacitySlider.Value;
            HardnessSlider.ValueChanged += (s, e) => _viewModel.Hardness = HardnessSlider.Value;
            SpacingSlider.ValueChanged += (s, e) => _viewModel.Spacing = SpacingSlider.Value;
            FlowSlider.ValueChanged += (s, e) => _viewModel.Flow = FlowSlider.Value;
            BlendingSlider.ValueChanged += (s, e) => _viewModel.Blending = BlendingSlider.Value;

            BrushNameTextBox.TextChanged += (s, e) => _viewModel.Name = BrushNameTextBox.Text;
        }
    }
}
