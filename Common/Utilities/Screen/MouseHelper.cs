﻿using System.Windows;

namespace Common.Utilities
{
    public static class MouseHelper
    {
        public static Point MousePosition
        {
            get
            {
                NativeMethods.POINT pt = new NativeMethods.POINT();
                NativeMethods.GetCursorPos(pt);
                return new Point(pt.x, pt.y);
            }
        }
    }
}
