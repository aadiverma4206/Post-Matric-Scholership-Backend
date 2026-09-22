using Dapper;
using Scholarship.Api.Data;
using Scholarship.Api.Interfaces;
using Scholarship.Api.Models;

namespace Scholarship.Api.Repositories;

public class ScholarshipApplicationRepository : IScholarshipApplicationRepository
{
    private readonly IDbConnectionFactory _db;

    public ScholarshipApplicationRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<ulong> CreateApplicationAsync(ScholarshipApplication application)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            INSERT INTO scholarship_applications (ApplicationNumber, StudentId, AcademicYearId, SchemeId, AcademicRecordId, OtrId, ApplicationStatusId, CurrentStep, IsLocked, CreatedAt, UpdatedAt)
            VALUES (@ApplicationNumber, @StudentId, @AcademicYearId, @SchemeId, @AcademicRecordId, @OtrId, @ApplicationStatusId, @CurrentStep, 0, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<ulong>(sql, application);
    }

    public async Task<ScholarshipApplication?> GetByIdAsync(ulong applicationId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT ApplicationId, ApplicationNumber, StudentId, AcademicYearId, SchemeId, AcademicRecordId, OtrId, ApplicationStatusId, CurrentStep, IsLocked, LockedAt, CreatedAt, UpdatedAt
            FROM scholarship_applications
            WHERE ApplicationId = @ApplicationId;";

        return await conn.QuerySingleOrDefaultAsync<ScholarshipApplication>(sql, new { ApplicationId = applicationId });
    }

    public async Task<ScholarshipApplication?> GetCurrentByStudentIdAsync(ulong studentId, uint academicYearId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT ApplicationId, ApplicationNumber, StudentId, AcademicYearId, SchemeId, AcademicRecordId, OtrId, ApplicationStatusId, CurrentStep, IsLocked, LockedAt, CreatedAt, UpdatedAt
            FROM scholarship_applications
            WHERE StudentId = @StudentId AND AcademicYearId = @AcademicYearId
            ORDER BY ApplicationId DESC LIMIT 1;";

        return await conn.QuerySingleOrDefaultAsync<ScholarshipApplication>(sql, new { StudentId = studentId, AcademicYearId = academicYearId });
    }

    public async Task<bool> UpdateStatusAndStepAsync(ulong applicationId, uint statusId, int currentStep)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            UPDATE scholarship_applications
            SET ApplicationStatusId = @StatusId, CurrentStep = @CurrentStep, UpdatedAt = CURRENT_TIMESTAMP
            WHERE ApplicationId = @ApplicationId;";

