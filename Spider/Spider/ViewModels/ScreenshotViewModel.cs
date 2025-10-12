using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Spider.Helpers;
using Spider.Models;
using Spider.Services;
using Spider.Views.Dialogs;
using Spider.Views.Windows;
using System.Diagnostics;

namespace Spider.ViewModels
{
    /// <summary>
    /// ViewModel для вкладки "Скриншоты"
    /// </summary>
    public class ScreenshotViewModel : INotifyPropertyChanged
    {
        private readonly SettingsService _settingsService;
        private ScreenshotSettings _settings;
        private HotkeyManager? _hotkeyManager;
        private Window? _mainWindow;
        
        private string _statusMessage = "Готов к работе";
        private bool _isRecording;

        #region Свойства

        public ScreenshotSettings Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsRecording
        {
            get => _isRecording;
            set
            {
                if (SetProperty(ref _isRecording, value))
                {
                    OnPropertyChanged(nameof(RecordButtonText));
                }
            }
        }

        public string RecordButtonText => IsRecording ? "⏹️ Остановить запись" : "⏺️ Начать запись";

        #endregion

        #region Команды

        public ICommand TakeScreenshotCommand { get; }
        public ICommand TakeFullScreenshotCommand { get; }
        public ICommand StartStopRecordingCommand { get; }
        public ICommand SelectScreenshotFolderCommand { get; }
        public ICommand SelectVideoFolderCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand OpenScreenshotFolderCommand { get; }
        public ICommand OpenVideoFolderCommand { get; }
        public ICommand OpenHotkeySettingsCommand { get; }

        #endregion

        #region Конструктор

        public ScreenshotViewModel()
        {
            _settingsService = new SettingsService();
            _settings = _settingsService.LoadScreenshotSettings();

            TakeScreenshotCommand = new RelayCommand(_ => TakeScreenshot());
            TakeFullScreenshotCommand = new RelayCommand(_ => TakeFullScreenshot());
            StartStopRecordingCommand = new RelayCommand(_ => ToggleRecording());
            SelectScreenshotFolderCommand = new RelayCommand(_ => SelectScreenshotFolder());
            SelectVideoFolderCommand = new RelayCommand(_ => SelectVideoFolder());
            SaveSettingsCommand = new RelayCommand(_ => SaveSettings());
            OpenScreenshotFolderCommand = new RelayCommand(_ => OpenFolder(Settings.ScreenshotPath));
            OpenVideoFolderCommand = new RelayCommand(_ => OpenFolder(Settings.VideoPath));
            OpenHotkeySettingsCommand = new RelayCommand(_ => OpenHotkeySettings());
        }

        #endregion

        #region Методы инициализации

        public void Initialize(Window mainWindow)
        {
            _mainWindow = mainWindow;
            RegisterHotkeys();
        }

        private void RegisterHotkeys()
        {
            if (_mainWindow == null) return;

            _hotkeyManager = new HotkeyManager();

            try
            {
                if (Settings.ScreenshotHotkey.IsEnabled)
                {
                    _hotkeyManager.RegisterHotkey(
                        _mainWindow,
                        Settings.ScreenshotHotkey.Modifiers,
                        Settings.ScreenshotHotkey.Key,
                        () => Application.Current.Dispatcher.Invoke(TakeScreenshot)
                    );
                }

                if (Settings.FullScreenshotHotkey.IsEnabled)
                {
                    _hotkeyManager.RegisterHotkey(
                        _mainWindow,
                        Settings.FullScreenshotHotkey.Modifiers,
                        Settings.FullScreenshotHotkey.Key,
                        () => Application.Current.Dispatcher.Invoke(TakeFullScreenshot)
                    );
                }

                if (Settings.VideoRecordHotkey.IsEnabled)
                {
                    _hotkeyManager.RegisterHotkey(
                        _mainWindow,
                        Settings.VideoRecordHotkey.Modifiers,
                        Settings.VideoRecordHotkey.Key,
                        () => Application.Current.Dispatcher.Invoke(ToggleRecording)
                    );
                }

                StatusMessage = "Горячие клавиши зарегистрированы";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка регистрации горячих клавиш: {ex.Message}";
            }
        }

        #endregion

        #region Методы скриншотов

