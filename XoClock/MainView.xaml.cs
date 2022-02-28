using System.Configuration;
using System.Windows;
using System.Windows.Input;
using Point = System.Windows.Point;
using System.Windows.Media;
using System.Threading;
using NLog;
using System;
using System.Runtime.InteropServices;

namespace XoClock
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private static ILogger _log = LogManager.GetCurrentClassLogger();
        bool _isMoving = false;
        Point _lastPosition;
        bool _lastTopMost = true;
        MainViewModel viewModel;
        PipeServer server;

        public MainView()
        {
            InitializeComponent();
            LoadColor();
        }

        private void LoadColor()
        {
            string htmlColor = ConfigurationManager.AppSettings.Get("FontColor");
            var myColor = (Color)ColorConverter.ConvertFromString(htmlColor);
            var brush = new SolidColorBrush(myColor);
            LblTime.Foreground = brush;
            LblDate.Foreground = brush;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Top = 0;
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            Left = screenWidth - Width;
            var clock = new TimerModel();
            viewModel = new MainViewModel(clock);
            DataContext = viewModel;
            var serverThread = new Thread(StartServer);
            serverThread.IsBackground = true;
            serverThread.Start();
        }

        private void StartServer()
        {
            server = new PipeServer(viewModel);
            bool isAlive = true;
            do
            {
                if (!server.OpenPort())
                {
                    _log.Error("cannot open pipe server. Already instance running ?");
                    return;
                }
                server.HandleClient();
                server.Close();
            } while (isAlive);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isMoving = true;
//            int cursorX = (int)(Left + Width / 2);
//            int cursorY = (int)(Top + Height / 2);
//            XoMouse.SetCursorPos(cursorX, cursorY);
            // _lastPosition = e.GetPosition(this);
            _lastPosition = XoMouse.GetCursorPos();
            Left = _lastPosition.X - Width / 2;
            Top = _lastPosition.Y - Height / 2;
//            int cursorY = (int)(Top + Height / 2);
            _log.Debug("_isMoving: "+ _isMoving + " from: "+_lastPosition);
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isMoving = false;
            _log.Debug("_isMoving: " + _isMoving);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMoving)
            {
                try
                {
                    // Point currentPosition = e.GetPosition(this);
                    Point currentPosition = XoMouse.GetCursorPos();
                    _log.Debug("Moving to position: " + currentPosition);
                    Left += currentPosition.X - _lastPosition.X;
                    Top += currentPosition.Y - _lastPosition.Y;
                    _lastPosition = currentPosition;
                }
                catch(Exception ex)
                {
                    _log.Error(ex);
                }
            }
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            viewModel.SwitchMode();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    if (viewModel.Mode == ClockMode.Chronometer)
                    {
                        viewModel.SwitchChronometerStatus();
                    }
                    break;
                case Key.D:
                    SwitchDisplayDate();
                    break;
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
                case Key.Escape:
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
                    break;
                case Key.Up:
                    Top--;
                    break;
                case Key.Down:
                    Top++;
                    break;
                case Key.Left:
                    Left--;
                    break;
                case Key.Right:
                    Left++;
                    break;

            }
        }

        private void SwitchDisplayDate()
        {
            if (LblDate.IsVisible)
            {
                LblDate.Visibility = Visibility.Collapsed;
            }
            else
            {
                LblDate.Visibility = Visibility.Visible;
            }
            _log.Debug("SwitchDisplayDate: "+ LblDate.Visibility);
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

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            _log.Debug("MouseLeave!");
        }
    }
}
