using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
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
            this.camKeyboard = new CamKeyboardManager();
            this.camKeyboard.NewFrameCaptured += OnNewFrameCaptured;
        }

        public ICommand Start
        {
            get
            {
                if (this.start == null)
                {
                    this.start = new AwaitableDelegateCommand(this.StartCapture);
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

        private async Task StartCapture()
        {
            await Task.Run(() => this.camKeyboard.StartCapturing());
        }

        public void OnNewFrameCaptured(object sender, OnCaptureEventHandlerArgs args)
        {
            Application.Current.Dispatcher.Invoke(
                DispatcherPriority.Send,
                (DispatcherOperationCallback)(arg =>
                {
                    this.currentFrame = args.Image;
                    OnPropertyChanged("GetImage");
                    this.currentProcessedFrame = args.ProcessedImage;
                    OnPropertyChanged("GetProcessedImage");
                    return null;
                }), null);

        }

    }
}
