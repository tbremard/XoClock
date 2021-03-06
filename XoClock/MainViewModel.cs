﻿using System;
using System.ComponentModel;
using NLog;

namespace XoClock
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private static ILogger _log = LogManager.GetCurrentClassLogger();
        public event PropertyChangedEventHandler PropertyChanged;
        IClock _timerModel;
        string _displayTime;
        ChronometerStatus _chronometerStatus;
        public ClockMode Mode { get; private set; }
        DateTime _startedTimestamp;
        DateTime _stoppedTimestamp;

        public string DisplayTime
        {
            get
            {
                return _displayTime;
            }
            set
            {
                if(value != _displayTime)
                {
                    _displayTime = value;
                    OnPropertyChanged("DisplayTime");
                }
            }
        }

        public MainViewModel(IClock clock)
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
            }
            if(Mode== ClockMode.Chronometer)
            {
                TimeSpan chronometerValue=TimeSpan.Zero;
                switch(_chronometerStatus)
                {
                    case ChronometerStatus.NotStarted:
                        chronometerValue = TimeSpan.Zero;
                        break;
                    case ChronometerStatus.Running:
                        chronometerValue = DateTime.Now - _startedTimestamp;
                        break;
                    case ChronometerStatus.Stopped:
                        chronometerValue = _stoppedTimestamp - _startedTimestamp;
                        break;
                }
                int centiSecond = chronometerValue.Milliseconds / 10;

                if (chronometerValue.Hours > 0)
                    s += chronometerValue.Hours.ToString("D2") + ":";

                if (chronometerValue.Minutes > 0)
                    s += chronometerValue.Minutes.ToString("D2") + "'";

                s += chronometerValue.Seconds.ToString("D2") +"\"" +
                    centiSecond.ToString("D2");
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
                SetMode(ClockMode.Chronometer);
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
                case ClockMode.Chronometer:
                    _timerModel.Period = 10;
                    _chronometerStatus = ChronometerStatus.NotStarted;
                    break;
                default:
                    break;
            }
        }

        public void SwitchChronometerStatus()
        {
            switch(_chronometerStatus)
            {
                case ChronometerStatus.NotStarted:
                    StartChrono();
                    break;
                case ChronometerStatus.Running:
                    StopChrono();
                    break;
                case ChronometerStatus.Stopped:
                    ResetChrono();
                    break;
            }
        }

        public void ResetChrono()
        {
            _log.Debug("reset chronometer...");
            _chronometerStatus = ChronometerStatus.NotStarted;
        }

        public void StopChrono()
        {
            _log.Debug("stoping chronometer...");
            _chronometerStatus = ChronometerStatus.Stopped;
            _stoppedTimestamp = DateTime.Now;
        }

        public void StartChrono()
        {
            _log.Debug("starting chronometer...");
            _chronometerStatus = ChronometerStatus.Running;
            _startedTimestamp = DateTime.Now;
        }
    }
}