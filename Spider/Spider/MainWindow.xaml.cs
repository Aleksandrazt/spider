using Spider.ViewModels;
using System.Windows;
using System.Windows.Controls;

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

        public MainWindow()
        {
            InitializeComponent();

            _categoriesViewModel = new CategoriesViewModel();
            _commandsViewModel = new CommandsViewModel();
            _dockerViewModel = new DockerViewModel();
            _screenshotViewModel = new ScreenshotViewModel();

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
                var selectedCommand = CommandsListBox.SelectedItem as Models.Command;
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
        }

        private void SaveScreenshotSettings_Click(object sender, RoutedEventArgs e)
        {
            _screenshotViewModel.SaveSettingsCommand.Execute(null);
        }

        protected override void OnClosed(EventArgs e)
        {
            _categoriesViewModel?.Dispose();
            _commandsViewModel?.Dispose();
            _dockerViewModel?.Dispose();
            _screenshotViewModel?.Dispose();
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