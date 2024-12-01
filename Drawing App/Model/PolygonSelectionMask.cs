using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drawing_App.Model
{
    public class PolygonSelectionMask
    {
        public List<Point> BoundaryPoints { get; private set; }

        public PolygonSelectionMask(IEnumerable<Point> points)
        {
            BoundaryPoints = new List<Point>(points);
        }

        /// <summary>
        /// Determines if a given point is inside the polygon using the ray-casting algorithm.
        /// </summary>
        public bool IsPointInside(Point p)
        {
            bool isInside = false;
            int count = BoundaryPoints.Count;


            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                if ((BoundaryPoints[i].Y > p.Y) != (BoundaryPoints[j].Y > p.Y) &&
                    (p.X < (BoundaryPoints[j].X - BoundaryPoints[i].X) * (p.Y - BoundaryPoints[i].Y) /
                     (BoundaryPoints[j].Y - BoundaryPoints[i].Y) + BoundaryPoints[i].X))
                {
                    isInside = !isInside;
                }
            }

            return isInside;
        }
    }
}
