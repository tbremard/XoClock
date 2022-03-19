using System;
using System.Threading;

namespace XoClock
{
    public class TimerCore : ITimerCore
    {
        public event TickEventHandler Tick;
        private readonly Timer _timer;
        private int _periodInMs;

        public int PeriodInMs
        {
            get
            {
                return _periodInMs;
            }
            set
            {
                _periodInMs = value;
                _timer.Change(0, value);
            }
        }

        public TimerCore()
        {
            _timer = new Timer(MyTimerCallback);
            PeriodInMs = 1000;
        }

        void MyTimerCallback(object state)
        {
            var args = new TickEventArgs(DateTime.Now);
            Tick?.Invoke(this, args);
        }
    }
}