using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.Structure;

namespace CamKeyboard.Core.Helpers
{
    public static class SurfaceUtility
    {
        public static SurfaceDimension GetDimension(Point topLeft, Point topRight, Point bottomLeft, Point bottomRight)
        {
            double maxHeightSquare = 0;
            double maxWidthSquare = 0;

            double tempDimensionSquare = 0;
            tempDimensionSquare = (topLeft.X - topRight.X) * (topLeft.X - topRight.X) +
                (topLeft.Y - topRight.Y) * (topLeft.Y - topRight.Y);
            if (tempDimensionSquare > maxWidthSquare)
            {
                maxWidthSquare = tempDimensionSquare;
            }

            tempDimensionSquare = (bottomLeft.X - bottomRight.X) * (bottomLeft.X - bottomRight.X) +
                                  (bottomLeft.Y - bottomRight.Y) * (bottomLeft.Y - bottomRight.Y);
            if (tempDimensionSquare > maxWidthSquare)
            {
                maxWidthSquare = tempDimensionSquare;
            }

            tempDimensionSquare = (topLeft.X - bottomLeft.X) * (topLeft.X - bottomLeft.X) +
                                 (topLeft.Y - bottomLeft.Y) * (topLeft.Y - bottomLeft.Y);
            if (tempDimensionSquare > maxHeightSquare)
            {
                maxHeightSquare = tempDimensionSquare;
            }

            tempDimensionSquare = (topRight.X - bottomRight.X) * (topRight.X - bottomRight.X) +
                                (topRight.Y - bottomRight.Y) * (topRight.Y - bottomRight.Y);
            if (tempDimensionSquare > maxHeightSquare)
            {
                maxHeightSquare = tempDimensionSquare;
            }

            return new SurfaceDimension()
            {
                Height = Math.Sqrt(maxHeightSquare),
                Width = Math.Sqrt(maxWidthSquare)
            };
        }
    }
}
