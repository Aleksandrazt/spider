using System.ComponentModel.DataAnnotations;

namespace Spider.Models
{
    public class DockerImage
    {
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        [StringLength(200)]
        public string ImageName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        public bool IsRunning { get; set; }

        public DockerProject? Project { get; set; }
    }
}
