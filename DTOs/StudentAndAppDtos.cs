using System.ComponentModel.DataAnnotations;

namespace Scholarship.Api.DTOs;

public class StudentProfileDto
{
    public ulong StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string FullName => $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ").Trim();
    public DateTime DateOfBirth { get; set; }
    public uint GenderId { get; set; }
    public string? GenderName { get; set; }
    public uint CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public uint ReligionId { get; set; }
    public string? ReligionName { get; set; }
    public string MaskedAadhaar { get; set; } = string.Empty;
    public string MaskedMobile { get; set; } = string.Empty;
    public string MaskedEmail { get; set; } = string.Empty;
    public string? AlternateMobile { get; set; }
    public string? AlternateEmail { get; set; }
    public ulong? PhotoDocumentId { get; set; }

    // Family details
    public string FatherGuardianName { get; set; } = string.Empty;
    public string MotherName { get; set; } = string.Empty;
    public bool IsOrphan { get; set; }
    public bool IsMotherSingleWoman { get; set; }
    public uint? FatherOccupationId { get; set; }
    public uint? MotherOccupationId { get; set; }
    public bool IsDifferentlyAbled { get; set; }
    public bool ParentsIlliterate { get; set; }

    public string? FirstNameHindi { get; set; }
    public string? MiddleNameHindi { get; set; }
    public string? LastNameHindi { get; set; }
    public string? FatherNameHindi { get; set; }
    public string? MotherNameHindi { get; set; }

    // Household details
    public uint? HouseholdCategoryId { get; set; }
    public decimal AnnualIncome { get; set; }
    public string? MaskedBplNumber { get; set; }
    public List<uint> ApplicableDeprivationCriteria { get; set; } = new();
}

public class UpdateStudentProfileDto
{
    public string? FatherGuardianName { get; set; }
    public string? MotherName { get; set; }
    public bool IsOrphan { get; set; }
    public bool IsMotherSingleWoman { get; set; }
    public uint? FatherOccupationId { get; set; }
    public uint? MotherOccupationId { get; set; }
    public bool IsDifferentlyAbled { get; set; }
    public bool ParentsIlliterate { get; set; }

    public string? AlternateMobile { get; set; }
    public string? AlternateEmail { get; set; }

    public string? FirstNameHindi { get; set; }
    public string? MiddleNameHindi { get; set; }
    public string? LastNameHindi { get; set; }
    public string? FatherNameHindi { get; set; }
    public string? MotherNameHindi { get; set; }

    public uint? ReligionId { get; set; }
    public uint? HouseholdCategoryId { get; set; }
    public decimal AnnualIncome { get; set; }
    public List<uint>? DeprivationCriteria { get; set; }
}

public class AddressDto
{
    public ulong AddressId { get; set; }
    public ulong StudentId { get; set; }
    public string AddressType { get; set; } = "PERMANENT";
    public string AddressLine { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
    public ulong StateId { get; set; }
    public string? StateName { get; set; }
    public ulong DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public ulong? BlockId { get; set; }
    public string? BlockName { get; set; }
    public ulong? VidhansabhaId { get; set; }
    public string? VidhansabhaName { get; set; }
    public ulong? CityVillageId { get; set; }
    public string? CityVillageName { get; set; }
    public ulong? PostOfficeId { get; set; }
    public string? PostOfficeName { get; set; }
}

public class SaveAddressDto
{
    [Required] public string AddressType { get; set; } = "PERMANENT"; // PERMANENT or CORRESPONDENCE
    [Required] public string AddressLine { get; set; } = string.Empty;
    [Required] public string Pincode { get; set; } = string.Empty;
    [Required] public ulong StateId { get; set; }
    [Required] public ulong DistrictId { get; set; }
    public ulong? BlockId { get; set; }
    public ulong? VidhansabhaId { get; set; }
    public ulong? CityVillageId { get; set; }
    public ulong? PostOfficeId { get; set; }
}

public class AcademicDetailsDto
{
    // Class 10th
    public ulong? Class10Id { get; set; }
    public string TenthRollNumber { get; set; } = string.Empty;
    public string SchoolType { get; set; } = "GOVERNMENT";
    public ushort TenthPassingYear { get; set; }
    public uint TenthBoardId { get; set; }
    public string? TenthBoardName { get; set; }
    public decimal TenthPercentage { get; set; }
    public ulong? TenthMarksheetDocId { get; set; }

