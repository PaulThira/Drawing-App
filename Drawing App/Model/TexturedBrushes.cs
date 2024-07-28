using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Drawing_App.Model
{
    public class TexturedBrushes:DrawingAttributes

    {
        public string url { get; set; } 
        public TexturedBrushes()
        {

        }
        public TexturedBrushes(DrawingAttributes d, string url)
        {
            this.url = url;
            this.Color=d.Color;
            this.Width=d.Width;
            this.Height=d.Height;
            this.StylusTip=d.StylusTip;

        }
        public ImageBrush GetTextureBrush()
        {
            if (string.IsNullOrEmpty(url))
                return null;

            BitmapImage bitmap = new BitmapImage(new Uri(url));
            return new ImageBrush(bitmap);
        }
    }
}
