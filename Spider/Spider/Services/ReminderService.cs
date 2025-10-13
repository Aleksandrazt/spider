using Spider.Models;
using System.Windows.Threading;

namespace Spider.Services
{
    /// <summary>
    /// Сервис для управления напоминаниями
    /// </summary>
    public class ReminderService : IDisposable
    {
        private readonly SettingsService _settingsService;
        private DispatcherTimer? _reminderTimer;
        private ReminderSettings _settings;
        
        /// <summary>
        /// Событие, вызываемое при наступлении времени напоминания
        /// </summary>
        public event EventHandler? ReminderTriggered;

        public ReminderService()
        {
            _settingsService = new SettingsService();
            _settings = _settingsService.LoadReminderSettings();
        }

        /// <summary>
        /// Запустить таймер напоминаний
        /// </summary>
        public void Start()
        {
            if (_settings.IsEnabled)
            {
                Stop(); // Остановить существующий таймер, если он есть
                
                _reminderTimer = new DispatcherTimer();
                _reminderTimer.Interval = TimeSpan.FromMinutes(_settings.IntervalMinutes);
                _reminderTimer.Tick += OnReminderTimerTick;
                _reminderTimer.Start();

                System.Diagnostics.Debug.WriteLine($"Таймер напоминаний запущен. Интервал: {_settings.IntervalMinutes} минут");
            }
        }

        /// <summary>
        /// Остановить таймер напоминаний
        /// </summary>
        public void Stop()
        {
            if (_reminderTimer != null)
            {
                _reminderTimer.Stop();
                _reminderTimer.Tick -= OnReminderTimerTick;
                _reminderTimer = null;
                
                System.Diagnostics.Debug.WriteLine("Таймер напоминаний остановлен");
            }
        }

        /// <summary>
        /// Обновить настройки и перезапустить таймер при необходимости
        /// </summary>
        public void UpdateSettings(ReminderSettings settings)
        {
            _settings = settings;
            _settingsService.SaveReminderSettings(settings);

            Stop();
            
            if (settings.IsEnabled)
            {
                Start();
            }
        }

        /// <summary>
        /// Получить текущие настройки
        /// </summary>
        public ReminderSettings GetSettings()
        {
            return _settings;
        }

        /// <summary>
        /// Проверить, запущен ли таймер
        /// </summary>
        public bool IsRunning => _reminderTimer != null && _reminderTimer.IsEnabled;

        private void OnReminderTimerTick(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Напоминание сработало!");
            ReminderTriggered?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}