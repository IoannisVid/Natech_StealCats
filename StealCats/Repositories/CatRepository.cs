using StealTheCats.Entities;
using StealTheCats.Entities.Models;
using StealTheCats.Interfaces;

namespace StealTheCats.Repositories
{
    public class CatRepository : Repository<Cat>, ICatRepository
    {
        public CatRepository(ApplicationDBContext DBContext) : base(DBContext) { }
    }
}