        private void TakeScreenshot()
        {
            try
            {
                StatusMessage = "Выберите область для скриншота...";
                
                if (_mainWindow != null)
                {
                    _mainWindow.Opacity = 0;
                }

                System.Threading.Thread.Sleep(100);

                var overlay = new ScreenshotOverlayWindow();
                if (overlay.ShowDialog() == true && !overlay.IsCancelled)
                {
                    var area = overlay.SelectedArea;
                    var screenshot = CaptureScreen(area);

                    if (screenshot != null)
                    {
                        var editor = new ScreenshotEditorWindow(screenshot);
                        if (editor.ShowDialog() == true && editor.EditedScreenshot != null)
                        {
                            ProcessScreenshot(editor.EditedScreenshot, editor.SaveToFile, editor.CopyToClipboard);
                        }
                    }
                }

                StatusMessage = "Готов к работе";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
                MessageBox.Show($"Ошибка при создании скриншота:\n{ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                if (_mainWindow != null)
                {
                    _mainWindow.Opacity = 1;
                    _mainWindow.Activate();
                }
            }
        }

        private void TakeFullScreenshot()
        {
            try
            {
                StatusMessage = "Создание полноэкранного скриншота...";
                
                if (_mainWindow != null)
                {
                    _mainWindow.Opacity = 0;
                }

                System.Threading.Thread.Sleep(100);

                var bounds = new Rect(
                    SystemParameters.VirtualScreenLeft,
                    SystemParameters.VirtualScreenTop,
                    SystemParameters.VirtualScreenWidth,
                    SystemParameters.VirtualScreenHeight
                );

                var screenshot = CaptureScreen(bounds);
                if (screenshot != null)
                {
                    ProcessScreenshot(screenshot, Settings.SaveToFile, Settings.CopyToClipboard);
                    StatusMessage = "Скриншот создан успешно";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
                MessageBox.Show($"Ошибка при создании скриншота:\n{ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                if (_mainWindow != null)
                {
                    _mainWindow.Opacity = 1;
                    _mainWindow.Activate();
                }
            }
        }

        private BitmapSource? CaptureScreen(Rect area)
        {
            try
            {
                var width = (int)area.Width;
                var height = (int)area.Height;
                var x = (int)area.X;
                var y = (int)area.Y;

                if (width <= 0 || height <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("Некорректные размеры области");
                    return null;
                }

                using var bitmap = new System.Drawing.Bitmap(width, height);
                using var graphics = System.Drawing.Graphics.FromImage(bitmap);
                
                graphics.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height));

                var hBitmap = bitmap.GetHbitmap();
                try
                {
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
                finally
                {
                    DeleteObject(hBitmap);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка захвата экрана: {ex.Message}");
                return null;
            }
        }

        private void ProcessScreenshot(BitmapSource screenshot, bool saveToFile, bool copyToClipboard)
        {
            if (copyToClipboard)
            {
                Clipboard.SetImage(screenshot);
                StatusMessage = "Скриншот скопирован в буфер обмена";
            }

            if (saveToFile)
            {
                var fileName = $"Screenshot_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.{Settings.ImageFormat.ToLower()}";
                var filePath = Path.Combine(Settings.ScreenshotPath, fileName);

                try
                {
                    if (!Directory.Exists(Settings.ScreenshotPath))
                    {
                        Directory.CreateDirectory(Settings.ScreenshotPath);
                    }

                    BitmapEncoder encoder = Settings.ImageFormat.ToUpper() switch
                    {
                        "PNG" => new PngBitmapEncoder(),
                        "JPEG" or "JPG" => new JpegBitmapEncoder { QualityLevel = Settings.JpegQuality },
                        "BMP" => new BmpBitmapEncoder(),
                        _ => new PngBitmapEncoder()
                    };

                    encoder.Frames.Add(BitmapFrame.Create(screenshot));
                    
                    using var stream = File.Create(filePath);
                    encoder.Save(stream);

                    StatusMessage = $"Скриншот сохранен: {fileName}";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Ошибка сохранения: {ex.Message}";
                }
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        #endregion

        #region Методы записи видео

        private void ToggleRecording()
        {
            if (IsRecording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }

        private void StartRecording()
        {
            MessageBox.Show("Функция записи видео находится в разработке.\n\nПока доступны только скриншоты.",
                          "В разработке",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
            
            StatusMessage = "Запись видео в разработке";
        }

        private void OnPauseResumeClicked(object? sender, EventArgs e)
        {
            // Заглушка для паузы записи
            StatusMessage = "Функция в разработке";
        }

        private void OnStopClicked(object? sender, EventArgs e)
        {
            // Заглушка для остановки записи
            IsRecording = false;
            StatusMessage = "Запись остановлена (заглушка)";
        }

        private void StopRecording()
        {
            // Заглушка для остановки записи
            IsRecording = false;
            StatusMessage = "Запись остановлена (заглушка)";
        }

        #endregion

        #region Методы настроек

        private void SelectScreenshotFolder()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Выберите папку для сохранения скриншотов",
                SelectedPath = Settings.ScreenshotPath
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Settings.ScreenshotPath = dialog.SelectedPath;
                OnPropertyChanged(nameof(Settings));
            }
        }

        private void SelectVideoFolder()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Выберите папку для сохранения видео",
                SelectedPath = Settings.VideoPath
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Settings.VideoPath = dialog.SelectedPath;
                OnPropertyChanged(nameof(Settings));
            }
        }

        private void SaveSettings()
        {
            try
            {
                _settingsService.SaveScreenshotSettings(Settings);
                
                _hotkeyManager?.Dispose();
                RegisterHotkeys();
                
                StatusMessage = "Настройки сохранены успешно";
                MessageBox.Show("Настройки сохранены.\nГорячие клавиши обновлены.",
                              "Успешно",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка сохранения настроек: {ex.Message}";
                MessageBox.Show($"Ошибка при сохранении настроек:\n{ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void OpenFolder(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                System.Diagnostics.Process.Start("explorer.exe", path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия папки:\n{ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void OpenHotkeySettings()
        {
            try
            {
                var dialog = new HotkeySettingsDialog(Settings);
                if (dialog.ShowDialog() == true)
                {
                    Settings.ScreenshotHotkey = dialog.Settings.ScreenshotHotkey;
                    Settings.FullScreenshotHotkey = dialog.Settings.FullScreenshotHotkey;
                    Settings.VideoRecordHotkey = dialog.Settings.VideoRecordHotkey;
                    
                    OnPropertyChanged(nameof(Settings));
                    
                    _settingsService.SaveScreenshotSettings(Settings);
                    
                    _hotkeyManager?.Dispose();
                    RegisterHotkeys();
                    
                    StatusMessage = "Горячие клавиши обновлены";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка настройки горячих клавиш:\n{ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _hotkeyManager?.Dispose();
            if (IsRecording)
            {
                StopRecording();
            }
        }

        #endregion
    }
}
