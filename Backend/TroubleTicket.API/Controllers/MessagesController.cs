using Microsoft.AspNetCore.Mvc;
using TroubleTicket.Core.Entities;
using TroubleTicket.Core.Interfaces;

namespace TroubleTicket.API.Controllers;

public class MessagesController : BaseController
{
    private readonly IJsonStorageService _storage;
    private readonly string FileName;

    public MessagesController(IJsonStorageService storage)
    {
        _storage = storage;
        FileName = Path.Combine(Directory.GetCurrentDirectory(), "Data", "messages.json");
    }

    [HttpGet("tickets/{ticketId}/messages")]
    public async Task<IActionResult> GetTicketMessages(string ticketId)
    {
        try
        {
            var allMessages = await _storage.ReadOrCreateAsync<List<Message>>(FileName);
            var messages = allMessages?.Where(m => m.TicketId == ticketId).ToList();
            
            if (messages == null || !messages.Any())
            {
                return Ok(new List<Message>());
            }
            
            return Ok(messages);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting messages: {ex}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("tickets/{ticketId}/messages")]
    public async Task<IActionResult> AddMessage(string ticketId, [FromBody] Message message)
    {
        var tickets = await _storage.ReadOrCreateAsync<List<Ticket>>(Path.Combine(Directory.GetCurrentDirectory(), "Data", "messages.json"));
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

        await _storage.WriteAsync(Path.Combine(Directory.GetCurrentDirectory(), "Data", "messages.json"), tickets);

        return Ok(message);
    }
}