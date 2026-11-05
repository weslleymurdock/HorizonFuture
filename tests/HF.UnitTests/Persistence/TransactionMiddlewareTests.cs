using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HF.Application.Common.Abstractions.Repositories;
using HF.Composition.Extensions;
using HF.Infrastructure.Persistence.Middlewares;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HF.Application.Tests.Persistence;

public class TransactionMiddlewareTests
{
    private sealed record TestTransactionalMessage : IRequest<string>, ITransactionalRequest;
    private sealed record TestNonTransactionalMessage : IRequest<string>;

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int BeginCount { get; private set; }
        public int CommitCount { get; private set; }
        public int RollbackCount { get; private set; }
        public int SaveChangesCount { get; private set; }
        public List<string> ExecutionOrder { get; } = [];

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCount++;
            ExecutionOrder.Add(nameof(SaveChangesAsync));
            return Task.FromResult(1);
        }

        public Task BeginTransactionAsync(CancellationToken cancellationToken)
        {
            BeginCount++;
            ExecutionOrder.Add(nameof(BeginTransactionAsync));
            return Task.CompletedTask;
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken)
        {
            CommitCount++;
            ExecutionOrder.Add(nameof(CommitTransactionAsync));
            return Task.CompletedTask;
        }

        public Task RollbackTransactionAsync(CancellationToken cancellationToken)
        {
            RollbackCount++;
            ExecutionOrder.Add(nameof(RollbackTransactionAsync));
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Handle_TransactionalMessage_ExecutesInTransaction_AndCommits()
    {
        var fakeUow = new FakeUnitOfWork();
        var logger = NullLogger<TransactionMiddleware<TestTransactionalMessage, string>>.Instance;
        var middleware = new TransactionMiddleware<TestTransactionalMessage, string>(fakeUow, logger);

        var message = new TestTransactionalMessage();
        bool delegateExecuted = false;

        var response = await middleware.Handle(
            message,
            (msg, ct) =>
            {
                delegateExecuted = true;
                fakeUow.ExecutionOrder.Add("HandlerExecution");
                return new ValueTask<string>("success");
            },
            CancellationToken.None);

        Assert.Equal("success", response);
        Assert.True(delegateExecuted);
        Assert.Equal(1, fakeUow.BeginCount);
        Assert.Equal(1, fakeUow.SaveChangesCount);
        Assert.Equal(1, fakeUow.CommitCount);
        Assert.Equal(0, fakeUow.RollbackCount);

        Assert.Equal(
            new[] { nameof(fakeUow.BeginTransactionAsync), "HandlerExecution", nameof(fakeUow.SaveChangesAsync), nameof(fakeUow.CommitTransactionAsync) },
            fakeUow.ExecutionOrder);
    }

    [Fact]
    public async Task Handle_NonTransactionalMessage_DoesNotOpenTransaction_AndDelegatesDirectly()
    {
        var fakeUow = new FakeUnitOfWork();
        var logger = NullLogger<TransactionMiddleware<TestNonTransactionalMessage, string>>.Instance;
        var middleware = new TransactionMiddleware<TestNonTransactionalMessage, string>(fakeUow, logger);

        var message = new TestNonTransactionalMessage();
        bool delegateExecuted = false;

        var response = await middleware.Handle(
            message,
            (msg, ct) =>
            {
                delegateExecuted = true;
                return new ValueTask<string>("bypassed");
            },
            CancellationToken.None);

        Assert.Equal("bypassed", response);
        Assert.True(delegateExecuted);
        Assert.Equal(0, fakeUow.BeginCount);
        Assert.Equal(0, fakeUow.SaveChangesCount);
        Assert.Equal(0, fakeUow.CommitCount);
        Assert.Equal(0, fakeUow.RollbackCount);
        Assert.Empty(fakeUow.ExecutionOrder);
    }

    [Fact]
    public async Task Handle_TransactionalMessage_WhenHandlerThrows_RollsBackTransaction()
    {
        var fakeUow = new FakeUnitOfWork();
        var logger = NullLogger<TransactionMiddleware<TestTransactionalMessage, string>>.Instance;
        var middleware = new TransactionMiddleware<TestTransactionalMessage, string>(fakeUow, logger);

        var message = new TestTransactionalMessage();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await middleware.Handle(
                message,
                (msg, ct) => throw new InvalidOperationException("Handler exploded"),
                CancellationToken.None);
        });

        Assert.Equal("Handler exploded", ex.Message);
        Assert.Equal(1, fakeUow.BeginCount);
        Assert.Equal(0, fakeUow.CommitCount);
        Assert.Equal(1, fakeUow.RollbackCount);
        Assert.Contains(nameof(fakeUow.RollbackTransactionAsync), fakeUow.ExecutionOrder);
    }

    [Fact]
    public void AddApplication_Registers_TransactionMiddleware_In_ServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();

        var registration = services.FirstOrDefault(s =>
            s.ServiceType.FullName?.Contains("TransactionMiddleware") == true ||
            s.ImplementationType?.FullName?.Contains("TransactionMiddleware") == true);

        Assert.NotNull(registration);
        Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
    }
}
