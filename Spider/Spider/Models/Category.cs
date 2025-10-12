using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Spider.Models
{
    /// <summary>
    /// Представляет категорию для организации данных
    /// </summary>
    [Table("Category")]
    public class Category : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string? _description;

        /// <summary>
        /// Уникальный идентификатор категории
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Название категории
        /// </summary>
        [Required(ErrorMessage = "Название категории обязательно для заполнения")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Название категории должно содержать от 2 до 50 символов")]
        [RegularExpression(@"^[a-zA-Zа-яА-Я0-9\s\-_]+$", ErrorMessage = "Название категории содержит недопустимые символы")]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Описание категории
        /// </summary>
        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string? Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Коллекция данных, принадлежащих этой категории
        /// </summary>
        public List<CategoryData> DataItems { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}