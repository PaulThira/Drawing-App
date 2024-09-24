using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Drawing_App.Model
{
    public  class ColorPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public SolidColorBrush Color { get; set; }
        public SolidColorBrush Stroked { get; set; }
        public ColorPoint() {
            X = 0;
            Y = 0;
            Color=new SolidColorBrush(Colors.Black);
        
        }
        public ColorPoint( double x,double y, SolidColorBrush colorBrush)
        {
            X = x;
            Y = y;
            Color = colorBrush;
            Stroked = new SolidColorBrush(Colors.Transparent);

        }
    }
}
