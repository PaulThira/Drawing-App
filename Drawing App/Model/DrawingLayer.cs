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
using Brushes = System.Windows.Media.Brushes;
using Brush = System.Windows.Media.Brush;
using Emgu.CV.Linemod;
using System.Security.Cryptography.Xml;
using Rectangle = System.Windows.Shapes.Rectangle;
namespace Drawing_App.Model

{
   public class DrawingLayer:Layer
    {
        public Canvas _canvas { get; set; }
        private bool _isResizing = false;
        private string _resizeDirection = "";
        // Flags to track if a stroke is currently being drawn
        private bool _isDrawing;
        private Rectangle _selectionRectangle;
        private Rectangle _boundingBox;
        private ShapeDetector _detector { get; set; }
        // The starting point of the current stroke
        private Point _startPoint;
        private Shape _currentShape;
        private Stack<UIElement> _undoStack = new Stack<UIElement>();
        private Stack<UIElement> _redoStack = new Stack<UIElement>();
        // The current stroke being drawn (as a Polyline)
        private Polyline _currentPolyline;
        private Polyline _mirroredPolyline;
        private Point lastPoint;
        private Point? secondLastPoint;
        private int ResizeDistanceThreshold = 55;
        public bool corectShapes {  get; set; }

        // Commands to interact with the ViewModel
        public ICommand StartStrokeCommand { get; }
        public ICommand ContinueStrokeCommand { get; }
        private List<Point> _originalPolygonPoints;
        public ICommand EndStrokeCommand { get; }
        public Brush _currentBrush { get; set; }
        public double thickness { get; set; }
        public Shape _selectedShape { get; set; }
        private bool _isDragging = false;
        public override UIElement VisualElement => _canvas;
        private TranslateTransform _transform;
        public override double ZoomLevel { get => base.ZoomLevel; set => base.ZoomLevel = value; }
        private Point _dragStartPoint; // Starting point for dragging
        private double _offsetX, _offsetY;
        public DrawingLayer(ICommand startStrokeCommand, ICommand continueStrokeCommand, ICommand endStrokeCommand ,double opacity = 1.0, bool isVisible = true, string name="Layer")
            : base(opacity, isVisible, name)
        {
            StartStrokeCommand = startStrokeCommand;
            ContinueStrokeCommand = continueStrokeCommand;
            EndStrokeCommand = endStrokeCommand;
            _canvas = new Canvas
            {
                Background = Brushes.White,
                Width = 1000,
                Height = 1000
            };
            _canvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
            _canvas.MouseMove += Canvas_MouseMove;
            _canvas.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;
            _canvas.MouseRightButtonDown += Canvas_MouseRightButtonDown;
            _canvas.MouseRightButtonUp += Canvas_MouseRightButtonUp;
            _canvas.KeyDown += Canvas_KeyDown;
            _canvas.Focusable = true; // Ensure the canvas can receive key input
            _canvas.Focus();
            _currentBrush = new SolidColorBrush(Colors.Black);
            thickness = 5;
            _detector = new ShapeDetector();
            corectShapes=true;
        }
        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (_selectedShape != null)
            {
                ResizeShapeWithArrows(e.Key.ToString());
            }
        }
        public override void ZoomIn()
        {
            ZoomLevel += 0.1;
            // Any additional behavior unique to DrawingLayer
            _canvas.LayoutTransform = new ScaleTransform(ZoomLevel, ZoomLevel);
            
        }

        // Override ZoomOut to customize zoom behavior for DrawingLayer
        public override void ZoomOut()
        {
            ZoomLevel = Math.Max(0.1, ZoomLevel - 0.1);
            _canvas.LayoutTransform = new ScaleTransform(ZoomLevel, ZoomLevel);
            
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
        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPoint = e.GetPosition(_canvas);
            double minDistance = double.MaxValue;
            Shape closestShape = null;
            
            // Loop through all the shapes in the canvas
            foreach (UIElement element in _canvas.Children)
            {
                if (element is Shape shape)
                {
                    Point shapeCenter = GetShapeCenter(shape);

                    // Calculate Euclidean distance between the clicked point and the shape's center
                    double distance = GetEuclideanDistance(clickPoint, shapeCenter);

                    // Keep track of the shape with the shortest distance
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestShape = shape;
                    }
                }
            }
           
