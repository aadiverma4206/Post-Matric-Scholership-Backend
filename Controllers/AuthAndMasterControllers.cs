using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scholarship.Api.DTOs;
using Scholarship.Api.Helpers;
using Scholarship.Api.Services;

namespace Scholarship.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IOtpService _otpService;

    public AuthController(IAuthService authService, IOtpService otpService)
    {
        _authService = authService;
        _otpService = otpService;
    }

    [HttpPost("student/register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResultDto>>> RegisterStudent([FromBody] StudentRegisterDto dto)
    {
        var result = await _authService.RegisterStudentAsync(dto);
        return Ok(ApiResponse<AuthResultDto>.Ok(result, "Student registered successfully."));
    }

    [HttpPost("student/login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResultDto>>> LoginStudent([FromBody] StudentLoginDto dto)
    {
        var result = await _authService.LoginStudentAsync(dto);
        return Ok(ApiResponse<AuthResultDto>.Ok(result, "Student logged in successfully."));
    }

    [HttpPost("official/login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResultDto>>> LoginOfficial([FromBody] OfficialLoginDto dto)
    {
        var result = await _authService.LoginOfficialAsync(dto);
        return Ok(ApiResponse<AuthResultDto>.Ok(result, "Official user logged in successfully."));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<string>>> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        string otpCode = await _authService.ForgotPasswordAsync(dto.UserName, dto.MobileOrEmail);
        return Ok(ApiResponse<string>.Ok(otpCode, "OTP has been generated and dispatched to your registered contact."));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPassword([FromBody] ForgotPasswordDto dto)
    {
        if (string.IsNullOrEmpty(dto.OtpCode) || string.IsNullOrEmpty(dto.NewPassword))
            return BadRequest(ApiResponse<bool>.Fail("OTP code and New Password are required."));

        bool success = await _authService.ResetPasswordAsync(dto.UserName, dto.OtpCode, dto.NewPassword);
        return Ok(ApiResponse<bool>.Ok(success, "Password has been successfully updated. You may now login."));
    }

    [HttpPost("forgot-userid")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ForgotUserIdResultDto>>> ForgotUserId([FromBody] ForgotUserIdDto dto)
    {
        var result = await _authService.ForgotUserIdAsync(dto.EmailOrMobile);
        return Ok(ApiResponse<ForgotUserIdResultDto>.Ok(result, "User ID retrieved successfully."));
    }

    [HttpPost("otp/generate")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> GenerateOtp([FromBody] OtpRequestDto dto)
    {
        ulong studentId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        string otpCode = await _otpService.GenerateAndSendOtpAsync(studentId, dto.Purpose, dto.ApplicationId);
        return Ok(ApiResponse<string>.Ok(otpCode, "OTP generated and dispatched successfully."));
    }

    [HttpPost("otp/verify")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> VerifyOtp([FromBody] OtpVerifyDto dto)
    {
        ulong studentId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        bool isValid = await _otpService.VerifyOtpAsync(studentId, dto.Purpose, dto.OtpCode, dto.ApplicationId);
        return Ok(ApiResponse<bool>.Ok(isValid, "OTP verified successfully."));
    }
}

[ApiController]
[Route("api/masters")]
public class MastersController : ControllerBase
{
    private readonly IMasterDataService _masterService;

    public MastersController(IMasterDataService masterService)
    {
        _masterService = masterService;
    }

    [HttpGet("academic-years")]
    public async Task<IActionResult> GetAcademicYears()
    {
        var list = await _masterService.GetAcademicYearsAsync();
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = (ulong)x.AcademicYearId, code = x.AcademicYearCode, name = x.AcademicYearCode, isCurrent = x.IsCurrent })));
    }

    [HttpGet("schemes")]
    public async Task<IActionResult> GetSchemes([FromQuery] uint? academicYearId)
    {
        var list = await _masterService.GetSchemesAsync(academicYearId);
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = (ulong)x.SchemeId, code = x.SchemeCode, name = x.SchemeName })));
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var list = await _masterService.GetCategoriesAsync();
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = (ulong)x.CategoryId, code = x.Code, name = x.Name })));
    }

    [HttpGet("genders")]
    public async Task<IActionResult> GetGenders()
    {
        var list = await _masterService.GetGendersAsync();
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = (ulong)x.GenderId, code = x.Code, name = x.Name })));
    }

    [HttpGet("religions")]
    public async Task<IActionResult> GetReligions()
    {
        var list = await _masterService.GetReligionsAsync();
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = (ulong)x.ReligionId, code = x.Code, name = x.Name })));
    }

    [HttpGet("states")]
    public async Task<IActionResult> GetStates()
    {
        var list = await _masterService.GetStatesAsync();
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = x.StateId, code = x.StateCode, name = x.StateName })));
    }

    [HttpGet("districts")]
    public async Task<IActionResult> GetDistricts([FromQuery] ulong stateId)
    {
        var list = await _masterService.GetDistrictsAsync(stateId);
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = x.DistrictId, code = x.DistrictCode, name = x.DistrictName, stateId = x.StateId })));
    }

    [HttpGet("blocks")]
    public async Task<IActionResult> GetBlocks([FromQuery] ulong districtId)
    {
        var list = await _masterService.GetBlocksAsync(districtId);
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = x.BlockId, code = x.BlockCode, name = x.BlockName, districtId = x.DistrictId })));
    }

    [HttpGet("vidhansabhas")]
    public async Task<IActionResult> GetVidhansabhas([FromQuery] ulong districtId)
    {
        var list = await _masterService.GetVidhansabhasAsync(districtId);
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = x.VidhansabhaId, code = x.VidhansabhaCode, name = x.VidhansabhaName, districtId = x.DistrictId })));
    }

    [HttpGet("cities-villages")]
    [HttpGet("cities")]
    public async Task<IActionResult> GetCitiesVillages([FromQuery] ulong? districtId, [FromQuery] ulong? blockId)
    {
        var list = await _masterService.GetCitiesVillagesAsync(districtId ?? 1, blockId);
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = x.CityVillageId, code = x.CityVillageId.ToString(), name = x.Name, districtId = x.DistrictId, blockId = x.BlockId })));
    }

    [HttpGet("post-offices")]
    public async Task<IActionResult> GetPostOffices([FromQuery] ulong? districtId, [FromQuery] string? pincode)
    {
        var list = await _masterService.GetPostOfficesAsync(districtId ?? 1);
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = x.PostOfficeId, code = x.Pincode, name = $"{x.PostOfficeName} ({x.Pincode})", districtId = x.DistrictId })));
    }

    [HttpGet("banks")]
    public async Task<IActionResult> GetBanks()
    {
        var list = await _masterService.GetBanksAsync();
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = x.BankId, code = x.BankCode, name = x.BankName })));
    }

    [HttpGet("bank-branches")]
    public async Task<IActionResult> GetBankBranches([FromQuery] ulong bankId)
    {
        var list = await _masterService.GetBankBranchesAsync(bankId);
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = x.BranchId, code = x.IFSCCode, name = $"{x.BranchName} ({x.IFSCCode})", bankId = x.BankId, ifscCode = x.IFSCCode })));
    }

    [HttpGet("course-types")]
    public async Task<IActionResult> GetCourseTypes()
    {
        var list = await _masterService.GetCourseTypesAsync();
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = (ulong)x.CourseTypeId, code = x.Code, name = x.Name })));
    }

    [HttpGet("courses")]
    public async Task<IActionResult> GetCourses([FromQuery] uint? courseTypeId)
    {
        var list = await _masterService.GetCoursesAsync(courseTypeId);
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = x.CourseId, code = x.CourseCode, name = x.CourseName, courseTypeId = x.CourseTypeId })));
    }

    [HttpGet("course-branches")]
    [HttpGet("branches")]
    public async Task<IActionResult> GetCourseBranches([FromQuery] ulong courseId)
    {
        var list = await _masterService.GetCourseBranchesAsync(courseId);
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = x.BranchId, code = x.BranchCode, name = x.BranchName, courseId = x.CourseId })));
    }

    [HttpGet("institutes")]
    public async Task<IActionResult> GetInstitutes([FromQuery] ulong districtId)
    {
        var list = await _masterService.GetInstitutesAsync(districtId);
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = x.InstituteId, code = x.InstituteCode, name = $"{x.InstituteName} / {x.InstituteCode}", districtId = x.DistrictId })));
    }

    [HttpGet("institute-courses")]
    public async Task<IActionResult> GetInstituteCourses([FromQuery] ulong instituteId)
    {
        var list = await _masterService.GetInstituteCoursesAsync(instituteId);
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = x.InstituteCourseId, code = x.CourseCode ?? x.CourseId.ToString(), name = $"Course #{x.CourseId}", instituteId = x.InstituteId })));
    }

    [HttpGet("statuses")]
    public async Task<IActionResult> GetStatuses()
    {
        var list = await _masterService.GetApplicationStatusesAsync();
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = (ulong)x.ApplicationStatusId, code = x.Code, name = x.Name })));
    }

    [HttpGet("occupations")]
    public async Task<IActionResult> GetOccupations()
    {
        var list = await _masterService.GetOccupationsAsync();
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = (ulong)x.OccupationId, code = x.Code, name = x.Name })));
    }

    [HttpGet("household-categories")]
    public async Task<IActionResult> GetHouseholdCategories()
    {
        var list = await _masterService.GetHouseholdCategoriesAsync();
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = (ulong)x.HouseholdCategoryId, code = x.Code, name = x.Name })));
    }

    [HttpGet("deprivation-criteria")]
    public async Task<IActionResult> GetDeprivationCriteria()
    {
        var list = await _masterService.GetDeprivationCriteriaAsync();
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = (ulong)x.CriterionId, code = x.Code, name = x.Name })));
    }

    [HttpGet("admission-types")]
    public async Task<IActionResult> GetAdmissionTypes()
    {
        var list = await _masterService.GetAdmissionTypesAsync();
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = (ulong)x.AdmissionTypeId, code = x.Code, name = x.Name })));
    }

    [HttpGet("study-modes")]
    public async Task<IActionResult> GetStudyModes()
    {
        var list = await _masterService.GetStudyModesAsync();
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = (ulong)x.StudyModeId, code = x.Code, name = x.Name })));
    }

    [HttpGet("education-boards")]
    public async Task<IActionResult> GetEducationBoards()
    {
        var list = await _masterService.GetEducationBoardsAsync();
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = (ulong)x.BoardId, code = x.Code, name = x.Name })));
    }

    [HttpGet("document-types")]
    public async Task<IActionResult> GetDocumentTypes()
    {
        var list = await _masterService.GetDocumentTypesAsync();
        return Ok(ApiResponse<object>.Ok(list.Select(x => new { id = (ulong)x.DocumentTypeId, code = x.Code, name = x.Name, maxSizeBytes = x.MaxSizeBytes, allowedMimeTypes = x.AllowedMimeTypes })));
    }
}
