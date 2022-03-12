using System;

namespace XoClock
{
    public class TickEventArgs: EventArgs
    {
        public DateTime Clock { get; private set; }
        public TickEventArgs(DateTime clock)
        {
            Clock = clock;
        }
    }
}