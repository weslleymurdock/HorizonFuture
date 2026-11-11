using HF.Domain.Common.ValueObjects;

namespace HF.Domain.SocialServices.Activities;

public sealed class SessionFacilitator : ValueObject
{
    public SessionFacilitator(string personId, FacilitatorRole role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(personId);

        PersonId = personId.Trim();
        Role = role;
    }

    public string PersonId { get; }
    public FacilitatorRole Role { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return PersonId;
        yield return Role;
    }
}
