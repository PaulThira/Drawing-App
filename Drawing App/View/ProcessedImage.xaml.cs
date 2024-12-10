using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Drawing_App.VM;

namespace Drawing_App.View
{
    /// <summary>
    /// Interaction logic for ProcessedImage.xaml
    /// </summary>
    public partial class ProcessedImage : Window
    {
        public BitmapImage Image { get; set; }
        public int correctWidth { get; set; } = 0;
        public int correctHeight { get; set; } = 0;
        public ProcessedImage()
        {
            InitializeComponent();
        }
        public ProcessedImage(IEnumerable<string> imagePaths)
        {
            InitializeComponent();
            var viewModel = new ProcessedImageVM();
            viewModel.LoadImages(imagePaths);
            DataContext = viewModel;
        }
        private BitmapImage ConvertBitmapSourceToBitmapImage(BitmapSource source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), "BitmapSource cannot be null.");

            // Use a MemoryStream to convert the BitmapSource to a BitmapImage
            using (var memoryStream = new MemoryStream())
            {
                // Choose a PNG encoder to preserve transparency (if needed)
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));

                // Save the BitmapSource to the stream
                encoder.Save(memoryStream);

                // Create a new BitmapImage from the memory stream
                memoryStream.Seek(0, SeekOrigin.Begin); // Reset stream position
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = new MemoryStream(memoryStream.ToArray()); // Use a separate stream to avoid disposal issues
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Make the BitmapImage thread-safe

                return bitmapImage;
            }
        }

        public ProcessedImage(BitmapSource zoomedImage)
        {
            InitializeComponent();

            
            // Initialize the ViewModel and set the DataContext
            BitmapImage bitmapImage = ConvertBitmapSourceToBitmapImage(zoomedImage);
            correctHeight=(int)bitmapImage.Height;
            correctWidth=(int)bitmapImage.Width;
            // Initialize the ViewModel and set the DataContext
            var viewModel = new ProcessedImageVM();
              // Assign converted BitmapImage to the ViewModel
            DataContext = viewModel;
            viewModel.CurrentImage = bitmapImage;
        }
        public ProcessedImage(BitmapImage image)
        {
            InitializeComponent();

            correctHeight = (int)image.Height;
            correctWidth = (int)image.Width;

            // Initialize the ViewModel and set the DataContext
            var viewModel = new ProcessedImageVM
            {
                CurrentImage = image
            };
            DataContext = viewModel;

            // No need to set Imagery dimensions manually
        }
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Get the clicked position relative to the image
            var image = sender as Image;
            var clickPosition = e.GetPosition(image);
            var width = image.ActualWidth;
            var height = image.ActualHeight;
            var ActualPos = new Point();
            ActualPos.X =(clickPosition.X*correctWidth)/width;
            ActualPos .Y =(clickPosition.Y*correctHeight)/height;
            // Pass the coordinates to the ViewModel
            var viewModel = DataContext as ProcessedImageVM;
            if (viewModel != null && viewModel.ImageClickCommand.CanExecute(ActualPos))
            {
                viewModel.ImageClickCommand.Execute(ActualPos);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            var viewModel = DataContext as ProcessedImageVM;
            if (viewModel != null )
            {
                Image=viewModel.CurrentImage;
                this.Close();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            var viewModel = DataContext as ProcessedImageVM;
            if (viewModel != null)
            {
                Image = viewModel.CurrentImage;
                this.Close();
            }

        }
    }
}
