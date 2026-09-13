using System.Security.Cryptography;
using Scholarship.Api.Constants;
using Scholarship.Api.DTOs;
using Scholarship.Api.Exceptions;
using Scholarship.Api.Helpers;
using Scholarship.Api.Interfaces;
using Scholarship.Api.Models;
using Scholarship.Api.Security;

namespace Scholarship.Api.Services;

public interface IStudentProfileService
{
    Task<StudentProfileDto> GetProfileAsync(ulong studentId);
    Task<AddressDto?> GetAddressAsync(ulong studentId, string type);
    Task<ulong> SaveAddressAsync(ulong studentId, SaveAddressDto dto);
    Task<AcademicDetailsDto> GetAcademicDetailsAsync(ulong studentId);
    Task SaveAcademicDetailsAsync(ulong studentId, SaveAcademicDetailsDto dto);
    Task<BankAccountDto?> GetBankAccountAsync(ulong studentId);
    Task<ulong> SaveBankAccountAsync(ulong studentId, SaveBankAccountDto dto);
    Task<List<CertificateDto>> GetCertificatesAsync(ulong studentId);
    Task<ulong> SaveCertificateAsync(ulong studentId, SaveCertificateDto dto);
}

public interface IScholarshipApplicationService
{
    Task<ApplicationSummaryDto?> GetCurrentApplicationAsync(ulong studentId, uint academicYearId);
    Task<ApplicationSummaryDto> SaveDraftApplicationAsync(ulong studentId, uint academicYearId, ulong schemeId, int currentStep);
    Task<bool> LockApplicationAsync(ulong studentId, ApplicationLockDto dto);
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task<StudentDashboardDto> GetStudentDashboardAsync(ulong studentId, uint academicYearId);
}

public interface IVerificationService
{
    Task<PagedResult<ApplicationSummaryDto>> GetInstituteApplicationsAsync(ulong instituteId, int? statusId, int page, int pageSize);
    Task<PagedResult<ApplicationSummaryDto>> GetDistrictApplicationsAsync(ulong districtId, int? statusId, int page, int pageSize);
    Task<bool> SubmitVerificationDecisionAsync(ulong verifiedBy, VerificationDecisionDto dto);
}

public interface IDocumentStorageService
{
    Task<Document> UploadDocumentAsync(ulong? studentId, uint documentTypeId, uint? academicYearId, IFormFile file);
    Task<(Stream stream, string contentType, string fileName)> DownloadDocumentAsync(ulong documentId);
    Task<List<DocumentDto>> GetStudentDocumentsAsync(ulong studentId);
}

public class StudentProfileService : IStudentProfileService
{
    private readonly IStudentRepository _studentRepo;
    private readonly IStudentVaultRepository _vaultRepo;
    private readonly IAddressRepository _addressRepo;
    private readonly IAcademicRepository _academicRepo;
    private readonly IBankRepository _bankRepo;
    private readonly ICertificateRepository _certRepo;
    private readonly ICryptoService _crypto;
    private readonly IHmacBlindHasher _hasher;

    public StudentProfileService(
        IStudentRepository studentRepo,
        IStudentVaultRepository vaultRepo,
        IAddressRepository addressRepo,
        IAcademicRepository academicRepo,
        IBankRepository bankRepo,
        ICertificateRepository certRepo,
        ICryptoService crypto,
        IHmacBlindHasher hasher)
    {
        _studentRepo = studentRepo;
        _vaultRepo = vaultRepo;
        _addressRepo = addressRepo;
        _academicRepo = academicRepo;
        _bankRepo = bankRepo;
        _certRepo = certRepo;
        _crypto = crypto;
        _hasher = hasher;
    }

