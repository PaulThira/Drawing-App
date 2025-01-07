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
using Emgu.CV.Dnn;
using System.Reflection.Emit;
using System.Windows.Media.Media3D;

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
        public GeometricTransformations geometricTransformations { get; set; }
        public BlendingModes blendingMode { get; set; }
        public override double ZoomLevel { get => base.ZoomLevel; set => base.ZoomLevel = value; }
        public bool lasso {  get; set; }
        public bool gradient {  get; set; }
        public Point first {  get; set; }
        public Point last { get; set; }
        public List<Point> pointsProjective {  get; set; }
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
            geometricTransformations = new GeometricTransformations();
            lasso=false;
            blendingMode=new BlendingModes();
            gradient=false;
            first=new Point();
            last=new Point();
            pointsProjective = new List<Point>();


        }
        public ImageLayer(BitmapImage image, double imageWidth = 665, double imageHeight = 563, double opacity = 1.0, bool isVisible = true, string name = "Layer") : base(opacity, isVisible, name)
        {
            pointsProjective = new List<Point>();
            _image = new Image
            {
                Source = image,
                Width = imageWidth,
                Height = imageHeight
            };
            _filePath = "";
            Bgr = ConvertImageToEmguImage(_image);

            basicOperations = new BasicOperations();
            _image.MouseDown += Image_MouseDown;
            _image.MouseMove += Image_MouseMove;
            _image.MouseUp += Image_MouseUp;
            Point = new Point();
            pointwiseOperations = new PointwiseOperations();
            thresholding = new Thresholding();
            highPass = new HighPass();
            lowPass = new LowPass();
            morphological = new MorphologicalOperations();
            segmentation = new Segmentation();
            geometricTransformations = new GeometricTransformations();
            lasso=false;
            blendingMode=new BlendingModes();
            gradient=false;
            first=new Point(); last=new Point();
        }
        public ImageLayer(Image<Bgr,byte> image, double imageWidth = 665, double imageHeight = 563, double opacity = 1.0, bool isVisible = true, string name = "Layer") : base(opacity, isVisible, name)
        {
            pointsProjective = new List<Point>();
            var k=ConvertToBitmapSource(image);
            _image = new Image
            {
                Source = k,
                Width = imageWidth,
                Height = imageHeight
            };
            _filePath = "";
            Bgr = image;

            basicOperations = new BasicOperations();
            _image.MouseDown += Image_MouseDown;
            _image.MouseMove += Image_MouseMove;
            _image.MouseUp += Image_MouseUp;
            Point = new Point();
            pointwiseOperations = new PointwiseOperations();
            thresholding = new Thresholding();
            highPass = new HighPass();
            lowPass = new LowPass();
            morphological = new MorphologicalOperations();
            segmentation = new Segmentation();
            geometricTransformations = new GeometricTransformations();
            lasso = false;
            blendingMode = new BlendingModes();
            gradient = false;
            first = new Point(); last = new Point();
        }
        public void AdjustContrast(double contrast)
        {
            Image<Bgr, byte> result = basicOperations.AdjustContrast(Bgr, contrast);
            var os = ConvertImageToBitmapImage(result);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void AdjustBrightness(int bright)
        {
            Image<Bgr, byte> result = basicOperations.AdjustBrightness(Bgr, bright);
            var os = ConvertImageToBitmapImage(result);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void Sepia()
        {
            Image<Bgr, byte> result = basicOperations.ApplySepia(Bgr);
            var os = ConvertImageToBitmapImage(result);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void PerspectiveWrap()
        {
            List<Point> sourcePoints = new List<Point>();
            List<Point> destinationPoints = new List<Point>();
            if (pointsProjective.Count >= 8)
            {
                for(int i = 0; i < 4; i++)
                {
                    sourcePoints.Add(pointsProjective[i]);
                    destinationPoints.Add(pointsProjective[i+4]);
                }
                Image<Bgr, byte> result = geometricTransformations.PerspectiveWrap(Bgr, sourcePoints, destinationPoints);
                var os = ConvertImageToBitmapImage(result);
                ProcessedImage p = new ProcessedImage(os);
                p.ShowDialog();
                if (p.DialogResult == true)
                {
                    _image.Source = p.Image;
                    Bgr = ConvertImageToEmguImage(_image);
                }
            }
            else
            {
                MessageBox.Show("Pick more points!");
            }
            
        }
        public List<Color> GradientColors()
        {
            
            if (first.X < Bgr.Width && first.Y < Bgr.Height && last.X < Bgr.Width && last.Y < Bgr.Height)
            {
                if (first.X >=0 && first.Y >=0 && last.X >=0 && last.Y >=0)
                {
                    Bgr c1 = Bgr[(int)first.Y,(int)first.X];
                    Bgr c2 = Bgr[(int)last.Y, (int)last.X];
                    List<Color> gradient = new List<Color>();

                    for (int i = 0; i < 5; i++)
                    {
                        float t = (float)i / (5 - 1);
                        byte blue = (byte)(c1.Blue + t * (c2.Blue - c1.Blue));
                        byte green = (byte)(c1.Green + t * (c2.Green - c1.Green));
                        byte red = (byte)(c1.Red + t * (c2.Red - c1.Red));
                        var col = new SolidColorBrush(Color.FromArgb(255, red, green, blue));
                      
                        gradient.Add(Color.FromArgb(255, red, green, blue));
                    }
                    return gradient;
                }
                return null;
            }
            return null;
        }
        public void Multiply(ImageLayer layer)
        {
            Image<Bgr, byte> result = blendingMode.Multiply(Bgr, layer.Bgr);
            var os = ConvertImageToBitmapImage(result);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void Screen(ImageLayer layer)
        {
            Image<Bgr, byte> result = blendingMode.Screen(Bgr, layer.Bgr);
            var os = ConvertImageToBitmapImage(result);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void Overlay(ImageLayer layer)
        {
            Image<Bgr, byte> result = blendingMode.Overlay(Bgr, layer.Bgr);
            var os = ConvertImageToBitmapImage(result);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void Add(ImageLayer layer)
        {
            Image<Bgr, byte> result = blendingMode.Add(Bgr, layer.Bgr);
            var os = ConvertImageToBitmapImage(result);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void Substract(ImageLayer layer)
        {
            Image<Bgr, byte> result = blendingMode.Substract(Bgr, layer.Bgr);
            var os = ConvertImageToBitmapImage(result);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void Difference(ImageLayer layer)
        {
            Image<Bgr, byte> result = blendingMode.Difference(Bgr, layer.Bgr);
            var os = ConvertImageToBitmapImage(result);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void Lighten(ImageLayer layer)
        {
            Image<Bgr, byte> result = blendingMode.Lighten(Bgr, layer.Bgr);
            var os = ConvertImageToBitmapImage(result);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void Darken(ImageLayer layer)
        {
            Image<Bgr, byte> result = blendingMode.Darken(Bgr, layer.Bgr);
            var os = ConvertImageToBitmapImage(result);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void SoftLight(ImageLayer layer)
        {
            Image<Bgr, byte> result = blendingMode.SoftLight(Bgr, layer.Bgr);
            var os = ConvertImageToBitmapImage(result);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void HardLight(ImageLayer layer)
        {
            Image<Bgr, byte> result = blendingMode.HardLight(Bgr, layer.Bgr);
            var os = ConvertImageToBitmapImage(result);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void Divide(ImageLayer layer)
        {
            Image<Bgr, byte> result = blendingMode.Divide(Bgr, layer.Bgr);
            var os = ConvertImageToBitmapImage(result);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void ColorBurn(ImageLayer layer)
        {
            Image<Bgr, byte> result = blendingMode.ColorBurn(Bgr, layer.Bgr);
            var os = ConvertImageToBitmapImage(result);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void ColorDoge(ImageLayer layer)
        {
            Image<Bgr, byte> result = blendingMode.ColorDoge(Bgr, layer.Bgr);
            var os = ConvertImageToBitmapImage(result);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void Exclusion(ImageLayer layer)
        {
            Image<Bgr, byte> result = blendingMode.Exclusion(Bgr, layer.Bgr);
            var os = ConvertImageToBitmapImage(result);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void OtsuThresholding()
        {
            Image<Gray, byte> grays = thresholding.OtsuThresholding(Bgr);
            var os = ConvertToBitmapSource(grays);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }
        }
        public void AffineTransformation(float[,] matrix)
        {
            Image<Bgr,byte> result=geometricTransformations.AffineTransformation(Bgr, matrix);
            var os = ConvertImageToBitmapImage(result);
            ProcessedImage p = new ProcessedImage(os);
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }

        }
        private BitmapImage ConvertImageToBitmapImage(Image<Bgr, byte> image)
        {
            // Create a memory stream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Save the Image<Bgr, byte> to the memory stream in BMP format
                image.ToBitmap().Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);

                // Reset the position of the stream to the beginning
                memoryStream.Seek(0, SeekOrigin.Begin);

                // Create a BitmapImage from the memory stream
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();

                return bitmapImage;
            }
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
        public DrawingLayer ConvertToDrawingLayer(ICommand StartStroke,ICommand ContinueStroke,ICommand EndStroke)
        {
            Canvas canvas = ConvertImageToCanvas(_image);
            return new DrawingLayer(canvas,StartStroke,ContinueStroke,EndStroke);

        }
        public Canvas ConvertImageToCanvas(Image originalImage)
        {
            // Create a new Canvas
            Image clonedImage = new Image
            {
                Source = originalImage.Source, // Copy the source
                Width = originalImage.Width,
                Height = originalImage.Height,
                Stretch = originalImage.Stretch
            };

            // Create a new Canvas
            Canvas canvas = new Canvas
            {
                Width = clonedImage.Width,  // Set canvas width
                Height = clonedImage.Height // Set canvas height
            };

            // Add the cloned Image to the Canvas
            canvas.Children.Add(clonedImage);

            // Position the Image at the top-left corner of the Canvas
            Canvas.SetLeft(clonedImage, 0);
            Canvas.SetTop(clonedImage, 0);

            return canvas;
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
            p.ShowDialog();
            if (p.DialogResult == true)
            {
                _image.Source = p.Image;
                Bgr = ConvertImageToEmguImage(_image);
            }

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
            if (e.LeftButton == MouseButtonState.Pressed&&lasso== true  )
            {
                // Start the lasso
                _isLassoActive = true;
                _lassoPoints.Clear(); // Clear previous selection
                Point startPoint = e.GetPosition((UIElement)sender);
                _lassoPoints.Add(startPoint);
                Console.WriteLine($"Lasso started at: {startPoint}");
            }
            if(e.RightButton== MouseButtonState.Pressed&& gradient == true)
            {
                last=first;
                first= e.GetPosition((UIElement)sender);
               
            }

            pointsProjective.Add(e.GetPosition((IInputElement)sender));

        }
        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isLassoActive && e.LeftButton == MouseButtonState.Released&&lasso== true)
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
            if (_isLassoActive && e.LeftButton == MouseButtonState.Pressed&&lasso == true)
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
            zoomWindow.ShowDialog();

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
