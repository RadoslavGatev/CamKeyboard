using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace CamKeyboard.Core
{
    class KeyboardImage
    {
        public readonly Byte Foreground = 255;
        public readonly Byte Background = 0;

        private Image<Bgr, Byte> frame { get; set; }
        private Image<Gray, Byte> binaryImage { get; set; }

        public KeyboardImage(Image<Bgr, Byte> frame)
        {
            this.frame = frame;
        }

        public Image<Gray, Byte> Analyze()
        {
            this.PreProcess();
            return this.binaryImage;
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

    }
}
