using System;
using System.Threading;

namespace XoClock
{
    internal class TimerModel : IClock
    {
        public event TickEventHandler Tick;
        private readonly Timer _timer;

        public TimerModel()
        {
            _timer = new Timer(MyTimerCallback);
            _timer.Change(0, 1000);
        }

        void MyTimerCallback(object state)
        {
            var args = new TickEventArgs(DateTime.Now);
            Tick(this, args);
        }
    }
}