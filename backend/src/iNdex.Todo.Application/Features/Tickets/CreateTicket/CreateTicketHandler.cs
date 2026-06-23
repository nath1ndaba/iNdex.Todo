using FluentValidation;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Contracts.Responses;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Domain.Enums;
using iNdex.Todo.Domain.Errors;

namespace iNdex.Todo.Application.Features.Tickets.CreateTicket;

public sealed class CreateTicketHandler(
    ITicketRepository ticketRepo,
    IUserRepository userRepo,
    IUnitOfWork uow)
    : IHandler<CreateTicketRequest, TicketResponse>
{
    public async Task<Result<TicketResponse>> HandleAsync(
        CreateTicketRequest request, CancellationToken ct = default)
    {
        var creator = await userRepo.GetByIdAsync(request.CreatedByUserId, ct);
        if (creator is null)
            return Result.Failure<TicketResponse>(Error.NotFound(nameof(User), request.CreatedByUserId));

        if (request.AssignedToUserId.HasValue)
        {
            var exists = await userRepo.ExistsAsync(request.AssignedToUserId.Value, ct);
            if (!exists)
                return Result.Failure<TicketResponse>(
                    Error.NotFound(nameof(User), request.AssignedToUserId.Value));
        }

        var number = await ticketRepo.GenerateTicketNumberAsync(ct);

        var ticket = new Ticket
        {
            Title             = request.Title,
            Description       = request.Description,
            TicketNumber      = number,
            Priority          = (TaskPriority)request.Priority,
            Type              = (TicketType)request.Type,
            DueDate           = request.DueDate,
            StartDate         = request.StartDate,
            EstimatedHours    = request.EstimatedHours,
            CreatedByUserId   = request.CreatedByUserId,
            AssignedToUserId  = request.AssignedToUserId,
            CreatedBy         = request.CreatedByUserId.ToString()
        };

        await ticketRepo.AddAsync(ticket, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(ticket.ToResponse(creator, null));
    }
}

public sealed class CreateTicketValidator : AbstractValidator<CreateTicketRequest>
{
    public CreateTicketValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Description).MaximumLength(5000).When(x => x.Description is not null);
        RuleFor(x => x.Priority).InclusiveBetween(0, 4);
        RuleFor(x => x.Type).InclusiveBetween(0, 4);
        RuleFor(x => x.EstimatedHours).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CreatedByUserId).NotEmpty();
    }
}

public static class TicketMappingExtensions
{
    public static TicketResponse ToResponse(this Ticket t, User creator, User? assignee)
        => new(
            t.Id,
            t.TicketNumber,
            t.Title,
            t.Description,
            t.Status.ToString(),
            t.Priority.ToString(),
            t.Type.ToString(),
            t.DueDate,
            t.StartDate,
            t.EstimatedHours,
            t.TimeLogs.Sum(l => l.Hours),
            t.CreatedByUserId,
            $"{creator.FirstName} {creator.LastName}",
            t.AssignedToUserId,
            assignee is null ? null : $"{assignee.FirstName} {assignee.LastName}",
            t.Comments.Count,
            t.CreatedAt);
}
