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
        public readonly int Foreground = 255;
        public readonly int Background = 0;

        private Image<Bgr, byte> frame { get; set; }
        private Image<Gray, byte> binaryImage { get; set; }

        public KeyboardImage(Image<Bgr, byte> frame)
        {
            this.frame = frame;
        }

        public Image<Gray, byte> Analyze()
        {
            this.PreProcess();
            return this.binaryImage;
        }

        private void PreProcess()
        {
            var grayscaleImage = this.frame.Convert<Gray, byte>();
            var integralImage = new Image<Gray, int>(this.frame.Width, this.frame.Height);

            int sum = 0;
            //Calculate integral image
            for (int i = 0; i < grayscaleImage.Rows; i++)
            {
                sum = 0;
                for (int j = 0; j < grayscaleImage.Cols; j++)
                {
                    sum = sum + (int)grayscaleImage[i, j].Intensity;
                    if (i == 0)
                    {
                        integralImage[i, j] = new Gray(sum);
                    }
                    else
                    {
                        integralImage[i, j] = new Gray(integralImage[i - 1, j].Intensity + sum);
                    }
                }
            }

            this.binaryImage = new Image<Gray, byte>(grayscaleImage.Width, grayscaleImage.Height);
            Point topLeft, bottomRight;
            Size halfSize = new Size((int)(grayscaleImage.Cols * 0.125) / 2, (int)(grayscaleImage.Rows * 0.125) / 2);
            int count;

            //Perform thresholding
            for (int i = 0; i < grayscaleImage.Rows; i++)
            {
                for (int j = 0; j < grayscaleImage.Cols; j++)
                {
                    topLeft = new Point(Math.Max(0, j - halfSize.Width),
                                     Math.Max(0, i - halfSize.Height));

                    bottomRight = new Point(Math.Min(grayscaleImage.Cols - 1, j + halfSize.Width),
                                         Math.Min(grayscaleImage.Rows - 1, i + halfSize.Height));

                    count = (bottomRight.X - topLeft.X + 1) * (bottomRight.Y - topLeft.Y + 1);

                    sum = (int)integralImage[bottomRight].Intensity;
                    sum -= (int)integralImage[Math.Max(0, topLeft.Y - 1),
                                                         bottomRight.X].Intensity;

                    sum -= (int)integralImage[bottomRight.Y, Math.Max(0,
                                                         topLeft.X - 1)].Intensity;
                    sum += (int)integralImage[Math.Max(0, topLeft.Y - 1),
                                                         Math.Max(0, topLeft.X - 1)].Intensity;

                    if (grayscaleImage[i, j].Intensity * count < sum * 0.85)
                    {
                       this.binaryImage[i, j] = new Gray(Foreground);
                    }
                    else
                    {
                        this.binaryImage[i, j] = new Gray(Background);
                    }
                }
            }

        }

    }
}
