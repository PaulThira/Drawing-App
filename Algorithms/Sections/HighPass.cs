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
   public class HighPass
    {
        public HighPass() { 
        }
        public Image<Bgr, byte> GausianBlur(Image<Bgr, byte> image)
        {
            double[] g1 = [0.075, 0.125, 0.075];
            double[] g2 = [0.125, 0.2, 0.125];
            double[] g3 = [0.075, 0.125, 0.075];
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
    }
}
