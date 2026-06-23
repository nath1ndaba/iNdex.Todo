using FluentValidation;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Contracts.Responses;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Domain.Errors;

namespace iNdex.Todo.Application.Features.TimeLog;

// ── Log Time ──────────────────────────────────────────────────────────────────

public sealed class LogTimeHandler(
    ITimeLogRepository logRepo,
    ITicketRepository ticketRepo,
    IUserRepository userRepo,
    IUnitOfWork uow)
    : IHandler<LogTimeRequest, TimeLogResponse>
{
    public async Task<Result<TimeLogResponse>> HandleAsync(
        LogTimeRequest request, CancellationToken ct = default)
    {
        var ticket = await ticketRepo.GetByIdAsync(request.TicketId, ct);
        if (ticket is null)
            return Result.Failure<TimeLogResponse>(Error.NotFound(nameof(Ticket), request.TicketId));

        var user = await userRepo.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return Result.Failure<TimeLogResponse>(Error.NotFound(nameof(User), request.UserId));

        var log = new Domain.Entities.TimeLog
        {
            TicketId    = request.TicketId,
            UserId      = request.UserId,
            Hours       = request.Hours,
            Description = request.Description,
            LoggedDate  = request.LoggedDate ?? DateTime.UtcNow,
            CreatedBy   = request.UserId.ToString()
        };

        await logRepo.AddAsync(log, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(new TimeLogResponse(
            log.Id,
            ticket.Id,
            ticket.TicketNumber,
            user.Id,
            $"{user.FirstName} {user.LastName}",
            log.Hours,
            log.Description,
            log.LoggedDate));
    }
}

public sealed class LogTimeValidator : AbstractValidator<LogTimeRequest>
{
    public LogTimeValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Hours)
            .GreaterThan(0).WithMessage("Hours must be greater than 0.")
            .LessThanOrEqualTo(24).WithMessage("Cannot log more than 24 hours in a single entry.");
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Please describe what you worked on.")
            .MaximumLength(1000);
    }
}

// ── Get Time Logs by Ticket ───────────────────────────────────────────────────

public sealed record GetTimeLogsByTicketQuery(Guid TicketId);

public sealed class GetTimeLogsByTicketHandler(
    ITimeLogRepository logRepo,
    ITicketRepository ticketRepo)
    : IHandler<GetTimeLogsByTicketQuery, TimeLogSummaryResponse>
{
    public async Task<Result<TimeLogSummaryResponse>> HandleAsync(
        GetTimeLogsByTicketQuery request, CancellationToken ct = default)
    {
        var ticket = await ticketRepo.GetByIdAsync(request.TicketId, ct);
        if (ticket is null)
            return Result.Failure<TimeLogSummaryResponse>(
                Error.NotFound(nameof(Ticket), request.TicketId));

        var logs = await logRepo.GetByTicketIdAsync(request.TicketId, ct);
        var total = logs.Sum(l => l.Hours);
        var remaining = Math.Max(0, ticket.EstimatedHours - (double)total);

        var logResponses = logs.Select(l => new TimeLogResponse(
            l.Id,
            l.TicketId,
            ticket.TicketNumber,
            l.UserId,
            $"{l.User.FirstName} {l.User.LastName}",
            l.Hours,
            l.Description,
            l.LoggedDate)).ToList();

        return Result.Success(new TimeLogSummaryResponse(
            ticket.Id,
            ticket.TicketNumber,
            ticket.Title,
            total,
            ticket.EstimatedHours,
            (decimal)remaining,
            logResponses));
    }
}

// ── Get Time Logs by User ─────────────────────────────────────────────────────

public sealed record GetTimeLogsByUserQuery(Guid UserId);

public sealed class GetTimeLogsByUserHandler(ITimeLogRepository logRepo)
    : IHandler<GetTimeLogsByUserQuery, List<TimeLogResponse>>
{
    public async Task<Result<List<TimeLogResponse>>> HandleAsync(
        GetTimeLogsByUserQuery request, CancellationToken ct = default)
    {
        var logs = await logRepo.GetByUserIdAsync(request.UserId, ct);

        return Result.Success(logs.Select(l => new TimeLogResponse(
            l.Id,
            l.TicketId,
            l.Ticket.TicketNumber,
            l.UserId,
            $"{l.User.FirstName} {l.User.LastName}",
            l.Hours,
            l.Description,
            l.LoggedDate)).ToList());
    }
}

// ── Delete Time Log ───────────────────────────────────────────────────────────

public sealed record DeleteTimeLogCommand(Guid Id);

public sealed class DeleteTimeLogHandler(ITimeLogRepository logRepo, IUnitOfWork uow)
    : IHandler<DeleteTimeLogCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(
        DeleteTimeLogCommand command, CancellationToken ct = default)
    {
        var exists = await logRepo.ExistsAsync(command.Id, ct);
        if (!exists)
            return Result.Failure<bool>(Error.NotFound("TimeLog", command.Id));

        await logRepo.DeleteAsync(command.Id, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}