        int rows = await conn.ExecuteAsync(sql, new { ApplicationId = applicationId, StatusId = statusId, CurrentStep = currentStep });
        return rows > 0;
    }

    public async Task<bool> LockApplicationAsync(ulong applicationId, uint lockedStatusId, ulong performedBy, ulong? otpTransactionId, string? reason)
    {
        using var conn = await _db.CreateConnectionAsync();
        using var tx = conn.BeginTransaction();
        try
        {
            const string updateSql = @"
                UPDATE scholarship_applications
                SET IsLocked = 1, ApplicationStatusId = @LockedStatusId, LockedAt = CURRENT_TIMESTAMP, UpdatedAt = CURRENT_TIMESTAMP
                WHERE ApplicationId = @ApplicationId AND IsLocked = 0;";

            int updated = await conn.ExecuteAsync(updateSql, new { ApplicationId = applicationId, LockedStatusId = lockedStatusId }, tx);
            if (updated == 0)
            {
                tx.Rollback();
                return false;
            }

            const string historySql = @"
                INSERT INTO application_lock_history (ApplicationId, Action, Reason, PerformedBy, OtpTransactionId, PerformedAt)
                VALUES (@ApplicationId, 'LOCKED', @Reason, @PerformedBy, @OtpTransactionId, CURRENT_TIMESTAMP);";

            await conn.ExecuteAsync(historySql, new
            {
                ApplicationId = applicationId,
                Reason = reason ?? "Student locked application via OTP verification",
                PerformedBy = performedBy,
                OtpTransactionId = otpTransactionId
            }, tx);

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task<List<ScholarshipApplication>> GetApplicationsForInstituteAsync(ulong instituteId, int? statusId, int skip, int take)
    {
        using var conn = await _db.CreateConnectionAsync();
        string sql = @"
            SELECT sa.ApplicationId, sa.ApplicationNumber, sa.StudentId, sa.AcademicYearId, sa.SchemeId, sa.AcademicRecordId, sa.OtrId, sa.ApplicationStatusId, sa.CurrentStep, sa.IsLocked, sa.LockedAt, sa.CreatedAt, sa.UpdatedAt
            FROM scholarship_applications sa
            INNER JOIN student_academic_records sar ON sa.AcademicRecordId = sar.AcademicRecordId
            INNER JOIN institute_courses ic ON sar.InstituteCourseId = ic.InstituteCourseId
            WHERE ic.InstituteId = @InstituteId
            AND (@StatusId IS NULL OR sa.ApplicationStatusId = @StatusId)
            ORDER BY sa.ApplicationId DESC
            LIMIT @Take OFFSET @Skip;";

        var list = await conn.QueryAsync<ScholarshipApplication>(sql, new { InstituteId = instituteId, StatusId = statusId, Skip = skip, Take = take });
        return list.AsList();
    }

    public async Task<int> GetApplicationsCountForInstituteAsync(ulong instituteId, int? statusId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT COUNT(1)
            FROM scholarship_applications sa
            INNER JOIN student_academic_records sar ON sa.AcademicRecordId = sar.AcademicRecordId
            INNER JOIN institute_courses ic ON sar.InstituteCourseId = ic.InstituteCourseId
            WHERE ic.InstituteId = @InstituteId
            AND (@StatusId IS NULL OR sa.ApplicationStatusId = @StatusId);";

        return await conn.ExecuteScalarAsync<int>(sql, new { InstituteId = instituteId, StatusId = statusId });
    }

    public async Task<List<ScholarshipApplication>> GetApplicationsForDistrictAsync(ulong districtId, int? statusId, int skip, int take)
    {
        using var conn = await _db.CreateConnectionAsync();
        string sql = @"
            SELECT sa.ApplicationId, sa.ApplicationNumber, sa.StudentId, sa.AcademicYearId, sa.SchemeId, sa.AcademicRecordId, sa.OtrId, sa.ApplicationStatusId, sa.CurrentStep, sa.IsLocked, sa.LockedAt, sa.CreatedAt, sa.UpdatedAt
            FROM scholarship_applications sa
            INNER JOIN student_academic_records sar ON sa.AcademicRecordId = sar.AcademicRecordId
            INNER JOIN institute_courses ic ON sar.InstituteCourseId = ic.InstituteCourseId
            INNER JOIN institutes inst ON ic.InstituteId = inst.InstituteId
            WHERE inst.DistrictId = @DistrictId
            AND (@StatusId IS NULL OR sa.ApplicationStatusId = @StatusId)
            ORDER BY sa.ApplicationId DESC
            LIMIT @Take OFFSET @Skip;";

        var list = await conn.QueryAsync<ScholarshipApplication>(sql, new { DistrictId = districtId, StatusId = statusId, Skip = skip, Take = take });
        return list.AsList();
    }

    public async Task<int> GetApplicationsCountForDistrictAsync(ulong districtId, int? statusId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT COUNT(1)
            FROM scholarship_applications sa
            INNER JOIN student_academic_records sar ON sa.AcademicRecordId = sar.AcademicRecordId
            INNER JOIN institute_courses ic ON sar.InstituteCourseId = ic.InstituteCourseId
            INNER JOIN institutes inst ON ic.InstituteId = inst.InstituteId
            WHERE inst.DistrictId = @DistrictId
            AND (@StatusId IS NULL OR sa.ApplicationStatusId = @StatusId);";

        return await conn.ExecuteScalarAsync<int>(sql, new { DistrictId = districtId, StatusId = statusId });
    }

    public async Task<DTOs.DashboardStatsDto> GetDashboardStatsAsync()
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT
                COUNT(1) AS TotalApplications,
                COALESCE(SUM(CASE WHEN ApplicationStatusId = 1 THEN 1 ELSE 0 END), 0) AS Draft,
                COALESCE(SUM(CASE WHEN ApplicationStatusId = 2 THEN 1 ELSE 0 END), 0) AS Submitted,
                COALESCE(SUM(CASE WHEN ApplicationStatusId = 4 THEN 1 ELSE 0 END), 0) AS Locked,
                COALESCE(SUM(CASE WHEN ApplicationStatusId IN (4, 5) THEN 1 ELSE 0 END), 0) AS PendingInstitute,
                COALESCE(SUM(CASE WHEN ApplicationStatusId = 6 THEN 1 ELSE 0 END), 0) AS InstituteApproved,
                COALESCE(SUM(CASE WHEN ApplicationStatusId = 8 THEN 1 ELSE 0 END), 0) AS InstituteRejected,
                COALESCE(SUM(CASE WHEN ApplicationStatusId = 6 THEN 1 ELSE 0 END), 0) AS PendingDistrict,
                COALESCE(SUM(CASE WHEN ApplicationStatusId = 7 THEN 1 ELSE 0 END), 0) AS Approved,
                COALESCE(SUM(CASE WHEN ApplicationStatusId = 8 THEN 1 ELSE 0 END), 0) AS Rejected,
                COALESCE(SUM(CASE WHEN ApplicationStatusId = 9 THEN 1 ELSE 0 END), 0) AS Reverted,
                COALESCE(SUM(CASE WHEN ApplicationStatusId = 12 THEN 1 ELSE 0 END), 0) AS Disbursed
            FROM scholarship_applications;";

        var stats = await conn.QuerySingleOrDefaultAsync<DTOs.DashboardStatsDto>(sql);
        return stats ?? new DTOs.DashboardStatsDto();
    }

    public async Task<DTOs.ApplicationSummaryDto?> GetApplicationSummaryDetailedAsync(ulong studentId, uint academicYearId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT 
                sa.ApplicationId,
                sa.ApplicationNumber,
                sa.StudentId,
                st.StudentCode,
                CONCAT(st.FirstName, ' ', COALESCE(st.LastName, '')) AS StudentName,
                sa.AcademicYearId,
                ay.AcademicYearCode AS AcademicYearCode,
                sa.SchemeId,
                sc.SchemeName,
                sa.ApplicationStatusId,
                ast.Code AS StatusCode,
                ast.Name AS StatusName,
                sa.CurrentStep,
                sa.IsLocked,
                sa.LockedAt,
                sa.CreatedAt,
                inst.InstituteName,
                c.CourseName
            FROM scholarship_applications sa
            INNER JOIN students st ON sa.StudentId = st.StudentId
            LEFT JOIN academic_years ay ON sa.AcademicYearId = ay.AcademicYearId
            LEFT JOIN scholarship_schemes sc ON sa.SchemeId = sc.SchemeId
            LEFT JOIN application_statuses ast ON sa.ApplicationStatusId = ast.ApplicationStatusId
            LEFT JOIN student_academic_records sar ON sa.AcademicRecordId = sar.AcademicRecordId
            LEFT JOIN institute_courses ic ON sar.InstituteCourseId = ic.InstituteCourseId
            LEFT JOIN institutes inst ON ic.InstituteId = inst.InstituteId
            LEFT JOIN courses c ON ic.CourseId = c.CourseId
            WHERE sa.StudentId = @StudentId AND (@AcademicYearId = 0 OR sa.AcademicYearId = @AcademicYearId)
            ORDER BY sa.ApplicationId DESC LIMIT 1;";

        return await conn.QuerySingleOrDefaultAsync<DTOs.ApplicationSummaryDto>(sql, new { StudentId = studentId, AcademicYearId = academicYearId });
    }
}

