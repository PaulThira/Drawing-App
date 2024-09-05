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
using Emgu.CV.Linemod;
namespace Drawing_App.Model
{
   public class DrawingLayer:Layer
    {
        private readonly Canvas _canvas;

        // Flags to track if a stroke is currently being drawn
        private bool _isDrawing;
        private ShapeDetector _detector { get; set; }
        // The starting point of the current stroke
        private Point _startPoint;
        private Shape _currentShape;
        private Stack<UIElement> _undoStack = new Stack<UIElement>();
        private Stack<UIElement> _redoStack = new Stack<UIElement>();
        // The current stroke being drawn (as a Polyline)
        private Polyline _currentPolyline;

        // Commands to interact with the ViewModel
        public ICommand StartStrokeCommand { get; }
        public ICommand ContinueStrokeCommand { get; }
        public ICommand EndStrokeCommand { get; }
        public Brush _currentBrush { get; set; }
        public double thickness { get; set; }
        public override UIElement VisualElement => _canvas;

        public DrawingLayer(ICommand startStrokeCommand, ICommand continueStrokeCommand, ICommand endStrokeCommand ,double opacity = 1.0, bool isVisible = true, string name="Layer")
            : base(opacity, isVisible, name)
        {
            StartStrokeCommand = startStrokeCommand;
            ContinueStrokeCommand = continueStrokeCommand;
            EndStrokeCommand = endStrokeCommand;
            _canvas = new Canvas
            {
                Background = Brushes.White,
                Width = 665,
                Height = 563
            };
            _canvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
            _canvas.MouseMove += Canvas_MouseMove;
            _canvas.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;
            _currentBrush = new SolidColorBrush(Colors.Black);
            thickness = 5;
            _detector = new ShapeDetector();
        }
        public void ExecuteDrawingAction(UIElement element)
        {
            
            _undoStack.Push(element);
            _redoStack.Clear(); // Clear redo stack on new action
        }
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = true;
            _startPoint = e.GetPosition(_canvas);
            StartStrokeCommand?.Execute(_startPoint);
            
        }

