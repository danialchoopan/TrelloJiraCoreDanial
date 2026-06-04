using Microsoft.EntityFrameworkCore;
using TrelloJiraCore.Core.Entities;
using TrelloJiraCore.Core.Interfaces;
using TrelloJiraCore.Infrastructure.Data;

namespace TrelloJiraCore.Infrastructure.Repositories;

public class ActivityLogRepository : Repository<ActivityLog>, IActivityLogRepository
{
    public ActivityLogRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ActivityLog>> GetByBoardIdAsync(int boardId)
    {
        return await _context.ActivityLogs
            .Where(a => a.BoardId == boardId)
            .OrderByDescending(a => a.Timestamp)
            .Take(50)
            .ToListAsync();
    }
}
