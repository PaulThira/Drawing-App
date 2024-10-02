using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Drawing_App.Model
{
    public class CustomPallete:BindableBase
    {
        public SolidColorBrush ColorBrush { get; set; }
        public int Index { get; set; }

        // Optional: Add more metadata if needed
       
        public ICommand PickedColor { get; set; }
        public CustomPallete(SolidColorBrush brush, int index, Action<int> onSelectColor) {
            PickedColor = new DelegateCommand(()=> onSelectColor(Index));
            ColorBrush = brush;
            Index = index;
        }
        public CustomPallete(SolidColorBrush brush, int index)
        {
            PickedColor = new DelegateCommand(Pick);
            ColorBrush = brush;
            Index = index;
        }
        private void Pick()
        {
            MessageBox.Show(Index.ToString());
        }
    }
}
