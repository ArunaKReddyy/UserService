using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Infrastructure.Identity;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Repositories;

public class UserRepository(UserManager<ApplicationUser> user, UserDbContext dbContext) : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager = user;
    private readonly UserDbContext _dbContext = dbContext;
    public async Task<bool> CreateUserAsync(User user, string password)
    {

        return (await _userManager.CreateAsync(MapToDomain(user), password)).Succeeded;
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        var applicationUser = await _userManager.FindByEmailAsync(email);
        return applicationUser != null ? MapToDomain(applicationUser) : null;
    }
    public async Task<User?> FindByUserNameAsync(string userName)
    {
        var appUser = await _userManager.FindByNameAsync(userName);
        if (appUser == null)
            return null;

        return MapToDomain(appUser);
    }
    public async Task<User?> FindByIdAsync(Guid id)
    {
        var applicationUser = await _userManager.FindByIdAsync(id.ToString());

        return applicationUser != null ? MapToDomain(applicationUser) : null;
    }

    public async Task<bool> AssignRoleAsync(User user, string roleName)
    {
        var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (applicationUser == null) return false;

        return (await _userManager.AddToRoleAsync(applicationUser, roleName)).Succeeded;
    }

    private static ApplicationUser MapToDomain(User user)
    {
        return new ApplicationUser
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            //IsEmailConfirmed = user.IsEmailConfirmed,
            IsActive = user.IsActive,
            PhoneNumber = user.PhoneNumber,
            FullName = user.FullName,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Addresses = [.. user.Addresses]
        };
    }

    private static User? MapToDomain(ApplicationUser applicationUser)
    {
        return new User
        {
            Id = applicationUser.Id,
            UserName = applicationUser.UserName,
            Email = applicationUser.Email,
            IsEmailConfirmed = applicationUser.EmailConfirmed,
            IsActive = applicationUser.IsActive,
            PhoneNumber = applicationUser.PhoneNumber,
            FullName = applicationUser.FullName,
            ProfilePhotoUrl = applicationUser.ProfilePhotoUrl,
            CreatedAt = applicationUser.CreatedAt,
            LastLoginAt = applicationUser.LastLoginAt,
            Addresses = applicationUser.Addresses.ToList()
        };
    }

    public async Task<string?> GenerateEmailConfirmationTokenAsync(User user)
    {
        var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (applicationUser == null) return null;
        return await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
    }
    public async Task<bool> VerifyConfirmaionEmailAsync(User user, string token)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser == null)
            return false;

        var result = await _userManager.ConfirmEmailAsync(appUser, token);
        return result.Succeeded;
    }
    public async Task<bool> UpdateUserAsync(User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser == null)
            return false;

        appUser.UserName = user.UserName;
        appUser.Email = user.Email;
        appUser.FullName = user.FullName;
        appUser.PhoneNumber = user.PhoneNumber;
        appUser.ProfilePhotoUrl = user.ProfilePhotoUrl;

        var result = await _userManager.UpdateAsync(appUser);
        return result.Succeeded;
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser == null)
            return null;

        var token = await _userManager.GeneratePasswordResetTokenAsync(appUser);
        return token;
    }

    public async Task<bool> ResetPasswordAsync(User user, string token, string newPassword)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser == null)
            return false;

        var result = await _userManager.ResetPasswordAsync(appUser, token, newPassword);
        return result.Succeeded;
    }

    public async Task<bool> ChangePasswordAsync(User user, string currentPassword, string newPassword)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser == null)
            return false;

        var result = await _userManager.ChangePasswordAsync(appUser, currentPassword, newPassword);
        return result.Succeeded;
    }

    #region Address
    public async Task<Guid> AddOrUpdateAddressAsync(Address address)
    {
        var existing = await _dbContext.Addresses.FindAsync(address.Id);
        if (existing == null)
        {
            await _dbContext.Addresses.AddAsync(address);
            await _dbContext.SaveChangesAsync();
            return address.Id; // New address Id
        }
        else
        {
            existing.AddresLine1 = address.AddresLine1;
            existing.AddresLine2 = address.AddresLine2;
            existing.City = address.City;
            existing.State = address.State;
            existing.PostalCode = address.PostalCode;
            existing.Country = address.Country;
            existing.IsDefaultBilling = address.IsDefaultBilling;
            existing.IsDefaultShipping = address.IsDefaultShipping;
            await _dbContext.SaveChangesAsync();
            return existing.Id; // Existing address Id
        }
    }
    public async Task<Address?> GetAddressByUserIdAndAddressIdAsync(Guid userId, Guid addressId)
    {
        return await _dbContext.Addresses.
            AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Id == addressId);
    }
    public async Task<bool> DeleteAddressAsync(Guid userId, Guid addressId)
    {
        var address = await _dbContext.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
        if (address == null)
            return false;

        _dbContext.Addresses.Remove(address);
        await _dbContext.SaveChangesAsync();
        return true;
    }
    public async Task<List<Address>> GetAddressesByUserIdAsync(Guid userId)
    {
        return await _dbContext.Addresses.Where(a => a.UserId == userId).ToListAsync();
    }

    #endregion

    #region Login
    public async Task<bool> IsLockedOutAsync(User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        return appUser != null && await _userManager.IsLockedOutAsync(appUser);
    }

    public async Task<bool> IsTwoFactorEnabledAsync(User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        return appUser != null && await _userManager.GetTwoFactorEnabledAsync(appUser);
    }

    public async Task IncrementAccessFailedCountAsync(User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser != null)
            await _userManager.AccessFailedAsync(appUser);
    }

    public async Task ResetAccessFailedCountAsync(User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser != null)
            await _userManager.ResetAccessFailedCountAsync(appUser);
    }

    public async Task<DateTime?> GetLockoutEndDateAsync(User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser == null)
            return null;

        // LockoutEnd can be null, so return nullable DateTime
        return appUser.LockoutEnd?.UtcDateTime;
    }

    public Task<int> GetMaxFailedAccessAttemptsAsync()
    {
        return Task.FromResult(_userManager.Options.Lockout.MaxFailedAccessAttempts);
    }

    public async Task<int> GetAccessFailedCountAsync(User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        return appUser?.AccessFailedCount ?? 0;
    }

    public async Task<bool> IsValidClientAsync(string clientId)
    {
        return await _dbContext.Clients.AnyAsync(c => c.ClientId == clientId);
    }
    public async Task<bool> CheckPasswordAsync(User user, string password)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser == null)
            return false;

        return await _userManager.CheckPasswordAsync(appUser, password);
    }
    public async Task<IList<string>> GetUserRolesAsync(User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser == null)
            return new List<string>();

        return await _userManager.GetRolesAsync(appUser);
    }
    public async Task UpdateLastLoginAsync(User user, DateTime loginTime)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser == null) return;
        appUser.LastLoginAt = loginTime;
        await _userManager.UpdateAsync(appUser);
    }

    public async Task<string> GenerateAndStoreRefreshTokenAsync(Guid userId, string clientId, string userAgent, string ipAddress)
    {
        // Revoke existing tokens for this user/client/useragent before issuing a new one
        await RevokeAllRefreshTokensAsync(userId, clientId, userAgent, ipAddress);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ClientId = clientId,
            UserAgent = userAgent,
            Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = ipAddress
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        return refreshToken.Token;
    }
    public async Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string ipAddress)
    {
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        await _dbContext.SaveChangesAsync();
    }
    private async Task RevokeAllRefreshTokensAsync(Guid userId, string clientId, string userAgent, string revokedByIp)
    {
        var tokens = await _dbContext.RefreshTokens
            .Where(t => t.UserId == userId
                && t.ClientId == clientId
                && t.UserAgent == userAgent
                && t.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = revokedByIp;
        }

        await _dbContext.SaveChangesAsync();
    }
    public async Task<bool> IsUserExistsAsync(Guid userId)
    {
        return await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Id == userId);
    }
    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
    }
    #endregion
}
