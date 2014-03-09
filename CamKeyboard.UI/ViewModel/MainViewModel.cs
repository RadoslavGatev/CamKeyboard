using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CamKeyboard.Core;
using CamKeyboard.UI.Helpers;

namespace CamKeyboard.UI.ViewModel
{
    class MainViewModel : BaseViewModel
    {
        private ICommand start;
        private CamKeyboardManager camKeyboard;
        private ImageSource currentFrame;
        private ImageSource currentProcessedFrame;

        public MainViewModel(string displayName)
            : base(displayName)
        {  
        }

        public ICommand Start
        {
            get
            {
                if (this.start == null)
                {
                    this.start = new RelayCommand(StartCapture);
                }
                return this.start;
            }
            protected set
            {
                this.start = value;
                OnPropertyChanged("Start");
            }
        }

        public ImageSource GetImage
        {
            get
            {
                return currentFrame;
            }
        }

        public ImageSource GetProcessedImage
        {
            get
            {
                return currentProcessedFrame;
            }
        }

        public void StartCapture()
        {
            this.camKeyboard = new CamKeyboardManager();
            this.camKeyboard.NewFrameCaptured += new OnCaptureEventHandler(OnNewFrameCaptured);
            this.camKeyboard.StartCapturing();
        }

        private void OnNewFrameCaptured(object sender, OnCaptureEventHandlerArgs args)
        {
            this.currentFrame = args.Image;
            OnPropertyChanged("GetImage");
            this.currentProcessedFrame = args.ProcessedImage;
            OnPropertyChanged("GetProcessedImage");
        }

    }
}
