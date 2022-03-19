﻿namespace XoClock
{
    public delegate void TickEventHandler(object sender, TickEventArgs e);

    internal interface ITimerCore
    {
        event TickEventHandler Tick;
        int PeriodInMs { get; set; }
    }
}