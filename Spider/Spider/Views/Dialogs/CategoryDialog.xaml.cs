using Spider.Models;
using System.Windows;

namespace Spider.Views.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для CategoryDialog.xaml
    /// </summary>
    public partial class CategoryDialog : Window
    {
        public Category Category { get; private set; }
        public bool IsEditMode { get; private set; }
        public string DialogTitle => IsEditMode ? "Редактировать категорию" : "Новая категория";
        public string OkButtonText => IsEditMode ? "Сохранить" : "Добавить";

        public CategoryDialog(Category? category = null)
        {
            InitializeComponent();
            
            IsEditMode = category != null;
            Category = category ?? new Category();
            
            DataContext = this;
            
            TitleTextBlock.Text = DialogTitle;
            OkButton.Content = OkButtonText;
            
            NameTextBox.Focus();
            
            if (IsEditMode)
            {
                NameTextBox.SelectAll();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Category.Name))
            {
                MessageBox.Show("Название категории обязательно для заполнения!",
                              "Ошибка валидации",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            if (Category.Name.Length < 2 || Category.Name.Length > 50)
            {
                MessageBox.Show("Название категории должно содержать от 2 до 50 символов!",
                              "Ошибка валидации",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                NameTextBox.Focus();
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