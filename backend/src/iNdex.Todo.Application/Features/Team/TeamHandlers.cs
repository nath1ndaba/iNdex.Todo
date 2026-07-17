using iNdex.Todo.AI.Gemini;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Contracts.Team;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Domain.Enums;

namespace iNdex.Todo.Application.Features.Team;

// ── Helper: build a snapshot for one user ────────────────────────────────────

internal static class SnapshotBuilder
{
    internal static async Task<TeamMemberSnapshot> BuildAsync(
        User user,
        ITicketRepository ticketRepo,
        ITimeLogRepository logRepo,
        CancellationToken ct)
    {
        var assigned  = await ticketRepo.GetByAssignedUserAsync(user.Id, ct);
        var logs      = await logRepo.GetByUserIdAsync(user.Id, ct);

        var now       = DateTime.UtcNow;
        var todayStart= now.Date;
        var weekStart = now.AddDays(-(int)now.DayOfWeek);

        var open        = assigned.Count(t => t.Status == TicketStatus.Open);
        var inProgress  = assigned.Count(t => t.Status == TicketStatus.InProgress);
        var inReview    = assigned.Count(t => t.Status == TicketStatus.InReview);
        var doneToday   = assigned.Count(t => t.Status == TicketStatus.Done && t.UpdatedAt >= todayStart);
        var doneWeek    = assigned.Count(t => t.Status == TicketStatus.Done && t.UpdatedAt >= weekStart);
        var doneAll     = assigned.Count(t => t.Status == TicketStatus.Done);

        var hoursToday  = logs.Where(l => l.LoggedDate >= todayStart).Sum(l => l.Hours);
        var hoursWeek   = logs.Where(l => l.LoggedDate >= weekStart).Sum(l => l.Hours);
        var hoursAll    = logs.Sum(l => l.Hours);

        var last30      = now.AddDays(-30);
        var recent      = assigned.Where(t => t.CreatedAt >= last30).ToList();
        var compRate    = recent.Count > 0
            ? (float)recent.Count(t => t.Status == TicketStatus.Done) / recent.Count
            : 0f;

        // Last activity = most recent of: ticket updated, comment, time log
        var lastTicket = assigned
        .Select(t => (DateTime?)(t.UpdatedAt ?? t.CreatedAt))
        .Max();

        var lastLog = logs
            .Select(l => (DateTime?)l.LoggedDate)
            .Max();

        DateTime? lastActivity = null;

        if (lastTicket.HasValue && lastLog.HasValue)
            lastActivity = lastTicket > lastLog ? lastTicket : lastLog;
        else
            lastActivity = lastTicket ?? lastLog;

        var inactiveDays = lastActivity.HasValue
            ? (now - lastActivity.Value).TotalDays
            : 999;

        var activityStatus = inactiveDays switch
        {
            < 0.5  => "Active",
            < 1    => "Idle",
            < 3    => "Idle",
            _      => "Inactive"
        };

        // Parse skills from JSON array stored as string
        var skills = new List<string>();
        if (!string.IsNullOrEmpty(user.SkillProfile))
        {
            try
            {
                skills = System.Text.Json.JsonSerializer.Deserialize<List<string>>(user.SkillProfile) ?? [];
            }
            catch { /* ignore malformed */ }
        }

        return new TeamMemberSnapshot(
            UserId:               user.Id,
            FullName:             $"{user.FirstName} {user.LastName}",
            Email:                user.Email,
            Role:                 user.Role,
            Department:           user.Department,
            AvatarInitials:       $"{user.FirstName[0]}{user.LastName[0]}".ToUpperInvariant(),
            OpenTickets:          open,
            InProgressTickets:    inProgress,
            InReviewTickets:      inReview,
            CompletedToday:       doneToday,
            CompletedThisWeek:    doneWeek,
            CompletedAllTime:     doneAll,
            HoursToday:           hoursToday,
            HoursThisWeek:        hoursWeek,
            HoursAllTime:         hoursAll,
            CompletionRate:       compRate,
            ProductivityScore:    user.ProductivityScore,
            LastActivityAt:       lastActivity,
            IsInactive:           inactiveDays >= 3,
            ActivityStatus:       activityStatus,
            TotalTicketsAssigned: assigned.Count,
            TotalTicketsCreated:  0,  // populated separately if needed
            MemberSince:          user.CreatedAt);
    }
}

// ── Get Full Team Overview ────────────────────────────────────────────────────

public sealed class GetTeamOverviewHandler(
    IUserRepository userRepo,
    ITicketRepository ticketRepo,
    ITimeLogRepository logRepo)
    : IHandler<GetTeamOverviewQuery, TeamOverviewResponse>
{
    public async Task<Result<TeamOverviewResponse>> HandleAsync(
        GetTeamOverviewQuery request, CancellationToken ct = default)
    {
        var users    = await userRepo.GetAllAsync(ct);
        var active   = users.Where(u => u.IsActive && !u.IsDeleted).ToList();

        var snapshots = new List<TeamMemberSnapshot>();
        foreach (var user in active)
            snapshots.Add(await SnapshotBuilder.BuildAsync(user, ticketRepo, logRepo, ct));

        // Sort: Inactive first (so manager sees who needs follow-up), then by open tickets desc
        var sorted = snapshots
            .OrderByDescending(s => s.IsInactive)
            .ThenByDescending(s => s.OpenTickets)
            .ToList();

        return Result.Success(new TeamOverviewResponse(
            TotalMembers:        sorted.Count,
            ActiveToday:         sorted.Count(s => s.ActivityStatus == "Active"),
            InactiveMembers:     sorted.Count(s => s.IsInactive),
            TotalOpenTickets:    sorted.Sum(s => s.OpenTickets + s.InProgressTickets),
            TotalHoursThisWeek:  (int)sorted.Sum(s => s.HoursThisWeek),
            Members:             sorted));
    }
}

