using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Scholarship.Api.Constants;
using Scholarship.Api.DTOs;
using Scholarship.Api.Exceptions;
using Scholarship.Api.Helpers;
using Scholarship.Api.Interfaces;
using Scholarship.Api.Models;
using Scholarship.Api.Security;

namespace Scholarship.Api.Services;

public interface IAuthService
{
    Task<AuthResultDto> RegisterStudentAsync(StudentRegisterDto dto);
    Task<AuthResultDto> LoginStudentAsync(StudentLoginDto dto);
    Task<AuthResultDto> LoginOfficialAsync(OfficialLoginDto dto);
    Task<string> ForgotPasswordAsync(string username, string mobileOrEmail);
    Task<bool> ResetPasswordAsync(string username, string otpCode, string newPassword);
    Task<ForgotUserIdResultDto> ForgotUserIdAsync(string emailOrMobile);
}

public interface IOtpService
{
    Task<string> GenerateAndSendOtpAsync(ulong studentId, string purpose, ulong? applicationId);
    Task<bool> VerifyOtpAsync(ulong studentId, string purpose, string otpCode, ulong? applicationId);
}

public interface IMasterDataService
{
    Task<List<AcademicYear>> GetAcademicYearsAsync();
    Task<List<ScholarshipScheme>> GetSchemesAsync(uint? academicYearId);
    Task<List<Category>> GetCategoriesAsync();
    Task<List<Gender>> GetGendersAsync();
    Task<List<Religion>> GetReligionsAsync();
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
    Task<List<ApplicationStatus>> GetApplicationStatusesAsync();
    Task<List<Occupation>> GetOccupationsAsync();
    Task<List<HouseholdCategory>> GetHouseholdCategoriesAsync();
    Task<List<DeprivationCriterion>> GetDeprivationCriteriaAsync();
    Task<List<AdmissionType>> GetAdmissionTypesAsync();
    Task<List<StudyMode>> GetStudyModesAsync();
    Task<List<EducationBoard>> GetEducationBoardsAsync();
    Task<List<DocumentType>> GetDocumentTypesAsync();
}

public class AuthService : IAuthService
{
    private readonly IStudentRepository _studentRepo;
    private readonly IStudentAuthRepository _authRepo;
    private readonly IStudentVaultRepository _vaultRepo;
    private readonly IAddressRepository _addressRepo;
    private readonly IPasswordHasher _hasher;
    private readonly ICryptoService _crypto;
    private readonly IHmacBlindHasher _blindHasher;
    private readonly IJwtTokenService _jwt;
    private readonly IOtpService _otpService;

    public AuthService(
        IStudentRepository studentRepo,
        IStudentAuthRepository authRepo,
        IStudentVaultRepository vaultRepo,
        IAddressRepository addressRepo,
        IPasswordHasher hasher,
        ICryptoService crypto,
        IHmacBlindHasher blindHasher,
        IJwtTokenService jwt,
        IOtpService otpService)
    {
        _studentRepo = studentRepo;
        _authRepo = authRepo;
        _vaultRepo = vaultRepo;
        _addressRepo = addressRepo;
        _hasher = hasher;
        _crypto = crypto;
        _blindHasher = blindHasher;
        _jwt = jwt;
        _otpService = otpService;
    }

