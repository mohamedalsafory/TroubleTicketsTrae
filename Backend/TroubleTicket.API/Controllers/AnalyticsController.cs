using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TroubleTicket.Core.Entities;
using TroubleTicket.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TroubleTicket.API.Controllers;

[Authorize(Roles = "admin")]
public class AnalyticsController : BaseController
{
    private readonly IJsonStorageService _storage;
    private const string TicketsFile = "tickets.json";
    private const string UsersFile = "users.json";

    public AnalyticsController(IJsonStorageService storage)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardMetrics()
    {
        var tickets = await _storage.ReadOrCreateAsync<List<Ticket>>(TicketsFile);
        var users = await _storage.ReadOrCreateAsync<List<User>>(UsersFile);

        var metrics = new
        {
            TotalTickets = tickets.Count,
            OpenTickets = tickets.Count(t => t.Status == "open"),
            ClosedTickets = tickets.Count(t => t.Status == "closed"),
            InProgressTickets = tickets.Count(t => t.Status == "in-progress"),
            HighPriorityTickets = tickets.Count(t => t.Priority == "high"),
            TotalAgents = users.Count(u => u.Role == "agent"),
            TotalClients = users.Count(u => u.Role == "client")
        };

        return Ok(metrics);
    }

    [HttpGet("agent-performance")]
    public async Task<IActionResult> GetAgentPerformance()
    {
        var tickets = await _storage.ReadOrCreateAsync<List<Ticket>>(TicketsFile);
        var users = await _storage.ReadOrCreateAsync<List<User>>(UsersFile);

        var agents = users.Where(u => u.Role == "agent").ToList();
        var performance = agents.Select(agent =>
        {
            var agentTickets = tickets.Where(t => t.AssignedToId == agent.Id).ToList();
            return new
            {
                AgentId = agent.Id,
                AgentName = agent.Name,
                TotalTickets = agentTickets.Count,
                ClosedTickets = agentTickets.Count(t => t.Status == "closed"),
                OpenTickets = agentTickets.Count(t => t.Status == "open"),
                HighPriorityTickets = agentTickets.Count(t => t.Priority == "high")
            };
        });

        return Ok(performance);
    }

    [HttpGet("ticket-statistics")]
    public async Task<IActionResult> GetTicketStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var tickets = await _storage.ReadOrCreateAsync<List<Ticket>>(TicketsFile);
        
        startDate ??= DateTime.UtcNow.AddMonths(-1);
        endDate ??= DateTime.UtcNow;

        var filteredTickets = tickets.Where(t => 
            t.CreatedAt >= startDate && 
            t.CreatedAt <= endDate
        );

        var statistics = new
        {
            TotalTickets = filteredTickets.Count(),
            ByStatus = new
            {
                Open = filteredTickets.Count(t => t.Status == "open"),
                InProgress = filteredTickets.Count(t => t.Status == "in-progress"),
                Closed = filteredTickets.Count(t => t.Status == "closed")
            },
            ByPriority = new
            {
                Low = filteredTickets.Count(t => t.Priority == "low"),
                Medium = filteredTickets.Count(t => t.Priority == "medium"),
                High = filteredTickets.Count(t => t.Priority == "high")
            },
            AverageResolutionTime = filteredTickets
                .Where(t => t.Status == "closed")
                .Select(t => (t.UpdatedAt - t.CreatedAt).TotalHours)
                .DefaultIfEmpty(0)
                .Average()
        };

        return Ok(statistics);
    }
}