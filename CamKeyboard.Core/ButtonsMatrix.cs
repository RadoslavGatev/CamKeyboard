using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CamKeyboard.Core.Properties;

namespace CamKeyboard.Core
{
    [Serializable]
    public class ButtonsMatrix
    {
        public double buttonWidth { get; set; }
        public double buttonHeight { get; set; }
        public int TopX { get; set; }
        public int TopY { get; set; }

        public Button[] Buttons { get; set; }

        public ButtonsMatrix(int topX, int topY, double keyboardHeight, double keyboardWidth)
        {
            this.buttonWidth = keyboardWidth / int.Parse(Resources.KeyboardColumns);
            this.buttonHeight = keyboardHeight / int.Parse(Resources.KeyboardRows);
            this.TopX = topX;
            this.TopY = topY;
            this.initializeButtons();
        }

        private void initializeButtons()
        {
            this.Buttons = new Button[]
            {
                new Button("1"),
                new Button("2"),
                new Button("3"),
                new Button("4"),
                new Button("5"),
                new Button("6"),
                new Button("7"),
                new Button("8"),
                new Button("9"),
                new Button("0"),
                new Button("_"),
                new Button("Backspace"),
                //
                new Button("Q"),
                new Button("W"),
                new Button("E"),
                new Button("R"),
                new Button("T"),
                new Button("Y"),
                new Button("U"),
                new Button("I"),
                new Button("O"),
                new Button("P"),
                new Button("["),
                new Button("]"),
                //
                new Button("A"),
                new Button("S"),
                new Button("D"),
                new Button("F"),
                new Button("G"),
                new Button("H"),
                new Button("J"),
                new Button("K"),
                new Button("L"),
                new Button(";"),
                new Button("|"),
                new Button("Enter"),
                //
                new Button("Z"),
                new Button("X"),
                new Button("C"),
                new Button("V"),
                new Button("B"),
                new Button("N"),
                new Button("M"),
                new Button("<"),
                new Button(">"),
                new Button("?"),
                new Button("{"),
                new Button("}"),
                //
                new Button("+"),
                new Button("-"),
                new Button("Space"),
                new Button("Space"),
                new Button("Space"),
                new Button(":"),
                new Button("'"),
                new Button(","),
                new Button("."),
                new Button("/"),
                new Button("\\"),
                new Button("\""),    
            };

            int currentX = TopX;
            int currentY = TopY;
            for (int i = 0; i < this.Buttons.Length; i++)
            {
                Buttons[i].X = currentX;
                Buttons[i].Y = currentY;

                if ((i + 1) % (int.Parse(Resources.KeyboardColumns)) != 0)
                {
                    currentX += (int)buttonWidth;
                }
                else
                {
                    currentX = TopX;
                    currentY += (int)buttonHeight;
                }
            }

        }

        public Button GetClickedButton(Point fingertipPoint)
        {
            for (int i = 0; i < this.Buttons.Length; i++)
            {
                var current = Buttons[i];
                bool isInHorizontalRange = current.X < fingertipPoint.X &&
                                           (current.X + buttonWidth) > fingertipPoint.X;
                bool isInVerticalRange = current.Y < fingertipPoint.Y &&
                                         (current.Y + buttonHeight) > fingertipPoint.Y;

                bool isInButton = isInHorizontalRange && isInVerticalRange;
                if (isInButton)
                {
                    return current;
                }
            }

            return null;
        }

    }
}