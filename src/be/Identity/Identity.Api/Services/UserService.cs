using Microsoft.EntityFrameworkCore;
using Identity.Api.Configuration;
using Identity.Api.Models;

namespace Identity.Api.Services;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetOrCreateUserAsync(SocialUserInfo socialUserInfo);
    Task<UserInfo> MapToUserInfoAsync(User user);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> DeactivateUserAsync(Guid userId);
}

public class UserService(IdentityDbContext context, ILogger<UserService> logger) : IUserService
{
    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await context.Users
            .Include(u => u.UserLogins)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await context.Users
            .Include(u => u.UserLogins)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetOrCreateUserAsync(SocialUserInfo socialUserInfo)
    {
        try
        {
            // First, try to find existing user by email
            var existingUser = await GetUserByEmailAsync(socialUserInfo.Email);
            
            if (existingUser != null)
            {
                // Check if this social login already exists
                var existingLogin = existingUser.UserLogins
                    .FirstOrDefault(ul => ul.Provider == socialUserInfo.Provider && 
                                         ul.ProviderUserId == socialUserInfo.Id);
                
                if (existingLogin == null)
                {
                    // Add new social login to existing user
                    var newLogin = new UserLogin
                    {
                        UserId = existingUser.Id,
                        Provider = socialUserInfo.Provider,
                        ProviderUserId = socialUserInfo.Id,
                        ProviderDisplayName = socialUserInfo.Name,
                        LastLoginAt = DateTime.UtcNow
                    };
                    
                    context.UserLogins.Add(newLogin);
                }
                else
                {
                    // Update last login time
                    existingLogin.LastLoginAt = DateTime.UtcNow;
                }
                
                // Update user info if needed
                if (existingUser.Name != socialUserInfo.Name || existingUser.PictureUrl != socialUserInfo.PictureUrl)
                {
                    existingUser.Name = socialUserInfo.Name;
                    existingUser.PictureUrl = socialUserInfo.PictureUrl;
                    existingUser.UpdatedAt = DateTime.UtcNow;
                }
                
                await context.SaveChangesAsync();
                return existingUser;
            }
            
            // Create new user
            var newUser = new User
            {
                Email = socialUserInfo.Email,
                Name = socialUserInfo.Name,
                PictureUrl = socialUserInfo.PictureUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            context.Users.Add(newUser);
            await context.SaveChangesAsync();
            
            // Add social login
            var userLogin = new UserLogin
            {
                UserId = newUser.Id,
                Provider = socialUserInfo.Provider,
                ProviderUserId = socialUserInfo.Id,
                ProviderDisplayName = socialUserInfo.Name,
                LastLoginAt = DateTime.UtcNow
            };
            
            context.UserLogins.Add(userLogin);
            await context.SaveChangesAsync();
            
            // Reload with navigation properties
            return await GetUserByIdAsync(newUser.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get or create user for {Email}", socialUserInfo.Email);
            return null;
        }
    }    public Task<UserInfo> MapToUserInfoAsync(User user)
    {        var providers = user.UserLogins.Select(ul => ul.Provider).Distinct().ToList();
        
        var userInfo = new UserInfo
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            PictureUrl = user.PictureUrl,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Providers = providers
        };
        
        return Task.FromResult(userInfo);
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        try
        {
            user.UpdatedAt = DateTime.UtcNow;
            context.Users.Update(user);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update user {UserId}", user.Id);
            return false;
        }
    }

    public async Task<bool> DeactivateUserAsync(Guid userId)
    {
        try
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return false;
            
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deactivate user {UserId}", userId);
            return false;
        }
    }
}
