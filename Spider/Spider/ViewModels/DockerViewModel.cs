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
using System.Text.RegularExpressions;

namespace Spider.ViewModels
{
    /// <summary>
    /// ViewModel для вкладки "Docker"
    /// </summary>
    public class DockerViewModel : INotifyPropertyChanged
    {
        private readonly DockerService _dockerService;
        private ObservableCollection<DockerProject> _projects;
        private ObservableCollection<DockerImageViewModel> _images;
        private DockerProject? _selectedProject;
        private DockerImageViewModel? _selectedImage;
        private string _buildOutput = string.Empty;
        private bool _isLoading;
        private bool _isBuilding;
        private Process? _currentProcess;

        #region Свойства

        /// <summary>
        /// Коллекция всех Docker проектов
        /// </summary>
        public ObservableCollection<DockerProject> Projects
        {
            get => _projects;
            set => SetProperty(ref _projects, value);
        }

        /// <summary>
        /// Коллекция образов выбранного проекта
        /// </summary>
        public ObservableCollection<DockerImageViewModel> Images
        {
            get => _images;
            set => SetProperty(ref _images, value);
        }

        /// <summary>
        /// Выбранный проект
        /// </summary>
        public DockerProject? SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (SetProperty(ref _selectedProject, value))
                {
                    OnPropertyChanged(nameof(SelectedProjectName));
                    OnPropertyChanged(nameof(IsProjectSelected));
                    _ = LoadProjectImagesAsync();
                }
            }
        }

        /// <summary>
        /// Выбранный образ
        /// </summary>
        public DockerImageViewModel? SelectedImage
        {
            get => _selectedImage;
            set => SetProperty(ref _selectedImage, value);
        }

        /// <summary>
        /// Вывод билда/логов
        /// </summary>
        public string BuildOutput
        {
            get => _buildOutput;
            set => SetProperty(ref _buildOutput, value);
        }

        /// <summary>
        /// Название выбранного проекта (для отображения)
        /// </summary>
        public string SelectedProjectName => SelectedProject?.Name ?? "(выберите проект)";

        /// <summary>
        /// Флаг: выбран ли проект
        /// </summary>
        public bool IsProjectSelected => SelectedProject != null;

        /// <summary>
        /// Флаг загрузки данных
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// Флаг выполнения операции Docker
        /// </summary>
        public bool IsBuilding
        {
            get => _isBuilding;
            set
            {
                if (SetProperty(ref _isBuilding, value))
                {
                    OnPropertyChanged(nameof(CanExecuteDockerCommands));
                }
            }
        }

        /// <summary>
        /// Можно ли выполнять Docker команды
        /// </summary>
        public bool CanExecuteDockerCommands => !IsBuilding && IsProjectSelected;

        #endregion

        #region Команды

        public ICommand LoadProjectsCommand { get; }
        public ICommand AddProjectCommand { get; }
        public ICommand EditProjectCommand { get; }
        public ICommand DeleteProjectCommand { get; }
        public ICommand RefreshImagesCommand { get; }
        public ICommand StartImageCommand { get; }
        public ICommand StopImageCommand { get; }
        public ICommand StartAllImagesCommand { get; }
        public ICommand StopAllImagesCommand { get; }
        public ICommand RebuildImageCommand { get; }
        public ICommand ClearOutputCommand { get; }

        #endregion

        #region Конструктор

        public DockerViewModel()
        {
            _dockerService = new DockerService();
            _projects = new ObservableCollection<DockerProject>();
            _images = new ObservableCollection<DockerImageViewModel>();

            LoadProjectsCommand = new RelayCommand(async _ => await LoadProjectsAsync());
            AddProjectCommand = new RelayCommand(_ => AddProject());
            EditProjectCommand = new RelayCommand(param => EditProject(param as DockerProject));
            DeleteProjectCommand = new RelayCommand(param => DeleteProject(param as DockerProject));
            RefreshImagesCommand = new RelayCommand(async _ => await LoadProjectImagesAsync(), _ => IsProjectSelected);
            StartImageCommand = new RelayCommand(async param => await StartImageAsync(param as DockerImageViewModel), _ => CanExecuteDockerCommands);
            StopImageCommand = new RelayCommand(async param => await StopImageAsync(param as DockerImageViewModel), _ => CanExecuteDockerCommands);
            StartAllImagesCommand = new RelayCommand(async _ => await StartAllImagesAsync(), _ => CanExecuteDockerCommands);
            StopAllImagesCommand = new RelayCommand(async _ => await StopAllImagesAsync(), _ => CanExecuteDockerCommands);
            RebuildImageCommand = new RelayCommand(async param => await RebuildImageAsync(param as DockerImageViewModel), _ => CanExecuteDockerCommands);
            ClearOutputCommand = new RelayCommand(_ => BuildOutput = string.Empty);

            _ = LoadProjectsAsync();
        }

        #endregion

        #region Методы загрузки данных

        /// <summary>
        /// Загрузка всех Docker проектов
        /// </summary>
        private async Task LoadProjectsAsync()
        {
            try
            {
                IsLoading = true;
                var projects = await _dockerService.GetDockerProjectsAsync();

                Projects.Clear();
                foreach (var project in projects)
                {
                    Projects.Add(project);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки проектов: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки проектов:\n{ex.Message}",
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
        /// Загрузка образов выбранного проекта
        /// </summary>
        private async Task LoadProjectImagesAsync()
        {
            Images.Clear();
            BuildOutput = string.Empty;

            if (SelectedProject == null) return;

            try
            {
                IsLoading = true;

                if (!System.IO.File.Exists(SelectedProject.DockerComposePath))
                {
                    MessageBox.Show($"Файл docker-compose не найден:\n{SelectedProject.DockerComposePath}",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    return;
                }

                var services = await ParseDockerComposeServicesAsync(SelectedProject.DockerComposePath);

                var runningContainers = await GetRunningContainersAsync(SelectedProject.DockerComposePath);

                foreach (var service in services)
                {
                    var imageViewModel = new DockerImageViewModel
                    {
                        ServiceName = service,
                        IsRunning = runningContainers.Contains(service),
                        ProjectId = SelectedProject.Id
                    };
                    Images.Add(imageViewModel);
                }

                if (Images.Count == 0)
                {
                    BuildOutput = "⚠️ В docker-compose файле не найдено сервисов.\n";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки образов: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки образов:\n{ex.Message}",
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
        /// Парсинг сервисов из docker-compose файла
        /// </summary>
        private async Task<List<string>> ParseDockerComposeServicesAsync(string composePath)
        {
            var services = new List<string>();
            
            try
            {
                var content = await System.IO.File.ReadAllTextAsync(composePath);
                
                var lines = content.Split('\n');
                bool inServices = false;
                
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    
                    if (trimmed.StartsWith("services:"))
                    {
                        inServices = true;
                        continue;
                    }
                    
                    if (inServices)
                    {
                        if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith(" ") && !line.StartsWith("\t"))
                        {
                            break;
                        }
                        
                        if (line.StartsWith("  ") && !line.StartsWith("    ") && trimmed.EndsWith(":"))
                        {
                            var serviceName = trimmed.TrimEnd(':');
                            if (!string.IsNullOrWhiteSpace(serviceName) && !serviceName.Contains(" "))
                            {
                                services.Add(serviceName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка парсинга docker-compose: {ex.Message}");
            }
            
            return services;
        }

        /// <summary>
        /// Получение списка запущенных контейнеров
        /// </summary>
        private async Task<HashSet<string>> GetRunningContainersAsync(string composePath)
        {
            var runningContainers = new HashSet<string>();
            
            try
            {
                var workDir = System.IO.Path.GetDirectoryName(composePath);
                if (string.IsNullOrEmpty(workDir)) return runningContainers;

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "docker-compose",
                        Arguments = "ps --services --filter \"status=running\"",
                        WorkingDirectory = workDir,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var service = line.Trim();
                    if (!string.IsNullOrWhiteSpace(service))
                    {
                        runningContainers.Add(service);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения статуса контейнеров: {ex.Message}");
            }
            
            return runningContainers;
        }

        #endregion

        #region Методы CRUD операций для проектов

        private void AddProject()
        {
            var dialog = new DockerProjectDialog();
            if (dialog.ShowDialog() == true)
            {
                _ = AddProjectAsync(dialog.Project);
            }
        }

        private async Task AddProjectAsync(DockerProject project)
        {
            try
            {
                await _dockerService.AddDockerProjectAsync(project);
                await LoadProjectsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении проекта:\n{ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void EditProject(DockerProject? project = null)
        {
            var projectToEdit = project ?? SelectedProject;

            if (projectToEdit == null)
            {
                MessageBox.Show("Выберите проект для редактирования!",
                              "Внимание",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
                return;
            }

            var dialog = new DockerProjectDialog(projectToEdit);
            if (dialog.ShowDialog() == true)
            {
                _ = UpdateProjectAsync(dialog.Project);
            }
        }

        private async void DeleteProject(DockerProject? project = null)
        {
            var projectToDelete = project ?? SelectedProject;

            if (projectToDelete == null)
            {
                MessageBox.Show("Выберите проект для удаления!",
                              "Внимание",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить проект '{projectToDelete.Name}'?",
                               "Подтверждение удаления",
                               MessageBoxButton.YesNo,
                               MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _dockerService.DeleteDockerProjectAsync(projectToDelete.Id);
                    await LoadProjectsAsync();
                    if (SelectedProject == projectToDelete)
                        SelectedProject = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении проекта:\n{ex.Message}",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }

        private async Task UpdateProjectAsync(DockerProject project)
        {
            try
            {
                await _dockerService.UpdateDockerProjectAsync(project);
                await LoadProjectsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении проекта:\n{ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        #endregion

        #region Методы Docker операций

        /// <summary>
        /// Запуск отдельного образа
        /// </summary>
        private async Task StartImageAsync(DockerImageViewModel? image)
        {
            if (image == null || SelectedProject == null) return;

            await ExecuteDockerCommandAsync(
                "up -d",
                image.ServiceName,
                $"Запуск сервиса: {image.ServiceName}"
            );
        }

        /// <summary>
        /// Остановка отдельного образа
        /// </summary>
        private async Task StopImageAsync(DockerImageViewModel? image)
        {
            if (image == null || SelectedProject == null) return;

            await ExecuteDockerCommandAsync(
                "stop",
                image.ServiceName,
                $"Остановка сервиса: {image.ServiceName}"
            );
        }

        /// <summary>
        /// Запуск всех образов
        /// </summary>
        private async Task StartAllImagesAsync()
        {
            if (SelectedProject == null) return;

            await ExecuteDockerCommandAsync(
                "up -d",
                "",
                "Запуск всех сервисов"
            );
        }

        /// <summary>
        /// Остановка всех образов
        /// </summary>
        private async Task StopAllImagesAsync()
        {
            if (SelectedProject == null) return;

            await ExecuteDockerCommandAsync(
                "down",
                "",
                "Остановка всех сервисов"
            );
        }

        /// <summary>
        /// Пересборка и запуск образа
        /// </summary>
        private async Task RebuildImageAsync(DockerImageViewModel? image)
        {
            if (image == null || SelectedProject == null) return;

            await ExecuteDockerCommandAsync(
                "up -d --build",
                image.ServiceName,
                $"Пересборка и запуск сервиса: {image.ServiceName}"
            );
        }

        /// <summary>
        /// Выполнение Docker команды
        /// </summary>
        private async Task ExecuteDockerCommandAsync(string command, string serviceName, string description)
        {
            if (SelectedProject == null) return;

            try
            {
                IsBuilding = true;

                var workDir = System.IO.Path.GetDirectoryName(SelectedProject.DockerComposePath);
                if (string.IsNullOrEmpty(workDir))
                {
                    BuildOutput += $"❌ ОШИБКА: Не удалось определить рабочую директорию\n\n";
                    return;
                }

                var fullCommand = $"docker-compose {command} {serviceName}".Trim();

                BuildOutput += $"═══════════════════════════════════════════════════\n";
                BuildOutput += $"🐳 {description}\n";
                BuildOutput += $"📁 Директория: {workDir}\n";
                BuildOutput += $"⚡ Команда: {fullCommand}\n";
                BuildOutput += $"═══════════════════════════════════════════════════\n\n";

                _currentProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "docker-compose",
                        Arguments = $"{command} {serviceName}".Trim(),
                        WorkingDirectory = workDir,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                _currentProcess.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            BuildOutput += e.Data + "\n";
                        });
                    }
                };

                _currentProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            BuildOutput += e.Data + "\n";
                        });
                    }
                };

                _currentProcess.Start();
                _currentProcess.BeginOutputReadLine();
                _currentProcess.BeginErrorReadLine();

                await Task.Run(() => _currentProcess.WaitForExit());

                var exitCode = _currentProcess.ExitCode;
                BuildOutput += $"\n═══════════════════════════════════════════════════\n";
                if (exitCode == 0)
                {
                    BuildOutput += $"✅ Операция выполнена успешно (код: {exitCode})\n";
                }
                else
                {
                    BuildOutput += $"❌ Операция завершена с ошибкой (код: {exitCode})\n";
                }
                BuildOutput += $"═══════════════════════════════════════════════════\n\n";

                await Task.Delay(1000);
                await LoadProjectImagesAsync();
            }
            catch (Exception ex)
            {
                BuildOutput += $"\n❌ ОШИБКА ВЫПОЛНЕНИЯ:\n{ex.Message}\n\n";
            }
            finally
            {
                _currentProcess?.Dispose();
                _currentProcess = null;
                IsBuilding = false;
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
            _currentProcess?.Kill(true);
            _currentProcess?.Dispose();
            _dockerService?.Dispose();
        }

        #endregion
    }

    /// <summary>
    /// ViewModel для отображения Docker образа с динамическим статусом
    /// </summary>
    public class DockerImageViewModel : INotifyPropertyChanged
    {
        private bool _isRunning;

        public int ProjectId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        public string Status => IsRunning ? "🟢 Запущен" : "🔴 Остановлен";
        public string StatusColor => IsRunning ? "#4CAF50" : "#F44336";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

