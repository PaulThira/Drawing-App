using Drawing_App.Model;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Drawing_App.VM
{
    public class PixelArtVM:BindableBase
    {
        private string _rows = "16";
        private string _columns = "16";
        private string _hue = "0";
        private string _saturation = "1";
        private string _value = "1";
        private SolidColorBrush _selectedColor;

        public string Rows
        {
            get => _rows;
            set => SetProperty(ref _rows, value);
        }

        public string Columns
        {
            get => _columns;
            set => SetProperty(ref _columns, value);
        }

        public string Hue
        {
            get => _hue;
            set
            {
                if (SetProperty(ref _hue, value))
                {
                    UpdateSelectedColor();
                }
            }
        }

        public string Saturation
        {
            get => _saturation;
            set
            {
                if (SetProperty(ref _saturation, value))
                {
                    UpdateSelectedColor();
                }
            }
        }

        public string Value
        {
            get => _value;
            set
            {
                if (SetProperty(ref _value, value))
                {
                    UpdateSelectedColor();
                }
            }
        }

        public SolidColorBrush SelectedColor
        {
            get => _selectedColor;
            private set => SetProperty(ref _selectedColor, value);
        }
        public int rowz { get; set; } = 16;
        public int colz { get; set; } = 16;
        public ICommand ApplyChangesCommand { get; }
        public ObservableCollection<Pixelz> PixelGrid { get; set; }
        public PixelArtVM()
        {
            ApplyChangesCommand = new DelegateCommand(OnApplyChanges);
            PixelGrid = new ObservableCollection<Pixelz>();
            UpdateSelectedColor();
            GeneratePixelGrid();
        }
        private void GeneratePixelGrid()
        {
            PixelGrid.Clear();
            var r = int.Parse(Rows);
            var c = int.Parse(Rows);
            rowz=r; colz=c;
            for (int row = 0; row < r; row++)
            {
                for (int col = 0; col < c; col++)
                {
                    PixelGrid.Add(new Pixelz { X = row, Y = col, Colorz = new SolidColorBrush(Colors.White) });
                }
            }
        }
        private void OnApplyChanges()
        {
            // Handle logic for applying changes to the grid dimensions
            // This could involve notifying the view to redraw the grid
        }

        private void UpdateSelectedColor()
        {
            var h=double.Parse(Hue);
            var sat=double.Parse(Saturation);
            var val=double.Parse(Value);
            SelectedColor = new SolidColorBrush(ColorFromHSV(h, sat, val));
        }

        private Color ColorFromHSV(double hue, double saturation, double value)
        {
            double chroma = value * saturation;
            double x = chroma * (1 - Math.Abs((hue / 60) % 2 - 1));
            double m = value - chroma;

            double r = 0, g = 0, b = 0;

            if (hue >= 0 && hue < 60) { r = chroma; g = x; b = 0; }
            else if (hue >= 60 && hue < 120) { r = x; g = chroma; b = 0; }
            else if (hue >= 120 && hue < 180) { r = 0; g = chroma; b = x; }
            else if (hue >= 180 && hue < 240) { r = 0; g = x; b = chroma; }
            else if (hue >= 240 && hue < 300) { r = x; g = 0; b = chroma; }
            else if (hue >= 300 && hue < 360) { r = chroma; g = 0; b = x; }

            return Color.FromRgb(
                (byte)((r + m) * 255),
                (byte)((g + m) * 255),
                (byte)((b + m) * 255)
            );
        }
    }
}
