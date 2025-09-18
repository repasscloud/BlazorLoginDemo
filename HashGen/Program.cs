using Microsoft.AspNetCore.Identity;

var hasher = new PasswordHasher<IdentityUser>();
var user   = new IdentityUser { UserName = "admin@example.com", Email = "admin@example.com" };
var hash   = hasher.HashPassword(user, "YourStrongP@ss1");
Console.WriteLine(hash);
