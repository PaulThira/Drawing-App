using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
        public Image<Bgr, byte> PerspectiveWrap(Image<Bgr, byte> input, List<Point> sourcePoints, List<Point> destinationPoints)
        {
            if (sourcePoints.Count < 4 || destinationPoints.Count < 4)
                throw new ArgumentException("Both source and destination must have exactly 4 points.");

            // Step 1: Compute the transformation matrix A
            double[,] A = CalculateProjectiveMatrix(sourcePoints, destinationPoints);

            // Step 2: Create an output image based on the destination dimensions
            int newWidth = input.Width;
            int newHeight = input.Height;
            var output = new Image<Bgr, byte>(newWidth, newHeight);
            output.SetValue(new Bgr(255, 255, 255)); // Default white background

            // Step 3: Map each pixel in the destination back to the source
            for (int yDest = 0; yDest < newHeight; yDest++)
            {
                for (int xDest = 0; xDest < newWidth; xDest++)
                {
                    // Map destination pixel (x', y') to source (x, y) using A-inverse
                    double[] sourceCoords = ApplyTransformationMatrix(A, xDest, yDest);
                    double xSource = sourceCoords[0];
                    double ySource = sourceCoords[1];

                    // Step 4: Check bounds and interpolate color
                    if (xSource >= 0 && xSource < input.Width - 1 && ySource >= 0 && ySource < input.Height - 1)
                    {
                        // Bilinear interpolation
                        output[yDest, xDest] = BilinearInterpolation(input, xSource, ySource);
                    }
                }
            }

            return output;
        }

        private double[,] CalculateProjectiveMatrix(List<Point> source, List<Point> destination)
        {
            double[,] L = new double[8, 8];
            double[] b = new double[8];

            for (int i = 0; i < 4; i++)
            {
                double x = source[i].X;
                double y = source[i].Y;
                double xPrime = destination[i].X;
                double yPrime = destination[i].Y;

                L[2 * i, 0] = x;
                L[2 * i, 1] = y;
                L[2 * i, 2] = 1;
                L[2 * i, 6] = -x * xPrime;
                L[2 * i, 7] = -y * xPrime;
                b[2 * i] = xPrime;

                L[2 * i + 1, 3] = x;
                L[2 * i + 1, 4] = y;
                L[2 * i + 1, 5] = 1;
                L[2 * i + 1, 6] = -x * yPrime;
                L[2 * i + 1, 7] = -y * yPrime;
                b[2 * i + 1] = yPrime;
            }

            double[] a = SolveLinearSystem(L, b);

            // Build the 3x3 matrix A
            double[,] A = {
        { a[0], a[1], a[2] },
        { a[3], a[4], a[5] },
        { a[6], a[7], 1    }
    };

            return A;
        }

        private double[] ApplyTransformationMatrix(double[,] A, int x, int y)
        {
            double xPrime = A[0, 0] * x + A[0, 1] * y + A[0, 2];
            double yPrime = A[1, 0] * x + A[1, 1] * y + A[1, 2];
            double h = A[2, 0] * x + A[2, 1] * y + A[2, 2];

            return new double[] { xPrime / h, yPrime / h };
        }

        private Bgr BilinearInterpolation(Image<Bgr, byte> input, double x, double y)
        {
            int x0 = (int)x;
            int y0 = (int)y;
            int x1 = x0 + 1;
            int y1 = y0 + 1;

            double dx = x - x0;
            double dy = y - y0;

            // Get the four neighboring pixels
            var Q11 = input[y0, x0];
            var Q21 = input[y0, x1];
            var Q12 = input[y1, x0];
            var Q22 = input[y1, x1];

            // Perform bilinear interpolation
            double blue = Q11.Blue * (1 - dx) * (1 - dy) +
                          Q21.Blue * dx * (1 - dy) +
                          Q12.Blue * (1 - dx) * dy +
                          Q22.Blue * dx * dy;

            double green = Q11.Green * (1 - dx) * (1 - dy) +
                           Q21.Green * dx * (1 - dy) +
                           Q12.Green * (1 - dx) * dy +
                           Q22.Green * dx * dy;

            double red = Q11.Red * (1 - dx) * (1 - dy) +
                         Q21.Red * dx * (1 - dy) +
                         Q12.Red * (1 - dx) * dy +
                         Q22.Red * dx * dy;

            return new Bgr(blue, green, red);
        }

        private double[] SolveLinearSystem(double[,] L, double[] b)
        {
            int n = b.Length;
            double[] x = new double[n];
            for (int i = 0; i < n; i++)
            {
                int max = i;
                for (int k = i + 1; k < n; k++)
                    if (Math.Abs(L[k, i]) > Math.Abs(L[max, i]))
                        max = k;

                for (int k = i; k < n; k++)
                {
                    double tmp = L[max, k];
                    L[max, k] = L[i, k];
                    L[i, k] = tmp;
                }
                double t = b[max];
                b[max] = b[i];
                b[i] = t;

                for (int k = i + 1; k < n; k++)
                {
                    double factor = L[k, i] / L[i, i];
                    b[k] -= factor * b[i];
                    for (int j = i; j < n; j++)
                        L[k, j] -= factor * L[i, j];
                }
            }

            for (int i = n - 1; i >= 0; i--)
            {
                double sum = 0;
                for (int j = i + 1; j < n; j++)
                    sum += L[i, j] * x[j];
                x[i] = (b[i] - sum) / L[i, i];
            }

            return x;
        }


    }
}
