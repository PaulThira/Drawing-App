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

namespace Drawing_App.VM
{
    public class SplineToolVM:BindableBase
    {
        public ObservableCollection<Point> points {  get; set; }
        private Canvas _canvas;
        
        public ICommand SplineToolCommand { get; }
        public SplineToolVM(Canvas canvas) {
            SplineToolCommand = new DelegateCommand(DrawSpline);

            points = new ObservableCollection<Point>();
            points.Add(new Point(0, 255));
            points.Add(new Point(255,0)); 
            _canvas = canvas;
        }
        public SplineToolVM() {
            SplineToolCommand = new DelegateCommand(DrawSpline);

            points = new ObservableCollection<Point>();
            points.Add(new Point(0, 255));
            points.Add(new Point(255, 0));
            _canvas = new Canvas();
            _canvas.Width = 255;
            _canvas.Height = 255;
        }
        private List<Point> CalculateBezierCurve(Point p0, Point p1, Point p2, int numPoints = 100)
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

            double tStep = 0.01; // Control the smoothness of the curve, adjust as needed

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
            for ( i = 0; i <= controlPoints.Count - 4; i += 3)
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
                Point pk = controlPoints[i - 1];
                Point p0 = controlPoints[i];
                Point p1 = controlPoints[i + 1];
                Point p2 = controlPoints[i + 2];

                // Calculate quadratic Bézier curve points
                List<Point> quadraticBezierPoints = CalculateBezierCurve(p0, p1, p2);
                List<Point> quadraticBezierPoints2 = CalculateBezierCurve(pk, p0, p1);

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
                foreach (var bezierPoint in quadraticBezierPoints2)
                {
                    quadraticBezierPolyline2.Points.Add(bezierPoint);
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

        }
    
}
}
