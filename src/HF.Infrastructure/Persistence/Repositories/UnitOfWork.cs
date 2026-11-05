using HF.Application.Common.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
namespace HF.Infrastructure.Persistence.Repositories;

/// <summary>
/// Coordinates EF Core persistence transactions.
/// </summary>
public class UnitOfWork(HFDbContext context) : IUnitOfWork,
    IAsyncDisposable
{

    private IDbContextTransaction?
        transaction;

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task BeginTransactionAsync(
        CancellationToken cancellationToken)
    {
        if (transaction is not null)
        {
            return;
        }

        transaction = await context.Database
            .BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(
        CancellationToken cancellationToken)
    {
        if (transaction is null)
        {
            return;
        }

        await transaction.CommitAsync(cancellationToken);
        await DisposeTransactionAsync();
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(
        CancellationToken cancellationToken)
    {
        if (transaction is null)
        {
            return;
        }

        await transaction.RollbackAsync(cancellationToken);
        await DisposeTransactionAsync();
    }


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (transaction is null)
        {
            return;
        }

        await transaction.RollbackAsync();
        await DisposeTransactionAsync();
    }

    private async ValueTask DisposeTransactionAsync()
    {
        if (transaction is null)
        {
            return;
        }

        await transaction.DisposeAsync();
        transaction = null;
    }
}

