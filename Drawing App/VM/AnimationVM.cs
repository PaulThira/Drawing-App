using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Drawing_App.VM
{
    public class AnimationVM:BindableBase
    {
        private BitmapImage _animationImage;
        private DispatcherTimer _animationTimer;
        private int _currentFrameIndex;

        public BitmapImage AnimationImage
        {
            get => _animationImage;
            set => SetProperty(ref _animationImage, value);
        }

        public DelegateCommand StartAnimationCommand { get; }
        public DelegateCommand PauseAnimationCommand { get; }
        public DelegateCommand SaveAnimationCommand { get; }
        public DelegateCommand ClosePreviewCommand { get; }

        public AnimationVM()
        {
            StartAnimationCommand = new DelegateCommand(StartAnimation);
            PauseAnimationCommand = new DelegateCommand(PauseAnimation);
            SaveAnimationCommand = new DelegateCommand(SaveAnimation);
            ClosePreviewCommand = new DelegateCommand(ClosePreview);

            _animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) }; // Adjust frame speed
            _animationTimer.Tick += UpdateAnimationFrame;
        }

        private void StartAnimation()
        {
            _animationTimer.Start();
        }

        private void PauseAnimation()
        {
            _animationTimer.Stop();
        }

        private void SaveAnimation()
        {
            // Save the animation to a file
            string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "animation_preview.gif");
            File.Copy(CurrentGifPath, savePath, overwrite: true);
        }

        private void ClosePreview()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateAnimationFrame(object sender, EventArgs e)
        {
            if (FramePaths.Count == 0) return;

            // Loop through frames
            _currentFrameIndex = (_currentFrameIndex + 1) % FramePaths.Count;

            AnimationImage = new BitmapImage(new Uri(FramePaths[_currentFrameIndex]));
        }

        public void LoadAnimationFrames(string gifPath)
        {
            CurrentGifPath = gifPath;

            // Extract frames from the GIF using Magick.NET
            FramePaths = ExtractFramesFromGif(gifPath);
            if (FramePaths.Count > 0)
            {
                _currentFrameIndex = 0;
                AnimationImage = new BitmapImage(new Uri(FramePaths[0]));
            }
        }

        private List<string> ExtractFramesFromGif(string gifPath)
        {
            var tempFramesPath = Path.Combine(Path.GetTempPath(), "gif_frames");
            Directory.CreateDirectory(tempFramesPath);
            var framePaths = new List<string>();

            using (var collection = new MagickImageCollection(gifPath))
            {
                int index = 0;
                foreach (var frame in collection)
                {
                    string framePath = Path.Combine(tempFramesPath, $"frame_{index++}.png");
                    frame.Write(framePath);
                    framePaths.Add(framePath);
                }
            }

            return framePaths;
        }

        private string CurrentGifPath { get; set; }
        private List<string> FramePaths { get; set; } = new List<string>();
        public event EventHandler CloseRequested;
    }
}
