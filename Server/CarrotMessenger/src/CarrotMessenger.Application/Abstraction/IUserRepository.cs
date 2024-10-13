using CarrotMessenger.Domain;

namespace CarrotMessenger.Identity;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetByQueryAsync(string query);
    Task AddUser (string query);
}