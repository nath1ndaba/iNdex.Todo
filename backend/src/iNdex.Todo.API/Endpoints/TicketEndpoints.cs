using FluentValidation;
using iNdex.Todo.API.Extensions;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Features.Tickets.CreateTicket;
using iNdex.Todo.Application.Features.Tickets.GetTickets;
using iNdex.Todo.Application.Features.TimeLog;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace iNdex.Todo.API.Endpoints;

public static class TicketEndpoints
{
    public static IEndpointRouteBuilder MapTicketEndpoints(this IEndpointRouteBuilder app)
    {
        var tickets = app.MapGroup("/api/tickets")
            .WithTags("Tickets")
            .WithOpenApi()
            .RequireAuthorization();

        tickets.MapPost("/", CreateTicket)
            .WithName("CreateTicket").WithSummary("Create a new ticket")
            .Produces<TicketResponse>(StatusCodes.Status201Created);

        tickets.MapGet("/", GetAllTickets)
            .WithName("GetAllTickets").WithSummary("Get all tickets")
            .Produces<List<TicketResponse>>();

        tickets.MapGet("/assigned/{userId:guid}", GetTicketsByUser)
            .WithName("GetTicketsByUser").WithSummary("Get tickets assigned to a user")
            .Produces<List<TicketResponse>>();

        tickets.MapGet("/{id:guid}", GetTicketById)
            .WithName("GetTicketById").WithSummary("Get ticket by ID")
            .Produces<TicketResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        tickets.MapPut("/{id:guid}", UpdateTicket)
            .WithName("UpdateTicket").WithSummary("Update a ticket")
            .Produces<TicketResponse>();

        tickets.MapDelete("/{id:guid}", DeleteTicket)
            .WithName("DeleteTicket").WithSummary("Delete a ticket")
            .Produces(StatusCodes.Status204NoContent);

        // ── Time Logs ─────────────────────────────────────────────────────────
        var timeLogs = app.MapGroup("/api/timelogs")
            .WithTags("TimeLogs")
            .WithOpenApi()
            .RequireAuthorization();

        timeLogs.MapPost("/", LogTime)
            .WithName("LogTime").WithSummary("Log time against a ticket")
            .Produces<TimeLogResponse>(StatusCodes.Status201Created);

        timeLogs.MapGet("/ticket/{ticketId:guid}", GetTimeLogsByTicket)
            .WithName("GetTimeLogsByTicket").WithSummary("Get time log summary for a ticket")
            .Produces<TimeLogSummaryResponse>();

        timeLogs.MapGet("/user/{userId:guid}", GetTimeLogsByUser)
            .WithName("GetTimeLogsByUser").WithSummary("Get all time logs for a user")
            .Produces<List<TimeLogResponse>>();

        timeLogs.MapDelete("/{id:guid}", DeleteTimeLog)
            .WithName("DeleteTimeLog").WithSummary("Delete a time log entry")
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }

    // ── Ticket handlers ───────────────────────────────────────────────────────

    private static async Task<IResult> CreateTicket(
        [FromBody]     CreateTicketRequest request,
        [FromServices] IValidator<CreateTicketRequest> validator,
        [FromServices] IHandler<CreateTicketRequest, TicketResponse> handler,
        CancellationToken ct)
    {
        var v = await validator.ValidateAndHandleAsync(request, ct);
        if (v.IsFailure) return v.ToHttpResult();
        var result = await handler.HandleAsync(request, ct);
        return result.ToHttpResult(StatusCodes.Status201Created);
    }

    private static async Task<IResult> GetAllTickets(
        [FromServices] IHandler<GetAllTicketsQuery, List<TicketResponse>> handler,
        CancellationToken ct)
        => (await handler.HandleAsync(new GetAllTicketsQuery(), ct)).ToHttpResult();

    private static async Task<IResult> GetTicketsByUser(
        Guid userId,
        [FromServices] IHandler<GetTicketsByUserQuery, List<TicketResponse>> handler,
        CancellationToken ct)
        => (await handler.HandleAsync(new GetTicketsByUserQuery(userId), ct)).ToHttpResult();

    private static async Task<IResult> GetTicketById(
        Guid id,
        [FromServices] IHandler<GetTicketByIdQuery, TicketResponse> handler,
        CancellationToken ct)
        => (await handler.HandleAsync(new GetTicketByIdQuery(id), ct)).ToHttpResult();

    private static async Task<IResult> UpdateTicket(
        Guid id,
        [FromBody]     UpdateTicketRequest request,
        [FromServices] IValidator<UpdateTicketRequest> validator,
        [FromServices] IHandler<UpdateTicketCommand, TicketResponse> handler,
        CancellationToken ct)
    {
        var v = await validator.ValidateAndHandleAsync(request, ct);
        if (v.IsFailure) return v.ToHttpResult();
        return (await handler.HandleAsync(new UpdateTicketCommand(id, request), ct)).ToHttpResult();
    }

    private static async Task<IResult> DeleteTicket(
        Guid id,
        [FromServices] IHandler<DeleteTicketCommand, bool> handler,
        CancellationToken ct)
        => (await handler.HandleAsync(new DeleteTicketCommand(id), ct))
            .ToHttpResult(StatusCodes.Status204NoContent);

    // ── TimeLog handlers ──────────────────────────────────────────────────────

    private static async Task<IResult> LogTime(
        [FromBody]     LogTimeRequest request,
        [FromServices] IValidator<LogTimeRequest> validator,
        [FromServices] IHandler<LogTimeRequest, TimeLogResponse> handler,
        CancellationToken ct)
    {
        var v = await validator.ValidateAndHandleAsync(request, ct);
        if (v.IsFailure) return v.ToHttpResult();
        return (await handler.HandleAsync(request, ct)).ToHttpResult(StatusCodes.Status201Created);
    }

    private static async Task<IResult> GetTimeLogsByTicket(
        Guid ticketId,
        [FromServices] IHandler<GetTimeLogsByTicketQuery, TimeLogSummaryResponse> handler,
        CancellationToken ct)
        => (await handler.HandleAsync(new GetTimeLogsByTicketQuery(ticketId), ct)).ToHttpResult();

    private static async Task<IResult> GetTimeLogsByUser(
        Guid userId,
        [FromServices] IHandler<GetTimeLogsByUserQuery, List<TimeLogResponse>> handler,
        CancellationToken ct)
        => (await handler.HandleAsync(new GetTimeLogsByUserQuery(userId), ct)).ToHttpResult();

    private static async Task<IResult> DeleteTimeLog(
        Guid id,
        [FromServices] IHandler<DeleteTimeLogCommand, bool> handler,
        CancellationToken ct)
        => (await handler.HandleAsync(new DeleteTimeLogCommand(id), ct))
            .ToHttpResult(StatusCodes.Status204NoContent);
}
