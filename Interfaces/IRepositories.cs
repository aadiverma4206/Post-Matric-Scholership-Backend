using Scholarship.Api.Models;

namespace Scholarship.Api.Interfaces;

public interface IStudentRepository
{
    Task<ulong> CreateStudentAsync(Student student);
    Task<Student?> GetByIdAsync(ulong studentId);
    Task<Student?> GetByCodeAsync(string studentCode);
    Task<bool> UpdateStudentAsync(Student student);
}

public interface IStudentAuthRepository
{
    Task<ulong> CreateCredentialAsync(StudentCredential credential);
    Task<StudentCredential?> GetByUsernameAsync(string username);
    Task<StudentCredential?> GetByStudentIdAsync(ulong studentId);
    Task UpdatePasswordAsync(ulong studentId, byte[] newPasswordHash);
    Task RecordLoginAttemptAsync(ulong credentialId, bool isSuccess);
    Task<User?> GetOfficialUserByUsernameAsync(string username);
    Task<List<string>> GetUserRolesAsync(ulong userId);
    Task<InstituteUser?> GetInstituteUserByUserIdAsync(ulong userId);
}

public interface IStudentVaultRepository
{
    Task SaveAadhaarAsync(StudentAadhaar aadhaar);
    Task<StudentAadhaar?> GetAadhaarByStudentIdAsync(ulong studentId);
    Task<bool> ExistsAadhaarHashAsync(byte[] aadhaarHash);

    Task SaveContactAsync(StudentContact contact);
    Task<StudentContact?> GetContactByStudentIdAsync(ulong studentId);
    Task<bool> ExistsMobileHashAsync(byte[] mobileHash);
    Task<bool> ExistsEmailHashAsync(byte[] emailHash);
    Task<ulong?> GetStudentIdByContactHashAsync(byte[] contactHash);

    Task SaveFamilyDetailsAsync(StudentFamilyDetail family);
    Task<StudentFamilyDetail?> GetFamilyDetailsByStudentIdAsync(ulong studentId);

    Task SaveHouseholdDetailsAsync(HouseholdDetail household);
    Task<HouseholdDetail?> GetHouseholdDetailsByStudentIdAsync(ulong studentId);

    Task SaveDeprivationCriteriaAsync(ulong studentId, IEnumerable<uint> criterionIds);
    Task<List<uint>> GetDeprivationCriteriaByStudentIdAsync(ulong studentId);

    Task SaveOtrAsync(StudentOtr otr);
    Task<StudentOtr?> GetOtrByStudentIdAsync(ulong studentId);
}

public interface IAddressRepository
{
    Task<ulong> SaveAddressAsync(Address address);
    Task<List<Address>> GetAddressesByStudentIdAsync(ulong studentId);
    Task<Address?> GetAddressByTypeAsync(ulong studentId, string addressType);
}

public interface IAcademicRepository
{
    Task Save10thDetailsAsync(Student10thDetail detail);
    Task<Student10thDetail?> Get10thDetailsByStudentIdAsync(ulong studentId);

    Task SavePreviousEducationAsync(PreviousEducation education);
    Task<PreviousEducation?> GetPreviousEducationByStudentIdAsync(ulong studentId);

    Task<ulong> SaveAcademicRecordAsync(StudentAcademicRecord record);
    Task<StudentAcademicRecord?> GetAcademicRecordByStudentIdAsync(ulong studentId);
}

public interface IBankRepository
{
    Task<ulong> SaveBankAccountAsync(StudentBankAccount account);
    Task<StudentBankAccount?> GetActiveBankAccountByStudentIdAsync(ulong studentId);
}

public interface ICertificateRepository
{
    Task<ulong> SaveCertificateAsync(StudentCertificate certificate);
    Task<List<StudentCertificate>> GetCertificatesByStudentIdAsync(ulong studentId);
    Task<StudentCertificate?> GetCertificateByTypeAsync(ulong studentId, uint certificateTypeId);
}

public interface IDocumentRepository
{
    Task<ulong> SaveDocumentAsync(Document document);
    Task<Document?> GetDocumentByIdAsync(ulong documentId);
    Task<List<Document>> GetDocumentsByStudentIdAsync(ulong studentId);
    Task UpdateEntityDocumentLinkAsync(ulong studentId, uint documentTypeId, ulong documentId);
}

