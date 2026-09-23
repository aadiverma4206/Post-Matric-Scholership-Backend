using Dapper;
using Scholarship.Api.Constants;
using Scholarship.Api.Data;
using Scholarship.Api.DTOs;
using Scholarship.Api.Exceptions;
using Scholarship.Api.Helpers;
using Scholarship.Api.Interfaces;

namespace Scholarship.Api.Services;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepo;
    private readonly IDbConnectionFactory _db;

    public AdminService(IAdminRepository adminRepo, IDbConnectionFactory db)
    {
        _adminRepo = adminRepo;
        _db = db;
    }

    public async Task<PagedResult<AdminStudentListItemDto>> GetStudentsAsync(
        ulong userId,
        string role,
        ulong? instituteIdClaim,
        string? search,
        ulong? districtId,
        int? statusId,
        uint? categoryId,
        int page,
        int pageSize)
    {
        ulong? effectiveDistrictId = districtId;
        ulong? effectiveInstituteId = null;

        if (role == AppRoles.DistrictAdmin)
        {
            using var conn = await _db.CreateConnectionAsync();
            var userDist = await conn.QueryFirstOrDefaultAsync<ulong?>(
                "SELECT DistrictId FROM user_districts WHERE UserId = @UserId AND IsActive = 1 LIMIT 1;",
                new { UserId = userId });

            if (userDist.HasValue)
            {
                effectiveDistrictId = userDist.Value;
            }
        }
        else if (role == AppRoles.InstituteAdmin)
        {
            if (instituteIdClaim.HasValue)
            {
                effectiveInstituteId = instituteIdClaim.Value;
            }
            else
            {
                using var conn = await _db.CreateConnectionAsync();
                effectiveInstituteId = await conn.QueryFirstOrDefaultAsync<ulong?>(
                    "SELECT InstituteId FROM institute_users WHERE UserId = @UserId AND IsActive = 1 LIMIT 1;",
                    new { UserId = userId });
            }
        }

        return await _adminRepo.GetStudentsAsync(
            effectiveDistrictId,
            effectiveInstituteId,
            search,
            statusId,
            categoryId,
            page,
            pageSize);
    }

    public async Task<AdminStudentDetailsDto?> GetStudentDetailsAsync(ulong studentId)
    {
        var details = await _adminRepo.GetStudentDetailsAsync(studentId);
        if (details == null)
            throw new NotFoundException($"Student with ID #{studentId} was not found.");

        return details;
    }

    public async Task<AdminDashboardStatsDto> GetStatsAsync(ulong userId, string role, ulong? instituteIdClaim)
    {
        ulong? effectiveDistrictId = null;
        ulong? effectiveInstituteId = null;

        if (role == AppRoles.DistrictAdmin)
        {
            using var conn = await _db.CreateConnectionAsync();
            effectiveDistrictId = await conn.QueryFirstOrDefaultAsync<ulong?>(
                "SELECT DistrictId FROM user_districts WHERE UserId = @UserId AND IsActive = 1 LIMIT 1;",
                new { UserId = userId });
        }
        else if (role == AppRoles.InstituteAdmin)
        {
            effectiveInstituteId = instituteIdClaim;
            if (!effectiveInstituteId.HasValue)
            {
                using var conn = await _db.CreateConnectionAsync();
                effectiveInstituteId = await conn.QueryFirstOrDefaultAsync<ulong?>(
                    "SELECT InstituteId FROM institute_users WHERE UserId = @UserId AND IsActive = 1 LIMIT 1;",
                    new { UserId = userId });
            }
        }

        return await _adminRepo.GetAdminStatsAsync(effectiveDistrictId, effectiveInstituteId);
    }
}
