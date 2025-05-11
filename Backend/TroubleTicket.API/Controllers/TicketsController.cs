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
        var response = await _storage.ReadOrCreateAsync<TicketResponse>(FileName);
        
        // Debugging output
        Console.WriteLine($"File path: {FileName}");
        Console.WriteLine($"Raw JSON content: {System.IO.File.ReadAllText(FileName)}");
        Console.WriteLine($"Deserialized response: {System.Text.Json.JsonSerializer.Serialize(response)}");
        
        if (response?.Tickets == null)
        {
            Console.WriteLine("Tickets list is null");
            return Ok(new List<Ticket>());
        }

        // Log each ticket's details
        foreach (var ticket in response.Tickets)
        {
            Console.WriteLine($"Ticket ID: {ticket.Id}");
            Console.WriteLine($"Title: {ticket.Title}");
            Console.WriteLine($"Description: {ticket.Description}");
            Console.WriteLine($"CategoryId: {ticket.CategoryId}");
            Console.WriteLine("----");
        }
        
        return Ok(response.Tickets);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var response = await _storage.ReadOrCreateAsync<TicketResponse>(FileName);
        Console.WriteLine($"Looking for ticket with ID: {id}");
        Console.WriteLine($"Available ticket IDs: {string.Join(", ", response.Tickets.Select(t => t.Id))}");
        
        var ticket = response.Tickets.FirstOrDefault(t => t.Id == id);
        
        if (ticket == null)
        {
            Console.WriteLine($"Ticket with ID {id} not found");
            return NotFound();
        }

        Console.WriteLine($"Found ticket: {System.Text.Json.JsonSerializer.Serialize(ticket)}");
        return Ok(ticket);
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

        var response = await _storage.ReadOrCreateAsync<TicketResponse>(FileName);
        
        ticket.Id = Guid.NewGuid().ToString("N");
        ticket.CreatedAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;
    
        // Log ticket details
        Console.WriteLine($"Creating ticket with ID: {ticket.Id}");
        Console.WriteLine($"Title: {ticket.Title}");
        Console.WriteLine($"Description: {ticket.Description}");
        
        response.Tickets.Add(ticket);
        await _storage.WriteAsync(FileName, response);

        return CreatedAtAction(nameof(Get), new { id = ticket.Id }, ticket);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Ticket update)
    {
        var response = await _storage.ReadOrCreateAsync<TicketResponse>(FileName);
        var ticket = response.Tickets.FirstOrDefault(t => t.Id == id);

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

        await _storage.WriteAsync(FileName, response);

        return Ok(ticket);
    }
}

public class TicketResponse
{
    [JsonPropertyName("tickets")]
    public List<Ticket> Tickets { get; set; } = new List<Ticket>();
}