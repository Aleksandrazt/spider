using Spider.Models;
using System.Windows;
using WpfOpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Spider.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for DockerProjectDialog.xaml
    /// </summary>
    public partial class DockerProjectDialog : Window
    {
        public DockerProject Project { get; private set; }
        public bool IsEditMode { get; private set; }
        public string DialogTitle => IsEditMode ? "Редактировать Docker проект" : "Новый Docker проект";
        public string OkButtonText => IsEditMode ? "Сохранить" : "Добавить";

        public DockerProjectDialog(DockerProject? project = null)
        {
            InitializeComponent();

            IsEditMode = project != null;
            Project = project ?? new DockerProject();

            // Устанавливаем DataContext на сам диалог для доступа к свойствам
            DataContext = this;

            // Устанавливаем заголовок и текст кнопки
            TitleTextBlock.Text = DialogTitle;
            OkButton.Content = OkButtonText;

            NameTextBox.Focus();

            if (IsEditMode)
            {
                NameTextBox.SelectAll();
            }
        }

        private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new WpfOpenFileDialog
            {
                Title = "Выберите docker-compose файл",
                Filter = "Docker Compose файлы (*.yml;*.yaml)|*.yml;*.yaml|Все файлы (*.*)|*.*",
                CheckFileExists = true
            };

            // Если путь уже указан, устанавливаем его как начальный
            if (!string.IsNullOrWhiteSpace(Project.DockerComposePath) && System.IO.File.Exists(Project.DockerComposePath))
            {
                dialog.InitialDirectory = System.IO.Path.GetDirectoryName(Project.DockerComposePath);
                dialog.FileName = System.IO.Path.GetFileName(Project.DockerComposePath);
            }

            if (dialog.ShowDialog() == true)
            {
                Project.DockerComposePath = dialog.FileName;
                DockerComposePathTextBox.Text = dialog.FileName;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем валидность данных
            if (string.IsNullOrWhiteSpace(Project.Name))
            {
                MessageBox.Show("Название проекта обязательно для заполнения!",
                              "Ошибка валидации",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            // Проверяем длину названия
            if (Project.Name.Length < 2 || Project.Name.Length > 50)
            {
                MessageBox.Show("Название проекта должно содержать от 2 до 50 символов!",
                              "Ошибка валидации",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            // Проверяем путь к docker-compose файлу
            if (string.IsNullOrWhiteSpace(Project.DockerComposePath))
            {
                MessageBox.Show("Путь к docker-compose файлу обязателен для заполнения!",
                              "Ошибка валидации",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                DockerComposePathTextBox.Focus();
                return;
            }

            // Проверяем существование файла
            if (!System.IO.File.Exists(Project.DockerComposePath))
            {
                var result = MessageBox.Show($"Файл '{Project.DockerComposePath}' не существует.\n\nВсё равно сохранить?",
                                           "Предупреждение",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    DockerComposePathTextBox.Focus();
                    return;
                }
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

