using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Spider.Models
{
    /// <summary>
    /// Представляет данные, принадлежащие определенной категории
    /// </summary>
    [Table("CategoryData")]
    public class CategoryData
    {
        /// <summary>
        /// Уникальный идентификатор данных
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор родительской категории
        /// </summary>
        [Required(ErrorMessage = "Связь с категорией обязательна")]
        [Range(1, int.MaxValue, ErrorMessage = "Идентификатор категории должен быть положительным числом")]
        [ForeignKey("CategoryId")]
        public int CategoryId { get; set; }

        /// <summary>
        /// Название данных
        /// </summary>
        [Required(ErrorMessage = "Название данных обязательно для заполнения")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Название должно содержать от 1 до 100 символов")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Значение данных
        /// </summary>
        [Required(ErrorMessage = "Значение данных обязательно для заполнения")]
        [StringLength(1000, ErrorMessage = "Значение не должно превышать 1000 символов")]
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Навигационное свойство для категории
        /// </summary>
        [JsonIgnore]
        public Category? Category { get; set; }
    }
}