// ── Get Single Member Detail ──────────────────────────────────────────────────

public sealed class GetTeamMemberDetailHandler(
    IUserRepository userRepo,
    ITicketRepository ticketRepo,
    ITimeLogRepository logRepo,
    GeminiService gemini)
    : IHandler<GetTeamMemberDetailQuery, TeamMemberDetail>
{
    public async Task<Result<TeamMemberDetail>> HandleAsync(
        GetTeamMemberDetailQuery request, CancellationToken ct = default)
    {
        var user = await userRepo.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return Result.Failure<TeamMemberDetail>(
                Domain.Errors.Error.NotFound(nameof(User), request.UserId));

        var snapshot = await SnapshotBuilder.BuildAsync(user, ticketRepo, logRepo, ct);

        // Recent tickets (last 20)
        var assigned = await ticketRepo.GetByAssignedUserAsync(user.Id, ct);
        var recentTickets = assigned
            .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
            .Take(20)
            .Select(t => new TeamTicketItem(
                t.Id, t.TicketNumber, t.Title,
                t.Status.ToString(), t.Priority.ToString(),
                t.CreatedAt, t.DueDate,
                t.DueDate.HasValue && t.DueDate.Value < DateTime.UtcNow && t.Status != TicketStatus.Done))
            .ToList();

        // Recent time logs (last 20)
        var logs = await logRepo.GetByUserIdAsync(user.Id, ct);
        var recentLogs = logs
            .OrderByDescending(l => l.LoggedDate)
            .Take(20)
            .Select(l => new TeamTimeLogItem(
                l.Id, l.Ticket.TicketNumber, l.Hours, l.Description, l.LoggedDate))
            .ToList();

        // Skills
        var skills = new List<string>();
        if (!string.IsNullOrEmpty(user.SkillProfile))
        {
            try { skills = System.Text.Json.JsonSerializer.Deserialize<List<string>>(user.SkillProfile) ?? []; }
            catch { }
        }

        // AI one-liner insight about this person
        var aiInsight = await GenerateInsightAsync(snapshot, ct);

        return Result.Success(new TeamMemberDetail(
            snapshot, recentTickets, recentLogs, skills, aiInsight));
    }

    private async Task<string?> GenerateInsightAsync(TeamMemberSnapshot s, CancellationToken ct)
    {
        try
        {
            var prompt = $"""
                In one sentence (max 20 words), summarise this team member's current performance for a manager.
                Name: {s.FullName}
                Open tickets: {s.OpenTickets}
                Completed this week: {s.CompletedThisWeek}
                Hours this week: {s.HoursThisWeek}
                Completion rate (30d): {s.CompletionRate:P0}
                Status: {s.ActivityStatus}
                Be direct and constructive. No fluff.
                """;
            //var model    = new Mscc.GenerativeAI.GenerativeModel(
            //    apiKey: string.Empty, // will use injected settings via GeminiService
            //    model: "gemini-1.5-flash");
            // Use GeminiService instead (it handles the key)
            return await gemini.SummarizeTicketAsync(
                $"Performance of {s.FullName}",
                $"Completed {s.CompletedThisWeek} tickets, {s.HoursThisWeek}h logged, {s.CompletionRate:P0} rate",
                [$"Status: {s.ActivityStatus}", $"Open tickets: {s.OpenTickets}"],
                ct);
        }
        catch { return null; }
    }
}

// ── Get Inactive Members ──────────────────────────────────────────────────────

public sealed class GetInactiveMembersHandler(
    IUserRepository userRepo,
    ITicketRepository ticketRepo,
    ITimeLogRepository logRepo)
    : IHandler<GetInactiveMembersQuery, InactiveMembersResponse>
{
    public async Task<Result<InactiveMembersResponse>> HandleAsync(
        GetInactiveMembersQuery request, CancellationToken ct = default)
    {
        var users    = await userRepo.GetAllAsync(ct);
        var active   = users.Where(u => u.IsActive && !u.IsDeleted).ToList();

        var snapshots = new List<TeamMemberSnapshot>();
        foreach (var user in active)
            snapshots.Add(await SnapshotBuilder.BuildAsync(user, ticketRepo, logRepo, ct));

        var inactive = snapshots
            .Where(s => s.IsInactive)
            .OrderBy(s => s.LastActivityAt ?? DateTime.MinValue)
            .ToList();

        return Result.Success(new InactiveMembersResponse(request.InactiveDays, inactive));
    }
}
