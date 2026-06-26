using iNdex.Todo.API.Extensions;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Features.Team;
using iNdex.Todo.Contracts.Team;
using Microsoft.AspNetCore.Mvc;

namespace iNdex.Todo.API.Endpoints;

public static class TeamEndpoints
{
    public static IEndpointRouteBuilder MapTeamEndpoints(this IEndpointRouteBuilder app)
    {
        var team = app.MapGroup("/api/team")
            .WithTags("Team")
            .WithOpenApi()
            .RequireAuthorization();

        team.MapGet("/overview", GetTeamOverview)
            .WithName("GetTeamOverview")
            .WithSummary("Get real-time snapshot of all team members — open tickets, hours, activity status")
            .Produces<TeamOverviewResponse>();

        team.MapGet("/member/{userId:guid}", GetTeamMemberDetail)
            .WithName("GetTeamMemberDetail")
            .WithSummary("Deep-dive on a single team member including tickets, time logs and AI insight")
            .Produces<TeamMemberDetail>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        team.MapGet("/inactive", GetInactiveMembers)
            .WithName("GetInactiveMembers")
            .WithSummary("List team members with no ticket activity in the last N days")
            .Produces<InactiveMembersResponse>();

        return app;
    }

    private static async Task<IResult> GetTeamOverview(
        [FromServices] IHandler<GetTeamOverviewQuery, TeamOverviewResponse> handler,
        CancellationToken ct)
        => (await handler.HandleAsync(new GetTeamOverviewQuery(), ct)).ToHttpResult();

    private static async Task<IResult> GetTeamMemberDetail(
        Guid userId,
        [FromServices] IHandler<GetTeamMemberDetailQuery, TeamMemberDetail> handler,
        CancellationToken ct)
        => (await handler.HandleAsync(new GetTeamMemberDetailQuery(userId), ct)).ToHttpResult();

    private static async Task<IResult> GetInactiveMembers(
        [FromQuery] int days,
        [FromServices] IHandler<GetInactiveMembersQuery, InactiveMembersResponse> handler,
        CancellationToken ct)
        => (await handler.HandleAsync(new GetInactiveMembersQuery(days > 0 ? days : 3), ct)).ToHttpResult();
}
