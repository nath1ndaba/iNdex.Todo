using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Infrastructure.Persistence;
using iNdex.Todo.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace iNdex.Todo.Infrastructure.Persistence.Repositories;

public sealed class TicketRepository(AppDbContext context)
    : Repository<Ticket>(context), ITicketRepository
{
    public override async Task<Ticket?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .Include(t => t.TimeLogs).ThenInclude(l => l.User)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<List<Ticket>> GetAllWithDetailsAsync(CancellationToken ct = default)
        => await DbSet
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .Include(t => t.TimeLogs)
            .Include(t => t.Comments)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

    public async Task<List<Ticket>> GetByAssignedUserAsync(Guid userId, CancellationToken ct = default)
        => await DbSet
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .Include(t => t.TimeLogs)
            .Where(t => t.AssignedToUserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

    public async Task<List<Ticket>> GetByCreatedUserAsync(Guid userId, CancellationToken ct = default)
        => await DbSet
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .Include(t => t.TimeLogs)
            .Where(t => t.CreatedByUserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

    public async Task<string> GenerateTicketNumberAsync(CancellationToken ct = default)
    {
        var count = await DbSet.CountAsync(ct);
        return $"TKT-{(count + 1):D4}";
    }
}

public sealed class TimeLogRepository(AppDbContext context)
    : Repository<Domain.Entities.TimeLog>(context), ITimeLogRepository
{
    public async Task<List<Domain.Entities.TimeLog>> GetByTicketIdAsync(
        Guid ticketId, CancellationToken ct = default)
        => await DbSet
            .Include(l => l.User)
            .Include(l => l.Ticket)
            .Where(l => l.TicketId == ticketId)
            .OrderByDescending(l => l.LoggedDate)
            .ToListAsync(ct);

    public async Task<List<Domain.Entities.TimeLog>> GetByUserIdAsync(
        Guid userId, CancellationToken ct = default)
        => await DbSet
            .Include(l => l.User)
            .Include(l => l.Ticket)
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.LoggedDate)
            .ToListAsync(ct);

    public async Task<decimal> GetTotalHoursByTicketAsync(
        Guid ticketId, CancellationToken ct = default)
        => await DbSet
            .Where(l => l.TicketId == ticketId)
            .SumAsync(l => l.Hours, ct);
}