    public async Task<StudentProfileDto> GetProfileAsync(ulong studentId)
    {
        var student = await _studentRepo.GetByIdAsync(studentId)
            ?? throw new NotFoundException("Student profile not found.");

        var aadhaar = await _vaultRepo.GetAadhaarByStudentIdAsync(studentId);
        var contact = await _vaultRepo.GetContactByStudentIdAsync(studentId);
        var family = await _vaultRepo.GetFamilyDetailsByStudentIdAsync(studentId);
        var household = await _vaultRepo.GetHouseholdDetailsByStudentIdAsync(studentId);
        var depCriteria = await _vaultRepo.GetDeprivationCriteriaByStudentIdAsync(studentId);

        string mobile = contact != null ? _crypto.Decrypt(contact.MobileEncrypted) : string.Empty;
        string email = contact != null ? _crypto.Decrypt(contact.EmailEncrypted) : string.Empty;

        return new StudentProfileDto
        {
            StudentId = student.StudentId,
            StudentCode = student.StudentCode,
            FirstName = student.FirstName,
            MiddleName = student.MiddleName,
            LastName = student.LastName,
            DateOfBirth = student.DateOfBirth,
            GenderId = student.GenderId,
            CategoryId = student.CategoryId,
            ReligionId = student.ReligionId,
            MaskedAadhaar = aadhaar?.MaskedAadhaar ?? "XXXX-XXXX-XXXX",
            MaskedMobile = MaskingHelper.MaskMobile(mobile),
            MaskedEmail = MaskingHelper.MaskEmail(email),
            PhotoDocumentId = student.PhotoDocumentId,
            FatherGuardianName = family?.FatherGuardianName ?? string.Empty,
            MotherName = family?.MotherName ?? string.Empty,
            IsOrphan = family?.IsOrphan ?? false,
            IsMotherSingleWoman = family?.IsMotherSingleWoman ?? false,
            FatherOccupationId = family?.FatherOccupationId,
            MotherOccupationId = family?.MotherOccupationId,
            IsDifferentlyAbled = family?.IsDifferentlyAbled ?? false,
            ParentsIlliterate = family?.ParentsIlliterate ?? false,
            HouseholdCategoryId = household?.HouseholdCategoryId,
            AnnualIncome = household?.AnnualIncome ?? 0,
            ApplicableDeprivationCriteria = depCriteria
        };
    }

    public async Task<AddressDto?> GetAddressAsync(ulong studentId, string type)
    {
        var addr = await _addressRepo.GetAddressByTypeAsync(studentId, type);
        if (addr == null) return null;

        return new AddressDto
        {
            AddressId = addr.AddressId,
            StudentId = addr.StudentId,
            AddressType = addr.AddressType,
            AddressLine = addr.AddressLine,
            Pincode = addr.Pincode,
            StateId = addr.StateId,
            DistrictId = addr.DistrictId,
            BlockId = addr.BlockId,
            VidhansabhaId = addr.VidhansabhaId,
            CityVillageId = addr.CityVillageId,
            PostOfficeId = addr.PostOfficeId
        };
    }

    public async Task<ulong> SaveAddressAsync(ulong studentId, SaveAddressDto dto)
    {
        var address = new Address
        {
            StudentId = studentId,
            AddressType = dto.AddressType,
            AddressLine = dto.AddressLine.Trim(),
            Pincode = dto.Pincode.Trim(),
            StateId = dto.StateId,
            DistrictId = dto.DistrictId,
            BlockId = dto.BlockId,
            VidhansabhaId = dto.VidhansabhaId,
            CityVillageId = dto.CityVillageId,
            PostOfficeId = dto.PostOfficeId
        };

        return await _addressRepo.SaveAddressAsync(address);
    }

