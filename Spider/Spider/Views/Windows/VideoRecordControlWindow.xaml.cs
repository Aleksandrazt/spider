using System.Windows;
using System.Windows.Threading;

namespace Spider.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для VideoRecordControlWindow.xaml
    /// Окно с элементами управления записью видео
    /// </summary>
    public partial class VideoRecordControlWindow : Window
    {
        private DispatcherTimer? _timer;
        private DateTime _startTime;
        private TimeSpan _pausedTime;
        private bool _isPaused;

        public event EventHandler? PauseResumeClicked;
        public event EventHandler? StopClicked;

        public bool IsPaused => _isPaused;

        public VideoRecordControlWindow()
        {
            InitializeComponent();
            
            _startTime = DateTime.Now;
            _pausedTime = TimeSpan.Zero;
            _isPaused = false;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Позиционируем окно в центре вверху экрана
            Left = (SystemParameters.PrimaryScreenWidth - ActualWidth) / 2;
            Top = 20;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (!_isPaused)
            {
                var elapsed = DateTime.Now - _startTime - _pausedTime;
                RecordingTimeText.Text = elapsed.ToString(@"mm\:ss");
            }
        }

        private void PauseResumeButton_Click(object sender, RoutedEventArgs e)
        {
            _isPaused = !_isPaused;
            
            if (_isPaused)
            {
                PauseResumeButton.Content = "▶️ Продолжить";
                RecordingIndicator.Opacity = 0.2;
                _pausedTime = DateTime.Now - _startTime - _pausedTime;
            }
            else
            {
                PauseResumeButton.Content = "⏸️ Пауза";
                RecordingIndicator.Opacity = 1.0;
                _startTime = DateTime.Now - _pausedTime;
            }

            PauseResumeClicked?.Invoke(this, EventArgs.Empty);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _timer?.Stop();
            StopClicked?.Invoke(this, EventArgs.Empty);
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer?.Stop();
            base.OnClosed(e);
        }
    }
}


