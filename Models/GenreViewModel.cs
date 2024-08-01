using System.ComponentModel.DataAnnotations;
namespace MusicPortalLaLaFa.Models
{
    public class GenreViewModel
    {
        public int GenreId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }
}