    public async Task<AcademicDetailsDto> GetAcademicDetailsAsync(ulong studentId)
    {
        var tenth = await _academicRepo.Get10thDetailsByStudentIdAsync(studentId);
        var prev = await _academicRepo.GetPreviousEducationByStudentIdAsync(studentId);
        var current = await _academicRepo.GetAcademicRecordByStudentIdAsync(studentId);

        return new AcademicDetailsDto
        {
            Class10Id = tenth?.Class10Id,
            TenthRollNumber = tenth?.RollNumber ?? string.Empty,
            SchoolType = tenth?.SchoolType ?? "GOVERNMENT",
            TenthPassingYear = tenth?.PassingYear ?? 0,
            TenthBoardId = tenth?.BoardId ?? 0,
            TenthPercentage = tenth?.Percentage ?? 0,
            TenthMarksheetDocId = tenth?.MarksheetDocumentId,

            PreviousEducationId = prev?.PreviousEducationId,
            PreviousCourseTypeId = prev?.CourseTypeId ?? 0,
            PreviousCourseId = prev?.CourseId ?? 0,
            PreviousBranchId = prev?.BranchId,
            PreviousInstituteName = prev?.InstituteName ?? string.Empty,
            PreviousRollNumber = prev?.RollNumber ?? string.Empty,
            PreviousPassingYear = prev?.PassingYear ?? 0,
            PreviousPercentage = prev?.Percentage ?? 0,
            PreviousMarksheetDocId = prev?.MarksheetDocumentId,

            AcademicRecordId = current?.AcademicRecordId,
            AcademicYearId = current?.AcademicYearId ?? 0,
            SchemeId = current?.SchemeId ?? 0,
            InstituteCourseId = current?.InstituteCourseId ?? 0,
            AdmissionDate = current?.AdmissionDate ?? DateTime.MinValue,
            EnrollmentNumber = current?.EnrollmentNumber ?? string.Empty,
            EnrollmentDate = current?.EnrollmentDate,
            AdmissionTypeId = current?.AdmissionTypeId ?? 0,
            StudyModeId = current?.StudyModeId ?? 0,
            CourseYear = current?.CourseYear ?? 1,
            IsHosteller = current?.IsHosteller ?? false,
            IsLateralEntry = current?.IsLateralEntry ?? false,
            IsLocked = current?.IsLocked ?? false
        };
    }

    public async Task SaveAcademicDetailsAsync(ulong studentId, SaveAcademicDetailsDto dto)
    {
        // 1. Save 10th
        var tenth = new Student10thDetail
        {
            StudentId = studentId,
            RollNumber = dto.TenthRollNumber.Trim(),
            SchoolType = dto.SchoolType,
            PassingYear = dto.TenthPassingYear,
            BoardId = dto.TenthBoardId,
            Percentage = dto.TenthPercentage,
            MarksheetDocumentId = dto.TenthMarksheetDocId
        };
        await _academicRepo.Save10thDetailsAsync(tenth);

        // 2. Save Current Academic Record
        var record = new StudentAcademicRecord
        {
            StudentId = studentId,
            AcademicYearId = dto.AcademicYearId,
            SchemeId = dto.SchemeId,
            InstituteCourseId = dto.InstituteCourseId,
            AdmissionDate = dto.AdmissionDate,
            EnrollmentNumber = dto.EnrollmentNumber.Trim(),
            EnrollmentDate = dto.EnrollmentDate,
            AdmissionTypeId = dto.AdmissionTypeId,
            StudyModeId = dto.StudyModeId,
            CourseYear = dto.CourseYear,
            IsHosteller = dto.IsHosteller,
            IsLateralEntry = dto.IsLateralEntry,
            IsLocked = false
        };
        ulong academicRecordId = await _academicRepo.SaveAcademicRecordAsync(record);

        // 3. Save Previous Education
        var prev = new PreviousEducation
        {
            StudentId = studentId,
            AcademicRecordId = academicRecordId,
            CourseTypeId = dto.PreviousCourseTypeId,
            CourseId = dto.PreviousCourseId,
            BranchId = dto.PreviousBranchId,
            InstituteName = dto.PreviousInstituteName.Trim(),
            RollNumber = dto.PreviousRollNumber.Trim(),
            PassingYear = dto.PreviousPassingYear,
            Percentage = dto.PreviousPercentage,
            MarksheetDocumentId = dto.PreviousMarksheetDocId
        };
        await _academicRepo.SavePreviousEducationAsync(prev);
    }

