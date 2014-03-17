using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CamKeyboard.Core.Helpers;

namespace CamKeyboard.Core
{
    public class KeyboardInfo
    {
        public KeyboardInfo(KeyboardVertices vertices, int imageHeight, int imageWidth)
        {
            bool isLessThan0 = vertices.BottomLeft.X < 0 || vertices.BottomLeft.Y < 0 ||
                           vertices.BottomRight.X < 0 || vertices.BottomRight.Y < 0 ||
                           vertices.TopLeft.X < 0 || vertices.TopLeft.Y < 0 ||
                           vertices.TopRight.X < 0 || vertices.TopRight.Y < 0;
            var dimensions = SurfaceUtility.GetDimension(vertices.TopLeft, vertices.TopRight,
                vertices.BottomLeft, vertices.BottomRight);
            //TODO Improve
            //bool isInRange = vertices.TopRight.X > dimensions.Width ||
            //               vertices.BottomLeft.Y > dimensions.Height ||
            //               vertices.BottomRight.X > dimensions.Width ||
            //               vertices.BottomRight.Y > dimensions.Height;
            bool isValid = dimensions.Height < dimensions.Width;
            if (!(!isLessThan0 && isValid))
            {
                throw new InvalidKeyboardInfoException();
            }

            this.Vertices = vertices;
            this.Dimensions = dimensions;
        }

        public SurfaceDimension Dimensions { get; set; }

        public KeyboardVertices Vertices { get; set; }
    }

    public class InvalidKeyboardInfoException : Exception
    {
    }
}
