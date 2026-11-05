using System;

namespace HF.Application.Common.Abstractions.Repositories;

/// <summary>
/// Marker interface for Mediator requests that require an explicit database transaction.
/// </summary>
public interface ITransactionalRequest { }
