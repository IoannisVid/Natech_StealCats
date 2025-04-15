using System.ComponentModel.DataAnnotations;

namespace StealTheCats.Entities.DataTransferObjects
{
    public class CatDto
    {
        public required string CatId { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public string? Image { get; set; }

        public DateTime CreatedAt { get; set; }
        public List<string> Tags { get; set; }
    }
}
