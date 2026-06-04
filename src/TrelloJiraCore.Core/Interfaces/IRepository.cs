using TrelloJiraCore.Core.Entities;

namespace TrelloJiraCore.Core.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task SaveChangesAsync();
}

public interface IBoardRepository : IRepository<Board>
{
    Task<Board?> GetBoardWithDetailsAsync(int id);
}

public interface ICardRepository : IRepository<Card>
{
}

public interface IActivityLogRepository : IRepository<ActivityLog>
{
}
