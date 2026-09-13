using Dapper;
using Scholarship.Api.Data;
using Scholarship.Api.Interfaces;
using Scholarship.Api.Models;

namespace Scholarship.Api.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly IDbConnectionFactory _db;

    public StudentRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<ulong> CreateStudentAsync(Student student)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            INSERT INTO students (StudentCode, FirstName, MiddleName, LastName, DateOfBirth, GenderId, CategoryId, ReligionId, PhotoDocumentId, IsActive, CreatedAt, UpdatedAt)
            VALUES (@StudentCode, @FirstName, @MiddleName, @LastName, @DateOfBirth, @GenderId, @CategoryId, @ReligionId, @PhotoDocumentId, @IsActive, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<ulong>(sql, student);
    }

    public async Task<Student?> GetByIdAsync(ulong studentId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT StudentId, StudentCode, FirstName, MiddleName, LastName, DateOfBirth, GenderId, CategoryId, ReligionId, PhotoDocumentId, IsActive, CreatedAt, UpdatedAt
            FROM students
            WHERE StudentId = @StudentId AND IsActive = 1;";

        return await conn.QuerySingleOrDefaultAsync<Student>(sql, new { StudentId = studentId });
    }

    public async Task<Student?> GetByCodeAsync(string studentCode)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT StudentId, StudentCode, FirstName, MiddleName, LastName, DateOfBirth, GenderId, CategoryId, ReligionId, PhotoDocumentId, IsActive, CreatedAt, UpdatedAt
            FROM students
            WHERE StudentCode = @StudentCode AND IsActive = 1;";

        return await conn.QuerySingleOrDefaultAsync<Student>(sql, new { StudentCode = studentCode });
    }

    public async Task<bool> UpdateStudentAsync(Student student)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            UPDATE students 
            SET FirstName = @FirstName, MiddleName = @MiddleName, LastName = @LastName,
                DateOfBirth = @DateOfBirth, GenderId = @GenderId, CategoryId = @CategoryId, 
                ReligionId = @ReligionId, PhotoDocumentId = @PhotoDocumentId, UpdatedAt = CURRENT_TIMESTAMP
            WHERE StudentId = @StudentId;";

        int affected = await conn.ExecuteAsync(sql, student);
        return affected > 0;
    }
}

public class StudentAuthRepository : IStudentAuthRepository
{
    private readonly IDbConnectionFactory _db;

