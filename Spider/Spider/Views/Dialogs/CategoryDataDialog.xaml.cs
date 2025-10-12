using Spider.Models;
using System.Windows;

namespace Spider.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for CategoryDataDialog.xaml
    /// </summary>
    public partial class CategoryDataDialog : Window
    {
        public CategoryData CategoryData { get; private set; }
        public bool IsEditMode { get; private set; }
        public string DialogTitle => IsEditMode ? "Редактировать данные" : "Новые данные";
        public string OkButtonText => IsEditMode ? "Сохранить" : "Добавить";

        public CategoryDataDialog(Category category, CategoryData? categoryData = null)
        {
            InitializeComponent();
            
            IsEditMode = categoryData != null;
            CategoryData = categoryData ?? new CategoryData { CategoryId = category.Id };
            
            DataContext = this;
            NameTextBox.Focus();
            
            if (IsEditMode)
            {
                NameTextBox.SelectAll();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CategoryData.Name))
            {
                MessageBox.Show("Название обязательно для заполнения!", 
                              "Ошибка валидации", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(CategoryData.Value))
            {
                MessageBox.Show("Значение обязательно для заполнения!", 
                              "Ошибка валидации", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
                ValueTextBox.Focus();
                return;
            }

            if (CategoryData.Name.Length < 1 || CategoryData.Name.Length > 100)
            {
                MessageBox.Show("Название должно содержать от 1 до 100 символов!", 
                              "Ошибка валидации", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            if (CategoryData.Value.Length > 1000)
            {
                MessageBox.Show("Значение не должно превышать 1000 символов!", 
                              "Ошибка валидации", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
                ValueTextBox.Focus();
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
