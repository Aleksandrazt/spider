using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Spider.Helpers;
using Spider.Models;
using Spider.Services;
using Spider.Views.Dialogs;

namespace Spider.ViewModels
{
    /// <summary>
    /// ViewModel для вкладки "Команды"
    /// </summary>
    public class CommandsViewModel : INotifyPropertyChanged
    {
        private readonly CommandService _commandService;
        private ObservableCollection<Command> _commands;
        private Command? _selectedCommand;
        private string _additionalArguments = string.Empty;
        private string _commandOutput = string.Empty;
        private bool _isLoading;
        private bool _isExecuting;
        private Process? _currentProcess;

        #region Свойства

        /// <summary>
        /// Коллекция всех команд
        /// </summary>
        public ObservableCollection<Command> Commands
        {
            get => _commands;
            set => SetProperty(ref _commands, value);
        }

        /// <summary>
        /// Выбранная команда
        /// </summary>
        public Command? SelectedCommand
        {
            get => _selectedCommand;
            set
            {
                if (SetProperty(ref _selectedCommand, value))
                {
                    OnPropertyChanged(nameof(SelectedCommandName));
                    OnPropertyChanged(nameof(IsCommandSelected));
                    AdditionalArguments = value?.Arguments ?? string.Empty;
                    CommandOutput = string.Empty;
                }
            }
        }

        /// <summary>
        /// Дополнительные аргументы для команды
        /// </summary>
        public string AdditionalArguments
        {
            get => _additionalArguments;
            set => SetProperty(ref _additionalArguments, value);
        }

        /// <summary>
        /// Вывод команды (как в терминале)
        /// </summary>
        public string CommandOutput
        {
            get => _commandOutput;
            set => SetProperty(ref _commandOutput, value);
        }

        /// <summary>
        /// Название выбранной команды (для отображения)
        /// </summary>
        public string SelectedCommandName => SelectedCommand?.Name ?? "(выберите команду)";

        /// <summary>
        /// Флаг: выбрана ли команда
        /// </summary>
        public bool IsCommandSelected => SelectedCommand != null;

        /// <summary>
        /// Флаг загрузки данных
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// Флаг выполнения команды
        /// </summary>
        public bool IsExecuting
        {
            get => _isExecuting;
            set
            {
                if (SetProperty(ref _isExecuting, value))
                {
                    OnPropertyChanged(nameof(CanExecute));
                    OnPropertyChanged(nameof(CanStop));
                }
            }
        }

        /// <summary>
        /// Можно ли запустить команду
        /// </summary>
        public bool CanExecute => !IsExecuting && IsCommandSelected;

        /// <summary>
        /// Можно ли остановить команду
        /// </summary>
        public bool CanStop => IsExecuting;

        #endregion

        #region Команды

        /// <summary>
        /// Команда загрузки списка команд
        /// </summary>
        public ICommand LoadCommandsCommand { get; }

        /// <summary>
        /// Команда добавления новой команды
        /// </summary>
        public ICommand AddCommandCommand { get; }

        /// <summary>
        /// Команда редактирования команды
        /// </summary>
        public ICommand EditCommandCommand { get; }

        /// <summary>
        /// Команда удаления команды
        /// </summary>
        public ICommand DeleteCommandCommand { get; }

        /// <summary>
        /// Команда выполнения выбранной команды
        /// </summary>
        public ICommand ExecuteCommandCommand { get; }

        /// <summary>
        /// Команда остановки выполнения команды
        /// </summary>
        public ICommand StopCommandCommand { get; }

        /// <summary>
        /// Команда очистки вывода
        /// </summary>
        public ICommand ClearOutputCommand { get; }

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор ViewModel
        /// </summary>
        public CommandsViewModel()
        {
            _commandService = new CommandService();
            _commands = new ObservableCollection<Command>();

            LoadCommandsCommand = new RelayCommand(async _ => await LoadCommandsAsync());
            AddCommandCommand = new RelayCommand(_ => AddCommand());
            EditCommandCommand = new RelayCommand(param => EditCommand(param as Command));
            DeleteCommandCommand = new RelayCommand(param => DeleteCommand(param as Command));
            ExecuteCommandCommand = new RelayCommand(async _ => await ExecuteCommandAsync(), _ => CanExecute);
            StopCommandCommand = new RelayCommand(_ => StopCommand(), _ => CanStop);
            ClearOutputCommand = new RelayCommand(_ => CommandOutput = string.Empty);

            _ = LoadCommandsAsync();
        }

        #endregion

        #region Методы загрузки данных

