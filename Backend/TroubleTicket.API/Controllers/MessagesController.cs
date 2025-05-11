using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TroubleTicket.Core.Entities;
using TroubleTicket.Core.Interfaces;

namespace TroubleTicket.API.Controllers;

[Authorize]
public class MessagesController : BaseController
{
    private readonly IJsonStorageService _storage;
    private readonly string TicketsFileName;

    public MessagesController(IJsonStorageService storage)
    {
        _storage = storage;
        TicketsFileName = Path.Combine(Directory.GetCurrentDirectory(), "Data", "messages.json");
    }

    [HttpGet("tickets/{ticketId}/messages")]
    public async Task<IActionResult> GetTicketMessages(string ticketId)
    {
        var tickets = await _storage.ReadOrCreateAsync<List<Ticket>>(TicketsFileName);
        var ticket = tickets.FirstOrDefault(t => t.Id == ticketId);

        if (ticket == null)
        {
            return NotFound();
        }

        if (!IsAdmin() && !IsServiceAgent() && ticket.CreatedById != GetUserId())
        {
            return Forbid();
        }

        return Ok(ticket.Messages);
    }

    [HttpPost("tickets/{ticketId}/messages")]
    public async Task<IActionResult> AddMessage(string ticketId, [FromBody] Message message)
    {
        var tickets = await _storage.ReadOrCreateAsync<List<Ticket>>(TicketsFileName);
        var ticket = tickets.FirstOrDefault(t => t.Id == ticketId);

        if (ticket == null)
        {
            return NotFound();
        }

        if (!IsAdmin() && !IsServiceAgent() && ticket.CreatedById != GetUserId())
        {
            return Forbid();
        }

        message.Id = Guid.NewGuid().ToString("N");
        message.TicketId = ticketId;
        message.UserId = GetUserId();
        message.CreatedAt = DateTime.UtcNow;

        ticket.Messages.Add(message);
        ticket.UpdatedAt = DateTime.UtcNow;

        await _storage.WriteAsync(TicketsFileName, tickets);

        return Ok(message);
    }
}