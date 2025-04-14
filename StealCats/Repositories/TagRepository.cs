using StealTheCats.Entities;
using StealTheCats.Entities.Models;
using StealTheCats.Interfaces;

namespace StealTheCats.Repositories
{
    public class TagRepository : Repository<Tag>, ITagRepository
    {
        public TagRepository(ApplicationDBContext DBContext) : base(DBContext) { }
    }
}
