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
        private ObservableCollection<CommandViewModel> _commands;
        private CommandViewModel? _selectedCommand;
        private string _additionalArguments = string.Empty;
        private bool _isLoading;

        #region Свойства

        /// <summary>
        /// Коллекция всех команд
        /// </summary>
        public ObservableCollection<CommandViewModel> Commands
        {
            get => _commands;
            set => SetProperty(ref _commands, value);
        }

        /// <summary>
        /// Выбранная команда
        /// </summary>
        public CommandViewModel? SelectedCommand
        {
            get => _selectedCommand;
            set
            {
                if (SetProperty(ref _selectedCommand, value))
                {
                    OnPropertyChanged(nameof(SelectedCommandName));
                    OnPropertyChanged(nameof(IsCommandSelected));
                    OnPropertyChanged(nameof(SelectedCommandOutput));
                    OnPropertyChanged(nameof(CommandOutput));
                    OnPropertyChanged(nameof(IsExecuting));
                    OnPropertyChanged(nameof(CanExecute));
                    OnPropertyChanged(nameof(CanStop));
                    AdditionalArguments = value?.Arguments ?? string.Empty;

                    if (value != null)
                    {
                        value.PropertyChanged += OnSelectedCommandPropertyChanged;
                    }

                    ((RelayCommand)ExecuteCommandCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)StopCommandCommand).RaiseCanExecuteChanged();
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
        /// Вывод выбранной команды (как в терминале)
        /// </summary>
        public string SelectedCommandOutput => SelectedCommand?.Output ?? string.Empty;

        /// <summary>
        /// Вывод команды (для совместимости с MainWindow.xaml.cs)
        /// </summary>
        public string CommandOutput => SelectedCommandOutput;

        /// <summary>
        /// Флаг выполнения команды (для совместимости с MainWindow.xaml.cs)
        /// </summary>
        public bool IsExecuting => SelectedCommand?.IsExecuting ?? false;

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
        /// Можно ли запустить выбранную команду
        /// </summary>
        public bool CanExecute => SelectedCommand?.CanExecute ?? false;

        /// <summary>
        /// Можно ли остановить выбранную команду
        /// </summary>
        public bool CanStop => SelectedCommand?.CanStop ?? false;

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
            _commands = new ObservableCollection<CommandViewModel>();

            LoadCommandsCommand = new RelayCommand(async _ => await LoadCommandsAsync());
            AddCommandCommand = new RelayCommand(_ => AddCommand());
            EditCommandCommand = new RelayCommand(param => EditCommand(param as CommandViewModel));
            DeleteCommandCommand = new RelayCommand(param => DeleteCommand(param as CommandViewModel));
            ExecuteCommandCommand = new RelayCommand(async _ => await ExecuteCommandAsync(), _ => CanExecute);
            StopCommandCommand = new RelayCommand(_ => StopCommand(), _ => CanStop);
            ClearOutputCommand = new RelayCommand(_ => 
            {
                if (SelectedCommand != null)
                    SelectedCommand.Output = string.Empty;
            });

            _ = LoadCommandsAsync();
            
            _ = Task.Run(async () =>
            {
                await Task.Delay(1000);
                if (!Commands.Any())
                {
                    await CreateTestCommand();
                }
            });
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

                System.Diagnostics.Debug.WriteLine($"[DEBUG] Загружено команд: {commands.Count}");
                
                Commands.Clear();
                foreach (var command in commands)
                {
                    Commands.Add(new CommandViewModel(command, this));
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Добавлена команда: {command.Name}");
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

        /// <summary>
        /// Создание тестовой команды для проверки
        /// </summary>
        private async Task CreateTestCommand()
        {
            try
            {
                var testCommand = new Models.Command
                {
                    Name = "Тестовая команда",
                    CommandText = "echo 'Привет, мир!'",
                    FolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Description = "Простая тестовая команда для проверки работы"
                };

                await _commandService.AddCommandAsync(testCommand);
                await LoadCommandsAsync();
                
                System.Diagnostics.Debug.WriteLine("[DEBUG] Создана тестовая команда");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Ошибка создания тестовой команды: {ex.Message}");
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
        private void EditCommand(CommandViewModel? commandViewModel = null)
        {
            var commandToEdit = commandViewModel ?? SelectedCommand;

            if (commandToEdit == null)
            {
                MessageBox.Show("Выберите команду для редактирования!",
                              "Внимание",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
                return;
            }

            var dialog = new CommandDialog(commandToEdit.Command);
            if (dialog.ShowDialog() == true)
            {
                _ = UpdateCommandAsync(dialog.Command);
            }
        }

        /// <summary>
        /// Удаление выбранной команды
        /// </summary>
        private async void DeleteCommand(CommandViewModel? commandViewModel = null)
        {
            var commandToDelete = commandViewModel ?? SelectedCommand;

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
        public async Task ExecuteCommandAsync()
        {
            if (SelectedCommand == null) return;

            await ExecuteCommandAsync(SelectedCommand);
        }

        /// <summary>
        /// Выполнение конкретной команды
        /// </summary>
        public async Task ExecuteCommandAsync(CommandViewModel commandViewModel)
        {
            try
            {
                commandViewModel.IsExecuting = true;

                if (!System.IO.Directory.Exists(commandViewModel.FolderPath))
                {
                    commandViewModel.Output += $"❌ ОШИБКА: Директория '{commandViewModel.FolderPath}' не найдена!\n\n";
                    commandViewModel.IsExecuting = false;
                    return;
                }

                var fullCommand = commandViewModel.CommandText;
                if (!string.IsNullOrWhiteSpace(AdditionalArguments))
                {
                    fullCommand += " " + AdditionalArguments;
                }

                commandViewModel.Output += $"═══════════════════════════════════════════════════\n";
                commandViewModel.Output += $"🚀 Запуск команды: {commandViewModel.Name}\n";
                commandViewModel.Output += $"📁 Рабочая директория: {commandViewModel.FolderPath}\n";
                commandViewModel.Output += $"⚡ Команда: {fullCommand}\n";
                commandViewModel.Output += $"═══════════════════════════════════════════════════\n\n";
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Команда запущена: {commandViewModel.Name}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Вывод после запуска: {commandViewModel.Output.Length} символов");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -Command \"{fullCommand}\"",
                        WorkingDirectory = commandViewModel.FolderPath,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = System.Text.Encoding.UTF8,
                        StandardErrorEncoding = System.Text.Encoding.UTF8
                    }
                };

                commandViewModel.CurrentProcess = process;

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            commandViewModel.Output += e.Data + "\n";
                            System.Diagnostics.Debug.WriteLine($"[DEBUG] Получен вывод: {e.Data}");
                        });
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            commandViewModel.Output += $"⚠️ {e.Data}\n";
                        });
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await Task.Run(() => process.WaitForExit());

                var exitCode = process.ExitCode;
                commandViewModel.Output += $"\n═══════════════════════════════════════════════════\n";
                if (exitCode == 0)
                {
                    commandViewModel.Output += $"✅ Команда выполнена успешно (код: {exitCode})\n";
                }
                else
                {
                    commandViewModel.Output += $"❌ Команда завершена с ошибкой (код: {exitCode})\n";
                }
                commandViewModel.Output += $"═══════════════════════════════════════════════════\n\n";
            }
            catch (Exception ex)
            {
                commandViewModel.Output += $"\n❌ ОШИБКА ВЫПОЛНЕНИЯ:\n{ex.Message}\n\n";
            }
            finally
            {
                commandViewModel.CurrentProcess?.Dispose();
                commandViewModel.CurrentProcess = null;
                commandViewModel.IsExecuting = false;
            }
        }

        /// <summary>
        /// Остановка выполнения команды
        /// </summary>
        private void StopCommand()
        {
            if (SelectedCommand?.CurrentProcess != null && !SelectedCommand.CurrentProcess.HasExited)
            {
                try
                {
                    SelectedCommand.CurrentProcess.Kill(entireProcessTree: true);
                    SelectedCommand.Output += $"\n⛔ Выполнение команды принудительно остановлено\n\n";
                }
                catch (Exception ex)
                {
                    SelectedCommand.Output += $"\n❌ Ошибка при остановке команды: {ex.Message}\n\n";
                }
            }
        }

        /// <summary>
        /// Остановка выполнения конкретной команды
        /// </summary>
        public void StopCommandFor(CommandViewModel commandViewModel)
        {
            if (commandViewModel.CurrentProcess != null && !commandViewModel.CurrentProcess.HasExited)
            {
                try
                {
                    commandViewModel.CurrentProcess.Kill(entireProcessTree: true);
                    commandViewModel.Output += $"\n⛔ Выполнение команды принудительно остановлено\n\n";
                }
                catch (Exception ex)
                {
                    commandViewModel.Output += $"\n❌ Ошибка при остановке команды: {ex.Message}\n\n";
                }
            }
        }

        #endregion

        #region Обработчики событий

        /// <summary>
        /// Обработчик изменений выбранной команды
        /// </summary>
        private void OnSelectedCommandPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == SelectedCommand)
            {
                if (e.PropertyName == nameof(CommandViewModel.Output))
                {
                    OnPropertyChanged(nameof(CommandOutput));
                    OnPropertyChanged(nameof(SelectedCommandOutput));
                }
                else if (e.PropertyName == nameof(CommandViewModel.IsExecuting))
                {
                    OnPropertyChanged(nameof(IsExecuting));
                    OnPropertyChanged(nameof(CanExecute));
                    OnPropertyChanged(nameof(CanStop));
                    
                    // Принудительно обновляем команды
                    ((RelayCommand)ExecuteCommandCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)StopCommandCommand).RaiseCanExecuteChanged();
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
            // Останавливаем все выполняющиеся команды
            foreach (var command in Commands)
            {
                if (command.CurrentProcess != null && !command.CurrentProcess.HasExited)
                {
                    try
                    {
                        command.CurrentProcess.Kill(entireProcessTree: true);
                        command.CurrentProcess.Dispose();
                    }
                    catch { }
                }
            }
            
            _commandService?.Dispose();
        }

        #endregion
    }

    /// <summary>
    /// ViewModel для отображения команды с состоянием выполнения
    /// </summary>
    public class CommandViewModel : INotifyPropertyChanged
    {
        private readonly Command _command;
        private readonly CommandsViewModel _parentViewModel;
        private bool _isExecuting;
        private Process? _currentProcess;
        private string _output = string.Empty;

        public CommandViewModel(Command command, CommandsViewModel parentViewModel)
        {
            _command = command;
            _parentViewModel = parentViewModel;

            ClearOutputCommand = new RelayCommand(_ => Output = string.Empty);
        }

        public Command Command => _command;
        public int Id => _command.Id;
        public string Name => _command.Name;
        public string CommandText => _command.CommandText;
        public string FolderPath => _command.FolderPath;
        public string? Description => _command.Description;
        public string? Arguments => _command.Arguments;

        public bool IsExecuting
        {
            get => _isExecuting;
            set
            {
                _isExecuting = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanExecute));
                OnPropertyChanged(nameof(CanStop));
            }
        }

        public string Output
        {
            get => _output;
            set
            {
                _output = value;
                OnPropertyChanged();
            }
        }

        public bool CanExecute => !IsExecuting;
        public bool CanStop => IsExecuting;

        public Process? CurrentProcess
        {
            get => _currentProcess;
            set => _currentProcess = value;
        }

        public ICommand ClearOutputCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

