namespace Scholarship.Api.Models;

public class AcademicYear
{
    public uint AcademicYearId { get; set; }
    public string AcademicYearCode { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class ScholarshipScheme
{
    public ulong SchemeId { get; set; }
    public string SchemeCode { get; set; } = string.Empty;
    public string SchemeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public uint? CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class Category
{
    public uint CategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class Gender
{
    public uint GenderId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class Religion
{
    public uint ReligionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class State
{
    public ulong StateId { get; set; }
    public string StateCode { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class District
{
    public ulong DistrictId { get; set; }
    public ulong StateId { get; set; }
    public string DistrictCode { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class Block
{
    public ulong BlockId { get; set; }
    public ulong DistrictId { get; set; }
    public string BlockCode { get; set; } = string.Empty;
    public string BlockName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class Vidhansabha
{
    public ulong VidhansabhaId { get; set; }
    public ulong StateId { get; set; }
    public ulong DistrictId { get; set; }
    public string VidhansabhaCode { get; set; } = string.Empty;
    public string VidhansabhaName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class CityVillage
{
    public ulong CityVillageId { get; set; }
    public ulong DistrictId { get; set; }
    public ulong? BlockId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "VILLAGE";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class PostOffice
{
    public ulong PostOfficeId { get; set; }
    public ulong DistrictId { get; set; }
    public string PostOfficeCode { get; set; } = string.Empty;
    public string PostOfficeName { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class Bank
{
    public ulong BankId { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class BankBranch
{
    public ulong BranchId { get; set; }
    public ulong BankId { get; set; }
    public string IFSCCode { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class CourseType
{
    public uint CourseTypeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class Course
{
    public ulong CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public decimal DurationYears { get; set; }
    public uint CourseTypeId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class CourseBranch
{
    public ulong BranchId { get; set; }
    public ulong CourseId { get; set; }
    public string BranchCode { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class Institute
{
    public ulong InstituteId { get; set; }
    public string InstituteCode { get; set; } = string.Empty;
    public string InstituteName { get; set; } = string.Empty;
    public uint InstituteTypeId { get; set; }
    public ulong StateId { get; set; }
    public ulong DistrictId { get; set; }
    public string? Address { get; set; }
    public bool IsGovernment { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class InstituteCourse
{
    public ulong InstituteCourseId { get; set; }
    public ulong InstituteId { get; set; }
    public ulong CourseId { get; set; }
    public ulong? BranchId { get; set; }
    public string? CourseCode { get; set; }
    public decimal DurationYears { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class ApplicationStatus
{
    public uint ApplicationStatusId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class DocumentType
{
    public uint DocumentTypeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ulong MaxSizeBytes { get; set; }
    public string AllowedMimeTypes { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class Occupation
{
    public uint OccupationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class HouseholdCategory
{
    public uint HouseholdCategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class DeprivationCriterion
{
    public uint CriterionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class AdmissionType
{
    public uint AdmissionTypeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class StudyMode
{
    public uint StudyModeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class EducationBoard
{
    public uint BoardId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
