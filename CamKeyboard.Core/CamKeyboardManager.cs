using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace CamKeyboard.Core
{
    public delegate void OnCaptureEventHandler(object sender, OnCaptureEventHandlerArgs args);
    public delegate void OnFrameProcessEventHandler(object sender, OnFrameProcessEventHandlerArgs args);


    public class CamKeyboardManager : IDisposable
    {
        public event OnCaptureEventHandler NewFrameCaptured;
        public event OnFrameProcessEventHandler NewFrameProcessed;
        public event OnButtonClickedEventHandler ButtonClicked;

        private Capture camera { get; set; }
        private int frameCounter = 0;
        private ButtonsMatrix buttons;
        private ButtonClickHistory buttonClickHistory = new ButtonClickHistory(1000 / 15);

        public int FrameCounter
        {
            get { return this.frameCounter; }
            set
            {
                this.frameCounter = value > 1000 ? 0 : value;
            }
        }

        public CamKeyboardManager()
        {
            var capture = new Capture(0);
            if (capture == null)
            {
                throw new NullReferenceException("Capture must not be null");
            }
           // capture.FlipHorizontal = true;
            this.camera = capture;
            buttonClickHistory.ButtonClicked += OnButtonClicked;
        }

        public CamKeyboardManager(string videoFileName)
        {
            if (videoFileName == null)
            {
                throw new ArgumentNullException("videoFileName must not be null!");
            }

            var capture = new Capture(videoFileName);

            if (capture == null)
            {
                throw new NullReferenceException("Capture must not be null");
            }

            capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS, 30);
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, 480);
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, 720);

            this.camera = capture;
            buttonClickHistory.ButtonClicked += OnButtonClicked;
        }

        public void StartCapturing()
        {
            this.camera.ImageGrabbed += ProcessImage;
            this.camera.Start();
        }

        public void Dispose()
        {
            if (this.camera != null)
            {
                this.camera.Stop();
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

        protected virtual void OnButtonClicked(object sender, OnButtonClickedEventHandlerArgs arg)
        {
            if (ButtonClicked != null)
            {
                ButtonClicked(this, arg);
            }
        }

        private void ProcessImage(object sender, EventArgs arg)
        {
            Image<Bgr, Byte> frame = this.camera.RetrieveBgrFrame();
            var currentTime = DateTime.Now;

            var onCaptureArgs = new OnCaptureEventHandlerArgs()
            {
                Image = BitmapSourceConverter.ToBitmapSource(frame),
            };

            if (this.FrameCounter % 2 == 0 && frame != null)
            {
                var workingThread = new Thread(() => PerfomAnalysis(frame, currentTime));

                workingThread.IsBackground = true;
                workingThread.Priority = ThreadPriority.Normal;
                workingThread.Start();
            }

            this.FrameCounter++;
            OnNewFrameCaptured(this, onCaptureArgs);
            Thread.Sleep(1000 / 30);
        }

        private void PerfomAnalysis(Image<Bgr, byte> frame, DateTime currentTime)
        {
            var image = new KeyboardImage(frame);
            Image<Bgr, Byte> processedImage = image.Analyze();

            if (KeyboardImage.KeyboardFoundFrame != null)
            {
                var buttonsMatrix = new ButtonsMatrix(KeyboardImage.KeyboardInfo.Vertices.TopLeft.X,
                    KeyboardImage.KeyboardInfo.Vertices.TopLeft.Y,
                    KeyboardImage.KeyboardInfo.Dimensions.Height,
                    KeyboardImage.KeyboardInfo.Dimensions.Width);

                var fingertipDetector = new FingertipDetector(KeyboardImage.KeyboardFoundFrame, frame);
                var fingertip = fingertipDetector.Detect();
                if (!fingertip.IsEmpty)
                {
                    var fingertipPoint = new Point((int)fingertip.X, (int)fingertip.Y);
                    var currentButton = buttonsMatrix.GetClickedButton(fingertipPoint);
                    if (currentButton != null)
                    {
                        this.buttonClickHistory.AddButton(currentButton, currentTime);
                        //Debug.Print(currentButton.Label + "\nfinger: " + fingertip.X +
                        //" " + fingertip.Y + "\nButton " + currentButton.X + " " + currentButton.Y +
                        //"\nTo     " + (currentButton.X + buttonsMatrix.buttonWidth) + " " + (currentButton.Y + buttonsMatrix.buttonHeight));
                    }
                }
                else
                {
                    this.buttonClickHistory.NotifyFingerTipIsOutOfKeyboard();
                }

                var frameForDrawing = frame.Copy();
                this.DrawObjects(ref frameForDrawing);
                OnNewFrameProcessed(this, new OnFrameProcessEventHandlerArgs()
                {
                    ProcessedImage = BitmapSourceConverter.ToBitmapSource(frameForDrawing)
                });
            }
        }

        private void DrawObjects(ref Image<Bgr, Byte> frame)
        {
            if (KeyboardImage.KeyboardInfo != null)
            {
                frame.DrawPolyline(new Point[]
                {
                    KeyboardImage.KeyboardInfo.Vertices.TopLeft, KeyboardImage.KeyboardInfo.Vertices.TopRight,
                    KeyboardImage.KeyboardInfo.Vertices.BottomRight, KeyboardImage.KeyboardInfo.Vertices.BottomLeft

                }, true, new Bgr(Color.Red), 2);
            }
        }
    }
}
