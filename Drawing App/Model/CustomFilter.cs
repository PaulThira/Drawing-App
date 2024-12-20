using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Drawing_App.Model
{
    public class CustomFilter
    {
        public string Name { get; set; }
        public double[,] Kernel { get; set; }
        public double Brightness { get; set; }
        public double Contrast { get; set; }
        public double Saturation { get; set; }
        public double Sharpness { get; set; }
        public double BlurRadius { get; set; }
        public double EdgeIntensity { get; set; }
        public ICommand ApplyFilterCommand { get; }
        Image<Bgr,byte> BgrImage { get; set; }
        public CustomFilter(string name, double[,] kernel, double brightness, double contrast, double saturation, double sharpness, double blurRadius, double edgeIntensity)
        {
            Name = name;
            Kernel = kernel;
            Brightness = brightness;
            Contrast = contrast;
            Saturation = saturation;
            Sharpness = sharpness;
            BlurRadius = blurRadius;
            EdgeIntensity = edgeIntensity;
            ApplyFilterCommand = new DelegateCommand(ApplyFilter);
        }
        private void ApplyFilter()
        {
            // Logic to apply the filter to the image
            ApplyToImage(BgrImage);
        }
        public Image<Bgr, byte> ApplyToImage(Image<Bgr, byte> image)
        {
            BgrImage = image;   
            if (Kernel != null)
            {
                Kernel = NormalizeKernel(Kernel);
              

                image = ApplyKernelManually(image, Kernel);
            }

            image = AdjustBrightnessAndContrastManually(image, Brightness, Contrast);
            image = AdjustSaturationManually(image, Saturation);

            if (Sharpness > 0)
            {
                image = AdjustSharpness(image, Sharpness);
            }

            if (BlurRadius > 0)
            {
                image = ApplyBlur(image, BlurRadius);
            }

            if (EdgeIntensity > 0)
            {
                image = SimplifiedEdgeDetection(image, EdgeIntensity);
            }

            return image;
        }
        private double[,] NormalizeKernel(double[,] kernel)
        {
            double sum = 0;

            for (int y = 0; y < kernel.GetLength(0); y++)
            {
                for (int x = 0; x < kernel.GetLength(1); x++)
                {
                    sum += kernel[y, x];
                }
            }

            if (sum == 0) sum = 1; // Prevent division by zero

            for (int y = 0; y < kernel.GetLength(0); y++)
            {
                for (int x = 0; x < kernel.GetLength(1); x++)
                {
                    kernel[y, x] /= sum;
                }
            }

            return kernel;
        }

        private Image<Bgr, byte> ApplyKernelManually(Image<Bgr, byte> image, double[,] kernel)
        {
            int kernelWidth = kernel.GetLength(1);
            int kernelHeight = kernel.GetLength(0);
            int halfKernelWidth = kernelWidth / 2;
            int halfKernelHeight = kernelHeight / 2;

            var result = new Image<Bgr, byte>(image.Width, image.Height);

            for (int y = halfKernelHeight; y < image.Height - halfKernelHeight; y++)
            {
                for (int x = halfKernelWidth; x < image.Width - halfKernelWidth; x++)
                {
                    double[] newColor = new double[3]; // B, G, R channels
                    for (int ky = 0; ky < kernelHeight; ky++)
                    {
                        for (int kx = 0; kx < kernelWidth; kx++)
                        {
                            int pixelX = x + kx - halfKernelWidth;
                            int pixelY = y + ky - halfKernelHeight;

                            var pixelColor = image[pixelY, pixelX];
                            double kernelValue = kernel[ky, kx];

                            newColor[0] += pixelColor.Blue * kernelValue;
                            newColor[1] += pixelColor.Green * kernelValue;
                            newColor[2] += pixelColor.Red * kernelValue;
                        }
                    }

                    // Clamp colors to byte range (0-255)
                    result[y, x] = new Bgr(
                        Math.Min(Math.Max(newColor[0], 0), 255),
                        Math.Min(Math.Max(newColor[1], 0), 255),
                        Math.Min(Math.Max(newColor[2], 0), 255)
                    );
                }
            }

            return result;
        }
        private Image<Bgr, byte> AdjustSharpness(Image<Bgr, byte> image, double sharpness)
        {
            // High-pass kernel
            double[,] highPassKernel = {
        { -1, -1, -1 },
        { -1,  8, -1 },
        { -1, -1, -1 }
    };

            // Scale kernel with sharpness
            for (int i = 0; i < highPassKernel.GetLength(0); i++)
            {
                for (int j = 0; j < highPassKernel.GetLength(1); j++)
                {
                    highPassKernel[i, j] *= sharpness;
                }
            }

            var highPassResult = ApplyKernelManually(image, highPassKernel);
            var result = new Image<Bgr, byte>(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var originalPixel = image[y, x];
                    var highPassPixel = highPassResult[y, x];

                    // Combine original with high-pass result
                    byte blue = (byte)Math.Min(Math.Max(originalPixel.Blue + highPassPixel.Blue, 0), 255);
                    byte green = (byte)Math.Min(Math.Max(originalPixel.Green + highPassPixel.Green, 0), 255);
                    byte red = (byte)Math.Min(Math.Max(originalPixel.Red + highPassPixel.Red, 0), 255);

                    result[y, x] = new Bgr(blue, green, red);
                }
            }

            return result;
        }

        private Image<Bgr, byte> ApplyBlur(Image<Bgr, byte> image, double blurIntensity)
        {
            // Simple average kernel for approximation
            double[,] blurKernel = {
        { 1, 1, 1 },
        { 1, 1, 1 },
        { 1, 1, 1 }
    };

            // Normalize kernel
            for (int i = 0; i < blurKernel.GetLength(0); i++)
            {
                for (int j = 0; j < blurKernel.GetLength(1); j++)
                {
                    blurKernel[i, j] /= 9.0;
                }
            }

            // Scale kernel with blur intensity
            for (int i = 0; i < blurKernel.GetLength(0); i++)
            {
                for (int j = 0; j < blurKernel.GetLength(1); j++)
                {
                    blurKernel[i, j] *= blurIntensity;
                }
            }

            return ApplyKernelManually(image, blurKernel);
        }

        private Image<Bgr, byte> SimplifiedEdgeDetection(Image<Bgr, byte> image, double edgeIntensity)
        {
            // Sobel kernels
            double[,] sobelX = {
        { -1, 0, 1 },
        { -2, 0, 2 },
        { -1, 0, 1 }
    };
            double[,] sobelY = {
        { -1, -2, -1 },
        { 0,  0,  0 },
        { 1,  2,  1 }
    };

            var gradientX = ApplyKernelManually(image, sobelX);
            var gradientY = ApplyKernelManually(image, sobelY);

            var result = new Image<Bgr, byte>(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var pixelX = gradientX[y, x];
                    var pixelY = gradientY[y, x];

                    // Compute gradient magnitude
                    byte blue = (byte)Math.Min(Math.Sqrt(pixelX.Blue * pixelX.Blue + pixelY.Blue * pixelY.Blue) * edgeIntensity, 255);
                    byte green = (byte)Math.Min(Math.Sqrt(pixelX.Green * pixelX.Green + pixelY.Green * pixelY.Green) * edgeIntensity, 255);
                    byte red = (byte)Math.Min(Math.Sqrt(pixelX.Red * pixelX.Red + pixelY.Red * pixelY.Red) * edgeIntensity, 255);

                    result[y, x] = new Bgr(blue, green, red);
                }
            }

            return result;
        }


        private Image<Bgr, byte> AdjustBrightnessAndContrastManually(Image<Bgr, byte> image, double brightness, double contrast)
        {
            var result = new Image<Bgr, byte>(image.Width, image.Height);

            // Normalize contrast (e.g., 50 means no change, range: 0-100)
            double contrastFactor = (contrast - 50) / 50.0; // Contrast range (-1 to 1)
            brightness -= 50; // Brightness range (-50 to 50)

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var pixel = image[y, x];

                    // Apply contrast and brightness
                    double blue = pixel.Blue / 255.0;
                    double green = pixel.Green / 255.0;
                    double red = pixel.Red / 255.0;

                    blue = ((blue - 0.5) * (1 + contrastFactor) + 0.5) * 255 + brightness;
                    green = ((green - 0.5) * (1 + contrastFactor) + 0.5) * 255 + brightness;
                    red = ((red - 0.5) * (1 + contrastFactor) + 0.5) * 255 + brightness;

                    // Clamp values to valid range
                    result[y, x] = new Bgr(
                        Math.Min(Math.Max(blue, 0), 255),
                        Math.Min(Math.Max(green, 0), 255),
                        Math.Min(Math.Max(red, 0), 255)
                    );
                }
            }

            return result;
        }


        private Image<Bgr, byte> AdjustSaturationManually(Image<Bgr, byte> image, double saturation)
        {
            var result = new Image<Bgr, byte>(image.Width, image.Height);

            double saturationFactor = saturation / 100.0; // Convert to range 0 to 1

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var pixel = image[y, x];

                    // Convert to grayscale for luminance
                    double luminance = 0.299 * pixel.Red + 0.587 * pixel.Green + 0.114 * pixel.Blue;

                    // Apply saturation adjustment
                    double blue = luminance + (pixel.Blue - luminance) * (1 + saturationFactor);
                    double green = luminance + (pixel.Green - luminance) * (1 + saturationFactor);
                    double red = luminance + (pixel.Red - luminance) * (1 + saturationFactor);

                    // Clamp values
                    result[y, x] = new Bgr(
                        Math.Min(Math.Max(blue, 0), 255),
                        Math.Min(Math.Max(green, 0), 255),
                        Math.Min(Math.Max(red, 0), 255)
                    );
                }
            }

            return result;
        }


        // Optional: Default constructor for serialization/deserialization
        public CustomFilter() { }

        // Method to save the filter to a file
        
    }
}
