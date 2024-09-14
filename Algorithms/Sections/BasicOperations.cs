using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Emgu.CV;
using Emgu.CV.Structure;


namespace Algorithms.Sections
{
    public class BasicOperations
    {
        public int[] blue {  get; set; }
        public int[] red { get; set; }
        public int[] green { get; set; }
        public void HistogramCalc( Image<Bgr,Byte> image)
        {
             blue = new int[256];
             green = new int[256];
             red = new int[256];
            for (int i = 0; i < image.Height; i++) {
                for (int j = 0; j < image.Width; j++)
                {
                    int b, g, r;
                    b = image.Data[i,j,0];  
                    g= image.Data[i,j,1];
                    r= image.Data[i,j,2];
                    blue[b]++;
                    green[g]++;
                    red[r]++;

                }
            }
            

        }
        public Image<Gray,Byte> BgrToGrayscale(Image<Bgr,Byte> image)
        {
            Image<Gray,Byte> gray = new Image<Gray, Byte>(image.Height,image.Width);
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    gray.Data[i,j,0]=(Byte)(0.299 * image.Data[i, j, 2] + 0.587 * image.Data[i, j, 1] + 0.114 * image.Data[i,j,0]);

                }
            }
            return gray;
        }
    }
}
