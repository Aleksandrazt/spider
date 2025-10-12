using System.ComponentModel.DataAnnotations;

namespace Spider.Models
{
    public class CategoryData
    {
        public int Id { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Value { get; set; } = string.Empty;

        public Category? Category { get; set; }
    }
}
