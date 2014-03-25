using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.VideoSurveillance;

namespace CamKeyboard.Core
{
    internal class FingertipDetector
    {
        private Image<Bgr, Byte> backgroundFrame;
        private Image<Bgr, Byte> substracted;

        private Image<Bgr, Byte> image { get; set; }

        public FingertipDetector(Image<Bgr, Byte> backgroundFrame, Image<Bgr, Byte> image)
        {
            this.image = image.Copy();
            this.backgroundFrame = backgroundFrame.Copy();
        }

        public PointF Detect()
        {
            this.SubstractBackground();
            var img = this.substracted.Convert<Hsv, Byte>();
            var skinContour = ExtractSkinContour(img);

            if (skinContour != null && skinContour.Length > 1)
            {
                var fingertipPoint = new PointF(skinContour[0].X, skinContour[0].Y);
                //this.image.DrawPolyline(skinContour, true, new Bgr(60, 45, 90), 2);
                //var fingertipCircle = new CircleF(fingertipPoint, 7);
                //this.image.DrawPolyline(new Point[]
                //{
                //    KeyboardImage.KeyboardInfo.Vertices.TopLeft, KeyboardImage.KeyboardInfo.Vertices.TopRight,
                //    KeyboardImage.KeyboardInfo.Vertices.BottomRight, KeyboardImage.KeyboardInfo.Vertices.BottomLeft

                //}, true, new Bgr(Color.Red), 2); 
                ////this.ApplyPCAAndDrawVectors(skinContour);
                //this.image.DrawPolyline(skinContour, true, new Bgr(Color.BlueViolet), 3);
                //image.Draw(fingertipCircle, new Bgr(Color.Blue), 2);
                return fingertipPoint;
            }

            return PointF.Empty;
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

        private Point[] ExtractSkinContour(Image<Hsv, Byte> img)
        {
            // var inRange = img.InRange(new Hsv(0, 10, 60), new Hsv(20, 150, 255));
            // var inRange = img.InRange(new Hsv(3, 19, 49), new Hsv(20, 150, 220));
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
                        // Debug.Print(contour.Area.ToString());

                        if (contour.Area > sizeOfBiggestContour)
                        {
                            sizeOfBiggestContour = contour.Area;
                            currContour = contour.ToArray();
                        }
                        contour = contour.HNext;
                    }
                    if (sizeOfBiggestContour > 35)
                    {
                        return currContour;
                    }
                }

                return null;
            }
        }

        private void ApplyPCAAndDrawVectors(Point[] currContour)
        {
            if (currContour != null)
            {
                Image<Gray, Double> dataImage = new Image<Gray, double>(2, currContour.Length);
                var data = dataImage.Data;
                for (int i = 0; i < dataImage.Rows; ++i)
                {
                    data[i, 0, 0] = currContour[i].X;
                    data[i, 1, 0] = currContour[i].Y;
                }
                dataImage.Data = data;
                int size = 2;
                Matrix<float> Mean = new Matrix<float>(1, size);
                Matrix<float> EigVals = new Matrix<float>(1, size);
                Matrix<float> EigVects = new Matrix<float>(size, size);
                Matrix<float> PCAFeatures = new Matrix<float>(size, size);
                try
                {
                    CvInvoke.cvCalcPCA(dataImage, Mean, EigVals, EigVects, Emgu.CV.CvEnum.PCA_TYPE.CV_PCA_DATA_AS_ROW);
                }
                catch (CvException)
                {
                    Debug.Print("exception");
                    return;
                }

                var positionOfObject = new PointF(Mean[0, 0], Mean[0, 1]);

                this.image.Draw(new CircleF(positionOfObject, 8), new Bgr(Color.Blue), 2);
                this.image.Draw(new CircleF(new PointF(currContour[0].X, currContour[0].Y), 8), new Bgr(Color.DarkGreen), 2);
                var endPointFirst = new PointF((float)(positionOfObject.X - (0.02 * EigVects[0, 0] * EigVals[0, 0])),
                    (float)(positionOfObject.Y -
                             (0.02 * EigVects[0, 1] * EigVals[0, 0])));

                this.image.Draw(new LineSegment2DF(positionOfObject, endPointFirst), new Bgr(Color.BlueViolet), 3);
                var endPointSecond = new PointF((float)(positionOfObject.X + (0.02 * EigVects[1, 0] * EigVals[0, 1])),
                  (float)(positionOfObject.Y +
                           (0.02 * EigVects[1, 1] * EigVals[0, 1])));

                this.image.Draw(new LineSegment2DF(positionOfObject, endPointSecond), new Bgr(Color.CadetBlue), 3);

            }
        }

    }
}
