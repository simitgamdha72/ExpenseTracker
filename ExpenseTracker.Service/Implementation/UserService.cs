using ExpenseTracker.Models.Dto;
using ExpenseTracker.Repository.Interface;
using ExpenseTracker.Service.Interface;

namespace ExpenseTracker.Service.Implementation;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileResponseDto?> GetUserByIdAsync(int? userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }
}
