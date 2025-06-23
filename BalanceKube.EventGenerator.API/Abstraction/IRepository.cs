using System.Linq.Expressions;
using BalancerKube.Common.Domain;

namespace BalanceKube.EventGenerator.API.Abstraction;

public interface IRepository<T> where T : Entity<Guid>
{
    Task CreateAsync(T entity);
    Task<IReadOnlyCollection<T>> GetAllAsync();
    Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> predicate);
    Task<T> GetAsync(Guid id);
    Task<T> GetAsync(Expression<Func<T, bool>> predicate);
    Task RemoveAsync(Guid id);
    Task UpdateAsync(T entity);
}
