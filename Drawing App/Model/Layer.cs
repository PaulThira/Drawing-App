using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Prism.Mvvm;
namespace Drawing_App.Model
{
   public abstract class Layer:BindableBase
    {
        private double _opacity;
        private bool _isVisible;
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);

        }

        public double Opacity
        {
            get => _opacity;
            set => SetProperty(ref _opacity, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        
        public abstract UIElement VisualElement {  get; }

        protected Layer(double opacity = 1.0, bool isVisible = true, string name = "Layer")
        {
            Opacity = opacity;
            IsVisible = isVisible;

            Name = name;
        }

    }
}