public class VerificationRepository : IVerificationRepository
{
    private readonly IDbConnectionFactory _db;

    public VerificationRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<ulong> RecordVerificationAsync(ApplicationVerification verification)
    {
        using var conn = await _db.CreateConnectionAsync();
        using var tx = conn.BeginTransaction();
        try
        {
            const string insertSql = @"
                INSERT INTO application_verifications (ApplicationId, VerificationLevel, VerifiedBy, Status, Remarks, VerifiedAt)
                VALUES (@ApplicationId, @VerificationLevel, @VerifiedBy, @Status, @Remarks, CURRENT_TIMESTAMP);
                SELECT LAST_INSERT_ID();";

            ulong id = await conn.ExecuteScalarAsync<ulong>(insertSql, verification, tx);

            // Update scholarship_applications status accordingly
            uint nextStatusId;
            if (verification.Status == "APPROVED")
            {
                nextStatusId = verification.VerificationLevel == "INSTITUTE" ? 6u : 7u; // District or Approved
            }
            else if (verification.Status == "TEMPORARY_REJECTED")
            {
                nextStatusId = 9u; // Revert / Sent Back
            }
            else
            {
                nextStatusId = 8u; // Rejected
            }

            const string updateAppSql = @"
                UPDATE scholarship_applications
                SET ApplicationStatusId = @StatusId,
                    IsLocked = CASE WHEN @StatusId = 9 THEN 0 ELSE IsLocked END,
                    UpdatedAt = CURRENT_TIMESTAMP
                WHERE ApplicationId = @ApplicationId;";

            await conn.ExecuteAsync(updateAppSql, new { StatusId = nextStatusId, verification.ApplicationId }, tx);

            tx.Commit();
            return id;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task<List<ApplicationVerification>> GetVerificationsByApplicationIdAsync(ulong applicationId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT VerificationId, ApplicationId, VerificationLevel, VerifiedBy, Status, Remarks, VerifiedAt
            FROM application_verifications
            WHERE ApplicationId = @ApplicationId
            ORDER BY VerificationId DESC;";

        var list = await conn.QueryAsync<ApplicationVerification>(sql, new { ApplicationId = applicationId });
        return list.AsList();
    }
}

public class OtpRepository : IOtpRepository
{
    private readonly IDbConnectionFactory _db;

    public OtpRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<ulong> CreateOtpAsync(OtpTransaction otp)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            INSERT INTO otp_transactions (StudentId, ApplicationId, Purpose, OtpHash, ExpiresAt, AttemptCount, MaxAttempts, IsUsed, CreatedAt)
            VALUES (@StudentId, @ApplicationId, @Purpose, @OtpHash, @ExpiresAt, 0, @MaxAttempts, 0, CURRENT_TIMESTAMP);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<ulong>(sql, otp);
    }

    public async Task<OtpTransaction?> GetLatestActiveOtpAsync(ulong studentId, string purpose, ulong? applicationId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT OtpTransactionId, StudentId, ApplicationId, Purpose, OtpHash, ExpiresAt, AttemptCount, MaxAttempts, VerifiedAt, IsUsed, CreatedAt
            FROM otp_transactions
            WHERE StudentId = @StudentId AND Purpose = @Purpose 
              AND (@ApplicationId IS NULL OR ApplicationId = @ApplicationId)
              AND IsUsed = 0 AND ExpiresAt > CURRENT_TIMESTAMP
            ORDER BY OtpTransactionId DESC LIMIT 1;";

        return await conn.QuerySingleOrDefaultAsync<OtpTransaction>(sql, new { StudentId = studentId, Purpose = purpose, ApplicationId = applicationId });
    }

    public async Task IncrementAttemptCountAsync(ulong otpTransactionId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            UPDATE otp_transactions
            SET AttemptCount = AttemptCount + 1
            WHERE OtpTransactionId = @OtpTransactionId;";

        await conn.ExecuteAsync(sql, new { OtpTransactionId = otpTransactionId });
    }

    public async Task MarkOtpUsedAsync(ulong otpTransactionId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            UPDATE otp_transactions
            SET IsUsed = 1, VerifiedAt = CURRENT_TIMESTAMP
            WHERE OtpTransactionId = @OtpTransactionId;";

        await conn.ExecuteAsync(sql, new { OtpTransactionId = otpTransactionId });
    }
}

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IDbConnectionFactory _db;

