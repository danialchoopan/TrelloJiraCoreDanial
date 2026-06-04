using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrelloJiraCore.Core.Entities;
using TrelloJiraCore.Infrastructure.Data;

namespace TrelloJiraCore.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.Users.ToListAsync();
    }
}
