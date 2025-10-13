using Newtonsoft.Json;
using Spider.Models;
using System.IO;

namespace Spider.Services
{
    /// <summary>
    /// Сервис для работы с настройками приложения
    /// </summary>
    public class SettingsService
    {
        private readonly string _settingsFilePath;
        private readonly string _reminderSettingsFilePath;

        public SettingsService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Spider"
            );
            
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            
            _settingsFilePath = Path.Combine(appDataPath, "settings.json");
            _reminderSettingsFilePath = Path.Combine(appDataPath, "reminder_settings.json");
        }

        /// <summary>
        /// Загрузить настройки скриншотов
        /// </summary>
        public ScreenshotSettings LoadScreenshotSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonConvert.DeserializeObject<ScreenshotSettings>(json);
                    return settings ?? new ScreenshotSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки настроек: {ex.Message}");
            }

            return new ScreenshotSettings();
        }

        /// <summary>
        /// Сохранить настройки скриншотов
        /// </summary>
        public void SaveScreenshotSettings(ScreenshotSettings settings)
        {
            try
            {
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения настроек: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Загрузить настройки напоминаний
        /// </summary>
        public ReminderSettings LoadReminderSettings()
        {
            try
            {
                if (File.Exists(_reminderSettingsFilePath))
                {
                    var json = File.ReadAllText(_reminderSettingsFilePath);
                    var settings = JsonConvert.DeserializeObject<ReminderSettings>(json);
                    return settings ?? new ReminderSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки настроек напоминаний: {ex.Message}");
            }

            return new ReminderSettings();
        }

        /// <summary>
        /// Сохранить настройки напоминаний
        /// </summary>
        public void SaveReminderSettings(ReminderSettings settings)
        {
            try
            {
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(_reminderSettingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения настроек напоминаний: {ex.Message}");
                throw;
            }
        }
    }
}