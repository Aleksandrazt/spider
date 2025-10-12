using Microsoft.EntityFrameworkCore;
using Spider.Models;


namespace Spider.Services
{
    public class CommandService : DataService
    {
        #region Команды

        /// <summary>
        /// Получить все команды
        /// </summary>
        public async Task<List<Command>> GetCommandsAsync()
        {
            return await _dbContext.Commands
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Получить команду по ID
        /// </summary>
        public async Task<Command?> GetCommandByIdAsync(int id)
        {
            return await _dbContext.Commands.FindAsync(id);
        }

        /// <summary>
        /// Добавить новую команду
        /// </summary>
        public async Task<Command> AddCommandAsync(Command command)
        {
            _dbContext.Commands.Add(command);
            await _dbContext.SaveChangesAsync();
            return command;
        }

        /// <summary>
        /// Обновить команду
        /// </summary>
        public async Task UpdateCommandAsync(Command command)
        {
            _dbContext.Commands.Update(command);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Удалить команду по ID
        /// </summary>
        public async Task DeleteCommandAsync(int id)
        {
            var command = await _dbContext.Commands.FindAsync(id);
            if (command != null)
            {
                _dbContext.Commands.Remove(command);
                await _dbContext.SaveChangesAsync();
            }
        }

        #endregion
    }
}
