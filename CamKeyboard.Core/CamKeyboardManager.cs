﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace CamKeyboard.Core
{
    public delegate void OnCaptureEventHandler(object sender, OnCaptureEventHandlerArgs args);

    public class CamKeyboardManager : IDisposable
    {
        private Capture camera { get; set; }

        public event OnCaptureEventHandler NewFrameCaptured;

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
            this.camera.ImageGrabbed += OnNewFrameCaptured;
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

        protected virtual void OnNewFrameCaptured(object sender, EventArgs arg)
        {
            Image<Bgr, Byte> frame = this.camera.RetrieveBgrFrame();

            var onCaptureArgs = new OnCaptureEventHandlerArgs()
            {
                Image = BitmapSourceConverter.ToBitmapSource(frame),
                ProcessedImage = BitmapSourceConverter.ToBitmapSource(frame)
            };

            if (NewFrameCaptured != null)
            {
                NewFrameCaptured(this, onCaptureArgs);
            }
        }

    }

}
