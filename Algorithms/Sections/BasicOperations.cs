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
        public int[] blue { get; set; }
        public int[] red { get; set; }
        public int[] green { get; set; }
        public int[] value { get; set; }
        public void HistogramCalc(Image<Bgr, Byte> image)
        {
            blue = new int[256];
            green = new int[256];
            red = new int[256];

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    int b, g, r;
                    b = image.Data[i, j, 0];
                    g = image.Data[i, j, 1];
                    r = image.Data[i, j, 2];
                    blue[b]++;
                    green[g]++;
                    red[r]++;

                }
            }


        }
        public Image<Bgr, byte> HsvToBgr(Image<Hsv, byte> hsvImage)
        {
            Image<Bgr, byte> bgrImage = new Image<Bgr, byte>(hsvImage.Width, hsvImage.Height);

            for (int i = 0; i < hsvImage.Height; i++)
            {
                for (int j = 0; j < hsvImage.Width; j++)
                {
                    // Extract HSV values
                    byte h = hsvImage.Data[i, j, 0]; // Hue is in the range [0, 180]
                    byte s = hsvImage.Data[i, j, 1]; // Saturation is in the range [0, 255]
                    byte v = hsvImage.Data[i, j, 2]; // Value is in the range [0, 255]

                    // Normalize saturation and value to [0, 1]
                    float S = s / 255.0f;
                    float V = v / 255.0f;

                    float C = S * V; // Chroma
                    float X = C * (1 - Math.Abs((h / 30.0f) % 2 - 1)); // Intermediate value
                    float m = V - C; // Match value

                    float r = 0, g = 0, b = 0;

                    // Calculate RGB values based on hue range
                    if (0 <= h && h < 30)
                    {
                        r = C;
                        g = X;
                        b = 0;
                    }
                    else if (30 <= h && h < 60)
                    {
                        r = X;
                        g = C;
                        b = 0;
                    }
                    else if (60 <= h && h < 90)
                    {
                        r = 0;
                        g = C;
                        b = X;
                    }
                    else if (90 <= h && h < 120)
                    {
                        r = 0;
                        g = X;
                        b = C;
                    }
                    else if (120 <= h && h < 150)
                    {
                        r = X;
                        g = 0;
                        b = C;
                    }
                    else if (150 <= h && h < 180)
                    {
                        r = C;
                        g = 0;
                        b = X;
                    }

                    // Convert to [0, 255] range and store in BGR image
                    bgrImage.Data[i, j, 0] = (byte)((b + m) * 255); // Blue
                    bgrImage.Data[i, j, 1] = (byte)((g + m) * 255); // Green
                    bgrImage.Data[i, j, 2] = (byte)((r + m) * 255); // Red
                }
            }

            return bgrImage;
        }

        public void HistogramCalcValue(Image<Hsv, Byte> image) { 
             value = new int[256];
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    int v;
                    v = image.Data[j,i, 2];
                    value[v]++;

                }
            }

        }
        public Image<Bgr,byte> Negative(Image<Hsv, byte> image) { 
            Image<Bgr,byte> result=new Image<Bgr, byte>(image.Width, image.Height);
            for(int i=0;i<result.Height; i++)
            {
                for(int j=0;j<result.Width; j++)
                {
                    result.Data[i,j,0]= (byte)(255 -image.Data[i,j,0]);
                    result.Data[i,j,1]=(byte)(255-image.Data[i,j,1]);
                    result.Data[i, j, 2] = (byte)(255 - image.Data[i,j,2]);
                }
            }
            return result;
        
        }
        public Image<Bgr,byte> HistogramEqualisation(Image<Bgr, Byte> image)
        {
            Image<Hsv,byte> hsv=BgrToHSV(image);
            HistogramCalcValue(hsv);
            int[] cummualtive = new int[256];
            cummualtive[0] = value[0];
            Image<Hsv,byte> equalize=new Image<Hsv, byte>(image.Width, image.Height);
            for (int i = 1; i < 256; i++)
            {
                cummualtive[i] = value[i]+cummualtive[i-1];
            }
            int totalPixels = hsv.Width * hsv.Height;
            byte[] equalizedLUT = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                if (totalPixels == cummualtive[0])
                {
                    equalizedLUT[i] = 0;
                }
                else
                {
                    equalizedLUT[i] = (byte)((cummualtive[i] - cummualtive[0]) * 255 / (totalPixels - cummualtive[0]));
                }
                
            }
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    equalize.Data[i, j,0]=hsv.Data[i, j,0];
                    equalize.Data[i,j,1]=hsv.Data[i,j,1];
                    equalize.Data[i, j, 2] = equalizedLUT[hsv.Data[i,j,2]];
                }
            }
            return HsvToBgr(equalize);
            
        }

        public Image<Gray, Byte> BgrToGrayscale(Image<Bgr, Byte> image)
        {
            Image<Gray, Byte> gray = new Image<Gray, Byte>(image.Width, image.Height);
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {

                    // Use BGR to Grayscale conversion formula
                    gray.Data[i, j, 0] = (Byte)(0.299 * image.Data[i, j, 2] +
                                                0.587 * image.Data[i, j, 1] +
                                                0.114 * image.Data[i, j, 0]);
                }
            }
            return gray;
        }
        public Image<Hsv, byte> BgrToHSV(Image<Bgr, byte> image)
        {
            Image<Hsv, byte> hsv = new Image<Hsv, byte>(image.Width, image.Height);
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    double b, g, r;
                   
                    b =  image.Data[i, j, 0]/255;
                    g = image.Data[i, j, 1]/255;
                    r = image.Data[i, j, 2] / 255;
                    double Cmax = Math.Max(r, Math.Max(g, b));
                    double Cmin = Math.Min(r, Math.Min(g, b));
                    double d = Cmax - Cmin;
                    double h = 0,s=0,v=0;
                    if (d == 0)
                    {

                    h = 0; 
                    
                    }
                    else if (Cmax == r)
                    {
                        h = (Math.PI / 3) * (((g - b) / d) % 6);
                    }
                    else if (Cmax == g)
                    {
                        h = (Math.PI / 3) * (((b-r) / d) + 2);
                    }
                    else if (Cmax == b) { 
                         h = (Math.PI / 3) * (((r-g) / d) +4);
                    }
                    if(h<0) { h += 360; }
                    if (Cmax == 0)
                    {
                        s=0;
                    }
                    else
                    {
                        s=d/Cmax;
                    }
                    v = Cmax;
                    hsv.Data[i, j, 0] = (byte)(h/2);
                    hsv.Data[i, j, 1] = (byte)(s*255);
                    hsv.Data[i,j,2] = (byte)(v*255);

                }
            }
            return hsv;
        }
    }
}
