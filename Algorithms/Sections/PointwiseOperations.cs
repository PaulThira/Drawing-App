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
    public class PointwiseOperations
    {
        public int[] blue { get; set; }
        public int[] red { get; set; }
        public int[] green { get; set; }
        public PointwiseOperations(int[] blue, int[] green, int[] red)
        {
            this.blue = blue;
            this.red = red;
            this.green = green;
        }
        public PointwiseOperations()
        {
           
        }

        public Image<Bgr,byte> ApplyLUT(Image<Bgr,byte> image)
        {
            Image<Bgr,byte> result=new Image<Bgr, byte>(image.Width, image.Height);
            for(int i = 0; i < image.Height; i++)
            {
                for(int j=0; j < image.Width; j++)
                {
                    result.Data[i, j, 0] =(byte) blue[image.Data[i, j,0]];
                    result.Data[i, j, 1] = (byte)green[image.Data[i, j, 1]];
                    result.Data[i, j, 2] = (byte)red[image.Data[i, j, 2]];

                }
            }
            return result;
        }
    }
}
