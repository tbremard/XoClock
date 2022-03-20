using System.Configuration;
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

namespace XoClock
{
    public partial class MainView : Window
    {
        private const string URI_FILE_PREFIX = "file://";
        private static ILogger _log = LogManager.GetCurrentClassLogger();
        bool _isMoving = false;
        Point _lastPosition;
        bool _lastTopMost = true;
        TimerModel model;
        PipeServer server;
        bool _isShiftDown = false;
        bool _isCtrlDown = false;
        bool _copyMode = false;
        Key _lastKey = Key.None;
        bool _highlightBorder = false; // flag used by Blink timer to flash border to ack clipboard copy
        Brush _defaultBorder;
        DispatcherTimer _blinkBorderTimer;
        double _cornerRadius;
        bool _isBold = false;
        readonly MotionGenerator _motionGenerator;

        public MainView()
        {
            InitializeComponent();
            _motionGenerator = new MotionGenerator();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadColor();
            PositionOnTopRightCorner();
            var core = new TimerCore();
            model = new TimerModel(core);
            DataContext = model;
            StartCommandServerThread();
            StartBlinker();
        }


        private void LoadColor()
        {
            _cornerRadius = MyBorder.CornerRadius.BottomLeft;
            string htmlColor = ConfigurationManager.AppSettings.Get("FontColor");
            if (!string.IsNullOrEmpty(htmlColor))
            {
                _log.Debug("Loaded color from config file: FontColor=" + htmlColor);
                SolidColorBrush brush = CreateBrush(htmlColor);
                TxtTime.Foreground = brush;
                TxtDate.Foreground = brush;
                var shadow = TxtTime.Effect as DropShadowEffect;
               // shadow.Color = CreateColor(htmlColor);
            }
            else
            {
                _log.Debug("FontColor is not set => using default");
            }
            htmlColor = ConfigurationManager.AppSettings.Get("TextDropShadowColor");
            if (!string.IsNullOrEmpty(htmlColor))
            {
                _log.Debug("Loaded color from config file: TextDropShadowColor=" + htmlColor);
                var shadow = TxtTime.Effect as DropShadowEffect;
                shadow.Color = CreateColor(htmlColor);
                shadow = TxtDate.Effect as DropShadowEffect;
                shadow.Color = CreateColor(htmlColor);
            }
            string bgImagePath = ConfigurationManager.AppSettings.Get("BgImage");
            if (!string.IsNullOrEmpty(htmlColor))
            {
                _log.Debug("Loaded bg image from config file: BgImage=" + bgImagePath);
                if (File.Exists(bgImagePath))
                {
                    string currentDirectory = Directory.GetCurrentDirectory();
                    Uri uriSource;
                    if (bgImagePath.Contains(":"))
                    {
                        uriSource = new Uri(URI_FILE_PREFIX + bgImagePath);
                    }
                    else
                    {
                        uriSource = new Uri(URI_FILE_PREFIX + Path.Combine(currentDirectory, bgImagePath));
                    }
                    _log.Debug("uriSource: "+ uriSource);
                    var bitmapImage = new BitmapImage(uriSource);
                    MyBorder.Background = new ImageBrush(bitmapImage);
                }
                else
                {
                    _log.Error("file not found: " + bgImagePath);
                }
            }
            htmlColor = ConfigurationManager.AppSettings.Get("BgColor");
            if (!string.IsNullOrEmpty(htmlColor))
            {
                _log.Debug("Loaded color from config file: BgColor=" + htmlColor);
                SolidColorBrush backgroundBrush = CreateBrush(htmlColor);
                MyBorder.Background = backgroundBrush;
            }
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
            server = new PipeServer(model);
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
            currentRadius.TopLeft = 0;
            currentRadius.TopRight = 0;
            if (Left <= 0 )
            {
                currentRadius.BottomRight = _cornerRadius;
                currentRadius.BottomLeft = 0;
            }
            else if (Left >= RightBorderOfScreen)
            {
                currentRadius.BottomRight = 0;
                currentRadius.BottomLeft = _cornerRadius;
            }
            else
            {
                currentRadius.BottomLeft = _cornerRadius;
                currentRadius.BottomRight = _cornerRadius;
            }
            MyBorder.CornerRadius = currentRadius;

        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            model.SwitchMode();
            if (model.Mode == ClockMode.Clock)
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
                    if (model.Mode == ClockMode.Chrono)
                    {
                        model.SwitchChronometerStatus();
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
                        if (model.Mode == ClockMode.Chrono)
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
                    this.WindowState = WindowState.Maximized;
                    this.ShowInTaskbar = false;
                    MyBorder.CornerRadius = new CornerRadius(0);
                    MyBorder.Padding = new Thickness(0);
                    MyBorder.Margin = new Thickness(0);
                    MyBorder.BorderThickness = new Thickness(0);
                    Topmost = _lastTopMost;
                    break;
                case Key.F2:
                case Key.Escape:
                    this.WindowState = WindowState.Normal;
                    this.ShowInTaskbar = false;
                    ResetBorder();
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
            MyBorder.CornerRadius = new CornerRadius(_cornerRadius);
            MyBorder.BorderThickness = new Thickness(3);
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
