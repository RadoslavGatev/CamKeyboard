using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;

namespace CamKeyboard.Core
{
    public class OnCaptureEventHandlerArgs
    {
        public BitmapSource Image { get; set; }
        public BitmapSource ProcessedImage { get; set; }
    }
}
