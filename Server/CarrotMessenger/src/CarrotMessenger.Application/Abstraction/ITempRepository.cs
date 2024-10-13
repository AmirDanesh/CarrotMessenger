namespace CarrotMessenger.Application.Abstraction;

public interface ITempRepository
{
    void AddTempUser(string username, string password, string email);
}