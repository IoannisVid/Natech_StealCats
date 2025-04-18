using StealTheCats.Entities;

namespace StealTheCats.Repositories
{
    public class EFRepository<T> : Repository<T> where T : class
    {
        public EFRepository(ApplicationDBContext dbContext) : base(dbContext) { }
    }
}
