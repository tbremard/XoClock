using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using NLog;
using System;
using System.Windows.Threading;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media.Effects;
using InterProcess;

namespace XoClock
{
    public partial class MainView : Window
    {
        private const string URI_FILE_PREFIX = "file://";
        private static ILogger _log = LogManager.GetCurrentClassLogger();
        bool _isMoving = false;
        Point _lastPosition;
        bool _lastTopMost = true;
        TimerModel _model;
        CommandDispatcher _server;
        bool _isShiftDown = false;
        bool _isCtrlDown = false;
        bool _copyMode = false;
        Key _lastKey = Key.None;
        bool _highlightBorder = false; // flag used by Blink timer to flash border to ack clipboard copy
        Brush _defaultBorder;
        DispatcherTimer _blinkBorderTimer;
        bool _isBold = false;
        readonly MotionGenerator _motionGenerator;
        StyleConfig _style;

        public MainView()
        {
            InitializeComponent();
            _motionGenerator = new MotionGenerator();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStyle();
            PositionOnTopRightCorner();
            var core = new TimerCore();
            _model = new TimerModel(core);
            DataContext = _model;
            StartCommandServerThread();
            StartBlinker();
        }

        private void LoadStyle()
        {
            _style = StyleConfig.Load();
            if (!string.IsNullOrEmpty(_style.FontColor))
            {
                _log.Debug("Loaded color from config file: FontColor=" + _style.FontColor);
                SolidColorBrush brush = CreateBrush(_style.FontColor);
                TxtTime.Foreground = brush;
                TxtDate.Foreground = brush;
            }
            else
            {
                _log.Debug("FontColor is not set => using default");
            }
            if (!string.IsNullOrEmpty(_style.TextDropShadowColor))
            {
                _log.Debug("Loaded color from config file: TextDropShadowColor=" + _style.TextDropShadowColor);
                var shadow = TxtTime.Effect as DropShadowEffect;
                shadow.Color = CreateColor(_style.TextDropShadowColor);
                shadow = TxtDate.Effect as DropShadowEffect;
                shadow.Color = CreateColor(_style.TextDropShadowColor);
            }
            if (!string.IsNullOrEmpty(_style.BackgroundImage))
            {
                _log.Debug("Loaded bg image from config file: BgImage=" + _style.BackgroundImage);
                if (File.Exists(_style.BackgroundImage))
                {
                    string currentDirectory = Directory.GetCurrentDirectory();
                    Uri uriSource;
                    if (_style.BackgroundImage.Contains(":"))
                    {
                        uriSource = new Uri(URI_FILE_PREFIX + _style.BackgroundImage);
                    }
                    else
                    {
                        uriSource = new Uri(URI_FILE_PREFIX + Path.Combine(currentDirectory, _style.BackgroundImage));
                    }
                    _log.Debug("uriSource: "+ uriSource);
                    var bitmapImage = new BitmapImage(uriSource);
                    MyBorder.Background = new ImageBrush(bitmapImage);
                }
                else
                {
                    _log.Error("file not found: " + _style.BackgroundImage);
                }
            }
            if (!string.IsNullOrEmpty(_style.BackgroundColor))
            {
                _log.Debug("Loaded color from config file: BgColor=" + _style.BackgroundColor);
                SolidColorBrush backgroundBrush = CreateBrush(_style.BackgroundColor);
                MyBorder.Background = backgroundBrush;
            }
            MyBorder.Background.Opacity = _style.BackgroundOpacity;
            MyBorder.CornerRadius = new CornerRadius(_style.TopCornerRadius, _style.TopCornerRadius, _style.BottomCornerRadius, _style.BottomCornerRadius);
            MyBorder.BorderThickness = new Thickness(_style.BorderThickness);
            MyBorder.BorderBrush = CreateBrush(_style.BorderColor);
        }

        private SolidColorBrush CreateBrush(string htmlColor)
        {
            Color color = CreateColor(htmlColor);
            var brush = new SolidColorBrush(color);
            return brush;
        }

        private static Color CreateColor(string htmlColor)
        {
            var ret = (Color)ColorConverter.ConvertFromString(htmlColor);
            return ret;
        }

        private void StartBlinker()
        {
            _defaultBorder = MyBorder.BorderBrush;
            _blinkBorderTimer = new DispatcherTimer();
            _blinkBorderTimer.Tick += BlinkBorderTimer_Tick;
            _blinkBorderTimer.Interval = TimeSpan.FromMilliseconds(300);
            _blinkBorderTimer.Start();
        }

        private void FlashBorder()
        {
            _log.Debug("FlashBorder()");
            _highlightBorder = true;
        }

        private void BlinkBorderTimer_Tick(object sender, EventArgs e)
        {
            if (_highlightBorder)
            {
                //var highlight = new SolidColorBrush();
                //highlight.Color = Colors.Aqua;
                MyBorder.BorderBrush = TxtTime.Foreground;
                _highlightBorder = false; // Auto Reset
            }
            else
            {
                MyBorder.BorderBrush = _defaultBorder;
            }
        }