    // Previous Education
    public ulong? PreviousEducationId { get; set; }
    public uint PreviousCourseTypeId { get; set; }
    public string? PreviousCourseTypeName { get; set; }
    public ulong PreviousCourseId { get; set; }
    public string? PreviousCourseName { get; set; }
    public ulong? PreviousBranchId { get; set; }
    public string? PreviousBranchName { get; set; }
    public string PreviousInstituteName { get; set; } = string.Empty;
    public string PreviousRollNumber { get; set; } = string.Empty;
    public ushort PreviousPassingYear { get; set; }
    public decimal PreviousPercentage { get; set; }
    public ulong? PreviousMarksheetDocId { get; set; }

    // Current Academic Record
    public ulong? AcademicRecordId { get; set; }
    public uint AcademicYearId { get; set; }
    public ulong SchemeId { get; set; }
    public ulong InstituteCourseId { get; set; }
    public ulong DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public ulong InstituteId { get; set; }
    public string? InstituteCode { get; set; }
    public string? InstituteName { get; set; }
    public uint? CourseTypeId { get; set; }
    public ulong CourseId { get; set; }
    public string? CourseCode { get; set; }
    public string? CourseName { get; set; }
    public ulong? BranchId { get; set; }
    public string? BranchName { get; set; }
    public DateTime AdmissionDate { get; set; }
    public string EnrollmentNumber { get; set; } = string.Empty;
    public DateTime? EnrollmentDate { get; set; }
    public uint AdmissionTypeId { get; set; }
    public uint StudyModeId { get; set; }
    public uint CourseYear { get; set; } = 1;
    public bool IsHosteller { get; set; }
    public bool IsLateralEntry { get; set; }
    public bool IsLocked { get; set; }
}

public class SaveAcademicDetailsDto
{
    // 10th
    [Required] public string TenthRollNumber { get; set; } = string.Empty;
    [Required] public string SchoolType { get; set; } = "GOVERNMENT";
    [Required] public ushort TenthPassingYear { get; set; }
    [Required] public uint TenthBoardId { get; set; }
    [Required] public decimal TenthPercentage { get; set; }
    public ulong? TenthMarksheetDocId { get; set; }

    // Previous
    [Required] public uint PreviousCourseTypeId { get; set; }
    [Required] public ulong PreviousCourseId { get; set; }
    public ulong? PreviousBranchId { get; set; }
    [Required] public string PreviousInstituteName { get; set; } = string.Empty;
    [Required] public string PreviousRollNumber { get; set; } = string.Empty;
    [Required] public ushort PreviousPassingYear { get; set; }
    [Required] public decimal PreviousPercentage { get; set; }
    public ulong? PreviousMarksheetDocId { get; set; }

