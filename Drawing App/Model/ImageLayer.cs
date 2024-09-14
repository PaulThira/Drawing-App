using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Algorithms.Sections;
using Emgu.CV;
using Emgu.CV.Reg;
using Emgu.CV.Structure;
using Drawing_App.View;
using System.IO;
using Bitmap = System.Drawing.Bitmap;
using System.Drawing.Imaging;

namespace Drawing_App.Model
{
    public class ImageLayer : Layer
    {
        private readonly Image _image;

        public Image<Bgr,Byte> Bgr { get; set; }
         
        public override UIElement VisualElement => _image;
        public string _filePath {  get; set; }
        public BasicOperations basicOperations { get; set; }
        public override double ZoomLevel { get => base.ZoomLevel; set => base.ZoomLevel = value; }
        public void CalculateHistogram()
        {
            basicOperations.HistogramCalc(Bgr);
            Histogram histogram =new Histogram(basicOperations.red, basicOperations.green, basicOperations.blue);
            histogram.Show();

        }

        public ImageLayer(string imagePath, double imageWidth=665, double imageHeight=563, double opacity = 1.0, bool isVisible = true, string name="Layer")
            : base(opacity, isVisible, name)
        {
            _image = new Image
            {
                Source = new BitmapImage(new Uri(imagePath)),
                Width = imageWidth,
                Height = imageHeight
            };
            _filePath = imagePath;
            Bgr= ConvertImageToEmguImage(_image);

            basicOperations = new BasicOperations();

        }

        public override void ZoomIn()
        {
            ZoomLevel += 0.1;
            _image.LayoutTransform = new ScaleTransform(ZoomLevel, ZoomLevel);
            
        }

        public override void ZoomOut()
        {
            ZoomLevel = Math.Max(0.1, ZoomLevel - 0.1);
            _image.LayoutTransform = new ScaleTransform(ZoomLevel, ZoomLevel);
            
        }
        private Image<Bgr, byte> ConvertImageToEmguImage(Image wpfImage)
        {
            // Get BitmapSource from the Image control's Source property
            BitmapSource bitmapSource = wpfImage.Source as BitmapSource;
            if (bitmapSource == null)
            {
                throw new InvalidOperationException("Image source is not a BitmapSource.");
            }

            // Create an empty Image<Bgr, byte> of the same dimensions as the BitmapSource
            Image<Bgr, byte> emguImage = new Image<Bgr, byte>(bitmapSource.PixelWidth, bitmapSource.PixelHeight);

            // Extract pixel data from BitmapSource
            byte[] rawPixelData = ExtractPixelData(bitmapSource);

            // Initialize the Image<Bgr, byte> pixel by pixel
            for (int y = 0; y < bitmapSource.PixelHeight; y++)
            {
                for (int x = 0; x < bitmapSource.PixelWidth; x++)
                {
                    int pixelIndex = (y * bitmapSource.PixelWidth + x) * 4; // 4 bytes per pixel (BGR + Alpha)

                    // Set the pixel values (BGR) in the EmguCV image
                    emguImage.Data[y, x, 0] = rawPixelData[pixelIndex];     // Blue channel
                    emguImage.Data[y, x, 1] = rawPixelData[pixelIndex + 1]; // Green channel
                    emguImage.Data[y, x, 2] = rawPixelData[pixelIndex + 2]; // Red channel
                                                                            // Alpha channel (rawPixelData[pixelIndex + 3]) is ignored here
                }
            }

            return emguImage;
        }
        private byte[] ExtractPixelData(BitmapSource bitmapSource)
        {
            // Calculate the stride (the width in bytes of a single row of pixels)
            int bytesPerPixel = (bitmapSource.Format.BitsPerPixel + 7) / 8;
            int stride = bitmapSource.PixelWidth * bytesPerPixel;

            // Create a buffer to hold the pixel data
            byte[] rawPixelData = new byte[bitmapSource.PixelHeight * stride];

            // Copy the pixel data into the buffer
            bitmapSource.CopyPixels(rawPixelData, stride, 0);

            return rawPixelData;
        }

        // Extract pixel data from BitmapSource into a byte[,,] array (BGR format)

        public override void Redo()
        {
            
        }
        public override void Undo()
        {

        }
    }
}