    public async Task<BankAccountDto?> GetBankAccountAsync(ulong studentId)
    {
        var acc = await _bankRepo.GetActiveBankAccountByStudentIdAsync(studentId);
        if (acc == null) return null;

        return new BankAccountDto
        {
            StudentBankAccountId = acc.StudentBankAccountId,
            StudentId = acc.StudentId,
            BankId = acc.BankId,
            BranchId = acc.BranchId,
            MaskedAccountNumber = acc.MaskedAccountNumber,
            IsAadhaarSeeded = acc.IsAadhaarSeeded,
            VerificationStatus = acc.VerificationStatus,
            IsActive = acc.IsActive
        };
    }

    public async Task<ulong> SaveBankAccountAsync(ulong studentId, SaveBankAccountDto dto)
    {
        if (dto.AccountNumber.Trim() != dto.ConfirmAccountNumber.Trim())
            throw new ValidationAppException("Account Number and Confirm Account Number do not match.");

        string accountNo = dto.AccountNumber.Trim();
        var account = new StudentBankAccount
        {
            StudentId = studentId,
            BankId = dto.BankId,
            BranchId = dto.BranchId,
            AccountNumberEncrypted = _crypto.Encrypt(accountNo),
            AccountNumberHash = _hasher.ComputeHash(accountNo),
            MaskedAccountNumber = MaskingHelper.MaskBankAccount(accountNo),
            IsAadhaarSeeded = dto.IsAadhaarSeeded,
            VerificationStatus = "VERIFIED",
            IsActive = true,
            ValidFrom = DateTime.UtcNow
        };

        return await _bankRepo.SaveBankAccountAsync(account);
    }

    public async Task<List<CertificateDto>> GetCertificatesAsync(ulong studentId)
    {
        var list = await _certRepo.GetCertificatesByStudentIdAsync(studentId);
        return list.Select(c =>
        {
            string refNo = _crypto.Decrypt(c.ReferenceNumberEncrypted);
            return new CertificateDto
            {
                CertificateId = c.CertificateId,
                StudentId = c.StudentId,
                CertificateTypeId = c.CertificateTypeId,
                IsOnlineGenerated = c.IsOnlineGenerated,
                GeneratedFrom = c.GeneratedFrom,
                MaskedReferenceNumber = refNo.Length > 4 ? $"{refNo[..3]}***{refNo[^3..]}" : "***",
                IssueDate = c.IssueDate,
                DocumentId = c.DocumentId,
                VerificationStatus = c.VerificationStatus
            };
        }).ToList();
    }

    public async Task<ulong> SaveCertificateAsync(ulong studentId, SaveCertificateDto dto)
    {
        string refNo = dto.ReferenceNumber.Trim();
        var cert = new StudentCertificate
        {
            StudentId = studentId,
            CertificateTypeId = dto.CertificateTypeId,
            IsOnlineGenerated = dto.IsOnlineGenerated,
            GeneratedFrom = dto.GeneratedFrom,
            ReferenceNumberEncrypted = _crypto.Encrypt(refNo),
            ReferenceNumberHash = _hasher.ComputeHash(refNo),
            IssueDate = dto.IssueDate,
            DocumentId = dto.DocumentId,
            VerificationStatus = "VERIFIED",
            VerifiedAt = DateTime.UtcNow
        };

        return await _certRepo.SaveCertificateAsync(cert);
    }
}

public class ScholarshipApplicationService : IScholarshipApplicationService
{
    private readonly IScholarshipApplicationRepository _appRepo;
    private readonly IAcademicRepository _academicRepo;
    private readonly IOtpService _otpService;
    private readonly IStudentRepository _studentRepo;
    private readonly IStudentVaultRepository _vaultRepo;
    private readonly IBankRepository _bankRepo;
    private readonly IDocumentRepository _docRepo;
    private readonly ICertificateRepository _certRepo;

