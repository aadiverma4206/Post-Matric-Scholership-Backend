using Scholarship.Api.DTOs;
using Scholarship.Api.Helpers;

namespace Scholarship.Api.Interfaces;

public interface IAdminRepository
{
    Task<PagedResult<AdminStudentListItemDto>> GetStudentsAsync(
        ulong? districtId, 
        ulong? instituteId, 
        string? search, 
        int? statusId, 
        uint? categoryId, 
        int page, 
        int pageSize);

    Task<AdminStudentDetailsDto?> GetStudentDetailsAsync(ulong studentId);

    Task<AdminDashboardStatsDto> GetAdminStatsAsync(ulong? districtId, ulong? instituteId);
}
