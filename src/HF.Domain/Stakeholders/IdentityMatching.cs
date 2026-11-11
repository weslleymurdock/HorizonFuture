namespace HF.Domain.Stakeholders;

public enum DuplicateMatchKind
{
    None = 0,
    Possible = 1,
    Exact = 2
}

public sealed record IdentityMatchResult(DuplicateMatchKind Kind, IReadOnlyCollection<string> Signals);

public static class IdentityMatching
{
    public static IdentityMatchResult Compare(Person left, Person right, string? leftDocument = null, string? rightDocument = null)
    {
        var signals = new List<string>();
        if (!string.IsNullOrWhiteSpace(leftDocument) &&
            string.Equals(Normalize(leftDocument), Normalize(rightDocument), StringComparison.Ordinal))
        {
            signals.Add("normalized_document");
        }

        if (string.Equals(Normalize(left.FullName), Normalize(right.FullName), StringComparison.Ordinal))
        {
            signals.Add("normalized_name");
        }

        if (left.DateOfBirth.HasValue && left.DateOfBirth == right.DateOfBirth)
        {
            signals.Add("date_of_birth");
        }

        var kind = signals.Contains("normalized_document", StringComparer.Ordinal) &&
                   (signals.Contains("date_of_birth", StringComparer.Ordinal) || signals.Contains("normalized_name", StringComparer.Ordinal))
            ? DuplicateMatchKind.Exact
            : signals.Count >= 2 ? DuplicateMatchKind.Possible : DuplicateMatchKind.None;

        return new IdentityMatchResult(kind, signals);
    }

    private static string Normalize(string? value) =>
        string.Join(' ', (value ?? string.Empty).Trim().ToUpperInvariant().Normalize().Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
