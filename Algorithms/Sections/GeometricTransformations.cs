using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algorithms.Sections
{
    public  class GeometricTransformations
    {
        public Image<Bgr, byte> AffineTransformation(Image<Bgr, byte> image, float[,] matrix)
        {
            var width = image.Width;
            var height = image.Height;

            // Define the corners of the original image
            float[,] corners =
            {
        { 0, 0 },
        { width, 0 },
        { 0, height },
        { width, height },
    };

            // Calculate the transformed corners
            float xMin = float.MaxValue, xMax = float.MinValue;
            float yMin = float.MaxValue, yMax = float.MinValue;
            for (int i = 0; i < 4; i++)
            {
                float x = corners[i, 0];
                float y = corners[i, 1];
                float xPrime = x * matrix[0, 0] + y * matrix[0, 1] + matrix[0, 2];
                float yPrime = x * matrix[1, 0] + y * matrix[1, 1] + matrix[1, 2];
                xMin = Math.Min(xMin, xPrime);
                xMax = Math.Max(xMax, xPrime);
                yMin = Math.Min(yMin, yPrime);
                yMax = Math.Max(yMax, yPrime);
            }

            // Compute the new dimensions of the output image
            int newWidth = (int)Math.Ceiling(xMax - xMin);
            int newHeight = (int)Math.Ceiling(yMax - yMin);

            // Calculate the translation to center the transformed image
            float xTranslation = (newWidth - width) / 2.0f - xMin;
            float yTranslation = (newHeight - height) / 2.0f - yMin;

            // Create a new matrix with centering translation
            float[,] centeredMatrix = (float[,])matrix.Clone();
            centeredMatrix[0, 2] += xTranslation;
            centeredMatrix[1, 2] += yTranslation;

            // Create the output image with a white background
            var outputImage = new Image<Bgr, byte>(newWidth, newHeight);
            outputImage.SetValue(new Bgr(255, 255, 255)); // Set background to white

            // Perform the transformation with inverse mapping
            for (int y = 0; y < outputImage.Height; y++)
            {
                for (int x = 0; x < outputImage.Width; x++)
                {
                    // Compute source coordinates using matrix inverse
                    float sourceX = (x * centeredMatrix[1, 1] - y * centeredMatrix[0, 1] +
                                    centeredMatrix[0, 1] * centeredMatrix[1, 2] -
                                    centeredMatrix[1, 1] * centeredMatrix[0, 2]) /
                                    (centeredMatrix[0, 0] * centeredMatrix[1, 1] -
                                     centeredMatrix[0, 1] * centeredMatrix[1, 0]);

                    float sourceY = (y - centeredMatrix[1, 0] * sourceX - centeredMatrix[1, 2]) /
                                    centeredMatrix[1, 1];

                    // Check if source coordinates are within image bounds
                    if (sourceX >= 0 && sourceX < width &&
                        sourceY >= 0 && sourceY < height)
                    {
                        // Bilinear interpolation for smoother results
                        int x0 = (int)Math.Floor(sourceX);
                        int y0 = (int)Math.Floor(sourceY);
                        int x1 = Math.Min(x0 + 1, width - 1);
                        int y1 = Math.Min(y0 + 1, height - 1);

                        float fx = sourceX - x0;
                        float fy = sourceY - y0;

                        // Interpolate pixel values
                        var p00 = image[y0, x0];
                        var p10 = image[y0, x1];
                        var p01 = image[y1, x0];
                        var p11 = image[y1, x1];

                        byte b = (byte)((1 - fx) * (1 - fy) * p00.Blue +
                                        fx * (1 - fy) * p10.Blue +
                                        (1 - fx) * fy * p01.Blue +
                                        fx * fy * p11.Blue);
                        byte g = (byte)((1 - fx) * (1 - fy) * p00.Green +
                                        fx * (1 - fy) * p10.Green +
                                        (1 - fx) * fy * p01.Green +
                                        fx * fy * p11.Green);
                        byte r = (byte)((1 - fx) * (1 - fy) * p00.Red +
                                        fx * (1 - fy) * p10.Red +
                                        (1 - fx) * fy * p01.Red +
                                        fx * fy * p11.Red);

                        outputImage[y, x] = new Bgr(b, g, r);
                    }
                }
            }

            return outputImage;
        }

    }
}
