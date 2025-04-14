using StealTheCats.Entities;
using StealTheCats.Interfaces;

namespace StealTheCats.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private ApplicationDBContext _dbContext;
        private readonly Dictionary<Type, object> _repositories = new();
        public UnitOfWork(ApplicationDBContext DBContext)
        {
            _dbContext = DBContext;
        }

        public IRepository<T> GetRepository<T>() where T : class
        {
            if (_repositories.ContainsKey(typeof(T)))
            {
                return (IRepository<T>)_repositories[typeof(T)];
            }

            var repository = new EFRepository<T>(_dbContext);
            _repositories.Add(typeof(T), repository);
            return repository;
        }

        public async Task SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
