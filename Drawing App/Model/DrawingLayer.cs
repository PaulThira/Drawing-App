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
