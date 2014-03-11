using System;
using System.Collections.Generic;
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

        public Image<Gray, Int32> Analyze()
        {
            this.PreProcess();
            //Hough transform parameters
            int phi_bins = PhiBins;
            int r_bins = (int)Math.Sqrt(this.frame.Cols * this.frame.Cols + this.frame.Rows * this.frame.Rows); ;
            int line_threshold = (int)(LineTreshold * r_bins);

            this.CalcHoughPlanes(PhiBins, r_bins);
            this.CalcHoughPeaks(line_threshold);
            return this.HoughPlane;
        }

        private void PreProcess()
        {
            var grayscaleImage = this.frame.Convert<Gray, Byte>();
            var integralImage = new Image<Gray, Double>(this.frame.Width, this.frame.Height);

            int sum = 0;
            var squaredSum = new Image<Gray, Double>(this.frame.Width, this.frame.Height);
            //Calculate integral image
            grayscaleImage.Integral(out integralImage, out squaredSum);

            this.binaryImage = new Image<Gray, Byte>(grayscaleImage.Width, grayscaleImage.Height);
            Point topLeft, bottomRight;
            Size halfSize = new Size((int)(grayscaleImage.Cols * 0.125) / 2, (int)(grayscaleImage.Rows * 0.125) / 2);
            int count;

            //Perform thresholding
            var binaryImageData = this.binaryImage.Data;
            for (int row = 0; row < grayscaleImage.Rows; row++)
            {
                for (int col = 0; col < grayscaleImage.Cols; col++)
                {
                    topLeft = new Point(Math.Max(0, col - halfSize.Width),
                                     Math.Max(0, row - halfSize.Height));

                    bottomRight = new Point(Math.Min(grayscaleImage.Cols - 1, col + halfSize.Width),
                                         Math.Min(grayscaleImage.Rows - 1, row + halfSize.Height));

                    count = (bottomRight.X - topLeft.X + 1) * (bottomRight.Y - topLeft.Y + 1);

                    sum = (int)integralImage.Data[bottomRight.Y, bottomRight.X, 0];
                    sum -= (int)integralImage.Data[Math.Max(0, topLeft.Y - 1),
                                                         bottomRight.X, 0];

                    sum -= (int)integralImage.Data[bottomRight.Y, Math.Max(0,
                                                         topLeft.X - 1), 0];
                    sum += (int)integralImage.Data[Math.Max(0, topLeft.Y - 1),
                                                         Math.Max(0, topLeft.X - 1), 0];

                    if (grayscaleImage[row, col].Intensity * count < sum * 0.85)
                    {
                        binaryImageData[row, col, 0] = Foreground;
                    }
                    else
                    {
                        binaryImageData[row, col, 0] = Background;
                    }
                }
            }

            this.binaryImage.Data = binaryImageData;
        }

        //--------------- Hough transform routines -------------//
        //Calculate the hough plane.
        //- phi_bins - number of angle bins
        //- r_bins - number of distance bins
        private float phi_step;         //angle step in the Hough plane
        private float r_step;           //distance step in the Hough plane
        private void CalcHoughPlanes(int phi_bins, int r_bins)
        {
            //Allocate memory for the hough plane
            HoughPlane = new Image<Gray, Int32>(r_bins, phi_bins);

            //Find the resolution of r and phi
            float diag_len = (float)Math.Sqrt(this.frame.Cols * this.frame.Cols + this.frame.Rows * this.frame.Rows);
            r_step = diag_len / r_bins;
            phi_step = (float)(2 * Math.PI / phi_bins);

            //Variable used through-out the function
            float phi = 0;

            //Calculate trigonometric look-up tables
            List<float> sinList = new List<float>(phi_bins);
            List<float> cosList = new List<float>(phi_bins);
            for (int i = 0; i < phi_bins; i++)
            {
                sinList.Add((float)Math.Sin(phi));
                cosList.Add((float)Math.Cos(phi));
                phi += phi_step;
            }

            //For each row of the input image
            for (int row = 0; row < this.binaryImage.Rows; row++)
            {
                //For each column of the input image
                for (int col = 0; col < this.binaryImage.Cols; col++)
                {
                    //pixel belongs to a line?
                    if (this.binaryImage.Data[row, col, 0] == Foreground)
                    {
                        //for each value along the hough plane phi axis
                        for (int phi_bin = 0; phi_bin < phi_bins; phi_bin++)
                        {
                            float r = col * cosList[phi_bin] + row * sinList[phi_bin];

                            if (r > 0)
                            {
                                int r_bin = (int)(r / r_step);
                                HoughPlane.Data[r_bin, phi_bin, 0]++;
                            }
                        }
                    }
                }
            }
        }

        //Find the peaks in the plane.
        //- line_threshold - threshold for the peak intensity
        private void CalcHoughPeaks(int line_threshold)
        {
            //Threshold the Hough plane
            TreshedPlane = new Image<Gray, byte>(this.HoughPlane.Cols, this.HoughPlane.Rows);
            for (int i = 0; i < this.HoughPlane.Rows; i++)
            {
                for (int j = 0; j < this.HoughPlane.Cols; j++)
                {
                    if (this.HoughPlane.Data[i, j, 0] > line_threshold)
                    {
                        this.TreshedPlane.Data[i, j, 0] = 255;
                    }
                    else
                    {
                        this.TreshedPlane.Data[i, j, 0] = 0;
                    }
                }
            }
            //Merge peaks close to each other
            int dilation_size = 2;
            //Mat element = Emgu.CV.CvInvoke. getStructuringElement(MORPH_ELLIPSE,
            //                                    Size( 2 * dilation_size + 1, 2 * dilation_size + 1),
            //                                    Point(dilation_size, dilation_size));
            //Mat dilated_plane;
            //dilate(threshed_plane, dilated_plane, element);

            ////Find the peaks in the hough_plane
            //vector<vector<Point> > contours;
            //vector<Vec4i> hierarchy;
            //findContours(dilated_plane, contours, hierarchy, CV_RETR_LIST , CV_CHAIN_APPROX_NONE);

            ////Find the centres of the peaks
            //peak_centers.clear();
            //for(unsigned int i = 0; i < contours.size(); i++)
            //{
            //    Point p = Point(0, 0);
            //    for(unsigned int j = 0; j < contours[i].size(); j++)
            //    {
            //        p += contours[i][j];
            //    }
            //    p *= 1.0 / contours[i].size();
            //    peak_centers.push_back(p);
            //}
        }
    }
}
