using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrelloJiraCore.Core.Entities;
using TrelloJiraCore.Core.Interfaces;
using TrelloJiraCore.Infrastructure.Data;

namespace TrelloJiraCore.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BoardsController : ControllerBase
{
    private readonly IBoardRepository _boardRepository;

    public BoardsController(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Board>>> GetBoards([FromQuery] string? search)
    {
        if (string.IsNullOrEmpty(search))
            return Ok(await _boardRepository.GetAllAsync());

        return Ok(await _boardRepository.SearchBoardsAsync(search));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Board>> GetBoard(int id)
    {
        var board = await _boardRepository.GetBoardWithDetailsAsync(id);
        if (board == null) return NotFound();
        return Ok(board);
    }

    [HttpPost]
    public async Task<ActionResult<Board>> CreateBoard(Board board)
    {
        await _boardRepository.AddAsync(board);
        await _boardRepository.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBoard), new { id = board.Id }, board);
    }
}
