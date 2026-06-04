namespace TrelloJiraCore.Core.Entities;

public class List
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int BoardId { get; set; }
    public Board? Board { get; set; }
    public int Order { get; set; }
    public ICollection<Card> Cards { get; set; } = new List<Card>();
}
