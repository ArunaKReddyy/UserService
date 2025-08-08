using Microsoft.AspNetCore.Identity;
using UserService.Domain.Entities;
namespace UserService.Infrastructure.Identity;
public class ApplicationUser : IdentityUser<Guid>
{
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? FullName { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    //public bool IsEmailConfirmed { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public ICollection<Address> Addresses { get; set; } = [];
    //public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
