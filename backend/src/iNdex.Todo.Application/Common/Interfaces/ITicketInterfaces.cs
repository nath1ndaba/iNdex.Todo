using iNdex.Todo.Domain.Entities;

namespace iNdex.Todo.Application.Common.Interfaces;

public interface ITicketRepository : IRepository<Ticket>
{
    Task<List<Ticket>> GetByAssignedUserAsync(Guid userId, CancellationToken ct = default);
    Task<List<Ticket>> GetByCreatedUserAsync(Guid userId, CancellationToken ct = default);
    Task<List<Ticket>> GetAllWithDetailsAsync(CancellationToken ct = default);
    Task<string> GenerateTicketNumberAsync(CancellationToken ct = default);
}

public interface ITimeLogRepository : IRepository<TimeLog>
{
    Task<List<TimeLog>> GetByTicketIdAsync(Guid ticketId, CancellationToken ct = default);
    Task<List<TimeLog>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<decimal> GetTotalHoursByTicketAsync(Guid ticketId, CancellationToken ct = default);
}
