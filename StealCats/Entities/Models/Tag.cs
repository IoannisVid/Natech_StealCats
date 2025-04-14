using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StealTheCats.Entities.Models
{
    public class Tag
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Cat's Temperament is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Timestamp is required")]
        public DateTimeOffset Created { get; set; }

        public ICollection<Cat> Cats { get; } = [];
    }
}
