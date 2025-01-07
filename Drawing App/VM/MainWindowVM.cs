using Drawing_App.Model;
using Drawing_App.View;
using Emgu.CV;
using Emgu.CV.Structure;
using ImageMagick;
using Microsoft.Win32;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using ColorPoint = Drawing_App.Model.ColorPoint;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;


// Ensure this matches your intended type





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
        private const int Radius = 50;
        private const int ColorCount = 360;
        public MirrorAxis axis {  get; set; }
        private double _brushSize;
        private double _hue;
        private double _saturation;
        private int selectedCustomBrushIndex=0;
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
        public Stack<DrawingBrush> drawingBrushes;
        private bool lasso {  get; set; }
        public ObservableCollection<Model.Layer> Layers { get; }
        private Model.Layer _selectedLayer;
        private ObservableCollection<ObservableCollection<CustomPallete>> _colorPalettes;

        public ObservableCollection<ObservableCollection<CustomPallete>> ColorPalettes
        {
            get => _colorPalettes;
            set => SetProperty(ref _colorPalettes, value);
        }

        // Property to track the selected layer
        public Model.Layer SelectedLayer
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
        private SolidColorBrush _selectedColor;
        public SolidColorBrush SelectedColor
        {
            get => _selectedColor;
            set { SetProperty(ref _selectedColor, value); }
        }

        public ICommand StartStrokeCommand { get; }
        public ICommand ContinueStrokeCommand { get; }
        public ICommand EndStrokeCommand { get; }
        public ICommand PencilCommand { get; }
        public ICommand PenCommand { get; }
        public ICommand MarkerCommand { get; }
        public ICommand BrushEngineCommand { get; }
        public ICommand LayerCheckedCommand { get; }
        public ICommand LayerUncheckedCommand { get; }
        public ICommand ChangeShapeKind { get; }
        private int _selectedLayerIndex;
        private byte t1, t2;
        public int basicBrushIndex {  get; set; }   
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
        public ICommand ResizeUpCommand { get; }
        public ICommand ResizeDownCommand { get; }
        public ICommand ResizeLeftCommand { get; }
        public ICommand ResizeRightCommand { get; }
        public ICommand GausianBlurrCommand { get; }
        public ICommand FastMedianFilter {  get; }
        public ICommand OpeningCommand { get; }
        public ICommand ClosingCommand { get; }
        public DelegateCommand<int?> BrushClickCommand { get; }
       public ObservableCollection<Model.CustomBrush> _brushes { get; set; }
        public ObservableCollection<String> _brush_names { get; set; }
        public ICommand SobelCommand { get; }
        public ICommand SaveThresholdCommand { get; }
        public ICommand CannyCommand { get; }
        public ICommand WatershedCommand { get; }
        public ICommand MagicWandCommand { get; }
        public ICommand SaveAsPSDFile { get; }
        public ICommand StarPen {  get; }
        public ObservableCollection<Model.ColorPoint> ColorPoints { get; set; }
        public ICommand ColorSelectedCommand { get; }
        public ICommand ConvertToDrawingLayerCommmand { get; }
        public ICommand ConvertToImageLayerCommmand { get; }
        public ICommand LoadPSDFileCommand { get; }
        public ICommand AffineTransformationCommand {  get; }
        public ICommand OtsuThresholdingCommand { get; }
        public ICommand LassoEnable {  get; }
        public ICommand MultiplyCommand { get; }
        public ICommand ScreenCommand { get; }
        public ICommand OverlayCommand { get; }
        public ICommand AddCommand {  get; }
        public ICommand SubstractCommand { get; }
        public ICommand DifferenceCommand { get; }
        public ICommand LightenCommand { get; }
        public ICommand DarkenCommand { get; }
        public ICommand SoftLightCommand { get; }
        public ICommand HardLightCommand { get; }
        public ICommand DivideCommand { get; }
        public ICommand ColorBurnCommand { get; }
        public ICommand ColorDogeCommand { get; }
        public ICommand ExclusionCommand { get; }
        public ICommand TurnOnGradient { get; }
        public ICommand GradientToolCommand { get; }
        public ICommand PerspectiveWrapCommand { get; }
        public ICommand SepiaCommand { get; }
        public ICommand AdjustBrightnessCommand { get; }
        public ICommand AdjustContrastCommand { get; }
        public ICommand CustomFiltersCommand { get; }
        public ObservableCollection<Model.CustomFilter> CustomFiltersList { get; set; }
        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetProperty(ref _selectedIndex, value);
        }
        public bool RepairMirrorMode {  get; set; } 
        public ICommand ApplyFilterCommand { get; }
        public MainWindowVM()

        {
            ApplyFilterCommand = new DelegateCommand<int?>(ApplyFilter);
            CustomFiltersList = new ObservableCollection<Model.CustomFilter>();
            CustomFiltersCommand =new DelegateCommand(CustomFilters);
            AdjustContrastCommand = new DelegateCommand(AdjustContrast);
            AdjustBrightnessCommand=new DelegateCommand(AdjustBrightness);
            SepiaCommand=new DelegateCommand(Sepia);
            PerspectiveWrapCommand = new DelegateCommand(PerspectiveWrap);
            TurnOnGradient = new DelegateCommand(TurnOn);
            GradientToolCommand = new DelegateCommand(Gradient);
            ExclusionCommand = new DelegateCommand<string?>(Exclusion);
            ColorDogeCommand=new DelegateCommand<string?>(ColorDoge);
            ColorBurnCommand=new DelegateCommand<string?>(ColorBurn);
            DivideCommand = new DelegateCommand<string?>(Divide);
            HardLightCommand=new DelegateCommand<string?>(HardLight);
            SoftLightCommand = new DelegateCommand<string?>(SoftLight);
            DarkenCommand=new DelegateCommand<string?>(Darken);
            LightenCommand = new DelegateCommand<string?>(Lighten);
            DifferenceCommand = new DelegateCommand<string?>(Difference);
            SubstractCommand = new DelegateCommand<string?>(Substract);
            AddCommand=new DelegateCommand<string?>(Add);
            OverlayCommand = new DelegateCommand<string?>(Overlay);
            ScreenCommand = new DelegateCommand<string?>(Screen);
            MultiplyCommand = new DelegateCommand<string?>(Multiply);
            LassoEnable = new DelegateCommand(Lasso);
            OtsuThresholdingCommand=new DelegateCommand(OtsuThresholding);
            AffineTransformationCommand = new DelegateCommand(AffineTransformations);
            LoadPSDFileCommand = new DelegateCommand(LoadPsdWithFileDialog);
            ConvertToDrawingLayerCommmand = new DelegateCommand(ConvertToDrawingLayer);
            ConvertToImageLayerCommmand = new DelegateCommand(ConvertToImageLayer);
            ColorPoints = new ObservableCollection<Model.ColorPoint>();
            StarPen=new DelegateCommand(PenTextureCall);
            SaveAsPSDFile = new DelegateCommand(SaveAsPsd);
            MagicWandCommand=new DelegateCommand(MagicWand);
            WatershedCommand=new DelegateCommand(Watershed);
            CannyCommand=new DelegateCommand(Canny);
            SaveThresholdCommand = new DelegateCommand(SaveThreshold);
            SobelCommand = new DelegateCommand(Sobel);
            BrushClickCommand = new DelegateCommand<int?>(BrushClick);
            OpeningCommand=new DelegateCommand(Opening);
            ClosingCommand=new DelegateCommand(Closing);
            FastMedianFilter = new DelegateCommand(Median);
            GausianBlurrCommand = new DelegateCommand(Gausian);
            ResizeUpCommand = new DelegateCommand(Up);
            ResizeDownCommand = new DelegateCommand(Down);
            ResizeLeftCommand = new DelegateCommand(Left);
            ResizeRightCommand = new DelegateCommand(Right);
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
            _brushes = new ObservableCollection<Model.CustomBrush>();
            _brush_names = new ObservableCollection<string>();
            
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
            BrushEngineCommand = new DelegateCommand(BrushEngine);
            Layers = new ObservableCollection<Model.Layer>();
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
            SelectedLayer = l;
            foreach (var layer in Layers)
            {
                layer.PropertyChanged += Layer_PropertyChanged;
            }
            LayerCheckedCommand = new DelegateCommand<Model.Layer>(OnLayerChecked);
            LayerUncheckedCommand = new DelegateCommand<Model.Layer>(OnLayerUnchecked);
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
            ColorSelectedCommand = new DelegateCommand<ColorPoint>(OnColorSelected);
            SelectedPalette = ColorPalettes[0];
            t1 = (byte)Threshold;
            GenerateColorWheel();
            lasso=false;
            drawingBrushes = new Stack<DrawingBrush>();
            basicBrushIndex=0;
        }
        private void ApplyFilter(int? i)
        {
            if (i != null&&i.Value>-1&&i.Value<CustomFiltersList.Count)
            {
                // Find the index of the selected filter in the CustomFiltersList
              
                if(SelectedLayer is ImageLayer i1)
                {
                   var bgr= CustomFiltersList[i.Value].ApplyToImage(i1.Bgr);

                    ImageLayer image = new ImageLayer(bgr);
                    var Result=new ProcessedImage(ImageLayer.ConvertToBitmapSource(bgr));
                    Result.ShowDialog();
                    Layers.Add(image);
                    Layers.Remove(i1);
                }
               
                // Add logic to apply the filter to the image
            }
        }
        private void CustomFilters()
        {
            View.CustomFilter customFilter = new View.CustomFilter();
            customFilter.ShowDialog();
            if (customFilter.DialogResult==true)
            {
                var filter = customFilter.c;
                if(SelectedLayer is ImageLayer i)
                {
                    filter.ApplyToImage(i.Bgr);
                }
               CustomFiltersList.Add(filter);   
            }
        }
        private void AdjustContrast()
        {
            if (SelectedLayer is ImageLayer i)
            {
                i.AdjustContrast(Threshold*2.5);
            }
        }
        private void AdjustBrightness()
        {

            if (SelectedLayer is ImageLayer i)
            {
                i.AdjustBrightness(Threshold);
            }
        }
        private void Sepia()
        {
            if (SelectedLayer is ImageLayer i)
            {
                i.Sepia();
            }
        }
        private void PerspectiveWrap()
        {
            if (SelectedLayer is ImageLayer i)
            {
                i.PerspectiveWrap();
            }
        }
        private void TurnOn()
        {
            if(SelectedLayer is ImageLayer i)
            {
                i.gradient=!i.gradient;
            }
        }
        private void Gradient()
        {
            if (SelectedLayer is ImageLayer i)
            {
                if(i.gradient == true)
                {
                   List<Color> newColors= i.GradientColors();
                    var pal = new ObservableCollection<CustomPallete>();
                    for(int k = 0; k < newColors.Count; k++)
                    {
                        pal.Add(new CustomPallete(new SolidColorBrush(newColors[k]),k,ColorSelected));
                    }
                    ColorPalettes.Add(pal);
                }
            }

        }
        public static System.Drawing.Color ConvertMediaColorToDrawingColor(System.Windows.Media.Color mediaColor)
        {
            return System.Drawing.Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
        }

        private void Multiply(string? p)
        {
            if (p == "0")
            {
                if(SelectedLayer is ImageLayer i)
                {
                    int index=Layers.IndexOf(i);
                    if (index >= 1)
                    {
                        index--;
                        var layer=Layers[index];
                        if(layer is ImageLayer i2)
                        {
                            i.Multiply(i2);
                         

                        }
                        else if (layer is DrawingLayer d)
                        {
                           var i3=d.ConvertToImageLayer();
                      
                            i.Multiply(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Multiply(i2);
                        

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                         
                            i.Multiply(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1=d.ConvertToImageLayer();
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Multiply(i2);
                    

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                          
                            d1.Multiply(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Multiply(i2);
                       
                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                    
                            d1.Multiply(i3);
                        }

                    }

                }
            }
            else if (p == "1")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index < Layers.Count-1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Multiply(i2);
                        

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                      
                            i.Multiply(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Multiply(i2);
                          

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                          
                            i.Multiply(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index < Layers.Count-1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Multiply(i2);
                         

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                      
                            d1.Multiply(i3);
                        }

                    }
                    else
                    {
                        index =0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Multiply(i2);
                      

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                        
                            d1.Multiply(i3);
                        }

                    }

                }
            }
        }
        private void Screen(string? p)
        {
            if (p == "0")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Screen(i2);
                          

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                          
                            i.Screen(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Screen(i2);
                           

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                       
                            i.Screen(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Screen(i2);
              

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                      
                            d1.Screen(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Screen(i2);
                          

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                          
                            d1.Screen(i3);
                        }

                    }

                }
            }
            else if (p == "1")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Screen(i2);
                       

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                          
                            i.Screen(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Screen(i2);
                          

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                     
                            i.Screen(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Screen(i2);
                       

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                     
                            d1.Screen(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Screen(i2);
                          

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                       
                            d1.Screen(i3);
                        }

                    }

                }
            }
        }
        private void Overlay(string? p)
        {
            if (p == "0")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Overlay(i2);
                       

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                         
                            i.Overlay(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Overlay(i2);
                           

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                         
                            i.Overlay(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Overlay(i2);
                          

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                          
                            d1.Overlay(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Overlay(i2);
                           

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                        
                            d1.Overlay(i3);
                        }

                    }

                }
            }
            else if (p == "1")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Overlay(i2);
                            

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                        
                            i.Overlay(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Overlay(i2);
                          

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                          
                            i.Overlay(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Overlay(i2);
                          

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                    
                            d1.Overlay(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Overlay(i2);
                         

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                          
                            d1.Overlay(i3);
                        }

                    }

                }
            }
        }
        private void Add(string? p)
        {
            if (p == "0")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Add(i2);
                        

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                         
                            i.Add(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Add(i2);
                        

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
              
                            i.Add(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Add(i2);
                          

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                      
                            d1.Add(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Add(i2);
                           

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                         
                            d1.Add(i3);
                        }

                    }

                }
            }
            else if (p == "1")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Add(i2);
                           

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                          
                            i.Add(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Add(i2);
                           

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                          
                            i.Add(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Add(i2);
                         

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                       
                            d1.Add(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Add(i2);
                          

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                           
                            d1.Add(i3);
                        }

                    }

                }
            }
        }
        private void Substract(string? p)
        {
            if (p == "0")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Substract(i2);
               

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                          
                            i.Substract(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Substract(i2);
                           

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                         
                            i.Substract(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Substract(i2);
                         

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                      
                            d1.Substract(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Substract(i2);
                         

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                          
                            d1.Substract(i3);
                        }

                    }

                }
            }
            else if (p == "1")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Substract(i2);
                          

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                           
                            i.Substract(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Substract(i2);
                    

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                         
                            i.Substract(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Substract(i2);
                  

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                       
                            d1.Substract(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Substract(i2);
                   

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                          
                            d1.Substract(i3);
                        }

                    }

                }
            }
        }
        private void Difference(string? p)
        {
            if (p == "0")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Difference(i2);
                         

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                         
                            i.Difference(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Difference(i2);
                          

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                         
                            i.Difference(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Difference(i2);
                       

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                         
                            d1.Difference(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Difference(i2);
                          

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                       
                            d1.Difference(i3);
                        }

                    }

                }
            }
            else if (p == "1")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Difference(i2);
                      

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                        
                            i.Difference(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Difference(i2);
                        

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                          
                            i3.Difference(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Difference(i2);
                  

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                      
                            d1.Difference(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Difference(i2);
                          

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                           
                            d1.Difference(i3);
                        }

                    }

                }
            }
        }
        private void Lighten(string? p)
        {
            if (p == "0")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Lighten(i2);
                          

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                        
                            i.Lighten(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Lighten(i2);
                         

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                    
                            i.Lighten(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Lighten(i2);
                           

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                        
                            d1.Lighten(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Lighten(i2);
                        

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                         
                            d1.Lighten(i3);
                        }

                    }

                }
            }
            else if (p == "1")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Lighten(i2);
                         

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                          
                            i.Lighten(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Lighten(i2);
                          

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                          
                            i.Lighten(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Lighten(i2);
                         

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                     
                            d1.Lighten(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Lighten(i2);
                         

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                       
                            d1.Lighten(i3);
                        }

                    }

                }
            }
        }
        private void Darken(string? p)
        {
            if (p == "0")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Darken(i2);
                           

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                     
                            i.Darken(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Darken(i2);
                        

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                         
                            i.Darken(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Darken(i2);
                        

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                        
                            d1.Darken(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Darken(i2);
                         

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                        
                            d1.Darken(i3);
                        }

                    }

                }
            }
            else if (p == "1")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Darken(i2);
                     

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                         
                            i.Darken(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Darken(i2);
                     

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();

                            i.Darken(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Darken(i2);
                     

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                         
                            d1.Darken(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Darken(i2);
                        

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                         
                            d1.Darken(i3);
                        }

                    }

                }
            }
        }
        private void SoftLight(string? p)
        {
            if (p == "0")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.SoftLight(i2);
                         

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                        
                            i.SoftLight(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.SoftLight(i2);
                          

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                          
                            i.SoftLight(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.SoftLight(i2);
                           

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                           
                            d1.SoftLight(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.SoftLight(i2);
                           

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                           
                            d1.SoftLight(i3);
                        }

                    }

                }
            }
            else if (p == "1")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.SoftLight(i2);
                         

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                           
                            i.SoftLight(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Multiply(i2);
                         

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                          
                            i.SoftLight(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.SoftLight(i2);
                           

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                          
                            d1.SoftLight(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.SoftLight(i2);
                       

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                           
                            d1.SoftLight(i3);
                        }

                    }

                }
            }
        }
        private void HardLight(string? p)
        {
            if (p == "0")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.HardLight(i2);
                         
                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                      
                            i.HardLight(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.HardLight(i2);
                          

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                         
                            i.HardLight(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.HardLight(i2);
                          

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                        
                            d1.HardLight(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.HardLight(i2);
                          

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                           
                            d1.HardLight(i3);
                        }

                    }

                }
            }
            else if (p == "1")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.HardLight(i2);
                        

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                      
                            i.HardLight(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.HardLight(i2);
                        

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                       
                            i.HardLight(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.HardLight(i2);
                      

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                       
                            d1.HardLight(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.HardLight(i2);
                        

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                       
                            d1.HardLight(i3);
                        }

                    }

                }
            }
        }
        private void Divide(string? p)
        {
            if (p == "0")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Divide(i2);
                         

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                         
                            i.Divide(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Divide(i2);
                         

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                    
                            i.Divide(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Divide(i2);
                         

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                        
                            d1.Divide(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Divide(i2);
                          

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                      
                            d1.Divide(i3);
                        }

                    }

                }
            }
            else if (p == "1")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Divide(i2);
                       

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                    
                            i.Divide(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Divide(i2);
                         

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                       
                            i.Divide(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Divide(i2);
                         

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                          
                            d1.Divide(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Divide(i2);
                        
                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                          
                            d1.Divide(i3);
                        }

                    }

                }
            }
        }
        private void ColorBurn(string? p)
        {
            if (p == "0")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.ColorBurn(i2);
                            

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                         
                            i.ColorBurn(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.ColorBurn(i2);
                           

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                           
                            i.ColorBurn(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.ColorBurn(i2);
                           

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                          
                            d1.ColorBurn(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.ColorBurn(i2);
                          

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                            
                            d1.ColorBurn(i3);
                        }

                    }

                }
            }
            else if (p == "1")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.ColorBurn(i2);
                          

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                           
                            i.ColorBurn(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.ColorBurn(i2);
                          

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                          
                            i.ColorBurn(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.ColorBurn(i2);
                           

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                           
                            d1.ColorBurn(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.ColorBurn(i2);
                            

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                         
                            d1.ColorBurn(i3);
                        }

                    }

                }
            }
        }
        private void ColorDoge(string? p)
        {
            if (p == "0")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.ColorDoge(i2);
                         

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                          
                            i.ColorDoge(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.ColorDoge(i2);
                            

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                           
                            i.ColorDoge(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.ColorDoge(i2);
                         

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                           
                            d1.ColorDoge(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.ColorDoge(i2);
                          

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                          
                            d1.ColorDoge(i3);
                        }

                    }

                }
            }
            else if (p == "1")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.ColorDoge(i2);
                          

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                         
                            i.ColorDoge(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.ColorDoge(i2);
                           

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                          
                            i.ColorDoge(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.ColorDoge(i2);
                          

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                         
                            d1.ColorDoge(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.ColorDoge(i2);
                          

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                          
                            d1.ColorDoge(i3);
                        }

                    }

                }
            }
        }
        private void Exclusion(string? p)
        {
            if (p == "0")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Exclusion(i2);
                        

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                        
                            i.Exclusion(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Exclusion(i2);
                         

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                         
                            i.Exclusion(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index >= 1)
                    {
                        index--;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Exclusion(i2);
                       

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                          
                            d1.Exclusion(i3);
                        }

                    }
                    else
                    {
                        index = Layers.Count - 1;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Exclusion(i2);
                      

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                           
                            d1.Exclusion(i3);
                        }

                    }

                }
            }
            else if (p == "1")
            {
                if (SelectedLayer is ImageLayer i)
                {
                    int index = Layers.IndexOf(i);
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                          
                            Layers.Remove(i2);

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                          
                            i.Exclusion(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            i.Exclusion(i2);
                            

                        }
                        else if (layer is DrawingLayer d)
                        {
                            var i3 = d.ConvertToImageLayer();
                         
                            i.Exclusion(i3);
                        }

                    }
                }
                else if (SelectedLayer is DrawingLayer d)
                {
                    int index = Layers.IndexOf(d);
                    var d1 = d.ConvertToImageLayer();
                    if (index < Layers.Count - 1)
                    {
                        index++;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Exclusion(i2);
                          

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                         
                            d1.Exclusion(i3);
                        }

                    }
                    else
                    {
                        index = 0;
                        var layer = Layers[index];
                        if (layer is ImageLayer i2)
                        {
                            d1.Exclusion(i2);
                           

                        }
                        else if (layer is DrawingLayer d2)
                        {
                            var i3 = d2.ConvertToImageLayer();
                           
                            d1.Exclusion(i3);
                        }

                    }

                }
            }
        }
        private void Lasso()
        {
            if (SelectedLayer is ImageLayer i)
            {
                lasso=!lasso;
                i.lasso=lasso;
            }
        }
        private void OtsuThresholding()
        {
            if (SelectedLayer is ImageLayer i)
            {
                i.OtsuThresholding();
            }
        }
        private void AffineTransformations()
        {
            if(SelectedLayer is ImageLayer i)
            {
                AffineTransformation a = new AffineTransformation(); 
                a.ShowDialog();
                if (a.DialogResult == true)
                {
                    i.AffineTransformation(a.Matrix);
                }
            }
        }
        public void LoadPsdWithFileDialog()
        {
            // Open File Dialog
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Photoshop Files (*.psd)|*.psd",
                Title = "Open Photoshop File"
            };
            var outputDirectory = "C:\\Users\\pault\\source\\repos\\Drawing App\\Drawing App\\Textures\\";
            if (openFileDialog.ShowDialog() == true)
            {
                string psdFilePath = openFileDialog.FileName;

                // Process the PSD file
                LoadAndDisplayPsdLayers(psdFilePath, outputDirectory);
            }
        }
        public void LoadAndDisplayPsdLayers(string psdFilePath, string outputDirectory)
        {
            // Ensure the output directory exists
            Directory.CreateDirectory(outputDirectory);

            // Load the PSD file
            using (MagickImageCollection psdLayers = new MagickImageCollection(psdFilePath))
            {
                int layerIndex = 0;

                foreach (MagickImage layer in psdLayers)
                {
                    // Save each layer as a temporary PNG file
                    BitmapImage bitmapImage = ConvertMagickImageToBitmapImage(layer);

                    // Convert to BitmapImage
                   

                    // Add the layer to the canvas
                    ImageLayer i =new ImageLayer(bitmapImage);
                    Layers.Add(i);  
                    
                    layerIndex++;
                
                }
            }
        }

        public BitmapImage ConvertMagickImageToBitmapImage(MagickImage magickImage)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Write the MagickImage to a memory stream
                magickImage.Write(memoryStream, MagickFormat.Png);
                memoryStream.Position = 0;

                // Convert the memory stream to a BitmapImage
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }
        private void ConvertToDrawingLayer()
        {
            if (SelectedLayer is ImageLayer i)
            {
                DrawingLayer d = i.ConvertToDrawingLayer(StartStrokeCommand, ContinueStrokeCommand, EndStrokeCommand);
                Layers.Remove(i); 
                Layers.Add(d);
                SelectedLayer = d;
            }

        }
        private void ConvertToImageLayer()
        {
            if (SelectedLayer is DrawingLayer d)
            {
                ImageLayer i = d.ConvertToImageLayer();
                Layers.Remove(d);
                Layers.Add(i);
                SelectedLayer = i;
            }

        }
        public void SaveAsPsd()
        {
            // Create a SaveFileDialog instance
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Photoshop Files (*.psd)|*.psd", // Restrict file type to PSD
                Title = "Save As Photoshop File",
                FileName = "output.psd" // Default file name
            };

            // Show the SaveFileDialog and check if the user selected a file
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName; // Get the selected file path

                // Create a new PSD file
                using (MagickImageCollection psd = new MagickImageCollection())
                {
                    foreach (var layer in Layers) // Layers is your list of layers
                    {
                        if (layer is ImageLayer imageLayer)
                        {
                            // Add ImageLayer as a separate PSD layer
                            MagickImage image = ConvertToMagickImage(imageLayer.Bgr);
                            image.Label = imageLayer.Name; // Set layer name
                            psd.Add(image);
                        }
                        else if (layer is DrawingLayer drawingLayer)
                        {
                            // Render the DrawingLayer's _canvas to a BitmapSource
                            BitmapSource canvasBitmapSource = RenderCanvasToBitmap(drawingLayer);
                            byte[] drawingImageBytes = ConvertToByteArray(canvasBitmapSource);
                            MagickImage drawingImage = new MagickImage(drawingImageBytes);
                            drawingImage.Label = drawingLayer.Name; // Set layer name
                            psd.Add(drawingImage);
                        }
                    }

                    // Save the PSD file to the chosen location
                    psd.Write(filePath);
                }
            }
        }
        private void OnColorSelected(Model.ColorPoint selectedColorPoint)
        {
            // Perform the action when a color is selected
            SelectedColor = selectedColorPoint.Color;
            var his = new HSVColours();
            _hue = (double)his.BGRtoHSV(SelectedColor.Color).Item1;
            UpdateColor();
            // Do something with the selected color (e.g., update UI or selected color)
        }

        // Convert EmguCV Image<Bgr, byte> to MagickImage
        private byte[] ConvertToByteArray(BitmapSource source)
        {
            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                return stream.ToArray();
            }
        }
        private MagickImage ConvertToMagickImage(Image<Bgr, byte> image)
        {
            using (var bitmap = image.ToBitmap()) // Use EmguCV's ToBitmap method
            {
                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                    return new MagickImage(stream.ToArray());
                }
            }
        }
        // Render _canvas to a BitmapSource
        private BitmapSource RenderCanvasToBitmap(DrawingLayer drawingLayer)
        {
            if (drawingLayer._canvas == null)
                throw new InvalidOperationException("The canvas is not initialized.");

            // Render the canvas to a RenderTargetBitmap
            var renderTarget = new RenderTargetBitmap(
                (int)drawingLayer._canvas.ActualWidth,
                (int)drawingLayer._canvas.ActualHeight,
                96, 96, PixelFormats.Pbgra32);

            // Render the canvas visual
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                var brush = new VisualBrush(drawingLayer._canvas);
                context.DrawRectangle(brush, null, new Rect(new System.Windows.Size(drawingLayer._canvas.ActualWidth, drawingLayer._canvas.ActualHeight)));
            }
            renderTarget.Render(visual);

            return renderTarget;
        }

        // Convert a BitmapSource to System.Drawing.Bitmap for MagickImage
        private Bitmap ConvertToBitmap(BitmapSource source)
        {
            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin); // Reset stream position
                return new Bitmap(stream);
            }
        }
        // Helper Method: Render WPF Canvas to System.Drawing.Bitmap
        private System.Drawing.Bitmap RenderCanvasToBitmap(System.Windows.Controls.Canvas canvas)
        {
            var size = new System.Windows.Size(canvas.ActualWidth, canvas.ActualHeight);
            var renderBitmap = new RenderTargetBitmap(
                (int)size.Width, (int)size.Height, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);

            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                context.DrawRectangle(new System.Windows.Media.VisualBrush(canvas), null, new System.Windows.Rect(size));
            }
            renderBitmap.Render(visual);

            // Convert RenderTargetBitmap to System.Drawing.Bitmap
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            using (var memoryStream = new MemoryStream())
            {
                encoder.Save(memoryStream);
                return new System.Drawing.Bitmap(memoryStream);
            }
        }







        private void MagicWand()
        {

            if (SelectedLayer is ImageLayer i)
            {
                i.MagicTool(_currentColor,Threshold);
            }
        }
        private void Watershed()
        {
            if (SelectedLayer is ImageLayer i)
            {
                i.Watershed();
            }
        }
        private void Canny()
        {
            if (SelectedLayer is ImageLayer i)
            {
                i.Canny(t1, t2);
            }

        }
        private void SaveThreshold()
        {
         
            t2 = t1;
            t1=(byte)Threshold; 
           
        }
        private void Sobel()
        {
            if(SelectedLayer is ImageLayer i)
            {
                i.Sobel();
            }
        }
        private void BrushClick(int? i)
        {
            if (i != null)
            {
                int k=i.Value;
                _currentBrush = _brushes[k].GetDrawingBrush();
                if (SelectedLayer is DrawingLayer d) { 
                    d.SetBrush(_currentBrush,12);
                    d.EraserMode=false;
                }
                
                selectedCustomBrushIndex = k;
                basicBrushIndex = -1;
            }

        }
        private void BrushEngine()
        {
            CustomBrushes customBrushes = new CustomBrushes();
             var ok=customBrushes.ShowDialog();
            if (ok == true) {
                customBrushes._brush.UpdateBrush();
                _brushes.Add(customBrushes._brush);
            }

        }
        private void Opening()
        {
            if (SelectedLayer is ImageLayer i)

            {
                float W = Threshold / 20;
                i.Opening((int)W);
            }
        }
        private void Closing()
        {
            if (SelectedLayer is ImageLayer i)

            {
                float W = Threshold / 20;
                i.Closing((int)W);
            }
        }

        private void Median()
        {
            if(SelectedLayer is ImageLayer i)

            {
                float W = Threshold / 10;
                i.FastMedianFilter((int)W);
            }
        }
        private void Gausian()
        {
            if (SelectedLayer is ImageLayer i)
            {
                i.GausianBlurr();
            }
        }
        private void Up()
        {
            if(SelectedLayer is DrawingLayer d)
            {
                d.ResizeShapeWithArrows("Up");
            }
        }
        private void Down()
        {
            if (SelectedLayer is DrawingLayer d)
            {
                d.ResizeShapeWithArrows("Down");
            }
        }
        private void Right()
        {
            if (SelectedLayer is DrawingLayer d)
            {
                d.ResizeShapeWithArrows("Right");
            }
        }
        private void Left()
        {
            if (SelectedLayer is DrawingLayer d)
            {
                d.ResizeShapeWithArrows("Left");
            }
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
            if (type == "1")
            {
                axis = MirrorAxis.Horizontal;
                if(SelectedLayer is DrawingLayer d)
                {
                    d._previousSymmetryState=d.RepairMirrorMode;
                    d.RepairMirrorMode=true;
                  
                }
            }
            else if (type == "2")
            {
                axis = MirrorAxis.Vertical;
                if (SelectedLayer is DrawingLayer d)
                {
                    d._previousSymmetryState = d.RepairMirrorMode;
                    d.RepairMirrorMode = true;
                   
                   
                }
            }
            else if (type == "3") { 
                axis = MirrorAxis.Both;
                if (SelectedLayer is DrawingLayer d)
                {d._previousSymmetryState = d.RepairMirrorMode;
                    d.RepairMirrorMode = true;
                  
                }
            }
            
            else
            {
                axis = MirrorAxis.None;
                MirrorModeEnabled = false;
                if (SelectedLayer is DrawingLayer d)
                {
                    d._previousSymmetryState = d.RepairMirrorMode;
                    d.RepairMirrorMode = false;
                }
            }
        }
      
        private void ColorSelected(int selectedIndex)
        {

            if (selectedIndex >= 0 && selectedIndex < SelectedPalette.Count)
            {
                var selectedBrush = SelectedPalette[selectedIndex];
                CurrentColor = selectedBrush.ColorBrush.Color;

              
                RectangleFill = selectedBrush.ColorBrush;
                _currentBrush.Drawing = new GeometryDrawing(
                brush: new SolidColorBrush(CurrentColor),   // Fill brush
                pen: new Pen(new SolidColorBrush(CurrentColor), 1), // Outline pen
                geometry: new RectangleGeometry(new Rect(0, 0, 10, 10))
            );
               
                // Perform actions with the selected color (e.g., display a message or update UI)
                if (basicBrushIndex == 0)
                {
                    Gouche();
                }
                if (basicBrushIndex == 1) {
                    Mechanical();
                }
                if (basicBrushIndex == 2) { 
                    MarkerCall();
                }
                if (basicBrushIndex == 3) { 
                     PenTextureCall();
                }
                if (basicBrushIndex == 4) { 
                    PenCall();
                }
                if (basicBrushIndex == 5)
                {
                    PencilCall();
                }
                if (basicBrushIndex == -1) { 
                    _currentBrush=_brushes[selectedCustomBrushIndex].GetDrawingBrush(CurrentColor.B,CurrentColor.G,CurrentColor.R);
                    UpdateBrush();
                }
             
                MessageBox.Show($"You clicked on color at index: {selectedIndex} ");
            }
        }


        private void OpenPalleteGenerator()
        {
            ColorPalleteGenerator color=new ColorPalleteGenerator();
           var ok= color.ShowDialog();
            if (ok == true)
            {
                ObservableCollection<CustomPallete> palettes = new ObservableCollection<CustomPallete>();
                for (int i = 0; i < color.pallete.Count; i++)
                {
                    CustomPallete c = new CustomPallete(color.pallete[i].ColorBrush, i, ColorSelected);
                    palettes.Add(c);
                }
                ColorPalettes.Add(palettes);
               
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
            drawingBrushes.Push(drawingBrush);
            // Set the current brush to simulate smooth gouache
            _currentBrush = drawingBrush;
            UpdateBrush();
            basicBrushIndex = 0;
            _usedColors.Add(CurrentColor);
            if (SelectedLayer is DrawingLayer d)
            {
                d.EraserMode = false;
            }
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

        private void GenerateColorWheel()
        {
            ColorPoints.Clear();

            // Generate colors based on the HSV color wheel
            for (int i = 0; i < ColorCount; i++)
            {
                // Calculate the angle for placing the color
                double angle = i * (360.0 / ColorCount);
                double radians = angle * (Math.PI / 180.0);

                // Position for the color based on angle and radius
                double x = Radius + (Radius * Math.Cos(radians));
                double y = Radius + (Radius * Math.Sin(radians));
                HSVColours h = new HSVColours();

                // Convert the angle to a color in the HSV model
                var hsvColor = h.ColorFromHSV((double)i, 1.0, 1.0);  // Full saturation, full value for bright colors

                // Add the color and position to the collection
                ColorPoints.Add(new Model.ColorPoint
                {
                    X = x,
                    Y = y,
                    Color = new SolidColorBrush(hsvColor)
                });
            }
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
        private void OnLayerChecked(Model.Layer selectedLayer)
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

        private void OnLayerUnchecked(Model.Layer deselectedLayer)
        {
            deselectedLayer._isSelected = false;
            SelectedLayer._isSelected = false;
        }
        private void Layer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model.Layer._isSelected) && sender is Model.Layer selectedLayer && selectedLayer._isSelected)
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
            var rectGeometry = new RectangleGeometry(new Rect(0, 0, 1, 1)); // Keep the original size for consistent stroke

            // Create a linear gradient for a consistent but slightly textured look
            var linearGradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
        {
            // Reduce opacity by lowering alpha values
            new GradientStop(Color.FromArgb(150, CurrentColor.R, CurrentColor.G, CurrentColor.B), 0.0), // Starting color with reduced opacity
            new GradientStop(Color.FromArgb(100, CurrentColor.R, CurrentColor.G, CurrentColor.B), 0.5), // Lighter and more transparent in the middle
            new GradientStop(Color.FromArgb(150, CurrentColor.R, CurrentColor.G, CurrentColor.B), 1.0) // Ending color with reduced opacity
        }
            };

            // Create the GeometryDrawing using the linear gradient brush and rectangle geometry
            var geometryDrawing = new GeometryDrawing(linearGradientBrush, null, rectGeometry);

            // Create the DrawingBrush with the GeometryDrawing
            var drawingBrush = new DrawingBrush(geometryDrawing)
            {
                TileMode = TileMode.None, // No tiling for a smooth, mechanical pencil stroke
                Viewport = new Rect(0, 0, 1, 1), // Fill the entire area (no size change)
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox // Relative scaling of the brush
            };

            drawingBrushes.Push(drawingBrush);

            // Set the current brush to the mechanical pencil effect brush
            _currentBrush = drawingBrush;
            UpdateBrush();
            _usedColors.Add(CurrentColor);
            basicBrushIndex = 1;

            if (SelectedLayer is DrawingLayer d)
            {
                d.EraserMode = false;
            }
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
            drawingBrushes.Push(drawingBrush);
            // Set the current brush to the brush marker effect
            _currentBrush = drawingBrush;
            UpdateBrush();
            _usedColors.Add(CurrentColor);
            basicBrushIndex = 2;
            if (SelectedLayer is DrawingLayer d)
            {
                d.EraserMode = false;
            }

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
            drawingBrushes.Push(drawingBrush);
            _currentBrush = drawingBrush;
            UpdateBrush();
            _usedColors.Add(_currentColor);
            basicBrushIndex = 3;
            if (SelectedLayer is DrawingLayer d)
            {
                d.EraserMode = false;
            }
        }
        private void PenTextureCall()
        {
            // Load the pencil texture from resources or a file path
            var textureUri = new Uri("C:\\Users\\pault\\source\\repos\\Drawing App\\Drawing App\\Textures\\Star.jpg"); // Adjust the path to your texture
            var bitmapImage = new BitmapImage(textureUri);

            // Create an ImageBrush with the loaded texture
            var textureBrush = new ImageBrush(bitmapImage)
            {
                Opacity = 1.0, // Set opacity
                Stretch = Stretch.Uniform, // Adjust to your preference (Uniform, UniformToFill, Fill, None)
                TileMode = TileMode.Tile, // Enable tiling for repeated patterns
                Viewport = new Rect(0, 0, 0.05, 0.05), // Adjust the size of the texture tiles
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox // Use relative coordinates for tiling
            };

            // Assign the texture brush to the current brush
            _currentBrush = new DrawingBrush
            {
                Drawing = new GeometryDrawing(textureBrush, null, new EllipseGeometry(new Point(0.5, 0.5), 0.5, 0.5))
            };

            drawingBrushes.Push(new DrawingBrush
            {
                Drawing = new GeometryDrawing(textureBrush, null, new EllipseGeometry(new Point(0.5, 0.5), 0.5, 0.5))
            });
            // Optionally, update the brush properties if needed
            UpdateBrush();
            basicBrushIndex = 4;

            if (SelectedLayer is DrawingLayer d)
            {
                d.EraserMode = false;
            }
            // Add the current color to used colors (for tracking or display purposes)
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
                drawingBrushes.Push(new DrawingBrush(pencilDrawing)
                {
                    TileMode = TileMode.Tile,
                    Viewport = new Rect(0, 0, 200, 200),  // Adjust the viewport size for desired effect
                    ViewportUnits = BrushMappingMode.Absolute,
                    Opacity = 0.8
                });
                // Update the brush for drawing
                UpdateBrush();
            }
            basicBrushIndex = 5;
            // Add the current color to the used color set
            _usedColors.Add(CurrentColor);
            if (SelectedLayer is DrawingLayer d)
            {
                d.EraserMode = false;
            }
            // Run only the heavy 'for' loop in a background task

        }
        private void StartStroke(Point? startPoint)
        {
          
            
           
            if(SelectedLayer is DrawingLayer drawingLayer)
            {
                if (startPoint != null)
                {
                    MirroredPoints.Clear();
                    Points.Push(startPoint.Value);
                    var m = GetMirroredPoint(startPoint.Value);
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
                    else if (i % 8 == 7)
                    {

                    }
                    i++;

                }
                HSVColours h = new HSVColours(colors);
                h.ColorHarmony();
                if (colors.Count >= 2)
                {
                    if (h.harmony == true)
                    {
                        Harmony = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        Harmony = new SolidColorBrush(Colors.Red);

                    }
                }
            }
            else if(SelectedLayer is ImageLayer imageLayer && lasso==true)
            {
                if (startPoint == null)
                    return;

                // Clear any existing boundary points
                Points.Clear();

                // Initialize the selection boundary with the starting point
                Points.Push(startPoint.Value);
               

                // Optionally, render the starting point visually
                RenderSelectionBoundary(imageLayer);
            }
           
        }

        private void ContinueStroke(Point? currentPoint)
        {
            if (SelectedLayer is DrawingLayer drawingLayer && currentPoint != null)
            {
                drawingLayer.ContinueStroke(currentPoint.Value);
                Points.Push(currentPoint.Value);    
                var m = GetMirroredPoint(currentPoint.Value);
                MirroredPoints.Push(m);
            }
            else if (SelectedLayer is ImageLayer imageLayer && lasso==true) {
                if (currentPoint == null)
                    return;

                // Add the current point to the selection boundary
                Points.Push(currentPoint.Value);

                // Update the rendered boundary
                RenderSelectionBoundary(imageLayer);
            }

            // Add the current point to the polyline
        

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


                if (MirrorModeEnabled)
                {
                    // Clear previous mirrored points
                    MirroredPoints.Clear();

                    // Generate mirrored points based on the current polyline's points
                    if (Points != null)
                    {
                        foreach (var point in Points)
                        {
                            Point mirroredPoint = GetMirroredPoint(point);
                            MirroredPoints.Push(mirroredPoint);
                        }

                        // Start the mirrored stroke
                        drawingLayer.StartStroke(MirroredPoints.First());

                        // Add mirrored points to the mirrored stroke
                        foreach (var mirroredPoint in MirroredPoints)
                        {
                            drawingLayer.ContinueStroke(mirroredPoint);
                        }

                        // Finalize the mirrored stroke
                        drawingLayer.EndStroke(true);
                    }
                    Points.Clear();
                    // Clear mirrored points after the mirrored stroke is finalized
                    MirroredPoints.Clear();
                }

            }
            else if (SelectedLayer is ImageLayer imageLayer && lasso == true) {
                if (Points.Count < 3)
                    return; // Ensure we have enough points to form a closed boundary

                // Close the boundary by connecting the last point to the first
                Points.Push(Points.First());

                // Apply the selection boundary (e.g., for cropping or masking)
                ApplySelectionBoundary(imageLayer);
            }
            

            // Clear any temporary data used for tracking mirrored points
           
        }
        private void RenderSelectionBoundary(ImageLayer imageLayer)
        {
            if (Points.Count < 2)
                return;

            // Create a geometry from the boundary points
            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure { StartPoint = Points.First() };

            foreach (var point in Points.Skip(1))
            {
                figure.Segments.Add(new LineSegment(point, true));
            }

            geometry.Figures.Add(figure);

            // Render the geometry as a boundary overlay on the Image Layer
            imageLayer.RenderBoundary();
        }
        private void ApplySelectionBoundary(ImageLayer imageLayer)
        {
            if (Points.Count < 3)
                return;

            // Create a polygon mask from the boundary points
            PolygonSelectionMask mask = new PolygonSelectionMask(Points);

            // Apply the mask to the Image Layer (e.g., for cropping or filtering)
            imageLayer.ApplyMask();
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

            // Create a transparent fill brush for the eraser
            var fillBrush = new SolidColorBrush(Colors.White); // Erase with white (or replace with transparent if supported)

            // Create a pen for the outline (optional)
            var outlinePen = new Pen(new SolidColorBrush(Colors.White), 0.05);

            // Define the GeometryDrawing using the fill brush, outline pen, and geometry
            var geometryDrawing = new GeometryDrawing(fillBrush, outlinePen, ellipseGeometry);

            // Create a DrawingBrush for the eraser
            var drawingBrush = new DrawingBrush(geometryDrawing)
            {
                TileMode = TileMode.Tile, // Repeat the drawing in both directions
                Viewport = new Rect(0, 0, 1, 1), // Define the size of one tile
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox // Size relative to the area being filled
            };

            // Assign the DrawingBrush to the current brush
            _currentBrush = drawingBrush;
            if(SelectedLayer is DrawingLayer d)
            {
                d.EraserMode= true;
            }
            UpdateBrush();
            // Set the brush to transparent (acts as an eraser)
           
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


        public void UpdateColor(int i = 0)
        {
            // Extract the current opacity from the brush
            double currentOpacity = _currentBrush?.Opacity ?? 1.0; // Default to fully opaque if _currentBrush is null

            // Calculate the new color based on the HSV values
            CurrentColor = ColorFromHSV(Hue, Saturation, Brightness);

            // Apply the current opacity to the new color
            CurrentColor = Color.FromArgb((byte)(currentOpacity * 255), CurrentColor.R, CurrentColor.G, CurrentColor.B);

            // Update the drawing attributes and the rectangle fill
            DrawingAttributes.Color = CurrentColor;
            RectangleFill = new SolidColorBrush(CurrentColor);

            // Update the brush in the selected layer
            if (SelectedLayer is DrawingLayer drawingLayer)
            {
                // Retrieve the updated brush from the selected custom brush
                if (_brushes != null && selectedCustomBrushIndex >= 0 && selectedCustomBrushIndex < _brushes.Count)
                {
                    _currentBrush = _brushes[selectedCustomBrushIndex].GetDrawingBrush(CurrentColor.B, CurrentColor.G, CurrentColor.R);
                }

                // Apply the updated brush to the layer
                drawingLayer.SetBrush(_currentBrush, BrushSize);
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
