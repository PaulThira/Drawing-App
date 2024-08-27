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

namespace Drawing_App.Model
{
   public class DrawingLayer:Layer
    {
        private readonly Canvas _canvas;

        // Flags to track if a stroke is currently being drawn
        private bool _isDrawing;

        // The starting point of the current stroke
        private Point _startPoint;
        private Shape _currentShape;

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
                _currentShape = new Rectangle
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
            _startPoint = startPoint;
            if (shapeType == ShapeKind.Circle)
            {
                _currentShape = new Ellipse
                {
                    Stroke = _currentBrush,
                    StrokeThickness = 1, // You can set this differently if needed
                    Width = thickness,  // Diameter
                    Height = thickness, // Diameter
                    Fill = Brushes.Transparent
                };
            }
            else if (shapeType == ShapeKind.Square)
            {
                _currentShape = new Rectangle
                {
                    Stroke = _currentBrush,
                    StrokeThickness = 1, // You can set this differently if needed
                    Width = thickness,  // Side length
                    Height = thickness, // Side length
                    Fill = Brushes.Transparent
                };
            }
            if (_currentShape != null)
            {
                Canvas.SetLeft(_currentShape, startPoint.X - thickness/2);
                Canvas.SetTop(_currentShape, startPoint.Y - thickness/2);
                _canvas.Children.Add(_currentShape);
            }

        }

        public void EndShape(Point endPoint)
        {
            if (_currentShape == null) return;

            if (_currentShape is Rectangle || _currentShape is Ellipse)
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

            _currentShape = null;
        }
        // Event handler for ending a stroke
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
            EndStrokeCommand?.Execute(null);
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
