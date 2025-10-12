using Spider.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Spider.Views.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для HotkeySettingsDialog.xaml
    /// </summary>
    public partial class HotkeySettingsDialog : Window, INotifyPropertyChanged
    {
        public ScreenshotSettings Settings { get; private set; }
        private ScreenshotSettings _originalSettings;

        public string ScreenshotHotkeyText => Settings.ScreenshotHotkey.ToString();
        public string FullScreenshotHotkeyText => Settings.FullScreenshotHotkey.ToString();
        public string VideoRecordHotkeyText => Settings.VideoRecordHotkey.ToString();

        public event PropertyChangedEventHandler? PropertyChanged;

        public HotkeySettingsDialog(ScreenshotSettings settings)
        {
            InitializeComponent();
            
            _originalSettings = settings;
            Settings = new ScreenshotSettings
            {
                ScreenshotPath = settings.ScreenshotPath,
                VideoPath = settings.VideoPath,
                CopyToClipboard = settings.CopyToClipboard,
                SaveToFile = settings.SaveToFile,
                ScreenshotHotkey = new HotkeySettings
                {
                    Key = settings.ScreenshotHotkey.Key,
                    Modifiers = settings.ScreenshotHotkey.Modifiers,
                    IsEnabled = settings.ScreenshotHotkey.IsEnabled
                },
                FullScreenshotHotkey = new HotkeySettings
                {
                    Key = settings.FullScreenshotHotkey.Key,
                    Modifiers = settings.FullScreenshotHotkey.Modifiers,
                    IsEnabled = settings.FullScreenshotHotkey.IsEnabled
                },
                VideoRecordHotkey = new HotkeySettings
                {
                    Key = settings.VideoRecordHotkey.Key,
                    Modifiers = settings.VideoRecordHotkey.Modifiers,
                    IsEnabled = settings.VideoRecordHotkey.IsEnabled
                },
                JpegQuality = settings.JpegQuality,
                ImageFormat = settings.ImageFormat
            };

            DataContext = this;
        }


        private void HotkeyTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (e.Key == Key.Tab || e.Key == Key.Enter || e.Key == Key.Escape)
                    return;

                var modifiers = Keyboard.Modifiers;
                var key = e.Key;

                if (key == Key.LeftCtrl || key == Key.RightCtrl ||
                    key == Key.LeftAlt || key == Key.RightAlt ||
                    key == Key.LeftShift || key == Key.RightShift ||
                    key == Key.LWin || key == Key.RWin)
                    return;

                var hotkeyText = FormatHotkey(modifiers, key);
                textBox.Text = hotkeyText;

                var hotkeySettings = new HotkeySettings
                {
                    Key = key,
                    Modifiers = modifiers,
                    IsEnabled = true
                };

                if (textBox == ScreenshotHotkeyTextBox)
                {
                    Settings.ScreenshotHotkey = hotkeySettings;
                    OnPropertyChanged(nameof(ScreenshotHotkeyText));
                }
                else if (textBox == FullScreenshotHotkeyTextBox)
                {
                    Settings.FullScreenshotHotkey = hotkeySettings;
                    OnPropertyChanged(nameof(FullScreenshotHotkeyText));
                }
                else if (textBox == VideoRecordHotkeyTextBox)
                {
                    Settings.VideoRecordHotkey = hotkeySettings;
                    OnPropertyChanged(nameof(VideoRecordHotkeyText));
                }

                e.Handled = true;
            }
        }

        private string FormatHotkey(ModifierKeys modifiers, Key key)
        {
            var parts = new List<string>();

            if (modifiers.HasFlag(ModifierKeys.Control))
                parts.Add("Ctrl");
            if (modifiers.HasFlag(ModifierKeys.Alt))
                parts.Add("Alt");
            if (modifiers.HasFlag(ModifierKeys.Shift))
                parts.Add("Shift");
            if (modifiers.HasFlag(ModifierKeys.Windows))
                parts.Add("Win");

            parts.Add(key.ToString());

            return string.Join(" + ", parts);
        }

        private void ResetToDefaults_Click(object sender, RoutedEventArgs e)
        {
            Settings.ScreenshotHotkey = new HotkeySettings 
            { 
                Key = Key.PrintScreen, 
                Modifiers = ModifierKeys.None,
                IsEnabled = true
            };
            Settings.FullScreenshotHotkey = new HotkeySettings 
            { 
                Key = Key.PrintScreen, 
                Modifiers = ModifierKeys.Control,
                IsEnabled = true
            };
            Settings.VideoRecordHotkey = new HotkeySettings 
            { 
                Key = Key.F9, 
                Modifiers = ModifierKeys.Control | ModifierKeys.Shift,
                IsEnabled = true
            };

            OnPropertyChanged(nameof(ScreenshotHotkeyText));
            OnPropertyChanged(nameof(FullScreenshotHotkeyText));
            OnPropertyChanged(nameof(VideoRecordHotkeyText));
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Settings = _originalSettings;
            DialogResult = false;
            Close();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

