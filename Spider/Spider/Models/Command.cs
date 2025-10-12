using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spider.Models
{
    /// <summary>
    /// Представляет команду для выполнения в системе
    /// </summary>
    [Table("Command")]
    public class Command
    {
        /// <summary>
        /// Уникальный идентификатор команды
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Путь к рабочей директории для выполнения команды
        /// </summary>
        [Required(ErrorMessage = "Путь к директории обязателен для заполнения")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Путь должен содержать от 1 до 1000 символов")]
        public string FolderPath { get; set; } = string.Empty;

        /// <summary>
        /// Текст команды для выполнения
        /// </summary>
        [Required(ErrorMessage = "Текст команды обязателен для заполнения")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Команда должна содержать от 1 до 1000 символов")]
        public string CommandText { get; set; } = string.Empty;

        /// <summary>
        /// Название команды
        /// </summary>
        [Required(ErrorMessage = "Название команды обязательно для заполнения")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Название должно содержать от 2 до 100 символов")]
        [RegularExpression(@"^[a-zA-Zа-яА-Я0-9\s\-_]+$", ErrorMessage = "Название команды содержит недопустимые символы")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание команды и её назначения
        /// </summary>
        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string? Description { get; set; }

        /// <summary>
        /// Аргументы команды (необязательные)
        /// </summary>
        [StringLength(500, ErrorMessage = "Аргументы не должны превышать 500 символов")]
        public string? Arguments { get; set; }

    }
}