public interface IScholarshipApplicationRepository
{
    Task<ulong> CreateApplicationAsync(ScholarshipApplication application);
    Task<ScholarshipApplication?> GetByIdAsync(ulong applicationId);
    Task<ScholarshipApplication?> GetCurrentByStudentIdAsync(ulong studentId, uint academicYearId);
    Task<bool> UpdateStatusAndStepAsync(ulong applicationId, uint statusId, int currentStep);
    Task<bool> LockApplicationAsync(ulong applicationId, uint lockedStatusId, ulong performedBy, ulong? otpTransactionId, string? reason);
    Task<List<ScholarshipApplication>> GetApplicationsForInstituteAsync(ulong instituteId, int? statusId, int skip, int take);
    Task<int> GetApplicationsCountForInstituteAsync(ulong instituteId, int? statusId);
    Task<List<ScholarshipApplication>> GetApplicationsForDistrictAsync(ulong districtId, int? statusId, int skip, int take);
    Task<int> GetApplicationsCountForDistrictAsync(ulong districtId, int? statusId);
    Task<DTOs.DashboardStatsDto> GetDashboardStatsAsync();
    Task<DTOs.ApplicationSummaryDto?> GetApplicationSummaryDetailedAsync(ulong studentId, uint academicYearId);
}

public interface IVerificationRepository
{
    Task<ulong> RecordVerificationAsync(ApplicationVerification verification);
    Task<List<ApplicationVerification>> GetVerificationsByApplicationIdAsync(ulong applicationId);
}

public interface IOtpRepository
{
    Task<ulong> CreateOtpAsync(OtpTransaction otp);
    Task<OtpTransaction?> GetLatestActiveOtpAsync(ulong studentId, string purpose, ulong? applicationId);
    Task IncrementAttemptCountAsync(ulong otpTransactionId);
    Task MarkOtpUsedAsync(ulong otpTransactionId);
}

public interface IMasterDataRepository
{
    Task<List<AcademicYear>> GetAcademicYearsAsync();
    Task<List<ScholarshipScheme>> GetSchemesAsync(uint? academicYearId);
    Task<List<Category>> GetCategoriesAsync();
    Task<List<Gender>> GetGendersAsync();
    Task<List<Religion>> GetReligionsAsync();
    Task<List<Occupation>> GetOccupationsAsync();
    Task<List<HouseholdCategory>> GetHouseholdCategoriesAsync();
    Task<List<DeprivationCriterion>> GetDeprivationCriteriaAsync();
    Task<List<AdmissionType>> GetAdmissionTypesAsync();
    Task<List<StudyMode>> GetStudyModesAsync();
    Task<List<EducationBoard>> GetEducationBoardsAsync();
    Task<List<DocumentType>> GetDocumentTypesAsync();
    Task<List<ApplicationStatus>> GetApplicationStatusesAsync();
    Task<List<State>> GetStatesAsync();
    Task<List<District>> GetDistrictsAsync(ulong stateId);
    Task<List<Block>> GetBlocksAsync(ulong districtId);
    Task<List<Vidhansabha>> GetVidhansabhasAsync(ulong districtId);
    Task<List<CityVillage>> GetCitiesVillagesAsync(ulong districtId, ulong? blockId);
    Task<List<PostOffice>> GetPostOfficesAsync(ulong districtId);
    Task<List<Bank>> GetBanksAsync();
    Task<List<BankBranch>> GetBankBranchesAsync(ulong bankId);
    Task<BankBranch?> GetBranchByIfscAsync(string ifsc);
    Task<List<CourseType>> GetCourseTypesAsync();
    Task<List<Course>> GetCoursesAsync(uint? courseTypeId);
    Task<List<CourseBranch>> GetCourseBranchesAsync(ulong courseId);
    Task<List<Institute>> GetInstitutesAsync(ulong districtId);
    Task<List<InstituteCourse>> GetInstituteCoursesAsync(ulong instituteId);
}

public interface IAuditLogRepository
{
    Task LogAuditAsync(AuditLog log);
    Task LogLoginHistoryAsync(LoginHistory log);
    Task LogSecurityEventAsync(SecurityEvent securityEvent);
}
