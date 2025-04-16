namespace StealTheCats.Entities.DataTransferObjects
{
    public class CatImageDto
    {
        public List<CatBreedDto> Breeds { get; set; }
        public string Id { get; set; }
        public string Url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] Image { get; set; }
    }
}
