using System;

namespace CamKeyboard.Core
{
    [Serializable]
    public class Button
    {
        public int X { get; set; }
        public int Y { get; set; }

        public int RowSpan { get; set; }
        public int ColSpan { get; set; }
        public string Label { get; set; }

        public Button(string label)
        {
            this.Label = label;
        }
    }
}