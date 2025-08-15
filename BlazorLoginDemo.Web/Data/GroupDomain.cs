using System.ComponentModel.DataAnnotations;

namespace BlazorLoginDemo.Web.Data;

public class GroupDomain
{
    public int Id { get; set; }

    public Guid GroupId { get; set; }
    public Group Group { get; set; } = default!;

    [MaxLength(190)] // safe for indexes
    public string Domain { get; set; } = default!; // store lowercase like "contoso.com"
}
