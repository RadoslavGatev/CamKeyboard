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
        private ICommand startCameraCommand;
        private ICommand loadVideoFromFileCommand;
        private CamKeyboardManager camKeyboard;
        private ImageSource currentFrame;
        private ImageSource currentProcessedFrame;

        public MainViewModel(string displayName)
            : base(displayName)
        {
        }

        public ICommand StartCameraCommand
        {
            get
            {
                if (this.startCameraCommand == null)
                {
                    this.startCameraCommand = new AwaitableDelegateCommand(this.StartCamera);
                }
                return this.startCameraCommand;
            }
            protected set
            {
                this.startCameraCommand = value;
                OnPropertyChanged("StartCameraCommand");
            }
        }

        public ICommand LoadVideoFromFileCommand
        {
            get
            {
                if (this.loadVideoFromFileCommand == null)
                {
                    this.loadVideoFromFileCommand = new AwaitableDelegateCommand(this.LoadVideoFromFile);
                }
                return this.loadVideoFromFileCommand;
            }
            protected set
            {
                this.loadVideoFromFileCommand = value;
                OnPropertyChanged("LoadVideoFromFileCommand");
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

        private async Task StartCamera()
        {
            if (this.camKeyboard != null)
            {
                this.camKeyboard.Dispose();
            }
            this.camKeyboard = new CamKeyboardManager();
            this.camKeyboard.NewFrameCaptured += OnNewFrameCaptured;
            this.camKeyboard.NewFrameProcessed += OnFrameProcessed;
            await Task.Run(() => this.camKeyboard.StartCapturing());
        }

        private async Task LoadVideoFromFile()
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Movie"; // Default file name
            dlg.DefaultExt = ".avi"; // Default file extension
            dlg.Filter = "Video files (.avi)|*.avi"; // Filter files by extension 

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            string filename = null;
            if (result == true)
            {
                // Open document 
                filename = dlg.FileName;
            }

            if (this.camKeyboard != null)
            {
                this.camKeyboard.Dispose();
            }
            this.camKeyboard = new CamKeyboardManager(filename);

            this.camKeyboard.NewFrameCaptured += OnNewFrameCaptured;
            this.camKeyboard.NewFrameProcessed += OnFrameProcessed;
            await Task.Run(() => this.camKeyboard.StartCapturing());
        }

        public void OnNewFrameCaptured(object sender, OnCaptureEventHandlerArgs args)
        {
            this.currentFrame = args.Image;
            OnPropertyChanged("GetImage");
        }

        public void OnFrameProcessed(object sender, OnFrameProcessEventHandlerArgs args)
        {
            this.currentProcessedFrame = args.ProcessedImage;
            OnPropertyChanged("GetProcessedImage");
        }

    }
}
