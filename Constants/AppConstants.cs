namespace Scholarship.Api.Constants;

public static class AppRoles
{
    public const string SuperAdmin = "SUPER_ADMIN";
    public const string DepartmentAdmin = "DEPARTMENT_ADMIN";
    public const string DistrictAdmin = "DISTRICT_ADMIN";
    public const string InstituteAdmin = "INSTITUTE_ADMIN";
    public const string Verifier = "VERIFIER";
    public const string PaymentAdmin = "PAYMENT_ADMIN";
    public const string Student = "STUDENT";

    // Aliases
    public const string DistrictOfficer = "DISTRICT_ADMIN";
    public const string Admin = "DEPARTMENT_ADMIN";
}

public static class AppStatuses
{
    public const int Draft = 1;
    public const int Submitted = 2;
    public const int OtpPending = 3;
    public const int Locked = 4;
    public const int InstituteVerification = 5;
    public const int DistrictVerification = 6;
    public const int Approved = 7;
    public const int Rejected = 8;
    public const int TemporaryRejected = 9; // Defective / Revert
    public const int PaymentPending = 10;
    public const int PaymentProcessing = 11;
    public const int Paid = 12;
    public const int Cancelled = 13;

    public const string CodeDraft = "DRAFT";
    public const string CodeLocked = "LOCKED";
    public const string CodeInstituteVerification = "INSTITUTE_VERIFICATION";
    public const string CodeDistrictVerification = "DISTRICT_VERIFICATION";
    public const string CodeApproved = "APPROVED";
    public const string CodeRejected = "REJECTED";
    public const string CodeDefective = "TEMPORARY_REJECTED";
}

public static class AppDocumentTypes
{
    public const int StudentPhoto = 1;
    public const int TenthMarksheet = 2;
    public const int PreviousMarksheet = 3;
    public const int BankPassbook = 4;
    public const int CasteCertificate = 5;
    public const int DomicileCertificate = 6;
    public const int IncomeCertificate = 7;
    public const int DifferentlyAbledCertificate = 8;
    public const int FeeReceipt = 9;
    public const int Other = 10;
}

public static class AppAddressTypes
{
    public const string Permanent = "PERMANENT";
    public const string Correspondence = "CORRESPONDENCE";
}
