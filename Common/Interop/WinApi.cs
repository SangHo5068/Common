using System;
using System.Runtime.InteropServices;

namespace Common.Interop
{
    public static class WinApi
    {
        private const string User32 = "user32.dll";



        #region Constants
        public enum WindowStyles
        {
            Maximize = 0x01000000
        }
        public enum ShowWindowMode
        {
            Maximize = 3,
            Restore = 9
        }
        #endregion //Constants

        #region User32
        [DllImport(User32)]
        public static extern Boolean ShowWindow(IntPtr hWnd, Int32 nCmdShow);

        [DllImport(User32)]
        public static extern int SetForegroundWindow(int hWnd);

        [DllImport(User32, SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        #endregion //User32
    }
}
