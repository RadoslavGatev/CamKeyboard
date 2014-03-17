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
        public static readonly int DifferenceTreshold = 3;

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

        public static bool IsLeftUpperThan(this Point firstPoint, Point secondPoint)
        {
            int xDifference = Math.Abs(firstPoint.X - secondPoint.X);
            int yDifference = Math.Abs(firstPoint.Y - secondPoint.Y);

            bool isLeftUpperThan = (firstPoint.IsMoreUpperThan(secondPoint) && firstPoint.IsMoreLeftThan(secondPoint)) ||
                (xDifference <= DifferenceTreshold && firstPoint.IsMoreUpperThan(secondPoint)) ||
                (yDifference <= DifferenceTreshold && firstPoint.IsMoreLeftThan(secondPoint));
            return isLeftUpperThan;
        }

        public static bool IsRightUpperThan(this Point firstPoint, Point secondPoint)
        {
            int xDifference = Math.Abs(firstPoint.X - secondPoint.X);
            int yDifference = Math.Abs(firstPoint.Y - secondPoint.Y);

            bool isRightUpperThan = (firstPoint.IsMoreUpperThan(secondPoint) && firstPoint.IsMoreRightThan(secondPoint)) ||
                (xDifference <= DifferenceTreshold && firstPoint.IsMoreUpperThan(secondPoint)) ||
                (yDifference <= DifferenceTreshold && firstPoint.IsMoreRightThan(secondPoint));
            return isRightUpperThan;
        }

        public static bool IsLeftBottomThan(this Point firstPoint, Point secondPoint)
        {
            int xDifference = Math.Abs(firstPoint.X - secondPoint.X);
            int yDifference = Math.Abs(firstPoint.Y - secondPoint.Y);

            bool isLeftBottomThan = (firstPoint.IsMoreLowerThan(secondPoint) && firstPoint.IsMoreLeftThan(secondPoint)) ||
                (xDifference <= DifferenceTreshold && firstPoint.IsMoreLowerThan(secondPoint)) ||
                (yDifference <= DifferenceTreshold && firstPoint.IsMoreLeftThan(secondPoint));
            return isLeftBottomThan;
        }

        public static bool IsRightBottomThan(this Point firstPoint, Point secondPoint)
        {
            int xDifference = Math.Abs(firstPoint.X - secondPoint.X);
            int yDifference = Math.Abs(firstPoint.Y - secondPoint.Y);

            bool isLeftBottomThan = (firstPoint.IsMoreLowerThan(secondPoint) && firstPoint.IsMoreRightThan(secondPoint)) ||
                (xDifference <= DifferenceTreshold && firstPoint.IsMoreLowerThan(secondPoint)) ||
                (yDifference <= DifferenceTreshold && firstPoint.IsMoreRightThan(secondPoint));
            return isLeftBottomThan;
        }
    }
}
