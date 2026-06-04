using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TrelloJiraCore.Core.Entities;
using TrelloJiraCore.Core.Interfaces;
using TrelloJiraCore.Web.DTOs;
using TrelloJiraCore.Web.Hubs;

namespace TrelloJiraCore.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    private readonly ICardRepository _cardRepository;
    private readonly IRepository<List> _listRepository;
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IHubContext<BoardHub> _hubContext;

    public CardsController(
        ICardRepository cardRepository,
        IRepository<List> listRepository,
        IActivityLogRepository activityLogRepository,
        IHubContext<BoardHub> hubContext)
    {
        _cardRepository = cardRepository;
        _listRepository = listRepository;
        _activityLogRepository = activityLogRepository;
        _hubContext = hubContext;
    }

    [HttpPut("{id}/move")]
    public async Task<IActionResult> MoveCard(int id, CardMoveDto moveDto)
    {
        var card = await _cardRepository.GetByIdAsync(id);
        if (card == null) return NotFound();

        int oldListId = card.ListId;
        card.ListId = moveDto.ToListId;
        card.Order = moveDto.NewOrder;

        _cardRepository.Update(card);

        var list = await _listRepository.GetByIdAsync(card.ListId);

        await _activityLogRepository.AddAsync(new ActivityLog
        {
            Action = $"کارت '{card.Title}' به لیست '{list?.Title}' منتقل شد",
            User = "کاربر سیستم",
            BoardId = list?.BoardId
        });

        await _cardRepository.SaveChangesAsync();

        if (list != null)
        {
            await _hubContext.Clients.Group($"Board_{list.BoardId}").SendAsync("OnCardMoved", id, oldListId, moveDto.ToListId);
        }

        return NoContent();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Card>> GetCard(int id)
    {
        var card = await _cardRepository.GetByIdAsync(id);
        if (card == null) return NotFound();
        return Ok(card);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCard(int id, CardUpdateDto updateDto)
    {
        var card = await _cardRepository.GetByIdAsync(id);
        if (card == null) return NotFound();

        card.Title = updateDto.Title;
        card.Description = updateDto.Description;
        card.DueDate = updateDto.DueDate;
        card.Priority = updateDto.Priority;
        card.Assignee = updateDto.Assignee;

        _cardRepository.Update(card);

        var list = await _listRepository.GetByIdAsync(card.ListId);
        await _activityLogRepository.AddAsync(new ActivityLog
        {
            Action = $"کارت '{card.Title}' ویرایش شد",
            User = "کاربر سیستم",
            BoardId = list?.BoardId
        });

        await _cardRepository.SaveChangesAsync();

        if (list != null)
        {
            await _hubContext.Clients.Group($"Board_{list.BoardId}").SendAsync("OnCardUpdated", id);
        }

        return NoContent();
    }
}
