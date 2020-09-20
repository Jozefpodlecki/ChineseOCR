using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Common.WinApi
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ShDragImage
    {
        public Size sizeDragImage;
        public Point ptOffset;
        public IntPtr hbmpDragImage;
        public int crColorKey;
    }
}
