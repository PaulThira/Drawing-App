using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Color = System.Windows.Media.Color;

namespace Drawing_App.Model
{
    public class HSVColours
    {
        public List<Color> Colors { get; set; }
        public bool harmony {  get; set; }
        public HSVColours() { 
         Colors = new List<Color>();
            harmony = false;
        }
        public Tuple<float, float, float> BGRtoHSV(Color color)
        {
            double r = color.R, g = color.G, b = color.B;
            float red = (float)(r / 255), green = (float)g / 255, blue = (float)b / 255;
            float cmax = Math.Max(red, Math.Max(blue, green));
            float cmin = Math.Min(red, Math.Min(blue, green));
            float d = cmax - cmin;
            float h, s, v;
            h = 0;
            if (d == 0)
            {
                h = (float)0;
            }
            else
            {
                if (cmax == red)
                {
                    h = (float)60 * (((green - blue) / d) % 60);
                }
                
                else if (cmax == green)
                {
                    h = (float)60 * ((blue - red) / d + 2);
                }
                else if (cmax == blue)
                {
                    h = (float)60 * ((green - red) / d + 4);
                }

            }
            if (cmax == 0)
            {
                s = 0;
            }
            else
            {
                s = (float)cmax / d;
            }
            v = cmax;
            Tuple<float, float, float> hsv = new Tuple<float, float, float>(h, s, v);
            return hsv;
        }
        public void ColorHarmony()
        {
            List<Tuple<float,float,float>>hsvs=new List<Tuple<float,float,float>>();
            foreach(Color color in Colors)
            {
              var hsv=BGRtoHSV(color);
                hsvs.Add(hsv);

            }
            float maxhue=0, minhue=361;
            foreach(Tuple<float,float,float> hsv in hsvs)
            {
                if(hsv.Item1 < minhue)
                {
                    minhue = hsv.Item1;
                }
                if(hsv.Item1 >maxhue)
                {
                    maxhue = hsv.Item1;
                }
            }
            float range=Math.Abs(180-Math.Abs(maxhue-minhue));
            if (Math.Abs(range - 30) <= 10)
            {
                harmony = true;
                MessageBox.Show("Nice Colors!!!");
            }
            else if(Math.Abs(range -60)<=10)
            {
                harmony = true;
                MessageBox.Show("Nice Colors!!!");
            }
            else if (Math.Abs(range - 90) <= 10)
            {
                harmony = true;
                MessageBox.Show("Nice Colors!!!");
            }
            else if (Math.Abs(range - 120) <= 10)
            {
                harmony = true;
                MessageBox.Show("Nice Colors!!!");
            }
            else if (Math.Abs(range - 180) <= 10)
            {
                harmony = true;
                MessageBox.Show("Nice Colors!!!");
            }
            else
            {
                harmony=false;
                MessageBox.Show("Kind of muddy:(((");
            }
        }

    }
}
