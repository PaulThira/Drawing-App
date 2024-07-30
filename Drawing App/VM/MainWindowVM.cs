using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Input;
using System.Xml.Linq;
using Prism.Commands;
using Microsoft.Win32;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Ink;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Drawing;
using Color = System.Windows.Media.Color;
using Drawing_App.Model;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using System.Security.Cryptography.Xml;


namespace Drawing_App.VM
{
    public class MainWindowVM : BindableBase
    {
        private double _brushSize;
        private double _hue;
        private double _saturation;
        private double _brightness;
        private double _opacity;
        private DrawingAttributes _drawingAttributes;
        private double _inkCanvasWidth;
        private double _inkCanvasHeight;
        private Brush _backgroundColor;
        private Color _currentColor;
        private HashSet<Color> _usedColors;
        private ObservableCollection<Color> _colorPalette;
        private Brush _rectangleFill;
        private Brush _color1;
        private Brush _color2;
        private Brush _color3;
        private Brush _color4;
        private Brush _color5;
        private Brush _color6;
        private Brush _color7;
        private Polyline _currentPolyline;
        
        public ObservableCollection<UIElement> DrawingElements { get; }

        public ICommand StartStrokeCommand { get; }
        public ICommand ContinueStrokeCommand { get; }
        public ICommand EndStrokeCommand { get; }
        public ICommand PencilCommand { get; }
        public ICommand PenCommand { get; }
        public ICommand MarkerCommand { get; }

        private ImageBrush _pencilBrush;
        private DrawingBrush _currentBrush;
        public MainWindowVM()

        {

            PenCommand = new DelegateCommand(PenCall);
            DrawingElements = new ObservableCollection<UIElement>();
            _currentColor = Colors.Cyan;
            StartStrokeCommand = new DelegateCommand<Point?>(StartStroke);
            ContinueStrokeCommand = new DelegateCommand<Point?>(ContinueStroke);
            EndStrokeCommand = new DelegateCommand(EndStroke);
            
            RectangleFill = Brushes.Cyan;
            _usedColors = new HashSet<Color>();
            _colorPalette = new ObservableCollection<Color>();
            SaveCommand = new DelegateCommand<ItemsControl>(SaveCall);
            EraserCommand = new DelegateCommand(EraserCall);
            PencilCommand = new DelegateCommand(PencilCall);
            MarkerCommand = new DelegateCommand(MarkerCall);
            
            _opacity = 1.0;
            BrushSize = 5;
            Hue = 0;
            Saturation = 1;
            Brightness = 1;
            Opacity = 1;
            Strokes = new StrokeCollection();
            DrawingAttributes = new DrawingAttributes
            {
                Color = Colors.Cyan,
                Width = 5,
                Height = 5,
                FitToCurve = true
            };

            InkCanvasWidth = 4200;  // Default width
            InkCanvasHeight = 2100;
            BackgroundColor = new SolidColorBrush(Colors.White);// Default height
            Color1 = new SolidColorBrush(Colors.Red);
            Color2 = new SolidColorBrush(Colors.Green);
            Color3 = new SolidColorBrush(Colors.Blue);
            Color4 = new SolidColorBrush(Colors.Yellow);
            Color5 = new SolidColorBrush(Colors.Orange);
            Color6 = new SolidColorBrush(Colors.Purple);
            Color7 = new SolidColorBrush(Colors.Pink);
            _currentBrush = new DrawingBrush();
            _currentBrush.Drawing = new GeometryDrawing(
                brush: new SolidColorBrush(CurrentColor),   // Fill brush
                pen: new Pen(new SolidColorBrush(CurrentColor), 1), // Outline pen
                geometry: new RectangleGeometry(new Rect(0, 0, 10, 10))
            );
        }
        private void MarkerCall()
        {
            var rectGeometry = new RectangleGeometry(new Rect(0, 0, 1, 1));

            // Create a radial gradient to simulate the soft edges of a marker stroke
            var gradientBrush = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.5, 0.5),
                Center = new Point(0.5, 0.5),
                RadiusX = 0.5,
                RadiusY = 0.5,
                GradientStops = new GradientStopCollection
        {
            new GradientStop(CurrentColor, 0.5), // Solid color at the center
            new GradientStop(Color.FromArgb(75, CurrentColor.R, CurrentColor.G, CurrentColor.B), 1) // Transparent at the edges
        }
            };

            // Create the GeometryDrawing using the gradient brush and rectangle geometry
            var geometryDrawing = new GeometryDrawing(gradientBrush, null, rectGeometry);