        // Event handler for continuing a stroke
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                Point currentPoint = e.GetPosition(_canvas);
                ContinueStrokeCommand?.Execute(currentPoint);
            }
        }
        public void StartShape(Point startPoint, ShapeKind shapeType)
        {
            _startPoint = startPoint;

            if (shapeType == ShapeKind.Rectangle)
            {
                _currentShape = new System.Windows.Shapes.Rectangle
                {
                    Stroke = _currentBrush,
                    StrokeThickness = thickness,
                    Fill = Brushes.Transparent
                };
            }
            else if (shapeType == ShapeKind.Ellipse)
            {
                _currentShape = new Ellipse
                {
                    Stroke = _currentBrush,
                    StrokeThickness = thickness,
                    Fill = Brushes.Transparent
                };
            }
            else if (shapeType == ShapeKind.Line)
            {
                _currentShape = new Line
                {
                    Stroke = _currentBrush,
                    StrokeThickness = thickness,
                    X1 = startPoint.X,
                    Y1 = startPoint.Y
                };
            }
           
            if (_currentShape != null)
            {
                
                _canvas.Children.Add(_currentShape);
            }
        }
        public void DrawShape(Point startPoint, ShapeKind shapeType)
        {
            double size = thickness * 3;
            _startPoint = startPoint;

            if (shapeType == ShapeKind.Circle)
            {
                _currentShape = new Ellipse
                {
                    Stroke = _currentBrush,
                    StrokeThickness = 1, // You can set this differently if needed
                    Width = size,  // Diameter
                    Height = size, // Diameter
                    Fill = Brushes.Transparent
                };
            }
            else if (shapeType == ShapeKind.Square)
            {
                _currentShape = new System.Windows.Shapes.Rectangle
                {
                    Stroke = _currentBrush,
                    StrokeThickness = 1, // You can set this differently if needed
                    Width = size,  // Side length
                    Height = size, // Side length
                    Fill = Brushes.Transparent
                };
            }
            else if (shapeType == ShapeKind.Heart)
            {
                PathFigure heartFigure = new PathFigure
                {
                    StartPoint = new Point(startPoint.X, startPoint.Y - size / 4)  // Top center of the heart
                };

                // Left side of the heart using a cubic Bézier curve
                heartFigure.Segments.Add(new BezierSegment(
                    new Point(startPoint.X - size / 2, startPoint.Y - size / 2),  // Control Point 1
                    new Point(startPoint.X - size / 2, startPoint.Y + size / 4),  // Control Point 2
                    new Point(startPoint.X, startPoint.Y + size / 2),             // End Point
                    true));

                // Right side of the heart using a cubic Bézier curve
                heartFigure.Segments.Add(new BezierSegment(
                    new Point(startPoint.X + size / 2, startPoint.Y + size / 4),  // Control Point 1
                    new Point(startPoint.X + size / 2, startPoint.Y - size / 2),  // Control Point 2
                    new Point(startPoint.X, startPoint.Y - size / 4),             // End Point (back to top center)
                    true));

                PathGeometry heartGeometry = new PathGeometry();
                heartGeometry.Figures.Add(heartFigure);

                _currentShape = new Path
                {
                    Stroke = _currentBrush,
                    StrokeThickness = 1,
                    Data = heartGeometry,
                    Fill = Brushes.Transparent
                };

                _canvas.Children.Add(_currentShape);
            }
            else if (shapeType == ShapeKind.Triangle)
            {
                double height = Math.Sqrt(3) / 2 * size;

                // Calculate the vertices of the triangle
                Point vertex1 = new Point(startPoint.X, startPoint.Y - 2 * height / 3);  // Top vertex
                Point vertex2 = new Point(startPoint.X - size / 2, startPoint.Y + height / 3);  // Bottom left vertex
                Point vertex3 = new Point(startPoint.X + size / 2, startPoint.Y + height / 3);  // Bottom right vertex

                // Create a polygon representing the equilateral triangle
                _currentShape = new Polygon
                {
                    Stroke = _currentBrush,
                    StrokeThickness = 5, // You can set this differently if needed
                    Fill = Brushes.Transparent,
                    Points = new PointCollection { vertex1, vertex2, vertex3 }
                };

                // Add the triangle to the canvas
                _canvas.Children.Add(_currentShape);
            }

            if (_currentShape != null && shapeType != ShapeKind.Heart&& shapeType != ShapeKind.Triangle)
            {
                Canvas.SetLeft(_currentShape, startPoint.X - size / 2);
                Canvas.SetTop(_currentShape, startPoint.Y - size / 2);
                _canvas.Children.Add(_currentShape);
            }
            if (_currentShape != null)
            {
                ExecuteDrawingAction(_currentShape);
            }

        }

        public void EndShape(Point endPoint)
        {
            if (_currentShape == null) return;

            if (_currentShape is System.Windows.Shapes.Rectangle || _currentShape is Ellipse)
            {
                double width = Math.Abs(endPoint.X - _startPoint.X);
                double height = Math.Abs(endPoint.Y - _startPoint.Y);

                _currentShape.Width = width;
                _currentShape.Height = height;

                Canvas.SetLeft(_currentShape, Math.Min(_startPoint.X, endPoint.X));
                Canvas.SetTop(_currentShape, Math.Min(_startPoint.Y, endPoint.Y));
            }
            else if (_currentShape is Line line)
            {
                line.X2 = endPoint.X;
                line.Y2 = endPoint.Y;
            }
            ExecuteDrawingAction(_currentShape);

            _currentShape = null;
        }
        // Event handler for ending a stroke
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
            EndStrokeCommand?.Execute(null);
        }
        public override void Undo()
        {
            if (_undoStack.Count > 0)
            {
                var lastAction = _undoStack.Pop();
                _canvas.Children.Remove(lastAction);
                _redoStack.Push(lastAction);
            }
        }

        // Override the Redo method from the Layer class
        public override void Redo()
        {
            if (_redoStack.Count > 0)
            {
                var lastUndoneAction = _redoStack.Pop();
                _canvas.Children.Add(lastUndoneAction);
                _undoStack.Push(lastUndoneAction);
            }
        }

        // Method to start a new stroke
        public void StartStroke(Point startPoint)
        {
            _currentPolyline = new Polyline
            {
                Stroke = _currentBrush, // Default color, could be set via a method or property
                StrokeThickness = thickness, // Default thickness, could be set via a method or property
                Points = new PointCollection { startPoint }
            };
            _canvas.Children.Add(_currentPolyline);
            ExecuteDrawingAction(_currentPolyline);
        }

        // Method to continue the current stroke
        public void ContinueStroke(Point currentPoint)
        {
            if (_currentPolyline != null)
            {
                _currentPolyline.Points.Add(currentPoint);
            }
        }

        // Method to finalize the current stroke
        public void EndStroke()
        {
            var points = _currentPolyline.Points;
            
            // Detect shapes using the ShapeDetector
            if (_detector.IsRectangle(points))
            {
                // Replace the polyline with a rectangle
                var startPoint = points.First();
                var furthestPoint = points.OrderByDescending(p => _detector.Distance(startPoint, p)).First();
                StartShape(startPoint, ShapeKind.Rectangle);
                EndShape(furthestPoint);
                _canvas.Children.Remove(_currentPolyline);
            }
            else if (_detector.IsEllipse(points))
            {
                // Replace the polyline with an ellipse
                var startPoint = points.First();
                var furthestPoint = points.OrderByDescending(p => _detector.Distance(startPoint, p)).First();
                StartShape(startPoint, ShapeKind.Ellipse);
                EndShape(furthestPoint);
                _canvas.Children.Remove(_currentPolyline);
            }
            else if (_detector.IsEquilateralTriangle(points))
            {
                // Replace the polyline with a triangle
                DrawShape(points.First(),ShapeKind.Triangle);
                _canvas.Children.Remove(_currentPolyline);
            }

            _currentPolyline = null;
        }

        // Method to configure the brush and stroke thickness (optional, for more flexibility)
        public void SetBrush(Brush brush, double thick)
        {
            _currentBrush=brush;
            thickness = thick;
        }
    }
}
