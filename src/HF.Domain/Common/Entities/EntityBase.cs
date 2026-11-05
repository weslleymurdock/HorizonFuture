namespace HF.Domain.Common.Entities;

public interface IEntityBase
{
    string Id { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
    string CreatedBy { get; set; }
    string? UpdatedBy { get; set; }
}


public class EntityBase(string Id) : IEntityBase
{
    public string Id { get; set; } = Id ?? Guid.CreateVersion7().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
}
