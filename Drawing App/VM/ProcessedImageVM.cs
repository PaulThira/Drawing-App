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

namespace Drawing_App.VM
{
    public class ProcessedImageVM: BindableBase
    {
        private ObservableCollection<ImageLayer> _images;
        private int _currentIndex;
        private BitmapImage _currentImage;
        private string _red;

        private string _green;
        private string _blue;
        public string Red
        {
            get { return _red; }
            set
            {
                SetProperty(ref _red, value);
                 // Optional: If you want to update something based on color changes
            }
        }    
        public string Green
        {
            get { return _green; }
            set
            {
                SetProperty(ref _green, value);
                 // Optional: If you want to update something based on color changes
            }
        }
        public string Blue
        {
            get { return _blue; }
            set
            {
                SetProperty(ref _blue, value);
                 // Optional: If you want to update something based on color changes
            }
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
            if (clickPosition != null)
            {
                var point = clickPosition.Value;
                int x, y;
                x = (int)point.X;
                y=(int)point.Y;
                WriteableBitmap writeableBitmap = new WriteableBitmap(CurrentImage);

                // Ensure the x and y coordinates are within the bounds of the image
                if (x < 0 || x >= writeableBitmap.PixelWidth || y < 0 || y >= writeableBitmap.PixelHeight)
                    throw new ArgumentOutOfRangeException("The pixel coordinates are outside the image bounds.");

                // Define the stride (number of bytes per row) and pixel format (assuming 32 bits per pixel)
                int bytesPerPixel = (writeableBitmap.Format.BitsPerPixel + 7) / 8;
                int stride = writeableBitmap.PixelWidth * bytesPerPixel;

                // Create a buffer to hold the pixel data (1 pixel = 4 bytes for BGR32)
                byte[] pixelData = new byte[bytesPerPixel];

                // Define the rectangle for the single pixel you want to copy
                Int32Rect rect = new Int32Rect(x, y, 1, 1);

                // Copy the pixel data into the buffer
                writeableBitmap.CopyPixels(rect, pixelData, stride, 0);

                // Extract the color components from the pixel data (assuming BGR32 format)
                if (pixelData.Length == 1)
                {
                    byte blue = pixelData[0];
                    byte green = pixelData[0];
                    byte red = pixelData[0];
                    byte alpha = pixelData[0];
                    Red = red.ToString();
                    Green = green.ToString();
                    Blue = blue.ToString();
                }
                else
                {
                    byte blue = pixelData[0];
                    byte green = pixelData[1];
                    byte red = pixelData[2];
                    byte alpha = pixelData[3];
                    Red = red.ToString();
                    Green = green.ToString();
                    Blue = blue.ToString();
                }
                


            }
        }
        public void LoadImages(IEnumerable<string> filePaths)
        {
            foreach (var filePath in filePaths)
            {
                Images.Add(new ImageLayer(filePath));
            }

            UpdateCurrentImage();
        }

        private void UpdateCurrentImage()
        {
            if (Images.Count > 0 && CurrentIndex >= 0 && CurrentIndex < Images.Count)
            {
                CurrentImage = new BitmapImage(new Uri(Images[CurrentIndex]._filePath));
            }
        }

        public ICommand NextImageCommand { get; }
        public ICommand PreviousImageCommand { get; }

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
