﻿using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Drawing_App.Model;
using ColorPoint = Drawing_App.Model.ColorPoint;
using System.Windows;
using System.Windows.Input;
using Emgu.CV.Cuda;

namespace Drawing_App.VM
{
    public class ColorPalleteGeneratorVM:BindableBase
    {
        private const int Radius = 200;
        private const int ColorCount = 360;
        private double _saturation;
        private double _value;
        private double _hue;
        private SolidColorBrush _selectedColor;
        private string _h;
        private string _s;
        private string _v;
       
        public string h
        {
            get => _h;
            set => SetProperty(ref _h, value);
        }

        public string s
        {
            get => _s;
            set => SetProperty(ref _s, value);
        }

        public string v
        {
            get => _v;
            set => SetProperty(ref _v, value);
        }
        public SolidColorBrush SelectedColor
        {
            get  => _selectedColor; 
            set { SetProperty(ref _selectedColor, value); }
        }
     
        public HSVColours HSVColourss { get; set; } = new HSVColours();// Define how many unique colors you want on the wheel
        public ObservableCollection<ColorPoint> ColorPoints { get; set; }
        public DelegateCommand GenerateColorWheelCommand { get; }
        public LinearGradientBrush SaturationValueGradient
        {
            get
            {
                // Create a diagonal gradient starting from white (top-left) to black (bottom-right)
                var gradientBrush = new LinearGradientBrush();
                gradientBrush.StartPoint = new Point(0, 0);
                gradientBrush.EndPoint = new Point(1, 1);

                gradientBrush.GradientStops.Add(new GradientStop(Colors.White, 0.0));   // Top-left corner
                gradientBrush.GradientStops.Add(new GradientStop(Colors.Black, 1.0));   // Bottom-right corner
                HSVColourss = new HSVColours();
                return gradientBrush;
            }
        }
        private ObservableCollection<Brush> _selectedPalette;
        public ObservableCollection<Brush> SelectedPalette
        {
            get => _selectedPalette;
            set => SetProperty(ref _selectedPalette, value);
        }
        public ICommand ColorSelectedCommand { get; }
        public ICommand AddColorCommand { get; }
        public ICommand ColorSchemeCommand { get; }
        public ColorPalleteGeneratorVM() {

            ColorSchemeCommand = new DelegateCommand<string?>(ColorScheme);
            ColorPoints = new ObservableCollection<ColorPoint>();
            GenerateColorWheelCommand = new DelegateCommand(GenerateColorWheel);
            ColorSelectedCommand = new DelegateCommand<ColorPoint>(OnColorSelected);
            SelectedColor = new SolidColorBrush(Colors.Cyan);
            AddColorCommand = new DelegateCommand(AddColor);
            SelectedPalette = new ObservableCollection<Brush>();
            
            h = "0";
            s = "0";
            v = "0";
            // Generate color wheel on initialization
            GenerateColorWheel();
        }
        private void ColorScheme(string? i)
        {
            if (i == "0") {
                ColorPoint c = (ColorPoint)(ColorPoints.FirstOrDefault(e => e.Color.Color == SelectedColor.Color));

                if (c != null)
                {
                    // Get the current hue, saturation, and value
                    var currentHue = (double)HSVColourss.BGRtoHSV(c.Color.Color).Item1;
                    var currentSaturation = (double)HSVColourss.BGRtoHSV(c.Color.Color).Item2;
                    var currentValue = (double)HSVColourss.BGRtoHSV(c.Color.Color).Item3;

                    // Calculate the opposite hue
                    var Hue1 = (currentHue + 120) % 360;
                    var Hue2 = (currentHue + 240) % 360;

                    // Generate the opposite color using the HSV values
                    Color Color1 = HSVColourss.ColorFromHSV(Hue1, currentSaturation, currentValue);
                    Color Color2 = HSVColourss.ColorFromHSV(Hue2, currentSaturation, currentValue);

                    // Find the nearest matching color in the ColorPoints collection
                    ColorPoint op1 = ColorPoints.FirstOrDefault(e => IsColorSimilar(e.Color.Color, Color1));
                    ColorPoint op2 = ColorPoints.FirstOrDefault(e => IsColorSimilar(e.Color.Color, Color2));

                    if (op2 != null&& op1!=null)
                    {
                        int index1 = ColorPoints.IndexOf(op1);
                        int index2 = ColorPoints.IndexOf(op2);

                        if (index1 != -1&& index2!=-1)
                        {
                           var colored= ColorPoints[index1];
                            SelectedPalette.Add(new SolidColorBrush(colored.Color.Color));
                            var coloured2= ColorPoints[index2];
                            SelectedPalette.Add(new SolidColorBrush(coloured2.Color.Color));    
                        }
                    }
                }
            }
            else if (i == "1")
            {
                ColorPoint c = (ColorPoint)(ColorPoints.FirstOrDefault(e => e.Color.Color == SelectedColor.Color));

                if (c != null)
                {
                    // Get the current hue, saturation, and value
                    var currentHue = (double)HSVColourss.BGRtoHSV(c.Color.Color).Item1;
                    var currentSaturation = (double)HSVColourss.BGRtoHSV(c.Color.Color).Item2;
                    var currentValue = (double)HSVColourss.BGRtoHSV(c.Color.Color).Item3;

                    // Calculate the opposite hue
                    var Hue1 = (currentHue + 180) % 360;
                  

                    // Generate the opposite color using the HSV values
                    Color Color1 = HSVColourss.ColorFromHSV(Hue1, currentSaturation, currentValue);
                  

                    // Find the nearest matching color in the ColorPoints collection
                    ColorPoint op1 = ColorPoints.FirstOrDefault(e => IsColorSimilar(e.Color.Color, Color1));
                   

                    if ( op1 != null)
                    {
                        int index1 = ColorPoints.IndexOf(op1);
                      

                        if (index1 != -1 )
                        {
                            var colored = ColorPoints[index1];
                            SelectedPalette.Add(new SolidColorBrush(colored.Color.Color));
                            
                        }
                    }
                }
            }
            else if (i == "2")
            {
                ColorPoint c = (ColorPoint)(ColorPoints.FirstOrDefault(e => e.Color.Color == SelectedColor.Color));

                if (c != null)
                {
                    // Get the current hue, saturation, and value
                    var currentHue = (double)HSVColourss.BGRtoHSV(c.Color.Color).Item1;
                    var currentSaturation = (double)HSVColourss.BGRtoHSV(c.Color.Color).Item2;
                    var currentValue = (double)HSVColourss.BGRtoHSV(c.Color.Color).Item3;

                    // Calculate the opposite hue
                    var Hue1 = (currentHue + 190) % 360;
                    var Hue2 = (currentHue + 170) % 360;

                    // Generate the opposite color using the HSV values
                    Color Color1 = HSVColourss.ColorFromHSV(Hue1, currentSaturation, currentValue);
                    Color Color2 = HSVColourss.ColorFromHSV(Hue2, currentSaturation, currentValue);

                    // Find the nearest matching color in the ColorPoints collection
                    ColorPoint op1 = ColorPoints.FirstOrDefault(e => IsColorSimilar(e.Color.Color, Color1));
                    ColorPoint op2 = ColorPoints.FirstOrDefault(e => IsColorSimilar(e.Color.Color, Color2));

                    if (op2 != null && op1 != null)
                    {
                        int index1 = ColorPoints.IndexOf(op1);
                        int index2 = ColorPoints.IndexOf(op2);

                        if (index1 != -1 && index2 != -1)
                        {
                            var colored = ColorPoints[index1];
                            SelectedPalette.Add(new SolidColorBrush(colored.Color.Color));
                            var coloured2 = ColorPoints[index2];
                            SelectedPalette.Add(new SolidColorBrush(coloured2.Color.Color));
                        }
                    }
                }
            }
            if (i == "3")
            {
                ColorPoint c = (ColorPoint)(ColorPoints.FirstOrDefault(e => e.Color.Color == SelectedColor.Color));

                if (c != null)
                {
                    // Get the current hue, saturation, and value
                    var currentHue = (double)HSVColourss.BGRtoHSV(c.Color.Color).Item1;
                    var currentSaturation = (double)HSVColourss.BGRtoHSV(c.Color.Color).Item2;
                    var currentValue = (double)HSVColourss.BGRtoHSV(c.Color.Color).Item3;

                    // Calculate the opposite hue
                    var Hue1 = (currentHue + 10) % 360;
                    var Hue2 = (currentHue + 350) % 360;

                    // Generate the opposite color using the HSV values
                    Color Color1 = HSVColourss.ColorFromHSV(Hue1, currentSaturation, currentValue);
                    Color Color2 = HSVColourss.ColorFromHSV(Hue2, currentSaturation, currentValue);

                    // Find the nearest matching color in the ColorPoints collection
                    ColorPoint op1 = ColorPoints.FirstOrDefault(e => IsColorSimilar(e.Color.Color, Color1));
                    ColorPoint op2 = ColorPoints.FirstOrDefault(e => IsColorSimilar(e.Color.Color, Color2));

                    if (op2 != null && op1 != null)
                    {
                        int index1 = ColorPoints.IndexOf(op1);
                        int index2 = ColorPoints.IndexOf(op2);

                        if (index1 != -1 && index2 != -1)
                        {
                            var colored = ColorPoints[index1];
                            SelectedPalette.Add(new SolidColorBrush(colored.Color.Color));
                            var coloured2 = ColorPoints[index2];
                            SelectedPalette.Add(new SolidColorBrush(coloured2.Color.Color));
                            
                        }
                    }
                }
            }
            if (i == "4")
            {
                ColorPoint c = (ColorPoint)(ColorPoints.FirstOrDefault(e => e.Color.Color == SelectedColor.Color));

                if (c != null)
                {
                    // Get the current hue, saturation, and value
                    var currentHue = (double)HSVColourss.BGRtoHSV(c.Color.Color).Item1;
                    var currentSaturation = (double)HSVColourss.BGRtoHSV(c.Color.Color).Item2;
                    var currentValue = (double)HSVColourss.BGRtoHSV(c.Color.Color).Item3;

                    // Calculate the opposite hue
                    var Hue1 = (currentHue + 90) % 360;
                    var Hue2 = (currentHue + 180) % 360;
                    var Hue3 = (currentHue + 270) % 360;

                    // Generate the opposite color using the HSV values
                    Color Color1 = HSVColourss.ColorFromHSV(Hue1, currentSaturation, currentValue);
                    Color Color2 = HSVColourss.ColorFromHSV(Hue2, currentSaturation, currentValue);
                    Color Color3 = HSVColourss.ColorFromHSV(Hue3, currentSaturation, currentValue);

                    // Find the nearest matching color in the ColorPoints collection
                    ColorPoint op1 = ColorPoints.FirstOrDefault(e => IsColorSimilar(e.Color.Color, Color1));
                    ColorPoint op2 = ColorPoints.FirstOrDefault(e => IsColorSimilar(e.Color.Color, Color2));
                    ColorPoint op3 = ColorPoints.FirstOrDefault(e => IsColorSimilar(e.Color.Color, Color3));

                    if (op2 != null && op1 != null&& op3!=null)
                    {
                        int index1 = ColorPoints.IndexOf(op1);
                        int index2 = ColorPoints.IndexOf(op2);
                        int index3 = ColorPoints.IndexOf(op3);

                        if (index1 != -1 && index2 != -1&&index3!=-1)
                        {
                            var colored = ColorPoints[index1];
                            SelectedPalette.Add(new SolidColorBrush(colored.Color.Color));
                            var coloured2 = ColorPoints[index2];
                            SelectedPalette.Add(new SolidColorBrush(coloured2.Color.Color));
                            var coloured3 = ColorPoints[index3];
                            SelectedPalette.Add(new SolidColorBrush(coloured3.Color.Color));
                        }
                    }
                }
            }
        }
        private bool IsColorSimilar(Color color1, Color color2, int tolerance = 10)
        {
            return Math.Abs(color1.R - color2.R) <= tolerance &&
                   Math.Abs(color1.G - color2.G) <= tolerance &&
                   Math.Abs(color1.B - color2.B) <= tolerance;
        }
        private void OnColorSelected(ColorPoint selectedColorPoint)
        {
            // Perform the action when a color is selected
             SelectedColor = selectedColorPoint.Color;
            _hue=(double)HSVColourss.BGRtoHSV(SelectedColor.Color).Item1;
            h = _hue.ToString();
            // Do something with the selected color (e.g., update UI or selected color)
        }
        private void AddColor()
        {
            // Add a new color to the palette (for example, a random color)
            SelectedPalette.Add(SelectedColor);
        }
        public void SelectSaturationValue(double x, double y, double width, double height)
        {
            // Normalize the coordinates to [0, 1] for both x and y
            _saturation = x / width;
            _value = 1 - (y / height); // Invert Y for value (top is high, bottom is low)
            s=_saturation.ToString();
            v= _value.ToString();
            // Assuming hue is fixed here; you can integrate this with a color wheel or hue slider
           // Set a default hue (you can bind this to your color wheel selection)

            // Update the selected color
            SelectedColor = new SolidColorBrush(HSVColourss.ColorFromHSV(_hue, _saturation, _value));
        }
        private void GenerateColorWheel()
        {
            ColorPoints.Clear();

            // Generate colors based on the HSV color wheel
            for (int i = 0; i < ColorCount; i++)
            {
                // Calculate the angle for placing the color
                double angle = i * (360.0 / ColorCount);
                double radians = angle * (Math.PI / 180.0);

                // Position for the color based on angle and radius
                double x = Radius + (Radius * Math.Cos(radians));
                double y = Radius + (Radius * Math.Sin(radians));

                // Convert the angle to a color in the HSV model
                var hsvColor = HSVColourss.ColorFromHSV((double)i, 1.0, 1.0);  // Full saturation, full value for bright colors

                // Add the color and position to the collection
                ColorPoints.Add(new ColorPoint
                {
                    X = x,
                    Y = y,
                    Color = new SolidColorBrush(hsvColor)
                });
            }
        }

       
    }
}