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
    }
}

