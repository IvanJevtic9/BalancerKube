using BalanceKube.EventGenerator.API.Entities.Base;
using System.Linq.Expressions;

namespace BalanceKube.EventGenerator.API.Persistence.Base
{
    public interface IRepository<T> where T : IEntity
    {
        Task CreateAsync(T entity);
        Task<IReadOnlyCollection<T>> GetAllAsync();
        Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> predicate);
        Task<T> GetAsync(Guid id);
        Task<T> GetAsync(Expression<Func<T, bool>> predicate);
        Task RemoveAsync(Guid id);
        Task UpdateAsync(T entity);
    }
}
