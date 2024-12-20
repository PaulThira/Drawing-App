using Drawing_App.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Drawing_App.VM
{
     public class CustomFilterVM:BindableBase
    {
        private ObservableCollection<FilterValue> _filterValues;

        public ObservableCollection<FilterValue> FilterValues
        {
            get => _filterValues;
            set => SetProperty(ref _filterValues, value);
        }
        private string _filterName;
        public string FilterName
        {
            get => _filterName;
            set => SetProperty(ref _filterName, value);
        }


        private double _brightness;
        private double _contrast;
        private double _saturation;
        private double _sharpness;
        private double _blurRadius;
        private double _edgeIntensity;

        public double Brightness
        {
            get => _brightness;
            set => SetProperty(ref _brightness, value);
        }

        public double Contrast
        {
            get => _contrast;
            set => SetProperty(ref _contrast, value);
        }

        public double Saturation
        {
            get => _saturation;
            set => SetProperty(ref _saturation, value);
        }

        public double Sharpness
        {
            get => _sharpness;
            set => SetProperty(ref _sharpness, value);
        }

        public double BlurRadius
        {
            get => _blurRadius;
            set => SetProperty(ref _blurRadius, value);
        }

        public double EdgeIntensity
        {
            get => _edgeIntensity;
            set => SetProperty(ref _edgeIntensity, value);
        }
        public ICommand LowPassFilterCommand { get; }
        public ICommand HighPassFilterCommand { get; }
        public CustomFilterVM() {
            FilterValues = new ObservableCollection<FilterValue>(
       Enumerable.Range(0, 49).Select(_ => new FilterValue(" ")) // Initialize with default values
   );
            Brightness = 50;
            Contrast = 50;
            Saturation = 50;
            Sharpness = 50;
            BlurRadius = 50;
            EdgeIntensity = 50;
            HighPassFilterCommand = new DelegateCommand(CheckHighPass);
            LowPassFilterCommand = new DelegateCommand(CheckLowPass);
            _filterName = "Lol";
        }
        private void CheckLowPass()
        {
            double sum = 0;
            CheckValidityOfTheFilter();
            // Sum all the coefficients in the matrix
            foreach (var value in FilterMatrix)
            {
                sum += value;
            }
            if (sum > 0)
            {
                MessageBox.Show("It is low pass");
            }
            else 
            {
                MessageBox.Show("It is not low pass");
            }
        }
        private void CheckHighPass()
        {
            double sum = 0;
            CheckValidityOfTheFilter();
            // Sum all the coefficients in the matrix
            foreach (var value in FilterMatrix)
            {
                sum += value;
            }
            if (sum < 0)
            {
                MessageBox.Show("It is high pass");
            }
            else
            {
                MessageBox.Show("It is not high pass");
            }
        }
        public double[,] FilterMatrix { get; set; } // Declare the matrix to store the filter values
        
        public void CheckValidityOfTheFilter()
        {
            int count = FilterValues.Count(e => e.Value != " ");

            // Validate count first
            if (count != 9 && count != 25 && count != 49)
            {
                MessageBox.Show("Invalid number of parameters");
                return;
            }

            // Validate based on count and positions
            if (!ValidatePositions(count, out var validPositions))
            {
                MessageBox.Show("Invalid Value or Position");
                return;
            }

            // Save values into the matrix
            SaveValuesToMatrix(validPositions, count);
            MessageBox.Show("Filter is valid and saved to matrix");
        }

        private bool ValidatePositions(int count, out List<int> validPositions)
        {
            validPositions = new List<int>();
            int centerIndex = 25; // Center for 7x7 grid
            int[] relativePositions;

            switch (count)
            {
                case 9:
                    relativePositions = Get3x3Positions(centerIndex);
                    break;
                case 25:
                    relativePositions = Get5x5Positions(centerIndex);
                    break;
                case 49:
                    relativePositions = Enumerable.Range(0, 49).ToArray();
                    break;
                default:
                    return false;
            }

            foreach (var index in relativePositions)
            {
                if (FilterValues[index].Value != " ")
                {
                    if (int.TryParse(FilterValues[index].Value, out _))
                    {
                        validPositions.Add(index);
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void SaveValuesToMatrix(List<int> validPositions, int count)
        {
            int size = (int)Math.Sqrt(count); // Determine the size of the matrix (3x3, 5x5, 7x7)
            FilterMatrix = new double[size, size]; // Initialize the matrix

            int centerIndex = 25; // Center index for 7x7 grid
            int offset = (7 - size) / 2; // Offset for smaller grids in a 7x7 layout

            foreach (var index in validPositions)
            {
                int row = (index / 7) - offset; // Map to the corresponding row in the matrix
                int col = (index % 7) - offset; // Map to the corresponding column in the matrix

                if (row >= 0 && row < size && col >= 0 && col < size)
                {
                    FilterMatrix[row, col] = double.Parse(FilterValues[index].Value);
                }
            }
        }

        private int[] Get3x3Positions(int centerIndex)
        {
            return new[]
            {
        centerIndex, // Center
        centerIndex - 1, centerIndex + 1, // Horizontal
        centerIndex - 7, centerIndex + 7, // Vertical
        centerIndex - 8, centerIndex - 6, // Diagonal Top
        centerIndex + 6, centerIndex + 8  // Diagonal Bottom
    };
        }

        private int[] Get5x5Positions(int centerIndex)
        {
            var positions = new List<int>();

            for (int i = -2; i <= 2; i++)
            {
                for (int j = -2; j <= 2; j++)
                {
                    positions.Add(centerIndex + i * 7 + j);
                }
            }

            return positions.ToArray();
        }

    }
}
