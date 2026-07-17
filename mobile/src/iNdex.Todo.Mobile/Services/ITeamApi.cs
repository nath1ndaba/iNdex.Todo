using Refit;

namespace iNdex.Todo.Mobile.Services;

public interface ITeamApi
{
    [Get("/api/team/overview")]
    Task<ApiResponse<TeamOverviewResponse>> GetTeamOverviewAsync();

    [Get("/api/team/member/{userId}")]
    Task<ApiResponse<TeamMemberDetail>> GetTeamMemberDetailAsync(Guid userId);

    [Get("/api/team/inactive")]
    Task<ApiResponse<InactiveMembersResponse>> GetInactiveMembersAsync([Query] int days = 3);
}
