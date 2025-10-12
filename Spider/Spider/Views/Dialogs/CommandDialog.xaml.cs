using Spider.Models;
using System.Windows;
using Microsoft.Win32;

namespace Spider.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for CommandDialog.xaml
    /// </summary>
    public partial class CommandDialog : Window
    {
        public Command Command { get; private set; }
        public bool IsEditMode { get; private set; }
        public string DialogTitle => IsEditMode ? "Редактировать команду" : "Новая команда";
        public string OkButtonText => IsEditMode ? "Сохранить" : "Добавить";

        public CommandDialog(Command? command = null)
        {
            InitializeComponent();

            IsEditMode = command != null;
            Command = command ?? new Command();

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

        private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Выберите рабочую директорию для команды",
                ShowNewFolderButton = true
            };

            // Если путь уже указан, устанавливаем его как начальный
            if (!string.IsNullOrWhiteSpace(Command.FolderPath) && System.IO.Directory.Exists(Command.FolderPath))
            {
                dialog.SelectedPath = Command.FolderPath;
            }

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Command.FolderPath = dialog.SelectedPath;
                FolderPathTextBox.Text = dialog.SelectedPath;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем валидность данных
            if (string.IsNullOrWhiteSpace(Command.Name))
            {
                MessageBox.Show("Название команды обязательно для заполнения!",
                              "Ошибка валидации",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            // Проверяем длину названия
            if (Command.Name.Length < 2 || Command.Name.Length > 100)
            {
                MessageBox.Show("Название команды должно содержать от 2 до 100 символов!",
                              "Ошибка валидации",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            // Проверяем рабочую директорию
            if (string.IsNullOrWhiteSpace(Command.FolderPath))
            {
                MessageBox.Show("Рабочая директория обязательна для заполнения!",
                              "Ошибка валидации",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                FolderPathTextBox.Focus();
                return;
            }

            // Проверяем существование директории
            if (!System.IO.Directory.Exists(Command.FolderPath))
            {
                var result = MessageBox.Show($"Директория '{Command.FolderPath}' не существует.\n\nВсё равно сохранить?",
                                           "Предупреждение",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    FolderPathTextBox.Focus();
                    return;
                }
            }

            // Проверяем команду
            if (string.IsNullOrWhiteSpace(Command.CommandText))
            {
                MessageBox.Show("Команда обязательна для заполнения!",
                              "Ошибка валидации",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                CommandTextBox.Focus();
                return;
            }

            // Проверяем длину команды
            if (Command.CommandText.Length < 1 || Command.CommandText.Length > 1000)
            {
                MessageBox.Show("Команда должна содержать от 1 до 1000 символов!",
                              "Ошибка валидации",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                CommandTextBox.Focus();
                return;
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

