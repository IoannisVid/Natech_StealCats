using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StealTheCats.Entities.Models
{
    public class Cat
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Image Id is required")]
        public string CatId { get; set; }

        [Required(ErrorMessage = "Width of Image is required")]
        public int Width { get; set; }

        [Required(ErrorMessage = "Height of Image is required")]
        public int Height { get; set; }

        [Required(ErrorMessage = "Cat Image is required")]
        public byte[] Image { get; set; }

        [Required(ErrorMessage = "Timestamp is required")]
        public DateTime Created { get; set; }

        public ICollection<Tag> Tags { get; } = [];
    }
}