    public ScholarshipApplicationService(
        IScholarshipApplicationRepository appRepo,
        IAcademicRepository academicRepo,
        IOtpService otpService,
        IStudentRepository studentRepo,
        IStudentVaultRepository vaultRepo,
        IBankRepository bankRepo,
        IDocumentRepository docRepo,
        ICertificateRepository certRepo)
    {
        _appRepo = appRepo;
        _academicRepo = academicRepo;
        _otpService = otpService;
        _studentRepo = studentRepo;
        _vaultRepo = vaultRepo;
        _bankRepo = bankRepo;
        _docRepo = docRepo;
        _certRepo = certRepo;
    }

    public async Task<ApplicationSummaryDto?> GetCurrentApplicationAsync(ulong studentId, uint academicYearId)
    {
        var detailed = await _appRepo.GetApplicationSummaryDetailedAsync(studentId, academicYearId);
        if (detailed != null) return detailed;

        var app = await _appRepo.GetCurrentByStudentIdAsync(studentId, academicYearId);
        if (app == null) return null;

        var student = await _studentRepo.GetByIdAsync(studentId);

        return new ApplicationSummaryDto
        {
            ApplicationId = app.ApplicationId,
            ApplicationNumber = app.ApplicationNumber,
            StudentId = app.StudentId,
            StudentCode = student?.StudentCode,
            StudentName = student != null ? $"{student.FirstName} {student.LastName}".Trim() : null,
            AcademicYearId = app.AcademicYearId,
            SchemeId = app.SchemeId,
            ApplicationStatusId = app.ApplicationStatusId,
            CurrentStep = app.CurrentStep,
            IsLocked = app.IsLocked,
            LockedAt = app.LockedAt,
            CreatedAt = app.CreatedAt
        };
    }

    public async Task<ApplicationSummaryDto> SaveDraftApplicationAsync(ulong studentId, uint academicYearId, ulong schemeId, int currentStep)
    {
        var existing = await _appRepo.GetCurrentByStudentIdAsync(studentId, academicYearId);
        if (existing != null)
        {
            if (existing.IsLocked)
                throw new ValidationAppException("This application has already been locked and cannot be modified.");

            await _appRepo.UpdateStatusAndStepAsync(existing.ApplicationId, AppStatuses.Draft, currentStep);
            return await GetCurrentApplicationAsync(studentId, academicYearId) ?? throw new AppException("Error updating draft.");
        }

        var acadRecord = await _academicRepo.GetAcademicRecordByStudentIdAsync(studentId)
            ?? throw new ValidationAppException("Please complete Academic and Course enrollment before starting an application.");

        string appNumber = $"SCH{DateTime.UtcNow.Year}{RandomNumberGenerator.GetInt32(1000000, 9999999)}";
        var app = new ScholarshipApplication
        {
            ApplicationNumber = appNumber,
            StudentId = studentId,
            AcademicYearId = academicYearId,
            SchemeId = schemeId,
            AcademicRecordId = acadRecord.AcademicRecordId,
            ApplicationStatusId = AppStatuses.Draft,
            CurrentStep = currentStep,
            IsLocked = false
        };

        ulong appId = await _appRepo.CreateApplicationAsync(app);
        return (await GetCurrentApplicationAsync(studentId, academicYearId))!;
    }

    public async Task<bool> LockApplicationAsync(ulong studentId, ApplicationLockDto dto)
    {
        var app = await _appRepo.GetByIdAsync(dto.ApplicationId);
        if (app == null || app.StudentId != studentId)
            throw new NotFoundException("Application not found.");

        if (app.IsLocked)
            throw new ValidationAppException("Application is already locked.");

        // Verify OTP
        bool isVerified = await _otpService.VerifyOtpAsync(studentId, "APPLICATION_LOCK", dto.OtpCode, dto.ApplicationId);
        if (!isVerified)
            throw new ValidationAppException("OTP validation failed.");

        // Lock atomically
        return await _appRepo.LockApplicationAsync(app.ApplicationId, AppStatuses.Locked, studentId, null, "Locked by student with OTP verification");
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        return await _appRepo.GetDashboardStatsAsync();
    }