        /// <summary>
        /// Загрузка всех команд
        /// </summary>
        private async Task LoadCommandsAsync()
        {
            try
            {
                IsLoading = true;
                var commands = await _commandService.GetCommandsAsync();

                Commands.Clear();
                foreach (var command in commands)
                {
                    Commands.Add(command);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки команд: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки команд:\n{ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Методы CRUD операций

        /// <summary>
        /// Добавление новой команды
        /// </summary>
        private void AddCommand()
        {
            var dialog = new CommandDialog();
            if (dialog.ShowDialog() == true)
            {
                _ = AddCommandAsync(dialog.Command);
            }
        }

        /// <summary>
        /// Асинхронное добавление команды
        /// </summary>
        private async Task AddCommandAsync(Command command)
        {
            try
            {
                await _commandService.AddCommandAsync(command);
                await LoadCommandsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении команды:\n{ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Редактирование выбранной команды
        /// </summary>
        private void EditCommand(Command? command = null)
        {
            var commandToEdit = command ?? SelectedCommand;

            if (commandToEdit == null)
            {
                MessageBox.Show("Выберите команду для редактирования!",
                              "Внимание",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
                return;
            }

            var dialog = new CommandDialog(commandToEdit);
            if (dialog.ShowDialog() == true)
            {
                _ = UpdateCommandAsync(dialog.Command);
            }
        }

        /// <summary>
        /// Удаление выбранной команды
        /// </summary>
        private async void DeleteCommand(Command? command = null)
        {
            var commandToDelete = command ?? SelectedCommand;

            if (commandToDelete == null)
            {
                MessageBox.Show("Выберите команду для удаления!",
                              "Внимание",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить команду '{commandToDelete.Name}'?",
                               "Подтверждение удаления",
                               MessageBoxButton.YesNo,
                               MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _commandService.DeleteCommandAsync(commandToDelete.Id);
                    await LoadCommandsAsync();
                    if (SelectedCommand == commandToDelete)
                        SelectedCommand = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении команды:\n{ex.Message}",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Асинхронное обновление команды
        /// </summary>
        private async Task UpdateCommandAsync(Command command)
        {
            try
            {
                await _commandService.UpdateCommandAsync(command);
                await LoadCommandsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении команды:\n{ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        #endregion

        #region Методы выполнения команд

        /// <summary>
        /// Выполнение выбранной команды
        /// </summary>
        private async Task ExecuteCommandAsync()
        {
            if (SelectedCommand == null) return;

            try
            {
                IsExecuting = true;

                if (!System.IO.Directory.Exists(SelectedCommand.FolderPath))
                {
                    CommandOutput += $"❌ ОШИБКА: Директория '{SelectedCommand.FolderPath}' не найдена!\n\n";
                    IsExecuting = false;
                    return;
                }

                var fullCommand = SelectedCommand.CommandText;
                if (!string.IsNullOrWhiteSpace(AdditionalArguments))
                {
                    fullCommand += " " + AdditionalArguments;
                }

                CommandOutput += $"═══════════════════════════════════════════════════\n";
                CommandOutput += $"🚀 Запуск команды: {SelectedCommand.Name}\n";
                CommandOutput += $"📁 Рабочая директория: {SelectedCommand.FolderPath}\n";
                CommandOutput += $"⚡ Команда: {fullCommand}\n";
                CommandOutput += $"═══════════════════════════════════════════════════\n\n";

                _currentProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -Command \"{fullCommand}\"",
                        WorkingDirectory = SelectedCommand.FolderPath,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = System.Text.Encoding.UTF8,
                        StandardErrorEncoding = System.Text.Encoding.UTF8
                    }
                };

                _currentProcess.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            CommandOutput += e.Data + "\n";
                        });
                    }
                };

                _currentProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            CommandOutput += $"⚠️ {e.Data}\n";
                        });
                    }
                };

                _currentProcess.Start();
                _currentProcess.BeginOutputReadLine();
                _currentProcess.BeginErrorReadLine();

                await Task.Run(() => _currentProcess.WaitForExit());

                var exitCode = _currentProcess.ExitCode;
                CommandOutput += $"\n═══════════════════════════════════════════════════\n";
                if (exitCode == 0)
                {
                    CommandOutput += $"✅ Команда выполнена успешно (код: {exitCode})\n";
                }
                else
                {
                    CommandOutput += $"❌ Команда завершена с ошибкой (код: {exitCode})\n";
                }
                CommandOutput += $"═══════════════════════════════════════════════════\n\n";
            }
            catch (Exception ex)
            {
                CommandOutput += $"\n❌ ОШИБКА ВЫПОЛНЕНИЯ:\n{ex.Message}\n\n";
            }
            finally
            {
                _currentProcess?.Dispose();
                _currentProcess = null;
                IsExecuting = false;
            }
        }

        /// <summary>
        /// Остановка выполнения команды
        /// </summary>
        private void StopCommand()
        {
            if (_currentProcess != null && !_currentProcess.HasExited)
            {
                try
                {
                    _currentProcess.Kill(entireProcessTree: true);
                    CommandOutput += $"\n⛔ Выполнение команды принудительно остановлено\n\n";
                }
                catch (Exception ex)
                {
                    CommandOutput += $"\n❌ Ошибка при остановке команды: {ex.Message}\n\n";
                }
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

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            StopCommand();
            _commandService?.Dispose();
        }

        #endregion
    }
}

