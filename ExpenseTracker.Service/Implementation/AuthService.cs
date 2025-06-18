using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Models;
using ExpenseTracker.Models.Validations.Constants.ErrorMessages;
using ExpenseTracker.Models.Validations.Constants.SuccessMessages;
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

    public async Task<string?> LoginAsync(LoginRequestDto loginRequestDto)
    {
        User? user = await _userRepository.GetByEmailAsync(loginRequestDto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequestDto.Password, user.PasswordHash))
        {
            return null;
        }

        return _jwt.GenerateToken(user);
    }

    public async Task<User> RegisterAsync(RegisterRequestDto registerRequestDto)
    {
        if (await _userRepository.EmailOrUsernameExistsAsync(registerRequestDto.Email, registerRequestDto.Username))
        {
            throw new Exception(ErrorMessages.EmailOrUsernameExists);
        }

        User user = new User
        {
            Username = registerRequestDto.Username,
            Email = registerRequestDto.Email,
            Firstname = registerRequestDto.Firstname,
            Lastname = registerRequestDto.Lastname,
            Contactnumber = registerRequestDto.Contactnumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequestDto.Password),
            RoleId = registerRequestDto.RoleId ?? 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Address = registerRequestDto.Address,
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return user;
    }
}
