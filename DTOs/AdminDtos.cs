namespace Scholarship.Api.DTOs;

public class AdminStudentListItemDto
{
    public ulong StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string FullName => string.Join(" ", new[] { FirstName, MiddleName, LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
    public DateTime DateOfBirth { get; set; }
    public uint GenderId { get; set; }
    public string GenderName { get; set; } = string.Empty;
    public uint CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ReligionName { get; set; } = string.Empty;
    public ulong? DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public string? AddressLine { get; set; }
    public string? Pincode { get; set; }
    public ulong? ApplicationId { get; set; }
    public string? ApplicationNumber { get; set; }
    public uint? ApplicationStatusId { get; set; }
    public string? ApplicationStatusCode { get; set; }
    public string? ApplicationStatusName { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockedAt { get; set; }
    public ulong? InstituteId { get; set; }
    public string? InstituteName { get; set; }
    public string? CourseName { get; set; }
    public string? BranchName { get; set; }
    public bool IsActive { get; set; }
    public DateTime RegistrationDate { get; set; }
}

public class AdminStudentDetailsDto
{
    // 1. Personal Identity
    public ulong StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string FullName => string.Join(" ", new[] { FirstName, MiddleName, LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
    public DateTime DateOfBirth { get; set; }
    public string GenderName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string ReligionName { get; set; } = string.Empty;
    public ulong? PhotoDocumentId { get; set; }
    public bool IsActive { get; set; }
    public DateTime RegistrationDate { get; set; }

    // 2. Aadhaar Details
    public string MaskedAadhaar { get; set; } = string.Empty;
    public string AadhaarVerificationStatus { get; set; } = "PENDING";
    public bool AadhaarConsentGiven { get; set; }
    public DateTime? AadhaarVerifiedAt { get; set; }

    // 3. Family & Household Details
    public string FatherGuardianName { get; set; } = string.Empty;
    public string MotherName { get; set; } = string.Empty;
    public bool IsOrphan { get; set; }
    public bool IsMotherSingleWoman { get; set; }
    public string? FatherOccupation { get; set; }
    public string? MotherOccupation { get; set; }
    public bool IsDifferentlyAbled { get; set; }
    public bool ParentsIlliterate { get; set; }
    public string? HouseholdCategory { get; set; }
    public decimal AnnualIncome { get; set; }

    // 4. Contact Details
    public string MaskedMobile { get; set; } = string.Empty;
    public string MaskedEmail { get; set; } = string.Empty;
    public bool IsContactVerified { get; set; }

    // 5. Permanent & Correspondence Address
    public string? PermanentAddressLine { get; set; }
    public string? PermanentPincode { get; set; }
    public string? PermanentDistrict { get; set; }
    public string? PermanentBlock { get; set; }
    public string? PermanentCityVillage { get; set; }
    public string? PermanentVidhansabha { get; set; }
    public string? PermanentState { get; set; }

    public string? CorrespondenceAddressLine { get; set; }
    public string? CorrespondencePincode { get; set; }
    public string? CorrespondenceDistrict { get; set; }
    public string? CorrespondenceState { get; set; }

    // 6. Current Academic Record
    public ulong? AcademicRecordId { get; set; }
    public string? InstituteCode { get; set; }
    public string? InstituteName { get; set; }
    public string? CourseName { get; set; }
    public string? BranchName { get; set; }
    public string? EnrollmentNumber { get; set; }
    public DateTime? EnrollmentDate { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public string? AdmissionTypeName { get; set; }
    public string? StudyModeName { get; set; }
    public int CourseYear { get; set; } = 1;
    public bool IsHosteller { get; set; }

    // 7. Bank Account
    public string? BankName { get; set; }
    public string? BankBranchName { get; set; }
    public string? IFSCCode { get; set; }
    public string? MaskedAccountNumber { get; set; }
    public bool IsAadhaarSeeded { get; set; }
    public string? BankVerificationStatus { get; set; }

    // 8. Application Summary
    public ulong? ApplicationId { get; set; }
    public string? ApplicationNumber { get; set; }
    public string? AcademicYearCode { get; set; }
    public string? SchemeName { get; set; }
    public string? ApplicationStatusCode { get; set; }
    public string? ApplicationStatusName { get; set; }
    public int CurrentStep { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockedAt { get; set; }
    public DateTime? ApplicationCreatedAt { get; set; }

    // 9. Documents
    public List<AdminStudentDocumentDto> Documents { get; set; } = new();

    // 10. Certificates
    public List<AdminStudentCertificateDto> Certificates { get; set; } = new();

    // 11. Verification Trail
    public List<AdminStudentVerificationDto> Verifications { get; set; } = new();
}

public class AdminStudentDocumentDto
{
    public ulong DocumentId { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public bool IsVerified { get; set; }
}

public class AdminStudentCertificateDto
{
    public ulong CertificateId { get; set; }
    public string CertificateTypeName { get; set; } = string.Empty;
    public string VerificationStatus { get; set; } = string.Empty;
    public DateTime? IssueDate { get; set; }
    public ulong? DocumentId { get; set; }
}

public class AdminStudentVerificationDto
{
    public ulong VerificationId { get; set; }
    public string VerificationLevel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string? VerifiedByUserName { get; set; }
    public DateTime VerifiedAt { get; set; }
}

public class AdminDashboardStatsDto
{
    public int TotalStudents { get; set; }
    public int TotalApplications { get; set; }
    public int PendingInstituteVerification { get; set; }
    public int PendingDistrictVerification { get; set; }
    public int ApprovedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public int DisbursedApplications { get; set; }
    public int TotalDistricts { get; set; }
    public int TotalInstitutes { get; set; }
}
