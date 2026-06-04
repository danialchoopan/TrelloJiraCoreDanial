namespace TrelloJiraCore.Core.Entities;

public class Board
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<List> Lists { get; set; } = new List<List>();
}
