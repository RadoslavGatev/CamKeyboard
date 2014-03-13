using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Configuration;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
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
            this.FindTheBiggestBlob();
            this.binaryImage.Erode(1);
            this.FindLines();

            return this.binaryImage;
        }

        private void PreProcess()
        {
            var grayscaleImage = this.frame.Convert<Gray, Byte>();
            Emgu.CV.CvInvoke.cvSmooth(grayscaleImage, grayscaleImage, SMOOTH_TYPE.CV_GAUSSIAN, 7, 7, 0, 4);

            grayscaleImage = grayscaleImage.ThresholdAdaptive(new Gray(255),
                ADAPTIVE_THRESHOLD_TYPE.CV_ADAPTIVE_THRESH_MEAN_C,
                THRESH.CV_THRESH_BINARY_INV, 5, new Gray(2));

            this.binaryImage = grayscaleImage.Dilate(1);


        }

        private void FindLines()
        {
            var norm = Math.Sqrt(this.frame.Cols * this.frame.Cols +
                                 this.frame.Rows * this.frame.Rows);
            var lines = this.binaryImage.HoughLinesBinary(1, Math.PI / 180, 200, 0, 3);
            if (lines.Length != 0)
                foreach (var line in lines[0])
                {
                    this.binaryImage.Draw(line, new Gray(128), 6);
                }
        }

        private void CorrectAndDrawLine(LineSegment2D line, Gray color)
        {
            //if(line[1]!=0)
            // {
            //     float m = -1/tan(line[1]);
            //     float c = line[0]/sin(line[1]);

            //     cv::line(img, Point(0, c), Point(img.size().width, m*img.size().width+c), rgb);
            // }
            // else
            // {
            //     cv::line(img, Point(line[0], 0), Point(line[0], img.size().height), rgb);
            // }
        }

        private void FindTheBiggestBlob()
        {
            int count = 0;
            double max = -1;
            Point maxPt = new Point();

            //find the largest blob
            //make all viewed blobs with Gray(64)
            for (int y = 0; y < this.binaryImage.Rows; y++)
            {
                for (int x = 0; x < this.binaryImage.Cols; x++)
                {
                    if (this.binaryImage.Data[y, x, 0] >= 128)
                    {
                        var comp = new MCvConnectedComp();
                        CvInvoke.cvFloodFill(this.binaryImage.Ptr, new Point(x, y), new MCvScalar(64), new MCvScalar(0),
                        new MCvScalar(0), out comp,
                        Emgu.CV.CvEnum.CONNECTIVITY.FOUR_CONNECTED,
                        Emgu.CV.CvEnum.FLOODFILL_FLAG.DEFAULT, IntPtr.Zero);

                        if (comp.area > max)
                        {
                            maxPt = new Point(x, y);
                            max = comp.area;
                        }
                    }
                }
            }

            //make the largest blob white
            var compSecond = new MCvConnectedComp();
            CvInvoke.cvFloodFill(this.binaryImage.Ptr, maxPt, new MCvScalar(255), new MCvScalar(0),
                         new MCvScalar(0), out compSecond,
                         Emgu.CV.CvEnum.CONNECTIVITY.FOUR_CONNECTED,
                         Emgu.CV.CvEnum.FLOODFILL_FLAG.DEFAULT, IntPtr.Zero);

            //make all other blobs black
            for (int y = 0; y < this.binaryImage.Rows; y++)
            {
                for (int x = 0; x < this.binaryImage.Cols; x++)
                {
                    if (this.binaryImage.Data[y, x, 0] == 64 && x != maxPt.X && y != maxPt.Y)
                    {
                        var compThird = new MCvConnectedComp();
                        CvInvoke.cvFloodFill(this.binaryImage.Ptr, new Point(x, y), new MCvScalar(0), new MCvScalar(0),
                                     new MCvScalar(0), out compSecond,
                                     Emgu.CV.CvEnum.CONNECTIVITY.FOUR_CONNECTED,
                                     Emgu.CV.CvEnum.FLOODFILL_FLAG.DEFAULT, IntPtr.Zero);
                    }
                }
            }
        }
    }
}

