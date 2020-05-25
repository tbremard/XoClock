using System.ComponentModel;

namespace XoClock
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        TimerModel timerModel;
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

        public MainViewModel()
        {
            _displayTime = "NA";
            timerModel = new TimerModel();
            timerModel.Tick += TimerModel_Tick;
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