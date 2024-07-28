using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace Drawing_App.Model
{
    public class TexturedStroke : Stroke
    {
        private readonly ImageBrush _textureBrush;
        public TexturedStroke(StylusPointCollection stylusPoints, ImageBrush textureBrush)
       : base(stylusPoints)
        {
            _textureBrush = textureBrush;
        }

        public TexturedStroke(StylusPointCollection stylusPoints, DrawingAttributes drawingAttributes) : base(stylusPoints, drawingAttributes)
        {
        }
        protected override void DrawCore(DrawingContext drawingContext, DrawingAttributes drawingAttributes)
        {
            Pen pen = new Pen(_textureBrush, drawingAttributes.Width)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };

            PathGeometry geometry = (PathGeometry)GetGeometry(drawingAttributes);
            drawingContext.DrawGeometry(null, pen, geometry);
        }
    }
}
