using System.Windows;
using System.Windows.Input;

namespace XoClock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        bool _isMoving = false;
        Point _lastPosition;

        public MainView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Top = 0;
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            Left = screenWidth-Width;
            var clock = new TimerModel();
            DataContext = new MainViewModel(clock);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isMoving = true;
            _lastPosition = e.GetPosition(this); 
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isMoving = false;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMoving)
            {
                Point currentPosition = e.GetPosition(this);
                Left += currentPosition.X - _lastPosition.X;
                Top += currentPosition.Y - _lastPosition.Y;
            }
        }
    }
}
