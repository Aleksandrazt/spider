using Microsoft.EntityFrameworkCore;
using Spider.Models;

namespace Spider.Services
{
    public class CategoryService : DataService
    {
        #region Категорий
        /// <summary>
        /// Получить все категории с их данными
        /// </summary>
        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _dbContext.Categories
                .Include(c => c.DataItems)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Получить категорию по ID
        /// </summary>
        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _dbContext.Categories
                .Include(c => c.DataItems)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Добавить новую категорию
        /// </summary>
        public async Task<Category> AddCategoryAsync(Category category)
        {
            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();
            return category;
        }

        /// <summary>
        /// Обновить категорию
        /// </summary>
        public async Task UpdateCategoryAsync(Category category)
        {
            _dbContext.Categories.Update(category);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Удалить категорию по ID
        /// </summary>
        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category != null)
            {
                _dbContext.Categories.Remove(category);
                await _dbContext.SaveChangesAsync();
            }
        }

        #endregion

        #region Данные категорий

        /// <summary>
        /// Получить все данные для конкретной категории
        /// </summary>
        public async Task<List<CategoryData>> GetCategoryDataAsync(int categoryId)
        {
            return await _dbContext.CategoryData
                .Where(cd => cd.CategoryId == categoryId)
                .OrderBy(cd => cd.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Добавить новые данные в категорию
        /// </summary>
        public async Task<CategoryData> AddCategoryDataAsync(CategoryData categoryData)
        {
            _dbContext.CategoryData.Add(categoryData);
            await _dbContext.SaveChangesAsync();
            return categoryData;
        }

        /// <summary>
        /// Обновить данные категории
        /// </summary>
        public async Task UpdateCategoryDataAsync(CategoryData categoryData)
        {
            _dbContext.CategoryData.Update(categoryData);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Удалить данные категории по ID
        /// </summary>
        public async Task DeleteCategoryDataAsync(int id)
        {
            var categoryData = await _dbContext.CategoryData.FindAsync(id);
            if (categoryData != null)
            {
                _dbContext.CategoryData.Remove(categoryData);
                await _dbContext.SaveChangesAsync();
            }
        }

        #endregion
    }
}
