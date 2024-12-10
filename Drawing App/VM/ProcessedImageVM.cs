using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Drawing_App.Model;
using Prism.Commands;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media;

namespace Drawing_App.VM
{
    public class ProcessedImageVM : BindableBase
    {
        private ObservableCollection<ImageLayer> _images;
        private int _currentIndex;
        private BitmapImage _currentImage;
        private string _red, _green, _blue;

        public string Red
        {
            get => _red;
            set => SetProperty(ref _red, value);
        }

        public string Green
        {
            get => _green;
            set => SetProperty(ref _green, value);
        }

        public string Blue
        {
            get => _blue;
            set => SetProperty(ref _blue, value);
        }

        public ObservableCollection<ImageLayer> Images
        {
            get => _images;
            set => SetProperty(ref _images, value);
        }

        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                SetProperty(ref _currentIndex, value);
                UpdateCurrentImage();
            }
        }

        public BitmapImage CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value);
        }

        public ICommand NextImageCommand { get; }
        public ICommand PreviousImageCommand { get; }
        public ICommand ImageClickCommand { get; }

        public ProcessedImageVM()
        {
            Images = new ObservableCollection<ImageLayer>();
            CurrentIndex = 0;
            ImageClickCommand = new DelegateCommand<Point?>(OnImageClick);
            NextImageCommand = new DelegateCommand(NextImage);
            PreviousImageCommand = new DelegateCommand(PreviousImage);

            Red = "0";
            Green = "0";
            Blue = "0";
        }

        private void OnImageClick(Point? clickPosition)
        {
            if (clickPosition == null)
            {
                Debug.WriteLine("Click position is null");
                return;
            }

            if (CurrentImage == null)
            {
                Debug.WriteLine("CurrentImage is null");
                return;
            }

            var point = clickPosition.Value;

            // Adjust for DPI scaling
            var dpiScale = VisualTreeHelper.GetDpi(Application.Current.MainWindow);
            int x = (int)(point.X * dpiScale.DpiScaleX);
            int y = (int)(point.Y * dpiScale.DpiScaleY);

            WriteableBitmap writeableBitmap = new WriteableBitmap(CurrentImage);

            Debug.WriteLine($"Click Position: X={x}, Y={y}");
            Debug.WriteLine($"Image Dimensions: Width={writeableBitmap.PixelWidth}, Height={writeableBitmap.PixelHeight}");
            Debug.WriteLine($"Pixel Format: {writeableBitmap.Format}");

            // Ensure click position is within bounds
            if (x < 0 || x >= writeableBitmap.PixelWidth || y < 0 || y >= writeableBitmap.PixelHeight)
            {
                Debug.WriteLine("Click position out of bounds");
                Red = Green = Blue = "Out of Bounds";
                return;
            }

            // Define stride and pixel data
            int bytesPerPixel = (writeableBitmap.Format.BitsPerPixel + 7) / 8;
            int stride = writeableBitmap.PixelWidth * bytesPerPixel;
            byte[] pixelData = new byte[bytesPerPixel];

            // Define rectangle and copy pixel data
            Int32Rect rect = new Int32Rect(x, y, 1, 1);
            writeableBitmap.CopyPixels(rect, pixelData, stride, 0);

            Debug.WriteLine($"Pixel Data Length: {pixelData.Length}");

            // Extract RGB values
            byte blue = pixelData[0];
            byte green = pixelData[1];
            byte red = pixelData[2];
            Red = red.ToString();
            Green = green.ToString();
            Blue = blue.ToString();

            Debug.WriteLine($"Extracted Colors - R: {Red}, G: {Green}, B: {Blue}");
        }


        public void LoadImages(IEnumerable<string> filePaths)
        {
            Images.Clear();
            foreach (var filePath in filePaths)
            {
                Images.Add(new ImageLayer(filePath));
            }
            CurrentIndex = 0;
            UpdateCurrentImage();
        }

        private void UpdateCurrentImage()
        {
            if (Images.Count > 0 && CurrentIndex >= 0 && CurrentIndex < Images.Count)
            {
                CurrentImage = new BitmapImage(new Uri(Images[CurrentIndex]._filePath));
            }
        }

        private void NextImage()
        {
            if (CurrentIndex < Images.Count - 1)
            {
                CurrentIndex++;
            }
        }

        private void PreviousImage()
        {
            if (CurrentIndex > 0)
            {
                CurrentIndex--;
            }
        }
    }

}
