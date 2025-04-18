namespace StealTheCats.Entities.Models
{
    public class CatParameters : QueryParameters
    {
        public string? Tag { get; set; }
        public string GetKeyString()
        {
            return $"{(Tag != null ? Tag + "_" : "")}page{PageNumber}_size{PageSize}";
        }
    }
}
