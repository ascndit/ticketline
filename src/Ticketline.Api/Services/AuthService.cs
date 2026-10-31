using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ticketline.Api.Data;
using Ticketline.Api.Dtos;
using Ticketline.Api.Models;

namespace Ticketline.Api.Services;

public class AuthService(AppDbContext db, IPasswordHasher<User> hasher, TokenService tokens)
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var exists = await db.Users.AnyAsync(user => user.Email == email);
        if (exists)
        {
            throw new InvalidOperationException("Пользователь с таким email уже зарегистрирован.");
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = hasher.HashPassword(user, request.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return ToResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(item => item.Email == email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Неверный email или пароль.");
        }

        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Неверный email или пароль.");
        }

        return ToResponse(user);
    }

    private AuthResponse ToResponse(User user)
    {
        return new AuthResponse
        {
            Token = tokens.Create(user),
            Name = user.Name,
            Email = user.Email
        };
    }
}
