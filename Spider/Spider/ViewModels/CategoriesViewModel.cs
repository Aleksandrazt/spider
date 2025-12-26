using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Spider.Helpers;
using Spider.Models;
using Spider.Services;
using Spider.Views.Dialogs;

namespace Spider.ViewModels
{
    /// <summary>
    /// ViewModel для вкладки "Категории"
    /// </summary>
    public class CategoriesViewModel : INotifyPropertyChanged
    {
        private readonly CategoryService _categoryService;
        private ObservableCollection<Category> _categories;
        private ObservableCollection<CategoryData> _categoryData;
        private Category? _selectedCategory;
        private CategoryData? _selectedCategoryData;
        private bool _isLoading;

        #region Свойства

        /// <summary>
        /// Коллекция всех категорий
        /// </summary>
        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        /// <summary>
        /// Коллекция данных выбранной категории
        /// </summary>
        public ObservableCollection<CategoryData> CategoryData
        {
            get => _categoryData;
            set => SetProperty(ref _categoryData, value);
        }

        /// <summary>
        /// Выбранная категория
        /// </summary>
        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    OnPropertyChanged(nameof(SelectedCategoryName));
                    _ = LoadCategoryDataAsync();
                }
            }
        }

        /// <summary>
        /// Выбранные данные категории
        /// </summary>
        public CategoryData? SelectedCategoryData
        {
            get => _selectedCategoryData;
            set => SetProperty(ref _selectedCategoryData, value);
        }

        /// <summary>
        /// Название выбранной категории (для отображения)
        /// </summary>
        public string SelectedCategoryName => SelectedCategory?.Name ?? "(выберите категорию)";

        /// <summary>
        /// Флаг загрузки данных
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        #endregion

        #region Команды

        /// <summary>
        /// Команда загрузки категорий
        /// </summary>
        public ICommand LoadCategoriesCommand { get; }

        /// <summary>
        /// Команда добавления новой категории
        /// </summary>
        public ICommand AddCategoryCommand { get; }

        /// <summary>
        /// Команда редактирования категории
        /// </summary>
        public ICommand EditCategoryCommand { get; }

        /// <summary>
        /// Команда удаления категории
        /// </summary>
        public ICommand DeleteCategoryCommand { get; }

        /// <summary>
        /// Команда добавления данных категории
        /// </summary>
        public ICommand AddCategoryDataCommand { get; }

        /// <summary>
        /// Команда редактирования данных категории
        /// </summary>
        public ICommand EditCategoryDataCommand { get; }

        /// <summary>
        /// Команда удаления данных категории
        /// </summary>
        public ICommand DeleteCategoryDataCommand { get; }

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор ViewModel
        /// </summary>
        public CategoriesViewModel()
        {
            _categoryService = new CategoryService();
            _categories = new ObservableCollection<Category>();
            _categoryData = new ObservableCollection<CategoryData>();

            // Инициализация команд
            LoadCategoriesCommand = new RelayCommand(async _ => await LoadCategoriesAsync());
            AddCategoryCommand = new RelayCommand(_ => AddCategory());
            EditCategoryCommand = new RelayCommand(param => EditCategory(param as Category));
            DeleteCategoryCommand = new RelayCommand(param => DeleteCategory(param as Category));
            AddCategoryDataCommand = new RelayCommand(_ => AddCategoryData(), _ => SelectedCategory != null);
            EditCategoryDataCommand = new RelayCommand(param => EditCategoryData(param as CategoryData));
            DeleteCategoryDataCommand = new RelayCommand(param => DeleteCategoryData(param as CategoryData));

            // Загружаем категории при создании ViewModel
            _ = LoadCategoriesAsync();
        }

        #endregion

        #region Методы загрузки данных

        /// <summary>
        /// Загрузка всех категорий
        /// </summary>
        public async Task LoadCategoriesAsync()
        {
            try
            {
                IsLoading = true;
                var categories = await _categoryService.GetCategoriesAsync();

                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки категорий: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Загрузка данных выбранной категории
        /// </summary>
        private async Task LoadCategoryDataAsync()
        {
            if (SelectedCategory == null)
            {
                CategoryData.Clear();
                return;
            }

            try
            {
                IsLoading = true;
                var data = await _categoryService.GetCategoryDataAsync(SelectedCategory.Id);

                CategoryData.Clear();
                foreach (var item in data)
                {
                    CategoryData.Add(item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки данных категории: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Методы CRUD операций

        /// <summary>
        /// Добавление новой категории
        /// </summary>
        private void AddCategory()
        {
            var dialog = new CategoryDialog();
            if (dialog.ShowDialog() == true)
            {
                _ = AddCategoryAsync(dialog.Category);
            }
        }

        /// <summary>
        /// Асинхронное добавление категории
        /// </summary>
        private async Task AddCategoryAsync(Category category)
        {
            try
            {
                await _categoryService.AddCategoryAsync(category);
                await LoadCategoriesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении категории:\n{ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Редактирование выбранной категории
        /// </summary>
        private void EditCategory(Category? category = null)
        {
            var categoryToEdit = category ?? SelectedCategory;
            
            if (categoryToEdit == null)
            {
                MessageBox.Show("Выберите категорию для редактирования!", 
                              "Внимание", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
                return;
            }

            var dialog = new CategoryDialog(categoryToEdit);
            if (dialog.ShowDialog() == true)
            {
                _ = UpdateCategoryAsync(dialog.Category);
            }
        }

        /// <summary>
        /// Удаление выбранной категории
        /// </summary>
        private async void DeleteCategory(Category? category = null)
        {
            var categoryToDelete = category ?? SelectedCategory;
            
            if (categoryToDelete == null)
            {
                MessageBox.Show("Выберите категорию для удаления!", 
                              "Внимание", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить категорию '{categoryToDelete.Name}'?\n\nВсе данные в этой категории также будут удалены!", 
                               "Подтверждение удаления", 
                               MessageBoxButton.YesNo, 
                               MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _categoryService.DeleteCategoryAsync(categoryToDelete.Id);
                    await LoadCategoriesAsync();
                    if (SelectedCategory == categoryToDelete)
                        SelectedCategory = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении категории:\n{ex.Message}", 
                                  "Ошибка", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Добавление новых данных в категорию
        /// </summary>
        private void AddCategoryData()
        {
            if (SelectedCategory == null) return;

            var dialog = new CategoryDataDialog(SelectedCategory);
            if (dialog.ShowDialog() == true)
            {
                _ = AddCategoryDataAsync(dialog.CategoryData);
            }
        }

        /// <summary>
        /// Редактирование выбранных данных категории
        /// </summary>
        private void EditCategoryData(CategoryData? categoryData = null)
        {
            var dataToEdit = categoryData ?? SelectedCategoryData;
            
            if (dataToEdit == null)
            {
                MessageBox.Show("Выберите данные для редактирования!", 
                              "Внимание", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
                return;
            }

            if (SelectedCategory == null)
            {
                MessageBox.Show("Сначала выберите категорию!", 
                              "Внимание", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
                return;
            }

            var dialog = new CategoryDataDialog(SelectedCategory, dataToEdit);
            if (dialog.ShowDialog() == true)
            {
                _ = UpdateCategoryDataAsync(dialog.CategoryData);
            }
        }

        /// <summary>
        /// Удаление выбранных данных категории
        /// </summary>
        private async void DeleteCategoryData(CategoryData? categoryData = null)
        {
            var dataToDelete = categoryData ?? SelectedCategoryData;
            
            if (dataToDelete == null)
            {
                MessageBox.Show("Выберите данные для удаления!", 
                              "Внимание", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить данные '{dataToDelete.Name}'?", 
                               "Подтверждение удаления", 
                               MessageBoxButton.YesNo, 
                               MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _categoryService.DeleteCategoryDataAsync(dataToDelete.Id);
                    await LoadCategoryDataAsync();
                    if (SelectedCategoryData == dataToDelete)
                        SelectedCategoryData = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении данных:\n{ex.Message}", 
                                  "Ошибка", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Асинхронное обновление категории
        /// </summary>
        private async Task UpdateCategoryAsync(Category category)
        {
            try
            {
                await _categoryService.UpdateCategoryAsync(category);
                await LoadCategoriesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении категории:\n{ex.Message}", 
                              "Ошибка", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Асинхронное добавление данных категории
        /// </summary>
        private async Task AddCategoryDataAsync(CategoryData categoryData)
        {
            try
            {
                await _categoryService.AddCategoryDataAsync(categoryData);
                await LoadCategoryDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении данных:\n{ex.Message}", 
                              "Ошибка", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Асинхронное обновление данных категории
        /// </summary>
        private async Task UpdateCategoryDataAsync(CategoryData categoryData)
        {
            try
            {
                await _categoryService.UpdateCategoryDataAsync(categoryData);
                await LoadCategoryDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных:\n{ex.Message}", 
                              "Ошибка", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
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

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            _categoryService?.Dispose();
        }

        #endregion
    }
}