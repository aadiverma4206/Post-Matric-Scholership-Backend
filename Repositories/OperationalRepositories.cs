using Dapper;
using Scholarship.Api.Data;
using Scholarship.Api.Interfaces;
using Scholarship.Api.Models;

namespace Scholarship.Api.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly IDbConnectionFactory _db;

    public AddressRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<ulong> SaveAddressAsync(Address address)
    {
        using var conn = await _db.CreateConnectionAsync();
        // Deactivate old current address of same type if present
        const string updateOldSql = @"
            UPDATE addresses SET IsCurrent = 0 
            WHERE StudentId = @StudentId AND AddressType = @AddressType;";
        await conn.ExecuteAsync(updateOldSql, new { address.StudentId, address.AddressType });

        const string insertSql = @"
            INSERT INTO addresses (StudentId, AddressType, AddressLine, Pincode, PostOfficeId, CityVillageId, DistrictId, BlockId, VidhansabhaId, StateId, IsCurrent, CreatedAt)
            VALUES (@StudentId, @AddressType, @AddressLine, @Pincode, @PostOfficeId, @CityVillageId, @DistrictId, @BlockId, @VidhansabhaId, @StateId, 1, CURRENT_TIMESTAMP);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<ulong>(insertSql, address);
    }

    public async Task<List<Address>> GetAddressesByStudentIdAsync(ulong studentId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT AddressId, StudentId, AddressType, AddressLine, Pincode, PostOfficeId, CityVillageId, DistrictId, BlockId, VidhansabhaId, StateId, IsCurrent, CreatedAt
            FROM addresses
            WHERE StudentId = @StudentId AND IsCurrent = 1;";

        var list = await conn.QueryAsync<Address>(sql, new { StudentId = studentId });
        return list.AsList();
    }

    public async Task<Address?> GetAddressByTypeAsync(ulong studentId, string addressType)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT AddressId, StudentId, AddressType, AddressLine, Pincode, PostOfficeId, CityVillageId, DistrictId, BlockId, VidhansabhaId, StateId, IsCurrent, CreatedAt
            FROM addresses
            WHERE StudentId = @StudentId AND AddressType = @AddressType AND IsCurrent = 1
            ORDER BY AddressId DESC LIMIT 1;";

        return await conn.QuerySingleOrDefaultAsync<Address>(sql, new { StudentId = studentId, AddressType = addressType });
    }
}

public class AcademicRepository : IAcademicRepository
{
    private readonly IDbConnectionFactory _db;

    public AcademicRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task Save10thDetailsAsync(Student10thDetail detail)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            INSERT INTO student_10th_details (StudentId, RollNumber, SchoolType, PassingYear, BoardId, Percentage, MarksheetDocumentId, CreatedAt)
            VALUES (@StudentId, @RollNumber, @SchoolType, @PassingYear, @BoardId, @Percentage, @MarksheetDocumentId, CURRENT_TIMESTAMP)
            ON DUPLICATE KEY UPDATE
                RollNumber = VALUES(RollNumber),
                SchoolType = VALUES(SchoolType),
                PassingYear = VALUES(PassingYear),
                BoardId = VALUES(BoardId),
                Percentage = VALUES(Percentage),
                MarksheetDocumentId = VALUES(MarksheetDocumentId);";

