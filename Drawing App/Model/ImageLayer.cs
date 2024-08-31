using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Drawing_App.Model
{
    public class ImageLayer : Layer
    {
        private readonly Image _image;



        public override UIElement VisualElement => _image;

        

        

        public ImageLayer(string imagePath, double opacity = 1.0, bool isVisible = true, string name="Layer")
            : base(opacity, isVisible, name)
        {
            _image = new Image
            {
                Source = new BitmapImage(new Uri(imagePath)),
                Width = 665,
                Height = 563
            };
         
        }
        public override void Redo()
        {
            
        }
        public override void Undo()
        {

        }
    }
}
