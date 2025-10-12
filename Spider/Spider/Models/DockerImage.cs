using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spider.Models
{
    /// <summary>
    /// Представляет Docker образ в системе
    /// </summary>
    [Table("DockerImage")]
    public class DockerImage
    {
        /// <summary>
        /// Уникальный идентификатор образа
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор проекта, к которому принадлежит образ
        /// </summary>
        [Required(ErrorMessage = "Связь с проектом обязательна")]
        [Range(1, int.MaxValue, ErrorMessage = "Идентификатор проекта должен быть положительным числом")]
        [ForeignKey("ProjectId")]
        public int ProjectId { get; set; }

        /// <summary>
        /// Название Docker образа
        /// </summary>
        [Required(ErrorMessage = "Название образа обязательно для заполнения")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Название образа должно содержать от 3 до 200 символов")]
        [RegularExpression(@"^[a-zA-Z0-9\-_/:\.]+$", ErrorMessage = "Название образа содержит недопустимые символы")]
        public string ImageName { get; set; } = string.Empty;

        /// <summary>
        /// Описание образа
        /// </summary>
        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string? Description { get; set; }

        /// <summary>
        /// Флаг, указывающий запущен ли образ в данный момент
        /// </summary>
        public bool IsRunning { get; set; } = false;

        /// <summary>
        /// Навигационное свойство для проекта
        /// </summary>
        public DockerProject? Project { get; set; }
    }
}