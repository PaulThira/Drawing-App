using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Drawing_App.Model;
using Prism.Commands;

namespace Drawing_App.VM
{
    public class ProcessedImageVM: BindableBase
    {
        private ObservableCollection<ImageLayer> _images;
        private int _currentIndex;
        private BitmapImage _currentImage;

        public ObservableCollection<ImageLayer> Images
        {
            get => _images;
            set => SetProperty(ref _images, value);
        }

        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                SetProperty(ref _currentIndex, value);
                UpdateCurrentImage();
            }
        }

        public BitmapImage CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value);
        }

        public ProcessedImageVM()
        {
            Images = new ObservableCollection<ImageLayer>();
            CurrentIndex = 0;

            NextImageCommand = new DelegateCommand(NextImage);
            PreviousImageCommand = new DelegateCommand(PreviousImage);
        }

        public void LoadImages(IEnumerable<string> filePaths)
        {
            foreach (var filePath in filePaths)
            {
                Images.Add(new ImageLayer(filePath));
            }

            UpdateCurrentImage();
        }

        private void UpdateCurrentImage()
        {
            if (Images.Count > 0 && CurrentIndex >= 0 && CurrentIndex < Images.Count)
            {
                CurrentImage = new BitmapImage(new Uri(Images[CurrentIndex]._filePath));
            }
        }

        public ICommand NextImageCommand { get; }
        public ICommand PreviousImageCommand { get; }

        private void NextImage()
        {
            if (CurrentIndex < Images.Count - 1)
            {
                CurrentIndex++;
            }
        }

        private void PreviousImage()
        {
            if (CurrentIndex > 0)
            {
                CurrentIndex--;
            }
        }
    }
}
