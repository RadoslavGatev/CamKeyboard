using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Net.Configuration;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CamKeyboard.Core.Helpers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Point = System.Drawing.Point;

namespace CamKeyboard.Core
{
    class KeyboardImage
    {
        public readonly Byte Foreground = 255;
        public readonly Byte Background = 0;
        public readonly int PhiBins = 4 * 360;
        public readonly double LineTreshold = 0.38;
        public readonly int LineClosenessTreshold = 20;

        private Image<Bgr, Byte> frame { get; set; }
        private Image<Gray, Byte> binaryImage { get; set; }
        private static KeyboardInfo keyboardInfo;

        public static Image<Bgr, Byte> KeyboardFoundFrame { get { return KeyboardImage.keyboardFoundFrame; } }
        private static Image<Bgr, Byte> keyboardFoundFrame = null;

        public static KeyboardInfo KeyboardInfo { get { return keyboardInfo; } }
        public static object thisLock = new object();

        public KeyboardImage(Image<Bgr, Byte> frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException("frame must not be null!");
            }

            this.frame = frame.Copy();
        }

        public Image<Bgr, Byte> Analyze()
        {
            this.PreProcess();
            this.FindTheBiggestBlob();
            this.binaryImage.Erode(1);
            var lines = this.FindLines();
            var keyboardVertices = this.TryRecognizeKeyboardVertives(lines);

            try
            {
                var keyboardInfo = new KeyboardInfo(keyboardVertices, this.frame.Height, this.frame.Width);
                double proportion = 3.69;
                int currentPrice = Int32.MaxValue;
                if (KeyboardImage.keyboardInfo != null)
                {
                    currentPrice = (int)Math.Abs(KeyboardImage.keyboardInfo.Dimensions.Height * proportion -
                                                     KeyboardImage.keyboardInfo.Dimensions.Width);
                }
                int newPrice = (int)Math.Abs(keyboardInfo.Dimensions.Height * proportion -
                    keyboardInfo.Dimensions.Width);
                if (currentPrice > newPrice)
                {
                    lock (thisLock)
                    {
                        KeyboardImage.keyboardInfo = keyboardInfo;
                        KeyboardImage.keyboardFoundFrame = this.frame.Copy();
                    }
                }
            }
            catch (InvalidKeyboardInfoException)
            {
                //the current keyboard values are invalid
            }
            this.WarpPerspective();

            return this.frame;
        }

        private void WarpPerspective()
        {
            if (KeyboardImage.keyboardInfo != null)
            {
                var surfaceDimensions = SurfaceUtility.GetDimension(KeyboardImage.keyboardInfo.Vertices.TopLeft,
                    KeyboardImage.keyboardInfo.Vertices.TopRight,
                    KeyboardImage.keyboardInfo.Vertices.BottomLeft,
                    KeyboardImage.keyboardInfo.Vertices.BottomRight);
                PointF[] srcs = new PointF[4];
                srcs[0] = new PointF(KeyboardImage.keyboardInfo.Vertices.TopLeft.X,
                    KeyboardImage.keyboardInfo.Vertices.TopLeft.Y);
                srcs[1] = new PointF(KeyboardImage.keyboardInfo.Vertices.TopRight.X,
                    KeyboardImage.keyboardInfo.Vertices.TopRight.Y);
                srcs[2] = new PointF(KeyboardImage.keyboardInfo.Vertices.BottomLeft.X,
                    KeyboardImage.keyboardInfo.Vertices.BottomLeft.Y);
                srcs[3] = new PointF(KeyboardImage.keyboardInfo.Vertices.BottomRight.X,
                    KeyboardImage.keyboardInfo.Vertices.BottomRight.Y);


                PointF[] dsts = new PointF[4];
                dsts[0] = new PointF(0, 0);
                dsts[1] = new PointF((float)(surfaceDimensions.Width - 1), 0);
                dsts[2] = new PointF(0, (float)(surfaceDimensions.Height - 1));
                dsts[3] = new PointF((float)(surfaceDimensions.Width - 1), (float)(surfaceDimensions.Height - 1));

                HomographyMatrix mywarpmat = CameraCalibration.GetPerspectiveTransform(srcs, dsts);
                var newImage = this.frame.WarpPerspective(mywarpmat, (int)surfaceDimensions.Width,
                    (int)surfaceDimensions.Height,
                    INTER.CV_INTER_NN, WARP.CV_WARP_DEFAULT, new Bgr());
                //var newImage = this.binaryImage.WarpPerspective(mywarpmat, Emgu.CV.CvEnum.INTER.CV_INTER_NN, Emgu.CV.CvEnum.WARP.CV_WARP_FILL_OUTLIERS, new Gray(0));
                this.frame = newImage;
            }
        }

