using Microsoft.EntityFrameworkCore;
using Spider.Models;

namespace Spider.Services
{
    public class DockerService : DataService
    {
        #region Docker проекты

        /// <summary>
        /// Получить все Docker проекты с их образами
        /// </summary>
        public async Task<List<DockerProject>> GetDockerProjectsAsync()
        {
            return await _dbContext.DockerProjects
                .Include(dp => dp.Images)
                .OrderBy(dp => dp.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Получить Docker проект по ID
        /// </summary>
        public async Task<DockerProject?> GetDockerProjectByIdAsync(int id)
        {
            return await _dbContext.DockerProjects
                .Include(dp => dp.Images)
                .FirstOrDefaultAsync(dp => dp.Id == id);
        }

        /// <summary>
        /// Добавить новый Docker проект
        /// </summary>
        public async Task<DockerProject> AddDockerProjectAsync(DockerProject project)
        {
            _dbContext.DockerProjects.Add(project);
            await _dbContext.SaveChangesAsync();
            return project;
        }

        /// <summary>
        /// Обновить Docker проект
        /// </summary>
        public async Task UpdateDockerProjectAsync(DockerProject project)
        {
            _dbContext.DockerProjects.Update(project);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Удалить Docker проект по ID
        /// </summary>
        public async Task DeleteDockerProjectAsync(int id)
        {
            var project = await _dbContext.DockerProjects.FindAsync(id);
            if (project != null)
            {
                _dbContext.DockerProjects.Remove(project);
                await _dbContext.SaveChangesAsync();
            }
        }

        #endregion

        #region Docker образы

        /// <summary>
        /// Получить все образы для конкретного проекта
        /// </summary>
        public async Task<List<DockerImage>> GetDockerImagesAsync(int projectId)
        {
            return await _dbContext.DockerImages
                .Where(di => di.ProjectId == projectId)
                .OrderBy(di => di.ImageName)
                .ToListAsync();
        }

        /// <summary>
        /// Добавить новый Docker образ
        /// </summary>
        public async Task<DockerImage> AddDockerImageAsync(DockerImage image)
        {
            _dbContext.DockerImages.Add(image);
            await _dbContext.SaveChangesAsync();
            return image;
        }

        /// <summary>
        /// Обновить Docker образ
        /// </summary>
        public async Task UpdateDockerImageAsync(DockerImage image)
        {
            _dbContext.DockerImages.Update(image);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Удалить Docker образ по ID
        /// </summary>
        public async Task DeleteDockerImageAsync(int id)
        {
            var image = await _dbContext.DockerImages.FindAsync(id);
            if (image != null)
            {
                _dbContext.DockerImages.Remove(image);
                await _dbContext.SaveChangesAsync();
            }
        }

        #endregion
    }
}
