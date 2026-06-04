using TrelloJiraCore.Core.Enums;

namespace TrelloJiraCore.Web.DTOs;

public class CardUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public Priority Priority { get; set; }
    public string Assignee { get; set; } = string.Empty;
}

public class CardMoveDto
{
    public int ToListId { get; set; }
    public int NewOrder { get; set; }
}
