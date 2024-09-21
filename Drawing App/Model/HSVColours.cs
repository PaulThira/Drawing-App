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
        public HSVColours( List<Color> h)
        {
            this.Colors = h;
            harmony = false;
        }
        
        public Tuple<float, float, float> BGRtoHSV(Color color)
        {

            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            float cmax = (float)Math.Max(r, Math.Max(g, b));
            float cmin = (float)Math.Min(r, Math.Min(g, b));
            float delta = cmax - cmin;

            float h = 0;
            float s = 0;
            float v = cmax;

            // Calculate Hue
            if (delta == 0)
            {
                h = 0; // If delta is 0, it's gray, so hue is undefined but we set it to 0
            }
            else if (cmax == r)
            {
                h = (float)(60 * (((g - b) / delta) % 6));
            }
            else if (cmax == g)
            {
                h = (float)(60 * (((b - r) / delta) + 2));
            }
            else if (cmax == b)
            {
                h = (float)(60 * (((r - g) / delta) + 4));
            }

            // Ensure hue is positive
            if (h < 0)
            {
                h += 360;
            }

            // Calculate Saturation
            if (cmax != 0)
            {
                s = delta / cmax;
            }

            // Value is already assigned to cmax
            return new Tuple<float, float, float>(h, s, v);
        }
        public Color ColorFromHSV(double h, double s, double v)
        {
            // Value is in the range [0, 255]

            // Normalize saturation and value to [0, 1]
            h = h %360;

            float C =(float)( s * v); // Chroma
            float X =(float)( C * (1 - Math.Abs((h / 60.0f) % 2 - 1))); // Intermediate value
            float m =(float)( v - C); // Match value

            float r = 0, g = 0, b = 0;

            // Calculate RGB values based on hue range
            if ( h < 60)
            {
                r = C;
                g = X;
                b = 0;
            }
            else if ( h < 120)
            {
                r = X;
                g = C;
                b = 0;
            }
            else if ( h < 180)
            {
                r = 0;
                g = C;
                b = X;
            }
            else if ( h < 240)
            {
                r = 0;
                g = X;
                b = C;
            }
            else if (h < 300)
            {
                r = X;
                g = 0;
                b = C;
            }
            else 
            {
                r = C;
                g = 0;
                b = X;
            }
            r += m; g += m; b += m;
            
            return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
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
            List<float> difs=new List<float>();
            
            for (int i= 0;i< hsvs.Count; i++)
            {
                for (int j = i+1; j<hsvs.Count;j++){
                    var dif = Math.Abs(hsvs[j].Item1 - hsvs[i].Item1);
                    if (dif>180)
                    {
                        dif = 360 - dif;
                    }
                    difs.Add(dif);

                }

            }
            if(difs.Count <= 1) { return; }
            minhue = difs.Min();
            maxhue = difs.Max();
            if (Math.Abs(minhue - 180) <= 10)
            {
                minhue = 0;
            }
            if (minhue <= 10)
            {
                if (Math.Abs(maxhue - 30) <= 10)
                {
                    harmony = true;

                }
                else if (Math.Abs(maxhue - 60) <= 10)
                {
                    harmony = true;

                }
                else if (Math.Abs(maxhue - 90) <= 10)
                {
                    harmony = true;

                }
                else if (Math.Abs(maxhue - 120) <= 10)
                {
                    harmony = true;

                }
                else if (Math.Abs(maxhue - 180) <= 10)
                {
                    harmony = true;

                }
                else
                {
                    harmony = false;

                }
            }
            else
            {
                harmony=false;
            }
            
            
        }

    }
}
