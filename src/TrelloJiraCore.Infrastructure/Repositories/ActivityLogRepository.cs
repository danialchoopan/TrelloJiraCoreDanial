using TrelloJiraCore.Core.Entities;
using TrelloJiraCore.Core.Interfaces;
using TrelloJiraCore.Infrastructure.Data;

namespace TrelloJiraCore.Infrastructure.Repositories;

public class ActivityLogRepository : Repository<ActivityLog>, IActivityLogRepository
{
    public ActivityLogRepository(AppDbContext context) : base(context)
    {
    }
}
