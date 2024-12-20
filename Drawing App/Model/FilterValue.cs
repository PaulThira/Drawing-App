using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drawing_App.Model
{
    public class FilterValue
    {
        private string _value;

        // The raw string value (e.g., input from UI)
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                IsValid = double.TryParse(_value, out _numericValue);
            }
        }

        // Indicates if the value is valid
        public bool IsValid { get; private set; }

        // The numeric representation (parsed from the string)
        private double _numericValue;
        public double NumericValue => IsValid ? _numericValue : 0.0;

        // Constructor
        public FilterValue(string value)
        {
            Value = value; // Automatically checks validity
        }

        public FilterValue()
        {
            Value = "null";
        }
    }
}
