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
        public void UpdateBrush(byte b = 0, byte g = 0, byte r = 0)
        {
            // Preserve the existing brush properties
            var currentOpacity = Opacity;
            var currentFlow = Flow;
            var currentHardness = Hardness;

            if (Texture != null)
            {
                // Case: Texture is set
                var textureBrush = new ImageBrush(Texture)
                {
                    Opacity = currentOpacity, // Apply opacity
                    Stretch = Stretch.Uniform, // Adjust to your preference
                    TileMode = TileMode.Tile, // Enable tiling for repeated patterns
                    Viewport = new Rect(0, 0, 0.05 * TextureScale, 0.05 * TextureScale), // Scale the texture
                    ViewportUnits = BrushMappingMode.RelativeToBoundingBox // Use relative coordinates for tiling
                };

                // Wrap the ImageBrush in a DrawingBrush for flexibility
                _drawingBrush = new DrawingBrush
                {
                    Drawing = new GeometryDrawing(textureBrush, null, new RectangleGeometry(new Rect(0, 0, 1, 1))),
                    TileMode = TileMode.None, // Disable tiling for DrawingBrush
                    Viewport = new Rect(0, 0, 1, 1),
                    ViewportUnits = BrushMappingMode.RelativeToBoundingBox
                };
            }
            else
            {
                // Case: No texture, use gradient
                var gradientBrush = new RadialGradientBrush
                {
                    GradientOrigin = new Point(0.5, 0.5),
                    Center = new Point(0.5, 0.5),
                    RadiusX = 1.0,
                    RadiusY = 1.0,
                    GradientStops = new GradientStopCollection
            {
                // Center color with flow and opacity applied
                new GradientStop(Color.FromArgb((byte)(currentOpacity * currentFlow * 255), r, g, b), 0.0),
                
                // Edge color with hardness, flow, and opacity applied
                new GradientStop(Color.FromArgb((byte)(currentOpacity * currentHardness * currentFlow * 255), r, g, b), 1.0)
            }
                };

                // Combine the geometry and the gradient brush into a DrawingBrush
                _drawingBrush = new DrawingBrush
                {
                    Drawing = new GeometryDrawing(gradientBrush, null, new EllipseGeometry(new Point(0.5, 0.5), 0.5, 0.5)),
                    TileMode = TileMode.None,
                    Viewport = new Rect(0, 0, 1, 1),
                    ViewportUnits = BrushMappingMode.RelativeToBoundingBox
                };
            }

            // Apply additional properties for spacing, blending, etc.
            
        
        }

        private void ApplyBlending(Drawing strokeDrawing, double blendingFactor)
        {
            if (strokeDrawing is GeometryDrawing geometryDrawing)
            {
                if (geometryDrawing.Brush != null)
                {
                    // Apply blending by adjusting the brush opacity
                    geometryDrawing.Brush.Opacity *= blendingFactor;
                }
            }
        }




        private void ApplyBlending(GeometryDrawing strokeDrawing, double blendingFactor)
        {
            // Implement blending logic here (placeholder for advanced blending algorithms)
            // This might involve pixel manipulation or custom shader logic to simulate blending
            // For now, we apply a basic alpha adjustment as a demonstration
            strokeDrawing.Brush.Opacity = blendingFactor;
        }



        // Helper method to combine brushes (e.g., gradient + texture)
        private DrawingBrush CombineBrushes(RadialGradientBrush gradientBrush, ImageBrush textureBrush)
        {
            // Create a DrawingGroup to combine the gradient and texture
            DrawingGroup drawingGroup = new DrawingGroup();

            // Add the gradient brush
            drawingGroup.Children.Add(new GeometryDrawing(gradientBrush, null, new RectangleGeometry(new Rect(0, 0, 1, 1))));

            // Add the texture brush
            drawingGroup.Children.Add(new GeometryDrawing(textureBrush, null, new RectangleGeometry(new Rect(0, 0, 1, 1))));

            // Create a DrawingBrush from the combined DrawingGroup
            DrawingBrush combinedBrush = new DrawingBrush(drawingGroup)
            {
                TileMode = TileMode.None,
                Viewport = new Rect(0, 0, 1, 1),
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox,
                Stretch = Stretch.Uniform
            };

            return combinedBrush;
        }


        // Expose the DrawingBrush for external use
        public DrawingBrush GetDrawingBrush(byte b=0,byte g=0,byte r=0)
        {
            UpdateBrush(b,g,r);
            return _drawingBrush;
        }
    }
}