    // Current Course Enrollment
    [Required] public uint AcademicYearId { get; set; }
    [Required] public ulong SchemeId { get; set; }
    [Required] public ulong InstituteCourseId { get; set; }
    [Required] public DateTime AdmissionDate { get; set; }
    [Required] public string EnrollmentNumber { get; set; } = string.Empty;
    public DateTime? EnrollmentDate { get; set; }
    [Required] public uint AdmissionTypeId { get; set; }
    [Required] public uint StudyModeId { get; set; }
    [Required] public uint CourseYear { get; set; } = 1;
    public bool IsHosteller { get; set; }
    public bool IsLateralEntry { get; set; }
}

public class BankAccountDto
{
    public ulong StudentBankAccountId { get; set; }
    public ulong StudentId { get; set; }
    public ulong BankId { get; set; }
    public string? BankName { get; set; }
    public ulong BranchId { get; set; }
    public string? BranchName { get; set; }
    public string? BranchAddress { get; set; }
    public string? IFSCCode { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string MaskedAccountNumber { get; set; } = string.Empty;
    public bool IsAadhaarSeeded { get; set; }
    public string VerificationStatus { get; set; } = "PENDING";
    public bool IsActive { get; set; }
    public ulong? PassbookDocumentId { get; set; }
}

public class SaveBankAccountDto
{
    [Required] public ulong BankId { get; set; }
    [Required] public ulong BranchId { get; set; }
    [Required] public string AccountNumber { get; set; } = string.Empty;
    [Required] public string ConfirmAccountNumber { get; set; } = string.Empty;
    public bool IsAadhaarSeeded { get; set; }
    public ulong? PassbookDocumentId { get; set; }
}

public class CertificateDto
{
    public ulong CertificateId { get; set; }
    public ulong StudentId { get; set; }
    public uint CertificateTypeId { get; set; }
    public string? CertificateTypeName { get; set; }
    public bool IsOnlineGenerated { get; set; }
    public string GeneratedFrom { get; set; } = "EDISTRICT_PORTAL";
    public string ReferenceNumber { get; set; } = string.Empty;
    public string MaskedReferenceNumber { get; set; } = string.Empty;
    public decimal AnnualIncome { get; set; }
    public DateTime? IssueDate { get; set; }
    public ulong? DocumentId { get; set; }
    public string VerificationStatus { get; set; } = "PENDING";
}

public class SaveCertificateDto
{
    [Required] public uint CertificateTypeId { get; set; }
    public bool IsOnlineGenerated { get; set; } = true;
    public string GeneratedFrom { get; set; } = "EDISTRICT_PORTAL";
    [Required] public string ReferenceNumber { get; set; } = string.Empty;
    public decimal AnnualIncome { get; set; }
    public DateTime? IssueDate { get; set; }
    public ulong? DocumentId { get; set; }
}

public class ApplicationSummaryDto
{
    public ulong ApplicationId { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public ulong StudentId { get; set; }
    public string? StudentCode { get; set; }
    public string? StudentName { get; set; }
    public uint AcademicYearId { get; set; }
    public string? AcademicYearCode { get; set; }
    public ulong SchemeId { get; set; }
    public string? SchemeName { get; set; }
    public uint ApplicationStatusId { get; set; }
    public string? StatusCode { get; set; }
    public string? StatusName { get; set; }
    public int CurrentStep { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? InstituteName { get; set; }
    public string? CourseName { get; set; }
}

public class ApplicationLockDto
{
    [Required] public ulong ApplicationId { get; set; }
    [Required] public string OtpCode { get; set; } = string.Empty;
}

public class VerificationDecisionDto
{
    [Required] public ulong ApplicationId { get; set; }
    [Required] public string VerificationLevel { get; set; } = "INSTITUTE"; // INSTITUTE or DISTRICT
    [Required] public string Status { get; set; } = "APPROVED"; // APPROVED, REJECTED, TEMPORARY_REJECTED
    public string? Remarks { get; set; }
}

public class DashboardStatsDto
{
    public int TotalApplications { get; set; }
    public int Draft { get; set; }
    public int Submitted { get; set; }
    public int Locked { get; set; }
    public int PendingInstitute { get; set; }
    public int InstituteApproved { get; set; }
    public int InstituteRejected { get; set; }
    public int PendingDistrict { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Reverted { get; set; }
    public int Disbursed { get; set; }
}

public class StudentDashboardDto
{
    public ulong ApplicationId { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public ulong StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = "2025-26";
    public string Scheme { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CurrentCourse { get; set; } = string.Empty;
    public string Institute { get; set; } = string.Empty;
    public uint ApplicationStatusId { get; set; }
    public string ApplicationStatus { get; set; } = "DRAFT";
    public int ProfileCompletion { get; set; }
    public int DocumentCompletion { get; set; }
    public string VerificationStatus { get; set; } = "PENDING";
    public string BankStatus { get; set; } = "PENDING";
    public string OtrStatus { get; set; } = "PENDING";
    public bool IsLocked { get; set; }
    public DateTime? LockedAt { get; set; }
}

public class DocumentDto
{
    public ulong DocumentId { get; set; }
    public string DocumentUUID { get; set; } = string.Empty;
    public ulong? StudentId { get; set; }
    public uint DocumentTypeId { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public uint? AcademicYearId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public ulong FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
    public bool IsVerified { get; set; }
}