    public AuditLogRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task LogAuditAsync(AuditLog log)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            INSERT INTO audit_logs (UserId, StudentId, Action, EntityName, EntityId, OldValueEncrypted, NewValueEncrypted, IPAddress, UserAgent, RequestId, CreatedAt)
            VALUES (@UserId, @StudentId, @Action, @EntityName, @EntityId, @OldValueEncrypted, @NewValueEncrypted, @IPAddress, @UserAgent, @RequestId, CURRENT_TIMESTAMP);";

        await conn.ExecuteAsync(sql, log);
    }

    public async Task LogLoginHistoryAsync(LoginHistory log)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            INSERT INTO login_history (UserId, StudentId, LoginTime, IPAddress, UserAgent, LoginStatus, FailureReason)
            VALUES (@UserId, @StudentId, CURRENT_TIMESTAMP, @IPAddress, @UserAgent, @LoginStatus, @FailureReason);";

        await conn.ExecuteAsync(sql, log);
    }

    public async Task LogSecurityEventAsync(SecurityEvent securityEvent)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            INSERT INTO security_events (UserId, StudentId, EventType, Severity, IPAddress, Description, CreatedAt)
            VALUES (@UserId, @StudentId, @EventType, @Severity, @IPAddress, @Description, CURRENT_TIMESTAMP);";

        await conn.ExecuteAsync(sql, securityEvent);
    }
}

public class MasterDataRepository : IMasterDataRepository
{
    private readonly IDbConnectionFactory _db;

