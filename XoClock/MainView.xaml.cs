﻿using System.Configuration;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using NLog;
using System;
using System.Windows.Threading;

namespace XoClock
{
    public partial class MainView : Window
    {
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
        bool _highlightBorder = false;
        Brush _defaultBorder;
        DispatcherTimer _blinkBorderTimer;

        public MainView()
        {
            InitializeComponent();
        }

        private void LoadColor()
        {
            string htmlColor = ConfigurationManager.AppSettings.Get("FontColor");
            _log.Debug("Loaded color from config file: FontColor="+htmlColor);
            var myColor = (Color)ColorConverter.ConvertFromString(htmlColor);
            var brush = new SolidColorBrush(myColor);
            LblTime.Foreground = brush;
            LblDate.Foreground = brush;
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

        private void StartBlinker()
        {
            _defaultBorder = BorderBrush;
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
                var highlight = new SolidColorBrush();
                highlight.Color = Colors.Aqua;
                BorderBrush = highlight;
                _highlightBorder = false; // Auto Reset
            }
            else
            {
                BorderBrush = _defaultBorder;
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
            Top = 0;
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            Left = screenWidth - Width;
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
                }
                catch(Exception ex)
                {
                    _log.Error(ex);
                }
            }
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            model.SwitchMode();
            if (model.Mode == ClockMode.Clock)
            {
                LblDate.Visibility = Visibility.Visible;
            }
            else
            {
                LblDate.Visibility = Visibility.Collapsed;
            }
        }

        bool _isBold = false;

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            if (key == _lastKey)
            {
                return;
            }
            _lastKey = key;
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
                            Clipboard.SetText(LblTime.Content.ToString());
                            FlashBorder();
                        }
                        else
                        {
                            _copyMode = true;
                        }
                    }
                    break;
                case Key.B:
                    int boldWeight = 800;
                    int normalWeight = 400;
                    FontWeight weight;
                    _log.Debug("_isBold: "+ _isBold);
                    if (_isBold)
                    {
                        weight = FontWeight.FromOpenTypeWeight(normalWeight);
                    }
                    else
                    {
                        weight = FontWeight.FromOpenTypeWeight(boldWeight);
                    }
                    _isBold = !_isBold;
                    LblTime.FontWeight = weight;//do not change the date, only the time
                    break;
                case Key.D:
                    if (_copyMode)
                    {
                        string date = LblDate.Content.ToString();
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
                        string buffer = LblTime.Content.ToString();
                        _log.Debug("Copy to clipboard: " + buffer);
                        Clipboard.SetText(buffer);
                        FlashBorder();
                    }
                    else
                    {
                        SwitchTopMost();
                    }
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
                    Close();
                    break;
                case Key.Up:
                    if (_isShiftDown)
                    {
                        MoveToTop();
                        break;
                    }
                    if (verticalCounter>0)
                    {
                        verticalCounter = 0;
                    }
                    verticalCounter--;
                    if (verticalCounter<-5)
                    {
                        Top -= 10;
                    }
                    else
                    {
                        Top--;
                    }
                    break;
                case Key.Down:
                    if (_isShiftDown)
                    {
                        MoveToBottom();
                        break;
                    }
                    if (verticalCounter < 0)
                    {
                        verticalCounter = 0;
                    }
                    verticalCounter++;
                    if (verticalCounter > 5)
                    {
                        Top += 10;
                    }
                    else
                    {
                        Top++;
                    }
                    break;
                case Key.Left:
                    if (_isShiftDown)
                    {
                        MoveToLeft();
                        break;
                    }
                    verticalCounter = 0;
                    Left--;
                    break;
                case Key.Right:
                    if (_isShiftDown)
                    {
                        MoveToRight();
                        break;
                    }
                    verticalCounter = 0;
                    Left++;
                    break;
            }
        }

        private void MoveToLeft()
        {
            Left = 0;
        }

        private void MoveToRight()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            Left = screenWidth - Width;
        }

        private void MoveToBottom()
        {
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            Top = screenHeight - Height;
        }

        private void MoveToTop()
        {
            Top = 0;
        }

        int verticalCounter = 0;
        int horizontalCounter = 0;

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


        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            _lastKey = Key.None;
            Key key = e.Key;
            _log.Debug("KeyUp  : " + key);
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