    public async Task<AuthResultDto> RegisterStudentAsync(StudentRegisterDto dto)
    {
        // 1. Validate matching confirm fields (only if provided)
        if (!string.IsNullOrEmpty(dto.ConfirmFirstName) && dto.FirstName.Trim() != dto.ConfirmFirstName.Trim())
            throw new ValidationAppException("First Name and Confirm First Name do not match.");
        if (dto.Password != dto.ConfirmPassword)
            throw new ValidationAppException("Password and Confirm Password do not match.");

        // 2. Validate uniqueness via blind indexing
        byte[] aadhaarHash = _blindHasher.ComputeHash(dto.AadhaarNumber);
        if (await _vaultRepo.ExistsAadhaarHashAsync(aadhaarHash))
            throw new ValidationAppException("Aadhaar Number is already registered in the system.");

        byte[] mobileHash = _blindHasher.ComputeHash(dto.MobileNumber);
        if (await _vaultRepo.ExistsMobileHashAsync(mobileHash))
            throw new ValidationAppException("Mobile Number is already registered in the system.");

        byte[] emailHash = _blindHasher.ComputeHash(dto.Email);
        if (await _vaultRepo.ExistsEmailHashAsync(emailHash))
            throw new ValidationAppException("Email Address is already registered in the system.");

        // 3. Generate or validate User ID: 11-digit unique numeric ID (first digit 1-9, remaining 0-9)
        string userName = dto.UserName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(userName))
        {
            // Auto-generate unique 11-digit numeric User ID (first digit 1-9, next 10 digits 0-9)
            for (int attempt = 0; attempt < 10; attempt++)
            {
                int firstDigit = RandomNumberGenerator.GetInt32(1, 10); // 1-9 (never 0)
                int part1 = RandomNumberGenerator.GetInt32(0, 100000);   // 5 digits
                int part2 = RandomNumberGenerator.GetInt32(0, 100000);   // 5 digits
                string candidate = $"{firstDigit}{part1:D5}{part2:D5}";
                var exists = await _authRepo.GetByUsernameAsync(candidate);
                if (exists == null)
                {
                    userName = candidate;
                    break;
                }
            }
            if (string.IsNullOrWhiteSpace(userName))
            {
                int firstDigit = RandomNumberGenerator.GetInt32(1, 10);
                string epoch = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 10000000000L).ToString("D10");
                userName = $"{firstDigit}{epoch}";
            }
        }
        else
        {
            var existingUser = await _authRepo.GetByUsernameAsync(userName);
            if (existingUser != null)
                throw new ValidationAppException("User ID is already taken. Please choose another.");
        }

        // Student registration code
        string studentCode = userName;

        // 4. Create Student Master Record
        var student = new Student
        {
            StudentCode = studentCode,
            FirstName = dto.FirstName.Trim(),
            MiddleName = dto.MiddleName?.Trim(),
            LastName = dto.LastName?.Trim(),
            DateOfBirth = dto.DateOfBirth,
            GenderId = dto.GenderId,
            CategoryId = dto.CategoryId,
            ReligionId = dto.ReligionId,
            IsActive = true
        };
        ulong studentId = await _studentRepo.CreateStudentAsync(student);

        // 5. Create Student Credentials
        byte[] passwordHash = _hasher.HashPassword(dto.Password);
        var credential = new StudentCredential
        {
            StudentId = studentId,
            UserName = userName,
            PasswordHash = passwordHash,
            PasswordAlgorithm = "Argon2id"
        };
        await _authRepo.CreateCredentialAsync(credential);

        // 6. Save Encrypted Aadhaar in Vault
        var aadhaar = new StudentAadhaar
        {
            StudentId = studentId,
            AadhaarEncrypted = _crypto.Encrypt(dto.AadhaarNumber),
            AadhaarHash = aadhaarHash,
            MaskedAadhaar = MaskingHelper.MaskAadhaar(dto.AadhaarNumber),
            VerificationStatus = "VERIFIED",
            VerifiedAt = DateTime.UtcNow,
            ConsentGiven = dto.AadhaarConsent
        };
        await _vaultRepo.SaveAadhaarAsync(aadhaar);

        // 7. Save Encrypted Contacts
        var contact = new StudentContact
        {
            StudentId = studentId,
            MobileEncrypted = _crypto.Encrypt(dto.MobileNumber),
            MobileHash = mobileHash,
            EmailEncrypted = _crypto.Encrypt(dto.Email),
            EmailHash = emailHash,
            AlternateMobileEncrypted = !string.IsNullOrEmpty(dto.AlternateMobile) ? _crypto.Encrypt(dto.AlternateMobile) : null,
            AlternateEmailEncrypted = !string.IsNullOrEmpty(dto.AlternateEmail) ? _crypto.Encrypt(dto.AlternateEmail) : null,
            IsVerified = true
        };
        await _vaultRepo.SaveContactAsync(contact);

        // 8. Save Family Details
        var family = new StudentFamilyDetail
        {
            StudentId = studentId,
            FatherGuardianName = dto.FatherGuardianName.Trim(),
            MotherName = dto.MotherName.Trim(),
            IsOrphan = dto.IsOrphan,
            IsMotherSingleWoman = dto.IsMotherSingleWoman
        };
        await _vaultRepo.SaveFamilyDetailsAsync(family);

        // 9. Save Permanent Address
        var address = new Address
        {
            StudentId = studentId,
            AddressType = AppAddressTypes.Permanent,
            AddressLine = dto.AddressLine.Trim(),
            Pincode = dto.Pincode.Trim(),
            StateId = dto.StateId,
            DistrictId = dto.DistrictId,
            BlockId = dto.BlockId,
            VidhansabhaId = dto.VidhansabhaId,
            CityVillageId = dto.CityVillageId,
            PostOfficeId = dto.PostOfficeId
        };
        await _addressRepo.SaveAddressAsync(address);

        // 10. Generate JWT
        string token = _jwt.GenerateAccessToken(studentId, userName, AppRoles.Student, studentCode: studentCode);
        string refreshToken = _jwt.GenerateRefreshToken();

        return new AuthResultDto
        {
            Token = token,
            RefreshToken = refreshToken,
            Role = AppRoles.Student,
            UserName = userName,
            FullName = $"{dto.FirstName} {dto.LastName}".Trim(),
            StudentCode = studentCode,
            StudentId = studentId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }

    public async Task<AuthResultDto> LoginStudentAsync(StudentLoginDto dto)
    {
        var credential = await _authRepo.GetByUsernameAsync(dto.UserName);
        if (credential == null)
            throw new UnauthorizedAppException("Invalid Username or Password.");

        if (credential.IsLocked && credential.LockedUntil.HasValue && credential.LockedUntil.Value > DateTime.UtcNow)
        {
            throw new UnauthorizedAppException($"Account is temporarily locked due to failed attempts until {credential.LockedUntil.Value:HH:mm} UTC.");
        }

        bool isValid = _hasher.VerifyPassword(dto.Password, credential.PasswordHash);
        await _authRepo.RecordLoginAttemptAsync(credential.CredentialId, isValid);

        if (!isValid)
            throw new UnauthorizedAppException("Invalid Username or Password.");

        var student = await _studentRepo.GetByIdAsync(credential.StudentId);
        string studentCode = student?.StudentCode ?? string.Empty;
        string fullName = student != null ? $"{student.FirstName} {student.LastName}".Trim() : string.Empty;

        string token = _jwt.GenerateAccessToken(credential.StudentId, credential.UserName, AppRoles.Student, studentCode: studentCode);
        string refreshToken = _jwt.GenerateRefreshToken();

        return new AuthResultDto
        {
            Token = token,
            RefreshToken = refreshToken,
            Role = AppRoles.Student,
            UserName = credential.UserName,
            FullName = fullName,
            StudentCode = studentCode,
            StudentId = credential.StudentId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }

    public async Task<AuthResultDto> LoginOfficialAsync(OfficialLoginDto dto)
    {
        var user = await _authRepo.GetOfficialUserByUsernameAsync(dto.UserName);
        if (user == null)
            throw new UnauthorizedAppException("Invalid Official Username or Password.");

        bool isValid = _hasher.VerifyPassword(dto.Password, user.PasswordHash);
        if (!isValid)
            throw new UnauthorizedAppException("Invalid Official Username or Password.");

        var roles = await _authRepo.GetUserRolesAsync(user.UserId);
        string primaryRole = roles.FirstOrDefault() ?? AppRoles.Verifier;

        ulong? instituteId = null;
        if (primaryRole == AppRoles.InstituteAdmin)
        {
            var instUser = await _authRepo.GetInstituteUserByUserIdAsync(user.UserId);
            instituteId = instUser?.InstituteId;
        }

        string token = _jwt.GenerateAccessToken(user.UserId, user.UserName, primaryRole, instituteId: instituteId);
        string refreshToken = _jwt.GenerateRefreshToken();

        return new AuthResultDto
        {
            Token = token,
            RefreshToken = refreshToken,
            Role = primaryRole,
            UserName = user.UserName,
            UserId = user.UserId,
            InstituteId = instituteId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }

    public async Task<string> ForgotPasswordAsync(string username, string mobileOrEmail)
    {
        var credential = await _authRepo.GetByUsernameAsync(username)
            ?? throw new NotFoundException("Student user account not found.");

        var contact = await _vaultRepo.GetContactByStudentIdAsync(credential.StudentId)
            ?? throw new NotFoundException("No contact information registered for this student.");

        byte[] inputHash = _blindHasher.ComputeHash(mobileOrEmail);
        bool matchesMobile = CryptographicOperations.FixedTimeEquals(contact.MobileHash, inputHash);
        bool matchesEmail = CryptographicOperations.FixedTimeEquals(contact.EmailHash, inputHash);

        if (!matchesMobile && !matchesEmail)
            throw new ValidationAppException("The provided mobile number or email address does not match our records.");

        return await _otpService.GenerateAndSendOtpAsync(credential.StudentId, "PASSWORD_RESET", null);
    }

    public async Task<bool> ResetPasswordAsync(string username, string otpCode, string newPassword)
    {
        var credential = await _authRepo.GetByUsernameAsync(username)
            ?? throw new NotFoundException("Student user account not found.");

        bool isVerified = await _otpService.VerifyOtpAsync(credential.StudentId, "PASSWORD_RESET", otpCode, null);
        if (!isVerified)
            throw new ValidationAppException("Invalid or expired OTP code.");

        byte[] newPasswordHash = _hasher.HashPassword(newPassword);
        await _authRepo.UpdatePasswordAsync(credential.StudentId, newPasswordHash);
        return true;
    }

    public async Task<ForgotUserIdResultDto> ForgotUserIdAsync(string emailOrMobile)
    {
        if (string.IsNullOrWhiteSpace(emailOrMobile))
            throw new ValidationAppException("Please enter your registered Email ID or Mobile Number.");

        string trimmedInput = emailOrMobile.Trim();
        byte[] contactHash = _blindHasher.ComputeHash(trimmedInput);

        ulong? studentId = await _vaultRepo.GetStudentIdByContactHashAsync(contactHash);
        if (!studentId.HasValue)
            throw new NotFoundException("No registered student account found with the provided Email ID or Mobile Number.");

        var credential = await _authRepo.GetByStudentIdAsync(studentId.Value)
            ?? throw new NotFoundException("No login credentials found for this account.");

        var student = await _studentRepo.GetByIdAsync(studentId.Value);
        var contact = await _vaultRepo.GetContactByStudentIdAsync(studentId.Value);

        string? maskedEmail = null;
        string? maskedMobile = null;
        if (contact != null)
        {
            try
            {
                string decryptedEmail = _crypto.Decrypt(contact.EmailEncrypted);
                maskedEmail = MaskingHelper.MaskEmail(decryptedEmail);
            }
            catch { }

            try
            {
                string decryptedMobile = _crypto.Decrypt(contact.MobileEncrypted);
                maskedMobile = MaskingHelper.MaskMobile(decryptedMobile);
            }
            catch { }
        }

        return new ForgotUserIdResultDto
        {
            UserName = credential.UserName,
            StudentCode = student?.StudentCode ?? credential.UserName,
            FullName = student != null ? $"{student.FirstName} {student.LastName}".Trim() : null,
            MaskedEmail = maskedEmail,
            MaskedMobile = maskedMobile
        };
    }
}

public class OtpService : IOtpService
{
    private readonly IOtpRepository _otpRepo;
    private readonly IHmacBlindHasher _hasher;

    public OtpService(IOtpRepository otpRepo, IHmacBlindHasher hasher)
    {
        _otpRepo = otpRepo;
        _hasher = hasher;
    }

    public async Task<string> GenerateAndSendOtpAsync(ulong studentId, string purpose, ulong? applicationId)
    {
        // Generate cryptographically secure 6-digit numeric OTP
        string otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        byte[] otpHash = _hasher.ComputeHash(otpCode);

        var otp = new OtpTransaction
        {
            StudentId = studentId,
            ApplicationId = applicationId,
            Purpose = purpose,
            OtpHash = otpHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            MaxAttempts = 3,
            IsUsed = false
        };

        await _otpRepo.CreateOtpAsync(otp);

        // In production, integrate with NIC SMS Gateway.
        // For development, OTP is returned or simulated.
        return otpCode;
    }

    public async Task<bool> VerifyOtpAsync(ulong studentId, string purpose, string otpCode, ulong? applicationId)
    {
        var activeOtp = await _otpRepo.GetLatestActiveOtpAsync(studentId, purpose, applicationId);
        if (activeOtp == null)
            throw new ValidationAppException("OTP has expired or does not exist. Please request a new OTP.");

        if (activeOtp.AttemptCount >= activeOtp.MaxAttempts)
            throw new ValidationAppException("Maximum OTP verification attempts exceeded.");

        byte[] candidateHash = _hasher.ComputeHash(otpCode);
        bool matches = CryptographicOperations.FixedTimeEquals(activeOtp.OtpHash, candidateHash);

        if (!matches)
        {
            await _otpRepo.IncrementAttemptCountAsync(activeOtp.OtpTransactionId);
            throw new ValidationAppException("Invalid OTP code. Please try again.");
        }

        await _otpRepo.MarkOtpUsedAsync(activeOtp.OtpTransactionId);
        return true;
    }
}

public class MasterDataService : IMasterDataService
{
    private readonly IMasterDataRepository _repo;
    private readonly IMemoryCache _cache;

    public MasterDataService(IMasterDataRepository repo, IMemoryCache cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public Task<List<AcademicYear>> GetAcademicYearsAsync() =>
        _cache.GetOrCreateAsync("ay", e => { e.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12); return _repo.GetAcademicYearsAsync(); })!;

    public Task<List<ScholarshipScheme>> GetSchemesAsync(uint? academicYearId) => _repo.GetSchemesAsync(academicYearId);
    public Task<List<Category>> GetCategoriesAsync() => _repo.GetCategoriesAsync();
    public Task<List<Gender>> GetGendersAsync() => _repo.GetGendersAsync();
    public Task<List<Religion>> GetReligionsAsync() => _repo.GetReligionsAsync();
    public Task<List<State>> GetStatesAsync() => _repo.GetStatesAsync();
    public Task<List<District>> GetDistrictsAsync(ulong stateId) => _repo.GetDistrictsAsync(stateId);
    public Task<List<Block>> GetBlocksAsync(ulong districtId) => _repo.GetBlocksAsync(districtId);
    public Task<List<Vidhansabha>> GetVidhansabhasAsync(ulong districtId) => _repo.GetVidhansabhasAsync(districtId);
    public Task<List<CityVillage>> GetCitiesVillagesAsync(ulong districtId, ulong? blockId) => _repo.GetCitiesVillagesAsync(districtId, blockId);
    public Task<List<PostOffice>> GetPostOfficesAsync(ulong districtId) => _repo.GetPostOfficesAsync(districtId);
    public Task<List<Bank>> GetBanksAsync() => _repo.GetBanksAsync();
    public Task<List<BankBranch>> GetBankBranchesAsync(ulong bankId) => _repo.GetBankBranchesAsync(bankId);
    public Task<BankBranch?> GetBranchByIfscAsync(string ifsc) => _repo.GetBranchByIfscAsync(ifsc);
    public Task<List<CourseType>> GetCourseTypesAsync() => _repo.GetCourseTypesAsync();
    public Task<List<Course>> GetCoursesAsync(uint? courseTypeId) => _repo.GetCoursesAsync(courseTypeId);
    public Task<List<CourseBranch>> GetCourseBranchesAsync(ulong courseId) => _repo.GetCourseBranchesAsync(courseId);
    public Task<List<Institute>> GetInstitutesAsync(ulong districtId) => _repo.GetInstitutesAsync(districtId);
    public Task<List<InstituteCourse>> GetInstituteCoursesAsync(ulong instituteId) => _repo.GetInstituteCoursesAsync(instituteId);
    public Task<List<ApplicationStatus>> GetApplicationStatusesAsync() => _repo.GetApplicationStatusesAsync();
    public Task<List<Occupation>> GetOccupationsAsync() => _repo.GetOccupationsAsync();
    public Task<List<HouseholdCategory>> GetHouseholdCategoriesAsync() => _repo.GetHouseholdCategoriesAsync();
    public Task<List<DeprivationCriterion>> GetDeprivationCriteriaAsync() => _repo.GetDeprivationCriteriaAsync();
    public Task<List<AdmissionType>> GetAdmissionTypesAsync() => _repo.GetAdmissionTypesAsync();
    public Task<List<StudyMode>> GetStudyModesAsync() => _repo.GetStudyModesAsync();
    public Task<List<EducationBoard>> GetEducationBoardsAsync() => _repo.GetEducationBoardsAsync();
    public Task<List<DocumentType>> GetDocumentTypesAsync() => _repo.GetDocumentTypesAsync();
}
