using Microsoft.EntityFrameworkCore;
using TrelloJiraCore.Core.Entities;
using TrelloJiraCore.Core.Interfaces;
using TrelloJiraCore.Infrastructure.Data;

namespace TrelloJiraCore.Infrastructure.Repositories;

public class BoardRepository : Repository<Board>, IBoardRepository
{
    public BoardRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Board?> GetBoardWithDetailsAsync(int id)
    {
        return await _context.Boards
            .Include(b => b.Lists.OrderBy(l => l.Order))
            .ThenInclude(l => l.Cards.OrderBy(c => c.Order))
            .FirstOrDefaultAsync(b => b.Id == id);
    }
}
