using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AiForge.Application.DTOs.Auth;
using AiForge.Application.Services;
using AiForge.Domain.Entities;
using AiForge.Infrastructure.Data;
using AiForge.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AiForge.Api.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AiForgeDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        AiForgeDbContext dbContext,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new AuthResponse { Success = false, Error = "User with this email already exists" };
        }

        // Get or create default organization
        var defaultOrg = await _dbContext.Organizations.FirstOrDefaultAsync(cancellationToken);

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            DefaultOrganizationId = defaultOrg?.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new AuthResponse { Success = false, Error = errors };
        }

        // Add default role
        await _userManager.AddToRoleAsync(user, "User");

        // Add to default organization if exists
        if (defaultOrg != null)
        {
            var membership = new OrganizationMember
            {
                Id = Guid.NewGuid(),
                OrganizationId = defaultOrg.Id,
                UserId = user.Id,
                Role = Domain.Enums.OrganizationRole.Member,
                JoinedAt = DateTime.UtcNow
            };
            _dbContext.OrganizationMembers.Add(membership);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var tokens = await GenerateTokensAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResponse
        {
            Success = true,
            User = MapToUserDto(user, roles),
            Tokens = tokens
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new AuthResponse { Success = false, Error = "Invalid email or password" };
        }

        if (!user.IsActive)
        {
            return new AuthResponse { Success = false, Error = "Account is disabled" };
        }

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
        {
            return new AuthResponse { Success = false, Error = "Invalid email or password" };
        }

        var tokens = await GenerateTokensAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResponse
        {
            Success = true,
            User = MapToUserDto(user, roles),
            Tokens = tokens
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        // In a production system, you'd store refresh tokens in the database
        // For now, we'll validate the refresh token as a JWT
        var principal = GetPrincipalFromExpiredToken(refreshToken);
        if (principal == null)
        {
            return new AuthResponse { Success = false, Error = "Invalid refresh token" };
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return new AuthResponse { Success = false, Error = "Invalid refresh token" };
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            return new AuthResponse { Success = false, Error = "User not found or inactive" };
        }

        var tokens = await GenerateTokensAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResponse
        {
            Success = true,
            User = MapToUserDto(user, roles),
            Tokens = tokens
        };
    }

    public Task<bool> LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // In a production system with stored refresh tokens, you'd invalidate them here
        // For now, logout is handled client-side by discarding tokens
        return Task.FromResult(true);
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return MapToUserDto(user, roles);
    }

    private async Task<TokenPair> GenerateTokensAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var accessTokenExpiration = DateTime.UtcNow.AddMinutes(
            _configuration.GetValue<int>("Jwt:AccessTokenExpirationMinutes", 15));
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(
            _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7));

        var accessToken = GenerateJwtToken(user, roles, accessTokenExpiration);
        var refreshToken = GenerateJwtToken(user, roles, refreshTokenExpiration, isRefreshToken: true);

        return new TokenPair
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = accessTokenExpiration,
            RefreshTokenExpiresAt = refreshTokenExpiration
        };
    }

    private string GenerateJwtToken(ApplicationUser user, IList<string> roles, DateTime expiration, bool isRefreshToken = false)
    {
        var secret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret not configured");
        var issuer = _configuration["Jwt:Issuer"] ?? "AiForge";
        var audience = _configuration["Jwt:Audience"] ?? "AiForge";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(ClaimTypes.Name, user.DisplayName),
            new("token_type", isRefreshToken ? "refresh" : "access")
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var secret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret not configured");

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateLifetime = false, // Allow expired tokens for refresh
            ValidIssuer = _configuration["Jwt:Issuer"] ?? "AiForge",
            ValidAudience = _configuration["Jwt:Audience"] ?? "AiForge"
        };

        try
        {
            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            // Verify it's a refresh token
            var tokenType = principal.FindFirst("token_type")?.Value;
            if (tokenType != "refresh")
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private static UserDto MapToUserDto(ApplicationUser user, IEnumerable<string> roles)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? "",
            DisplayName = user.DisplayName,
            DefaultOrganizationId = user.DefaultOrganizationId,
            IsActive = user.IsActive,
            Roles = roles
        };
    }
}