        private void StartCommandServerThread()
        {
            var serverThread = new Thread(PipeServerEntryPoint);
            serverThread.IsBackground = true;
            serverThread.Start();
        }

        private void PositionOnTopRightCorner()
        {
            MoveToTop();
            MoveToRight();
            MoveToRight();
            UpdateCorners();
        }

        private void PipeServerEntryPoint()
        {
            _server = new CommandDispatcher();
            _server.CommandReceived += _server_CommandReceived;
            bool isAlive = true;
            do
            {
                if (!_server.Listen())
                {
                    _log.Error("cannot open pipe server. Already instance running ?");
                    return;
                }
                _server.HandleClient();
                _server.Close();
            } while (isAlive);
        }

        private void _server_CommandReceived(object sender, CommandReceivedEventArgs e)
        {
            DispatchToModel(e.Command);
        }

        private void DispatchToModel(string command)
        {
            if (command == XoClockCommand.START_CHRONO.ToString())
            {
                _model.StartChrono();
            }
            if (command == XoClockCommand.STOP_CHRONO.ToString())
            {
                _model.StopChrono();
            }
            if (command == XoClockCommand.RESET_CHRONO.ToString())
            {
                _model.ResetChrono();
            }
            if (command == XoClockCommand.MODE_CHRONO.ToString())
            {
                _model.SetMode(ClockMode.Chrono);
            }
            if (command == XoClockCommand.MODE_CLOCK.ToString())
            {
                _model.SetMode(ClockMode.Clock);
            }
            if (command == XoClockCommand.KILL.ToString())
            {
                Application.Current.Dispatcher.InvokeShutdown();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isMoving = true;
            _lastPosition = XoMouse.GetCursorPos();
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
                    Point currentPosition = XoMouse.GetCursorPos();
                    _log.Debug("Moving to position: " + currentPosition);
                    Left += currentPosition.X - _lastPosition.X;
                    Top += currentPosition.Y - _lastPosition.Y;
                    _lastPosition = currentPosition;
                    UpdateCorners();
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
            }
        }

