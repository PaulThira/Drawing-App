using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Drawing;
using Point = System.Drawing.Point;

namespace Algorithms.Sections
{
    public class Segmentation
    {
        public Image<Gray, byte> Watershed(Image<Bgr, byte> image)
        {
            // High-pass filtering and grayscale conversion
            var highPass = new HighPass();
            var basicOperations = new BasicOperations();
            var blurred = highPass.FastMedianFilter(image, 5);
            var grays = basicOperations.BgrToGrayscale(blurred);

            // Prepare data structures
            var watersheds = new HashSet<Point>();
            var levels = new HashSet<Point>[256];
            for (int i = 0; i < 256; i++)
            {
                levels[i] = new HashSet<Point>();
            }

            // Categorize pixels by intensity
            for (int i = 1; i < image.Height - 1; i++)
            {
                for (int j = 1; j < image.Width - 1; j++)
                {
                    int pixel = grays.Data[i, j, 0];
                    levels[pixel].Add(new Point(j, i)); // Note: swapped x and y
                }
            }

            // Find min and max intensity levels
            int min = Array.FindIndex(levels, set => set.Count > 0);
            int max = Array.FindLastIndex(levels, set => set.Count > 0);

            // Initialize basins
            var basins = new List<HashSet<Point>>();
            for (int k = 0; k <= max; k++)
            {
                basins.Add(new HashSet<Point>());
            }

            // Initial basin for minimum intensity
            foreach (Point p in levels[min])
            {
                basins[min].Add(p);
            }

            // Watershed algorithm
            for (int level = min + 1; level <= max; level++)
            {
                foreach (Point p in levels[level])
                {
                    List<int> neighborLabels = GetNeighborLabels(basins, p, level, image.Height, image.Width);

                    if (neighborLabels.Count == 0)
                    {
                        // New basin
                        basins[level].Add(p);
                    }
                    else if (neighborLabels.Count == 1)
                    {
                        // Extend existing basin
                        basins[neighborLabels[0]].Add(p);
                    }
                    else
                    {
                        // Add to watershed line
                        watersheds.Add(p);
                    }
                }
            }

            return ConvertBasinsToImage(basins, watersheds, grays.Size);
        }

        private List<int> GetNeighborLabels(List<HashSet<Point>> basins, Point p, int level, int height, int width)
        {
            var labels = new List<int>();
            int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

            for (int i = 0; i < 8; i++)
            {
                int nx = p.X + dx[i];
                int ny = p.Y + dy[i];

                // Correct bounds checking
                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    for (int basinLevel = 0; basinLevel < basins.Count; basinLevel++)
                    {
                        if (basins[basinLevel].Contains(new Point(nx, ny)) && !labels.Contains(basinLevel))
                        {
                            labels.Add(basinLevel);
                        }
                    }
                }
            }
            return labels;
        }

        private Image<Gray, byte> ConvertBasinsToImage(List<HashSet<Point>> basins, HashSet<Point> watersheds, System.Drawing.Size size)
        {
            var result = new Image<Gray, byte>(size);

            // Assign basin labels
            byte label = 1;
            foreach (var basin in basins)
            {
                foreach (Point p in basin)
                {
                    result.Data[p.Y, p.X, 0] = label; // Note: swapped coordinates
                }
                label = (byte)((label + 1) % 255);
            }

            // Assign watershed lines
            foreach (Point p in watersheds)
            {
                result.Data[p.Y, p.X, 0] = 255; // Note: swapped coordinates
            }

            return result;
        }

        public Image<Bgr, byte> MagicTool(Image<Bgr, byte> image, System.Windows.Media.Color color, int T)
        {
            var result = new Image<Bgr, byte>(image.Size);
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    int b = image.Data[i, j, 0] - color.B;
                    int g = image.Data[i, j, 1] - color.G;
                    int r = image.Data[i, j, 2] - color.R;

                    double value = Math.Sqrt(b * b + g * g + r * r);

                    if (value <= T)
                    {
                        result.Data[i, j, 0] = image.Data[i, j, 0];
                        result.Data[i, j, 1] = image.Data[i, j, 1];
                        result.Data[i, j, 2] = image.Data[i, j, 2];
                    }
                    else
                    {
                        result.Data[i, j, 0] = 255;
                        result.Data[i, j, 1] = 255;
                        result.Data[i, j, 2] = 255;
                    }
                }
            }
            return result;
        }


    }
}
