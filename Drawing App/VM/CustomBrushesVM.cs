using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;
using Rect = System.Windows.Rect;
namespace Drawing_App.VM
{
    public class CustomBrushesVM : BindableBase
    {
        // Editable properties
        private double _opacity;
        public double Opacity
        {
            get => _opacity;
            set
            {
                SetProperty(ref _opacity, value);
                UpdateBrushPreview();
            }
        }

        private double _hardness;
        public double Hardness
        {
            get => _hardness;
            set
            {
                SetProperty(ref _hardness, value);
                UpdateBrushPreview();
            }
        }

        private double _spacing;
        public double Spacing
        {
            get => _spacing;
            set
            {
                SetProperty(ref _spacing, value);
                UpdateBrushPreview();
            }
        }

        private double _flow;
        public double Flow
        {
            get => _flow;
            set
            {
                SetProperty(ref _flow, value);
                UpdateBrushPreview();
            }
        }

        private double _blending;
        public double Blending
        {
            get => _blending;
            set
            {
                SetProperty(ref _blending, value);
                UpdateBrushPreview();
            }
        }
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        private double _textureScale;
        public double TextureScale
        {
            get => _textureScale;
            set
            {
                SetProperty(ref _textureScale, value);
                UpdateBrushPreview();
            }
        }

        private BitmapImage _texture;
        public BitmapImage Texture
        {
            get => _texture;
            set
            {
                SetProperty(ref _texture, value);
                UpdateBrushPreview();
            }
        }

        private Brush _brushPreview;
        public Brush BrushPreview
        {
            get => _brushPreview;
            set => SetProperty(ref _brushPreview, value);
        }

        // Commands
        public DelegateCommand SaveBrushCommand { get; }
        public DelegateCommand PreviewBrushCommand { get; }
        public DelegateCommand ImportTextureCommand { get; }

        // Constructor
        public CustomBrushesVM()
        {
            // Initialize default values
            Opacity = 1.0;
            Hardness = 0.5;
            Spacing = 1.0;
            Flow = 1.0;
            Blending = 0.5;
            TextureScale = 1.0;

            // Initialize commands
            SaveBrushCommand = new DelegateCommand(SaveBrush);
            PreviewBrushCommand = new DelegateCommand(PreviewBrush);
            ImportTextureCommand = new DelegateCommand(ImportTexture);

            // Initial brush preview
            UpdateBrushPreview();
        }

        private void SaveBrush()
        {
            // Save the current brush configuration to a file or settings
            // Implementation depends on your application's requirements
        }

        private void PreviewBrush()
        {
            // Trigger a preview of the current brush on the canvas
            // Implementation depends on your application's brush engine
        }

        private void ImportTexture()
        {
            // Open a file dialog to import a texture
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Title = "Select a Texture"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(openFileDialog.FileName, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    Texture = bitmap;
                }
                catch (Exception ex)
                {
                    // Handle exceptions (e.g., invalid image file)
                }
            }
        }

        private void UpdateBrushPreview()
        {
            // Generate the DrawingBrush dynamically based on current settings
            var gradientBrush = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.5, 0.5),
                Center = new Point(0.5, 0.5),
                RadiusX = 1.0 - Hardness, // Hardness affects edge softness
                RadiusY = 1.0 - Hardness,
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb((byte)(Opacity * 255), 0, 0, 0), 0.0), // Center fully opaque
                    new GradientStop(Color.FromArgb((byte)(Opacity * 255 * (1 - Hardness)), 0, 0, 0), 1.0) // Edge transparency
                }
            };

            Geometry geometry = new EllipseGeometry(new Point(0.5, 0.5), TextureScale / 2, TextureScale / 2);

            var strokeDrawing = new GeometryDrawing(gradientBrush, null, geometry);

            // Apply texture if available
            if (Texture != null)
            {
                var textureBrush = new ImageBrush(Texture)
                {
                    TileMode = TileMode.Tile,
                    Viewport = new Rect(0, 0, TextureScale / 100, TextureScale / 100),
                    ViewportUnits = BrushMappingMode.RelativeToBoundingBox
                };
                strokeDrawing.Brush = textureBrush;
            }

            BrushPreview = new DrawingBrush(strokeDrawing)
            {
                TileMode = TileMode.None,
                Viewport = new Rect(0, 0, 1, 1),
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox
            };
        }
    }
}
