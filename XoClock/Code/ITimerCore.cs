using System;

namespace XoClock
{
    internal interface ITimerCore
    {
        event EventHandler<TickEventArgs> Tick;
        int PeriodInMs { get; set; }
    }
}