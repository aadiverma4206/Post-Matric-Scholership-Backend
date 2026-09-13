using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scholarship.Api.Constants;
using Scholarship.Api.DTOs;
using Scholarship.Api.Helpers;
using Scholarship.Api.Services;

namespace Scholarship.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IStudentProfileService _profileService;

    public ProfileController(IStudentProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<StudentProfileDto>>> GetProfile()
    {
        ulong studentId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var profile = await _profileService.GetProfileAsync(studentId);
        return Ok(ApiResponse<StudentProfileDto>.Ok(profile));
    }
}

[ApiController]
[Route("api/addresses")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly IStudentProfileService _profileService;

    public AddressesController(IStudentProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("{type}")]
    public async Task<ActionResult<ApiResponse<AddressDto?>>> GetAddress(string type)
    {
        ulong studentId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var address = await _profileService.GetAddressAsync(studentId, type.ToUpperInvariant());
        return Ok(ApiResponse<AddressDto?>.Ok(address));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ulong>>> SaveAddress([FromBody] SaveAddressDto dto)
    {
        ulong studentId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        ulong addressId = await _profileService.SaveAddressAsync(studentId, dto);
        return Ok(ApiResponse<ulong>.Ok(addressId, "Address saved successfully."));
    }
}

[ApiController]
[Route("api/academic")]
[Authorize]
public class AcademicController : ControllerBase
{
    private readonly IStudentProfileService _profileService;

    public AcademicController(IStudentProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<AcademicDetailsDto>>> GetAcademicDetails()
    {
        ulong studentId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var details = await _profileService.GetAcademicDetailsAsync(studentId);
        return Ok(ApiResponse<AcademicDetailsDto>.Ok(details));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<bool>>> SaveAcademicDetails([FromBody] SaveAcademicDetailsDto dto)
    {
        ulong studentId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        await _profileService.SaveAcademicDetailsAsync(studentId, dto);
        return Ok(ApiResponse<bool>.Ok(true, "Academic details saved successfully."));
    }
}

[ApiController]
[Route("api/banks")]
[Authorize]
public class BanksController : ControllerBase
{
    private readonly IStudentProfileService _profileService;

    public BanksController(IStudentProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<BankAccountDto?>>> GetBank()
    {
        ulong studentId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var bank = await _profileService.GetBankAccountAsync(studentId);
        return Ok(ApiResponse<BankAccountDto?>.Ok(bank));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ulong>>> SaveBank([FromBody] SaveBankAccountDto dto)
    {
        ulong studentId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        ulong id = await _profileService.SaveBankAccountAsync(studentId, dto);
        return Ok(ApiResponse<ulong>.Ok(id, "Bank account saved successfully."));
    }
}

[ApiController]
[Route("api/certificates")]
[Authorize]
public class CertificatesController : ControllerBase
{
    private readonly IStudentProfileService _profileService;

    public CertificatesController(IStudentProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CertificateDto>>>> GetCertificates()
    {
        ulong studentId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var certs = await _profileService.GetCertificatesAsync(studentId);
        return Ok(ApiResponse<List<CertificateDto>>.Ok(certs));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ulong>>> SaveCertificate([FromBody] SaveCertificateDto dto)
    {
        ulong studentId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        ulong id = await _profileService.SaveCertificateAsync(studentId, dto);
        return Ok(ApiResponse<ulong>.Ok(id, "Certificate saved successfully."));
    }
}

[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentStorageService _storageService;

    public DocumentsController(IDocumentStorageService storageService)
    {
        _storageService = storageService;
    }

    [HttpGet("student")]
    [HttpGet]
    public async Task<IActionResult> GetStudentDocuments()
    {
        ulong studentId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var docs = await _storageService.GetStudentDocumentsAsync(studentId);
        return Ok(ApiResponse<List<DocumentDto>>.Ok(docs));
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadDocument([FromForm] uint documentTypeId, [FromForm] uint? academicYearId, IFormFile file)
    {
        ulong studentId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var doc = await _storageService.UploadDocumentAsync(studentId, documentTypeId, academicYearId, file);
        return Ok(ApiResponse<object>.Ok(new { doc.DocumentId, doc.FileName }, "Document uploaded successfully."));
    }

    [HttpGet("{documentId}/download")]
    public async Task<IActionResult> DownloadDocument(ulong documentId)
    {
        var (stream, contentType, fileName) = await _storageService.DownloadDocumentAsync(documentId);
        return File(stream, contentType, fileName);
    }
}

[ApiController]
[Route("api/applications")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IScholarshipApplicationService _appService;

    public ApplicationsController(IScholarshipApplicationService appService)
    {
        _appService = appService;
    }

    [HttpGet("current")]
    public async Task<ActionResult<ApiResponse<ApplicationSummaryDto?>>> GetCurrentApplication([FromQuery] uint academicYearId)
    {
        ulong studentId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var app = await _appService.GetCurrentApplicationAsync(studentId, academicYearId);
        return Ok(ApiResponse<ApplicationSummaryDto?>.Ok(app));
    }

    [HttpPost("draft")]
    public async Task<ActionResult<ApiResponse<ApplicationSummaryDto>>> SaveDraft([FromQuery] uint academicYearId, [FromQuery] ulong schemeId, [FromQuery] int currentStep)
    {
        ulong studentId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var app = await _appService.SaveDraftApplicationAsync(studentId, academicYearId, schemeId, currentStep);
        return Ok(ApiResponse<ApplicationSummaryDto>.Ok(app, "Application draft updated."));
    }

    [HttpPost("lock")]
    public async Task<ActionResult<ApiResponse<bool>>> LockApplication([FromBody] ApplicationLockDto dto)
    {
        ulong studentId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        bool locked = await _appService.LockApplicationAsync(studentId, dto);
        return Ok(ApiResponse<bool>.Ok(locked, "Application locked successfully. It is now ready for Institute verification."));
    }
}

[ApiController]
[Route("api/verification")]
[Authorize]
public class VerificationController : ControllerBase
{
    private readonly IVerificationService _verifService;

    public VerificationController(IVerificationService verifService)
    {
        _verifService = verifService;
    }

    [HttpGet("institute/queue")]
    public async Task<IActionResult> GetInstituteQueue([FromQuery] ulong instituteId, [FromQuery] int? statusId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _verifService.GetInstituteApplicationsAsync(instituteId, statusId, page, pageSize);
        return Ok(ApiResponse<PagedResult<ApplicationSummaryDto>>.Ok(result));
    }

    [HttpGet("district/queue")]
    public async Task<IActionResult> GetDistrictQueue([FromQuery] ulong districtId, [FromQuery] int? statusId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _verifService.GetDistrictApplicationsAsync(districtId, statusId, page, pageSize);
        return Ok(ApiResponse<PagedResult<ApplicationSummaryDto>>.Ok(result));
    }

    [HttpPost("decision")]
    public async Task<IActionResult> SubmitDecision([FromBody] VerificationDecisionDto dto)
    {
        ulong officerId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        bool success = await _verifService.SubmitVerificationDecisionAsync(officerId, dto);
        return Ok(ApiResponse<bool>.Ok(success, $"Verification {dto.Status} recorded successfully."));
    }
}

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IScholarshipApplicationService _appService;

    public DashboardController(IScholarshipApplicationService appService)
    {
        _appService = appService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _appService.GetDashboardStatsAsync();
        return Ok(ApiResponse<DashboardStatsDto>.Ok(stats));
    }

    [HttpGet("student")]
    public async Task<IActionResult> GetStudentDashboard([FromQuery] uint academicYearId = 2)
    {
        ulong studentId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var dashboard = await _appService.GetStudentDashboardAsync(studentId, academicYearId);
        return Ok(ApiResponse<StudentDashboardDto>.Ok(dashboard));
    }
}
