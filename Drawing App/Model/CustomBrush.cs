using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;
using Rect = System.Windows.Rect;
namespace Drawing_App.Model
{
    public class CustomBrush
    {
        public string Name { get; set; } // Name of the brush

        // Texture for the brush
        public BitmapImage Texture { get; set; }

        // Brush properties
        public double Opacity { get; set; } = 1.0;         // Overall opacity
        public double Hardness { get; set; } = 0.5;        // Edge softness (0 = soft, 1 = hard)
        public double Spacing { get; set; } = 1.0;         // Brush spacing
        public double Flow { get; set; } = 1.0;            // Paint flow rate
        public double Blending { get; set; } = 0.5;        // Blending factor (interaction with canvas)
        public double TextureScale { get; set; } = 1.0;    // Scale of the brush texture

        // Internal DrawingBrush instance
        private DrawingBrush _drawingBrush;

        public CustomBrush()
        {
            _drawingBrush = new DrawingBrush();
            UpdateBrush();
        }

        // Method to update the DrawingBrush based on the current properties
        public void UpdateBrush()
        {
            // Define the gradient brush for the radial effect
            var gradientBrush = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.5, 0.5),
                Center = new Point(0.5, 0.5),
                RadiusX = 1.0,
                RadiusY = 1.0,
                GradientStops = new GradientStopCollection
        {
            new GradientStop(Color.FromArgb((byte)(Opacity * 255), 0, 0, 0), 0.0),
            new GradientStop(Color.FromArgb((byte)(Opacity * Hardness * 255), 0, 0, 0), 1.0)
        }
            };

            // If a texture is set, apply it as a mask
            if (Texture != null)
            {
                var textureBrush = new ImageBrush(Texture)
                {
                    Opacity = Opacity,
                    Stretch = Stretch.Uniform
                };

                gradientBrush = CombineBrushes(gradientBrush, textureBrush);
            }

            // Define geometry for the entire canvas
            var geometry = new EllipseGeometry(new Point(0.5, 0.5), 0.5, 0.5);

            // Combine the geometry and the brush into a DrawingBrush
            var strokeDrawing = new GeometryDrawing(gradientBrush, null, geometry);

            // Update the DrawingBrush instance
            _drawingBrush.Drawing = strokeDrawing;

            // Ensure the brush covers the entire canvas
            _drawingBrush.TileMode = TileMode.None;
            _drawingBrush.Viewport = new Rect(0, 0, 1, 1);
            _drawingBrush.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
        }


        // Helper method to combine brushes (e.g., gradient + texture)
        private RadialGradientBrush CombineBrushes(RadialGradientBrush gradientBrush, ImageBrush textureBrush)
        {
            // Logic to blend textureBrush with gradientBrush
            // This may require creating a custom shader or overlaying them
            return gradientBrush; // For simplicity, just return the gradient for now
        }

        // Expose the DrawingBrush for external use
        public DrawingBrush GetDrawingBrush()
        {
            UpdateBrush();
            return _drawingBrush;
        }
    }
}