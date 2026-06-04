using TrelloJiraCore.Core.Entities;
using TrelloJiraCore.Core.Interfaces;
using TrelloJiraCore.Infrastructure.Data;

namespace TrelloJiraCore.Infrastructure.Repositories;

public class CardRepository : Repository<Card>, ICardRepository
{
    public CardRepository(AppDbContext context) : base(context)
    {
    }
}
