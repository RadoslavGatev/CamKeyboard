using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.VideoSurveillance;

namespace CamKeyboard.Core
{
    class FingertipDetector
    {
        private Image<Bgr, Byte> backgroundFrame;
        private Image<Bgr, Byte> substracted;

        private Image<Bgr, Byte> image { get; set; }
        public FingertipDetector(Image<Bgr, Byte> backgroundFrame, Image<Bgr, Byte> image)
        {
            this.image = image.Copy();
            this.backgroundFrame = backgroundFrame.Copy();
        }

        public Image<Bgr, Byte> Detect()
        {
            this.SubstractBackground();
            var img = this.substracted.Convert<Hsv, Byte>();
            Point[] currContour = ExtractSkinContours(img);

            if (currContour != null)
            {
                this.image.DrawPolyline(currContour, true, new Bgr(60, 45, 90), 2);
            }

            return this.image;
        }

        private void SubstractBackground()
        {
            var diff = this.backgroundFrame.Convert<Gray, Byte>();

            var img = image.Convert<Gray, Byte>();
            var background = backgroundFrame.Convert<Gray, Byte>();
            CvInvoke.cvAbsDiff(img.Ptr, background.Ptr, diff.Ptr);
            diff.ThresholdAdaptive(new Gray(200),
                 ADAPTIVE_THRESHOLD_TYPE.CV_ADAPTIVE_THRESH_MEAN_C,
                 THRESH.CV_THRESH_BINARY, 5, new Gray(2));
            diff.Dilate(2);
            var substractedImage = this.image.Copy(diff);

            this.substracted = substractedImage;
        }

        private Point[] ExtractSkinContours(Image<Hsv, Byte> img)
        {
            // var inRange = img.InRange(new Hsv(0, 10, 60), new Hsv(20, 150, 255));
            var inRange = img.InRange(new Hsv(3, 19, 49), new Hsv(20, 150, 220));

            using (var mem = new MemStorage())
            {
                Contour<Point> contours = inRange.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                    RETR_TYPE.CV_RETR_TREE, mem);

                if (contours != null)
                {
                    double sizeOfBiggestContour = 0;
                    var contour = contours;
                    var currContour = contour.ToArray();
                    while (contour != null)
                    {
                        if (contour.Area > sizeOfBiggestContour)
                        {
                            sizeOfBiggestContour = contour.Area;
                            currContour = contour.ToArray();
                        }

                        contour = contour.HNext;
                    }

                    return currContour;
                }

                return null;

            }
        }

        private void ExtractContourAndHull(Image<Gray, byte> skin)
        {

            using (MemStorage storage = new MemStorage())
            {
                Contour<Point> contours = skin.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_NONE,
                    Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST, storage);
                Contour<Point> biggestContour = null;

                Double Result1 = 0;
                Double Result2 = 0;

                while (contours != null)
                {
                    Result1 = contours.Area;
                    if (Result1 > Result2)
                    {
                        Result2 = Result1;
                        biggestContour = contours;
                    }
                    contours = contours.HNext;
                }

                if (biggestContour != null)
                {
                    this.image.DrawPolyline(biggestContour.ToArray(), true, new Bgr(Color.LimeGreen), 2);
                }
            }
        }
    }
}