    public async Task<StudentDashboardDto> GetStudentDashboardAsync(ulong studentId, uint academicYearId)
    {
        var student = await _studentRepo.GetByIdAsync(studentId);
        var app = await _appRepo.GetApplicationSummaryDetailedAsync(studentId, academicYearId);
        var bank = await _bankRepo.GetActiveBankAccountByStudentIdAsync(studentId);
        var otr = await _vaultRepo.GetOtrByStudentIdAsync(studentId);
        var certs = await _certRepo.GetCertificatesByStudentIdAsync(studentId);
        var docs = await _docRepo.GetDocumentsByStudentIdAsync(studentId);
        var family = await _vaultRepo.GetFamilyDetailsByStudentIdAsync(studentId);
        var aadhaar = await _vaultRepo.GetAadhaarByStudentIdAsync(studentId);

        // 1. Profile Completion %
        int profileScore = 0;
        if (student != null && !string.IsNullOrWhiteSpace(student.FirstName)) profileScore += 25;
        if (family != null && !string.IsNullOrWhiteSpace(family.FatherGuardianName)) profileScore += 25;
        if (bank != null) profileScore += 25;
        if (certs != null && certs.Any()) profileScore += 25;

        // 2. Document Completion %
        int docScore = 0;
        if (docs != null)
        {
            if (docs.Any(d => d.DocumentTypeId == AppDocumentTypes.StudentPhoto)) docScore += 20;
            if (docs.Any(d => d.DocumentTypeId == AppDocumentTypes.TenthMarksheet)) docScore += 20;
            if (docs.Any(d => d.DocumentTypeId == AppDocumentTypes.PreviousMarksheet)) docScore += 20;
            if (docs.Any(d => d.DocumentTypeId == AppDocumentTypes.BankPassbook)) docScore += 20;
            if (docs.Any(d => d.DocumentTypeId == AppDocumentTypes.CasteCertificate || d.DocumentTypeId == AppDocumentTypes.IncomeCertificate)) docScore += 20;
        }

        // 3. Status determination
        string verificationStatus = "PENDING";
        if (app != null)
        {
            if (app.ApplicationStatusId == AppStatuses.Approved) verificationStatus = "APPROVED";
            else if (app.ApplicationStatusId == AppStatuses.DistrictVerification) verificationStatus = "INSTITUTE_APPROVED";
            else if (app.ApplicationStatusId == AppStatuses.TemporaryRejected) verificationStatus = "REVERTED";
            else if (app.ApplicationStatusId == AppStatuses.Rejected) verificationStatus = "REJECTED";
            else if (app.IsLocked) verificationStatus = "UNDER_INSTITUTE_VERIFICATION";
        }

        string bankStatus = bank != null && bank.IsAadhaarSeeded ? "SEEDED & VERIFIED" : (bank != null ? "PENDING SEEDING" : "NOT ADDED");
        string otrStatus = otr != null ? "LINKED & VERIFIED" : (aadhaar != null ? "E-KYC VERIFIED" : "PENDING");

        return new StudentDashboardDto
        {
            ApplicationId = app?.ApplicationId ?? 0,
            ApplicationNumber = app?.ApplicationNumber ?? "N/A",
            StudentId = studentId,
            StudentCode = student?.StudentCode ?? "N/A",
            StudentName = student != null ? $"{student.FirstName} {student.LastName}".Trim() : "Student",
            AcademicYear = app?.AcademicYearCode ?? "2025-26",
            Scheme = app?.SchemeName ?? "Post Matric Scholarship",
            Category = student?.CategoryId == 1 ? "SC" : (student?.CategoryId == 2 ? "ST" : (student?.CategoryId == 3 ? "OBC" : "GENERAL")),
            CurrentCourse = app?.CourseName ?? "Not Selected",
            Institute = app?.InstituteName ?? "Not Selected",
            ApplicationStatusId = app?.ApplicationStatusId ?? AppStatuses.Draft,
            ApplicationStatus = app?.StatusName ?? (app?.IsLocked == true ? "LOCKED" : "DRAFT"),
            ProfileCompletion = profileScore,
            DocumentCompletion = docScore,
            VerificationStatus = verificationStatus,
            BankStatus = bankStatus,
            OtrStatus = otrStatus,
            IsLocked = app?.IsLocked ?? false,
            LockedAt = app?.LockedAt
        };
    }
}

