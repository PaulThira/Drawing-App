using Drawing_App.Model;
using ImageMagick;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Drawing_App.VM
{
    public class PixelArtVM:BindableBase
    {
        private string _rows = "16";
        private string _columns = "16";
        private string _hue = "0";
        private string _saturation = "0";
        private string _value = "1";
        private SolidColorBrush _selectedColor;

        public string Rows
        {
            get => _rows;
            set => SetProperty(ref _rows, value);
        }
        private Color _currentColor;

        // Property with notification support
        public Color CurrentColor
        {
            get => _currentColor;
            set => SetProperty(ref _currentColor, value); // Notify UI of changes
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
        public ICommand ChangeColorCommand { get; set; }
        public ICommand SaveGridAsImageCommand { get;  }
        public PixelArtVM()
        {
            ApplyChangesCommand = new DelegateCommand(OnApplyChanges);
            PixelGrid = new ObservableCollection<Pixelz>();
            UpdateSelectedColor();
            GeneratePixelGrid();
            ChangeColorCommand = new DelegateCommand<Tuple<int?, int?>>(ChangeColor);
            SaveGridAsImageCommand = new DelegateCommand(SaveGridAsImage);

        }
        private void SaveGridAsImage()
        {
            int pixelWidth = colz * 10; // Width of the image (10px per cell)
            int pixelHeight = rowz * 10; // Height of the image (10px per cell)
            var dpi = 96;

            // Create a WriteableBitmap
            var bitmap = new WriteableBitmap(pixelWidth, pixelHeight, dpi, dpi, PixelFormats.Pbgra32, null);

            // Lock the bitmap for writing
            bitmap.Lock();

            foreach (var pixel in PixelGrid)
            {
                if (pixel.X.HasValue && pixel.Y.HasValue)
                {
                    int x = pixel.X.Value * 10;
                    int y = pixel.Y.Value * 10;

                    // Get the color from the PixelGrid
                    Color color = pixel.Colorz.Color;

                    // Fill the corresponding area with the color
                    for (int i = 0; i < 10; i++) // Cell size 10x10
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            int pixelX = x + i;
                            int pixelY = y + j;

                            if (pixelX < pixelWidth && pixelY < pixelHeight)
                            {
                                // Calculate pixel address
                                IntPtr pBackBuffer = bitmap.BackBuffer + pixelY * bitmap.BackBufferStride + pixelX * 4;

                                // Set pixel color
                                int colorData = (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
                                System.Runtime.InteropServices.Marshal.WriteInt32(pBackBuffer, colorData);
                            }
                        }
                    }
                }
            }

            bitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, pixelWidth, pixelHeight));
            bitmap.Unlock();

            // Save the bitmap
            SaveBitmapToFile(bitmap);
        }

        private void SaveBitmapToFile(WriteableBitmap bitmap)
        {
            // Open Save File Dialog
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp",
                Title = "Save PixelGrid as Image",
                FileName = "PixelGrid"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                // Choose encoder based on file extension
                BitmapEncoder encoder = null;
                if (filePath.EndsWith(".png"))
                    encoder = new PngBitmapEncoder();
                else if (filePath.EndsWith(".jpg"))
                    encoder = new JpegBitmapEncoder();
                else if (filePath.EndsWith(".bmp"))
                    encoder = new BmpBitmapEncoder();

                if (encoder != null)
                {
                    // Encode the WriteableBitmap
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }

                    MessageBox.Show("Image saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Unsupported file format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void ChangeColor(Tuple<int?,int?> pos)
        {
            PixelGrid[pos.Item1.Value * rowz + pos.Item2.Value].Colorz = new SolidColorBrush(CurrentColor);
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
                    PixelGrid.Add(new Pixelz { X = row, Y = col, Colorz = new SolidColorBrush(CurrentColor) });

                }
            }
        }

        private void OnApplyChanges()
        {
            rowz = int.Parse(Rows); // Convert Rows to integer
            colz = int.Parse(Columns); // Convert Columns to integer

            // Create a copy of the current PixelGrid as a list
            List<Pixelz> pixels = PixelGrid.ToList();

            // Clear the existing PixelGrid
            PixelGrid.Clear();

            // Populate the new grid
            for (int row = 0; row < rowz; row++)
            {
                for (int col = 0; col < colz; col++)
                {
                    int index = row * colz + col; // Calculate the index in the original list
                    if (index < pixels.Count)
                    {
                        // Use the existing pixel's color
                        PixelGrid.Add(new Pixelz
                        {
                            X = row,
                            Y = col,
                            Colorz = new SolidColorBrush(pixels[index].Colorz.Color)
                        });
                    }
                    else
                    {
                        // Use the default CurrentColor for new pixels
                        PixelGrid.Add(new Pixelz
                        {
                            X = row,
                            Y = col,
                            Colorz = new SolidColorBrush(CurrentColor)
                        });
                    }
                }
            }
        }


        private void UpdateSelectedColor()
        {
            var h=double.Parse(Hue);
            var sat=double.Parse(Saturation);
            var val=double.Parse(Value);
            SelectedColor = new SolidColorBrush(ColorFromHSV(h, sat, val));
            CurrentColor=ColorFromHSV(h, sat, val);
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