    public MasterDataRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<List<AcademicYear>> GetAcademicYearsAsync()
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT AcademicYearId, AcademicYearCode, StartDate, EndDate, IsCurrent, IsActive, CreatedAt FROM academic_years WHERE IsActive = 1 ORDER BY StartDate DESC;";
        var list = await conn.QueryAsync<AcademicYear>(sql);
        return list.AsList();
    }

    public async Task<List<ScholarshipScheme>> GetSchemesAsync(uint? academicYearId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT SchemeId, SchemeCode, SchemeName, Description, CategoryId, IsActive, CreatedAt FROM scholarship_schemes WHERE IsActive = 1;";
        var list = await conn.QueryAsync<ScholarshipScheme>(sql);
        return list.AsList();
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT CategoryId, Code, Name, IsActive FROM categories WHERE IsActive = 1;";
        var list = await conn.QueryAsync<Category>(sql);
        return list.AsList();
    }

    public async Task<List<Gender>> GetGendersAsync()
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT GenderId, Code, Name, IsActive FROM genders WHERE IsActive = 1;";
        var list = await conn.QueryAsync<Gender>(sql);
        return list.AsList();
    }

    public async Task<List<Religion>> GetReligionsAsync()
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT ReligionId, Code, Name, IsActive FROM religions WHERE IsActive = 1;";
        var list = await conn.QueryAsync<Religion>(sql);
        return list.AsList();
    }

    public async Task<List<Occupation>> GetOccupationsAsync()
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT OccupationId, Code, Name, IsActive FROM occupations WHERE IsActive = 1;";
        var list = await conn.QueryAsync<Occupation>(sql);
        return list.AsList();
    }

    public async Task<List<HouseholdCategory>> GetHouseholdCategoriesAsync()
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT HouseholdCategoryId, Code, Name, IsActive FROM household_categories WHERE IsActive = 1;";
        var list = await conn.QueryAsync<HouseholdCategory>(sql);
        return list.AsList();
    }

    public async Task<List<DeprivationCriterion>> GetDeprivationCriteriaAsync()
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT CriterionId, Code, Name, IsActive FROM deprivation_criteria WHERE IsActive = 1;";
        var list = await conn.QueryAsync<DeprivationCriterion>(sql);
        return list.AsList();
    }

    public async Task<List<AdmissionType>> GetAdmissionTypesAsync()
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT AdmissionTypeId, Code, Name, IsActive FROM admission_types WHERE IsActive = 1;";
        var list = await conn.QueryAsync<AdmissionType>(sql);
        return list.AsList();
    }

    public async Task<List<StudyMode>> GetStudyModesAsync()
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT StudyModeId, Code, Name, IsActive FROM study_modes WHERE IsActive = 1;";
        var list = await conn.QueryAsync<StudyMode>(sql);
        return list.AsList();
    }

    public async Task<List<EducationBoard>> GetEducationBoardsAsync()
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT BoardId, Code, Name, IsActive FROM education_boards WHERE IsActive = 1;";
        var list = await conn.QueryAsync<EducationBoard>(sql);
        return list.AsList();
    }

    public async Task<List<DocumentType>> GetDocumentTypesAsync()
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT DocumentTypeId, Code, Name, MaxSizeBytes, AllowedMimeTypes, IsActive FROM document_types WHERE IsActive = 1;";
        var list = await conn.QueryAsync<DocumentType>(sql);
        return list.AsList();
    }

    public async Task<List<ApplicationStatus>> GetApplicationStatusesAsync()
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT ApplicationStatusId, Code, Name, Description, IsActive FROM application_statuses WHERE IsActive = 1;";
        var list = await conn.QueryAsync<ApplicationStatus>(sql);
        return list.AsList();
    }

    public async Task<List<State>> GetStatesAsync()
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT StateId, StateCode, StateName, IsActive, CreatedAt FROM states WHERE IsActive = 1;";
        var list = await conn.QueryAsync<State>(sql);
        return list.AsList();
    }

    public async Task<List<District>> GetDistrictsAsync(ulong stateId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT DistrictId, StateId, DistrictCode, DistrictName, IsActive, CreatedAt FROM districts WHERE StateId = @StateId AND IsActive = 1;";
        var list = await conn.QueryAsync<District>(sql, new { StateId = stateId });
        return list.AsList();
    }

    public async Task<List<Block>> GetBlocksAsync(ulong districtId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT BlockId, DistrictId, BlockCode, BlockName, IsActive, CreatedAt FROM blocks WHERE DistrictId = @DistrictId AND IsActive = 1;";
        var list = await conn.QueryAsync<Block>(sql, new { DistrictId = districtId });
        return list.AsList();
    }

    public async Task<List<Vidhansabha>> GetVidhansabhasAsync(ulong districtId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT VidhansabhaId, StateId, DistrictId, VidhansabhaCode, VidhansabhaName, IsActive, CreatedAt FROM vidhansabhas WHERE DistrictId = @DistrictId AND IsActive = 1;";
        var list = await conn.QueryAsync<Vidhansabha>(sql, new { DistrictId = districtId });
        return list.AsList();
    }

    public async Task<List<CityVillage>> GetCitiesVillagesAsync(ulong districtId, ulong? blockId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT CityVillageId, DistrictId, BlockId, Name, Type, IsActive, CreatedAt FROM cities_villages WHERE DistrictId = @DistrictId AND (@BlockId IS NULL OR BlockId = @BlockId) AND IsActive = 1;";
        var list = await conn.QueryAsync<CityVillage>(sql, new { DistrictId = districtId, BlockId = blockId });
        return list.AsList();
    }

    public async Task<List<PostOffice>> GetPostOfficesAsync(ulong districtId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT PostOfficeId, DistrictId, PostOfficeCode, PostOfficeName, Pincode, IsActive, CreatedAt FROM post_offices WHERE DistrictId = @DistrictId AND IsActive = 1;";
        var list = await conn.QueryAsync<PostOffice>(sql, new { DistrictId = districtId });
        return list.AsList();
    }

    public async Task<List<Bank>> GetBanksAsync()
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT BankId, BankCode, BankName, IsActive, CreatedAt FROM banks WHERE IsActive = 1;";
        var list = await conn.QueryAsync<Bank>(sql);
        return list.AsList();
    }

    public async Task<List<BankBranch>> GetBankBranchesAsync(ulong bankId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT BranchId, BankId, IFSCCode, BranchName, Address, IsActive, CreatedAt FROM bank_branches WHERE BankId = @BankId AND IsActive = 1;";
        var list = await conn.QueryAsync<BankBranch>(sql, new { BankId = bankId });
        return list.AsList();
    }

    public async Task<BankBranch?> GetBranchByIfscAsync(string ifsc)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT BranchId, BankId, IFSCCode, BranchName, Address, IsActive, CreatedAt FROM bank_branches WHERE UPPER(IFSCCode) = UPPER(@IFSCCode) AND IsActive = 1 LIMIT 1;";
        return await conn.QueryFirstOrDefaultAsync<BankBranch>(sql, new { IFSCCode = ifsc?.Trim() });
    }

    public async Task<List<CourseType>> GetCourseTypesAsync()
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT CourseTypeId, Code, Name, IsActive FROM course_types WHERE IsActive = 1;";
        var list = await conn.QueryAsync<CourseType>(sql);
        return list.AsList();
    }

    public async Task<List<Course>> GetCoursesAsync(uint? courseTypeId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT CourseId, CourseCode, CourseName, DurationYears, CourseTypeId, IsActive, CreatedAt FROM courses WHERE (@CourseTypeId IS NULL OR CourseTypeId = @CourseTypeId) AND IsActive = 1;";
        var list = await conn.QueryAsync<Course>(sql, new { CourseTypeId = courseTypeId });
        return list.AsList();
    }

    public async Task<List<CourseBranch>> GetCourseBranchesAsync(ulong courseId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT BranchId, CourseId, BranchCode, BranchName, IsActive, CreatedAt FROM course_branches WHERE CourseId = @CourseId AND IsActive = 1;";
        var list = await conn.QueryAsync<CourseBranch>(sql, new { CourseId = courseId });
        return list.AsList();
    }

    public async Task<List<Institute>> GetInstitutesAsync(ulong districtId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT InstituteId, InstituteCode, InstituteName, InstituteTypeId, StateId, DistrictId, Address, IsGovernment, IsActive, CreatedAt FROM institutes WHERE DistrictId = @DistrictId AND IsActive = 1;";
        var list = await conn.QueryAsync<Institute>(sql, new { DistrictId = districtId });
        return list.AsList();
    }

    public async Task<List<InstituteCourse>> GetInstituteCoursesAsync(ulong instituteId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT InstituteCourseId, InstituteId, CourseId, BranchId, CourseCode, DurationYears, IsActive, CreatedAt FROM institute_courses WHERE InstituteId = @InstituteId AND IsActive = 1;";
        var list = await conn.QueryAsync<InstituteCourse>(sql, new { InstituteId = instituteId });
        return list.AsList();
    }
}
