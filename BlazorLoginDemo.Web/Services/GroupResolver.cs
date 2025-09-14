// // Services/GroupResolver.cs  (namespace BlazorLoginDemo.Web.Data)
// using Microsoft.EntityFrameworkCore;
// using BlazorLoginDemo.Shared.Services.Interfaces;

// namespace BlazorLoginDemo.Web.Data;

// public class GroupResolver : IGroupResolver
// {
//     private readonly IServiceScopeFactory _scopeFactory;

//     public GroupResolver(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

//     public async Task<Guid?> ResolveGroupIdForEmailAsync(string? email, CancellationToken ct = default)
//     {
//         if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) return null;
//         var domain = email[(email.IndexOf('@') + 1)..].Trim().ToLowerInvariant();

//         using var scope = _scopeFactory.CreateScope();
//         var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

//         var mapped = await db.GroupDomains
//             .Where(d => d.Domain == domain)
//             .Select(d => d.GroupId)
//             .FirstOrDefaultAsync(ct);

//         if (mapped != Guid.Empty) return mapped;

//         var catchAll = await db.Groups
//             .Where(g => g.IsCatchAll && g.IsActive)
//             .Select(g => g.Id)
//             .FirstOrDefaultAsync(ct);

//         return catchAll == Guid.Empty ? (Guid?)null : catchAll;
//     }
// }
