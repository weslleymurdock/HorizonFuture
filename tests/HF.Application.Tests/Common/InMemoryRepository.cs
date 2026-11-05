using HF.Application.Common.Abstractions.Repositories;
using HF.Domain.Common.Entities;
using System.Linq.Expressions;

namespace HF.Application.Tests.Common;

public class InMemoryRepository<TEntity> : IRepository<TEntity>
    where TEntity : EntityBase
{
    private readonly List<TEntity> _items = [];

    public IReadOnlyList<TEntity> Items => _items.AsReadOnly();

    public Task<TEntity?> GetByIdAsync(string id, Expression<Func<TEntity, object?>>[] includes, CancellationToken cancellationToken)
    {
        return Task.FromResult(_items.FirstOrDefault(x => x.Id == id));
    }

    public Task<TEntity?> GetTrackedByIdAsync(string id, Expression<Func<TEntity, object?>>[] includes, CancellationToken cancellationToken)
    {
        return Task.FromResult(_items.FirstOrDefault(x => x.Id == id));
    }

    public Task<IReadOnlyCollection<TEntity?>> ListTrackedAsync(Expression<Func<TEntity, bool>>? predicate, Expression<Func<TEntity, object?>>[] includes, CancellationToken cancellationToken)
    {
        IEnumerable<TEntity> query = _items;
        if (predicate is not null)
        {
            query = query.Where(predicate.Compile());
        }

        return Task.FromResult<IReadOnlyCollection<TEntity?>>(query.ToList());
    }

    public Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object?>>[] includes, CancellationToken cancellationToken)
    {
        return Task.FromResult(_items.FirstOrDefault(predicate.Compile()));
    }

    public Task<IReadOnlyList<TEntity>> ListAsync(Expression<Func<TEntity, bool>>? predicate, Expression<Func<TEntity, object?>>[] includes, CancellationToken cancellationToken)
    {
        IEnumerable<TEntity> query = _items;
        if (predicate is not null)
        {
            query = query.Where(predicate.Compile());
        }

        return Task.FromResult<IReadOnlyList<TEntity>>(query.ToList());
    }

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return Task.FromResult(_items.Any(predicate.Compile()));
    }

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        _items.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        var existing = _items.FindIndex(x => x.Id == entity.Id);
        if (existing >= 0)
        {
            _items[existing] = entity;
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(TEntity entity, CancellationToken cancellationToken)
    {
        _items.RemoveAll(x => x.Id == entity.Id);
        return Task.CompletedTask;
    }

    public Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        var ids = entities.Select(e => e.Id).ToHashSet();
        _items.RemoveAll(x => ids.Contains(x.Id));
        return Task.CompletedTask;
    }
}

