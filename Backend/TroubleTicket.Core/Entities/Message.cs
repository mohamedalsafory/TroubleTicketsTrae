using System;
using System.Collections.Generic;

namespace TroubleTicket.Core.Entities;

public class Message
{
    public string Id { get; set; } = string.Empty;
    public string TicketId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Attachments { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
 
 
 