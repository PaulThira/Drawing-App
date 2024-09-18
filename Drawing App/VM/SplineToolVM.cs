using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Drawing_App.VM
{
    public class SplineToolVM : BindableBase
    {
        public ObservableCollection<Point> points { get; set; }
        private Canvas _canvas;

        public ICommand SplineToolCommand { get; }
        public SplineToolVM(Canvas canvas)
        {
            SplineToolCommand = new DelegateCommand(DrawSpline);

            points = new ObservableCollection<Point>();
            points.Add(new Point(0, 255));
            points.Add(new Point(255, 0));
            _canvas = canvas;
        }
        public int[] LUT { get; set; }
        public SplineToolVM()
        {
            SplineToolCommand = new DelegateCommand(DrawSpline);

            points = new ObservableCollection<Point>();
            points.Add(new Point(0, 255));
            points.Add(new Point(255, 0));
            _canvas = new Canvas();
            _canvas.Width = 255;
            _canvas.Height = 255;
            LUT = new int[256];
        }
        private List<Point> CalculateBezierCurve(Point p0, Point p1, Point p2, int numPoints = 1000)
        {
            List<Point> bezierPoints = new List<Point>();
            for (int i = 0; i <= numPoints; i++)
            {
                double t = i / (double)numPoints;
                double x = (1 - t) * (1 - t) * p0.X + 2 * (1 - t) * t * p1.X + t * t * p2.X;
                double y = (1 - t) * (1 - t) * p0.Y + 2 * (1 - t) * t * p1.Y + t * t * p2.Y;
                bezierPoints.Add(new Point(x, y));
            }
            return bezierPoints;
        }
        List<Point> CalculateCubicBezierCurve(Point p0, Point p1, Point p2, Point p3)
        {
            List<Point> curvePoints = new List<Point>();

            double tStep = 0.001; // Control the smoothness of the curve, adjust as needed

            for (double t = 0; t <= 1; t += tStep)
            {
                double x = Math.Pow(1 - t, 3) * p0.X +
                           3 * Math.Pow(1 - t, 2) * t * p1.X +
                           3 * (1 - t) * Math.Pow(t, 2) * p2.X +
                           Math.Pow(t, 3) * p3.X;

                double y = Math.Pow(1 - t, 3) * p0.Y +
                           3 * Math.Pow(1 - t, 2) * t * p1.Y +
                           3 * (1 - t) * Math.Pow(t, 2) * p2.Y +
                           Math.Pow(t, 3) * p3.Y;

                curvePoints.Add(new Point(x, y));
            }

            return curvePoints;
        }
        private void DrawSpline()
        {
            if (points.Count < 3)
                return;

            _canvas.Children.Clear(); // Clear the canvas before drawing

            // Create the Path to draw the spline
            var controlPoints = points.OrderBy(p => p.X).ToList();
            int i;
            // Iterate through the points in sets of 4 for cubic Bézier curves
            for (i = 0; i <= controlPoints.Count - 4; i += 3)
            {
                Point p0 = controlPoints[i];
                Point p1 = controlPoints[i + 1];
                Point p2 = controlPoints[i + 2];
                Point p3 = controlPoints[i + 3];

                // Calculate cubic Bézier curve points
                List<Point> bezierPoints = CalculateCubicBezierCurve(p0, p1, p2, p3);

                // Create a Polyline to represent the Bézier curve
                Polyline bezierPolyline = new Polyline
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };

                // Add all the Bézier curve points to the polyline
                foreach (var bezierPoint in bezierPoints)
                {
                    bezierPolyline.Points.Add(bezierPoint);
                }

                _canvas.Children.Add(bezierPolyline); // Add the curve to the canvas
            }

            // Optional: If there are leftover points (e.g., the last point isn't used in a full segment), 
            // connect them with a simple line
            if (controlPoints.Count - i == 3)
            {

                Point p0 = controlPoints[i];
                Point p1 = controlPoints[i + 1];
                Point p2 = controlPoints[i + 2];

                // Calculate quadratic Bézier curve points
                List<Point> quadraticBezierPoints = CalculateBezierCurve(p0, p1, p2);


                // Create a Polyline to represent the Bézier curve
                Polyline quadraticBezierPolyline = new Polyline
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };
                Polyline quadraticBezierPolyline2 = new Polyline
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };
                // Add all the quadratic Bézier curve points to the polyline
                foreach (var bezierPoint in quadraticBezierPoints)
                {
                    quadraticBezierPolyline.Points.Add(bezierPoint);
                }


                _canvas.Children.Add(quadraticBezierPolyline);
                // Add the curve to the canvas
            }
            
            if (controlPoints.Count - i == 2)
            {
                Point p0 = controlPoints[i];
                Point p1 = controlPoints[i + 1];

                // Create a Line to connect the two points
                Line line = new Line
                {
                    X1 = p0.X,
                    Y1 = p0.Y,
                    X2 = p1.X,
                    Y2 = p1.Y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };

                _canvas.Children.Add(line); // Add the line to the canvas
            }
            CalculateLUT();
            int d = LUT[10];
            d++;

        }
        
        private void CalculateLUT()
        {
            LUT = new int[256];
            foreach (UIElement element in _canvas.Children)
            {
                
                 if (element is Polyline polyline) { 
                    var points= polyline.Points;
                    foreach (var p in points) {
                        
                        int i = (int)Math.Round(p.X);
                        if (p.X - Math.Truncate(p.X) <= 0.3)
                        {
                            int k = (int)(255 - Math.Round(p.Y));
                            LUT[i] = k;
                        }
                        
                    }
                }
                else if (element is Line line)
                {
                    var p1 = new Point(line.X1, line.Y1);
                    var p2 = new Point(line.X2, line.Y2);
                    List<Point> interpolatedPoints = new List<Point>();

                    for (int step = 0; step <= 1000; step++)
                    {
                        // Compute the interpolation factor t
                        double t = (double)step / 1000;

                        // Linearly interpolate between p1 and p2
                        double x_t = (1 - t) * p1.X + t * p2.X;
                        double y_t = (1 - t) * p1.Y + t * p2.Y;
                        if (x_t - Math.Truncate(x_t)<0.3){
                            int k=(int)Math.Round(x_t);
                            LUT[k] = (int)(255-Math.Round(y_t));

                        }

                        // Add the interpolated point to the list
                        interpolatedPoints.Add(new Point(x_t, y_t));
                    }
                }
            }
            for (int i = 1; i < 255; i++) {
                if (LUT[i] == 0) {
                    LUT[i] = (int)((LUT[i - 1] + LUT[i+1])/2);
                }
            }
            LUT[255] = 255;

        }
    }
}
