﻿using System;
using System.ComponentModel;
using System.Windows.Input;

namespace XoClock
{
    public enum ChronometerStatus
    {
        NotStarted,
        Running,
        Stopped
    }
    internal class MainViewModel : INotifyPropertyChanged
    {
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
                s = chronometerValue.Hours.ToString("D2")   +":" + 
                    chronometerValue.Minutes.ToString("D2") +":" +
                    chronometerValue.Seconds.ToString("D2") +"." +
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
                Mode = ClockMode.Chronometer;
                _timerModel.Period = 10;
                _chronometerStatus = ChronometerStatus.NotStarted;
            }
            else
            {
                Mode = ClockMode.Clock;
                _timerModel.Period = 1000;
            }
        }

        internal void KeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    if (Mode == ClockMode.Chronometer)
                    {
                        SwitchChronometerStatus();
                    }
                    break;

            }
        }

        private void SwitchChronometerStatus()
        {
            switch(_chronometerStatus)
            {
                case ChronometerStatus.NotStarted:
                    _chronometerStatus = ChronometerStatus.Running;
                    _startedTimestamp = DateTime.Now;
                    break;
                case ChronometerStatus.Running:
                    _chronometerStatus = ChronometerStatus.Stopped;
                    _stoppedTimestamp = DateTime.Now;
                    break;
                case ChronometerStatus.Stopped:
                    _chronometerStatus = ChronometerStatus.NotStarted;
                    break;
            }
        }
    }
}