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
            HighPass highPass = new HighPass();
            BasicOperations basicOperations = new BasicOperations();
            Image<Bgr, byte> blurr = highPass.FastMedianFilter(image, 5);
            Image<Gray, byte> grays = basicOperations.BgrToGrayscale(blurr);
            HashSet<Point> watersheds = new HashSet<Point>();
            HashSet<Point>[] levels = new HashSet<Point>[256];
            for (int i = 0; i < 256; i++)
            {
                levels[i] = new HashSet<Point>();

            }
            for (int i = 1; i < image.Height - 1; i++)
            {
                for (int j = 1; j < image.Width - 1; j++)
                {
                    int pixel = grays.Data[i, j, 0];
                    levels[pixel].Add(new Point(i, j));

                }
            }
            var min = -1;
            var max = 256;
            for (int k = 0; k < levels.Length; k++)
            {
                if (levels[k].Count > 0)
                {
                    min = k;
                    break;
                }
            }
            for (int k = levels.Length-1; k >=0; k--)
            {
                if (levels[k].Count > 0)
                {
                    max = k;
                    break;
                }
            }
            var lmin = levels[min];
            var lmax = levels[max];
            List<HashSet<Point>> basins = new List<HashSet<Point>>();
           
            for (int k = 0; k < levels.Length; k++) { 
                basins.Add(new HashSet<Point>());
            }
            foreach (Point p in levels[min])
                basins[min].Add(p);
            for (int level = min + 1; level <= max; level++)
            {
                foreach (Point p in levels[level])
                {
                    List<int> neighborLabels = GetNeighborLabels(basins, p, level);
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

            // Convert basins and watersheds to an output image
            return ConvertBasinsToImage(basins, watersheds, grays.Size);




        }
        public Image<Bgr,byte> MagicTool(Image<Bgr,byte> image,System.Windows.Media.Color color,int T)
        {
            Image<Bgr,byte> result=new Image<Bgr,byte>(image.Size);
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    int b = (image.Data[i, j, 0] - color.B);
                    int g=(image.Data[i, j, 1] - color.G);
                    int r =(image.Data[i,j, 2] - color.R);  
                    double value=Math.Sqrt(b*b+g*g+r*r);
                    if (value <= T)
                    {
                        result.Data[i, j, 0] = image.Data[i, j, 0];
                        result.Data[i, j, 1] = image.Data[i, j, 1];
                        result.Data[i, j, 2] = image.Data[i, j, 2];

                    }
                    else {
                        result.Data[i, j, 0] = 255;
                        result.Data[i, j, 1] = 255;
                        result.Data[i, j, 2] = 255;

                    }

                }
            }
            return result;

        }
        private List<int> GetNeighborLabels(List<HashSet<Point>> basins, Point p, int level)
        {
            List<int> labels = new List<int>();
            int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

            for (int i = 0; i < 8; i++)
            {
                int nx = p.X + dx[i];
                int ny = p.Y + dy[i];

                if (nx >= 0 && nx < basins[level].Count && ny >= 0 && ny < basins[level].Count)
                {
                    foreach (var basin in basins)
                    {
                        if (basin.Contains(new Point(nx, ny)) && !labels.Contains(level))
                        {
                            labels.Add(level);
                        }
                    }
                }
            }
            return labels;
        }

        // Convert basins and watersheds to an output image
        private Image<Gray, byte> ConvertBasinsToImage(List<HashSet<Point>> basins, HashSet<Point> watersheds, System.Drawing.Size size)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(size);

            // Assign basin labels
            byte label = 1;
            foreach (var basin in basins)
            {
                foreach (Point p in basin)
                {
                    result.Data[p.X, p.Y, 0] = label;
                }
                label = (byte)((label + 1) % 255); // Avoid overflow
            }

            // Assign watershed lines
            foreach (Point p in watersheds)
            {
                result.Data[p.X, p.Y, 0] = 255;
            }

            return result;
        }


    }
}
