using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace CamKeyboard.Core
{
    public delegate void OnCaptureEventHandler(object sender, OnCaptureEventHandlerArgs args);
    public delegate void OnFrameProcessEventHandler(object sender, OnFrameProcessEventHandlerArgs args);


    public class CamKeyboardManager : IDisposable
    {
        public event OnCaptureEventHandler NewFrameCaptured;
        public event OnFrameProcessEventHandler NewFrameProcessed;

        private Capture camera { get; set; }
        private int frameCounter = 0;

        public int FrameCounter
        {
            get { return this.frameCounter; }
            set
            {
                if (value > 1000)
                {
                    this.frameCounter = 0;
                }
                else
                {
                    this.frameCounter = value;
                }
            }
        }

        public CamKeyboardManager()
        {
            var capture = new Capture(0);
            if (capture == null)
            {
                throw new NullReferenceException("Capture must not be null");
            }

            this.camera = capture;
        }

        public void StartCapturing()
        {
            this.camera.ImageGrabbed += ProcessImage;
            this.camera.FlipHorizontal = true;
            this.camera.Start();
        }

        public void Dispose()
        {
            if (this.camera != null)
            {
                this.camera.Dispose();
            }
        }

        protected virtual void OnNewFrameCaptured(object sender, OnCaptureEventHandlerArgs arg)
        {
            if (NewFrameCaptured != null)
            {
                NewFrameCaptured(this, arg);
            }
        }

        protected virtual void OnNewFrameProcessed(object sender, OnFrameProcessEventHandlerArgs arg)
        {
            if (NewFrameProcessed != null)
            {
                NewFrameProcessed(this, arg);
            }
        }

        private void ProcessImage(object sender, EventArgs arg)
        {
            Image<Bgr, Byte> frame = this.camera.RetrieveBgrFrame();

            var onCaptureArgs = new OnCaptureEventHandlerArgs()
            {
                Image = BitmapSourceConverter.ToBitmapSource(frame),
            };

            if (this.FrameCounter % 2 == 0)
            {
                var workingThread = new Thread(() =>
                {
                    var image = new KeyboardImage(frame);
                    Image<Gray, Int32> processedImage = image.Analyze();
                    OnNewFrameProcessed(this, new OnFrameProcessEventHandlerArgs()
                    {
                        ProcessedImage = BitmapSourceConverter.ToBitmapSource(processedImage)
                    });
                });
                workingThread.IsBackground = true;
                workingThread.Priority = ThreadPriority.Normal;
                workingThread.Start();
            }
            this.FrameCounter++;
            OnNewFrameCaptured(this, onCaptureArgs);
        }




    }

}
