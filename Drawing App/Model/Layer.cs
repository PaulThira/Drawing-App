﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
namespace Drawing_App.Model
{
   public abstract class Layer:BindableBase
    {
        private double _opacity;
        private bool _isVisible;
        private string _name;
        public bool _isSelected {  get; set; }
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
            set
            {
                if (SetProperty(ref _isVisible, value))
                {
                    // Trigger the command when visibility changes
                    OnVisibilityChangedCommand?.Execute(_isVisible);
                }
            }
        }
        private double _zoomLevel = 1.0;
        public virtual double ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                if (_zoomLevel != value)
                {
                    _zoomLevel = value;
                    RaisePropertyChanged(nameof(ZoomLevel));  // Notify UI
                }
            }
        }
        public ICommand OnVisibilityChangedCommand { get; }
        public abstract UIElement VisualElement {  get; }

        protected Layer(double opacity = 1.0, bool isVisible = true, string name = "Layer")
        {
            Opacity = opacity;
            IsVisible = isVisible;

            Name = name;
            OnVisibilityChangedCommand = new DelegateCommand<bool?>(OnVisibilityChanged);
            _isSelected = false;
            


        }
         

        // Virtual methods for zooming
        public virtual void ZoomIn()
        {
            ZoomLevel += 0.1; // Default behavior
           
        }

        public virtual void ZoomOut()
        {
            ZoomLevel = Math.Max(0.1, ZoomLevel - 0.1); // Default behavior
            
        }
        public virtual void Undo()
        {
            // Default implementation (if any) or abstract
        }
        
        public virtual void Redo()
        {
            // Default implementation (if any) or abstract
        }
        protected virtual void OnVisibilityChanged(bool? isVisible)
        {
            // Your logic here
            if (isVisible==true)
            {
                VisualElement.Visibility = Visibility.Visible;
            }
            else
            {
                VisualElement.Visibility = Visibility.Hidden;
                // Layer became invisible
            }
        }

    }
}
