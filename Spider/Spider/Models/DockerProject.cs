using System.ComponentModel.DataAnnotations;

namespace Spider.Models
{
    public class DockerProject
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Description { get; set; }

        [Required]
        [StringLength(500)]
        public string DockerComposePath { get; set; } = string.Empty;
        public List<DockerImage> Images { get; set; } = new();
    }
}
