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
            
            // Привязываем команды к кнопкам в ListBox (категории)
            // Команды уже привязаны через XAML с RelativeSource
            
            // Привязываем команды к кнопкам в DataGrid (данные категорий)
            // Команды уже привязаны через XAML с RelativeSource
        }

        protected override void OnClosed(EventArgs e)
        {
            // Освобождаем ресурсы при закрытии окна
            _categoriesViewModel?.Dispose();
            base.OnClosed(e);
        }
    }
}