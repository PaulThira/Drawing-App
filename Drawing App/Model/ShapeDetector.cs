using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using Drawing_App.VM;
using System.Drawing;
using Point = System.Windows.Point;
using System.Xml.Linq;

namespace Drawing_App.Model
{
    public class ShapeDetector
    {
        private readonly double _tolerance;

        public ShapeDetector(double tolerance = 0.1)
        {
            _tolerance = tolerance;
        }

        public bool IsRectangle(PointCollection points)
        {
            if (points.Count < 4)
                return false;

            // Identify the starting and ending points
            Point startPoint = points.First();
            Point endPoint = points.Last();

            // Find the furthest point from the start point
            Point furthestPoint = points.OrderByDescending(p => Distance(startPoint, p)).First();

            // Calculate the pseudo-corners
            Point pseudoCorner1 = new Point(startPoint.X, furthestPoint.Y);
            Point pseudoCorner2 = new Point(furthestPoint.X, startPoint.Y);

            // Check if points close to pseudo-corners exist
            bool hasPseudoCorner1 = points.Any(p => Distance(p, pseudoCorner1) < _tolerance*100);
            bool hasPseudoCorner2 = points.Any(p => Distance(p, pseudoCorner2) < _tolerance*100);

            if (hasPseudoCorner1 && hasPseudoCorner2)
            {
                // Validate the shape as a rectangle
                double side1 = Distance(startPoint, pseudoCorner1);
                double side2 = Distance(pseudoCorner1, furthestPoint);
                double side3 = Distance(furthestPoint, pseudoCorner2);
                double side4 = Distance(pseudoCorner2, startPoint);

                double diagonal1 = Distance(startPoint, furthestPoint);
                double diagonal2 = Distance(pseudoCorner1, pseudoCorner2);

                // Check if opposite sides and diagonals are roughly equal
                return AreCloseEnough(side1, side3) && AreCloseEnough(side2, side4) && AreCloseEnough(diagonal1, diagonal2);
            }

            return false;
        }

        public bool IsEllipse(PointCollection points)
        {
            if (points.Count < 4)
                return false;

            // Identify the starting and ending points
            Point startPoint = points.First();
            Point endPoint = points.Last();

            // Find the furthest point from the start point
            Point furthestPoint = points.OrderByDescending(p => Distance(startPoint, p)).First();

            // Calculate the bounding box center
            Point center = new Point((startPoint.X + furthestPoint.X) / 2, (startPoint.Y + furthestPoint.Y) / 2);

            // Calculate the semi-major axis (a) and semi-minor axis (b)
            double a = Distance(startPoint, furthestPoint) / 2;
            double b = Distance(points.OrderBy(p => Distance(center, p)).First(), center); // Approximation

            // Check if the points fit the ellipse equation
            foreach (var point in points)
            {
                double ellipseEquation = Math.Pow(point.X - center.X, 2) / Math.Pow(a, 2) +
                                         Math.Pow(point.Y - center.Y, 2) / Math.Pow(b, 2);

                if (Math.Abs(ellipseEquation - 1) > _tolerance*100)  // Use a small tolerance for approximation
                {
                    return false;
                }
            }

            return true;
        }
        public bool IsEquilateralTriangle(PointCollection points)
        {
            if (points.Count < 3)
                return false;

            // Step 1: Identify the starting and ending points
            Point startPoint = points.First();
            Point endPoint = points.Last();

            // Step 2: Check if the shape is closed
            if (Distance(startPoint, endPoint) > _tolerance * 100)
                return false;

            // Step 3: Find the furthest point from the start point
            Point furthestPoint = points.OrderByDescending(p => Distance(startPoint, p)).First();

            // Step 4: Identify the third point (it should be the point that is closest to equidistant from both start and furthest points)
            Point thirdPoint = points.FirstOrDefault(p => p != startPoint && p != furthestPoint &&
                Math.Abs(Distance(startPoint, p) - Distance(furthestPoint, p)) < _tolerance);

            if (thirdPoint == null)
                return false;

            // Step 5: Calculate distances between the three points
            double side1 = Distance(startPoint, furthestPoint);
            double side2 = Distance(furthestPoint, thirdPoint);
            double side3 = Distance(thirdPoint, startPoint);

            // Step 6: Validate the shape as an equilateral triangle
            return AreCloseEnoughTriangle(side1, side2) && AreCloseEnoughTriangle(side2, side3);
        }
        public double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        public bool AreCloseEnough(double value1, double value2)
        {
            return Math.Abs(value1 - value2) < _tolerance;
        }
        public bool AreCloseEnoughTriangle(double value1, double value2)
        {
            return Math.Abs(value1 - value2) < _tolerance * 100;
        }
    }
}
