using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Algorithms.Sections
{
   public class HighPass
    {
        public HighPass() { 
        }
        public Image<Bgr, byte> GausianBlur(Image<Bgr, byte> image)
        {
            double[] g1 = [0.0625, 0.125, 0.0625];
            double[] g2 = [0.125, 0.25, 0.125];
            double[] g3 = [0.0625, 0.125, 0.0625];
            double[,] gfilter = new double[3, 3];
            for (int i = 0; i < 3; i++)
            {
                
                for (int j = 0;j < 3; j++)
                {
                    if (i == 0)
                    {
                        gfilter[i, j] = g1[j];
                    }
                    if (i == 1)
                    {
                        gfilter[i, j] = g2[j];
                    }
                    if (i == 2)
                    {
                        gfilter[i, j] = g3[j];
                    }
                }
            }
            var result=image.Clone();
            for (int i = 1; i < image.Height - 1; i++)
            {
                for(int j=1; j < image.Width - 1; j++)
                {
                    double sumB=0,sumG=0,sumR=0;
                    for(int k1 = -1; k1 <= 1; k1++)
                    {
                        for(int k2= -1; k2 <= 1; k2++)
                        {
                            sumB += image.Data[i + k1, j + k2, 0] * gfilter[k1 + 1, k2 + 1];
                            sumG += image.Data[i + k1, j + k2,1] * gfilter[k1 + 1, k2 + 1];
                            sumR += image.Data[i + k1, j + k2, 2] * gfilter[k1 + 1, k2 + 1];
                        }
                    }
                    result.Data[i, j, 0] = (byte)Math.Min(255, Math.Max(0, sumB));
                    result.Data[i, j, 1] = (byte)Math.Min(255, Math.Max(0, sumG));
                    result.Data[i, j, 2] = (byte)Math.Min(255, Math.Max(0, sumR));

                }
            }
            return result;
        }
        public Image<Bgr, byte> FastMedianFilter(Image<Bgr, byte> image, int windowSize)
        {
            // Ensure window size is odd
            if (windowSize % 2 == 0)
                windowSize++;
            MessageBox.Show(windowSize.ToString());
            int radius = windowSize / 2;
            int width = image.Width;
            int height = image.Height;

            // Create a copy of the image to store the result
            Image<Bgr, byte> result = new Image<Bgr, byte>(width, height);

            // Histogram array for pixel intensity (0-255)
            int[] red = new int[256];
            int[] green = new int[256];
            int[] blue = new int[256];

            // Process each pixel in the image
            for (int y = 0; y < height; y++)
            {
                // Clear the histogram
                Array.Clear(red, 0, red.Length);
                Array.Clear(green, 0, green.Length);
                Array.Clear(blue, 0, blue.Length);

                // Initialize the histogram for the first window in the row
                for (int wy = -radius; wy <= radius; wy++)
                {
                    int row = Math.Clamp(y + wy, 0, height - 1); // Clamp to valid range
                    for (int wx = -radius; wx <= radius; wx++)
                    {
                        int col = Math.Clamp(wx, 0, width - 1); // Clamp to valid range
                        blue[image.Data[row, col, 0]]++;
                        green[image.Data[row, col, 1]]++;
                        red[image.Data[row, col, 2]]++;
                    }
                }

                // Median for the first pixel in the row
                result.Data[y, 0, 0] = (byte)FindMedianFromHistogram(blue, windowSize * windowSize);
                result.Data[y, 0, 1] = (byte)FindMedianFromHistogram(green, windowSize * windowSize);
                result.Data[y, 0, 2] = (byte)FindMedianFromHistogram(red, windowSize * windowSize);
                // Slide the window horizontally
                for (int x = 1; x < width; x++)
                {
                    // Remove pixels leaving the window (left edge)
                    for (int wy = -radius; wy <= radius; wy++)
                    {
                        int row = Math.Clamp(y + wy, 0, height - 1);
                        int colToRemove = Math.Clamp(x - radius - 1, 0, width - 1);
                        blue[image.Data[row, colToRemove, 0]]--;
                        green[image.Data[row, colToRemove, 1]]--;
                        red[image.Data[row, colToRemove, 2]]--;
                    }

                    // Add pixels entering the window (right edge)
                    for (int wy = -radius; wy <= radius; wy++)
                    {
                        int row = Math.Clamp(y + wy, 0, height - 1);
                        int colToAdd = Math.Clamp(x + radius, 0, width - 1);
                        blue[image.Data[row, colToAdd, 0]]++;
                        green[image.Data[row, colToAdd, 1]]++;
                        red[image.Data[row, colToAdd, 2]]++;
                    }

                    // Compute the median for the current window
                    result.Data[y, x, 0] = (byte)FindMedianFromHistogram(blue, windowSize * windowSize);
                    result.Data[y, x, 1] = (byte)FindMedianFromHistogram(green, windowSize * windowSize);
                    result.Data[y, x, 2] = (byte)FindMedianFromHistogram(red, windowSize * windowSize);
                }
            }

            return result;
        }

        private int FindMedianFromHistogram(int[] histogram, int totalPixels)
        {
            int cumulativeSum = 0;
            int medianPosition = totalPixels / 2;

            for (int i = 0; i < histogram.Length; i++)
            {
                cumulativeSum += histogram[i];
                if (cumulativeSum > medianPosition)
                {
                    return i; // The median intensity value
                }
            }

            return 0; // Fallback in case of error (shouldn't occur)
        }

    }
}
