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
                image = AdjustEdgeIntensity(image, EdgeIntensity);
            }

            return image;
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
            // Kernel for Laplacian operator
            double[,] laplacianKernel = {
        { 0, -1, 0 },
        { -1, 4, -1 },
        { 0, -1, 0 }
    };

            // Apply Laplacian kernel
            var edgeImage = ApplyKernelManually(image, laplacianKernel);

            // Combine original image with edges
            var result = new Image<Bgr, byte>(image.Width, image.Height);
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var originalPixel = image[y, x];
                    var edgePixel = edgeImage[y, x];

                    // Adjust sharpness by mixing original and edge image
                    byte blue = (byte)Math.Min(Math.Max(originalPixel.Blue + sharpness * edgePixel.Blue, 0), 255);
                    byte green = (byte)Math.Min(Math.Max(originalPixel.Green + sharpness * edgePixel.Green, 0), 255);
                    byte red = (byte)Math.Min(Math.Max(originalPixel.Red + sharpness * edgePixel.Red, 0), 255);

                    result[y, x] = new Bgr(blue, green, red);
                }
            }

            return result;
        }
        private Image<Bgr, byte> ApplyBlur(Image<Bgr, byte> image, double blurRadius)
        {
            // Generate Gaussian kernel based on blur radius
            int size = (int)(2 * blurRadius + 1); // Kernel size based on radius
            double[,] gaussianKernel = GenerateGaussianKernel(size, blurRadius);

            // Apply Gaussian kernel
            return ApplyKernelManually(image, gaussianKernel);
        }

        private double[,] GenerateGaussianKernel(int size, double sigma)
        {
            double[,] kernel = new double[size, size];
            double sum = 0;
            int halfSize = size / 2;

            for (int y = -halfSize; y <= halfSize; y++)
            {
                for (int x = -halfSize; x <= halfSize; x++)
                {
                    double value = Math.Exp(-(x * x + y * y) / (2 * sigma * sigma)) / (2 * Math.PI * sigma * sigma);
                    kernel[y + halfSize, x + halfSize] = value;
                    sum += value;
                }
            }

            // Normalize kernel
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    kernel[y, x] /= sum;
                }
            }

            return kernel;
        }
        private Image<Bgr, byte> AdjustEdgeIntensity(Image<Bgr, byte> image, double edgeIntensity)
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

            // Apply Sobel kernels
            var gradientX = ApplyKernelManually(image, sobelX);
            var gradientY = ApplyKernelManually(image, sobelY);

            // Combine gradients and scale
            var result = new Image<Bgr, byte>(image.Width, image.Height);
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var pixelX = gradientX[y, x];
                    var pixelY = gradientY[y, x];

                    // Magnitude of gradient
                    byte blue = (byte)Math.Min(Math.Max(edgeIntensity * Math.Sqrt(pixelX.Blue * pixelX.Blue + pixelY.Blue * pixelY.Blue), 0), 255);
                    byte green = (byte)Math.Min(Math.Max(edgeIntensity * Math.Sqrt(pixelX.Green * pixelX.Green + pixelY.Green * pixelY.Green), 0), 255);
                    byte red = (byte)Math.Min(Math.Max(edgeIntensity * Math.Sqrt(pixelX.Red * pixelX.Red + pixelY.Red * pixelY.Red), 0), 255);

                    result[y, x] = new Bgr(blue, green, red);
                }
            }

            return result;
        }

        private Image<Bgr, byte> AdjustBrightnessAndContrastManually(Image<Bgr, byte> image, double brightness, double contrast)
        {
            var result = new Image<Bgr, byte>(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var pixelColor = image[y, x];

                    // Apply contrast and brightness adjustments
                    byte blue = (byte)Math.Min(Math.Max((pixelColor.Blue * contrast) + brightness, 0), 255);
                    byte green = (byte)Math.Min(Math.Max((pixelColor.Green * contrast) + brightness, 0), 255);
                    byte red = (byte)Math.Min(Math.Max((pixelColor.Red * contrast) + brightness, 0), 255);

                    result[y, x] = new Bgr(blue, green, red);
                }
            }

            return result;
        }

        private Image<Bgr, byte> AdjustSaturationManually(Image<Bgr, byte> image, double saturation)
        {
            var result = new Image<Bgr, byte>(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var pixelColor = image[y, x];

                    // Convert BGR to grayscale to get luminance
                    double luminance = 0.299 * pixelColor.Red + 0.587 * pixelColor.Green + 0.114 * pixelColor.Blue;

                    // Apply saturation adjustment
                    byte blue = (byte)Math.Min(Math.Max(luminance + (pixelColor.Blue - luminance) * (1 + saturation / 100.0), 0), 255);
                    byte green = (byte)Math.Min(Math.Max(luminance + (pixelColor.Green - luminance) * (1 + saturation / 100.0), 0), 255);
                    byte red = (byte)Math.Min(Math.Max(luminance + (pixelColor.Red - luminance) * (1 + saturation / 100.0), 0), 255);

                    result[y, x] = new Bgr(blue, green, red);
                }
            }

            return result;
        }
    
    // Optional: Default constructor for serialization/deserialization
    public CustomFilter() { }

        // Method to save the filter to a file
        
    }
}
