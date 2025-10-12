using Spider.Models;
using System.Windows;
using Microsoft.Win32;

namespace Spider.Views.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для CommandDialog.xaml
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

            DataContext = this;

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
            if (string.IsNullOrWhiteSpace(Command.Name))
            {
                MessageBox.Show("Название команды обязательно для заполнения!",
                              "Ошибка валидации",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            if (Command.Name.Length < 2 || Command.Name.Length > 100)
            {
                MessageBox.Show("Название команды должно содержать от 2 до 100 символов!",
                              "Ошибка валидации",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(Command.FolderPath))
            {
                MessageBox.Show("Рабочая директория обязательна для заполнения!",
                              "Ошибка валидации",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                FolderPathTextBox.Focus();
                return;
            }

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

            if (string.IsNullOrWhiteSpace(Command.CommandText))
            {
                MessageBox.Show("Команда обязательна для заполнения!",
                              "Ошибка валидации",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                CommandTextBox.Focus();
                return;
            }

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

