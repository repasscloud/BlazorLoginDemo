using System.ComponentModel.DataAnnotations;

namespace BlazorLoginDemo.Web.Data;

public class Group
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(128)]
    public string Name { get; set; } = default!;   // e.g., "Contoso"

    public bool IsActive { get; set; } = true;

    // Exactly one group should have this set (the fallback)
    public bool IsCatchAll { get; set; } = false;

    public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<GroupDomain> Domains { get; set; } = new List<GroupDomain>();
}
