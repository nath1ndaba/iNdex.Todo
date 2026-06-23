using FluentValidation;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Application.Features.Tickets.CreateTicket;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Contracts.Responses;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Domain.Enums;
using iNdex.Todo.Domain.Errors;

namespace iNdex.Todo.Application.Features.Tickets.GetTickets;

// ── Queries ───────────────────────────────────────────────────────────────────

public sealed record GetAllTicketsQuery;
public sealed record GetTicketsByUserQuery(Guid UserId);
public sealed record GetTicketByIdQuery(Guid Id);

public sealed class GetAllTicketsHandler(ITicketRepository repo)
    : IHandler<GetAllTicketsQuery, List<TicketResponse>>
{
    public async Task<Result<List<TicketResponse>>> HandleAsync(
        GetAllTicketsQuery request, CancellationToken ct = default)
    {
        var tickets = await repo.GetAllWithDetailsAsync(ct);
        return Result.Success(tickets.Select(t =>
            t.ToResponse(t.CreatedByUser, t.AssignedToUser)).ToList());
    }
}

public sealed class GetTicketsByUserHandler(ITicketRepository repo)
    : IHandler<GetTicketsByUserQuery, List<TicketResponse>>
{
    public async Task<Result<List<TicketResponse>>> HandleAsync(
        GetTicketsByUserQuery request, CancellationToken ct = default)
    {
        var tickets = await repo.GetByAssignedUserAsync(request.UserId, ct);
        return Result.Success(tickets.Select(t =>
            t.ToResponse(t.CreatedByUser, t.AssignedToUser)).ToList());
    }
}

public sealed class GetTicketByIdHandler(ITicketRepository repo)
    : IHandler<GetTicketByIdQuery, TicketResponse>
{
    public async Task<Result<TicketResponse>> HandleAsync(
        GetTicketByIdQuery request, CancellationToken ct = default)
    {
        var ticket = await repo.GetByIdAsync(request.Id, ct);
        if (ticket is null)
            return Result.Failure<TicketResponse>(Error.NotFound(nameof(Ticket), request.Id));

        return Result.Success(ticket.ToResponse(ticket.CreatedByUser, ticket.AssignedToUser));
    }
}

// ── Update ────────────────────────────────────────────────────────────────────

public sealed record UpdateTicketCommand(Guid Id, UpdateTicketRequest Request);

public sealed class UpdateTicketHandler(
    ITicketRepository repo,
    IUserRepository userRepo,
    IUnitOfWork uow)
    : IHandler<UpdateTicketCommand, TicketResponse>
{
    public async Task<Result<TicketResponse>> HandleAsync(
        UpdateTicketCommand command, CancellationToken ct = default)
    {
        var ticket = await repo.GetByIdAsync(command.Id, ct);
        if (ticket is null)
            return Result.Failure<TicketResponse>(Error.NotFound(nameof(Ticket), command.Id));

        var req = command.Request;

        ticket.Title            = req.Title;
        ticket.Description      = req.Description;
        ticket.Priority         = (TaskPriority)req.Priority;
        ticket.Type             = (TicketType)req.Type;
        ticket.Status           = (TicketStatus)req.Status;
        ticket.DueDate          = req.DueDate;
        ticket.StartDate        = req.StartDate;
        ticket.EstimatedHours   = req.EstimatedHours;
        ticket.AssignedToUserId = req.AssignedToUserId;
        ticket.UpdatedAt        = DateTime.UtcNow;

        await repo.UpdateAsync(ticket, ct);
        await uow.SaveChangesAsync(ct);

        var assignee = req.AssignedToUserId.HasValue
            ? await userRepo.GetByIdAsync(req.AssignedToUserId.Value, ct)
            : null;

        return Result.Success(ticket.ToResponse(ticket.CreatedByUser, assignee));
    }
}

public sealed class UpdateTicketValidator : AbstractValidator<UpdateTicketRequest>
{
    public UpdateTicketValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Priority).InclusiveBetween(0, 4);
        RuleFor(x => x.Type).InclusiveBetween(0, 4);
        RuleFor(x => x.Status).InclusiveBetween(0, 4);
        RuleFor(x => x.EstimatedHours).GreaterThanOrEqualTo(0);
    }
}

// ── Delete ────────────────────────────────────────────────────────────────────

public sealed record DeleteTicketCommand(Guid Id);

public sealed class DeleteTicketHandler(ITicketRepository repo, IUnitOfWork uow)
    : IHandler<DeleteTicketCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(
        DeleteTicketCommand command, CancellationToken ct = default)
    {
        var exists = await repo.ExistsAsync(command.Id, ct);
        if (!exists)
            return Result.Failure<bool>(Error.NotFound(nameof(Ticket), command.Id));

        await repo.DeleteAsync(command.Id, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}
