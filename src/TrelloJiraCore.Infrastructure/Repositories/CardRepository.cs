using Microsoft.EntityFrameworkCore;
using TrelloJiraCore.Core.Entities;
using TrelloJiraCore.Core.Interfaces;
using TrelloJiraCore.Infrastructure.Data;

namespace TrelloJiraCore.Infrastructure.Repositories;

public class CardRepository : Repository<Card>, ICardRepository
{
    public CardRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Card>> SearchCardsAsync(int boardId, string query)
    {
        return await _context.Cards
            .Include(c => c.List)
            .Where(c => c.List.BoardId == boardId && (c.Title.Contains(query) || c.Description.Contains(query)))
            .ToListAsync();
    }
}