            // If we found a closest shape, select it
            if (closestShape != null)
            {
                // Optionally highlight the selected shape, for example:
                _selectedShape = closestShape;
                _isDragging = true;
                _boundingBox = GetBoundingBox(_selectedShape);
                _canvas.Children.Add(_boundingBox);
                // Get the mouse's position relative to the canvas
                Point mousePosition = e.GetPosition(_canvas);
                if (_selectedShape is Polygon polygon)
                {
                    // Store the original points of the polygon before dragging
                    _originalPolygonPoints = polygon.Points.ToList();
                    Point mousePos = e.GetPosition(_canvas);
                    _offsetX = mousePos.X;
                    _offsetY = mousePos.Y;
                }
                else
                {
                    _offsetX = mousePosition.X - Canvas.GetLeft(_selectedShape);
                    _offsetY = mousePosition.Y - Canvas.GetTop(_selectedShape);
                }
                // Calculate the offset between the mouse and the shape's position
                
               
                // Capture the mouse to track movements even if it leaves the shape's bounds
                _canvas.CaptureMouse();
            }

        }

        private void StartResizing(Shape shape, Point clickPoint)
        {
            // Example: Adjust bounding box to show resize mode
            _boundingBox.Stroke = Brushes.Green;
            _boundingBox.StrokeThickness = 2;

            // Initialize resizing logic based on shape type
            if (shape is Polygon polygon)
            {
                // Resize polygon-specific logic
                _originalPolygonPoints = polygon.Points.ToList();
            }
            else
            {
                double shapeLeft = Canvas.GetLeft(shape);
                double shapeTop = Canvas.GetTop(shape);
                double shapeRight = shapeLeft + shape.Width;
                double shapeBottom = shapeTop + shape.Height;

                if (Math.Abs(clickPoint.X - shapeLeft) < ResizeDistanceThreshold)
                {
                    _resizeDirection = "Left";
                }
                else if (Math.Abs(clickPoint.X - shapeRight) < ResizeDistanceThreshold)
                {
                    _resizeDirection = "Right";
                }
                if (Math.Abs(clickPoint.Y - shapeTop) < ResizeDistanceThreshold)
                {
                    _resizeDirection += "Top";
                }
                else if (Math.Abs(clickPoint.Y - shapeBottom) < ResizeDistanceThreshold)
                {
                    _resizeDirection += "Bottom";
                }

                _offsetX = clickPoint.X - Canvas.GetLeft(shape);
                _offsetY = clickPoint.Y - Canvas.GetTop(shape);
            }
            _canvas.CaptureMouse();

            // Indicate that resizing has started (could use a flag or call a specific resize method)
            // Additional logic to handle resizing interaction goes here
        }
        private void Canvas_MouseMiddleButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_selectedShape != null)
            {
                _isResizing = true;
                _resizeDirection = "All"; // Resize both width and height proportionally
                _canvas.CaptureMouse();
            }
        }

        private void Canvas_MouseMiddleButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isResizing = false;
            _canvas.ReleaseMouseCapture();
        }
        private void Canvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _canvas.ReleaseMouseCapture();

                if (_selectedShape is Polygon polygon)
                {
                    // Commit the updated points for the polygon
                    var updatedPoints = new PointCollection(polygon.Points);
                    polygon.Points = updatedPoints;
                }
                else if (_selectedShape != null)
                {
                    // Commit the new size and position for non-polygon shapes
                    double newWidth = _selectedShape.Width;
                    double newHeight = _selectedShape.Height;
                    double newX = Canvas.GetLeft(_selectedShape);
                    double newY = Canvas.GetTop(_selectedShape);

                    _selectedShape.Width = newWidth;
                    _selectedShape.Height = newHeight;
                    Canvas.SetLeft(_selectedShape, newX);
                    Canvas.SetTop(_selectedShape, newY);
                }
            }

            // End resizing mode
            if (_isResizing)
            {
                _isResizing = false;
                _resizeDirection = "";

                // Commit final transformations for resizing
                if (_selectedShape is Polygon polygon)
                {
                    // Persist final points for the polygon
                    var updatedPoints = new PointCollection(polygon.Points);
                    polygon.Points = updatedPoints;
                }
                else if (_selectedShape != null)
                {
                    // Persist width, height, and position for other shapes
                    double newWidth = _selectedShape.Width;
                    double newHeight = _selectedShape.Height;
                    double newX = Canvas.GetLeft(_selectedShape);
                    double newY = Canvas.GetTop(_selectedShape);

                    _selectedShape.Width = newWidth;
                    _selectedShape.Height = newHeight;
                    Canvas.SetLeft(_selectedShape, newX);
                    Canvas.SetTop(_selectedShape, newY);
                }

                // Force a visual update
                _selectedShape?.InvalidateVisual();
            }

            // Remove the bounding box if it exists
            if (_boundingBox != null)
            {
                _canvas.Children.Remove(_boundingBox);
                _boundingBox = null;
            }

            // Clear selected shape to stop interactions
            _selectedShape = null;
        }
        private Point GetShapeCenter(Shape shape)
        {
            if (shape is Polygon polygon)
            {
                // Calculate the centroid (center) of the polygon
                double sumX = 0, sumY = 0;
                int numPoints = polygon.Points.Count;

                foreach (var point in polygon.Points)
                {
                    sumX += point.X;
                    sumY += point.Y;
                }

                // The centroid is the average of the X and Y coordinates
                double centerX = sumX / numPoints;
                double centerY = sumY / numPoints;

                return new Point(centerX, centerY);
            }
            else
            {
                // For other shapes (Rectangle, Ellipse, etc.)
                double left = Canvas.GetLeft(shape);
                double top = Canvas.GetTop(shape);
                double centerX = left + (shape.Width / 2);
                double centerY = top + (shape.Height / 2);

                return new Point(centerX, centerY);
            }
        }

        // Helper method to calculate Euclidean distance
        private double GetEuclideanDistance(Point point1, Point point2)
        {
            double dx = point1.X - point2.X;
            double dy = point1.Y - point2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        // Event handler for continuing a stroke
        private Rectangle GetBoundingBox(Shape shape)
        {
            double left = Canvas.GetLeft(shape);
            double top = Canvas.GetTop(shape);

            Rectangle boundingBox = new Rectangle
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 1,
                Fill = Brushes.Transparent
            };

            if (shape is Rectangle || shape is Ellipse)
            {
                boundingBox.Width = shape.Width;
                boundingBox.Height = shape.Height;
            }
            else if (shape is Polygon polygon)
            {
                double minX = polygon.Points.Min(p => p.X);
                double maxX = polygon.Points.Max(p => p.X);
                double minY = polygon.Points.Min(p => p.Y);
                double maxY = polygon.Points.Max(p => p.Y);

                boundingBox.Width = maxX - minX;
                boundingBox.Height = maxY - minY;

                Canvas.SetLeft(boundingBox, minX);
                Canvas.SetTop(boundingBox, minY);
            }

            Canvas.SetLeft(boundingBox, left);
            Canvas.SetTop(boundingBox, top);

            return boundingBox;
        }
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing&&!_isDragging)
            {
                Point currentPoint = e.GetPosition(_canvas);
                ContinueStrokeCommand?.Execute(currentPoint);
            }
            else if (_isResizing && _selectedShape != null)
            {
                Point currentMousePosition = e.GetPosition(_canvas);
                
                UpdateBoundingBox(_selectedShape);
            }
            else
            {
                if (_selectedShape != null)
                {
                    Point currentMousePosition = e.GetPosition(_canvas);
                    if (_selectedShape is Polygon polygon)
                    {
                        // Calculate how much the mouse has moved since the original position
                        double deltaX = currentMousePosition.X - _offsetX;
                        double deltaY = currentMousePosition.Y - _offsetY;

                        // Move each point of the polygon relative to the original points
                        for (int i = 0; i < polygon.Points.Count; i++)
                        {
                            var originalPoint = _originalPolygonPoints[i];
                            polygon.Points[i] = new Point(originalPoint.X + deltaX, originalPoint.Y + deltaY);
                        }
                    }
                    else
                    {
                        // For other shapes (Rectangle, Ellipse, etc.), use Canvas.SetLeft and SetTop
                        Canvas.SetLeft(_selectedShape, currentMousePosition.X - _offsetX);
                        Canvas.SetTop(_selectedShape, currentMousePosition.Y - _offsetY);
                    }
                    UpdateBoundingBox(_selectedShape);
                }
            }
        }
        public void ResizeShapeWithArrows(string direction)
        {
            if (_selectedShape == null) return;
            _isResizing = true;
            double resizeAmount = 5; // Adjust as needed

            if (_selectedShape is Rectangle || _selectedShape is Ellipse)
            {
                switch (direction)
                {
                    case "Up":
                        _selectedShape.Height = Math.Max(0, _selectedShape.Height - resizeAmount);
                        break;
                    case "Down":
                        _selectedShape.Height += resizeAmount;
                        break;
                    case "Left":
                        _selectedShape.Width = Math.Max(0, _selectedShape.Width - resizeAmount);
                        break;
                    case "Right":
                        _selectedShape.Width += resizeAmount;
                        break;
                }
            }
            else if (_selectedShape is Polygon polygon)
            {
                Point center =GetShapeCenter(polygon);

                // Adjust the scale factor based on direction
                double scaleFactorX = 1.0, scaleFactorY = 1.0;
                switch (direction)
                {
                    case "Up":
                        scaleFactorY -= resizeAmount / 100.0;
                        break;
                    case "Down":
                        scaleFactorY += resizeAmount / 100.0;
                        break;
                    case "Left":
                        scaleFactorX -= resizeAmount / 100.0;
                        break;
                    case "Right":
                        scaleFactorX += resizeAmount / 100.0;
                        break;
                }

                ScalePolygon(polygon, center, scaleFactorX, scaleFactorY);
            }

            // Update the bounding box to match the resized shape
            UpdateBoundingBox(_selectedShape);
        }

        private void ScalePolygon(Polygon polygon, Point center, double scaleFactorX, double scaleFactorY)
        {
            var newPoints = new PointCollection();
            foreach (Point point in polygon.Points)
            {
                double newX = center.X + (point.X - center.X) * scaleFactorX;
                double newY = center.Y + (point.Y - center.Y) * scaleFactorY;
                newPoints.Add(new Point(newX, newY));
            }
            _canvas.Children.Remove(polygon);

            // Create a new polygon with the updated points and styles
            Polygon newPolygon = new Polygon
            {
                Points = newPoints,
                Stroke = polygon.Stroke,
                StrokeThickness = polygon.StrokeThickness,
                Fill = polygon.Fill
            };

            // Add the new polygon to the canvas
            _canvas.Children.Add(newPolygon);

            // Optionally, set it as the currently selected shape
            _selectedShape = newPolygon;
        }
        private void UpdateBoundingBox(Shape shape)
        {
            if (_boundingBox == null) return;

            if (shape is Polygon polygon)
            {
                // For polygons, recalculate the bounding box based on the updated points
                double minX = polygon.Points.Min(p => p.X);
                double maxX = polygon.Points.Max(p => p.X);
                double minY = polygon.Points.Min(p => p.Y);
                double maxY = polygon.Points.Max(p => p.Y);

                _boundingBox.Width = maxX - minX;
                _boundingBox.Height = maxY - minY;

                Canvas.SetLeft(_boundingBox, minX);
                Canvas.SetTop(_boundingBox, minY);
            }
            else
            {
                // For other shapes (Rectangle, Ellipse, etc.), update the bounding box position
                _boundingBox.Width = shape.Width;
                _boundingBox.Height = shape.Height;

                Canvas.SetLeft(_boundingBox, Canvas.GetLeft(shape));
                Canvas.SetTop(_boundingBox, Canvas.GetTop(shape));
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
        public void DrawShape(Point startPoint, ShapeKind shapeType,double radius=0)
        {
            double size = thickness * 3;
            _startPoint = startPoint;
            if (radius != 0)
            {
                size =radius;
            }

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
        public void RotateShape(Point startPoint, double angle)
        {
            if (_currentShape == null) return;

            RotateTransform rotateTransform = new RotateTransform(angle, _currentShape.Width / 2, _currentShape.Height / 2);
            _currentShape.RenderTransform = rotateTransform;
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
        public void StartStroke(Point startPoint,bool mirrored=false)
        {
            if (mirrored == false)
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
            else
            {
                _mirroredPolyline = new Polyline
                {
                    Stroke = _currentBrush, // Default color, could be set via a method or property
                    StrokeThickness = thickness, // Default thickness, could be set via a method or property
                    Points = new PointCollection { startPoint }
                };
                _canvas.Children.Add(_mirroredPolyline);
                ExecuteDrawingAction(_mirroredPolyline);
            }
            
        }

        // Method to continue the current stroke
        public void ContinueStroke(Point currentPoint,bool mirrored=false)
        {
            if (mirrored == false)
            {
                if (_currentPolyline != null)
                {
                    _currentPolyline.Points.Add(currentPoint);
                }
            }
            else {
                if (_mirroredPolyline != null)
                {
                    _mirroredPolyline.Points.Add(currentPoint);
                }
            }
            
        }

        // Method to finalize the current stroke
        public void EndStroke(bool mirrored=false)
        {
            if (mirrored == false)
            {
                var points = _currentPolyline.Points;
                if (corectShapes)
                {
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
                        DrawShape(points.First(), ShapeKind.Triangle, _detector.radius);
                        _canvas.Children.Remove(_currentPolyline);
                    }
                }
                // Detect shapes using the ShapeDetector



                _currentPolyline = null;
            }
            else
            {
                var points = _mirroredPolyline.Points;
                if (corectShapes)
                {
                    if (_detector.IsRectangle(points))
                    {
                        // Replace the polyline with a rectangle
                        var startPoint = points.First();
                        var furthestPoint = points.OrderByDescending(p => _detector.Distance(startPoint, p)).First();
                        StartShape(startPoint, ShapeKind.Rectangle);
                        EndShape(furthestPoint);
                        _canvas.Children.Remove(_mirroredPolyline);
                    }

                    else if (_detector.IsEllipse(points))
                    {
                        // Replace the polyline with an ellipse
                        var startPoint = points.First();
                        var furthestPoint = points.OrderByDescending(p => _detector.Distance(startPoint, p)).First();
                        StartShape(startPoint, ShapeKind.Ellipse);
                        EndShape(furthestPoint);
                        _canvas.Children.Remove(_mirroredPolyline);
                    }
                    else if (_detector.IsEquilateralTriangle(points))
                    {
                        // Replace the polyline with a triangle
                        DrawShape(points.First(), ShapeKind.Triangle, _detector.radius);
                        _canvas.Children.Remove(_mirroredPolyline);
                    }
                }
                // Detect shapes using the ShapeDetector



                _mirroredPolyline = null;
            }
        }

        // Method to configure the brush and stroke thickness (optional, for more flexibility)
        public void SetBrush(System.Windows.Media.Brush brush, double thick)
        {
            _currentBrush=brush;
            thickness = thick;
        }
    }
}
