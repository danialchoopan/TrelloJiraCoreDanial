using Microsoft.AspNetCore.Mvc;
using TrelloJiraCore.Core.Entities;
using TrelloJiraCore.Core.Interfaces;

namespace TrelloJiraCore.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivityLogsController : ControllerBase
{
    private readonly IActivityLogRepository _activityLogRepository;

    public ActivityLogsController(IActivityLogRepository activityLogRepository)
    {
        _activityLogRepository = activityLogRepository;
    }

    [HttpGet("board/{boardId}")]
    public async Task<ActionResult<IEnumerable<ActivityLog>>> GetByBoard(int boardId)
    {
        return Ok(await _activityLogRepository.GetByBoardIdAsync(boardId));
    }
}