        await conn.ExecuteAsync(sql, detail);
    }

    public async Task<Student10thDetail?> Get10thDetailsByStudentIdAsync(ulong studentId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT Class10Id, StudentId, RollNumber, SchoolType, PassingYear, BoardId, Percentage, MarksheetDocumentId, CreatedAt
            FROM student_10th_details
            WHERE StudentId = @StudentId;";

        return await conn.QuerySingleOrDefaultAsync<Student10thDetail>(sql, new { StudentId = studentId });
    }

    public async Task SavePreviousEducationAsync(PreviousEducation education)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            INSERT INTO previous_education (StudentId, AcademicRecordId, CourseTypeId, CourseId, BranchId, InstituteName, RollNumber, PassingYear, Percentage, MarksheetDocumentId, CreatedAt)
            VALUES (@StudentId, @AcademicRecordId, @CourseTypeId, @CourseId, @BranchId, @InstituteName, @RollNumber, @PassingYear, @Percentage, @MarksheetDocumentId, CURRENT_TIMESTAMP);";

        await conn.ExecuteAsync(sql, education);
    }

    public async Task<PreviousEducation?> GetPreviousEducationByStudentIdAsync(ulong studentId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT PreviousEducationId, StudentId, AcademicRecordId, CourseTypeId, CourseId, BranchId, InstituteName, RollNumber, PassingYear, Percentage, MarksheetDocumentId, CreatedAt
            FROM previous_education
            WHERE StudentId = @StudentId
            ORDER BY PreviousEducationId DESC LIMIT 1;";

        return await conn.QuerySingleOrDefaultAsync<PreviousEducation>(sql, new { StudentId = studentId });
    }

    public async Task<ulong> SaveAcademicRecordAsync(StudentAcademicRecord record)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            INSERT INTO student_academic_records (StudentId, AcademicYearId, SchemeId, InstituteCourseId, AdmissionDate, EnrollmentNumber, EnrollmentDate, AdmissionTypeId, StudyModeId, CourseYear, IsHosteller, IsLateralEntry, IsLocked, CreatedAt)
            VALUES (@StudentId, @AcademicYearId, @SchemeId, @InstituteCourseId, @AdmissionDate, @EnrollmentNumber, @EnrollmentDate, @AdmissionTypeId, @StudyModeId, @CourseYear, @IsHosteller, @IsLateralEntry, @IsLocked, CURRENT_TIMESTAMP);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<ulong>(sql, record);
    }

    public async Task<StudentAcademicRecord?> GetAcademicRecordByStudentIdAsync(ulong studentId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT AcademicRecordId, StudentId, AcademicYearId, SchemeId, InstituteCourseId, AdmissionDate, EnrollmentNumber, EnrollmentDate, AdmissionTypeId, StudyModeId, CourseYear, IsHosteller, IsLateralEntry, IsLocked, CreatedAt
            FROM student_academic_records
            WHERE StudentId = @StudentId
            ORDER BY AcademicRecordId DESC LIMIT 1;";

        return await conn.QuerySingleOrDefaultAsync<StudentAcademicRecord>(sql, new { StudentId = studentId });
    }
}

public class BankRepository : IBankRepository
{
    private readonly IDbConnectionFactory _db;

    public BankRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<ulong> SaveBankAccountAsync(StudentBankAccount account)
    {
        using var conn = await _db.CreateConnectionAsync();
        // Deactivate older active accounts
        const string deactivateSql = @"
            UPDATE student_bank_accounts
            SET IsActive = 0, ValidTo = CURRENT_TIMESTAMP
            WHERE StudentId = @StudentId AND IsActive = 1;";
        await conn.ExecuteAsync(deactivateSql, new { account.StudentId });

        const string insertSql = @"
            INSERT INTO student_bank_accounts (StudentId, BankId, BranchId, AccountNumberEncrypted, AccountNumberHash, MaskedAccountNumber, IsAadhaarSeeded, VerificationStatus, IsActive, ValidFrom, CreatedAt)
            VALUES (@StudentId, @BankId, @BranchId, @AccountNumberEncrypted, @AccountNumberHash, @MaskedAccountNumber, @IsAadhaarSeeded, 'VERIFIED', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<ulong>(insertSql, account);
    }

    public async Task<StudentBankAccount?> GetActiveBankAccountByStudentIdAsync(ulong studentId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT StudentBankAccountId, StudentId, BankId, BranchId, AccountNumberEncrypted, AccountNumberHash, MaskedAccountNumber, IsAadhaarSeeded, VerificationStatus, IsActive, ValidFrom, ValidTo, CreatedAt
            FROM student_bank_accounts
            WHERE StudentId = @StudentId AND IsActive = 1
            ORDER BY StudentBankAccountId DESC LIMIT 1;";

        return await conn.QuerySingleOrDefaultAsync<StudentBankAccount>(sql, new { StudentId = studentId });
    }
}

public class CertificateRepository : ICertificateRepository
{
    private readonly IDbConnectionFactory _db;

