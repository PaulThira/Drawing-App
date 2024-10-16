﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Input;
using System.Xml.Linq;
using Prism.Commands;
using Microsoft.Win32;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Ink;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Drawing;
using Color = System.Windows.Media.Color;
using Drawing_App.Model;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using System.Security.Cryptography.Xml;
using System.ComponentModel;
using Drawing_App.View;
using Brush = System.Windows.Media.Brush;
using Pen = System.Windows.Media.Pen;
using Brushes = System.Windows.Media.Brushes;
using Application = System.Windows.Application;
using System.Windows.Media.Effects;

namespace Drawing_App.VM
{
    public class MainWindowVM : BindableBase
    {
        public bool MirrorModeEnabled {  get; set; }
        private int threshold;
        public int Threshold
        {
            get => threshold;
            set => SetProperty(ref threshold, value);
        }
        public MirrorAxis axis {  get; set; }
        private double _brushSize;
        private double _hue;
        private double _saturation;
        private double _brightness;
        private double _opacity;
        private DrawingAttributes _drawingAttributes;
        private double _inkCanvasWidth;
        private double _inkCanvasHeight;
        private Brush _backgroundColor;
        private Color _currentColor;
        private HashSet<Color> _usedColors;
        private ObservableCollection<Color> _colorPalette;
        private Brush _rectangleFill;
        private Brush _color1;
        private Brush _color2;
        private Brush _color3;
        private Brush _color4;
        private Brush _color5;
        private Brush _color6;
        private Brush _color7;
        private Brush _color8;
        private Brush harmony;
        private Polyline _currentPolyline;
        public Stack<Point> Points = new Stack<Point>();
        public Stack<Point> MirroredPoints = new Stack<Point>();
        public ObservableCollection<Layer> Layers { get; }
        private Layer _selectedLayer;
        private ObservableCollection<ObservableCollection<CustomPallete>> _colorPalettes;

        public ObservableCollection<ObservableCollection<CustomPallete>> ColorPalettes
        {
            get => _colorPalettes;
            set => SetProperty(ref _colorPalettes, value);
        }

        // Property to track the selected layer
        public Layer SelectedLayer
        {
            get => _selectedLayer;
            set => SetProperty(ref _selectedLayer, value);
        }

        public ICommand HistoCommand { get; set; }

        // Commands for layer operations
        public ICommand AddImageLayerCommand { get; }
        public ICommand AddDrawingLayerCommand { get; }
        public ICommand RemoveLayerCommand { get; }

        public ObservableCollection<UIElement> DrawingElements { get; }

        public ICommand StartStrokeCommand { get; }
        public ICommand ContinueStrokeCommand { get; }
        public ICommand EndStrokeCommand { get; }
        public ICommand PencilCommand { get; }
        public ICommand PenCommand { get; }
        public ICommand MarkerCommand { get; }

