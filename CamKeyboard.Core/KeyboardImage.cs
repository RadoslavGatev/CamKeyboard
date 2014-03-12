using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace CamKeyboard.Core
{
    class KeyboardImage
    {
        public readonly Byte Foreground = 255;
        public readonly Byte Background = 0;
        public readonly int PhiBins = 4 * 360;
        public readonly double LineTreshold = 0.38;

        private Image<Bgr, Byte> frame { get; set; }
        private Image<Gray, Byte> binaryImage { get; set; }
        private Image<Gray, Int32> HoughPlane { get; set; }
        private Image<Gray, Byte> TreshedPlane { get; set; }

        public KeyboardImage(Image<Bgr, Byte> frame)
        {
            this.frame = frame;
        }

        public Image<Gray, Byte> Analyze()
        {
            this.PreProcess();
            this.CalcHoughPeaks();
            return this.binaryImage;
        }

        private void PreProcess()
        {
            var grayscaleImage = this.frame.Convert<Gray, Byte>();
            this.binaryImage = grayscaleImage;

        }

        //Find the peaks in the plane.
        //- line_threshold - threshold for the peak intensity
        private void CalcHoughPeaks()
        {
            Gray cannyThreshold = new Gray(255);
            Gray cannyThresholdLinking = new Gray(0);
            var norm = Math.Sqrt(this.frame.Cols * this.frame.Cols +
                                 this.frame.Rows * this.frame.Rows);
            var lines = this.binaryImage.HoughLines(255, 0, 2, //Distance resolution in pixel-related units
                Math.PI / 45.0, //Angle resolution measured in radians.
                38, //threshold
                10, //min Line length
               7 //gap between lines
                );
            if (lines.Length != 0)
                foreach (var line in lines[0])
                {
                    this.binaryImage.Draw(line, new Gray(200), 10);
                }
            Debug.Print(lines.Length.ToString());
        }
    }
}

