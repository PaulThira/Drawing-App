using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Emgu.CV;
using Emgu.CV.Structure;
using Drawing_App;
using Drawing_App.View;

namespace Algorithms.Sections
{
    public class BasicOperations
    {
        public void HistogramCalc( Image<Bgr,Byte> image)
        {
            int[] blue = new int[256];
            int[] green = new int[256];
            int[] red = new int[256];
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
            Histogram histogram=new Histogram(blue, green, red);
            histogram.Show();

        }
    }
}
