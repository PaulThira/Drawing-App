using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Input;
using System.Windows;
using Emgu.CV;

namespace Drawing_App.Model
{
    public class Pixelz:BindableBase
    {
        private SolidColorBrush _colorz;
        public int? X { get; set; } // Row index
        public int? Y { get; set; } // Column index
        public SolidColorBrush Colorz
        {
            get => _colorz;
            set => SetProperty(ref _colorz, value); // Notify UI of changes
        }

        public ICommand Click { get; set; }

        public Pixelz()
        {
            // Use your custom DelegateCommand<T>
            Click = new DelegateCommand<System.Windows.Media.Color?>(ChangeColor);
        }

        private void ChangeColor(System.Windows.Media.Color? c)
        {
            if (c != null) {
                Colorz = new SolidColorBrush(c.Value);
              
            }
            else
            {
                MessageBox.Show("It is null");
            }
        }
        // Method to change color



    }
}
