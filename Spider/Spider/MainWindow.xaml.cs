using Spider.ViewModels;
using System.Windows;

namespace Spider
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly CategoriesViewModel _categoriesViewModel;

        public MainWindow()
        {
            InitializeComponent();

            // Создаем ViewModel для категорий
            _categoriesViewModel = new CategoriesViewModel();

            // Устанавливаем DataContext для привязки данных
            DataContext = _categoriesViewModel;

            // Привязываем коллекции к UI элементам
            CategoriesListBox.ItemsSource = _categoriesViewModel.Categories;
            CategoryDataGrid.ItemsSource = _categoriesViewModel.CategoryData;

            // Привязываем выбранные элементы
            CategoriesListBox.SelectionChanged += (s, e) =>
            {
                _categoriesViewModel.SelectedCategory = CategoriesListBox.SelectedItem as Models.Category;
            };

            CategoryDataGrid.SelectionChanged += (s, e) =>
            {
                _categoriesViewModel.SelectedCategoryData = CategoryDataGrid.SelectedItem as Models.CategoryData;
            };

            // Привязываем команды к кнопкам
            AddCategoryButton.Command = _categoriesViewModel.AddCategoryCommand;
            AddCategoryDataButton.Command = _categoriesViewModel.AddCategoryDataCommand;
        }

        protected override void OnClosed(EventArgs e)
        {
            // Освобождаем ресурсы при закрытии окна
            _categoriesViewModel?.Dispose();
            base.OnClosed(e);
        }
    }
}