using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spider.Models
{
    /// <summary>
    /// Представляет Docker проект с композ файлом
    /// </summary>
    [Table("DockerProject")]
    public class DockerProject
    {
        /// <summary>
        /// Уникальный идентификатор проекта
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Название проекта
        /// </summary>
        [Required(ErrorMessage = "Название проекта обязательно для заполнения")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Название проекта должно содержать от 2 до 50 символов")]
        [RegularExpression(@"^[a-zA-Zа-яА-Я0-9\s\-_]+$", ErrorMessage = "Название проекта содержит недопустимые символы")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание проекта и его назначения
        /// </summary>
        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string? Description { get; set; }

        /// <summary>
        /// Путь к docker-compose файлу проекта
        /// </summary>
        [Required(ErrorMessage = "Путь к docker-compose файлу обязателен")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Путь должен содержать от 1 до 500 символов")]
        public string DockerComposePath { get; set; } = string.Empty;

        /// <summary>
        /// Коллекция образов, принадлежащих проекту
        /// </summary>
        public List<DockerImage> Images { get; set; } = new();

    }
}