namespace CarrotMessenger.Infrastructure.Common.Persistence;

public interface IUnitOfWork
{
    Task CommitChangesAsync();
}