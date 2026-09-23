using System.Text;
using Dapper;
using Scholarship.Api.Data;
using Scholarship.Api.DTOs;
using Scholarship.Api.Helpers;
using Scholarship.Api.Interfaces;
using Scholarship.Api.Security;

namespace Scholarship.Api.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly IDbConnectionFactory _db;
    private readonly ICryptoService _crypto;

    public AdminRepository(IDbConnectionFactory db, ICryptoService crypto)
    {
        _db = db;
        _crypto = crypto;
    }

    public async Task<PagedResult<AdminStudentListItemDto>> GetStudentsAsync(
        ulong? districtId,
        ulong? instituteId,
        string? search,
        int? statusId,
        uint? categoryId,
        int page,
        int pageSize)
    {
        using var conn = await _db.CreateConnectionAsync();

        var whereClauses = new List<string> { "s.IsActive = 1" };
        var parameters = new DynamicParameters();

        if (districtId.HasValue && districtId.Value > 0)
        {
            whereClauses.Add("a.DistrictId = @DistrictId");
            parameters.Add("DistrictId", districtId.Value);
        }

        if (instituteId.HasValue && instituteId.Value > 0)
        {
            whereClauses.Add("ic.InstituteId = @InstituteId");
            parameters.Add("InstituteId", instituteId.Value);
        }

        if (statusId.HasValue && statusId.Value > 0)
        {
            whereClauses.Add("sa.ApplicationStatusId = @StatusId");
            parameters.Add("StatusId", statusId.Value);
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            whereClauses.Add("s.CategoryId = @CategoryId");
            parameters.Add("CategoryId", categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            whereClauses.Add(@"(
                s.StudentCode LIKE @Search 
                OR s.FirstName LIKE @Search 
                OR s.LastName LIKE @Search 
                OR CONCAT(s.FirstName, ' ', IFNULL(s.LastName, '')) LIKE @Search
                OR sa.ApplicationNumber LIKE @Search
                OR a.Pincode LIKE @Search
            )");
            parameters.Add("Search", $"%{search.Trim()}%");
        }

        string whereSql = string.Join(" AND ", whereClauses);

        string countSql = $@"
            SELECT COUNT(DISTINCT s.StudentId)
            FROM students s
            LEFT JOIN addresses a ON a.StudentId = s.StudentId AND a.IsCurrent = 1 AND a.AddressType = 'PERMANENT'
            LEFT JOIN scholarship_applications sa ON sa.StudentId = s.StudentId
            LEFT JOIN student_academic_records sar ON sar.StudentId = s.StudentId
            LEFT JOIN institute_courses ic ON ic.InstituteCourseId = sar.InstituteCourseId
            WHERE {whereSql};";

        int totalCount = await conn.ExecuteScalarAsync<int>(countSql, parameters);

        int skip = Math.Max(0, (page - 1) * pageSize);
        parameters.Add("Skip", skip);
        parameters.Add("Take", pageSize);

        string dataSql = $@"
            SELECT 
                s.StudentId,
                s.StudentCode,
                s.FirstName,
                s.MiddleName,
                s.LastName,
                s.DateOfBirth,
                s.GenderId,
                COALESCE(g.Name, 'Not Specified') AS GenderName,
                s.CategoryId,
                COALESCE(c.Name, 'Not Specified') AS CategoryName,
                COALESCE(r.Name, 'Not Specified') AS ReligionName,
                d.DistrictId,
                d.DistrictName,
                a.AddressLine,
                a.Pincode,
                sa.ApplicationId,
                sa.ApplicationNumber,
                sa.ApplicationStatusId,
                ast.Code AS ApplicationStatusCode,
                COALESCE(ast.Name, 'No Application') AS ApplicationStatusName,
                COALESCE(sa.IsLocked, 0) AS IsLocked,
                sa.LockedAt,
                i.InstituteId,
                i.InstituteName,
                crs.CourseName,
                cb.BranchName,
                s.IsActive,
                s.CreatedAt AS RegistrationDate
            FROM students s
            LEFT JOIN genders g ON g.GenderId = s.GenderId
            LEFT JOIN categories c ON c.CategoryId = s.CategoryId
            LEFT JOIN religions r ON r.ReligionId = s.ReligionId
            LEFT JOIN addresses a ON a.StudentId = s.StudentId AND a.IsCurrent = 1 AND a.AddressType = 'PERMANENT'
            LEFT JOIN districts d ON d.DistrictId = a.DistrictId
            LEFT JOIN scholarship_applications sa ON sa.StudentId = s.StudentId
            LEFT JOIN application_statuses ast ON ast.ApplicationStatusId = sa.ApplicationStatusId
            LEFT JOIN student_academic_records sar ON sar.StudentId = s.StudentId
            LEFT JOIN institute_courses ic ON ic.InstituteCourseId = sar.InstituteCourseId
            LEFT JOIN institutes i ON i.InstituteId = ic.InstituteId
            LEFT JOIN courses crs ON crs.CourseId = ic.CourseId
            LEFT JOIN course_branches cb ON cb.BranchId = ic.BranchId
            WHERE {whereSql}
            ORDER BY s.StudentId DESC
            LIMIT @Take OFFSET @Skip;";

        var list = (await conn.QueryAsync<AdminStudentListItemDto>(dataSql, parameters)).AsList();

        return new PagedResult<AdminStudentListItemDto>
        {
            Items = list,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<AdminStudentDetailsDto?> GetStudentDetailsAsync(ulong studentId)
    {
        using var conn = await _db.CreateConnectionAsync();

        // 1. Basic Student Info
        const string studentSql = @"
            SELECT 
                s.StudentId, s.StudentCode, s.FirstName, s.MiddleName, s.LastName,
                s.DateOfBirth, s.PhotoDocumentId, s.IsActive, s.CreatedAt AS RegistrationDate,
                COALESCE(g.Name, 'Not Specified') AS GenderName,
                COALESCE(c.Name, 'Not Specified') AS CategoryName,
                COALESCE(r.Name, 'Not Specified') AS ReligionName
            FROM students s
            LEFT JOIN genders g ON g.GenderId = s.GenderId
            LEFT JOIN categories c ON c.CategoryId = s.CategoryId
            LEFT JOIN religions r ON r.ReligionId = s.ReligionId
            WHERE s.StudentId = @StudentId AND s.IsActive = 1;";

        var details = await conn.QuerySingleOrDefaultAsync<AdminStudentDetailsDto>(studentSql, new { StudentId = studentId });
        if (details == null) return null;

        // 2. Aadhaar Details
        const string aadhaarSql = @"
            SELECT MaskedAadhaar, VerificationStatus AS AadhaarVerificationStatus, ConsentGiven AS AadhaarConsentGiven, VerifiedAt AS AadhaarVerifiedAt
            FROM student_aadhaar
            WHERE StudentId = @StudentId;";
        var aadhaar = await conn.QuerySingleOrDefaultAsync(aadhaarSql, new { StudentId = studentId });
        if (aadhaar != null)
        {
            details.MaskedAadhaar = aadhaar.MaskedAadhaar ?? "XXXX-XXXX-XXXX";
            details.AadhaarVerificationStatus = aadhaar.AadhaarVerificationStatus ?? "PENDING";
            details.AadhaarConsentGiven = Convert.ToBoolean(aadhaar.AadhaarConsentGiven);
            details.AadhaarVerifiedAt = aadhaar.AadhaarVerifiedAt;
        }

        // 3. Family Details
        const string familySql = @"
            SELECT 
                f.FatherGuardianName, f.MotherName, f.IsOrphan, f.IsMotherSingleWoman,
                f.IsDifferentlyAbled, f.ParentsIlliterate,
                fo.Name AS FatherOccupation, mo.Name AS MotherOccupation
            FROM student_family_details f
            LEFT JOIN occupations fo ON fo.OccupationId = f.FatherOccupationId
            LEFT JOIN occupations mo ON mo.OccupationId = f.MotherOccupationId
            WHERE f.StudentId = @StudentId;";
        var family = await conn.QuerySingleOrDefaultAsync(familySql, new { StudentId = studentId });
        if (family != null)
        {
            details.FatherGuardianName = family.FatherGuardianName ?? "";
            details.MotherName = family.MotherName ?? "";
            details.IsOrphan = Convert.ToBoolean(family.IsOrphan);
            details.IsMotherSingleWoman = Convert.ToBoolean(family.IsMotherSingleWoman);
            details.IsDifferentlyAbled = Convert.ToBoolean(family.IsDifferentlyAbled);
            details.ParentsIlliterate = Convert.ToBoolean(family.ParentsIlliterate);
            details.FatherOccupation = family.FatherOccupation;
            details.MotherOccupation = family.MotherOccupation;
        }

        // 4. Household Details
        const string householdSql = @"
            SELECT hc.Name AS HouseholdCategory, h.AnnualIncome
            FROM household_details h
            LEFT JOIN household_categories hc ON hc.HouseholdCategoryId = h.HouseholdCategoryId
            WHERE h.StudentId = @StudentId;";
        var household = await conn.QuerySingleOrDefaultAsync(householdSql, new { StudentId = studentId });
        if (household != null)
        {
            details.HouseholdCategory = household.HouseholdCategory;
            details.AnnualIncome = household.AnnualIncome ?? 0;
        }

        // 5. Contact Details (Decrypt)
        const string contactSql = @"
            SELECT MobileEncrypted, EmailEncrypted, IsVerified
            FROM student_contacts
            WHERE StudentId = @StudentId
            ORDER BY ContactId DESC LIMIT 1;";
        var contact = await conn.QuerySingleOrDefaultAsync(contactSql, new { StudentId = studentId });
        if (contact != null)
        {
            try
            {
                byte[] mobileBytes = (byte[])contact.MobileEncrypted;
                string mobile = _crypto.Decrypt(mobileBytes);
                details.MaskedMobile = MaskingHelper.MaskMobile(mobile);
            }
            catch
            {
                details.MaskedMobile = "XXXXXX" + details.StudentId;
            }

            try
            {
                byte[] emailBytes = (byte[])contact.EmailEncrypted;
                string email = _crypto.Decrypt(emailBytes);
                details.MaskedEmail = MaskingHelper.MaskEmail(email);
            }
            catch
            {
                details.MaskedEmail = "user***@portal.gov.in";
            }

            details.IsContactVerified = Convert.ToBoolean(contact.IsVerified);
        }

        // 6. Addresses (Permanent and Correspondence)
        const string addressSql = @"
            SELECT 
                a.AddressType, a.AddressLine, a.Pincode,
                cv.Name AS CityVillage, b.BlockName, v.VidhansabhaName,
                d.DistrictName, s.StateName
            FROM addresses a
            LEFT JOIN cities_villages cv ON cv.CityVillageId = a.CityVillageId
            LEFT JOIN blocks b ON b.BlockId = a.BlockId
            LEFT JOIN vidhansabhas v ON v.VidhansabhaId = a.VidhansabhaId
            LEFT JOIN districts d ON d.DistrictId = a.DistrictId
            LEFT JOIN states s ON s.StateId = a.StateId
            WHERE a.StudentId = @StudentId AND a.IsCurrent = 1;";
        var addresses = (await conn.QueryAsync(addressSql, new { StudentId = studentId })).AsList();

        var perm = addresses.FirstOrDefault(a => (string)a.AddressType == "PERMANENT");
        if (perm != null)
        {
            details.PermanentAddressLine = perm.AddressLine;
            details.PermanentPincode = perm.Pincode;
            details.PermanentCityVillage = perm.CityVillage;
            details.PermanentBlock = perm.BlockName;
            details.PermanentVidhansabha = perm.VidhansabhaName;
            details.PermanentDistrict = perm.DistrictName;
            details.PermanentState = perm.StateName;
        }

        var corr = addresses.FirstOrDefault(a => (string)a.AddressType == "CORRESPONDENCE");
        if (corr != null)
        {
            details.CorrespondenceAddressLine = corr.AddressLine;
            details.CorrespondencePincode = corr.Pincode;
            details.CorrespondenceDistrict = corr.DistrictName;
            details.CorrespondenceState = corr.StateName;
        }

        // 7. Academic Details
        const string acadSql = @"
            SELECT 
                sar.AcademicRecordId, sar.EnrollmentNumber, sar.EnrollmentDate, sar.AdmissionDate,
                sar.CourseYear, sar.IsHosteller,
                i.InstituteCode, i.InstituteName,
                c.CourseName, cb.BranchName,
                at.Name AS AdmissionTypeName, sm.Name AS StudyModeName
            FROM student_academic_records sar
            JOIN institute_courses ic ON ic.InstituteCourseId = sar.InstituteCourseId
            JOIN institutes i ON i.InstituteId = ic.InstituteId
            JOIN courses c ON c.CourseId = ic.CourseId
            LEFT JOIN course_branches cb ON cb.BranchId = ic.BranchId
            LEFT JOIN admission_types at ON at.AdmissionTypeId = sar.AdmissionTypeId
            LEFT JOIN study_modes sm ON sm.StudyModeId = sar.StudyModeId
            WHERE sar.StudentId = @StudentId
            ORDER BY sar.AcademicRecordId DESC LIMIT 1;";
        var acad = await conn.QuerySingleOrDefaultAsync(acadSql, new { StudentId = studentId });
        if (acad != null)
        {
            details.AcademicRecordId = acad.AcademicRecordId;
            details.InstituteCode = acad.InstituteCode;
            details.InstituteName = acad.InstituteName;
            details.CourseName = acad.CourseName;
            details.BranchName = acad.BranchName;
            details.EnrollmentNumber = acad.EnrollmentNumber;
            details.EnrollmentDate = acad.EnrollmentDate;
            details.AdmissionDate = acad.AdmissionDate;
            details.AdmissionTypeName = acad.AdmissionTypeName;
            details.StudyModeName = acad.StudyModeName;
            details.CourseYear = acad.CourseYear;
            details.IsHosteller = Convert.ToBoolean(acad.IsHosteller);
        }

        // 8. Bank Account Details
        const string bankSql = @"
            SELECT 
                b.BankName, bb.BranchName AS BankBranchName, bb.IFSCCode,
                sba.MaskedAccountNumber, sba.IsAadhaarSeeded, sba.VerificationStatus AS BankVerificationStatus
            FROM student_bank_accounts sba
            JOIN banks b ON b.BankId = sba.BankId
            JOIN bank_branches bb ON bb.BranchId = sba.BranchId
            WHERE sba.StudentId = @StudentId AND sba.IsActive = 1
            ORDER BY sba.StudentBankAccountId DESC LIMIT 1;";
        var bank = await conn.QuerySingleOrDefaultAsync(bankSql, new { StudentId = studentId });
        if (bank != null)
        {
            details.BankName = bank.BankName;
            details.BankBranchName = bank.BankBranchName;
            details.IFSCCode = bank.IFSCCode;
            details.MaskedAccountNumber = bank.MaskedAccountNumber;
            details.IsAadhaarSeeded = Convert.ToBoolean(bank.IsAadhaarSeeded);
            details.BankVerificationStatus = bank.BankVerificationStatus;
        }

        // 9. Application Details
        const string appSql = @"
            SELECT 
                sa.ApplicationId, sa.ApplicationNumber, sa.CurrentStep, sa.IsLocked, sa.LockedAt, sa.CreatedAt AS ApplicationCreatedAt,
                ay.AcademicYearCode,
                sch.SchemeName,
                ast.Code AS ApplicationStatusCode, ast.Name AS ApplicationStatusName
            FROM scholarship_applications sa
            JOIN academic_years ay ON ay.AcademicYearId = sa.AcademicYearId
            JOIN scholarship_schemes sch ON sch.SchemeId = sa.SchemeId
            JOIN application_statuses ast ON ast.ApplicationStatusId = sa.ApplicationStatusId
            WHERE sa.StudentId = @StudentId
            ORDER BY sa.ApplicationId DESC LIMIT 1;";
        var app = await conn.QuerySingleOrDefaultAsync(appSql, new { StudentId = studentId });
        if (app != null)
        {
            details.ApplicationId = app.ApplicationId;
            details.ApplicationNumber = app.ApplicationNumber;
            details.AcademicYearCode = app.AcademicYearCode;
            details.SchemeName = app.SchemeName;
            details.ApplicationStatusCode = app.ApplicationStatusCode;
            details.ApplicationStatusName = app.ApplicationStatusName;
            details.CurrentStep = app.CurrentStep;
            details.IsLocked = Convert.ToBoolean(app.IsLocked);
            details.LockedAt = app.LockedAt;
            details.ApplicationCreatedAt = app.ApplicationCreatedAt;

            // 10. Verification Trail for this Application
            const string verifSql = @"
                SELECT 
                    av.VerificationId, av.VerificationLevel, av.Status, av.Remarks, av.VerifiedAt,
                    u.UserName AS VerifiedByUserName
                FROM application_verifications av
                LEFT JOIN users u ON u.UserId = av.VerifiedBy
                WHERE av.ApplicationId = @ApplicationId
                ORDER BY av.VerificationId ASC;";
            details.Verifications = (await conn.QueryAsync<AdminStudentVerificationDto>(verifSql, new { ApplicationId = details.ApplicationId })).AsList();
        }

        // 11. Documents
        const string docSql = @"
            SELECT 
                d.DocumentId, d.FileName, d.FileSizeBytes, d.MimeType, d.UploadedAt, d.IsVerified,
                dt.Name AS DocumentTypeName
            FROM documents d
            JOIN document_types dt ON dt.DocumentTypeId = d.DocumentTypeId
            WHERE d.StudentId = @StudentId AND d.IsActive = 1 AND d.IsDeleted = 0
            ORDER BY d.DocumentId DESC;";
        details.Documents = (await conn.QueryAsync<AdminStudentDocumentDto>(docSql, new { StudentId = studentId })).AsList();

        // 12. Certificates
        const string certSql = @"
            SELECT 
                sc.CertificateId, sc.VerificationStatus, sc.IssueDate, sc.DocumentId,
                ct.Name AS CertificateTypeName
            FROM student_certificates sc
            JOIN certificate_types ct ON ct.CertificateTypeId = sc.CertificateTypeId
            WHERE sc.StudentId = @StudentId
            ORDER BY sc.CertificateId DESC;";
        details.Certificates = (await conn.QueryAsync<AdminStudentCertificateDto>(certSql, new { StudentId = studentId })).AsList();

        return details;
    }

    public async Task<AdminDashboardStatsDto> GetAdminStatsAsync(ulong? districtId, ulong? instituteId)
    {
        using var conn = await _db.CreateConnectionAsync();

        string filterSql = "1=1";
        var param = new DynamicParameters();

        if (districtId.HasValue && districtId.Value > 0)
        {
            filterSql += " AND a.DistrictId = @DistrictId";
            param.Add("DistrictId", districtId.Value);
        }

        if (instituteId.HasValue && instituteId.Value > 0)
        {
            filterSql += " AND ic.InstituteId = @InstituteId";
            param.Add("InstituteId", instituteId.Value);
        }

        string sql = $@"
            SELECT 
                COUNT(DISTINCT s.StudentId) AS TotalStudents,
                COUNT(DISTINCT sa.ApplicationId) AS TotalApplications,
                COUNT(DISTINCT CASE WHEN ast.Code = 'INSTITUTE_VERIFICATION' THEN sa.ApplicationId END) AS PendingInstituteVerification,
                COUNT(DISTINCT CASE WHEN ast.Code = 'DISTRICT_VERIFICATION' THEN sa.ApplicationId END) AS PendingDistrictVerification,
                COUNT(DISTINCT CASE WHEN ast.Code = 'APPROVED' THEN sa.ApplicationId END) AS ApprovedApplications,
                COUNT(DISTINCT CASE WHEN ast.Code = 'REJECTED' THEN sa.ApplicationId END) AS RejectedApplications,
                COUNT(DISTINCT CASE WHEN ast.Code = 'PAID' THEN sa.ApplicationId END) AS DisbursedApplications
            FROM students s
            LEFT JOIN addresses a ON a.StudentId = s.StudentId AND a.IsCurrent = 1 AND a.AddressType = 'PERMANENT'
            LEFT JOIN scholarship_applications sa ON sa.StudentId = s.StudentId
            LEFT JOIN application_statuses ast ON ast.ApplicationStatusId = sa.ApplicationStatusId
            LEFT JOIN student_academic_records sar ON sar.StudentId = s.StudentId
            LEFT JOIN institute_courses ic ON ic.InstituteCourseId = sar.InstituteCourseId
            WHERE s.IsActive = 1 AND {filterSql};";

        var stats = await conn.QuerySingleOrDefaultAsync<AdminDashboardStatsDto>(sql, param) ?? new AdminDashboardStatsDto();

        stats.TotalDistricts = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM districts WHERE IsActive = 1;");
        stats.TotalInstitutes = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM institutes WHERE IsActive = 1;");

        return stats;
    }
}
