using System.Runtime.InteropServices;
using System.Windows; // Or use whatever point class you like for the implicit cast operator

namespace XoClock
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public static implicit operator Point(POINT point)
        {
            return new Point(point.X, point.Y);
        }
    }

    public static class XoMouse
    {
        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPos()
        {
            POINT lpPoint;
            bool success = GetCursorPos(out lpPoint);
            return lpPoint;
        }

        [DllImport("User32.dll")]
        public static extern bool SetCursorPos(int X, int Y);
    }
}
