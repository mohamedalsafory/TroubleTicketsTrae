using System;
using System.Collections.Generic;

namespace TroubleTicket.Core.Entities;

public class Ticket
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string Priority { get; set; } = "medium";
    public string Status { get; set; } = "open";
    public string CreatedById { get; set; } = string.Empty;
    public string? AssignedToId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<Message> Messages { get; set; } = new();
}
