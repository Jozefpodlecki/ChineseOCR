using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Common.WinApi
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct FileGroupDescriptorW
    {
        public uint cItems;
        public FileDescriptorW[] fgd;
    }
}
