using System.ComponentModel.DataAnnotations;

namespace Spider.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Description { get; set; }

        public List<CategoryData> DataItems { get; set; } = new();

    }
}
