using Microsoft.EntityFrameworkCore;
using Ticketline.Api.Data;
using Ticketline.Api.Dtos;
using Ticketline.Api.Models;

namespace Ticketline.Api.Services;

public class TicketService(AppDbContext db)
{
    public async Task<PagedResult<TicketResponse>> GetMineAsync(int ownerId, TicketQuery query)
    {
        var tickets = db.Tickets
            .AsNoTracking()
            .Where(ticket => ticket.OwnerId == ownerId)
            .OrderByDescending(ticket => ticket.Id);

        var totalCount = await tickets.CountAsync();
        var items = await tickets
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ticket => new TicketResponse
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Body = ticket.Body,
                Status = ticket.Status,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt
            })
            .ToListAsync();

        return new PagedResult<TicketResponse>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize),
            Items = items
        };
    }

    public async Task<TicketResponse?> GetAsync(int ownerId, int id)
    {
        var ticket = await db.Tickets.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id && item.OwnerId == ownerId);
        return ticket == null ? null : ToResponse(ticket);
    }

    public async Task<TicketResponse> CreateAsync(int ownerId, CreateTicketRequest request)
    {
        var now = DateTime.UtcNow;
        var ticket = new Ticket
        {
            OwnerId = ownerId,
            Title = request.Title.Trim(),
            Body = request.Body.Trim(),
            Status = TicketStatus.Open,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();
        return ToResponse(ticket);
    }

    public async Task<TicketResponse?> UpdateAsync(int ownerId, int id, UpdateTicketRequest request)
    {
        var ticket = await db.Tickets
            .FirstOrDefaultAsync(item => item.Id == id && item.OwnerId == ownerId);
        if (ticket == null)
        {
            return null;
        }

        TicketStatus.EnsureTransition(ticket.Status, request.Status);
        ticket.Title = request.Title.Trim();
        ticket.Body = request.Body.Trim();
        ticket.Status = request.Status;
        ticket.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return ToResponse(ticket);
    }

    private static TicketResponse ToResponse(Ticket ticket)
    {
        return new TicketResponse
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Body = ticket.Body,
            Status = ticket.Status,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt
        };
    }
}
