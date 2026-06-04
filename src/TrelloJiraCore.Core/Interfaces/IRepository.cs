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
    Task<IEnumerable<Board>> SearchBoardsAsync(string query);
}

public interface ICardRepository : IRepository<Card>
{
    Task<IEnumerable<Card>> SearchCardsAsync(int boardId, string query);
}

public interface IActivityLogRepository : IRepository<ActivityLog>
{
    Task<IEnumerable<ActivityLog>> GetByBoardIdAsync(int boardId);
}
