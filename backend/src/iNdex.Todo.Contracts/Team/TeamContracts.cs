namespace iNdex.Todo.Contracts.Team;

// ── Responses ────────────────────────────────────────────────────────────────

/// <summary>
/// Snapshot of one team member — shown on the manager's team overview grid.
/// Updated in real time via SignalR.
/// </summary>
public sealed record TeamMemberSnapshot(
    Guid     UserId,
    string   FullName,
    string   Email,
    string?  Role,
    string?  Department,
    string?  AvatarInitials,

    // Current activity
    int      OpenTickets,
    int      InProgressTickets,
    int      InReviewTickets,
    int      CompletedToday,
    int      CompletedThisWeek,
    int      CompletedAllTime,

    // Time tracking
    decimal  HoursToday,
    decimal  HoursThisWeek,
    decimal  HoursAllTime,

    // Health indicators
    float    CompletionRate,         // 0-1 — completed / assigned last 30 days
    float?   ProductivityScore,      // from AI layer
    DateTime? LastActivityAt,        // last time they opened/closed/commented on anything
    bool     IsInactive,             // no activity in last 3 days (working days)
    string   ActivityStatus,         // "Active", "Idle", "Inactive", "Offline"

    // Totals
    int      TotalTicketsAssigned,
    int      TotalTicketsCreated,
    DateTime MemberSince
);

/// <summary>
/// Full drill-down on a single team member for the manager.
/// </summary>
public sealed record TeamMemberDetail(
    TeamMemberSnapshot Snapshot,
    List<TeamTicketItem>  RecentTickets,
    List<TeamTimeLogItem> RecentTimeLogs,
    List<string>          Skills,
    string?               AiInsight          // Gemini one-liner about this person's performance
);

public sealed record TeamTicketItem(
    Guid     Id,
    string   TicketNumber,
    string   Title,
    string   Status,
    string   Priority,
    DateTime CreatedAt,
    DateTime? DueDate,
    bool     IsOverdue
);

public sealed record TeamTimeLogItem(
    Guid     Id,
    string   TicketNumber,
    decimal  Hours,
    string   Description,
    DateTime LoggedDate
);

/// <summary>
/// Full team overview — one snapshot per member, sorted by activity status.
/// </summary>
public sealed record TeamOverviewResponse(
    int                       TotalMembers,
    int                       ActiveToday,
    int                       InactiveMembers,
    int                       TotalOpenTickets,
    int                       TotalHoursThisWeek,
    List<TeamMemberSnapshot>  Members
);

/// <summary>
/// Members who have had zero ticket activity for N or more days.
/// </summary>
public sealed record InactiveMembersResponse(
    int                       InactiveDaysThreshold,
    List<TeamMemberSnapshot>  Members
);

// ── Requests ─────────────────────────────────────────────────────────────────

public sealed record GetTeamOverviewQuery;
public sealed record GetTeamMemberDetailQuery(Guid UserId);
public sealed record GetInactiveMembersQuery(int InactiveDays = 3);
