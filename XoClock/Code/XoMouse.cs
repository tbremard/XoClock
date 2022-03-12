using System.Runtime.InteropServices;
using System.Windows; // Or use whatever point class you like for the implicit cast operator

namespace XoClock
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NATIVE_POINT
    {
        public int X;
        public int Y;

        public static implicit operator Point(NATIVE_POINT point)
        {
            return new Point(point.X, point.Y);
        }
    }

    public static class XoMouse
    {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out NATIVE_POINT lpPoint);
        [DllImport("User32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        public static Point GetCursorPos()
        {
            NATIVE_POINT lpPoint;
            bool success = GetCursorPos(out lpPoint);
            return lpPoint;
        }
    }
}
