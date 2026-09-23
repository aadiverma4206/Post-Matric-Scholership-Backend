using Scholarship.Api.DTOs;
using Scholarship.Api.Helpers;

namespace Scholarship.Api.Services;

public interface IAdminService
{
    Task<PagedResult<AdminStudentListItemDto>> GetStudentsAsync(
        ulong userId,
        string role,
        ulong? instituteIdClaim,
        string? search,
        ulong? districtId,
        int? statusId,
        uint? categoryId,
        int page,
        int pageSize);

    Task<AdminStudentDetailsDto?> GetStudentDetailsAsync(ulong studentId);

    Task<AdminDashboardStatsDto> GetStatsAsync(ulong userId, string role, ulong? instituteIdClaim);
}
