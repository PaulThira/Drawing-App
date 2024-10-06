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
    public class Thresholding
    {
        BasicOperations basicOperations;
        public int[] grayHistogram {  get; set; }   
        public Thresholding() { 
        basicOperations = new BasicOperations();
        grayHistogram = new int[256];
        }
        public Image<Gray,byte> Triangle(Image<Bgr,byte> image)
        {
            var grays=basicOperations.BgrToGrayscale(image);
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    grayHistogram[grays.Data[i,j,0]]++;

                }
            }
            var total=image.Height*image.Width;
            for (int i = 0; i < grayHistogram.Length; i++)
            {
                grayHistogram[i]/= total;
            }
            var max=grayHistogram.Max();
            var maxi = 0;
            
            for(int i=0;i< grayHistogram.Length; i++)
            {
                if (grayHistogram[i] == max)
                {
                    maxi=i;
                }
            }
            int mini1 = 0,mini2=255;
            for(int i=0; i< grayHistogram.Length; i++)
            {
                if(grayHistogram[i] != 0)
                {
                    mini1 = i;
                    break;
                }
            }
            for (int i = 255; i >=0; i--)
            {
                if (grayHistogram[i] != 0)
                {
                    mini2 = i;
                    break;
                }
            }
            var mini = 0;
            if (Math.Abs(mini1 - maxi) > Math.Abs(maxi-mini2))
            {
                mini = mini1;
            }
            else
            {
                mini = mini2;
            }
            double h = Math.Sqrt((maxi - mini) * (maxi - mini) + (grayHistogram[maxi] - grayHistogram[mini]) * (grayHistogram[maxi] - grayHistogram[mini]));
            if (mini > maxi)
            {
                int temp = mini;
                mini = maxi;
                maxi = temp;
            }

            // Now the following loop will work correctly regardless of initial values of mini and maxi
            double maxd = 0;
            int id = mini;  // To store the threshold candidate
            for (int i = mini; i <= maxi; i++)
            {
                // Calculate the perpendicular distance of histogram bin 'i' to the line
                double distance = Math.Abs((grayHistogram[maxi] - grayHistogram[mini]) * i
                                        - (maxi - mini) * grayHistogram[i]
                                        + maxi * grayHistogram[mini] - mini * grayHistogram[maxi]) / h;

                // Find the maximum distance
                if (distance > maxd)
                {
                    maxd = distance;
                    id = i;  // Update the threshold index
                }
            }
            double offset = 0.2 * (maxi - mini);

            // Add this offset to the threshold index
            double finalThreshold = id + offset;
            Image<Gray, byte> result = new Image<Gray, byte>(image.Width, image.Height);
            // Ensure finalThreshold remains within valid bounds (0 to 255)
            finalThreshold = Math.Min(255, Math.Max(0, finalThreshold));
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    if (grays.Data[i, j, 0] > finalThreshold)
                    {
                        result.Data[i, j, 0] = 255;
                    }
                    else
                    {
                        result.Data[i, j, 0] = 0;
                    }

                }
            }
            return result;
        }
    }
}
