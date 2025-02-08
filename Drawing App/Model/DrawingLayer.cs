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
using SkiaSharp;
using System.IO;
using System.Windows.Media.Imaging;
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
        public bool EraserMode {  get; set; }   
        public bool corectShapes {  get; set; }
        public List<Point> Points { get; set; }

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
        private int currentCount;
        public MirrorAxis _currentSymetryMode {  get; set; }

        private Canvas _mirroredLayer;
        public bool RepairMirrorMode { get; set; }

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
            _detector = new ShapeDetector(10);
            corectShapes=false;
            _canvas.MouseWheel += Canvas_MouseWheel;
            currentCount = 0;
            _mirroredLayer = new Canvas();
            _canvas.Children.Add(_mirroredLayer);
            EraserMode=false;
            RepairMirrorMode = false;
            Points = new List<Point>();
            _canvas.StylusDown += CanvasStylusDown;
            _canvas.StylusUp += CanvasStylusUp;
            _canvas.StylusMove += CanvasStylusMove;

        }
        public DrawingLayer(Canvas canvas,ICommand startStrokeCommand, ICommand continueStrokeCommand, ICommand endStrokeCommand, double opacity = 1.0, bool isVisible = true, string name = "Layer")
            : base(opacity, isVisible, name)
        {
            StartStrokeCommand = startStrokeCommand;
            ContinueStrokeCommand = continueStrokeCommand;
            EndStrokeCommand = endStrokeCommand;
            _canvas = canvas;
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
            _detector = new ShapeDetector(10);
            corectShapes = false;
            _canvas.MouseWheel += Canvas_MouseWheel;
            currentCount = 0;
            _mirroredLayer = new Canvas();
            _canvas.Children.Add(_mirroredLayer);
            EraserMode=false;
            RepairMirrorMode = false;
            Points = new List<Point>();
            _canvas.StylusDown += CanvasStylusDown;
            _canvas.StylusUp += CanvasStylusUp;
            _canvas.StylusMove += CanvasStylusMove;

        }

        private void CanvasStylusUp(object sender, StylusEventArgs e)
        {
            _isDrawing = false;
            EndStrokeCommand?.Execute(null);
        }

        private void CanvasStylusDown(object sender, StylusDownEventArgs e)
        {
            _isDrawing = true;
            _startPoint = e.GetPosition(_canvas);
            StartStrokeCommand?.Execute(_startPoint);
        }

        private void CanvasStylusMove(object sender, StylusEventArgs e)
        {
            if (_isDrawing && !_isDragging)
            {
                // Handle drawing a stroke
                Point currentPoint = e.GetPosition(_canvas);
                ContinueStrokeCommand?.Execute(currentPoint);
            }
            else if (_isDragging && _selectedShape != null)
            {

                // Get the current mouse position
                Point currentMousePosition = e.GetPosition(_canvas);
                Points.Add(currentMousePosition);
                // Calculate exact new position based on initial drag offset
                double newX = currentMousePosition.X - _offsetX;
                double newY = currentMousePosition.Y - _offsetY;

                // Move the shape to follow the cursor closely
                if (_selectedShape is Polygon polygon)
                {
                    var newPoints = new PointCollection();
                    foreach (var originalPoint in _originalPolygonPoints)
                    {
                        newPoints.Add(new Point(originalPoint.X + newX, originalPoint.Y + newY));
                    }
                    polygon.Points = newPoints;
                }
                else if (_selectedShape is Line line && Points.Count >= 2)
                {
                    double deltaX = currentMousePosition.X - Points[Points.Count - 2].X;
                    double deltaY = currentMousePosition.Y - Points[Points.Count - 2].Y;

                    line.X1 += deltaX;
                    line.Y1 += deltaY;
                    line.X2 += deltaX;
                    line.Y2 += deltaY;
                }
                else
                {
                    Canvas.SetLeft(_selectedShape, newX);
                    Canvas.SetTop(_selectedShape, newY);
                }

                // Update the bounding box to reflect the new position
                UpdateBoundingBox(_selectedShape);
            }

        }

        private double _rotationAngle = 0; // Track the cumulative rotation angle

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Only rotate if a shape is selected
            if (_selectedShape == null) return;

            // Determine the rotation angle increment based on scroll direction
            double rotationIncrement = 1; // Degrees per scroll tick (adjust as desired)
            _rotationAngle += e.Delta > 0 ? rotationIncrement : -rotationIncrement;

            // Determine the center of rotation
            Point rotationCenter;
            if (_selectedShape is Polygon polygon)
            {
                // For polygons, calculate the centroid
                rotationCenter = GetPolygonCentroid(polygon);
            }
            else if (_selectedShape is Line line)
            {
                // For lines, calculate the midpoint
                rotationCenter = new Point(
                    (line.X1 + line.X2) / 2,
                    (line.Y1 + line.Y2) / 2
                );

                // Rotate the endpoints of the line
                Point rotatedStart = RotatePoint(new Point(line.X1, line.Y1), rotationCenter, _rotationAngle);
                Point rotatedEnd = RotatePoint(new Point(line.X2, line.Y2), rotationCenter, _rotationAngle);

                line.X1 = rotatedStart.X;
                line.Y1 = rotatedStart.Y;
                line.X2 = rotatedEnd.X;
                line.Y2 = rotatedEnd.Y;

                // Update the bounding box for the line
                UpdateBoundingBox(_selectedShape);

                return; // No need for further transformations for lines
            }
            else
            {
                // For other shapes, use their center point
                rotationCenter = new Point(_selectedShape.Width / 2, _selectedShape.Height / 2);
            }

            // Apply the rotation transformation for other shapes
            RotateTransform rotateTransform = new RotateTransform(_rotationAngle, rotationCenter.X, rotationCenter.Y);

            if (_selectedShape.RenderTransform is TransformGroup transformGroup)
            {
                // Remove previous rotation and apply the new rotation
                transformGroup.Children.OfType<RotateTransform>().ToList().ForEach(r => transformGroup.Children.Remove(r));
                transformGroup.Children.Add(rotateTransform);
            }
            else
            {
                // Create a TransformGroup if none exists and apply the rotation
                TransformGroup newTransformGroup = new TransformGroup();
                newTransformGroup.Children.Add(rotateTransform);
                _selectedShape.RenderTransform = newTransformGroup;
            }

            UpdateBoundingBox(_selectedShape);

            // Optional: Force a visual update
            _selectedShape.InvalidateVisual();
        }
        private Point RotatePoint(Point point, Point center, double angle)
        {
            double radians = angle * Math.PI / 180;
            double cosTheta = Math.Cos(radians);
            double sinTheta = Math.Sin(radians);

            double dx = point.X - center.X;
            double dy = point.Y - center.Y;

            double rotatedX = cosTheta * dx - sinTheta * dy + center.X;
            double rotatedY = sinTheta * dx + cosTheta * dy + center.Y;

            return new Point(rotatedX, rotatedY);
        }

        private Point GetPolygonCentroid(Polygon polygon)
        {
            double sumX = 0, sumY = 0;
            int numPoints = polygon.Points.Count;

            foreach (var point in polygon.Points)
            {
                sumX += point.X;
                sumY += point.Y;
            }

            // Calculate the average to get the centroid
            double centerX = sumX / numPoints;
            double centerY = sumY / numPoints;

            return new Point(centerX, centerY);
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
            Points.Clear();
            Points.Add(clickPoint);

            // Loop through all the shapes in the canvas
            foreach (UIElement element in _canvas.Children)
            {
                if (element is Shape shape)
                {
                    double distance;

                    if (shape is Line line)
                    {
                        // Get the closest point on the line to the clicked point
                        Point closestPointOnLine = GetClosestPointOnLine(
                            new Point(line.X1, line.Y1),
                            new Point(line.X2, line.Y2),
                            clickPoint
                        );
                        // Calculate distance from the clicked point to the closest point on the line
                        distance = GetEuclideanDistance(clickPoint, closestPointOnLine);
                    }
                    else
                    {
                        // For other shapes, calculate distance to the shape's center
                        Point shapeCenter = GetShapeCenter(shape);
                        distance = GetEuclideanDistance(clickPoint, shapeCenter);
                    }

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
                _selectedShape = closestShape;
                _isDragging = true;

                if (_selectedShape is Line line)
                {
                    // Store the offset relative to the closest point on the line
                    Point closestPointOnLine = GetClosestPointOnLine(
                        new Point(line.X1, line.Y1),
                        new Point(line.X2, line.Y2),
                        clickPoint
                    );
                    _offsetX = clickPoint.X - closestPointOnLine.X;
                    _offsetY = clickPoint.Y - closestPointOnLine.Y;
                }
                else if (_selectedShape is Polygon polygon)
                {
                    // Store the original points of the polygon before dragging
                    _originalPolygonPoints = polygon.Points.ToList();
                    _offsetX = clickPoint.X;
                    _offsetY = clickPoint.Y;
                }
                else
                {
                    // Calculate the offset for other shapes
                    _offsetX = clickPoint.X - Canvas.GetLeft(_selectedShape);
                    _offsetY = clickPoint.Y - Canvas.GetTop(_selectedShape);
                }

                // Create a bounding box around the selected shape (if needed)
                _boundingBox = GetBoundingBox(_selectedShape);
                _canvas.Children.Add(_boundingBox);

                // Capture the mouse to track movements even if it leaves the shape's bounds
                _canvas.CaptureMouse();
            }
        }
        private Point GetClosestPointOnLine(Point start, Point end, Point point)
        {
            double dx = end.X - start.X;
            double dy = end.Y - start.Y;

            if (dx == 0 && dy == 0)
            {
                // The line segment is a point
                return start;
            }

            // Calculate the projection of the point onto the line (clamped to the segment)
            double t = ((point.X - start.X) * dx + (point.Y - start.Y) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0, Math.Min(1, t)); // Clamp t to the range [0, 1]

            // Return the closest point on the line segment
            return new Point(start.X + t * dx, start.Y + t * dy);
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
            if (_isDrawing && !_isDragging)
            {
                // Handle drawing a stroke
                Point currentPoint = e.GetPosition(_canvas);
                ContinueStrokeCommand?.Execute(currentPoint);
            }
            else if (_isDragging && _selectedShape != null)
            {
               
                // Get the current mouse position
                Point currentMousePosition = e.GetPosition(_canvas);
                Points.Add(currentMousePosition);
                // Calculate exact new position based on initial drag offset
                double newX = currentMousePosition.X - _offsetX;
                double newY = currentMousePosition.Y - _offsetY;

                // Move the shape to follow the cursor closely
                if (_selectedShape is Polygon polygon)
                {
                    var newPoints = new PointCollection();
                    foreach (var originalPoint in _originalPolygonPoints)
                    {
                        newPoints.Add(new Point(originalPoint.X + newX, originalPoint.Y + newY));
                    }
                    polygon.Points = newPoints;
                }
                else if (_selectedShape is Line line&&Points.Count>=2)
                {
                    double deltaX = currentMousePosition.X - Points[Points.Count-2].X;
                    double deltaY = currentMousePosition.Y - Points[Points.Count - 2].Y;

                    line.X1 += deltaX;
                    line.Y1 += deltaY;
                    line.X2 += deltaX;
                    line.Y2 += deltaY;
                }
                else
                {
                    Canvas.SetLeft(_selectedShape, newX);
                    Canvas.SetTop(_selectedShape, newY);
                }

                // Update the bounding box to reflect the new position
                UpdateBoundingBox(_selectedShape);
            }
        }
       


        public ImageLayer ConvertToImageLayer()
        {
            BitmapImage bitmap = ConvertCanvasToBitmapImage(_canvas);
            return new ImageLayer(bitmap,_canvas.ActualWidth,_canvas.ActualHeight);
        }
        public BitmapImage ConvertCanvasToBitmapImage(Canvas canvas)
        {
            // Measure and arrange the canvas to ensure its content is properly rendered
            System.Windows.Size size = new System.Windows.Size(canvas.ActualWidth, canvas.ActualHeight);
            canvas.Measure(size);
            canvas.Arrange(new Rect(size));

            // Render the canvas to a RenderTargetBitmap
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)canvas.ActualWidth,  // Width of the canvas
                (int)canvas.ActualHeight, // Height of the canvas
                96d,                      // DPI (horizontal)
                96d,                      // DPI (vertical)
                PixelFormats.Pbgra32      // Pixel format
            );
            renderBitmap.Render(canvas);

            // Convert RenderTargetBitmap to a BitmapImage
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Save the RenderTargetBitmap to a memory stream
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                encoder.Save(memoryStream);

                // Load the memory stream into the BitmapImage
                memoryStream.Position = 0; // Reset the stream position
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Freeze for thread safety
            }

            return bitmapImage;
        }
        public BitmapSource ConvertCanvasToBitmap(Canvas canvas)
        {
            // Measure and arrange the canvas to ensure all its content is properly laid out
            System.Windows.Size size = new System.Windows.Size(canvas.ActualWidth, canvas.ActualHeight);
            canvas.Measure(size);
            canvas.Arrange(new Rect(size));

            // Create a RenderTargetBitmap to hold the rendered canvas
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)canvas.ActualWidth,  // Width of the canvas
                (int)canvas.ActualHeight, // Height of the canvas
                96d,                      // DPI (horizontal)
                96d,                      // DPI (vertical)
                PixelFormats.Pbgra32      // Pixel format
            );

            // Render the canvas to the RenderTargetBitmap
            renderBitmap.Render(canvas);

            return renderBitmap;
        }

        // Example of saving the BitmapSource to a file
        public void SaveCanvasAsImage(Canvas canvas, string filePath)
        {
            BitmapSource bitmap = ConvertCanvasToBitmap(canvas);

            // Encode the BitmapSource to a PNG file
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
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
                Point center = GetShapeCenter(polygon);

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
            else if (_selectedShape is Line line)
            {
                // Calculate the slope of the line
                double slope = (line.Y2 - line.Y1) / (line.X2 - line.X1);
                double dx = resizeAmount; // Increment on the X-axis
                double dy = slope * dx;  // Increment on the Y-axis

                switch (direction)
                {
                    case "Left":
                        // Extend at the start point (X1, Y1)
                        line.X1 -= dx;
                        line.Y1 -= dy;
                        break;

                    case "Right":
                        // Extend at the end point (X2, Y2)
                        line.X2 += dx;
                        line.Y2 += dy;
                        break;
                    case "Up": // Shrink the first point (X1, Y1)
                        line.X1 += dx;
                        line.Y1 += dy;
                        break;

                    case "Down": // Shrink the last point (X2, Y2)
                        line.X2 -= dx;
                        line.Y2 -= dy;
                        break;
                }
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
            if (_canvas.Children[_canvas.Children.Count-1] is Polyline polyline) { 
                _startPoint=polyline.Points.First();
                if (shapeType == ShapeKind.Rectangle)
                {
                    _currentShape = new System.Windows.Shapes.Rectangle
                    {
                        Stroke = _currentBrush,
                        StrokeThickness = thickness,
                        Fill = Brushes.Transparent,
                    };
                    _undoStack.Push(_currentShape);
                }
                else if (shapeType == ShapeKind.Ellipse)
                {
                    _currentShape = new Ellipse
                    {
                        Stroke = _currentBrush,
                        StrokeThickness = thickness,
                        Fill = Brushes.Transparent
                    };
                    _undoStack.Push(_currentShape);
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
                    _undoStack.Push(_currentShape);
                }

                if (_currentShape != null)
                {

                    _canvas.Children.Add(_currentShape);
                }
            }
            return;
           
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
                    StrokeThickness = thickness, // You can set this differently if needed
                    Width = size,  // Diameter
                    Height = size, // Diameter
                    Fill = Brushes.Transparent
                };
                _undoStack.Push(_currentShape);
            }
            else if (shapeType == ShapeKind.Square)
            {
                _currentShape = new System.Windows.Shapes.Rectangle
                {
                    Stroke = _currentBrush,
                    StrokeThickness = thickness, // You can set this differently if needed
                    Width = size,  // Side length
                    Height = size, // Side length
                    Fill = Brushes.Transparent
                };
                _undoStack.Push(_currentShape);
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

                _currentShape = new System.Windows.Shapes.Path
                {
                    Stroke = _currentBrush,
                    StrokeThickness = 1,
                    Data = heartGeometry,
                    Fill = Brushes.Transparent
                };

                _canvas.Children.Add(_currentShape);
                _undoStack.Push(_currentShape);
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
                _undoStack.Push(_currentShape);
            }

            if (_currentShape != null && shapeType != ShapeKind.Heart&& shapeType != ShapeKind.Triangle)
            {
                Canvas.SetLeft(_currentShape, startPoint.X - size / 2);
                Canvas.SetTop(_currentShape, startPoint.Y - size / 2);
                _canvas.Children.Add(_currentShape);
                _undoStack.Push(_currentShape);
            }
            if (_currentShape != null)
            {
                ExecuteDrawingAction(_currentShape);
            }

        }
        public void ApplyEraseMode(Point erasePoint)
        {
            // Iterate through all the children of the canvas
            foreach (var child in _canvas.Children)
            {
                if (child is Shape shape)
                {
                    // Get the center of the shape
                    Point shapeCenter = GetShapeCenter(shape);

                    // Calculate the Euclidean distance between the erase point and the shape center
                    double distance = GetEuclideanDistance(erasePoint, shapeCenter);

                    // If the shape is within the erase radius, erase it
                    if (distance <= 50)
                    {
                        shape.Stroke = _currentBrush;
                        shape.Fill = _currentBrush;
                    }
                }
            }
        }

        // Helper method to calculate the Euclidean distance between two points
        
        public void EndShape(Point endPoint)
        {
            if (_currentShape == null) return;
            if (_canvas.Children[_canvas.Children.Count - 2] is Polyline p) {
                endPoint=p.Points.Last();
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
            }
            

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
        public bool _previousSymmetryState { get; set; }

        public void StartStroke(Point startPoint, bool enableSymmetry = false)
        {
           

            _currentPolyline = new Polyline
            {
                Stroke = _currentBrush,
                StrokeThickness = thickness,
                Points = new PointCollection { startPoint }
            };
            _canvas.Children.Add(_currentPolyline);
            _undoStack.Push(_currentPolyline);
            currentCount++;

            if (EraserMode == true)
            {
                ApplyEraseMode(startPoint);
            }

            if (enableSymmetry)

            {
                _mirroredLayer.Children.Clear();
                _mirroredPolyline = new Polyline
                {
                    Stroke = _currentBrush,
                    StrokeThickness = thickness,
                    Points = new PointCollection { GetMirroredPoint(startPoint) }
                };
                _mirroredLayer.Children.Add(_mirroredPolyline);
            }
        }

        public void EndStroke(bool enableSymmetry = false)
        {
            if (_currentPolyline == null) return;

            var points = _currentPolyline.Points;
            bool shapeDetected = false;

            if (corectShapes)
            {
                if (_detector.IsEllipse(points))
                {
                    var startPoint = points.First();
                    var furthestPoint = points.OrderByDescending(p => _detector.Distance(startPoint, p)).First();
                    StartShape(startPoint, ShapeKind.Ellipse);
                    EndShape(furthestPoint);
                    shapeDetected = true;
                }

                else if(_detector.IsRectangle(points))
                {
                    var startPoint = points.First();
                    var furthestPoint = points.OrderByDescending(p => _detector.Distance(startPoint, p)).First();
                    StartShape(startPoint, ShapeKind.Rectangle);
                    EndShape(furthestPoint);
                    shapeDetected = true;
                }
                else if (_detector.IsEquilateralTriangle(points))
                {
                    DrawShape(points.First(), ShapeKind.Triangle, _detector.radius);
                    shapeDetected = true;
                }
            }

            if (shapeDetected)
            {
                _canvas.Children.Remove(_currentPolyline);
                _undoStack.Pop(); // Remove the original polyline from undo stack

                if (enableSymmetry && _mirroredPolyline != null)
                {
                    _mirroredLayer.Children.Remove(_mirroredPolyline);
                }
            }

            if (EraserMode == true)
            {
                ApplyEraseMode(_currentPolyline.Points.Last());
            }

            _currentPolyline = null;
            _mirroredPolyline = null;
        }


        private Polyline ClonePolyline(Polyline originalPolyline)
        {
            return new Polyline
            {
                Stroke = originalPolyline.Stroke,
                StrokeThickness = originalPolyline.StrokeThickness,
                Points = new PointCollection(originalPolyline.Points) // Clone points
            };
        }

        public void ContinueStroke(Point currentPoint, bool enableSymmetry = false)
        {
            _currentPolyline?.Points.Add(currentPoint);

            if (EraserMode == true)
            {
                ApplyEraseMode(currentPoint);
            }
            if (enableSymmetry && _mirroredPolyline != null)
            {
                Point mirroredPoint = GetMirroredPoint(currentPoint);
                _mirroredPolyline.Points.Add(mirroredPoint);
            }
        }

      
        private Point GetMirroredPoint(Point originalPoint)
        {
            double canvasCenterX = _canvas.ActualWidth / 2;
            double mirroredX = 2 * canvasCenterX - originalPoint.X;

            return new Point(mirroredX, originalPoint.Y);
        }

        private void RenderSymmetry(Point point, bool isEndStroke)
        {
            // Define symmetry axis (vertical symmetry example)
            double canvasCenterX = _canvas.ActualWidth / 2;

            // Calculate mirrored point
            Point mirroredPoint = new Point(2 * canvasCenterX - point.X, point.Y);

            if (!isEndStroke)
            {
                // Add the mirrored point dynamically
                if (_currentPolyline != null)
                {
                    _currentPolyline.Points.Add(mirroredPoint);
                }
            }
            else
            {
                // Finalize the mirrored polyline
                var mirroredPolyline = new Polyline
                {
                    Stroke = _currentBrush,
                    StrokeThickness = thickness,
                    Points = new PointCollection(_currentPolyline.Points.Select(p =>
                        new Point(2 * canvasCenterX - p.X, p.Y)))
                };
                _canvas.Children.Add(mirroredPolyline);
            }
        }

        // Method to continue the current stroke

       

        // Helper to clean up redundant children from the canvas
        private void CleanupCanvas()
        {
            var polylines = _canvas.Children.OfType<Polyline>().ToList();
            foreach (var polyline in polylines)
            {
                if (polyline.Points.Count == 0)
                {
                    _canvas.Children.Remove(polyline);
                }
            }
        }




        // Method to finalize the current stroke

        private void FinalizePolyline(Polyline polyline)
        {
            if (polyline == null) return;

            var points = polyline.Points;

            if (corectShapes)
            {
                if (_detector.IsRectangle(points))
                {
                    ReplaceWithRectangle(points);
                }
                else if (_detector.IsEllipse(points))
                {
                    ReplaceWithEllipse(points);
                }
                else if (_detector.IsEquilateralTriangle(points))
                {
                    ReplaceWithTriangle(points);
                }
            }
        }

        private void ReplaceWithRectangle(PointCollection points)
        {
            var startPoint = points.First();
            var furthestPoint = points.OrderByDescending(p => _detector.Distance(startPoint, p)).First();
            StartShape(startPoint, ShapeKind.Rectangle);
            EndShape(furthestPoint);
            _canvas.Children.Remove(_currentPolyline);
        }

        private void ReplaceWithEllipse(PointCollection points)
        {
            var startPoint = points.First();
            var furthestPoint = points.OrderByDescending(p => _detector.Distance(startPoint, p)).First();
            StartShape(startPoint, ShapeKind.Ellipse);
            EndShape(furthestPoint);
            _canvas.Children.Remove(_currentPolyline);
        }

        private void ReplaceWithTriangle(PointCollection points)
        {
            DrawShape(points.First(), ShapeKind.Triangle, _detector.radius);
            _canvas.Children.Remove(_currentPolyline);
        }

        private void SyncPolylinePoints()
        {
            if (_mirroredPolyline == null || _currentPolyline == null) return;

            var mirroredCount = _mirroredPolyline.Points.Count;
            var currentCount = _currentPolyline.Points.Count;

            if (mirroredCount < currentCount)
            {
                while (_mirroredPolyline.Points.Count < _currentPolyline.Points.Count)
                {
                    _mirroredPolyline.Points.Add(_mirroredPolyline.Points.Last());
                }
            }
            else if (currentCount < mirroredCount)
            {
                while (_currentPolyline.Points.Count < _mirroredPolyline.Points.Count)
                {
                    _currentPolyline.Points.Add(_currentPolyline.Points.Last());
                }
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
