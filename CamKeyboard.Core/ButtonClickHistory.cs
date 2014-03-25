using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CamKeyboard.Core.Properties;

namespace CamKeyboard.Core
{
    public delegate void OnButtonClickedEventHandler(object sender, OnButtonClickedEventHandlerArgs args);
    class ButtonClickHistory
    {
        public event OnButtonClickedEventHandler ButtonClicked;

        private Object thisLock = new Object();
        private IDictionary<string, List<DateTime>> buttonHistory = null;
        private readonly int buttonClickedInMilliseconds = int.Parse(Resources.ButtonClickedTimeInMilliseconds);
        private readonly int millisecondBetweenFrames = 0;
        private string LastButtonClicked = null;
        private bool IsPointInKeyboard = true;

        public ButtonClickHistory(int millisecondBetweenFrames)
        {
            this.buttonHistory = new Dictionary<string, List<DateTime>>();
            this.millisecondBetweenFrames = millisecondBetweenFrames;
        }

        public void AddButton(Button button, DateTime timeOfFrame)
        {
            lock (thisLock)
            {

                if (buttonHistory.ContainsKey(button.Label))
                {
                    buttonHistory[button.Label].Add(timeOfFrame);
                }
                else
                {
                    buttonHistory.Add(button.Label, new List<DateTime>(new DateTime[] { timeOfFrame }));
                }
                this.checkIsClicked();
            }
        }

        public void NotifyFingerTipIsOutOfKeyboard()
        {
            LastButtonClicked = null;
           // this.buttonHistory = new Dictionary<string, List<DateTime>>();
        }

        private void checkIsClicked()
        {
            List<string> keysToNuke = new List<string>();
            foreach (var buttonClicks in buttonHistory)
            {
                buttonClicks.Value.Sort();
                var timeHistory = buttonClicks.Value;

                DateTime? startTime = null;
                for (int i = 0; i < timeHistory.Count - 1; i++)
                {
                    if (i == 0 || startTime == null)
                    {
                        startTime = timeHistory[i];
                    }
                    var left = timeHistory[i];
                    var right = timeHistory[i + 1];
                    TimeSpan span = right - left;
                    if (span.Milliseconds > this.millisecondBetweenFrames * 4)
                    {
                        startTime = null;
                        continue;
                    }

                    TimeSpan spanFromTheStart = right - startTime.Value;
                    if (spanFromTheStart.Milliseconds >= buttonClickedInMilliseconds)
                    {
                        OnButtonClicked(buttonClicks.Key);
                        //add for deletion
                        keysToNuke.Add(buttonClicks.Key);
                        break;
                    }
                }
            }
            lock (thisLock)
            {
                foreach (var key in keysToNuke)
                {
                    this.buttonHistory.Remove(key);
                }
            }
        }

        private void OnButtonClicked(string buttonLabel)
        {
            if (LastButtonClicked == null || buttonLabel != LastButtonClicked)
            {
                LastButtonClicked = buttonLabel;
                if (ButtonClicked != null)
                {
                    var args = new OnButtonClickedEventHandlerArgs()
                    {
                        ButtonLabel = buttonLabel
                    };

                    ButtonClicked(this, args);
                }
            }
        }
    }
}
