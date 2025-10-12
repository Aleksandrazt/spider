using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Spider.Helpers;
using Spider.Models;
using Spider.Services;

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
            EditCategoryCommand = new RelayCommand(_ => EditCategory());
            DeleteCategoryCommand = new RelayCommand(_ => DeleteCategory());
            AddCategoryDataCommand = new RelayCommand(_ => AddCategoryData(), _ => SelectedCategory != null);
            EditCategoryDataCommand = new RelayCommand(_ => EditCategoryData());
            DeleteCategoryDataCommand = new RelayCommand(_ => DeleteCategoryData());

            // Загружаем категории при создании ViewModel
            _ = LoadCategoriesAsync();
        }

        #endregion

        #region Методы загрузки данных

        /// <summary>
        /// Загрузка всех категорий
        /// </summary>
        private async Task LoadCategoriesAsync()
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
                // TODO: Показать ошибку пользователю
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
                // TODO: Показать ошибку пользователю
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
            // TODO: Открыть диалоговое окно для добавления категории
            System.Diagnostics.Debug.WriteLine("Добавление новой категории");
        }

        /// <summary>
        /// Редактирование выбранной категории
        /// </summary>
        private void EditCategory()
        {
            if (SelectedCategory == null)
            {
                // TODO: Показать сообщение пользователю
                return;
            }

            // TODO: Открыть диалоговое окно для редактирования категории
            System.Diagnostics.Debug.WriteLine($"Редактирование категории: {SelectedCategory.Name}");
        }

        /// <summary>
        /// Удаление выбранной категории
        /// </summary>
        private async void DeleteCategory()
        {
            if (SelectedCategory == null)
            {
                // TODO: Показать сообщение пользователю
                return;
            }

            // TODO: Показать подтверждение удаления
            try
            {
                await _categoryService.DeleteCategoryAsync(SelectedCategory.Id);
                await LoadCategoriesAsync(); // Перезагружаем список
                SelectedCategory = null;
            }
            catch (Exception ex)
            {
                // TODO: Показать ошибку пользователю
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления категории: {ex.Message}");
            }
        }

        /// <summary>
        /// Добавление новых данных в категорию
        /// </summary>
        private void AddCategoryData()
        {
            if (SelectedCategory == null) return;

            // TODO: Открыть диалоговое окно для добавления данных
            System.Diagnostics.Debug.WriteLine($"Добавление данных в категорию: {SelectedCategory.Name}");
        }

        /// <summary>
        /// Редактирование выбранных данных категории
        /// </summary>
        private void EditCategoryData()
        {
            if (SelectedCategoryData == null)
            {
                // TODO: Показать сообщение пользователю
                return;
            }

            // TODO: Открыть диалоговое окно для редактирования данных
            System.Diagnostics.Debug.WriteLine($"Редактирование данных: {SelectedCategoryData.Name}");
        }

        /// <summary>
        /// Удаление выбранных данных категории
        /// </summary>
        private async void DeleteCategoryData()
        {
            if (SelectedCategoryData == null)
            {
                // TODO: Показать сообщение пользователю
                return;
            }

            // TODO: Показать подтверждение удаления
            try
            {
                await _categoryService.DeleteCategoryDataAsync(SelectedCategoryData.Id);
                await LoadCategoryDataAsync(); // Перезагружаем данные
            }
            catch (Exception ex)
            {
                // TODO: Показать ошибку пользователю
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления данных: {ex.Message}");
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