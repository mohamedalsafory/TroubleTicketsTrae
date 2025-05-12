using Microsoft.AspNetCore.Mvc;
using TroubleTicket.Core.Entities;
using TroubleTicket.Core.Interfaces;
using System.Text.Json.Serialization;
using System.IO;

namespace TroubleTicket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly IJsonStorageService _storage;
    private readonly string FileName;

    public TicketsController(IJsonStorageService storage)
    {
        _storage = storage;
        FileName = Path.Combine(Directory.GetCurrentDirectory(), "Data", "tickets.json");
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var tickets = await _storage.ReadOrCreateAsync<List<Ticket>>(FileName);
            
            if (tickets == null)
            {
                return Ok(new List<Ticket>());
            }
            
            return Ok(tickets);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting tickets: {ex}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        try
        {
            var tickets = await _storage.ReadOrCreateAsync<List<Ticket>>(FileName);
            
            Console.WriteLine($"Looking for ticket with ID: {id}");
            Console.WriteLine($"Available ticket IDs: {string.Join(", ", tickets?.Select(t => t.Id) ?? Array.Empty<string>())}");
            
            if (tickets == null)
            {
                return NotFound(new { message = "No tickets found" });
            }
            
            var ticket = tickets.FirstOrDefault(t => t.Id == id);
            
            if (ticket == null)
            {
                return NotFound(new { message = $"Ticket with ID {id} not found" });
            }
            
            return Ok(ticket);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting ticket: {ex}");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Ticket ticket)
    {
        // Validate required fields
        if (string.IsNullOrEmpty(ticket.Title) || 
            string.IsNullOrEmpty(ticket.Description))
        {
            return BadRequest("Title and description are required");
        }

        var tickets = await _storage.ReadOrCreateAsync<List<Ticket>>(FileName);
        
        ticket.Id = Guid.NewGuid().ToString("N");
        ticket.CreatedAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;
    
        // Log ticket details
        Console.WriteLine($"Creating ticket with ID: {ticket.Id}");
        Console.WriteLine($"Title: {ticket.Title}");
        Console.WriteLine($"Description: {ticket.Description}");
        
        tickets.Add(ticket);
        await _storage.WriteAsync(FileName, tickets);

        return CreatedAtAction(nameof(Get), new { id = ticket.Id }, ticket);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Ticket update)
    {
        var tickets = await _storage.ReadOrCreateAsync<List<Ticket>>(FileName);
        var ticket = tickets.FirstOrDefault(t => t.Id == id);

        if (ticket == null)
        {
            return NotFound();
        }

        ticket.Title = update.Title;
        ticket.Description = update.Description;
        ticket.Status = update.Status;
        ticket.Priority = update.Priority;
        ticket.CategoryId = update.CategoryId;
        ticket.AssignedToId = update.AssignedToId;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _storage.WriteAsync(FileName, tickets);

        return Ok(ticket);
    }
}

public class TicketResponse
{
    [JsonPropertyName("tickets")]
    public List<Ticket> Tickets { get; set; } = new List<Ticket>();
}