public class VerificationService : IVerificationService
{
    private readonly IScholarshipApplicationRepository _appRepo;
    private readonly IVerificationRepository _verifRepo;

    public VerificationService(
        IScholarshipApplicationRepository appRepo,
        IVerificationRepository verifRepo)
    {
        _appRepo = appRepo;
        _verifRepo = verifRepo;
    }

    public async Task<PagedResult<ApplicationSummaryDto>> GetInstituteApplicationsAsync(ulong instituteId, int? statusId, int page, int pageSize)
    {
        int skip = (page - 1) * pageSize;
        var apps = await _appRepo.GetApplicationsForInstituteAsync(instituteId, statusId, skip, pageSize);
        int total = await _appRepo.GetApplicationsCountForInstituteAsync(instituteId, statusId);

        var dtos = apps.Select(a => new ApplicationSummaryDto
        {
            ApplicationId = a.ApplicationId,
            ApplicationNumber = a.ApplicationNumber,
            StudentId = a.StudentId,
            AcademicYearId = a.AcademicYearId,
            SchemeId = a.SchemeId,
            ApplicationStatusId = a.ApplicationStatusId,
            CurrentStep = a.CurrentStep,
            IsLocked = a.IsLocked,
            LockedAt = a.LockedAt,
            CreatedAt = a.CreatedAt
        }).ToList();

        return new PagedResult<ApplicationSummaryDto>
        {
            Items = dtos,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<ApplicationSummaryDto>> GetDistrictApplicationsAsync(ulong districtId, int? statusId, int page, int pageSize)
    {
        int skip = (page - 1) * pageSize;
        var apps = await _appRepo.GetApplicationsForDistrictAsync(districtId, statusId, skip, pageSize);
        int total = await _appRepo.GetApplicationsCountForDistrictAsync(districtId, statusId);

        var dtos = apps.Select(a => new ApplicationSummaryDto
        {
            ApplicationId = a.ApplicationId,
            ApplicationNumber = a.ApplicationNumber,
            StudentId = a.StudentId,
            AcademicYearId = a.AcademicYearId,
            SchemeId = a.SchemeId,
            ApplicationStatusId = a.ApplicationStatusId,
            CurrentStep = a.CurrentStep,
            IsLocked = a.IsLocked,
            LockedAt = a.LockedAt,
            CreatedAt = a.CreatedAt
        }).ToList();

        return new PagedResult<ApplicationSummaryDto>
        {
            Items = dtos,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> SubmitVerificationDecisionAsync(ulong verifiedBy, VerificationDecisionDto dto)
    {
        var app = await _appRepo.GetByIdAsync(dto.ApplicationId)
            ?? throw new NotFoundException("Application not found.");

        if (dto.VerificationLevel == "INSTITUTE")
        {
            if (app.ApplicationStatusId != AppStatuses.Locked && app.ApplicationStatusId != AppStatuses.InstituteVerification)
            {
                throw new ValidationAppException($"Invalid state transition. Application status is currently {app.ApplicationStatusId}, which cannot be reviewed by the Institute.");
            }
        }
        else if (dto.VerificationLevel == "DISTRICT")
        {
            if (app.ApplicationStatusId != AppStatuses.DistrictVerification)
            {
                throw new ValidationAppException($"Invalid state transition. Application must be approved by Institute and marked for District Verification before District scrutiny.");
            }
        }
        else
        {
            throw new ValidationAppException($"Unsupported verification level '{dto.VerificationLevel}'.");
        }

        var verification = new ApplicationVerification
        {
            ApplicationId = dto.ApplicationId,
            VerificationLevel = dto.VerificationLevel,
            VerifiedBy = verifiedBy,
            Status = dto.Status.ToUpperInvariant(),
            Remarks = dto.Remarks
        };

        ulong verifId = await _verifRepo.RecordVerificationAsync(verification);
        return verifId > 0;
    }
}

public class DocumentStorageService : IDocumentStorageService
{
    private readonly IDocumentRepository _docRepo;
    private readonly string _storagePath;

    public DocumentStorageService(IDocumentRepository docRepo, IConfiguration config)
    {
        _docRepo = docRepo;
        _storagePath = config["Storage:Path"] ?? Path.Combine(AppContext.BaseDirectory, "UploadedDocuments");
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<Document> UploadDocumentAsync(ulong? studentId, uint documentTypeId, uint? academicYearId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ValidationAppException("No file provided for upload.");

        // Security check: restrict size (e.g. max 5MB)
        if (file.Length > 5 * 1024 * 1024)
            throw new ValidationAppException("File size exceeds 5MB limit.");

        // Security check: allowed extensions
        var allowedExts = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
        string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExts.Contains(ext))
            throw new ValidationAppException("Only PDF, JPG, and PNG files are allowed.");

        // Unique storage key (UUID)
        string storageKey = $"{Guid.NewGuid():N}{ext}";
        string fullPath = Path.Combine(_storagePath, storageKey);

        byte[] fileHash;
        using (var stream = file.OpenReadStream())
        {
            fileHash = await SHA256.HashDataAsync(stream);
        }

        using (var fileStream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        var doc = new Document
        {
            StudentId = studentId,
            DocumentTypeId = documentTypeId,
            AcademicYearId = academicYearId,
            FileName = Path.GetFileName(file.FileName),
            StorageKey = storageKey,
            FileHash = fileHash,
            MimeType = file.ContentType,
            FileSizeBytes = (ulong)file.Length,
            IsVerified = false,
            IsActive = true
        };

        ulong docId = await _docRepo.SaveDocumentAsync(doc);
        doc.DocumentId = docId;

        // Automatically link document to student profile / bank / academic / certificates / application
        if (studentId.HasValue)
        {
            await _docRepo.UpdateEntityDocumentLinkAsync(studentId.Value, documentTypeId, docId);
        }

        return doc;
    }

    public async Task<List<DocumentDto>> GetStudentDocumentsAsync(ulong studentId)
    {
        var docs = await _docRepo.GetDocumentsByStudentIdAsync(studentId);
        return docs.Select(d => new DocumentDto
        {
            DocumentId = d.DocumentId,
            DocumentUUID = d.DocumentUUID,
            StudentId = d.StudentId,
            DocumentTypeId = d.DocumentTypeId,
            AcademicYearId = d.AcademicYearId,
            FileName = d.FileName,
            MimeType = d.MimeType,
            FileSizeBytes = d.FileSizeBytes,
            UploadedAt = d.UploadedAt,
            IsVerified = d.IsVerified
        }).ToList();
    }

    public async Task<(Stream stream, string contentType, string fileName)> DownloadDocumentAsync(ulong documentId)
    {
        var doc = await _docRepo.GetDocumentByIdAsync(documentId)
            ?? throw new NotFoundException("Document not found.");

        string fullPath = Path.Combine(_storagePath, doc.StorageKey);
        if (!File.Exists(fullPath))
            throw new NotFoundException("Document file missing on disk.");

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return (stream, doc.MimeType, doc.FileName);
    }
}
