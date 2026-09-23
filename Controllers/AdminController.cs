using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scholarship.Api.DTOs;
using Scholarship.Api.Helpers;
using Scholarship.Api.Services;

namespace Scholarship.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "SUPER_ADMIN,DEPARTMENT_ADMIN,DISTRICT_ADMIN,INSTITUTE_ADMIN,VERIFIER,PAYMENT_ADMIN")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("students")]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminStudentListItemDto>>>> GetStudents(
        [FromQuery] string? search,
        [FromQuery] ulong? districtId,
        [FromQuery] int? statusId,
        [FromQuery] uint? categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15)
    {
        ulong userId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        string role = User.FindFirstValue(ClaimTypes.Role) ?? "";
        
        ulong? instituteIdClaim = null;
        var instClaim = User.FindFirst("InstituteId")?.Value;
        if (!string.IsNullOrEmpty(instClaim) && ulong.TryParse(instClaim, out ulong iid))
        {
            instituteIdClaim = iid;
        }

        var result = await _adminService.GetStudentsAsync(
            userId,
            role,
            instituteIdClaim,
            search,
            districtId,
            statusId,
            categoryId,
            page,
            pageSize);

        return Ok(ApiResponse<PagedResult<AdminStudentListItemDto>>.Ok(result));
    }

    [HttpGet("students/{id}")]
    public async Task<ActionResult<ApiResponse<AdminStudentDetailsDto>>> GetStudentDetails(ulong id)
    {
        var details = await _adminService.GetStudentDetailsAsync(id);
        if (details == null)
            return NotFound(ApiResponse<AdminStudentDetailsDto>.Fail("Student details not found."));

        return Ok(ApiResponse<AdminStudentDetailsDto>.Ok(details));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<AdminDashboardStatsDto>>> GetStats()
    {
        ulong userId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        string role = User.FindFirstValue(ClaimTypes.Role) ?? "";

        ulong? instituteIdClaim = null;
        var instClaim = User.FindFirst("InstituteId")?.Value;
        if (!string.IsNullOrEmpty(instClaim) && ulong.TryParse(instClaim, out ulong iid))
        {
            instituteIdClaim = iid;
        }

        var stats = await _adminService.GetStatsAsync(userId, role, instituteIdClaim);
        return Ok(ApiResponse<AdminDashboardStatsDto>.Ok(stats));
    }
}
