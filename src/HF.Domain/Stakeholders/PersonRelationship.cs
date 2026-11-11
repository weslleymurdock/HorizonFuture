using HF.Domain.Common.Aggregates;
using HF.Domain.Common.Exceptions;

namespace HF.Domain.Stakeholders;

public enum PersonRelationshipType
{
    Parent = 1,
    LegalGuardian = 2,
    Dependent = 3,
    Representative = 4,
    EmergencyContact = 5,
    FamilyMember = 6
}

public sealed class PersonRelationship : AggregateRoot
{
    private PersonRelationship() { }

    public PersonRelationship(
        string personId,
        string relatedPersonId,
        PersonRelationshipType type,
        DateOnly startsOn,
        string? id = null) : base(id)
    {
        if (personId == relatedPersonId)
        {
            throw new DomainException("person_relationship.self_reference", "A person cannot have a relationship with itself.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(personId);
        ArgumentException.ThrowIfNullOrWhiteSpace(relatedPersonId);
        if (!Enum.IsDefined(type))
        {
            throw new DomainException("person_relationship.invalid_type", "The relationship type is invalid.");
        }

        PersonId = personId;
        RelatedPersonId = relatedPersonId;
        Type = type;
        StartsOn = startsOn;
    }

    public string PersonId { get; private set; } = string.Empty;
    public string RelatedPersonId { get; private set; } = string.Empty;
    public PersonRelationshipType Type { get; private set; }
    public DateOnly StartsOn { get; private set; }
    public DateOnly? EndsOn { get; private set; }

    public void End(DateOnly endedOn)
    {
        if (EndsOn.HasValue)
        {
            throw new DomainException("person_relationship.already_ended", "The relationship is already ended.");
        }

        if (endedOn < StartsOn)
        {
            throw new DomainException("person_relationship.invalid_period", "The relationship cannot end before it starts.");
        }

        EndsOn = endedOn;
        UpdatedAt = DateTime.UtcNow;
    }
}
