using System.ComponentModel.DataAnnotations;

namespace Scholarship.Api.DTOs;

public class StudentRegisterDto
{
    [Required] public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? ConfirmFirstName { get; set; }
    public string? ConfirmMiddleName { get; set; }
    public string? ConfirmLastName { get; set; }

    [Required] public bool IsOrphan { get; set; }
    [Required] public string FatherGuardianName { get; set; } = string.Empty;
    [Required] public string MotherName { get; set; } = string.Empty;
    public bool IsMotherSingleWoman { get; set; }

    [Required] public DateTime DateOfBirth { get; set; }
    [Required] public uint CategoryId { get; set; }
    [Required] public uint GenderId { get; set; }
    [Required] public uint ReligionId { get; set; }

    [Required] public string AadhaarNumber { get; set; } = string.Empty;
    [Required] public string MobileNumber { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    public string? AlternateMobile { get; set; }
    public string? AlternateEmail { get; set; }
    [Required] public bool AadhaarConsent { get; set; }

    // Address
    [Required] public string AddressLine { get; set; } = string.Empty;
    [Required] public string Pincode { get; set; } = string.Empty;
    [Required] public ulong StateId { get; set; }
    [Required] public ulong DistrictId { get; set; }
    public ulong? BlockId { get; set; }
    public ulong? VidhansabhaId { get; set; }
    public ulong? CityVillageId { get; set; }
    public ulong? PostOfficeId { get; set; }

    // Credentials
    public string? UserName { get; set; }
    [Required, MinLength(8)] public string Password { get; set; } = string.Empty;
    [Required] public string ConfirmPassword { get; set; } = string.Empty;
}

public class ForgotUserIdDto
{
    [Required] public string EmailOrMobile { get; set; } = string.Empty;
}

public class ForgotUserIdResultDto
{
    public string UserName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? MaskedEmail { get; set; }
    public string? MaskedMobile { get; set; }
}

public class StudentLoginDto
{
    [Required] public string UserName { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
    public string? CaptchaToken { get; set; }
    public string? CaptchaInput { get; set; }
}

public class OfficialLoginDto
{
    [Required] public string UserName { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
    public string? CaptchaToken { get; set; }
    public string? CaptchaInput { get; set; }
}

public class AuthResultDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? StudentCode { get; set; }
    public ulong? StudentId { get; set; }
    public ulong? UserId { get; set; }
    public ulong? InstituteId { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class OtpRequestDto
{
    [Required] public ulong StudentId { get; set; }
    [Required] public string Purpose { get; set; } = "APPLICATION_LOCK";
    public ulong? ApplicationId { get; set; }
}

public class OtpVerifyDto
{
    [Required] public ulong StudentId { get; set; }
    [Required] public string Purpose { get; set; } = "APPLICATION_LOCK";
    [Required] public string OtpCode { get; set; } = string.Empty;
    public ulong? ApplicationId { get; set; }
}

public class ForgotPasswordDto
{
    [Required] public string UserName { get; set; } = string.Empty;
    [Required] public string MobileOrEmail { get; set; } = string.Empty;
    public string? OtpCode { get; set; }
    public string? NewPassword { get; set; }
}
