using TrelloJiraCore.Core.Enums;

namespace TrelloJiraCore.Core.Entities;

public class Card
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public Priority Priority { get; set; }
    public string Assignee { get; set; } = string.Empty;
    public int ListId { get; set; }
    public List? List { get; set; }
    public int Order { get; set; }
}
