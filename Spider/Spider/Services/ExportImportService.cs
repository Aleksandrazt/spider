using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Spider.Data;
using Spider.Models;
using System.IO;
using System.Windows;

namespace Spider.Services
{
    /// <summary>
    /// Сервис для экспорта и импорта данных приложения
    /// </summary>
    public class ExportImportService : DataService
    {
        private readonly CategoryService _categoryService;
        private readonly CommandService _commandService;
        private readonly DockerService _dockerService;

        public ExportImportService()
        {
            _categoryService = new CategoryService();
            _commandService = new CommandService();
            _dockerService = new DockerService();
        }

        /// <summary>
        /// Экспортировать все данные в файл
        /// </summary>
        public async Task<bool> ExportDataAsync(string filePath)
        {
            try
            {
                var exportData = new ExportData
                {
                    Version = "1.0",
                    ExportDate = DateTime.Now
                };

                var categories = await _categoryService.GetCategoriesAsync();
                exportData.Categories = categories;

                var commands = await _commandService.GetCommandsAsync();
                exportData.Commands = commands;

                var dockerProjects = await _dockerService.GetDockerProjectsAsync();
                exportData.DockerProjects = dockerProjects;

                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                };
                var json = JsonConvert.SerializeObject(exportData, settings);
                
                await File.WriteAllTextAsync(filePath, json);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте данных: {ex.Message}", 
                              "Ошибка экспорта", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Импортировать данные из файла
        /// </summary>
        public async Task<bool> ImportDataAsync(string filePath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                
                var exportData = JsonConvert.DeserializeObject<ExportData>(json);
                
                if (exportData == null)
                {
                    MessageBox.Show("Не удалось прочитать данные из файла.", 
                                  "Ошибка импорта", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Error);
                    return false;
                }

                var result = MessageBox.Show(
                    "Импорт данных заменит все существующие данные в базе.\n\n" +
                    "Продолжить?",
                    "Подтверждение импорта",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    return false;
                }

                await ClearAllDataAsync();

                foreach (var category in exportData.Categories)
                {
                    var categoryData = category.DataItems?.ToList() ?? new List<CategoryData>();
                    category.DataItems = new List<CategoryData>();

                    var newCategory = new Category
                    {
                        Name = category.Name,
                        Description = category.Description
                    };

                    var addedCategory = await _categoryService.AddCategoryAsync(newCategory);

                    foreach (var data in categoryData)
                    {
                        var newData = new CategoryData
                        {
                            CategoryId = addedCategory.Id,
                            Name = data.Name,
                            Value = data.Value
                        };
                        await _categoryService.AddCategoryDataAsync(newData);
                    }
                }

                foreach (var command in exportData.Commands)
                {
                    var newCommand = new Command
                    {
                        Name = command.Name,
                        Description = command.Description,
                        CommandText = command.CommandText,
                        FolderPath = command.FolderPath,
                        Arguments = command.Arguments
                    };
                    await _commandService.AddCommandAsync(newCommand);
                }

                foreach (var project in exportData.DockerProjects)
                {
                    var images = project.Images?.ToList() ?? new List<DockerImage>();
                    project.Images = new List<DockerImage>();

                    var newProject = new DockerProject
                    {
                        Name = project.Name,
                        Description = project.Description,
                        DockerComposePath = project.DockerComposePath
                    };

                    var addedProject = await _dockerService.AddDockerProjectAsync(newProject);

                    foreach (var image in images)
                    {
                        var newImage = new DockerImage
                        {
                            ProjectId = addedProject.Id,
                            ImageName = image.ImageName,
                            Description = image.Description,
                            IsRunning = false
                        };
                        await _dockerService.AddDockerImageAsync(newImage);
                    }
                }

                MessageBox.Show("Данные успешно импортированы!", 
                              "Успех", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте данных: {ex.Message}", 
                              "Ошибка импорта", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Очистить все данные из базы
        /// </summary>
        private async Task ClearAllDataAsync()
        {
            var allCategoryData = await _dbContext.CategoryData.ToListAsync();
            _dbContext.CategoryData.RemoveRange(allCategoryData);

            var allCategories = await _dbContext.Categories.ToListAsync();
            _dbContext.Categories.RemoveRange(allCategories);

            var allImages = await _dbContext.DockerImages.ToListAsync();
            _dbContext.DockerImages.RemoveRange(allImages);

            var allProjects = await _dbContext.DockerProjects.ToListAsync();
            _dbContext.DockerProjects.RemoveRange(allProjects);

            var allCommands = await _dbContext.Commands.ToListAsync();
            _dbContext.Commands.RemoveRange(allCommands);

            await _dbContext.SaveChangesAsync();
        }

        public new void Dispose()
        {
            _categoryService?.Dispose();
            _commandService?.Dispose();
            _dockerService?.Dispose();
            base.Dispose();
        }
    }
}

