using System;
using System.Collections.Generic;
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

namespace Drawing_App.View
{
    /// <summary>
    /// Interaction logic for Animation.xaml
    /// </summary>
    public partial class Animation : Window
    {
        public Animation()
        {
            InitializeComponent();
        }
        public Animation(string animationPath, string animationFormat)
        {
            InitializeComponent();

            if (animationFormat.Equals("gif", StringComparison.OrdinalIgnoreCase))
            {
                LoadGifAnimation(animationPath);
            }
            else if (animationFormat.Equals("mp4", StringComparison.OrdinalIgnoreCase))
            {
                // Placeholder for MP4 functionality
                MessageBox.Show("MP4 preview not yet implemented.");
            }
            else
            {
                MessageBox.Show($"Unsupported animation format: {animationFormat}");
            }
        }

        private void LoadGifAnimation(string gifPath)
        {
            var gifUri = new Uri(gifPath, UriKind.Absolute);
            AnimationImage.Source = new BitmapImage(gifUri);
        }
    }
}
