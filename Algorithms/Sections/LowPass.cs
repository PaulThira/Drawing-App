using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algorithms.Sections
{
    public class LowPass
    {
        public LowPass() { }
        public  Image<Bgr, byte> AngleImage(Image<Gray, byte> image)
        {
            // Sobel kernels
            int[,] Sx = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] Sy = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

            // Result image
            Image<Bgr, byte> result = new Image<Bgr, byte>(image.Size);

            // Loop through pixels
            for (int y = 1; y < image.Height - 1; ++y)
            {
                for (int x = 1; x < image.Width - 1; ++x)
                {
                    double fx = 0, fy = 0;

                    // Compute gradients
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            fx += image.Data[y + i, x + j, 0] * Sx[i + 1, j + 1];
                            fy += image.Data[y + i, x + j, 0] * Sy[i + 1, j + 1];
                        }
                    }

                    // Compute angle (theta) in radians
                    double theta = Math.Atan2(fy, fx);

                    // Map angle to colors
                    if (theta >= -Math.PI / 2 && theta < -3 * Math.PI / 8)
                    {
                        result.Data[y, x, 0] = 0;   // Blue
                        result.Data[y, x, 1] = 255; // Green
                        result.Data[y, x, 2] = 0;   // Red
                    }
                    else if (theta >= -3 * Math.PI / 8 && theta < -Math.PI / 8)
                    {
                        result.Data[y, x, 0] = 255;
                        result.Data[y, x, 1] = 0;
                        result.Data[y, x, 2] = 0;
                    }
                    else if (theta >= -Math.PI / 8 && theta < Math.PI / 8)
                    {
                        result.Data[y, x, 0] = 0;
                        result.Data[y, x, 1] = 0;
                        result.Data[y, x, 2] = 255;
                    }
                    else if (theta >= Math.PI / 8 && theta < 3 * Math.PI / 8)
                    {
                        result.Data[y, x, 0] = 0;
                        result.Data[y, x, 1] = 255;
                        result.Data[y, x, 2] = 255;
                    }
                    else if (theta >= 3 * Math.PI / 8 && theta <= Math.PI / 2)
                    {
                        result.Data[y, x, 0] = 0;
                        result.Data[y, x, 1] = 255;
                        result.Data[y, x, 2] = 0;
                    }
                    else if (theta >= Math.PI / 2 || theta < -Math.PI / 2)
                    {
                        result.Data[y, x, 0] = 255; // Handle extreme cases
                        result.Data[y, x, 1] = 255;
                        result.Data[y, x, 2] = 255;
                    }
                }
            }

            return result;
        }
        public  Image<Gray, byte> GradientImage(Image<Bgr, byte> image, byte T1, byte T2)
        {
            // Sobel kernels
            int[,] Sx = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] Sy = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

            // Convert the input to grayscale
            Image<Gray, byte> grayImage = image.Convert<Gray, byte>();
            Image<Gray, byte> gradientImage = new Image<Gray, byte>(image.Size);
            Image<Gray, float> directionImage = new Image<Gray, float>(image.Size);

            // Calculate gradient magnitudes and directions
            for (int y = 1; y < grayImage.Height - 1; ++y)
            {
                for (int x = 1; x < grayImage.Width - 1; ++x)
                {
                    double gx = 0, gy = 0;

                    // Compute gradients using Sobel filters
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            gx += grayImage.Data[y + i, x + j, 0] * Sx[i + 1, j + 1];
                            gy += grayImage.Data[y + i, x + j, 0] * Sy[i + 1, j + 1];
                        }
                    }

                    // Calculate gradient magnitude and direction
                    double magnitude = Math.Sqrt(gx * gx + gy * gy);
                    double theta = Math.Atan2(gy, gx); // Direction in radians
                    theta = theta < 0 ? theta + Math.PI : theta; // Normalize to [0, π]

                    // Store magnitude and direction
                    gradientImage.Data[y, x, 0] = (byte)Math.Min(magnitude, 255);
                    directionImage.Data[y, x, 0] = (float)theta;
                }
            }

            // Apply Non-Maximum Suppression (NMS)
            Image<Gray, byte> suppressedImage = NonMaximaSuppression(gradientImage, directionImage);

            // Double-thresholding
            Image<Gray, byte> result = new Image<Gray, byte>(suppressedImage.Size);
            for (int y = 0; y < suppressedImage.Height; y++)
            {
                for (int x = 0; x < suppressedImage.Width; x++)
                {
                    byte pixel = suppressedImage.Data[y, x, 0];

                    if (pixel >= T2)
                    {
                        result.Data[y, x, 0] = 255; // Strong edge
                    }
                    else if (pixel < T1)
                    {
                        result.Data[y, x, 0] = 0; // Non-edge
                    }
                    else
                    {
                        // Weak edge
                        bool isConnectedToStrongEdge = CheckStrongEdgeNeighbor(result, x, y, T2);
                        result.Data[y, x, 0] = isConnectedToStrongEdge ? (byte)255 : (byte)0;
                    }
                }
            }

            return result;
        }
        public  Image<Gray, byte> HThresholding(Image<Gray, byte> image, byte t1, byte t2)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(image.Size);

            // First pass: Apply the thresholds
            for (int y = 0; y < image.Height; ++y)
            {
                for (int x = 0; x < image.Width; ++x)
                {
                    byte pixel = image.Data[y, x, 0];
                    if (pixel >= t2)
                    {
                        result.Data[y, x, 0] = 255; // Strong edge
                    }
                    else if (pixel < t1)
                    {
                        result.Data[y, x, 0] = 0; // Non-edge
                    }
                    else
                    {
                        result.Data[y, x, 0] = 128; // Weak edge (temporarily mark as gray)
                    }
                }
            }

            // Second pass: Hysteresis linking
            for (int y = 1; y < image.Height - 1; ++y)
            {
                for (int x = 1; x < image.Width - 1; ++x)
                {
                    // Process weak edges (marked as 128)
                    if (result.Data[y, x, 0] == 128)
                    {
                        bool hasStrongNeighbor = false;

                        // Check 8-connected neighbors
                        for (int i = -1; i <= 1; ++i)
                        {
                            for (int j = -1; j <= 1; ++j)
                            {
                                if (result.Data[y + i, x + j, 0] == 255)
                                {
                                    hasStrongNeighbor = true;
                                    break;
                                }
                            }
                            if (hasStrongNeighbor)
                                break;
                        }

                        // Promote weak edge to strong edge if connected to a strong edge
                        if (hasStrongNeighbor)
                        {
                            result.Data[y, x, 0] = 255;
                        }
                        else
                        {
                            result.Data[y, x, 0] = 0; // Suppress weak edge
                        }
                    }
                }
            }

            return result;
        }

        // Helper function to check if a weak edge is connected to a strong edge
        private  bool CheckStrongEdgeNeighbor(Image<Gray, byte> image, int x, int y, byte T2)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;

                    int neighborX = x + i;
                    int neighborY = y + j;

                    if (neighborX >= 0 && neighborX < image.Width && neighborY >= 0 && neighborY < image.Height)
                    {
                        if (image.Data[neighborY, neighborX, 0] >= T2)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        // Suppresses non-maximum values based on gradient direction
        public  Image<Gray, byte> NonMaximaSuppression(Image<Gray, byte> gradientImage, Image<Gray, float> directionImage)
        {
            // Initialize the result image
            Image<Gray, byte> result = new Image<Gray, byte>(gradientImage.Size);

            // Loop through every pixel except the borders
            for (int y = 1; y < gradientImage.Height - 1; ++y)
            {
                for (int x = 1; x < gradientImage.Width - 1; ++x)
                {
                    // Get gradient magnitude and direction
                    float magnitude = gradientImage.Data[y, x, 0];
                    float theta = directionImage.Data[y, x, 0]; // Assume this contains the gradient direction in radians

                    // Normalize theta to [0, 180°]
                    theta = (theta < 0) ? theta + (float)Math.PI : theta;

                    // Determine the neighbors to compare based on direction
                    byte neighbor1 = 0, neighbor2 = 0;

                    if ((theta >= 0 && theta < Math.PI / 8) || (theta >= 7 * Math.PI / 8 && theta <= Math.PI))
                    {
                        // Horizontal edge (0°)
                        neighbor1 = gradientImage.Data[y, x - 1, 0]; // Left
                        neighbor2 = gradientImage.Data[y, x + 1, 0]; // Right
                    }
                    else if (theta >= Math.PI / 8 && theta < 3 * Math.PI / 8)
                    {
                        // Diagonal edge (45°)
                        neighbor1 = gradientImage.Data[y - 1, x + 1, 0]; // Top-right
                        neighbor2 = gradientImage.Data[y + 1, x - 1, 0]; // Bottom-left
                    }
                    else if (theta >= 3 * Math.PI / 8 && theta < 5 * Math.PI / 8)
                    {
                        // Vertical edge (90°)
                        neighbor1 = gradientImage.Data[y - 1, x, 0]; // Top
                        neighbor2 = gradientImage.Data[y + 1, x, 0]; // Bottom
                    }
                    else if (theta >= 5 * Math.PI / 8 && theta < 7 * Math.PI / 8)
                    {
                        // Diagonal edge (135°)
                        neighbor1 = gradientImage.Data[y - 1, x - 1, 0]; // Top-left
                        neighbor2 = gradientImage.Data[y + 1, x + 1, 0]; // Bottom-right
                    }

                    // Suppress non-maximum values
                    if (magnitude >= neighbor1 && magnitude >= neighbor2)
                    {
                        result.Data[y, x, 0] = (byte)magnitude; // Keep the maximum value
                    }
                    else
                    {
                        result.Data[y, x, 0] = 0; // Suppress non-maximum
                    }
                }
            }

            return result;
        }

        public Image<Gray,byte> Sobel(Image<Bgr,byte> image)
        {
            BasicOperations basicOperations = new BasicOperations();
            int[][]Sx=new int[3][];
            int[][]Sy=new int[3][];
            var grays=basicOperations.BgrToGrayscale(image);
            for (int i = 0; i < 3; i++)
            {
                Sx[i]=new int[3];
                Sy[i]=new int[3];
            }
            Sx[0][0] = -1;
            Sx[0][1] = 0;
            Sx[0][2] = 1;
            Sx[1][0] = -2;
            Sx[1][1] = 0;
            Sx[1][2] = 2;
            Sx[2][0] = -1;
            Sx[2][1] = 0;
            Sx[2][2] = 1;
            Sy[0][0] = -1;
            Sy[0][1] = -2;
            Sy[0][2] = -1;
            Sy[1][0] = -0;
            Sy[1][1] = 0;
            Sy[1][2] = 0;
            Sy[2][0] = 1;
            Sy[2][1] = 2;
            Sy[2][2] = 1;
            var result = new Image<Gray, byte>(image.Width, image.Height);
            for (int i = 1; i < image.Height - 1; i++)
            {
                for (int j = 1; j < image.Width - 1; j++)
                {
                    double fx = 0, fy = 0;

                    for (int k1 = -1; k1 <= 1; k1++)
                    {
                        for (int k2 = -1; k2 <= 1; k2++)
                        {
                            // Correctly access neighboring pixels using (i + k1, j + k2)
                            byte pixel = grays.Data[i + k1, j + k2, 0];

                            // Apply the Sobel kernel
                            fx += pixel * Sx[k1 + 1][ k2 + 1];
                            fy += pixel * Sy[k1 + 1][ k2 + 1];
                        }
                    }

                    // Calculate the gradient magnitude
                    double magnitude = Math.Sqrt(fx * fx + fy * fy);

                    // Clamp the result to the range [0, 255] and assign to the result image
                    result.Data[i, j, 0] = (byte)Math.Min(255, magnitude);
                }
            }
            return result;

        }
        public Image<Gray, byte> Canny(Image<Bgr, byte> image, byte T1, byte T2)
        {
            if (T1 >= T2)
            {
                var d = T1;
                T1 = T2;
                T2 = T1;
            }
           

            // Optional: Apply Gaussian smoothing
            Image<Bgr, byte> smoothed = image.SmoothGaussian(3);

            // Compute Canny edge detection using GradientImage
            return GradientImage(smoothed, T1, T2);
        }

    }
}