    public StudentAuthRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<ulong> CreateCredentialAsync(StudentCredential credential)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            INSERT INTO student_credentials (StudentId, UserName, PasswordHash, PasswordAlgorithm, PasswordChangedAt, FailedLoginCount, IsLocked, CreatedAt)
            VALUES (@StudentId, @UserName, @PasswordHash, @PasswordAlgorithm, CURRENT_TIMESTAMP, 0, 0, CURRENT_TIMESTAMP);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<ulong>(sql, credential);
    }

    public async Task<StudentCredential?> GetByUsernameAsync(string username)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT CredentialId, StudentId, UserName, PasswordHash, PasswordAlgorithm, PasswordChangedAt, FailedLoginCount, LockedUntil, IsLocked, LastLoginAt, CreatedAt
            FROM student_credentials
            WHERE UserName = @UserName;";

        return await conn.QuerySingleOrDefaultAsync<StudentCredential>(sql, new { UserName = username });
    }

    public async Task<StudentCredential?> GetByStudentIdAsync(ulong studentId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT CredentialId, StudentId, UserName, PasswordHash, PasswordAlgorithm, PasswordChangedAt, FailedLoginCount, LockedUntil, IsLocked, LastLoginAt, CreatedAt
            FROM student_credentials
            WHERE StudentId = @StudentId;";

        return await conn.QuerySingleOrDefaultAsync<StudentCredential>(sql, new { StudentId = studentId });
    }

    public async Task UpdatePasswordAsync(ulong studentId, byte[] newPasswordHash)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            UPDATE student_credentials
            SET PasswordHash = @PasswordHash, PasswordChangedAt = CURRENT_TIMESTAMP, FailedLoginCount = 0, IsLocked = 0, LockedUntil = NULL
            WHERE StudentId = @StudentId;";

        await conn.ExecuteAsync(sql, new { StudentId = studentId, PasswordHash = newPasswordHash });
    }

    public async Task RecordLoginAttemptAsync(ulong credentialId, bool isSuccess)
    {
        using var conn = await _db.CreateConnectionAsync();
        if (isSuccess)
        {
            const string sql = @"
                UPDATE student_credentials
                SET FailedLoginCount = 0, IsLocked = 0, LockedUntil = NULL, LastLoginAt = CURRENT_TIMESTAMP
                WHERE CredentialId = @CredentialId;";
            await conn.ExecuteAsync(sql, new { CredentialId = credentialId });
        }
        else
        {
            const string sql = @"
                UPDATE student_credentials
                SET FailedLoginCount = FailedLoginCount + 1,
                    IsLocked = CASE WHEN FailedLoginCount + 1 >= 5 THEN 1 ELSE 0 END,
                    LockedUntil = CASE WHEN FailedLoginCount + 1 >= 5 THEN DATE_ADD(CURRENT_TIMESTAMP, INTERVAL 30 MINUTE) ELSE NULL END
                WHERE CredentialId = @CredentialId;";
            await conn.ExecuteAsync(sql, new { CredentialId = credentialId });
        }
    }

    public async Task<User?> GetOfficialUserByUsernameAsync(string username)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT UserId, UserName, PasswordHash, UserTypeId, IsActive, LastLoginAt, CreatedAt
            FROM users
            WHERE UserName = @UserName AND IsActive = 1;";

        return await conn.QuerySingleOrDefaultAsync<User>(sql, new { UserName = username });
    }

    public async Task<List<string>> GetUserRolesAsync(ulong userId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT r.Code
            FROM roles r
            INNER JOIN user_roles ur ON r.RoleId = ur.RoleId
            WHERE ur.UserId = @UserId AND r.IsActive = 1;";

        var roles = await conn.QueryAsync<string>(sql, new { UserId = userId });
        return roles.AsList();
    }

    public async Task<InstituteUser?> GetInstituteUserByUserIdAsync(ulong userId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT InstituteUserId, UserId, InstituteId, IsActive, CreatedAt
            FROM institute_users
            WHERE UserId = @UserId AND IsActive = 1;";

        return await conn.QuerySingleOrDefaultAsync<InstituteUser>(sql, new { UserId = userId });
    }
}

public class StudentVaultRepository : IStudentVaultRepository
{
    private readonly IDbConnectionFactory _db;

    public StudentVaultRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task SaveAadhaarAsync(StudentAadhaar aadhaar)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            INSERT INTO student_aadhaar (StudentId, AadhaarEncrypted, AadhaarHash, MaskedAadhaar, VerificationStatus, VerifiedAt, ConsentGiven, ConsentAt, CreatedAt)
            VALUES (@StudentId, @AadhaarEncrypted, @AadhaarHash, @MaskedAadhaar, @VerificationStatus, @VerifiedAt, @ConsentGiven, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
            ON DUPLICATE KEY UPDATE 
                AadhaarEncrypted = VALUES(AadhaarEncrypted),
                AadhaarHash = VALUES(AadhaarHash),
                MaskedAadhaar = VALUES(MaskedAadhaar),
                VerificationStatus = VALUES(VerificationStatus),
                VerifiedAt = VALUES(VerifiedAt);";

        await conn.ExecuteAsync(sql, aadhaar);
    }

    public async Task<StudentAadhaar?> GetAadhaarByStudentIdAsync(ulong studentId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT AadhaarId, StudentId, AadhaarEncrypted, AadhaarHash, MaskedAadhaar, VerificationStatus, VerifiedAt, ConsentGiven, ConsentAt, CreatedAt
            FROM student_aadhaar
            WHERE StudentId = @StudentId;";

        return await conn.QuerySingleOrDefaultAsync<StudentAadhaar>(sql, new { StudentId = studentId });
    }

