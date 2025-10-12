using Spider.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Spider
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly CategoriesViewModel _categoriesViewModel;
        private readonly CommandsViewModel _commandsViewModel;
        private readonly DockerViewModel _dockerViewModel;

        public MainWindow()
        {
            InitializeComponent();

            // Создаем ViewModel для категорий
            _categoriesViewModel = new CategoriesViewModel();

            // Создаем ViewModel для команд
            _commandsViewModel = new CommandsViewModel();
            
            // Создаем ViewModel для Docker
            _dockerViewModel = new DockerViewModel();

            // Устанавливаем DataContext для привязки данных (для категорий через XAML)
            DataContext = _categoriesViewModel;

            #region Инициализация вкладки Категории

            // Привязываем коллекции к UI элементам
            CategoriesListBox.ItemsSource = _categoriesViewModel.Categories;
            CategoryDataGrid.ItemsSource = _categoriesViewModel.CategoryData;

            // Привязываем выбранные элементы
            CategoriesListBox.SelectionChanged += (s, e) =>
            {
                var selectedCategory = CategoriesListBox.SelectedItem as Models.Category;
                _categoriesViewModel.SelectedCategory = selectedCategory;
                
                // Управляем видимостью элементов в правой панели
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

            // Привязываем команды к кнопкам
            AddCategoryButton.Command = _categoriesViewModel.AddCategoryCommand;
            AddCategoryDataButton.Command = _categoriesViewModel.AddCategoryDataCommand;
            
            // Инициализируем состояние кнопки "Добавить данные"
            AddCategoryDataButton.Visibility = Visibility.Collapsed;

            #endregion

            #region Инициализация вкладки Команды

            // Устанавливаем DataContext для вкладки Команды
            // Для команд в ListBox используется RelativeSource, поэтому нужно установить DataContext на Window
            // Но так как у нас уже установлен DataContext для категорий, нам нужен другой подход

            // Привязываем коллекции к UI элементам
            CommandsListBox.ItemsSource = _commandsViewModel.Commands;

            // Привязываем выбранную команду
            CommandsListBox.SelectionChanged += (s, e) =>
            {
                var selectedCommand = CommandsListBox.SelectedItem as Models.Command;
                _commandsViewModel.SelectedCommand = selectedCommand;
                
                // Управляем видимостью элементов в правой панели
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

            // Привязываем поле дополнительных аргументов
            AdditionalArgumentsTextBox.TextChanged += (s, e) =>
            {
                _commandsViewModel.AdditionalArguments = AdditionalArgumentsTextBox.Text;
            };

            // Привязываем вывод команды
            _commandsViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_commandsViewModel.CommandOutput))
                {
                    CommandOutputTextBox.Text = _commandsViewModel.CommandOutput;
                    // Автоматически прокручиваем вниз
                    CommandOutputTextBox.ScrollToEnd();
                }
                else if (e.PropertyName == nameof(_commandsViewModel.IsExecuting))
                {
                    // Управляем доступностью кнопок
                    ExecuteCommandButton.IsEnabled = _commandsViewModel.CanExecute;
                    StopCommandButton.IsEnabled = _commandsViewModel.CanStop;
                }
                else if (e.PropertyName == nameof(_commandsViewModel.AdditionalArguments))
                {
                    // Обновляем текст в TextBox, если он изменился в ViewModel
                    if (AdditionalArgumentsTextBox.Text != _commandsViewModel.AdditionalArguments)
                    {
                        AdditionalArgumentsTextBox.Text = _commandsViewModel.AdditionalArguments;
                    }
                }
            };

            // Привязываем команды к кнопкам
            AddCommandButton.Command = _commandsViewModel.AddCommandCommand;
            ExecuteCommandButton.Command = _commandsViewModel.ExecuteCommandCommand;
            StopCommandButton.Command = _commandsViewModel.StopCommandCommand;
            ClearOutputButton.Command = _commandsViewModel.ClearOutputCommand;

            // Инициализируем состояние панели управления
            CommandControlPanel.Visibility = Visibility.Collapsed;
            OutputArea.Visibility = Visibility.Collapsed;

            // Для команд редактирования и удаления нужно использовать обработчики событий
            // так как DataContext у Window установлен на _categoriesViewModel
            // Создаем временный класс для хранения всех ViewModel
            var combinedDataContext = new CombinedViewModel
            {
                CategoriesViewModel = _categoriesViewModel,
                CommandsViewModel = _commandsViewModel,
                DockerViewModel = _dockerViewModel
            };
            
            // Обновляем DataContext
            DataContext = combinedDataContext;

            #endregion

            #region Инициализация вкладки Docker

            // Привязываем коллекции к UI элементам
            DockerProjectsListBox.ItemsSource = _dockerViewModel.Projects;
            DockerImagesDataGrid.ItemsSource = _dockerViewModel.Images;

            // Привязываем выбранный проект
            DockerProjectsListBox.SelectionChanged += (s, e) =>
            {
                var selectedProject = DockerProjectsListBox.SelectedItem as Models.DockerProject;
                _dockerViewModel.SelectedProject = selectedProject;
                
                // Управляем видимостью элементов в правой панели
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

            // Привязываем вывод билда
            _dockerViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_dockerViewModel.BuildOutput))
                {
                    DockerBuildOutputTextBox.Text = _dockerViewModel.BuildOutput;
                    DockerBuildOutputTextBox.ScrollToEnd();
                }
            };

            // Привязываем команды к кнопкам
            AddDockerProjectButton.Command = _dockerViewModel.AddProjectCommand;
            RefreshDockerImagesButton.Command = _dockerViewModel.RefreshImagesCommand;
            StartAllDockerImagesButton.Command = _dockerViewModel.StartAllImagesCommand;
            StopAllDockerImagesButton.Command = _dockerViewModel.StopAllImagesCommand;
            ClearDockerOutputButton.Command = _dockerViewModel.ClearOutputCommand;

            // Инициализируем состояние панели управления
            DockerControlPanel.Visibility = Visibility.Collapsed;
            DockerImagesDataGrid.Visibility = Visibility.Collapsed;

            #endregion
        }

        protected override void OnClosed(EventArgs e)
        {
            // Освобождаем ресурсы при закрытии окна
            _categoriesViewModel?.Dispose();
            _commandsViewModel?.Dispose();
            _dockerViewModel?.Dispose();
            base.OnClosed(e);
        }

        // Вспомогательный класс для объединения ViewModel
        private class CombinedViewModel
        {
            public CategoriesViewModel? CategoriesViewModel { get; set; }
            public CommandsViewModel? CommandsViewModel { get; set; }
            public DockerViewModel? DockerViewModel { get; set; }
            
            // Прокси свойства для категорий
            public System.Windows.Input.ICommand? EditCategoryCommand => CategoriesViewModel?.EditCategoryCommand;
            public System.Windows.Input.ICommand? DeleteCategoryCommand => CategoriesViewModel?.DeleteCategoryCommand;
            public System.Windows.Input.ICommand? EditCategoryDataCommand => CategoriesViewModel?.EditCategoryDataCommand;
            public System.Windows.Input.ICommand? DeleteCategoryDataCommand => CategoriesViewModel?.DeleteCategoryDataCommand;
            
            // Прокси свойства для команд
            public System.Windows.Input.ICommand? EditCommandCommand => CommandsViewModel?.EditCommandCommand;
            public System.Windows.Input.ICommand? DeleteCommandCommand => CommandsViewModel?.DeleteCommandCommand;
            
            // Прокси свойства для Docker
            public System.Windows.Input.ICommand? EditDockerProjectCommand => DockerViewModel?.EditProjectCommand;
            public System.Windows.Input.ICommand? DeleteDockerProjectCommand => DockerViewModel?.DeleteProjectCommand;
            public System.Windows.Input.ICommand? StartDockerImageCommand => DockerViewModel?.StartImageCommand;
            public System.Windows.Input.ICommand? StopDockerImageCommand => DockerViewModel?.StopImageCommand;
            public System.Windows.Input.ICommand? RebuildDockerImageCommand => DockerViewModel?.RebuildImageCommand;
        }
    }
}