        private void UpdateCorners()
        {
            var currentRadius = MyBorder.CornerRadius;
            if (Top<=0)
            {
                currentRadius.TopLeft = 0;
                currentRadius.TopRight = 0;
            }
            else
            {
                currentRadius.TopLeft = _style.TopCornerRadius;
                currentRadius.TopRight = _style.TopCornerRadius; 
            }
            if (Left <= 0 )
            {
                currentRadius.BottomRight = _style.BottomCornerRadius;
                currentRadius.BottomLeft = 0;
                currentRadius.TopLeft = 0;
            }
            else if (Left >= RightBorderOfScreen)
            {
                currentRadius.BottomRight = 0;
                currentRadius.BottomLeft = _style.BottomCornerRadius;
            }
            else
            {
                currentRadius.BottomLeft = _style.BottomCornerRadius;
                currentRadius.BottomRight = _style.BottomCornerRadius;
            }
            MyBorder.CornerRadius = currentRadius;
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _model.SwitchMode();
            if (_model.Mode == ClockMode.Clock)
            {
                TxtDate.Visibility = Visibility.Visible;
            }
            else
            {
                TxtDate.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            double offset;
            Key key = e.Key;
            if (!IsArrow(key))
            {
                if (key == _lastKey)
                {
                    return;
                }
                _lastKey = key;
            }
            _log.Debug("KeyDown: " + key);
            switch (key)
            {
                case Key.Space:
                    if (_model.Mode == ClockMode.Chrono)
                    {
                        _model.SwitchChronometerStatus();
                    }
                    break;
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    _isCtrlDown = true;
                    break;
                case Key.LeftShift:
                case Key.RightShift:
                    _isShiftDown = true;
                    break;
                case Key.C:
                    //if (Keyboard.Modifiers == ModifierKeys.Control) < could also use this
                    if (_isCtrlDown)
                    {
                        if (_model.Mode == ClockMode.Chrono)
                        {
                            Clipboard.SetText(TxtTime.Text.ToString());
                            FlashBorder();
                        }
                        else
                        {
                            _copyMode = true;
                        }
                    }
                    break;
                case Key.B:
                    const int boldWeight = 800;
                    const int normalWeight = 400;
                    FontWeight weight;
                    if (_isBold)
                    {
                        weight = FontWeight.FromOpenTypeWeight(normalWeight);
                    }
                    else
                    {
                        weight = FontWeight.FromOpenTypeWeight(boldWeight);
                    }
                    _isBold = !_isBold;
                    _log.Debug("_isBold: " + _isBold);
                    TxtTime.FontWeight = weight;//do not change the date, only the time
                    break;
                case Key.D:
                    if (_copyMode)
                    {
                        string date = TxtDate.Text.ToString();
                        _log.Debug("Copy to clipboard: "+date);
                        Clipboard.SetText(date);
                        FlashBorder();
                    }
                    else
                    {
                        SwitchDisplayDate();
                    }
                    break;
                case Key.R:
                    SwitchResizeMode();
                    break;
                case Key.T:
                    if (_isShiftDown)
                    {
                        MoveToTop();
                    }
                    else if (_copyMode)
                    {
                        string buffer = TxtTime.Text.ToString();
                        _log.Debug("Copy to clipboard: " + buffer);
                        Clipboard.SetText(buffer);
                        FlashBorder();
                    }
                    else
                    {
                        SwitchTopMost();
                        FlashBorder();
                    }
                    break;
                case Key.F1:
                    WindowState = WindowState.Maximized;
                    ShowInTaskbar = false;
                    MyBorder.CornerRadius = new CornerRadius(0);
                    MyBorder.Padding = new Thickness(0);
                    MyBorder.Margin = new Thickness(0);
                    MyBorder.BorderThickness = new Thickness(0);
                    Topmost = _lastTopMost;
                    break;
                case Key.F2:
                case Key.Escape:
                    WindowState = WindowState.Normal;
                    ShowInTaskbar = false;
                    ResetBorder();
                    Topmost = _lastTopMost;
                    break;
                case Key.F3:
                    WindowState = WindowState.Minimized;
                    ShowInTaskbar = true;
                    Topmost = false;
                    break;
                case Key.F5:
                    LoadStyle();
                    break;
                case Key.Subtract:
                    OpacityDecrease();
                    break;
                case Key.Add:
                    OpacityIncrease();
                    break;
                case Key.X:
                    Close();
                    break;
                case Key.Up:
                    if (_isShiftDown)
                    {
                        MoveToTop();
                        break;
                    }
                    offset = _motionGenerator.Continue();
                    Top -= offset;
                    if (Top<0)
                    {
                        Top = 0;
                    }
                    break;
                case Key.Down:
                    if (_isShiftDown)
                    {
                        MoveToBottom();
                        break;
                    }
                    offset = _motionGenerator.Continue();
                    Top += offset;
                    break;
                case Key.Left:
                    if (_isShiftDown)
                    {
                        MoveToLeft();
                        break;
                    }
                    offset = _motionGenerator.Continue();
                    Left -= offset;
                    if (Left < 0)
                    {
                        Left = 0;
                    }
                    break;
                case Key.Right:
                    if (_isShiftDown)
                    {
                        MoveToRight();
                        break;
                    }
                    offset = _motionGenerator.Continue();
                    Left += offset;
                    break;
            }
            UpdateCorners();
        }

        private bool IsArrow(Key key)
        {
            if (key == Key.Left)
            {
                return true;
            }
            if (key == Key.Right)
            {
                return true;
            }
            if (key == Key.Up)
            {
                return true;
            }
            if (key == Key.Down)
            {
                return true;
            }
            return false;
        }

        private void ResetBorder()
        {
            MyBorder.CornerRadius = new CornerRadius(_style.BottomCornerRadius);
            MyBorder.BorderThickness = new Thickness(_style.BorderThickness);
        }

        public double HorizontalCenter
        {
            get { return SystemParameters.PrimaryScreenWidth / 2 - Width / 2; }
        }
               
        private void MoveToLeft()
        {
            _log.Debug("MoveToLeft");
            if (Left <= HorizontalCenter)
            {
                Left = 0;
            }
            else
            {
                Left = HorizontalCenter;
            }
        }

        private void MoveToRight()
        {
            _log.Debug("MoveToRight");
            if (Left < HorizontalCenter)
            {
                Left = HorizontalCenter;
            }
            else
            {
                Left = RightBorderOfScreen;
            }
        }

        public double RightBorderOfScreen 
        { 
            get
            {
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                var effect = MyBorder.Effect as DropShadowEffect;
                double offset = effect.BlurRadius;// + MyBorder.CornerRadius.TopRight;
                double ret = screenWidth - Width + offset;
                return ret;
            }
        }

        private void MoveToBottom()
        {
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            Top = screenHeight - Height;
        }

        private void MoveToTop()
        {
            Top = 0;
            var currentRadius = MyBorder.CornerRadius;
            currentRadius.TopLeft = 0;
            currentRadius.TopRight = 0;
            MyBorder.CornerRadius = currentRadius;
        }

        private void SwitchDisplayDate()
        {
            if (TxtDate.IsVisible)
            {
                TxtDate.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtDate.Visibility = Visibility.Visible;
            }
            _log.Debug("SwitchDisplayDate: "+ TxtDate.Visibility);
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
            _log.Debug("Topmost: "+ Topmost);
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

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            _lastKey = Key.None;
            Key key = e.Key;
            _log.Debug("KeyUp  : " + key);
            _motionGenerator.Stop();
            switch (key)
            {
                case Key.LeftShift:
                case Key.RightShift:
                    _isShiftDown = false;
                    break;
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    _isCtrlDown = false;
                    _copyMode = false;
                    break;
            }
        }
    }
}
