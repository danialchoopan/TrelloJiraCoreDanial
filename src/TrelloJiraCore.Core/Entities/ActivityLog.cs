namespace TrelloJiraCore.Core.Entities;

public class ActivityLog
{
    public int Id { get; set; }
    public string User { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int? BoardId { get; set; }
    public Board? Board { get; set; }
}