    public async Task<bool> ExistsAadhaarHashAsync(byte[] aadhaarHash)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT COUNT(1) FROM student_aadhaar WHERE AadhaarHash = @AadhaarHash;";
        int count = await conn.ExecuteScalarAsync<int>(sql, new { AadhaarHash = aadhaarHash });
        return count > 0;
    }

    public async Task SaveContactAsync(StudentContact contact)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            INSERT INTO student_contacts (StudentId, MobileEncrypted, MobileHash, EmailEncrypted, EmailHash, AlternateMobileEncrypted, AlternateEmailEncrypted, IsVerified, CreatedAt)
            VALUES (@StudentId, @MobileEncrypted, @MobileHash, @EmailEncrypted, @EmailHash, @AlternateMobileEncrypted, @AlternateEmailEncrypted, @IsVerified, CURRENT_TIMESTAMP);";

        await conn.ExecuteAsync(sql, contact);
    }

    public async Task<StudentContact?> GetContactByStudentIdAsync(ulong studentId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT ContactId, StudentId, MobileEncrypted, MobileHash, EmailEncrypted, EmailHash, AlternateMobileEncrypted, AlternateEmailEncrypted, IsVerified, CreatedAt
            FROM student_contacts
            WHERE StudentId = @StudentId
            ORDER BY ContactId DESC LIMIT 1;";

        return await conn.QuerySingleOrDefaultAsync<StudentContact>(sql, new { StudentId = studentId });
    }

    public async Task<bool> ExistsMobileHashAsync(byte[] mobileHash)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT COUNT(1) FROM student_contacts WHERE MobileHash = @MobileHash;";
        int count = await conn.ExecuteScalarAsync<int>(sql, new { MobileHash = mobileHash });
        return count > 0;
    }

    public async Task<bool> ExistsEmailHashAsync(byte[] emailHash)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT COUNT(1) FROM student_contacts WHERE EmailHash = @EmailHash;";
        int count = await conn.ExecuteScalarAsync<int>(sql, new { EmailHash = emailHash });
        return count > 0;
    }

    public async Task<ulong?> GetStudentIdByContactHashAsync(byte[] contactHash)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT StudentId 
            FROM student_contacts 
            WHERE EmailHash = @ContactHash OR MobileHash = @ContactHash 
            ORDER BY ContactId DESC LIMIT 1;";
        return await conn.QueryFirstOrDefaultAsync<ulong?>(sql, new { ContactHash = contactHash });
    }

    public async Task SaveFamilyDetailsAsync(StudentFamilyDetail family)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            INSERT INTO student_family_details (StudentId, FatherGuardianName, MotherName, IsOrphan, IsMotherSingleWoman, FatherOccupationId, MotherOccupationId, IsDifferentlyAbled, ParentsIlliterate, CreatedAt)
            VALUES (@StudentId, @FatherGuardianName, @MotherName, @IsOrphan, @IsMotherSingleWoman, @FatherOccupationId, @MotherOccupationId, @IsDifferentlyAbled, @ParentsIlliterate, CURRENT_TIMESTAMP)
            ON DUPLICATE KEY UPDATE
                FatherGuardianName = VALUES(FatherGuardianName),
                MotherName = VALUES(MotherName),
                IsOrphan = VALUES(IsOrphan),
                IsMotherSingleWoman = VALUES(IsMotherSingleWoman),
                FatherOccupationId = VALUES(FatherOccupationId),
                MotherOccupationId = VALUES(MotherOccupationId),
                IsDifferentlyAbled = VALUES(IsDifferentlyAbled),
                ParentsIlliterate = VALUES(ParentsIlliterate);";

        await conn.ExecuteAsync(sql, family);
    }

    public async Task<StudentFamilyDetail?> GetFamilyDetailsByStudentIdAsync(ulong studentId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT FamilyDetailId, StudentId, FatherGuardianName, MotherName, IsOrphan, IsMotherSingleWoman, FatherOccupationId, MotherOccupationId, IsDifferentlyAbled, ParentsIlliterate, CreatedAt
            FROM student_family_details
            WHERE StudentId = @StudentId;";

        return await conn.QuerySingleOrDefaultAsync<StudentFamilyDetail>(sql, new { StudentId = studentId });
    }

    public async Task SaveHouseholdDetailsAsync(HouseholdDetail household)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            INSERT INTO household_details (StudentId, HouseholdCategoryId, BplNumberEncrypted, BplNumberHash, AnnualIncome, CreatedAt)
            VALUES (@StudentId, @HouseholdCategoryId, @BplNumberEncrypted, @BplNumberHash, @AnnualIncome, CURRENT_TIMESTAMP)
            ON DUPLICATE KEY UPDATE
                HouseholdCategoryId = VALUES(HouseholdCategoryId),
                BplNumberEncrypted = VALUES(BplNumberEncrypted),
                BplNumberHash = VALUES(BplNumberHash),
                AnnualIncome = VALUES(AnnualIncome);";

        await conn.ExecuteAsync(sql, household);
    }

    public async Task<HouseholdDetail?> GetHouseholdDetailsByStudentIdAsync(ulong studentId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT HouseholdDetailId, StudentId, HouseholdCategoryId, BplNumberEncrypted, BplNumberHash, AnnualIncome, CreatedAt
            FROM household_details
            WHERE StudentId = @StudentId;";

        return await conn.QuerySingleOrDefaultAsync<HouseholdDetail>(sql, new { StudentId = studentId });
    }

    public async Task SaveDeprivationCriteriaAsync(ulong studentId, IEnumerable<uint> criterionIds)
    {
        using var conn = await _db.CreateConnectionAsync();
        await conn.ExecuteAsync("DELETE FROM household_deprivation WHERE StudentId = @StudentId;", new { StudentId = studentId });

        if (criterionIds != null && criterionIds.Any())
        {
            const string insertSql = @"
                INSERT INTO household_deprivation (StudentId, CriterionId, IsApplicable, CreatedAt)
                VALUES (@StudentId, @CriterionId, 1, CURRENT_TIMESTAMP);";

            var rows = criterionIds.Select(id => new { StudentId = studentId, CriterionId = id });
            await conn.ExecuteAsync(insertSql, rows);
        }
    }

    public async Task<List<uint>> GetDeprivationCriteriaByStudentIdAsync(ulong studentId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = "SELECT CriterionId FROM household_deprivation WHERE StudentId = @StudentId AND IsApplicable = 1;";
        var results = await conn.QueryAsync<uint>(sql, new { StudentId = studentId });
        return results.AsList();
    }

    public async Task SaveOtrAsync(StudentOtr otr)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            INSERT INTO student_otr (StudentId, OtrNumberEncrypted, OtrNumberHash, VerificationStatus, VerifiedAt, VerificationSource, CreatedAt)
            VALUES (@StudentId, @OtrNumberEncrypted, @OtrNumberHash, @VerificationStatus, @VerifiedAt, @VerificationSource, CURRENT_TIMESTAMP)
            ON DUPLICATE KEY UPDATE
                OtrNumberEncrypted = VALUES(OtrNumberEncrypted),
                OtrNumberHash = VALUES(OtrNumberHash),
                VerificationStatus = VALUES(VerificationStatus),
                VerifiedAt = VALUES(VerifiedAt);";

        await conn.ExecuteAsync(sql, otr);
    }

    public async Task<StudentOtr?> GetOtrByStudentIdAsync(ulong studentId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT OtrId, StudentId, OtrNumberEncrypted, OtrNumberHash, VerificationStatus, VerifiedAt, VerificationSource, CreatedAt
            FROM student_otr
            WHERE StudentId = @StudentId
            ORDER BY OtrId DESC LIMIT 1;";

        return await conn.QuerySingleOrDefaultAsync<StudentOtr>(sql, new { StudentId = studentId });
    }
}

