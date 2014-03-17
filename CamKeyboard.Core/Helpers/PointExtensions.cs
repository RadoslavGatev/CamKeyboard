using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamKeyboard.Core.Helpers
{
    public static class PointExtensions
    {
        public static bool IsMoreLeftThan(this Point firstPoint, Point secondPoint)
        {
            bool isLeftThan = (firstPoint.X < secondPoint.X);
            return isLeftThan;
        }

        public static bool IsMoreRightThan(this Point firstPoint, Point secondPoint)
        {
            bool isMoreRightThan = (firstPoint.X > secondPoint.X);
            return isMoreRightThan;
        }

        public static bool IsMoreUpperThan(this Point firstPoint, Point secondPoint)
        {
            bool isMoreUpperThan = (firstPoint.Y < secondPoint.Y);
            return isMoreUpperThan;
        }

        public static bool IsMoreLowerThan(this Point firstPoint, Point secondPoint)
        {
            bool isMoreLowerThan = (firstPoint.Y > secondPoint.Y);
            return isMoreLowerThan;
        }
    }
}
