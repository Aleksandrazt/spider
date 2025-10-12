using Spider.Data;

namespace Spider.Services
{
    /// <summary>
    /// Сервис для работы с данными приложения
    /// </summary>
    public class DataService : IDisposable
    {
        protected readonly DatabaseContext _dbContext;

        /// <summary>
        /// Конструктор сервиса
        /// </summary>
        public DataService() 
        { 
            _dbContext = new DatabaseContext();
            _dbContext.Database.EnsureCreated();
        }

        /// <summary>
        /// Освобождение ресурсов базы данных
        /// </summary>
        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}
