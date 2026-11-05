using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HF.Application.Common.Abstractions.Repositories;
using Mediator;
using Xunit;

namespace HF.Application.Tests.Persistence;

/// <summary>
/// Architecture and reflection tests validating transactional request marking rules
/// across HF.Application features.
/// </summary>
public class TransactionalRequestsArchitectureTests
{
    private static readonly Assembly ApplicationAssembly = typeof(ITransactionalRequest).Assembly;

    private static readonly HashSet<string> ExcludedAuthNamespaces =
    [
        "HF.Application.Auth.Requests"
    ];

    [Fact]
    public void All_NonAuth_WriteCommands_Implementing_GenericRepository_Must_Implement_ITransactionalRequest()
    {
        var allRequestTypes = ApplicationAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && ImplementsIRequest(t))
            .ToList();

        var nonAuthWriteCommands = allRequestTypes
            .Where(t => !ExcludedAuthNamespaces.Contains(t.Namespace ?? string.Empty))
            .Where(t => IsWriteCommand(t.Name))
            .ToList();

        Assert.NotEmpty(nonAuthWriteCommands);

        var missingTransactional = nonAuthWriteCommands
            .Where(t => !typeof(ITransactionalRequest).IsAssignableFrom(t))
            .Select(t => t.FullName)
            .ToList();

        Assert.True(
            missingTransactional.Count == 0,
            $"The following non-Auth write commands do not implement ITransactionalRequest:\n{string.Join("\n", missingTransactional)}");

        // Exactly 42 write commands (14 entity types * 3: Create, Update, Delete)
        Assert.Equal(42, nonAuthWriteCommands.Count);
    }

    [Fact]
    public void All_Queries_Must_Not_Implement_ITransactionalRequest()
    {
        var allRequestTypes = ApplicationAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && ImplementsIRequest(t))
            .ToList();

        var queryRequests = allRequestTypes
            .Where(t => IsQuery(t.Name))
            .ToList();

        Assert.NotEmpty(queryRequests);

        var wronglyMarkedQueries = queryRequests
            .Where(t => typeof(ITransactionalRequest).IsAssignableFrom(t))
            .Select(t => t.FullName)
            .ToList();

        Assert.True(
            wronglyMarkedQueries.Count == 0,
            $"The following query requests incorrectly implement ITransactionalRequest:\n{string.Join("\n", wronglyMarkedQueries)}");
    }

    [Fact]
    public void All_Auth_Requests_Using_UserManager_Must_Be_Excluded_From_ITransactionalRequest()
    {
        var authRequestTypes = ApplicationAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && ImplementsIRequest(t))
            .Where(t => ExcludedAuthNamespaces.Contains(t.Namespace ?? string.Empty))
            .ToList();

        Assert.NotEmpty(authRequestTypes);

        var wronglyMarkedAuth = authRequestTypes
            .Where(t => typeof(ITransactionalRequest).IsAssignableFrom(t))
            .Select(t => t.FullName)
            .ToList();

        Assert.True(
            wronglyMarkedAuth.Count == 0,
            $"The following Auth requests incorrectly implement ITransactionalRequest:\n{string.Join("\n", wronglyMarkedAuth)}");
    }

    [Fact]
    public void NonAuth_WriteCommands_Include_All_Expected_Entities()
    {
        var writeCommands = ApplicationAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && ImplementsIRequest(t))
            .Where(t => typeof(ITransactionalRequest).IsAssignableFrom(t))
            .Select(t => t.Name)
            .ToList();

        string[] expectedEntities =
        [
            "Asset", "Liability", "Attendance", "ClassSession",
            "Collaborator", "Course", "Subject", "Director",
            "Enrollment", "Manager", "Student", "TeacherAssignment",
            "Teacher", "Volunteer"
        ];

        foreach (var entity in expectedEntities)
        {
            Assert.Contains(writeCommands, name => name.Contains(entity));
        }
    }

    private static bool ImplementsIRequest(Type type)
    {
        return type.GetInterfaces().Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));
    }

    private static bool IsWriteCommand(string typeName)
    {
        return typeName.StartsWith("Create", StringComparison.Ordinal) ||
               typeName.StartsWith("Update", StringComparison.Ordinal) ||
               typeName.StartsWith("Delete", StringComparison.Ordinal);
    }

    private static bool IsQuery(string typeName)
    {
        return typeName.StartsWith("Get", StringComparison.Ordinal) ||
               typeName.StartsWith("List", StringComparison.Ordinal);
    }
}
