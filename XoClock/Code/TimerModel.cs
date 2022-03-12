using System;
using System.ComponentModel;
using NLog;

namespace XoClock
{
    internal class TimerModel : INotifyPropertyChanged
    {
        private static ILogger _log = LogManager.GetCurrentClassLogger();
        public event PropertyChangedEventHandler PropertyChanged;
        public ClockMode Mode { get; private set; }
        ITimerCore _timerModel;
        string _displayTime;
        string _displayDate;
        ChronoStatus _chronometerStatus;
        DateTime _startedTimestamp;
        DateTime _stoppedTimestamp;

        public string DisplayTime
        {
            get
            {
                return _displayTime;
            }
            private set
            {
                if(value != _displayTime)
                {
                    _displayTime = value;
                    OnPropertyChanged("DisplayTime");
                }
            }
        }

        public string DisplayDate 
        {
            get
            {
                return _displayDate;
            }
            private set
            {
                if (value != _displayDate)
                {
                    _displayDate = value;
                    OnPropertyChanged("DisplayDate");
                }
            }
        }

        public TimerModel(ITimerCore clock)
        {
            _timerModel = clock;
            _timerModel.Tick += TimerModel_Tick;
            Mode = ClockMode.Clock;
        }

        private void TimerModel_Tick(object sender, TickEventArgs e)
        {
            string s=null;
            if (Mode == ClockMode.Clock)
            {
                s = e.Clock.ToString("HH:mm:ss");
                DisplayDate = e.Clock.ToString("yyyy-MM-dd");
            }
            if(Mode== ClockMode.Chrono)
            {
                TimeSpan chronometerValue=TimeSpan.Zero;
                switch(_chronometerStatus)
                {
                    case ChronoStatus.NotStarted:
                        chronometerValue = TimeSpan.Zero;
                        break;
                    case ChronoStatus.Running:
                        chronometerValue = DateTime.Now - _startedTimestamp;
                        break;
                    case ChronoStatus.Stopped:
                        chronometerValue = _stoppedTimestamp - _startedTimestamp;
                        break;
                }
                s = string.Empty;
                if (chronometerValue.Hours > 0)
                    s += chronometerValue.Hours.ToString("D2") + ":";
                if (chronometerValue.Minutes > 0)
                    s += chronometerValue.Minutes.ToString("D2") + "'";
                int centiseconds = chronometerValue.Milliseconds / 10;
                s += chronometerValue.Seconds.ToString("D2") +"\"" +
                    centiseconds.ToString("D2");
            }
            DisplayTime = s;
        }

        private void OnPropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, e);
        }

        internal void SwitchMode()
        {
            if(Mode == ClockMode.Clock)
            {
                SetMode(ClockMode.Chrono);
            }
            else
            {
                SetMode(ClockMode.Clock);
            }
        }

        public void SetMode(ClockMode mode)
        {
            Mode = mode;
            switch (mode)
            {
                case ClockMode.Clock:
                    _timerModel.Period = 1000;
                    break;
                case ClockMode.Chrono:
                    _timerModel.Period = 10;
                    _chronometerStatus = ChronoStatus.NotStarted;
                    break;
                default:
                    break;
            }
        }

        public void SwitchChronometerStatus()
        {
            switch(_chronometerStatus)
            {
                case ChronoStatus.NotStarted:
                    StartChrono();
                    break;
                case ChronoStatus.Running:
                    StopChrono();
                    break;
                case ChronoStatus.Stopped:
                    ResetChrono();
                    break;
            }
        }

        public void ResetChrono()
        {
            _log.Debug("Reset chronometer...");
            _chronometerStatus = ChronoStatus.NotStarted;
        }

        public void StopChrono()
        {
            _log.Debug("Stoping chronometer...");
            _chronometerStatus = ChronoStatus.Stopped;
            _stoppedTimestamp = DateTime.Now;
        }

        public void StartChrono()
        {
            _log.Debug("Starting chronometer...");
            _chronometerStatus = ChronoStatus.Running;
            _startedTimestamp = DateTime.Now;
        }
    }
}