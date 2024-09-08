using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
namespace Drawing_App.VM
{
    public class HistogramVM:BindableBase
    {
        public ObservableCollection<UIElement> RedHistogramBars { get; set; }
        public ObservableCollection<UIElement> GreenHistogramBars { get; set; }
        public ObservableCollection<UIElement> BlueHistogramBars { get; set; }
        public HistogramVM(int[] redHistogram, int[] greenHistogram, int[] blueHistogram) {

            RedHistogramBars = new ObservableCollection<UIElement>();
            GreenHistogramBars = new ObservableCollection<UIElement>();
            BlueHistogramBars = new ObservableCollection<UIElement>();

            // Populate the bars for each histogram
            DrawHistogram(RedHistogramBars, redHistogram, Colors.Red);
            DrawHistogram(GreenHistogramBars, greenHistogram, Colors.Green);
            DrawHistogram(BlueHistogramBars, blueHistogram, Colors.Blue);
        }
        private void DrawHistogram(ObservableCollection<UIElement> barsCollection, int[] histogram, Color color)
        {
            // Clear previous bars
            barsCollection.Clear();

            // Find the maximum frequency to scale the bars
            int maxFrequency = histogram.Max();

            // Set the bar width based on a fixed canvas size (or you can pass the actual size dynamically)
            double canvasWidth = 256;  // Assume 256 bins
            double canvasHeight = 100; // Example canvas height
            double barWidth = canvasWidth / 256;

            // Draw each bar based on the histogram values
            for (int i = 0; i < 256; i++)
            {
                double barHeight = (histogram[i] / (double)maxFrequency) * canvasHeight;

                var bar = new Rectangle
                {
                    Fill = new SolidColorBrush(color),
                    Width = barWidth,
                    Height = barHeight
                };

                // Set position of the bar using a TranslateTransform (to be done later in the Canvas)
                Canvas.SetLeft(bar, i * barWidth);
                Canvas.SetTop(bar, canvasHeight - barHeight); // Draw from the bottom up

                barsCollection.Add(bar);
            }
        }
    }
}
