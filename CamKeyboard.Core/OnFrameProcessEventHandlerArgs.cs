﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace CamKeyboard.Core
{
    public class OnFrameProcessEventHandlerArgs
    {
        public BitmapSource ProcessedImage { get; set; }
    }
}
