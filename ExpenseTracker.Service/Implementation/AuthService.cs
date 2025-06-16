using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Models;
using ExpenseTracker.Repository.Interface;
using ExpenseTracker.Service.Interface;
using ExpenseTracker.Service.Validations;

namespace ExpenseTracker.Service.Implementation;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtService _jwt;

    public AuthService(IUserRepository userRepository, JwtService jwt)
    {
        _userRepository = userRepository;
        _jwt = jwt;
    }

    public async Task<string?> LoginAsync(LoginDto loginDto)
    {
        User? user = await _userRepository.GetByEmailAsync(loginDto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            return null;
        }

        return _jwt.GenerateToken(user);
    }

    public async Task<string> RegisterAsync(RegisterDto registerDto)
    {
        if (await _userRepository.EmailOrUsernameExistsAsync(registerDto.Email, registerDto.Username))
        {
            throw new Exception("Exist");
        }

        User user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            Firstname = registerDto.Firstname,
            Lastname = registerDto.Lastname,
            Phone = registerDto.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            RoleId = registerDto.RoleId ?? 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Address = registerDto.Address,
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return "User registered successfully";
    }
}