            // Create the DrawingBrush with the GeometryDrawing
            var drawingBrush = new DrawingBrush(geometryDrawing)
            {
                TileMode = TileMode.None, // No tiling for a continuous marker effect
                Viewport = new Rect(0, 0, 1, 1), // Fill the entire area
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox
            };
            _currentBrush=drawingBrush;
            _usedColors.Add(_currentColor);
        }
        private void PenCall()
        {
            // Load the pencil texture from resources or a file path

            var rectGeometry = new RectangleGeometry(new Rect(0, 0, 1, 1));

            // Create a solid color brush with the specified color
            var solidColorBrush = new SolidColorBrush(CurrentColor);

            // Create a GeometryDrawing using the solid color brush and the rectangle geometry
            var geometryDrawing = new GeometryDrawing(solidColorBrush, null, rectGeometry);

            // Create the DrawingBrush with the GeometryDrawing
            var drawingBrush = new DrawingBrush(geometryDrawing)
            {
                // Set the Viewport to cover the entire area; no tiling needed
                Viewport = new Rect(0, 0, 1, 1),
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox,
                TileMode = TileMode.None // No tiling for a solid color effect
            };
            _currentBrush = drawingBrush;
            _usedColors.Add(_currentColor);
        }
        private void PencilCall()
        {
            // Load the pencil texture from resources or a file path
            var lineGroup = new GeometryGroup();
            for (int i = 0; i < 300; i++) // Increased number of lines for a denser texture
            {
                lineGroup.Children.Add(new LineGeometry(new Point(i * 5, 0), new Point(0, i * 5)));
            }

            // Use the CurrentBrushColor to simulate the pencil color
            var pencilPen = new Pen(new SolidColorBrush(CurrentColor), 1); // Thin lines for texture

            // Create the GeometryDrawing using the line group and pencil pen
            var pencilDrawing = new GeometryDrawing(null, pencilPen, lineGroup);

            // Optionally, combine with an additional color overlay or background if desired
            _currentBrush = new DrawingBrush(pencilDrawing)
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, 1000, 1000), // Adjust the viewport size for desired effect
                ViewportUnits = BrushMappingMode.Absolute,
                Opacity=0.5
            };
            _usedColors.Add(_currentColor);
        }
        private void StartStroke(Point? startPoint)
        {
            UpdateColor();
            if (startPoint == null) return;

            _currentPolyline = new Polyline
            {
                Stroke = _currentBrush,
                StrokeThickness = BrushSize,
                
                Points = new PointCollection { startPoint.Value }
            };
            DrawingElements.Add(_currentPolyline);


            _usedColors.Add(_currentColor);
            List<Color> colors = new List<Color>();
            int i = 0;
            foreach (var color in _usedColors)
            {

                Brush newBrush = new SolidColorBrush(color);
                colors.Add(color);
                if (i % 7 == 0)
                {
                    Color1 = newBrush;
                }
                else if (i % 7 == 1)
                {
                    Color2 = newBrush;
                }
                else if (i % 7 == 2)
                {
                    Color3 = newBrush;
                }
                else if (i % 7 == 3)
                {
                    Color4 = newBrush;
                }
                else if (i % 7 == 4)
                {
                    Color5 = newBrush;
                }
                else if (i % 7 == 5)
                {
                    Color6 = newBrush;
                }
                else if (i % 7 == 6)
                {
                    Color7 = newBrush;
                }
                i++;

            }
            HSVColours h = new HSVColours(colors);
            h.ColorHarmony();
            if(colors.Count>=2)
            {
                if(h.harmony==true)
                {
                    
                }
                else
                {
                    

                }
            }
        }

        private void ContinueStroke(Point? currentPoint)
        {
            if (currentPoint == null || _currentPolyline == null) return;
            _currentPolyline.Points.Add(currentPoint.Value);
        }

        private void EndStroke()
        {
            List<Color> colors = new List<Color>();
            foreach(var color in _usedColors)
            {
                colors.Add(color);
            }
            

            _currentPolyline = null;
        }



        public Brush Color1
        {
            get => _color1;
            set => SetProperty(ref _color1, value);
        }

        public Brush Color2
        {
            get => _color2;
            set => SetProperty(ref _color2, value);
        }

        public Brush Color3
        {
            get => _color3;
            set => SetProperty(ref _color3, value);
        }

        public Brush Color4
        {
            get => _color4;
            set => SetProperty(ref _color4, value);
        }

        public Brush Color5
        {
            get => _color5;
            set => SetProperty(ref _color5, value);
        }

        public Brush Color6
        {
            get => _color6;
            set => SetProperty(ref _color6, value);
        }

        public Brush Color7
        {
            get => _color7;
            set => SetProperty(ref _color7, value);
        }
        public Brush RectangleFill
        {
            get => _rectangleFill;
            set => SetProperty(ref _rectangleFill, value);
        }
        public Brush BackgroundColor
        {
            get => _backgroundColor;
            set=> _backgroundColor = value;
            
        }

        public StrokeCollection Strokes { get; set; }

        public DrawingAttributes DrawingAttributes
        {
            get => _drawingAttributes;
            set => SetProperty(ref _drawingAttributes, value);
        }

        public double BrushSize
        {
            get => _brushSize;
            set => SetProperty(ref _brushSize, value);
        }

        public double Hue
        {
            get => _hue;
            set => SetProperty(ref _hue, value);
        }

        public double Saturation
        {
            get => _saturation;
            set => SetProperty(ref _saturation, value);
        }

        public double Brightness
        {
            get => _brightness;
            set => SetProperty(ref _brightness, value);
        }

        public double Opacity
        {
            get => _opacity;
            set
            {
                if (SetProperty(ref _opacity, value))
                {
                    UpdateOpacity(value);
                }
            }
        }

        public double InkCanvasWidth
        {
            get => _inkCanvasWidth;
            set => SetProperty(ref _inkCanvasWidth, value);
        }

        public double InkCanvasHeight
        {
            get => _inkCanvasHeight;
            set => SetProperty(ref _inkCanvasHeight, value);
        }

        public ICommand SaveCommand { get; }

        public ICommand EraserCommand { get; }
        private void EraserCall()
        {
            var ellipseGeometry = new EllipseGeometry(new Point(0.5, 0.5), 0.5, 0.5);

            // Define the brush and pen for the geometry
            var fillBrush = new SolidColorBrush(Colors.White);
            var outlinePen = new Pen(new SolidColorBrush(Colors.White), 0.05);

            // Create the GeometryDrawing using the fill brush, outline pen, and geometry
            var geometryDrawing = new GeometryDrawing(fillBrush, outlinePen, ellipseGeometry);

            // Create the DrawingBrush with the GeometryDrawing
            var drawingBrush = new DrawingBrush(geometryDrawing)
            {
                TileMode = TileMode.Tile, // Repeat the drawing in both directions
                Viewport = new Rect(0, 0, 1, 1), // Define the size of one tile
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox // Size relative to the area being filled
            };
            _currentBrush = drawingBrush;
        }

        private void SaveCall(ItemsControl itemsControl)
        {
            if (itemsControl == null) return;

            // Find the Canvas within the ItemsControl
            Canvas canvas = FindVisualChild<Canvas>(itemsControl);

            if (canvas == null) return;

            // Define the size of the canvas
            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;

            // Create a RenderTargetBitmap to render the canvas
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)width, (int)height, 96d, 96d, PixelFormats.Pbgra32);

            // Render the canvas to the RenderTargetBitmap
            renderBitmap.Render(canvas);

            // Open SaveFileDialog to allow the user to specify the file path
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp",
                Title = "Save Canvas As Image",
                DefaultExt = "png"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Get the chosen file path
                string filePath = saveFileDialog.FileName;

                // Determine the file extension to select the appropriate encoder
                BitmapEncoder encoder;
                switch (System.IO.Path.GetExtension(filePath).ToLower())
                {
                    case ".jpg":
                    case ".jpeg":
                        encoder = new JpegBitmapEncoder();
                        break;
                    case ".bmp":
                        encoder = new BmpBitmapEncoder();
                        break;
                    default:
                        encoder = new PngBitmapEncoder();
                        break;
                }

                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                // Save the image to the file
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
        }
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                {
                    return t;
                }
                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }
            return null;
        }
        public Color CurrentColor
    {
        get => _currentColor;
        set => SetProperty(ref _currentColor, value);
    }

    public ObservableCollection<Color> ColorPalette
    {
        get => _colorPalette;
        set => SetProperty(ref _colorPalette, value);
    }

    

    private void UpdateBrush()
    {
        DrawingAttributes.Width = BrushSize;

        DrawingAttributes.Height = BrushSize;
            
    }

    private void UpdateColor()
    {
        CurrentColor = ColorFromHSV(Hue, Saturation, Brightness);
        DrawingAttributes.Color = CurrentColor;
        RectangleFill=new SolidColorBrush(CurrentColor);
       
        

        }


    private void UpdateOpacity(double value)
    {
        
    }
        
        public void OnBrushSizeChanged(double newValue)
        {
            BrushSize = newValue;
            UpdateBrush();
        }
        public void OnHueChanged(double newValue)
        {
            Hue = newValue;
            UpdateColor();
        }

        public void OnSaturationChanged(double newValue)
        {
            Saturation = newValue;
            UpdateColor();
        }

        public void OnBrightnessChanged(double newValue)
        {
            Brightness = newValue;
            UpdateColor();
        }

        public void OnOpacityChanged(double newValue)
        {
            Opacity = newValue;
        }
        private Color ColorFromHSV(double hue, double saturation, double brightness)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        double f = hue / 60 - Math.Floor(hue / 60);

        brightness = brightness * 255;
        byte v = Convert.ToByte(brightness);
        byte p = Convert.ToByte(brightness * (1 - saturation));
        byte q = Convert.ToByte(brightness * (1 - f * saturation));
        byte t = Convert.ToByte(brightness * (1 - (1 - f) * saturation));

        if (hi == 0)
            return Color.FromRgb(v, t, p);
        else if (hi == 1)
            return Color.FromRgb(q, v, p);
        else if (hi == 2)
            return Color.FromRgb(p, v, t);
        else if (hi == 3)
            return Color.FromRgb(p, q, v);
        else if (hi == 4)
            return Color.FromRgb(t, p, v);
        else
            return Color.FromRgb(v, p, q);
    }
        
       
}
}
