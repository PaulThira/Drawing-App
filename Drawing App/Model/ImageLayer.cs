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

using System.Drawing.Imaging;
using System.Windows.Input;

namespace Drawing_App.Model
{
    public class ImageLayer : Layer
    {
        private readonly Image _image;

        public Image<Bgr, Byte> Bgr { get; set; }
        public Point Point { get; set; }
        public override UIElement VisualElement => _image;
        public string _filePath { get; set; }
        public BasicOperations basicOperations { get; set; }
        public Thresholding thresholding { get; set; }
        public PointwiseOperations pointwiseOperations { get; set; }
        public HighPass highPass { get; set; }
        public MorphologicalOperations morphological {get;set;}
        public LowPass lowPass { get; set; }
        public Segmentation segmentation { get; set; }
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
            _image.MouseDown += Image_MouseDown;
            _image.MouseMove += Image_MouseMove;
            _image.MouseUp += Image_MouseUp;
            Point= new Point();
            pointwiseOperations = new PointwiseOperations();
            thresholding = new Thresholding();
            highPass = new HighPass();
            lowPass = new LowPass();
            morphological = new MorphologicalOperations();
            segmentation = new Segmentation();

        }
        public void MagicTool(Color color,int T)
        {
            Image<Bgr, byte> o = segmentation.MagicTool(Bgr,color,T);
            var os = ConvertToBitmapSource(o);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }

        }
        public void Watershed()
        {
            Image<Gray, byte> i = segmentation.Watershed(Bgr);
            var os = ConvertToBitmapSource(i);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void Canny(byte T1,byte T2)
        {
            Image<Gray, byte> i = lowPass.Canny(Bgr,T1,T2);
            var os = ConvertToBitmapSource(i);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void Opening(int i)
        {
            Image<Bgr,byte> o=morphological.Opening(Bgr,i,i);
            var os=ConvertToBitmapSource(o);
            ProcessedImage p=new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }

        }
        public void Sobel()
        {
            Image<Gray, byte> i = lowPass.Sobel(Bgr);
            var os = ConvertToBitmapSource(i);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }

        }
        public void Closing(int i)
        {
            Image<Bgr, byte> o = morphological.Closing(Bgr, i, i);
            var os = ConvertToBitmapSource(o);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void GausianBlurr()
        {
            Image<Bgr,byte> blurr=highPass.GausianBlur(Bgr);
            var blurs=ConvertToBitmapSource(blurr);
            ProcessedImage p = new ProcessedImage(blurs);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }

        }
        public void FastMedianFilter(int k)
        {
            Image<Bgr, byte> blurr = highPass.FastMedianFilter(Bgr,k);
            var fm = ConvertToBitmapSource(blurr);
            ProcessedImage p = new ProcessedImage(fm);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void GrayscaleConversion()
        {
            Image<Gray,byte> gray=basicOperations.BgrToGrayscale(Bgr);
            var grays=ConvertToBitmapSource(gray);
            ProcessedImage p = new ProcessedImage(grays);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }

        }
        public void Thresholding(int g)
        {
            Image<Gray, byte> gray = thresholding.InputThresholding(Bgr,g);
            var grays = ConvertToBitmapSource(gray);
            ProcessedImage p = new ProcessedImage(grays);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void TriangleThresholding()
        {
            Image<Gray,byte>gray=thresholding.Triangle(Bgr);
            var grays = ConvertToBitmapSource(gray);
            ProcessedImage p = new ProcessedImage( grays);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void HistogramEqualisation()
        {
            Image<Bgr,byte> result=basicOperations.HistogramEqualisation(Bgr);
            var results = ConvertToBitmapSource(Bgr);
            ProcessedImage processedImage = new ProcessedImage(results);
            processedImage.ShowDialog();
            if (processedImage.DialogResult == true)
            {
                _image.Source = processedImage.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public static BitmapSource ConvertToBitmapSource(Image<Gray, byte> grayImage)
        {
            // Get the width, height, and stride of the image
            int width = grayImage.Width;
            int height = grayImage.Height;
            int stride = width; // Gray image, so 1 byte per pixel

            // Get a pointer to the image data
            IntPtr ptr = grayImage.Mat.DataPointer;

            // Create a new BitmapSource
            BitmapSource bitmapSource = BitmapSource.Create(
                width,
                height,
                96, 96,  // DPI
                System.Windows.Media.PixelFormats.Gray8, // Pixel format for a Gray image
                null,  // No palette needed for Gray images
                ptr,
                stride * height,
                stride);

            return bitmapSource;
        }
        private List<Point> _lassoPoints = new List<Point>();
        private bool _isLassoActive = false;
        public void ApplySplinedTransformation(int[] blue, int[] green, int[] red)
        {
            pointwiseOperations.blue=blue; pointwiseOperations.green=green; pointwiseOperations.red=red;
           var result= pointwiseOperations.ApplyLUT(Bgr);
            ProcessedImage p=new ProcessedImage(ConvertToBitmapSource(result));
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void ApplyNegative()
        {
            var result=basicOperations.Negative(Bgr);

            ProcessedImage p = new ProcessedImage(ConvertToBitmapSource(result));
            p.Show();
        }
        public static BitmapSource ConvertToBitmapSource(Image<Bgr, byte> bgrImage)
        {
            // Get the width, height, and stride of the image
            int width = bgrImage.Width;
            int height = bgrImage.Height;
            int stride = width * 3; // 3 bytes per pixel (BGR format)

            // Get a pointer to the image data
            IntPtr ptr = bgrImage.Mat.DataPointer;

            // Create a new BitmapSource
            BitmapSource bitmapSource = BitmapSource.Create(
                width,
                height,
                96, 96,  // DPI
                System.Windows.Media.PixelFormats.Bgr24, // Pixel format for a BGR image (8 bits per channel, 3 channels)
                null,  // No palette needed for BGR images
                ptr,
                stride * height,
                stride);

            return bitmapSource;
        }


        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point = e.GetPosition((UIElement)sender);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Start the lasso
                _isLassoActive = true;
                _lassoPoints.Clear(); // Clear previous selection
                Point startPoint = e.GetPosition((UIElement)sender);
                _lassoPoints.Add(startPoint);
                Console.WriteLine($"Lasso started at: {startPoint}");
            }


        }
        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isLassoActive && e.LeftButton == MouseButtonState.Released)
            {
                // Finalize the lasso
                _isLassoActive = false;

                // Optionally close the loop
                if (_lassoPoints.Count > 1)
                {
                    _lassoPoints.Add(_lassoPoints[0]); // Close the loop
                }

                Console.WriteLine("Lasso finalized");
                ApplyMask();
            }
        }
        private void Image_MouseMove(object sender, MouseEventArgs e) {
            if (_isLassoActive && e.LeftButton == MouseButtonState.Pressed)
            {
                // Add points to the lasso path
                Point currentPoint = e.GetPosition((UIElement)sender);
                _lassoPoints.Add(currentPoint);
                Console.WriteLine($"Lasso point added: {currentPoint}");

                // Optional: Render the lasso boundary visually
                RenderBoundary();
            }
        }
        public void ZoomedPixels()
        {
            int x = (int)Point.X;
            int y = (int)Point.Y;
            GetZoomedPixels(x, y);
        }
        private void GetZoomedPixels(int x, int y)
        {
            // Assuming 'image' is your Image<Bgr, byte> or a similar bitmap object
            int[,,] surroundingPixels = new int[3, 3,3];

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int newX = x + i;
                    int newY = y + j;

                    if (IsValidPixel(newX, newY)) // Check if the pixel is within bounds
                    {
                        // Get pixel color (you could adapt this to extract RGB values)
                        surroundingPixels[i + 1, j + 1,0] = Bgr.Data[newY, newX, 0];
                        surroundingPixels[i + 1, j + 1, 1] = Bgr.Data[newY, newX, 1];
                        surroundingPixels[i + 1, j + 1, 2] = Bgr.Data[newY, newX, 2]; // Assuming BGR image
                    }
                }
            }
            BitmapSource zoomedBitmap = CreateZoomedBitmap(surroundingPixels, 10); // Zoom factor of 10

            // Display the zoomed view
            var zoomWindow = new ProcessedImage(zoomedBitmap);
            zoomWindow.Show();

            // Display the zoomed view

        }
        private BitmapSource CreateZoomedBitmap(int[,,] surroundingPixels, int zoomFactor)
        {
            int gridSize = surroundingPixels.GetLength(0); // Should be 3 in this case (3x3 grid)
            int zoomedWidth = gridSize * zoomFactor;
            int zoomedHeight = gridSize * zoomFactor;

            // Create a WriteableBitmap to hold the zoomed-in image
            WriteableBitmap zoomedBitmap = new WriteableBitmap(zoomedWidth, zoomedHeight, 96, 96, PixelFormats.Bgr32, null);

            // Create a DrawingVisual for overlaying text (RGB values)
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                zoomedBitmap.Lock();

                // Loop through each pixel in the surroundingPixels array
                for (int i = 0; i < gridSize; i++)
                {
                    for (int j = 0; j < gridSize; j++)
                    {
                        // Extract BGR values from the surroundingPixels array
                        byte blue = (byte)surroundingPixels[i, j, 0]; // Blue channel
                        byte green = (byte)surroundingPixels[i, j, 1]; // Green channel
                        byte red = (byte)surroundingPixels[i, j, 2]; // Red channel

                        // Create a Color using the BGR values
                        System.Windows.Media.Color pixelColor = System.Windows.Media.Color.FromRgb(red, green, blue);

                        // Draw the pixel on the WriteableBitmap, zoomed by the zoomFactor
                        DrawZoomedPixel(zoomedBitmap, pixelColor, i * zoomFactor, j * zoomFactor, zoomFactor);

                        // Draw the RGB values on the pixel
                        DrawRgbText(drawingContext, red, green, blue, i * zoomFactor, j * zoomFactor, zoomFactor);
                    }
                }

                zoomedBitmap.Unlock();

                // Finish drawing text overlay
                drawingContext.Close();
            }

            // Combine the WriteableBitmap (zoomed pixels) with the DrawingVisual (text)
            return CombineBitmapWithVisual(zoomedBitmap, drawingVisual);
        }

        private void DrawRgbText(DrawingContext drawingContext, byte red, byte green, byte blue, int x, int y, int zoomFactor)
        {
            // Create the RGB text
            string rgbText = $"R:{red} G:{green} B:{blue}";

            // Set the font and brush for the text
            var typeface = new Typeface("Arial");
            var fontSize = zoomFactor / 3; // Scale the font size according to the zoom factor to fit inside the pixel
            var textBrush = Brushes.Black;

            // Position the text inside the zoomed pixel, with a small margin
            var textPosition = new Point(x + zoomFactor * 0.1, y + zoomFactor * 0.3);  // Position the text inside the pixel block

            // Draw the text on the DrawingContext
            drawingContext.DrawText(
                new FormattedText(rgbText,
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    typeface, fontSize, textBrush, 1.25),
                textPosition);
        }

        private BitmapSource CombineBitmapWithVisual(WriteableBitmap bitmap, DrawingVisual visual)
        {
            // Create a RenderTargetBitmap to combine the bitmap and text overlay
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                bitmap.PixelWidth, bitmap.PixelHeight, bitmap.DpiX, bitmap.DpiY, PixelFormats.Pbgra32);

            // Create a DrawingVisual to combine both
            using (DrawingContext context = visual.RenderOpen())
            {
                context.DrawImage(bitmap, new Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            }

            // Render the combined image
            renderBitmap.Render(visual);
            return renderBitmap;
        }


        private void DrawZoomedPixel(WriteableBitmap bitmap, System.Windows.Media.Color color, int x, int y, int zoomFactor)
        {
            // Convert the color to a byte array
            byte[] pixelData = { color.B, color.G, color.R, color.A };

            // Fill a zoomed square for the pixel
            for (int i = 0; i < zoomFactor; i++)
            {
                for (int j = 0; j < zoomFactor; j++)
                {
                    bitmap.WritePixels(new Int32Rect(x + i, y + j, 1, 1), pixelData, 4, 0);
                }
            }
        }

        public void RenderBoundary()
        {
            // Create a DrawingVisual for rendering the boundary
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                // Create a pen for the boundary (customize color and thickness)
                Pen boundaryPen = new Pen(Brushes.Red, 2);

                // Draw the boundary polygon
                PathFigure pathFigure = new PathFigure
                {
                    StartPoint = _lassoPoints[0],
                    IsClosed = true,
                    IsFilled = false
                };

                foreach (var point in _lassoPoints.Skip(1))
                {
                    pathFigure.Segments.Add(new LineSegment(point, true));
                }

                PathGeometry geometry = new PathGeometry(new[] { pathFigure });
                context.DrawGeometry(null, boundaryPen, geometry);
            }

            // Convert the DrawingVisual to an ImageSource
            RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)_image.Width, (int)_image.Height, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(visual);

            // Display the boundary as an overlay
            _image.Source = new DrawingImage(visual.Drawing);
        }

        public void ApplyMask()
        {
            // Create a mask with the same size as the image
            Image<Gray, byte> mask = new Image<Gray, byte>(Bgr.Size);

            // Convert List<Point> to Emgu.CV.Structure.PointF array
            var contour = new Emgu.CV.Util.VectorOfPoint(
                _lassoPoints.Select(p => new System.Drawing.Point((int)p.X, (int)p.Y)).ToArray()
            );

            // Wrap the contour in a VectorOfVectorOfPoint
            var contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
            contours.Push(contour);

            // Fill the mask with white inside the boundary
            CvInvoke.FillPoly(mask, contours, new Gray(255).MCvScalar);

            // Apply the mask to the image
            var result = new Image<Bgr, byte>(Bgr.Size);

            for (int y = 0; y < Bgr.Height; y++)
            {
                for (int x = 0; x < Bgr.Width; x++)
                {
                    // If the mask is white, keep the original pixel
                    if (mask.Data[y, x, 0] == 255)
                    {
                        result.Data[y, x, 0] = Bgr.Data[y, x, 0]; // Blue
                        result.Data[y, x, 1] = Bgr.Data[y, x, 1]; // Green
                        result.Data[y, x, 2] = Bgr.Data[y, x, 2]; // Red
                    }
                    else
                    {
                        // Otherwise, set the pixel to white
                        result.Data[y, x, 0] = 255; // Blue
                        result.Data[y, x, 1] = 255; // Green
                        result.Data[y, x, 2] = 255; // Red
                    }
                }
            }


            // Display the masked image
            ProcessedImage p = new ProcessedImage(ConvertToBitmapSource(result));
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source=p.Image;
                Bgr=ConvertImageToEmguImage(_image);
            }
        }



        private bool IsValidPixel(int x, int y)
        {
            return (x >= 0 && x < _image.Width && y >= 0 && y < _image.Height);
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

            // Get image properties
            int width = bitmapSource.PixelWidth;
            int height = bitmapSource.PixelHeight;

            // Calculate the stride (the number of bytes in a single row, including padding)
            int bytesPerPixel = (bitmapSource.Format.BitsPerPixel + 7) / 8;
            int stride = width * bytesPerPixel;

            // Extract pixel data from BitmapSource
            byte[] rawPixelData = new byte[height * stride+2];
            bitmapSource.CopyPixels(rawPixelData, stride, 0);

            // Create an EmguCV Image<Bgr, byte> of the same dimensions
            Image<Bgr, byte> emguImage = new Image<Bgr, byte>(width, height);

            // Map pixel data to the EmguCV image
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Calculate the pixel index considering the stride
                    int pixelIndex = y * stride + x * bytesPerPixel;

                    // Set the pixel values (BGR channels) in the EmguCV image
                    emguImage.Data[y, x, 0] = rawPixelData[pixelIndex];     // Blue
                    emguImage.Data[y, x, 1] = rawPixelData[pixelIndex + 1]; // Green
                    emguImage.Data[y, x, 2] = rawPixelData[pixelIndex + 2]; // Red
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
