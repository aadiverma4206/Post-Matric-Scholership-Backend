namespace Scholarship.Api.Models;

public class Student
{
    public ulong StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public uint GenderId { get; set; }
    public uint CategoryId { get; set; }
    public uint ReligionId { get; set; }
    public ulong? PhotoDocumentId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class StudentCredential
{
    public ulong CredentialId { get; set; }
    public ulong StudentId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public string PasswordAlgorithm { get; set; } = "Argon2id";
    public DateTime PasswordChangedAt { get; set; }
    public int FailedLoginCount { get; set; }
    public DateTime? LockedUntil { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StudentAadhaar
{
    public ulong AadhaarId { get; set; }
    public ulong StudentId { get; set; }
    public byte[] AadhaarEncrypted { get; set; } = Array.Empty<byte>();
    public byte[] AadhaarHash { get; set; } = Array.Empty<byte>();
    public string MaskedAadhaar { get; set; } = string.Empty;
    public string VerificationStatus { get; set; } = "PENDING";
    public DateTime? VerifiedAt { get; set; }
    public bool ConsentGiven { get; set; } = true;
    public DateTime ConsentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StudentContact
{
    public ulong ContactId { get; set; }
    public ulong StudentId { get; set; }
    public byte[] MobileEncrypted { get; set; } = Array.Empty<byte>();
    public byte[] MobileHash { get; set; } = Array.Empty<byte>();
    public byte[] EmailEncrypted { get; set; } = Array.Empty<byte>();
    public byte[] EmailHash { get; set; } = Array.Empty<byte>();
    public byte[]? AlternateMobileEncrypted { get; set; }
    public byte[]? AlternateEmailEncrypted { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StudentFamilyDetail
{
    public ulong FamilyDetailId { get; set; }
    public ulong StudentId { get; set; }
    public string FatherGuardianName { get; set; } = string.Empty;
    public string MotherName { get; set; } = string.Empty;
    public bool IsOrphan { get; set; }
    public bool IsMotherSingleWoman { get; set; }
    public uint? FatherOccupationId { get; set; }
    public uint? MotherOccupationId { get; set; }
    public bool IsDifferentlyAbled { get; set; }
    public bool ParentsIlliterate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Address
{
    public ulong AddressId { get; set; }
    public ulong StudentId { get; set; }
    public string AddressType { get; set; } = "PERMANENT"; // PERMANENT or CORRESPONDENCE
    public string AddressLine { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
    public ulong? PostOfficeId { get; set; }
    public ulong? CityVillageId { get; set; }
    public ulong DistrictId { get; set; }
    public ulong? BlockId { get; set; }
    public ulong? VidhansabhaId { get; set; }
    public ulong StateId { get; set; }
    public bool IsCurrent { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class HouseholdDetail
{
    public ulong HouseholdDetailId { get; set; }
    public ulong StudentId { get; set; }
    public uint HouseholdCategoryId { get; set; }
    public byte[]? BplNumberEncrypted { get; set; }
    public byte[]? BplNumberHash { get; set; }
    public decimal AnnualIncome { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class HouseholdDeprivation
{
    public ulong DeprivationId { get; set; }
    public ulong StudentId { get; set; }
    public uint CriterionId { get; set; }
    public bool IsApplicable { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Student10thDetail
{
    public ulong Class10Id { get; set; }
    public ulong StudentId { get; set; }
    public string RollNumber { get; set; } = string.Empty;
    public string SchoolType { get; set; } = "GOVERNMENT";
    public ushort PassingYear { get; set; }
    public uint BoardId { get; set; }
    public decimal Percentage { get; set; }
    public ulong? MarksheetDocumentId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PreviousEducation
{
    public ulong PreviousEducationId { get; set; }
    public ulong StudentId { get; set; }
    public ulong? AcademicRecordId { get; set; }
    public uint CourseTypeId { get; set; }
    public ulong CourseId { get; set; }
    public ulong? BranchId { get; set; }
    public string InstituteName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public ushort PassingYear { get; set; }
    public decimal Percentage { get; set; }
    public ulong? MarksheetDocumentId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StudentAcademicRecord
{
    public ulong AcademicRecordId { get; set; }
    public ulong StudentId { get; set; }
    public uint AcademicYearId { get; set; }
    public ulong SchemeId { get; set; }
    public ulong InstituteCourseId { get; set; }
    public DateTime AdmissionDate { get; set; }
    public string EnrollmentNumber { get; set; } = string.Empty;
    public DateTime? EnrollmentDate { get; set; }
    public uint AdmissionTypeId { get; set; }
    public uint StudyModeId { get; set; }
    public uint CourseYear { get; set; } = 1;
    public bool IsHosteller { get; set; }
    public bool IsLateralEntry { get; set; }
    public bool IsLocked { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StudentBankAccount
{
    public ulong StudentBankAccountId { get; set; }
    public ulong StudentId { get; set; }
    public ulong BankId { get; set; }
    public ulong BranchId { get; set; }
    public byte[] AccountNumberEncrypted { get; set; } = Array.Empty<byte>();
    public byte[] AccountNumberHash { get; set; } = Array.Empty<byte>();
    public string MaskedAccountNumber { get; set; } = string.Empty;
    public bool IsAadhaarSeeded { get; set; }
    public string VerificationStatus { get; set; } = "PENDING";
    public bool IsActive { get; set; } = true;
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StudentCertificate
{
    public ulong CertificateId { get; set; }
    public ulong StudentId { get; set; }
    public uint? AcademicYearId { get; set; }
    public uint CertificateTypeId { get; set; }
    public bool IsOnlineGenerated { get; set; } = true;
    public string GeneratedFrom { get; set; } = "EDISTRICT_PORTAL";
    public byte[] ReferenceNumberEncrypted { get; set; } = Array.Empty<byte>();
    public byte[] ReferenceNumberHash { get; set; } = Array.Empty<byte>();
    public DateTime? IssueDate { get; set; }
    public ulong? DocumentId { get; set; }
    public string VerificationStatus { get; set; } = "PENDING";
    public DateTime? VerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Document
{
    public ulong DocumentId { get; set; }
    public string DocumentUUID { get; set; } = Guid.NewGuid().ToString();
    public ulong? StudentId { get; set; }
    public uint DocumentTypeId { get; set; }
    public uint? AcademicYearId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public byte[] FileHash { get; set; } = Array.Empty<byte>();
    public string MimeType { get; set; } = string.Empty;
    public ulong FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
    public ulong? UploadedBy { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ScholarshipApplication
{
    public ulong ApplicationId { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public ulong StudentId { get; set; }
    public uint AcademicYearId { get; set; }
    public ulong SchemeId { get; set; }
    public ulong AcademicRecordId { get; set; }
    public ulong? OtrId { get; set; }
    public uint ApplicationStatusId { get; set; }
    public int CurrentStep { get; set; } = 1;
    public bool IsLocked { get; set; }
    public DateTime? LockedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ApplicationLockHistory
{
    public ulong LockHistoryId { get; set; }
    public ulong ApplicationId { get; set; }
    public string Action { get; set; } = "LOCKED";
    public string? Reason { get; set; }
    public ulong PerformedBy { get; set; }
    public ulong? OtpTransactionId { get; set; }
    public DateTime PerformedAt { get; set; }
}

public class ApplicationVerification
{
    public ulong VerificationId { get; set; }
    public ulong ApplicationId { get; set; }
    public string VerificationLevel { get; set; } = "INSTITUTE"; // INSTITUTE, DISTRICT, STATE
    public ulong VerifiedBy { get; set; }
    public string Status { get; set; } = "APPROVED"; // APPROVED, REJECTED, TEMPORARY_REJECTED
    public string? Remarks { get; set; }
    public DateTime VerifiedAt { get; set; }
}

public class OtpTransaction
{
    public ulong OtpTransactionId { get; set; }
    public ulong StudentId { get; set; }
    public ulong? ApplicationId { get; set; }
    public string Purpose { get; set; } = "APPLICATION_LOCK";
    public byte[] OtpHash { get; set; } = Array.Empty<byte>();
    public DateTime ExpiresAt { get; set; }
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; } = 3;
    public DateTime? VerifiedAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AuditLog
{
    public ulong AuditLogId { get; set; }
    public ulong? UserId { get; set; }
    public ulong? StudentId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public ulong? EntityId { get; set; }
    public byte[]? OldValueEncrypted { get; set; }
    public byte[]? NewValueEncrypted { get; set; }
    public string IPAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public string? RequestId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LoginHistory
{
    public ulong LoginHistoryId { get; set; }
    public ulong? UserId { get; set; }
    public ulong? StudentId { get; set; }
    public DateTime LoginTime { get; set; }
    public string IPAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public string LoginStatus { get; set; } = "SUCCESS";
    public string? FailureReason { get; set; }
}

public class SecurityEvent
{
    public ulong SecurityEventId { get; set; }
    public ulong? UserId { get; set; }
    public ulong? StudentId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Severity { get; set; } = "MEDIUM";
    public string IPAddress { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class User
{
    public ulong UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public uint UserTypeId { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Role
{
    public uint RoleId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class InstituteUser
{
    public ulong InstituteUserId { get; set; }
    public ulong UserId { get; set; }
    public ulong InstituteId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class StudentOtr
{
    public ulong OtrId { get; set; }
    public ulong StudentId { get; set; }
    public byte[] OtrNumberEncrypted { get; set; } = Array.Empty<byte>();
    public byte[] OtrNumberHash { get; set; } = Array.Empty<byte>();
    public string VerificationStatus { get; set; } = "VERIFIED";
    public DateTime VerifiedAt { get; set; } = DateTime.UtcNow;
    public string VerificationSource { get; set; } = "NSP_API";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
