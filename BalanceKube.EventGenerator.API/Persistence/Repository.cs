using MongoDB.Driver;
using System.Linq.Expressions;
using BalancerKube.Common.Domain;
using BalanceKube.EventGenerator.API.Abstraction;

namespace BalanceKube.EventGenerator.API.Persistence;

public class Repository<T> : IRepository<T> where T : Entity<Guid>
{
    private readonly IMongoCollection<T> _collection;
    private readonly FilterDefinitionBuilder<T> _filterBuilder = Builders<T>.Filter;

    public Repository(IMongoDatabase database, string collectionName)
    {
        _collection = database.GetCollection<T>(collectionName);
    }

    public async Task CreateAsync(T entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        await _collection.InsertOneAsync(entity);
    }

    public async Task<IReadOnlyCollection<T>> GetAllAsync()
    {
        return await _collection.Find(_filterBuilder.Empty).ToListAsync();
    }

    public async Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> predicate)
    {
        return await _collection.Find(predicate).ToListAsync();
    }

    public async Task<T> GetAsync(Guid id)
    {
        FilterDefinition<T> filter = _filterBuilder.Eq(entity => entity.Id, id);

        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<T> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await _collection.Find(predicate).FirstOrDefaultAsync();
    }

    public async Task RemoveAsync(Guid id)
    {
        FilterDefinition<T> filter = _filterBuilder.Eq(_entity => _entity.Id, id);

        await _collection.DeleteOneAsync(filter);
    }

    public async Task UpdateAsync(T entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        FilterDefinition<T> filter = _filterBuilder.Eq(entity => entity.Id, entity.Id);

        await _collection.ReplaceOneAsync(filter, entity);
    }
}