    public CertificateRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<ulong> SaveCertificateAsync(StudentCertificate certificate)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            INSERT INTO student_certificates (StudentId, AcademicYearId, CertificateTypeId, IsOnlineGenerated, GeneratedFrom, ReferenceNumberEncrypted, ReferenceNumberHash, IssueDate, DocumentId, VerificationStatus, VerifiedAt, CreatedAt)
            VALUES (@StudentId, @AcademicYearId, @CertificateTypeId, @IsOnlineGenerated, @GeneratedFrom, @ReferenceNumberEncrypted, @ReferenceNumberHash, @IssueDate, @DocumentId, @VerificationStatus, @VerifiedAt, CURRENT_TIMESTAMP);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<ulong>(sql, certificate);
    }

    public async Task<List<StudentCertificate>> GetCertificatesByStudentIdAsync(ulong studentId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT CertificateId, StudentId, AcademicYearId, CertificateTypeId, IsOnlineGenerated, GeneratedFrom, ReferenceNumberEncrypted, ReferenceNumberHash, IssueDate, DocumentId, VerificationStatus, VerifiedAt, CreatedAt
            FROM student_certificates
            WHERE StudentId = @StudentId;";

        var list = await conn.QueryAsync<StudentCertificate>(sql, new { StudentId = studentId });
        return list.AsList();
    }

    public async Task<StudentCertificate?> GetCertificateByTypeAsync(ulong studentId, uint certificateTypeId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT CertificateId, StudentId, AcademicYearId, CertificateTypeId, IsOnlineGenerated, GeneratedFrom, ReferenceNumberEncrypted, ReferenceNumberHash, IssueDate, DocumentId, VerificationStatus, VerifiedAt, CreatedAt
            FROM student_certificates
            WHERE StudentId = @StudentId AND CertificateTypeId = @CertificateTypeId
            ORDER BY CertificateId DESC LIMIT 1;";

        return await conn.QuerySingleOrDefaultAsync<StudentCertificate>(sql, new { StudentId = studentId, CertificateTypeId = certificateTypeId });
    }
}

public class DocumentRepository : IDocumentRepository
{
    private readonly IDbConnectionFactory _db;

    public DocumentRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<ulong> SaveDocumentAsync(Document document)
    {
        using var conn = await _db.CreateConnectionAsync();
        if (string.IsNullOrEmpty(document.DocumentUUID))
        {
            document.DocumentUUID = Guid.NewGuid().ToString();
        }

        const string sql = @"
            INSERT INTO documents (DocumentUUID, StudentId, DocumentTypeId, AcademicYearId, FileName, StorageKey, FileHash, MimeType, FileSizeBytes, UploadedAt, UploadedBy, IsVerified, IsActive, IsDeleted)
            VALUES (@DocumentUUID, @StudentId, @DocumentTypeId, @AcademicYearId, @FileName, @StorageKey, @FileHash, @MimeType, @FileSizeBytes, CURRENT_TIMESTAMP, @UploadedBy, @IsVerified, 1, 0);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<ulong>(sql, document);
    }

    public async Task<Document?> GetDocumentByIdAsync(ulong documentId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT DocumentId, DocumentUUID, StudentId, DocumentTypeId, AcademicYearId, FileName, StorageKey, FileHash, MimeType, FileSizeBytes, UploadedAt, UploadedBy, IsVerified, IsActive
            FROM documents
            WHERE DocumentId = @DocumentId AND IsActive = 1 AND IsDeleted = 0;";

        return await conn.QuerySingleOrDefaultAsync<Document>(sql, new { DocumentId = documentId });
    }

