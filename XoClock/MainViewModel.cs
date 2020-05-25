using System.ComponentModel;

namespace XoClock
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        IClock _timerModel;
        string _displayTime;

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
        }

        private void TimerModel_Tick(object sender, TickEventArgs e)
        {
            DisplayTime = e.Clock.ToLongTimeString();
        }

        private void OnPropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, e);
        }
    }
}