        public ICommand LayerCheckedCommand { get; }
        public ICommand LayerUncheckedCommand { get; }
        public ICommand ChangeShapeKind { get; }
        private int _selectedLayerIndex;
        private ObservableCollection<CustomPallete> _selectedPalette;
        public ObservableCollection<CustomPallete> SelectedPalette
        {
            get => _selectedPalette;
            set => SetProperty(ref _selectedPalette, value);
        }
        public int SelectedLayerIndex { get =>_selectedLayerIndex;
            set
            {
                if (SetProperty(ref _selectedLayerIndex, value))
                {
                    // Update the selected layer based on the index
                    foreach (var layer in Layers)
                    {
                        layer._isSelected = false;
                    }
                    if (_selectedLayerIndex >= 0 && _selectedLayerIndex < Layers.Count)
                    {
                        Layers[_selectedLayerIndex]._isSelected = true;
                    }
                }
            }

        }
        private ShapeKind _selectedShapeType = ShapeKind.None;
        public ShapeKind SelectedShapeType
        {
            get => _selectedShapeType;
            set => SetProperty(ref _selectedShapeType, value);
        }
        public string width {  get; set; }  
        public string height { get; set; }
        public ICommand UpdateDimensionsCanvas { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand ReferencesCommand { get; }
        public ICommand ShapeDetectCommand { get; }
        private ImageBrush _pencilBrush;
        private DrawingBrush _currentBrush;
        public ICommand MechanicalCommand { get; }
        public ICommand GoucheCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand SuperZoomCommand { get; }
        public ICommand GrayscaleCommand { get; }
        public ICommand HistogramEqualisationCommand { get; }
        public ICommand Spline { get; }
        public ICommand NegativeFilterCommand { get; }
        private bool? result;
        public ICommand NextPaletteCommand { get; }
        public ICommand PreviousPaletteCommand { get; }
        public ICommand PalletteGeneratorCommand { get; }
        public ICommand PickColorFromPalleteCommand { get; }
        public ICommand MirrorCommand { get; }
        public ICommand TriangleTCommand { get; }
        public ICommand ThresholdCommand { get; }
        public MainWindowVM()

        {
            ThresholdCommand = new DelegateCommand(Thresholding);
            TriangleTCommand = new DelegateCommand(Triangle);
            MirrorCommand = new DelegateCommand<string>(Mirror);
            PalletteGeneratorCommand = new DelegateCommand(OpenPalleteGenerator);
            NegativeFilterCommand = new DelegateCommand(NegativeFilter);
            Spline = new DelegateCommand(Splined);
            GrayscaleCommand=new DelegateCommand(Grayscale);
            SuperZoomCommand=new DelegateCommand(SuperZoom);
            HistogramEqualisationCommand=new DelegateCommand(HistoEqual);
            MirrorModeEnabled=false;
            
            ZoomInCommand = new DelegateCommand(ZoomIn);
            ZoomOutCommand = new DelegateCommand(ZoomOut);
            GoucheCommand = new DelegateCommand(Gouche);
            UpdateDimensionsCanvas = new DelegateCommand(UpdateCanvas);
            HistoCommand = new DelegateCommand(Histo);
            ShapeDetectCommand = new DelegateCommand(ShapeDetects);
            ReferencesCommand = new DelegateCommand(References);
            UndoCommand = new DelegateCommand(Undo).ObservesProperty(() => SelectedLayer);
            RedoCommand = new DelegateCommand(Redo).ObservesProperty(() => SelectedLayer);
            PenCommand = new DelegateCommand(PenCall);
            DrawingElements = new ObservableCollection<UIElement>();
            ChangeShapeKind = new DelegateCommand<ShapeKind?>(SetShapeType);
            _currentColor = Colors.Cyan;
            StartStrokeCommand = new DelegateCommand<Point?>(StartStroke);
            ContinueStrokeCommand = new DelegateCommand<Point?>(ContinueStroke);
            EndStrokeCommand = new DelegateCommand(EndStroke);
            AddImageLayerCommand = new DelegateCommand(AddImageLayer);
            AddDrawingLayerCommand = new DelegateCommand(AddDrawingLayer);
            RemoveLayerCommand = new DelegateCommand(RemoveLayer, CanRemoveLayer)
                .ObservesProperty(() => SelectedLayer);
            RectangleFill = Brushes.Cyan;
            _usedColors = new HashSet<Color>();
            _colorPalette = new ObservableCollection<Color>();
            SaveCommand = new DelegateCommand<ItemsControl>(SaveCall);
            EraserCommand = new DelegateCommand(EraserCall);
            PencilCommand = new DelegateCommand(PencilCall);
            MarkerCommand = new DelegateCommand(MarkerCall);
            MechanicalCommand = new DelegateCommand(Mechanical);
         
            Layers = new ObservableCollection<Layer>();
            _opacity = 1.0;
            BrushSize = 5;
            Hue = 0;
            Saturation = 1;
            Brightness = 1;
            Opacity = 1;
            Strokes = new StrokeCollection();
            DrawingAttributes = new DrawingAttributes
            {
                Color = Colors.Cyan,
                Width = 5,
                Height = 5,
                FitToCurve = true
            };
            _selectedLayerIndex = -1;
            width = "500";
            height = "500";
            InkCanvasWidth = 4200;  // Default width
            InkCanvasHeight = 2100;
            BackgroundColor = new SolidColorBrush(Colors.White);// Default height
            Color1 = new SolidColorBrush(Colors.Red);
            Color2 = new SolidColorBrush(Colors.Green);
            Color3 = new SolidColorBrush(Colors.Blue);
            Color4 = new SolidColorBrush(Colors.Yellow);
            Color5 = new SolidColorBrush(Colors.Orange);
            Color6 = new SolidColorBrush(Colors.Purple);
            Color7 = new SolidColorBrush(Colors.Pink);
            Color8 = new SolidColorBrush(Colors.Lavender);
            Harmony =new SolidColorBrush(Colors.Green);
            _currentBrush = new DrawingBrush();
            _currentBrush.Drawing = new GeometryDrawing(
                brush: new SolidColorBrush(CurrentColor),   // Fill brush
                pen: new Pen(new SolidColorBrush(CurrentColor), 1), // Outline pen
                geometry: new RectangleGeometry(new Rect(0, 0, 10, 10))
            );
            DrawingLayer l=new DrawingLayer(StartStrokeCommand, ContinueStrokeCommand, EndStrokeCommand);
            Layers.Add(l);
            foreach (var layer in Layers)
            {
                layer.PropertyChanged += Layer_PropertyChanged;
            }
            LayerCheckedCommand = new DelegateCommand<Layer>(OnLayerChecked);
            LayerUncheckedCommand = new DelegateCommand<Layer>(OnLayerUnchecked);
            ColorPalettes = new ObservableCollection<ObservableCollection<CustomPallete>>();
            ColorPalettes.Add(new ObservableCollection<CustomPallete>
{
    new CustomPallete(new SolidColorBrush(Colors.Red), 0,ColorSelected),
    new CustomPallete(new SolidColorBrush(Colors.Sienna), 1,ColorSelected),
    new CustomPallete(new SolidColorBrush(Colors.Maroon), 2,ColorSelected),
    new CustomPallete(new SolidColorBrush(Colors.Crimson), 3,ColorSelected)
});

            ColorPalettes.Add(new ObservableCollection<CustomPallete>
{
    new CustomPallete(new SolidColorBrush(Colors.Cyan), 0,ColorSelected),
    new CustomPallete(new SolidColorBrush(Colors.Teal), 1,ColorSelected),
    new CustomPallete(new SolidColorBrush(Colors.Lavender), 2,ColorSelected),
    new CustomPallete(new SolidColorBrush(Colors.Orchid), 3,ColorSelected)
});

            ColorPalettes.Add(new ObservableCollection<CustomPallete>
{
    new CustomPallete(new SolidColorBrush(Colors.Salmon), 0,ColorSelected),
    new CustomPallete(new SolidColorBrush(Colors.Coral), 1,ColorSelected),
    new CustomPallete(new SolidColorBrush(Colors.Pink), 2,ColorSelected),
    new CustomPallete(new SolidColorBrush(Colors.Magenta), 3,ColorSelected)
});

            NextPaletteCommand = new DelegateCommand(MoveToNextPalette);
            PreviousPaletteCommand = new DelegateCommand(MoveToPreviousPalette);
            SelectedPalette = ColorPalettes[0];
        }
        public void Thresholding()
        {
            if (SelectedLayer is ImageLayer i)
            {
                i.Thresholding(Threshold);
            }

        }
        public void Triangle()
        {
            if(SelectedLayer is ImageLayer i)
            {
                i.TriangleThresholding();
            }
        }
        private void Mirror(string type)

        {
            MirrorModeEnabled = true;
            if (type == "0")
            {
                axis = MirrorAxis.Horizontal;
            }
            else if (type == "1")
            {
                axis = MirrorAxis.Vertical;
            }
            else if (type == "2") { 
                axis = MirrorAxis.Both;
            }
            else
            {
                MirrorModeEnabled = false;
            }
        }
        private void ColorSelected(int selectedIndex)
        {
            
            if (selectedIndex >= 0 && selectedIndex < SelectedPalette.Count)
            {
                var selectedBrush = SelectedPalette[selectedIndex];
                CurrentColor=selectedBrush.ColorBrush.Color;

               
                RectangleFill = selectedBrush.ColorBrush;
                _currentBrush.Drawing = new GeometryDrawing(
                brush: new SolidColorBrush(CurrentColor),   // Fill brush
                pen: new Pen(new SolidColorBrush(CurrentColor), 1), // Outline pen
                geometry: new RectangleGeometry(new Rect(0, 0, 10, 10))
            );
                UpdateColor(1);
                // Perform actions with the selected color (e.g., display a message or update UI)

                MessageBox.Show($"You clicked on color at index: {selectedIndex} ");
            }
        }
        private void OpenPalleteGenerator()
        {
            ColorPalleteGenerator color=new ColorPalleteGenerator();
           var ok= color.ShowDialog();
            if (ok == true)
            {
                ColorPalettes.Add(color.pallete);
                MessageBox.Show("Success");
            }
        }

        private void MoveToNextPalette()
        {
            int currentIndex = ColorPalettes.IndexOf(SelectedPalette);
            if (currentIndex == ColorPalettes.Count - 1)
            {
                SelectedPalette = ColorPalettes[0]; // Go to first palette
            }
            else
            {
                SelectedPalette = ColorPalettes[currentIndex + 1];
            }
        }
        private Point GetMirroredPoint(Point originalPoint)
        {
            double mirroredX = originalPoint.X;
            double mirroredY = originalPoint.Y;
            double canvasHeight = double.Parse(height);
            double canvasWidth = double.Parse(width);

            if (axis == MirrorAxis.Horizontal || axis == MirrorAxis.Both)
            {
                mirroredY = canvasHeight - originalPoint.Y;
            }

            if (axis == MirrorAxis.Vertical || axis == MirrorAxis.Both)
            {
                mirroredX = canvasWidth - originalPoint.X;
            }

            return new Point(mirroredX, mirroredY);
        }
        private void MoveToPreviousPalette()
        {
            int currentIndex = ColorPalettes.IndexOf(SelectedPalette);
            if (currentIndex == 0)
            {
                SelectedPalette = ColorPalettes[ColorPalettes.Count - 1]; // Go to last palette
            }
            else
            {
                SelectedPalette = ColorPalettes[currentIndex - 1];
            }
        }
        private void NegativeFilter()
        {
            if(SelectedLayer is ImageLayer i)
            {
                i.ApplyNegative();
            }
        }
        private void Splined()
        {
            int[] blue, green, red;
            SplineTool splineTool = new SplineTool();
      

            // Show the window as a dialog (execution is paused until the window is closed)
            bool? dialogResult = splineTool.ShowDialog();
            
            blue = new int[256];
            if (dialogResult == true || dialogResult == null)
            {
                var viewModel = splineTool.DataContext as SplineToolVM;
                if (viewModel != null)
                {
                    blue = viewModel.LUT;  // Access the LUT from the ViewModel
                    
                }


            }
            SplineTool splineTool1 = new SplineTool();


            // Show the window as a dialog (execution is paused until the window is closed)
            bool? dialogResult1 = splineTool1.ShowDialog();

            green = new int[256];
            if (dialogResult1 == true || dialogResult1 == null)
            {
                var viewModel = splineTool1.DataContext as SplineToolVM;
                if (viewModel != null)
                {
                    green = viewModel.LUT;  // Access the LUT from the ViewModel

                }


            }
            SplineTool splineTool2 = new SplineTool();


            // Show the window as a dialog (execution is paused until the window is closed)
            bool? dialogResult2 = splineTool2.ShowDialog();

            red = new int[256];
            if (dialogResult2 == true || dialogResult2 == null)
            {
                var viewModel = splineTool.DataContext as SplineToolVM;
                if (viewModel != null)
                {
                    red = viewModel.LUT;  // Access the LUT from the ViewModel

                }


            }
            if(SelectedLayer is ImageLayer i&& blue!=null&&green!=null&&red!=null)
            {
                i.ApplySplinedTransformation(blue,green,red);
            }
        }
        private void ApplyLUT()
        {
            if (result == true)
            {
                MessageBox.Show("True");

            }
        }
        private void HistoEqual()
        {
            if (SelectedLayer is ImageLayer i)
            {
                i.HistogramEqualisation();
            }
        }
        private void Grayscale()
        {
            if (SelectedLayer is ImageLayer i)
            {
                i.GrayscaleConversion();
            }
        }
        private void SuperZoom()
        {
            if (SelectedLayer is ImageLayer i)
            {
                i.ZoomedPixels();
            }
        }
        private void ZoomIn()
        {
            foreach (var layer in Layers)
            {
                layer.ZoomLevel += 0.1;
                layer.ZoomIn();
                
                // Calls the overridden method
            }// Increase zoom level
        }

        // Logic for Zooming Out
        private void ZoomOut()
        {
            foreach (var layer in Layers)
            {
                layer.ZoomLevel = Math.Max(0.1, layer.ZoomLevel - 0.1);
                layer.ZoomOut();
                
            } // Decrease zoom level but don't go below 0.1
        }
        private void Gouche() {
            var strokeGeometry = new RectangleGeometry(new Rect(0, 0, 1, 1));

            // Use a radial gradient to simulate paint-like transparency and softness
            var gradientBrush = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.5, 0.5),
                Center = new Point(0.5, 0.5),
                RadiusX = 0.7,
                RadiusY = 0.7,
                GradientStops = new GradientStopCollection
        {
            new GradientStop(CurrentColor, 0.0), // Fully opaque in the center
            new GradientStop(Color.FromArgb(200, CurrentColor.R, CurrentColor.G, CurrentColor.B), 0.6), // Slightly transparent near edges
            new GradientStop(Color.FromArgb(50, CurrentColor.R, CurrentColor.G, CurrentColor.B), 1.0)   // Highly transparent at the very edge
        }
            };

