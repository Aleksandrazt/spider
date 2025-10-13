using System.Windows.Input;

namespace Spider.Models
{
    /// <summary>
    /// Настройки для модуля скриншотов и видеозаписи
    /// </summary>
    public class ScreenshotSettings
    {
        /// <summary>
        /// Путь для сохранения скриншотов
        /// </summary>
        public string ScreenshotPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

        /// <summary>
        /// Путь для сохранения видео
        /// </summary>
        public string VideoPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

        /// <summary>
        /// Автоматически копировать в буфер обмена
        /// </summary>
        public bool CopyToClipboard { get; set; } = true;

        /// <summary>
        /// Сохранять в файл
        /// </summary>
        public bool SaveToFile { get; set; } = true;

        /// <summary>
        /// Частота кадров для видеозаписи (FPS)
        /// </summary>
        public int VideoFrameRate { get; set; } = 30;

        /// <summary>
        /// Качество видео (1-100)
        /// </summary>
        public int VideoQuality { get; set; } = 90;

        /// <summary>
        /// Горячая клавиша для скриншота области
        /// </summary>
        public HotkeySettings ScreenshotHotkey { get; set; } = new HotkeySettings 
        { 
            Key = Key.PrintScreen, 
            Modifiers = ModifierKeys.None 
        };

        /// <summary>
        /// Горячая клавиша для полного скриншота
        /// </summary>
        public HotkeySettings FullScreenshotHotkey { get; set; } = new HotkeySettings 
        { 
            Key = Key.PrintScreen, 
            Modifiers = ModifierKeys.Control 
        };

        /// <summary>
        /// Горячая клавиша для начала/остановки записи видео
        /// </summary>
        public HotkeySettings VideoRecordHotkey { get; set; } = new HotkeySettings 
        { 
            Key = Key.F9, 
            Modifiers = ModifierKeys.Control | ModifierKeys.Shift 
        };

        /// <summary>
        /// Качество JPEG (1-100)
        /// </summary>
        public int JpegQuality { get; set; } = 95;

        /// <summary>
        /// Формат файла скриншота
        /// </summary>
        public string ImageFormat { get; set; } = "PNG"; // PNG, JPEG, BMP
    }

    /// <summary>
    /// Настройки горячей клавиши
    /// </summary>
    public class HotkeySettings
    {
        /// <summary>
        /// Клавиша
        /// </summary>
        public Key Key { get; set; }

        /// <summary>
        /// Модификаторы (Ctrl, Alt, Shift, Win)
        /// </summary>
        public ModifierKeys Modifiers { get; set; }

        /// <summary>
        /// Включена ли горячая клавиша
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        public override string ToString()
        {
            var parts = new List<string>();
            
            if (Modifiers.HasFlag(ModifierKeys.Control))
                parts.Add("Ctrl");
            if (Modifiers.HasFlag(ModifierKeys.Alt))
                parts.Add("Alt");
            if (Modifiers.HasFlag(ModifierKeys.Shift))
                parts.Add("Shift");
            if (Modifiers.HasFlag(ModifierKeys.Windows))
                parts.Add("Win");
                
            parts.Add(Key.ToString());
            
            return string.Join(" + ", parts);
        }
    }
}