        private KeyboardVertices TryRecognizeKeyboardVertives(IEnumerable<LineSegment2D> lines)
        {
            var topLeft = new Point(frame.Width * 2, frame.Height * 2);
            var topRight = new Point(-frame.Width * 2, frame.Height * 2);
            var bottomLeft = new Point(frame.Width * 2, -frame.Height * 2);
            var bottomRight = new Point(-frame.Width * 2, -frame.Height * 2);
            foreach (var line in lines)
            {
                if (line.P1.IsLeftUpperThan(topLeft))
                {
                    topLeft = line.P1;
                }
                if (line.P2.IsLeftUpperThan(topLeft))
                {
                    topLeft = line.P2;
                }

                if (line.P1.IsRightUpperThan(topRight))
                {
                    topRight = line.P1;
                }
                if (line.P2.IsRightUpperThan(topRight))
                {
                    topRight = line.P2;
                }

                if (line.P1.IsLeftBottomThan(bottomLeft))
                {
                    bottomLeft = line.P1;
                }
                if (line.P2.IsLeftBottomThan(bottomLeft))
                {
                    bottomLeft = line.P2;
                }

                if (line.P1.IsRightBottomThan(bottomRight))
                {
                    bottomRight = line.P1;
                }
                if (line.P2.IsRightBottomThan(bottomRight))
                {
                    bottomRight = line.P2;
                }
            }

            var keyboardVertices = new KeyboardVertices()
            {
                BottomLeft = bottomLeft,
                BottomRight = bottomRight,
                TopLeft = topLeft,
                TopRight = topRight
            };
            return keyboardVertices;
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

        private IEnumerable<LineSegment2D> FindLines()
        {
            var lines = this.binaryImage.HoughLinesBinary(1, Math.PI / 180, 200, 3, 20);
            if (lines.Length != 0)
            {
                var mergedLines = this.MergeRelatedLines(lines[0]);

                foreach (var line in mergedLines)
                {
                    this.binaryImage.Draw(line, new Gray(128), 2);
                }
                return mergedLines;
            }
            return null;
        }

        private double AngleBetweenInDegrees(Point p1, Point p2)
        {
            double angle = Math.Abs(Math.Atan2(p1.Y - p2.Y, p1.X - p2.X)) * 180 / Math.PI;
            return angle;
        }

        private class MergedLineModel
        {
            public LineSegment2D Line { get; set; }
            public bool IsValid { get; set; }
        }
        private IEnumerable<LineSegment2D> MergeRelatedLines(LineSegment2D[] lines)
        {

            var mergedLines = lines.Select(
                x => new MergedLineModel
                {
                    Line = x,
                    IsValid = true
                }).ToList();
            if (lines.Any())
            {
                foreach (var currentLine in mergedLines)
                {
                    if (currentLine.IsValid == false)
                    {
                        continue;
                    }

                    double currentAngle = AngleBetweenInDegrees(currentLine.Line.P1, currentLine.Line.P2);
                    foreach (var comparedLine in mergedLines)
                    {
                        if (comparedLine.IsValid == false ||
                            (currentLine.Line.Equals(comparedLine.Line) && currentLine.IsValid == comparedLine.IsValid))
                        {
                            continue;
                        }

                        var comparedLineAngle = AngleBetweenInDegrees(comparedLine.Line.P1, comparedLine.Line.P2);
                        bool areLinesSimiliar = Math.Abs(comparedLine.Line.Length - currentLine.Line.Length) < 20 &&
                                          Math.Abs(comparedLineAngle - currentAngle) < 10;
                        bool areLinesCloseEnough = ((double)(comparedLine.Line.P1.X - currentLine.Line.P1.X) *
                                                    (comparedLine.Line.P1.X - currentLine.Line.P1.X) +
                                                    (comparedLine.Line.P1.Y - currentLine.Line.P1.Y) *
                                                    (comparedLine.Line.P1.Y - currentLine.Line.P1.Y) <
                                                    LineClosenessTreshold * LineClosenessTreshold) &&
                                                   ((double)(comparedLine.Line.P2.X - currentLine.Line.P2.X) *
                                                   (comparedLine.Line.P2.X - currentLine.Line.P2.X) +
                                                    (comparedLine.Line.P2.Y - currentLine.Line.P2.Y) *
                                                    (comparedLine.Line.P2.Y - currentLine.Line.P2.Y) <
                                                    LineClosenessTreshold * LineClosenessTreshold);
                        if (areLinesSimiliar && areLinesCloseEnough)
                        {
                            // Merge the two
                            var p1 = new Point((currentLine.Line.P1.X + comparedLine.Line.P1.X) / 2,
                                (currentLine.Line.P1.Y + comparedLine.Line.P1.Y) / 2);
                            var p2 = new Point((currentLine.Line.P2.X + comparedLine.Line.P2.X) / 2,
                                (currentLine.Line.P2.Y + comparedLine.Line.P2.Y) / 2);
                            currentLine.Line = new LineSegment2D(p1, p2);
                            comparedLine.IsValid = false;
                        }
                    }
                }
            }
            return mergedLines.Where(x => x.IsValid == true).Select(x => x.Line).ToList();
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
                                     new MCvScalar(0), out compThird,
                                     Emgu.CV.CvEnum.CONNECTIVITY.FOUR_CONNECTED,
                                     Emgu.CV.CvEnum.FLOODFILL_FLAG.DEFAULT, IntPtr.Zero);
                    }
                }
            }
        }
    }
}

