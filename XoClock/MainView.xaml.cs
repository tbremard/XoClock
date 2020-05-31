using System.Windows;
using System.Windows.Input;

namespace XoClock
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        bool _isMoving = false;
        Point _lastPosition;
        bool _lastTopMost = true;
        MainViewModel viewModel;

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
            viewModel = new MainViewModel(clock);
            DataContext = viewModel;
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

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            viewModel.SwitchMode();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            viewModel.KeyDown(e);
            switch (e.Key)
            {
                case Key.R:
                    SwitchResizeMode();
                    break;
                case Key.T:
                    SwitchTopMost();
                    break;
                case Key.F1:
                    this.WindowState = WindowState.Maximized;
                    this.ShowInTaskbar = false;
                    Topmost = _lastTopMost;
                    break;
                case Key.F2:
                    this.WindowState = WindowState.Normal;
                    this.ShowInTaskbar = false;
                    Topmost = _lastTopMost;
                    break;
                case Key.F3:
                    this.WindowState = WindowState.Minimized;
                    this.ShowInTaskbar = true;
                    Topmost = false;
                    break;
                case Key.Subtract:
                    OpacityDecrease();
                    break;
                case Key.Add:
                    OpacityIncrease();
                    break;
                case Key.X:
                    Application.Current.Shutdown();
                    break;
            }

        }

        private void OpacityIncrease()
        {
            if (Opacity <= 1)
                Opacity += 0.1;
        }

        private void OpacityDecrease()
        {
            if (Opacity > 0.4)
                this.Opacity -= 0.1;
        }

        private void SwitchTopMost()
        {
            Topmost = !Topmost;
            _lastTopMost = Topmost;
        }

        private void SwitchResizeMode()
        {
            if(this.ResizeMode == ResizeMode.NoResize)
            {
                ResizeMode = ResizeMode.CanResizeWithGrip;
            }
            else
            {
                ResizeMode = ResizeMode.NoResize;
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(e.Delta>0)
            {
                OpacityIncrease();
            }
            else
            {
                OpacityDecrease();
            }
        }
    }
}