            // Create the main stroke drawing with the geometry and gradient brush
            var strokeDrawing = new GeometryDrawing(gradientBrush, null, strokeGeometry);

            // Create the DrawingBrush
            var drawingBrush = new DrawingBrush(strokeDrawing)
            {
                TileMode = TileMode.None, // No tiling, to make the brush continuous
                Viewport = new Rect(0, 0, 1, 1), // Define the size of the brush stroke
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox
            };

            // Set the current brush to simulate smooth gouache
            _currentBrush = drawingBrush;
            UpdateBrush();
            _usedColors.Add(CurrentColor);
        }
        private void UpdateCanvas()
        {
            if (SelectedLayer is DrawingLayer imageLayer)
            {
                try
                {
                    int w, h;
                    w = int.Parse(width);
                    h = int.Parse(height);
                    imageLayer._canvas.Width = w;
                    imageLayer._canvas.Height = h;
                }
                catch {
                    MessageBox.Show("Please insert numbers");
                }

            }
        }
        private void Histo()
        {
            if (SelectedLayer is ImageLayer imageLayer)
            {
                imageLayer.CalculateHistogram();
            }
        }
        private void ShapeDetects()
        {
            if(SelectedLayer is DrawingLayer drawingLayer)
            {
                drawingLayer.corectShapes = !(drawingLayer.corectShapes);
            }
        }
        private void References()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp",
                Title = "Select Images",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Convert the list of selected image file paths into an ObservableCollection
                var selectedImages = new ObservableCollection<string>(openFileDialog.FileNames.ToList());

