using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StealTheCats.Entities;
using StealTheCats.Interfaces;

namespace StealTheCats.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected ApplicationDBContext DBContext { get; set; }
        public Repository(ApplicationDBContext dbContext)
        {
            DBContext = dbContext;
        }
        public IQueryable<T> GetAll() => DBContext.Set<T>().AsNoTracking();
        public IQueryable<T> GetByCondition(Expression<Func<T, bool>> expression) =>
            DBContext.Set<T>().Where(expression).AsNoTracking();
        public void Create(T entity) => DBContext.Set<T>().Add(entity);
        public void Update(T entity) => DBContext.Set<T>().Update(entity);
        public void Delete(T entity) => DBContext.Set<T>().Remove(entity);
    }
}
