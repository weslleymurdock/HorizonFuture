using System.Globalization;
using System.Text;

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
        var normalizedLeftDocument = Normalize(leftDocument);
        var normalizedRightDocument = Normalize(rightDocument);
        if (!string.IsNullOrEmpty(normalizedLeftDocument) &&
            string.Equals(normalizedLeftDocument, normalizedRightDocument, StringComparison.Ordinal))
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

        var hasDocument = signals.Contains("normalized_document", StringComparer.Ordinal);
        var hasSupportingIdentity = signals.Contains("normalized_name", StringComparer.Ordinal) ||
            signals.Contains("date_of_birth", StringComparer.Ordinal);
        var kind = hasDocument && hasSupportingIdentity
            ? DuplicateMatchKind.Exact
            : signals.Count >= 2 ? DuplicateMatchKind.Possible : DuplicateMatchKind.None;

        return new IdentityMatchResult(kind, signals);
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decomposed = value.Trim().ToUpperInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return string.Join(' ', builder.ToString().Normalize(NormalizationForm.FormC)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