                // Pass the ObservableCollection to the ImageSliderWindow
                var imageSliderWindow = new ProcessedImage(selectedImages);
                imageSliderWindow.Show();
            }
        }
        private void Undo()
        {
            SelectedLayer?.Undo();
        }

        

        private void Redo()
        {
            SelectedLayer?.Redo();
        }

        
        public void SetShapeType(ShapeKind? shapeType)
        {
            if (shapeType.HasValue) { 
                _selectedShapeType= shapeType.Value;
                if (Points.Count >= 2)
                {
                    Point? p1 = Points.Pop();
                    Point? p2 = Points.Pop();
                    if (_selectedShapeType == ShapeKind.Heart||_selectedShapeType==ShapeKind.Triangle)
                    {
                        DrawShape((p2, _selectedShapeType));
                    }
                    else if (_selectedShapeType == ShapeKind.Square || _selectedShapeType == ShapeKind.Circle)
                    {
                        DrawShape((p2, _selectedShapeType));
                    }
                    else
                    {
                        StartShape((p1, _selectedShapeType));
                        EndShape(p2);
                    }
                }
                
                
                
            }
        }
        private void DrawShape((Point?,ShapeKind) data)
        {
            var (startPoint, shapeType) = data;
            if (startPoint == null || shapeType == ShapeKind.None || !(SelectedLayer is DrawingLayer drawingLayer)) return;
            drawingLayer.DrawShape(startPoint.Value, shapeType);
        }
        private void StartShape((Point?, ShapeKind) data)
        {
            var (startPoint, shapeType) = data;
            if (startPoint == null || shapeType == ShapeKind.None || !(SelectedLayer is DrawingLayer drawingLayer)) return;
            drawingLayer.StartShape(startPoint.Value, shapeType);
        }

        private void EndShape(Point? endPoint)
        {
            if (endPoint == null || !(SelectedLayer is DrawingLayer drawingLayer)) return;
            drawingLayer.EndShape(endPoint.Value);
        }
        private void OnLayerChecked(Layer selectedLayer)
        {
            foreach (var layer in Layers)
            {
                if (layer != selectedLayer)
                {
                    layer._isSelected = false;
                }
            }
            selectedLayer._isSelected = true;
            SelectedLayer = selectedLayer;
        }

        private void OnLayerUnchecked(Layer deselectedLayer)
        {
            deselectedLayer._isSelected = false;
            SelectedLayer._isSelected = false;
        }
        private void Layer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Layer._isSelected) && sender is Layer selectedLayer && selectedLayer._isSelected)
            {
                // Deselect all other layers when one layer is selected
                foreach (var layer in Layers)
                {
                    if (layer != selectedLayer && layer._isSelected)
                    {
                        layer._isSelected = false;
                    }

                }
            }
        }
       
        private void AddImageLayer()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp",
                Title = "Select an Image"
            };

            // Show the dialog and check if the user selected a file
            if (openFileDialog.ShowDialog() == true)
            {
                // Load the image from the file
                var bitmapImage = new BitmapImage(new Uri(openFileDialog.FileName));

                // Extract the dimensions of the image
                double imageWidth = bitmapImage.PixelWidth;
                double imageHeight = bitmapImage.PixelHeight;

                // Create a new ImageLayer with the selected image file and its dimensions
                var newLayer = new ImageLayer(openFileDialog.FileName, imageWidth, imageHeight);

                Layers.Add(newLayer);
                SelectedLayer = newLayer;
            }
        }

        // Method to add a new drawing layer
        private void AddDrawingLayer()
        {
            var newLayer = new DrawingLayer(StartStrokeCommand, ContinueStrokeCommand, EndStrokeCommand);
            newLayer.IsVisible = true;
            newLayer.Name = "Layer" + " " + (Layers.Count + 1).ToString();
            Layers.Add(newLayer);
            SelectedLayer = newLayer;
        }

        // Method to remove the selected layer
        private void RemoveLayer()
        {
            if (SelectedLayer != null)
            {
                int index = Layers.IndexOf(SelectedLayer);
                Layers.Remove(SelectedLayer);
                SelectedLayer=Layers.Last();
                
            }
        }

        // Check if the selected layer can be removed
        private bool CanRemoveLayer()
        {
            return SelectedLayer != null;
        }
        private void Mechanical()
        {
            var rectGeometry = new RectangleGeometry(new Rect(0, 0, 1, 1)); // Rectangular geometry for consistent stroke

            // Create a linear gradient for a consistent but slightly textured look
            var linearGradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
    {
        // Solid mechanical pencil-like color with subtle texture
        new GradientStop(CurrentColor, 0.0),
        new GradientStop(Color.FromArgb(230, CurrentColor.R, CurrentColor.G, CurrentColor.B), 0.5), // Slightly lighter in the middle
        new GradientStop(CurrentColor, 1.0)
    }
            };

            // Create the GeometryDrawing using the linear gradient brush and rectangle geometry
            var geometryDrawing = new GeometryDrawing(linearGradientBrush, null, rectGeometry);

            // Create the DrawingBrush with the GeometryDrawing
            var drawingBrush = new DrawingBrush(geometryDrawing)
            {
                TileMode = TileMode.None, // No tiling for a smooth, mechanical pencil stroke
                Viewport = new Rect(0, 0, 1, 1), // Fill the entire area
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox // Relative scaling of the brush
            };

            // Set the current brush to the mechanical pencil effect brush
            _currentBrush = drawingBrush;
            UpdateBrush();
            _usedColors.Add(CurrentColor);
        }
        private void MarkerCall()
        {
            var ellipseGeometry = new EllipseGeometry(new Point(0.5, 0.5), 0.5, 0.3); // Elliptical shape for a brush marker tip

            // Create a radial gradient to simulate the soft, blended edges of a brush marker stroke
            var gradientBrush = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.5, 0.5), // Center of the gradient
                Center = new Point(0.5, 0.5), // Center point
                RadiusX = 1.0, // Larger radius for smoother transition
                RadiusY = 0.75, // Elliptical gradient
                GradientStops = new GradientStopCollection
    {
        // Solid color at the center with softer opacity
        new GradientStop(Color.FromArgb(180, CurrentColor.R, CurrentColor.G, CurrentColor.B), 0.1), // Stronger at the center
        new GradientStop(Color.FromArgb(100, CurrentColor.R, CurrentColor.G, CurrentColor.B), 0.5), // Mid-opacity in the middle
        new GradientStop(Color.FromArgb(50, CurrentColor.R, CurrentColor.G, CurrentColor.B), 1.0) // Softer towards the edges
    }
            };

            // Create the GeometryDrawing using the gradient brush and elliptical geometry
            var geometryDrawing = new GeometryDrawing(gradientBrush, null, ellipseGeometry);

            // Create the DrawingBrush with the GeometryDrawing
            var drawingBrush = new DrawingBrush(geometryDrawing)
            {
                TileMode = TileMode.None, // No tiling for a continuous brush effect
                Viewport = new Rect(0, 0, 1, 1), // Fill the entire area
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox // Relative scaling of the brush
            };

            // Set the current brush to the brush marker effect
            _currentBrush = drawingBrush;
            UpdateBrush();
            _usedColors.Add(CurrentColor);

        }
        private void PenCall()
        {
            // Load the pencil texture from resources or a file path
          
            var rectGeometry = new RectangleGeometry(new Rect(0, 0, 1, 1));

            // Create a solid color brush with the specified color
            var solidColorBrush = new SolidColorBrush(CurrentColor);

            // Create a GeometryDrawing using the solid color brush and the rectangle geometry
            var geometryDrawing = new GeometryDrawing(solidColorBrush, null, rectGeometry);

            // Create the DrawingBrush with the GeometryDrawing
            var drawingBrush = new DrawingBrush(geometryDrawing)
            {
                // Set the Viewport to cover the entire area; no tiling needed
                Viewport = new Rect(0, 0, 1, 1),
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox,
                TileMode = TileMode.None // No tiling for a solid color effect
            };
            _currentBrush = drawingBrush;
            UpdateBrush();
            _usedColors.Add(_currentColor);
        }
        private void PencilCall()
        {
            Random rand = new Random();

            // Create a simple pencil-like texture with 50 straight horizontal lines
            var lineGroup = new GeometryGroup();

            // Add 50 straight horizontal lines to the group
            for (int i = 0; i < 50; i++)
            {
                // Calculate the y-offset for each line
                double offset = i * 10;

                // Randomize the line thickness for a natural pencil effect
                double randomThickness = rand.NextDouble() * 20 + 1; // Random thickness between 1 and 3

                // Add straight horizontal lines to the group
                lineGroup.Children.Add(new LineGeometry(new Point(0, offset), new Point(100, offset))); // Horizontal line

                // Create a pen with a random thickness
                var pencilColor = new SolidColorBrush(CurrentColor)
                {
                    Opacity = 0.85
                };

                var pencilPen = new Pen(pencilColor, randomThickness)
                {
                    LineJoin = PenLineJoin.Round,
                    StartLineCap = PenLineCap.Round,
                    EndLineCap = PenLineCap.Round
                };

                // Create the GeometryDrawing using the line group and pencil pen
                var pencilDrawing = new GeometryDrawing(null, pencilPen, lineGroup);

                // Use the drawing as a tiled texture on a DrawingBrush
                _currentBrush = new DrawingBrush(pencilDrawing)
                {
                    TileMode = TileMode.Tile,
                    Viewport = new Rect(0, 0, 200, 200),  // Adjust the viewport size for desired effect
                    ViewportUnits = BrushMappingMode.Absolute,
                    Opacity = 0.8
                };

                // Update the brush for drawing
                UpdateBrush();
            }

            // Add the current color to the used color set
            _usedColors.Add(CurrentColor);

            // Run only the heavy 'for' loop in a background task

        }
        private void StartStroke(Point? startPoint)
        {
          
            
            if (startPoint == null || SelectedLayer is not DrawingLayer drawingLayer)
                return;
            if (startPoint != null)
            {
                Points.Push(startPoint.Value);
                var m=GetMirroredPoint(startPoint.Value);
                MirroredPoints.Push(m);

                
            }
            

            // Initialize a new polyline to represent the stroke
            drawingLayer.StartStroke(startPoint.Value);


            _usedColors.Add(_currentColor);
            List<Color> colors = new List<Color>();
            int i = 0;
            foreach (var color in _usedColors)
            {

                Brush newBrush = new SolidColorBrush(color);
                colors.Add(color);
                if (i % 8 == 0)
                {
                    Color1 = newBrush;
                }
                else if (i % 8 == 1)
                {
                    Color2 = newBrush;
                }
                else if (i % 8 == 2)
                {
                    Color3 = newBrush;
                }
                else if (i % 8 == 3)
                {
                    Color4 = newBrush;
                }
                else if (i % 8 == 4)
                {
                    Color5 = newBrush;
                }
                else if (i % 8 == 5)
                {
                    Color6 = newBrush;
                }
                else if (i % 8 == 6)
                {
                    Color7 = newBrush;
                }
                else if(i % 8 == 7)
                {

                }
                i++;

            }
            HSVColours h = new HSVColours(colors);
            h.ColorHarmony();
            if(colors.Count>=2)
            {
                if(h.harmony==true)
                {
                    Harmony=new SolidColorBrush(Colors.Green);
                }
                else
                {
                    Harmony = new SolidColorBrush(Colors.Red);

                }
            }
        }

        private void ContinueStroke(Point? currentPoint)
        {
            if (currentPoint == null || SelectedLayer is not DrawingLayer drawingLayer)
                return;

            // Add the current point to the polyline
            drawingLayer.ContinueStroke(currentPoint.Value);
            var m = GetMirroredPoint(currentPoint.Value);
            MirroredPoints.Push(m);

        }

        private void EndStroke()
        {
            List<Color> colors = new List<Color>();
            foreach(var color in _usedColors)
            {
                colors.Add(color);
            }


            if (SelectedLayer is DrawingLayer drawingLayer)
            {
                
                // Finalize the stroke
                drawingLayer.EndStroke();


                if (MirrorModeEnabled) {
                    drawingLayer.StartStroke(MirroredPoints.First(),true);
                    foreach (var point in MirroredPoints.Skip(1))
                    {
                        drawingLayer.ContinueStroke(point,true);
                    }
                    drawingLayer.EndStroke(true);
                }
                
            }
            

            // Clear any temporary data used for tracking mirrored points
           
        }


        public Brush Harmony
        {
            get => harmony;
            set => SetProperty(ref harmony, value);
        }
        public Brush Color1
        {
            get => _color1;
            set => SetProperty(ref _color1, value);
        }

        public Brush Color2
        {
            get => _color2;
            set => SetProperty(ref _color2, value);
        }

        public Brush Color3
        {
            get => _color3;
            set => SetProperty(ref _color3, value);
        }

        public Brush Color4
        {
            get => _color4;
            set => SetProperty(ref _color4, value);
        }

        public Brush Color5
        {
            get => _color5;
            set => SetProperty(ref _color5, value);
        }

        public Brush Color6
        {
            get => _color6;
            set => SetProperty(ref _color6, value);
        }

        public Brush Color7
        {
            get => _color7;
            set => SetProperty(ref _color7, value);
        }
        public Brush Color8
        {
            get => _color8;
            set => SetProperty(ref _color7, value);
        }

        public Brush RectangleFill
        {
            get => _rectangleFill;
            set => SetProperty(ref _rectangleFill, value);
        }
        public Brush BackgroundColor
        {
            get => _backgroundColor;
            set=> _backgroundColor = value;
            
        }

        public StrokeCollection Strokes { get; set; }

        public DrawingAttributes DrawingAttributes
        {
            get => _drawingAttributes;
            set => SetProperty(ref _drawingAttributes, value);
        }

        public double BrushSize
        {
            get => _brushSize;
            set => SetProperty(ref _brushSize, value);
        }

        public double Hue
        {
            get => _hue;
            set => SetProperty(ref _hue, value);
        }

        public double Saturation
        {
            get => _saturation;
            set => SetProperty(ref _saturation, value);
        }

        public double Brightness
        {
            get => _brightness;
            set => SetProperty(ref _brightness, value);
        }

        public double Opacity
        {
            get => _opacity;
            set
            {
                if (SetProperty(ref _opacity, value))
                {
                    UpdateOpacity(value);
                }
            }
        }

        public double InkCanvasWidth
        {
            get => _inkCanvasWidth;
            set => SetProperty(ref _inkCanvasWidth, value);
        }

        public double InkCanvasHeight
        {
            get => _inkCanvasHeight;
            set => SetProperty(ref _inkCanvasHeight, value);
        }

        public ICommand SaveCommand { get; }

        public ICommand EraserCommand { get; }
        private void EraserCall()
        {
            var ellipseGeometry = new EllipseGeometry(new Point(0.5, 0.5), 0.5, 0.5);

            // Define the brush and pen for the geometry
            var fillBrush = new SolidColorBrush(Colors.White);
            var outlinePen = new Pen(new SolidColorBrush(Colors.White), 0.05);

            // Create the GeometryDrawing using the fill brush, outline pen, and geometry
            var geometryDrawing = new GeometryDrawing(fillBrush, outlinePen, ellipseGeometry);

            // Create the DrawingBrush with the GeometryDrawing
            var drawingBrush = new DrawingBrush(geometryDrawing)
            {
                TileMode = TileMode.Tile, // Repeat the drawing in both directions
                Viewport = new Rect(0, 0, 1, 1), // Define the size of one tile
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox // Size relative to the area being filled
            };
            _currentBrush = drawingBrush;
            UpdateBrush();
        }

        private void SaveCall(ItemsControl itemsControl)
        {
            if (itemsControl == null) return;

            // Find the Canvas within the ItemsControl
            Canvas canvas = FindVisualChild<Canvas>(itemsControl);

            if (canvas == null) return;

            // Define the size of the canvas
            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;

            // Create a RenderTargetBitmap to render the canvas
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)width, (int)height, 96d, 96d, PixelFormats.Pbgra32);

            // Render the canvas to the RenderTargetBitmap
            renderBitmap.Render(canvas);

            // Open SaveFileDialog to allow the user to specify the file path
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp",
                Title = "Save Canvas As Image",
                DefaultExt = "png"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Get the chosen file path
                string filePath = saveFileDialog.FileName;

                // Determine the file extension to select the appropriate encoder
                BitmapEncoder encoder;
                switch (System.IO.Path.GetExtension(filePath).ToLower())
                {
                    case ".jpg":
                    case ".jpeg":
                        encoder = new JpegBitmapEncoder();
                        break;
                    case ".bmp":
                        encoder = new BmpBitmapEncoder();
                        break;
                    default:
                        encoder = new PngBitmapEncoder();
                        break;
                }

                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                // Save the image to the file
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
        }
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                {
                    return t;
                }
                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }
            return null;
        }
        public Color CurrentColor
    {
        get => _currentColor;
        set => SetProperty(ref _currentColor, value);
    }

    public ObservableCollection<Color> ColorPalette
    {
        get => _colorPalette;
        set => SetProperty(ref _colorPalette, value);
    }

    

    public void UpdateBrush()
    {
        DrawingAttributes.Width = BrushSize;

        DrawingAttributes.Height = BrushSize;
            if (SelectedLayer is DrawingLayer drawingLayer)
            {
                // If it is a DrawingLayer, end the stroke
                drawingLayer.SetBrush(_currentBrush, BrushSize);
            }

        }

    public void UpdateColor(int i=0)
    {
            if (i == 0)
            {
                CurrentColor = ColorFromHSV(Hue, Saturation, Brightness);
                DrawingAttributes.Color = CurrentColor;
                RectangleFill = new SolidColorBrush(CurrentColor);
                if (SelectedLayer is DrawingLayer drawingLayer)
                {
                    // If it is a DrawingLayer, end the stroke
                    drawingLayer.SetBrush(_currentBrush, BrushSize);
                }
            }
            else
            {
               
                DrawingAttributes.Color = CurrentColor;
                RectangleFill = new SolidColorBrush(CurrentColor);
                if (SelectedLayer is DrawingLayer drawingLayer)
                {
                    // If it is a DrawingLayer, end the stroke
                    drawingLayer.SetBrush(_currentBrush, BrushSize);
                }
            }
        



        }


    private void UpdateOpacity(double value)
    {
            SelectedLayer.Opacity = value;
        
    }
        
        public void OnBrushSizeChanged(double newValue)
        {
            BrushSize = newValue;
            UpdateBrush();
        }
        public void OnHueChanged(double newValue)
        {
            Hue = newValue;
            UpdateColor();
        }

        public void OnSaturationChanged(double newValue)
        {
            Saturation = newValue;
            UpdateColor();
        }

        public void OnBrightnessChanged(double newValue)
        {
            Brightness = newValue;
            UpdateColor();
        }

        public void OnOpacityChanged(double newValue)
        {
            Opacity = newValue;
        }
        private Color ColorFromHSV(double hue, double saturation, double brightness)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        double f = hue / 60 - Math.Floor(hue / 60);

        brightness = brightness * 255;
        byte v = Convert.ToByte(brightness);
        byte p = Convert.ToByte(brightness * (1 - saturation));
        byte q = Convert.ToByte(brightness * (1 - f * saturation));
        byte t = Convert.ToByte(brightness * (1 - (1 - f) * saturation));

        if (hi == 0)
            return Color.FromRgb(v, t, p);
        else if (hi == 1)
            return Color.FromRgb(q, v, p);
        else if (hi == 2)
            return Color.FromRgb(p, v, t);
        else if (hi == 3)
            return Color.FromRgb(p, q, v);
        else if (hi == 4)
            return Color.FromRgb(t, p, v);
        else
            return Color.FromRgb(v, p, q);
    }
        
       
}
}
