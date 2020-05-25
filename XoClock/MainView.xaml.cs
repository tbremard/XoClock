using System.Windows;

namespace XoClock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
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
    }
}
