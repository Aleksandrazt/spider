using System.ComponentModel.DataAnnotations;

namespace Spider.Models
{
    public class Command
    {
        public int Id { get; set; }

        [Required]
        [StringLength(1000)]
        public string FolderPath { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CommandText { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Description { get; set; }
    }
}
