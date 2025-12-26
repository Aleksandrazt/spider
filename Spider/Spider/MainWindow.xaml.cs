using Spider.ViewModels;
using Spider.Services;
using Spider.Models;
using Spider.Views.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Spider
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly CategoriesViewModel _categoriesViewModel;
        private readonly CommandsViewModel _commandsViewModel;
        private readonly DockerViewModel _dockerViewModel;
        private readonly ScreenshotViewModel _screenshotViewModel;
        private readonly ReminderService _reminderService;
        private readonly ExportImportService _exportImportService;
        private DispatcherTimer? _snoozeTimer;
        private bool _isReminderWindowShowing;

        public MainWindow()
        {
            InitializeComponent();

            _categoriesViewModel = new CategoriesViewModel();
            _commandsViewModel = new CommandsViewModel();
            _dockerViewModel = new DockerViewModel();
            _screenshotViewModel = new ScreenshotViewModel();
            _reminderService = new ReminderService();
            _exportImportService = new ExportImportService();

            DataContext = _categoriesViewModel;

            #region Инициализация вкладки Категории

            CategoriesListBox.ItemsSource = _categoriesViewModel.Categories;
            CategoryDataGrid.ItemsSource = _categoriesViewModel.CategoryData;

            CategoriesListBox.SelectionChanged += (s, e) =>
            {
                var selectedCategory = CategoriesListBox.SelectedItem as Models.Category;
                _categoriesViewModel.SelectedCategory = selectedCategory;
                
                if (selectedCategory != null)
                {
                    SelectedCategoryName.Text = selectedCategory.Name;
                    CategoryDataGrid.Visibility = Visibility.Visible;
                    NoCategoryMessage.Visibility = Visibility.Collapsed;
                    AddCategoryDataButton.Visibility = Visibility.Visible;
                }
                else
                {
                    SelectedCategoryName.Text = "(выберите категорию)";
                    CategoryDataGrid.Visibility = Visibility.Collapsed;
                    NoCategoryMessage.Visibility = Visibility.Visible;
                    AddCategoryDataButton.Visibility = Visibility.Collapsed;
                }
            };

            CategoryDataGrid.SelectionChanged += (s, e) =>
            {
                _categoriesViewModel.SelectedCategoryData = CategoryDataGrid.SelectedItem as Models.CategoryData;
            };

            AddCategoryButton.Command = _categoriesViewModel.AddCategoryCommand;
            AddCategoryDataButton.Command = _categoriesViewModel.AddCategoryDataCommand;
            AddCategoryDataButton.Visibility = Visibility.Collapsed;
            
            #endregion

            #region Инициализация вкладки Команды

            CommandsListBox.ItemsSource = _commandsViewModel.Commands;

            CommandsListBox.SelectionChanged += (s, e) =>
            {
                var selectedCommand = CommandsListBox.SelectedItem as CommandViewModel;
                _commandsViewModel.SelectedCommand = selectedCommand;
                
                if (selectedCommand != null)
                {
                    this.SelectedCommandName.Text = selectedCommand.Name;
                    CommandControlPanel.Visibility = Visibility.Visible;
                    OutputArea.Visibility = Visibility.Visible;
                    NoCommandMessage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.SelectedCommandName.Text = "(выберите команду)";
                    CommandControlPanel.Visibility = Visibility.Collapsed;
                    OutputArea.Visibility = Visibility.Collapsed;
                    NoCommandMessage.Visibility = Visibility.Visible;
                }
            };

            AdditionalArgumentsTextBox.TextChanged += (s, e) =>
            {
                _commandsViewModel.AdditionalArguments = AdditionalArgumentsTextBox.Text;
            };

            _commandsViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_commandsViewModel.CommandOutput))
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Обновление вывода: {_commandsViewModel.CommandOutput.Length} символов");
                    CommandOutputTextBox.Text = _commandsViewModel.CommandOutput;
                    CommandOutputTextBox.ScrollToEnd();
                }
                else if (e.PropertyName == nameof(_commandsViewModel.IsExecuting))
                {
                    ExecuteCommandButton.IsEnabled = _commandsViewModel.CanExecute;
                    StopCommandButton.IsEnabled = _commandsViewModel.CanStop;
                }
                else if (e.PropertyName == nameof(_commandsViewModel.AdditionalArguments))
                {
                    if (AdditionalArgumentsTextBox.Text != _commandsViewModel.AdditionalArguments)
                    {
                        AdditionalArgumentsTextBox.Text = _commandsViewModel.AdditionalArguments;
                    }
                }
            };

            AddCommandButton.Command = _commandsViewModel.AddCommandCommand;
            ExecuteCommandButton.Command = _commandsViewModel.ExecuteCommandCommand;
            StopCommandButton.Command = _commandsViewModel.StopCommandCommand;
            ClearOutputButton.Command = _commandsViewModel.ClearOutputCommand;

            CommandControlPanel.Visibility = Visibility.Collapsed;
            OutputArea.Visibility = Visibility.Collapsed;

            var combinedDataContext = new CombinedViewModel
            {
                CategoriesViewModel = _categoriesViewModel,
                CommandsViewModel = _commandsViewModel,
                DockerViewModel = _dockerViewModel
            };
            
            DataContext = combinedDataContext;

            #endregion

            #region Инициализация вкладки Docker

            DockerProjectsListBox.ItemsSource = _dockerViewModel.Projects;
            DockerImagesDataGrid.ItemsSource = _dockerViewModel.Images;

            DockerProjectsListBox.SelectionChanged += (s, e) =>
            {
                var selectedProject = DockerProjectsListBox.SelectedItem as Models.DockerProject;
                _dockerViewModel.SelectedProject = selectedProject;
                
                if (selectedProject != null)
                {
                    SelectedDockerProjectName.Text = selectedProject.Name;
                    DockerControlPanel.Visibility = Visibility.Visible;
                    DockerImagesDataGrid.Visibility = Visibility.Visible;
                    NoDockerProjectMessage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SelectedDockerProjectName.Text = "(выберите проект)";
                    DockerControlPanel.Visibility = Visibility.Collapsed;
                    DockerImagesDataGrid.Visibility = Visibility.Collapsed;
                    NoDockerProjectMessage.Visibility = Visibility.Visible;
                }
            };

            _dockerViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_dockerViewModel.BuildOutput))
                {
                    DockerBuildOutputTextBox.Text = _dockerViewModel.BuildOutput;
                    DockerBuildOutputTextBox.ScrollToEnd();
                }
            };

            AddDockerProjectButton.Command = _dockerViewModel.AddProjectCommand;
            RefreshDockerImagesButton.Command = _dockerViewModel.RefreshImagesCommand;
            StartAllDockerImagesButton.Command = _dockerViewModel.StartAllImagesCommand;
            StopAllDockerImagesButton.Command = _dockerViewModel.StopAllImagesCommand;
            ClearDockerOutputButton.Command = _dockerViewModel.ClearOutputCommand;

            DockerControlPanel.Visibility = Visibility.Collapsed;
            DockerImagesDataGrid.Visibility = Visibility.Collapsed;

            #endregion

            #region Инициализация вкладки Скриншоты

            _screenshotViewModel.Initialize(this);

            ScreenshotPathTextBox.Text = _screenshotViewModel.Settings.ScreenshotPath;
            VideoPathTextBox.Text = _screenshotViewModel.Settings.VideoPath;
            SaveToFileCheckBox.IsChecked = _screenshotViewModel.Settings.SaveToFile;
            CopyToClipboardCheckBox.IsChecked = _screenshotViewModel.Settings.CopyToClipboard;
            
            var formatIndex = _screenshotViewModel.Settings.ImageFormat.ToUpper() switch
            {
                "PNG" => 0,
                "JPEG" or "JPG" => 1,
                "BMP" => 2,
                _ => 0
            };
            ImageFormatComboBox.SelectedIndex = formatIndex;

            ScreenshotHotkeyTextBox.Text = _screenshotViewModel.Settings.ScreenshotHotkey.ToString();
            FullScreenshotHotkeyTextBox.Text = _screenshotViewModel.Settings.FullScreenshotHotkey.ToString();
            VideoRecordHotkeyTextBox.Text = _screenshotViewModel.Settings.VideoRecordHotkey.ToString();

            _screenshotViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_screenshotViewModel.StatusMessage))
                {
                    ScreenshotStatusText.Text = _screenshotViewModel.StatusMessage;
                }
                else if (e.PropertyName == nameof(_screenshotViewModel.RecordButtonText))
                {
                    StartStopRecordingButton.Content = _screenshotViewModel.RecordButtonText;
                }
            };

            TakeScreenshotButton.Command = _screenshotViewModel.TakeScreenshotCommand;
            TakeFullScreenshotButton.Command = _screenshotViewModel.TakeFullScreenshotCommand;
            StartStopRecordingButton.Command = _screenshotViewModel.StartStopRecordingCommand;
            SelectScreenshotFolderButton.Command = _screenshotViewModel.SelectScreenshotFolderCommand;
            SelectVideoFolderButton.Command = _screenshotViewModel.SelectVideoFolderCommand;
            OpenScreenshotFolderButton.Command = _screenshotViewModel.OpenScreenshotFolderCommand;
            OpenVideoFolderButton.Command = _screenshotViewModel.OpenVideoFolderCommand;
            ConfigureHotkeysButton.Command = _screenshotViewModel.OpenHotkeySettingsCommand;
            SaveScreenshotSettingsButton.Click += SaveScreenshotSettings_Click;

            SaveToFileCheckBox.Checked += (s, e) => _screenshotViewModel.Settings.SaveToFile = true;
            SaveToFileCheckBox.Unchecked += (s, e) => _screenshotViewModel.Settings.SaveToFile = false;
            CopyToClipboardCheckBox.Checked += (s, e) => _screenshotViewModel.Settings.CopyToClipboard = true;
            CopyToClipboardCheckBox.Unchecked += (s, e) => _screenshotViewModel.Settings.CopyToClipboard = false;
            
            ImageFormatComboBox.SelectionChanged += (s, e) =>
            {
                if (ImageFormatComboBox.SelectedItem is ComboBoxItem item)
                {
                    _screenshotViewModel.Settings.ImageFormat = item.Content.ToString() ?? "PNG";
                }
            };

            _screenshotViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_screenshotViewModel.Settings))
                {
                    ScreenshotPathTextBox.Text = _screenshotViewModel.Settings.ScreenshotPath;
                    VideoPathTextBox.Text = _screenshotViewModel.Settings.VideoPath;
                    
                    ScreenshotHotkeyTextBox.Text = _screenshotViewModel.Settings.ScreenshotHotkey.ToString();
                    FullScreenshotHotkeyTextBox.Text = _screenshotViewModel.Settings.FullScreenshotHotkey.ToString();
                    VideoRecordHotkeyTextBox.Text = _screenshotViewModel.Settings.VideoRecordHotkey.ToString();
                }
            };

            #endregion

            #region Инициализация вкладки Напоминания

            var reminderSettings = _reminderService.GetSettings();
            EnableRemindersCheckBox.IsChecked = reminderSettings.IsEnabled;
            IntervalMinutesTextBox.Text = reminderSettings.IntervalMinutes.ToString();
            ReminderTitleTextBox.Text = reminderSettings.ReminderTitle;
            ReminderMessageTextBox.Text = reminderSettings.ReminderMessage;

            UpdateReminderStatus();

            EnableRemindersCheckBox.Checked += (s, e) => UpdateReminderStatus();
            EnableRemindersCheckBox.Unchecked += (s, e) => UpdateReminderStatus();

            SaveReminderSettingsButton.Click += SaveReminderSettings_Click;
            TestReminderButton.Click += TestReminder_Click;

            _reminderService.ReminderTriggered += OnReminderTriggered;

            // Запустить таймер, если напоминания включены
            if (reminderSettings.IsEnabled)
            {
                _reminderService.Start();
            }

            #endregion

            #region Инициализация кнопок экспорта/импорта

            ExportDataButton.Click += ExportDataButton_Click;
            ImportDataButton.Click += ImportDataButton_Click;

            #endregion
        }

        private async void ExportDataButton_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                FileName = $"spider_export_{DateTime.Now:yyyyMMdd_HHmmss}.json",
                Title = "Экспорт данных"
            };

            if (saveDialog.ShowDialog() == true)
            {
                ExportDataButton.IsEnabled = false;
                ExportDataButton.Content = "⏳ Экспорт...";
                
                try
                {
                    var success = await _exportImportService.ExportDataAsync(saveDialog.FileName);
                    if (success)
                    {
                        MessageBox.Show(
                            $"Данные успешно экспортированы в файл:\n{saveDialog.FileName}",
                            "Экспорт завершен",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                finally
                {
                    ExportDataButton.IsEnabled = true;
                    ExportDataButton.Content = "📤 Экспорт данных";
                }
            }
        }

        private async void ImportDataButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                Title = "Импорт данных"
            };

            if (openDialog.ShowDialog() == true)
            {
                ImportDataButton.IsEnabled = false;
                ImportDataButton.Content = "⏳ Импорт...";
                
                try
                {
                    var success = await _exportImportService.ImportDataAsync(openDialog.FileName);
                    if (success)
                    {
                        await RefreshAllDataAsync();
                    }
                }
                finally
                {
                    ImportDataButton.IsEnabled = true;
                    ImportDataButton.Content = "📥 Импорт данных";
                }
            }
        }

        private async Task RefreshAllDataAsync()
        {
            await _categoriesViewModel.LoadCategoriesAsync();
            
            await _commandsViewModel.LoadCommandsAsync();
            
            await _dockerViewModel.LoadProjectsAsync();
        }

        private void SaveScreenshotSettings_Click(object sender, RoutedEventArgs e)
        {
            _screenshotViewModel.SaveSettingsCommand.Execute(null);
        }

        #region Обработчики напоминаний

        private void SaveReminderSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация интервала
                if (!int.TryParse(IntervalMinutesTextBox.Text, out int intervalMinutes) || intervalMinutes < 1)
                {
                    MessageBox.Show("Пожалуйста, введите корректный интервал (минимум 1 минута)", 
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var settings = new ReminderSettings
                {
                    IsEnabled = EnableRemindersCheckBox.IsChecked == true,
                    IntervalMinutes = intervalMinutes,
                    ReminderTitle = ReminderTitleTextBox.Text,
                    ReminderMessage = ReminderMessageTextBox.Text
                };

                _reminderService.UpdateSettings(settings);
                UpdateReminderStatus();

                MessageBox.Show("Настройки напоминаний сохранены успешно!", 
                              "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении настроек: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TestReminder_Click(object sender, RoutedEventArgs e)
        {
            ShowReminderWindow();
        }

        private void OnReminderTriggered(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ShowReminderWindow();
            });
        }

        private void ShowReminderWindow()
        {
            // Предотвращаем показ нескольких окон одновременно
            if (_isReminderWindowShowing)
            {
                return;
            }

            try
            {
                _isReminderWindowShowing = true;
                
                var settings = _reminderService.GetSettings();
                var reminderWindow = new ReminderWindow(settings.ReminderTitle, settings.ReminderMessage);
                
                reminderWindow.ShowDialog();

                // Если пользователь нажал "Напомнить позже"
                if (reminderWindow.Snoozed)
                {
                    StartSnoozeTimer();
                }
                else
                {
                    // Пользователь закрыл окно нормально - возобновляем основной таймер
                    if (_reminderService.GetSettings().IsEnabled && !_reminderService.IsRunning)
                    {
                        _reminderService.Start();
                    }
                }
            }
            finally
            {
                _isReminderWindowShowing = false;
            }
        }

        private void StartSnoozeTimer()
        {
            // Остановить основной таймер
            _reminderService.Stop();

            // Правильно очищаем предыдущий snooze таймер
            CleanupSnoozeTimer();

            // Создать таймер отложенного напоминания на 5 минут
            _snoozeTimer = new DispatcherTimer();
            _snoozeTimer.Interval = TimeSpan.FromMinutes(5);
            _snoozeTimer.Tick += OnSnoozeTimerTick;
            _snoozeTimer.Start();
            
            System.Diagnostics.Debug.WriteLine("Snooze таймер запущен на 5 минут");
        }

        private void OnSnoozeTimerTick(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Snooze таймер сработал");
            
            // Останавливаем и очищаем snooze таймер
            CleanupSnoozeTimer();
            
            // Показываем напоминание
            ShowReminderWindow();
            
            // Если после показа напоминания snooze таймер не был запущен снова,
            // перезапускаем основной таймер
            if (_snoozeTimer == null && _reminderService.GetSettings().IsEnabled)
            {
                _reminderService.Start();
                System.Diagnostics.Debug.WriteLine("Основной таймер возобновлен");
            }
        }

        private void CleanupSnoozeTimer()
        {
            if (_snoozeTimer != null)
            {
                _snoozeTimer.Stop();
                _snoozeTimer.Tick -= OnSnoozeTimerTick;
                _snoozeTimer = null;
                System.Diagnostics.Debug.WriteLine("Snooze таймер очищен");
            }
        }

        private void UpdateReminderStatus()
        {
            if (EnableRemindersCheckBox.IsChecked == true)
            {
                ReminderStatusText.Text = "(включено)";
                ReminderStatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(76, 175, 80)); // Green
            }
            else
            {
                ReminderStatusText.Text = "(выключено)";
                ReminderStatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(153, 153, 153)); // Gray
            }
        }

        #endregion

        protected override void OnClosed(EventArgs e)
        {
            _categoriesViewModel?.Dispose();
            _commandsViewModel?.Dispose();
            _dockerViewModel?.Dispose();
            _screenshotViewModel?.Dispose();
            _reminderService?.Dispose();
            _exportImportService?.Dispose();
            CleanupSnoozeTimer();
            base.OnClosed(e);
        }

        private class CombinedViewModel
        {
            public CategoriesViewModel? CategoriesViewModel { get; set; }
            public CommandsViewModel? CommandsViewModel { get; set; }
            public DockerViewModel? DockerViewModel { get; set; }
            
            public System.Windows.Input.ICommand? EditCategoryCommand => CategoriesViewModel?.EditCategoryCommand;
            public System.Windows.Input.ICommand? DeleteCategoryCommand => CategoriesViewModel?.DeleteCategoryCommand;
            public System.Windows.Input.ICommand? EditCategoryDataCommand => CategoriesViewModel?.EditCategoryDataCommand;
            public System.Windows.Input.ICommand? DeleteCategoryDataCommand => CategoriesViewModel?.DeleteCategoryDataCommand;
            
            public System.Windows.Input.ICommand? EditCommandCommand => CommandsViewModel?.EditCommandCommand;
            public System.Windows.Input.ICommand? DeleteCommandCommand => CommandsViewModel?.DeleteCommandCommand;
            
            public System.Windows.Input.ICommand? EditDockerProjectCommand => DockerViewModel?.EditProjectCommand;
            public System.Windows.Input.ICommand? DeleteDockerProjectCommand => DockerViewModel?.DeleteProjectCommand;
            public System.Windows.Input.ICommand? StartDockerImageCommand => DockerViewModel?.StartImageCommand;
            public System.Windows.Input.ICommand? StopDockerImageCommand => DockerViewModel?.StopImageCommand;
            public System.Windows.Input.ICommand? RebuildDockerImageCommand => DockerViewModel?.RebuildImageCommand;
        }
    }
}