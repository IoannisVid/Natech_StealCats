using StealTheCats.Entities;
using StealTheCats.Interfaces;

namespace StealTheCats.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDBContext _dbContext;
        private Dictionary<Type, object> _repositories;
        public UnitOfWork(ApplicationDBContext DBContext)
        {
            _dbContext = DBContext;
            _repositories = new Dictionary<Type, object>();
        }

        public IRepository<T> GetRepository<T>() where T : class
        {
            if (_repositories.ContainsKey(typeof(T)))
            {
                return (IRepository<T>)_repositories[typeof(T)];
            }

            var repository = new Repository<T>(_dbContext);
            _repositories.Add(typeof(T), repository);
            return repository;
        }

        public void Save()
        {
            _dbContext.SaveChanges();
        }
    }
}
