using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MaoCerta.Application.DTOs;
using MaoCerta.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MaoCerta.Application.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    }

    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("User with email {Email} already exists", registerDto.Email);
                throw new Exception("Este email já está cadastrado.");
            }

            var user = new ApplicationUser
            {
                UserName = registerDto.FirstName,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.Phone,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errorMessages = result.Errors.Select(e => e.Description).ToList();
                _logger.LogError("Failed to create user: {Errors}", string.Join(", ", errorMessages));
                throw new Exception(string.Join(" ", errorMessages));
            }

            _logger.LogInformation("User {Email} registered successfully", registerDto.Email);
            return await GenerateTokenAsync(user);
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login attempt with non-existent email: {Email}", loginDto.Email);
                    return null;
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Failed login attempt for user: {Email}", loginDto.Email);
                    return null;
                }

                _logger.LogInformation("User {Email} logged in successfully", loginDto.Email);
                return await GenerateTokenAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return null;
            }
        }

        private async Task<AuthResponseDto> GenerateTokenAsync(ApplicationUser user)
        {
            var secretKey = GetRequiredEnvVar("JWT_SECRET_KEY");
            var issuer = GetRequiredEnvVar("JWT_ISSUER");
            var audience = GetRequiredEnvVar("JWT_AUDIENCE");
            var expiryMinutes = int.Parse(GetRequiredEnvVar("JWT_EXPIRY_MINUTES"));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName)
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            var userRole = roles.FirstOrDefault() ?? "User";

            return new AuthResponseDto
            {
                Token = tokenString,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
                User = new DTOs.UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    Name = $"{user.FirstName} {user.LastName}",
                    Role = userRole
                }
            };
        }

        public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("Usuario nao encontrado.");
            }
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Falha ao alterar senha para {UserId}: {Errors}", userId, errors);
                throw new Exception(errors);
            }
            _logger.LogInformation("Senha alterada para usuario {UserId}", userId);
        }
        private static string GetRequiredEnvVar(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Environment variable '{key}' is missing. Please configure it in your .env file.");
            }

            return value;
        }
    }
}
