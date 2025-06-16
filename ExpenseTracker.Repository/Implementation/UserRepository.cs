using ExpenseTracker.Models.Models;
using ExpenseTracker.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Repository.Implementation;

public class UserRepository : Repository<User>, IUserRepository
{
    private readonly ExpenseTrackerContext _context;

    public UserRepository(ExpenseTrackerContext context) : base(context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> EmailOrUsernameExistsAsync(string email, string username)
    {
        return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower() || u.Username.ToLower() == username.ToLower());
    }
}
