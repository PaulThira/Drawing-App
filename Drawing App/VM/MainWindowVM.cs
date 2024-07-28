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


        public MainWindowVM()

        {
            DrawingElements = new ObservableCollection<UIElement>();
            _currentColor = Colors.Cyan;
            StartStrokeCommand = new DelegateCommand<Point?>(StartStroke);
            ContinueStrokeCommand = new DelegateCommand<Point?>(ContinueStroke);
            EndStrokeCommand = new DelegateCommand(EndStroke);
            
            RectangleFill = Brushes.Cyan;
            _usedColors = new HashSet<Color>();
            _colorPalette = new ObservableCollection<Color>();
            SaveCommand = new DelegateCommand<InkCanvas>(SaveCall);
            EraserCommand = new DelegateCommand(EraserCall);
            
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
        }
        private void StartStroke(Point? startPoint)
        {
            if (startPoint == null) return;

            _currentPolyline = new Polyline
            {
                Stroke = RectangleFill,
                StrokeThickness = BrushSize,
                Points = new PointCollection { startPoint.Value }
            };
            DrawingElements.Add(_currentPolyline);
        }

        private void ContinueStroke(Point? currentPoint)
        {
            if (currentPoint == null || _currentPolyline == null) return;
            _currentPolyline.Points.Add(currentPoint.Value);
        }

        private void EndStroke()
        {
            
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
            DrawingElements.RemoveAt(DrawingElements.Count-1);
        }

        private void SaveCall(InkCanvas inkCanvas)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image|*.png";
            if (saveFileDialog.ShowDialog() == true)
            {
                string filename = saveFileDialog.FileName;
                int width = (int)InkCanvasWidth;
                int height = (int)InkCanvasHeight;

                RenderTargetBitmap renderBitmap = new RenderTargetBitmap(width, height, 96d, 96d, PixelFormats.Pbgra32);
                DrawingVisual dv = new DrawingVisual();
                using (DrawingContext dc = dv.RenderOpen())
                {
                    VisualBrush vb = new VisualBrush(inkCanvas);
                    dc.DrawRectangle(vb, null, new Rect(new System.Windows.Point(), new System.Windows.Size(width, height)));
                }
                renderBitmap.Render(dv);

                using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                    encoder.Save(fs);
                }

                MessageBox.Show($"Image saved to {filename}", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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
            RectangleFill = new SolidColorBrush(CurrentColor);
        }

    private void UpdateOpacity(double value)
    {
        // Update inkCanvas opacity logic can be handled here
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
        public void HandleStrokeCollected(Stroke stroke)
        {
            if (DrawingAttributes is TexturedBrushes texturedBrushes)
            {
                var textureBrush = texturedBrushes.GetTextureBrush();
                if (textureBrush != null)
                {
                    var stylusPoints = stroke.StylusPoints;
                    var texturedStroke = new TexturedStroke(stylusPoints, textureBrush);
                    stroke = texturedStroke;
                }
            }
        }
        public void HandleStrokesChanged(StrokeCollectionChangedEventArgs e)
    {
            if (_usedColors.Count > 7)
            {
                _usedColors.Clear();
            }
            foreach (var stroke in e.Added)
            {
            if (_usedColors.Add(stroke.DrawingAttributes.Color))
            {
                ColorPalette.Add(stroke.DrawingAttributes.Color);
            }
            }
            int i = 0;
            List<Color> colors = new List<Color>(); 
            foreach(var color in _usedColors)
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
            HSVColours h=new HSVColours();
            h.Colors = colors;
            h.ColorHarmony();
        }
}
}