    public async Task<List<Document>> GetDocumentsByStudentIdAsync(ulong studentId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT DocumentId, DocumentUUID, StudentId, DocumentTypeId, AcademicYearId, FileName, StorageKey, FileHash, MimeType, FileSizeBytes, UploadedAt, UploadedBy, IsVerified, IsActive
            FROM documents
            WHERE StudentId = @StudentId AND IsActive = 1 AND IsDeleted = 0
            ORDER BY DocumentId DESC;";

        var list = await conn.QueryAsync<Document>(sql, new { StudentId = studentId });
        return list.AsList();
    }

    public async Task UpdateEntityDocumentLinkAsync(ulong studentId, uint documentTypeId, ulong documentId)
    {
        using var conn = await _db.CreateConnectionAsync();

        switch (documentTypeId)
        {
            case 1: // Student Photo
                await conn.ExecuteAsync(
                    "UPDATE students SET PhotoDocumentId = @DocumentId, UpdatedAt = CURRENT_TIMESTAMP WHERE StudentId = @StudentId;",
                    new { StudentId = studentId, DocumentId = documentId });
                break;

            case 2: // 10th Marksheet
                await conn.ExecuteAsync(
                    "UPDATE student_10th_details SET MarksheetDocumentId = @DocumentId WHERE StudentId = @StudentId;",
                    new { StudentId = studentId, DocumentId = documentId });
                break;

            case 3: // Previous Qualifying Exam Marksheet
                await conn.ExecuteAsync(
                    "UPDATE previous_education SET MarksheetDocumentId = @DocumentId WHERE StudentId = @StudentId;",
                    new { StudentId = studentId, DocumentId = documentId });
                break;

            case 4: // Bank Passbook
                await conn.ExecuteAsync(
                    "UPDATE student_bank_accounts SET PassbookDocumentId = @DocumentId WHERE StudentId = @StudentId AND IsActive = 1;",
                    new { StudentId = studentId, DocumentId = documentId });
                break;

            case 5: // Caste Certificate
                await conn.ExecuteAsync(
                    "UPDATE student_certificates SET DocumentId = @DocumentId WHERE StudentId = @StudentId AND CertificateTypeId = 1;",
                    new { StudentId = studentId, DocumentId = documentId });
                break;

            case 6: // Domicile Certificate
                await conn.ExecuteAsync(
                    "UPDATE student_certificates SET DocumentId = @DocumentId WHERE StudentId = @StudentId AND CertificateTypeId = 2;",
                    new { StudentId = studentId, DocumentId = documentId });
                break;

            case 7: // Income Certificate
                await conn.ExecuteAsync(
                    "UPDATE student_certificates SET DocumentId = @DocumentId WHERE StudentId = @StudentId AND CertificateTypeId = 3;",
                    new { StudentId = studentId, DocumentId = documentId });
                break;

            case 8: // Disability Certificate
            case 15:
                await conn.ExecuteAsync(
                    "UPDATE student_certificates SET DocumentId = @DocumentId WHERE StudentId = @StudentId AND CertificateTypeId = 4;",
                    new { StudentId = studentId, DocumentId = documentId });
                break;
        }

        // Attach to latest application if present
        const string linkAppSql = @"
            INSERT INTO application_documents (ApplicationId, DocumentId, DocumentTypeId, IsRequired, IsLatest, VerificationStatus, UploadedAt, CreatedAt, UpdatedAt)
            SELECT ApplicationId, @DocumentId, @DocumentTypeId, 1, 1, 'PENDING', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
            FROM scholarship_applications
            WHERE StudentId = @StudentId
            ORDER BY ApplicationId DESC LIMIT 1
            ON DUPLICATE KEY UPDATE DocumentId = VALUES(DocumentId), UpdatedAt = CURRENT_TIMESTAMP;";

        try
        {
            await conn.ExecuteAsync(linkAppSql, new { StudentId = studentId, DocumentId = documentId, DocumentTypeId = documentTypeId });
        }
        catch
        {
            // Ignore if application doesn't exist yet
        }
    }
}
