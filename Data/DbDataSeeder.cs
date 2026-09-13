using Dapper;
using Scholarship.Api.Constants;
using Scholarship.Api.Security;

namespace Scholarship.Api.Data;

public class DbDataSeeder
{
    private readonly IDbConnectionFactory _db;
    private readonly IPasswordHasher _hasher;

    public DbDataSeeder(IDbConnectionFactory db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task SeedAsync()
    {
        using var conn = await _db.CreateConnectionAsync();

        // 1. Seed State: Chhattisgarh if not exists
        int stateCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM states;");
        if (stateCount == 0)
        {
            await conn.ExecuteAsync(@"
                INSERT INTO states (StateId, StateCode, StateName, IsActive, CreatedAt)
                VALUES (1, 'CG', 'Chhattisgarh', 1, CURRENT_TIMESTAMP)
                ON DUPLICATE KEY UPDATE StateName=VALUES(StateName);");
        }

        // 2. Seed Districts if not exists
        int districtCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM districts;");
        if (districtCount == 0)
        {
            await conn.ExecuteAsync(@"
                INSERT INTO districts (DistrictId, StateId, DistrictCode, DistrictName, IsActive, CreatedAt) VALUES
                (1, 1, 'RAIPUR', 'RAIPUR', 1, CURRENT_TIMESTAMP),
                (2, 1, 'DURG', 'DURG', 1, CURRENT_TIMESTAMP),
                (3, 1, 'BILASPUR', 'BILASPUR', 1, CURRENT_TIMESTAMP),
                (4, 1, 'BALOD', 'BALOD', 1, CURRENT_TIMESTAMP),
                (5, 1, 'DHAMTARI', 'DHAMTARI', 1, CURRENT_TIMESTAMP),
                (6, 1, 'RAJNANDGAON', 'RAJNANDGAON', 1, CURRENT_TIMESTAMP),
                (7, 1, 'JANJGIR-CHAMPA', 'JANJGIR-CHAMPA', 1, CURRENT_TIMESTAMP),
                (8, 1, 'KORBA', 'KORBA', 1, CURRENT_TIMESTAMP),
                (9, 1, 'BASTAR', 'BASTAR', 1, CURRENT_TIMESTAMP),
                (10, 1, 'SURGUJA', 'SURGUJA', 1, CURRENT_TIMESTAMP);");
        }

        // 3. Seed Blocks for Raipur if not exists
        int blockCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM blocks;");
        if (blockCount == 0)
        {
            await conn.ExecuteAsync(@"
                INSERT INTO blocks (BlockId, DistrictId, BlockCode, BlockName, IsActive, CreatedAt) VALUES
                (1, 1, 'DHARSIWA', 'DHARSIWA', 1, CURRENT_TIMESTAMP),
                (2, 1, 'ARANG', 'ARANG', 1, CURRENT_TIMESTAMP),
                (3, 1, 'ABHANPUR', 'ABHANPUR', 1, CURRENT_TIMESTAMP),
                (4, 1, 'TILDA', 'TILDA', 1, CURRENT_TIMESTAMP);");
        }

        // 4. Seed Vidhansabhas for Raipur if not exists
        int vidhansabhaCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM vidhansabhas;");
        if (vidhansabhaCount == 0)
        {
            await conn.ExecuteAsync(@"
                INSERT INTO vidhansabhas (VidhansabhaId, StateId, DistrictId, VidhansabhaCode, VidhansabhaName, IsActive, CreatedAt) VALUES
                (1, 1, 1, 'RAIPUR_GRAMIN', 'Raipur Gramin', 1, CURRENT_TIMESTAMP),
                (2, 1, 1, 'RAIPUR_CITY_NORTH', 'Raipur City North', 1, CURRENT_TIMESTAMP),
                (3, 1, 1, 'RAIPUR_CITY_SOUTH', 'Raipur City South', 1, CURRENT_TIMESTAMP),
                (4, 1, 1, 'RAIPUR_CITY_WEST', 'Raipur City West', 1, CURRENT_TIMESTAMP),
                (5, 1, 1, 'ABHANPUR', 'Abhanpur', 1, CURRENT_TIMESTAMP),
                (6, 1, 1, 'ARANG', 'Arang', 1, CURRENT_TIMESTAMP),
                (7, 1, 1, 'DHARSIWA', 'Dharsiwa', 1, CURRENT_TIMESTAMP);");
        }

        // 5. Seed Cities / Villages if not exists
        int cityCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM cities_villages;");
        if (cityCount == 0)
        {
            await conn.ExecuteAsync(@"
                INSERT INTO cities_villages (CityVillageId, DistrictId, BlockId, Name, Type, IsActive, CreatedAt) VALUES
                (1, 1, 1, 'SONDRA', 'VILLAGE', 1, CURRENT_TIMESTAMP),
                (2, 1, 1, 'DHARSIWA', 'TOWN', 1, CURRENT_TIMESTAMP),
                (3, 1, 2, 'ARANG', 'TOWN', 1, CURRENT_TIMESTAMP),
                (4, 1, 3, 'ABHANPUR', 'TOWN', 1, CURRENT_TIMESTAMP),
                (5, 1, 4, 'TILDA NEORA', 'TOWN', 1, CURRENT_TIMESTAMP);");
        }

        // 6. Seed Post Offices if not exists
        int poCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM post_offices;");
        if (poCount == 0)
        {
            await conn.ExecuteAsync(@"
                INSERT INTO post_offices (PostOfficeId, DistrictId, PostOfficeCode, PostOfficeName, Pincode, IsActive, CreatedAt) VALUES
                (1, 1, 'PO493221', 'SONDRA B.O', '493221', 1, CURRENT_TIMESTAMP),
                (2, 1, 'PO492001', 'RAIPUR G.P.O', '492001', 1, CURRENT_TIMESTAMP),
                (3, 1, 'PO493441', 'ARANG S.O', '493441', 1, CURRENT_TIMESTAMP),
                (4, 1, 'PO493661', 'ABHANPUR S.O', '493661', 1, CURRENT_TIMESTAMP),
                (5, 1, 'PO493114', 'TILDA S.O', '493114', 1, CURRENT_TIMESTAMP);");
        }

        // 7. Seed Banks if not exists
        int bankCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM banks;");
        if (bankCount == 0)
        {
            await conn.ExecuteAsync(@"
                INSERT INTO banks (BankId, BankCode, BankName, IsActive, CreatedAt) VALUES
                (1, 'SBI', 'STATE BANK OF INDIA', 1, CURRENT_TIMESTAMP),
                (2, 'PNB', 'PUNJAB NATIONAL BANK', 1, CURRENT_TIMESTAMP),
                (3, 'BOB', 'BANK OF BARODA', 1, CURRENT_TIMESTAMP),
                (4, 'CRGB', 'CHHATTISGARH RAJYA GRAMIN BANK', 1, CURRENT_TIMESTAMP),
                (5, 'HDFC', 'HDFC BANK LTD', 1, CURRENT_TIMESTAMP),
                (6, 'BOI', 'BANK OF INDIA', 1, CURRENT_TIMESTAMP),
                (7, 'CANARA', 'CANARA BANK', 1, CURRENT_TIMESTAMP),
                (8, 'UNION', 'UNION BANK OF INDIA', 1, CURRENT_TIMESTAMP),
                (9, 'AXIS', 'AXIS BANK', 1, CURRENT_TIMESTAMP),
                (10, 'ICICI', 'ICICI BANK LTD', 1, CURRENT_TIMESTAMP);");
        }

        // 8. Seed Bank Branches if not exists
        int branchCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM bank_branches;");
        if (branchCount == 0)
        {
            await conn.ExecuteAsync(@"
                INSERT INTO bank_branches (BranchId, BankId, IFSCCode, BranchName, Address, IsActive, CreatedAt) VALUES
                (1, 1, 'SBIN0002852', 'GCET, RAIPUR', 'GOVT COLLEGE OF ENGG CAMPUS, SEJBAHAR, RAIPUR', 1, CURRENT_TIMESTAMP),
                (2, 1, 'SBIN0000461', 'RAIPUR MAIN BRANCH', 'JAISTAMBH CHOWK, RAIPUR', 1, CURRENT_TIMESTAMP),
                (3, 4, 'CRGB0001002', 'DHARSIWA BRANCH', 'MAIN ROAD, DHARSIWA, RAIPUR', 1, CURRENT_TIMESTAMP),
                (4, 2, 'PUNB0123400', 'KATORA TALAB BRANCH', 'KATORA TALAB, RAIPUR', 1, CURRENT_TIMESTAMP);");
        }

        // 9. Seed Courses if not exists
        int courseCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM courses;");
        if (courseCount == 0)
        {
            await conn.ExecuteAsync(@"
                INSERT INTO courses (CourseId, CourseCode, CourseName, DurationYears, CourseTypeId, IsActive, CreatedAt) VALUES
                (1, '10356', 'PGDCA (1 Year Course)', 1.0, 9, 1, CURRENT_TIMESTAMP),
                (2, '10101', 'B. Sc.', 3.0, 5, 1, CURRENT_TIMESTAMP),
                (3, '10102', 'B. Sc. (Hons.)', 3.0, 5, 1, CURRENT_TIMESTAMP),
                (4, '10201', 'B. C. A. (3 Year Course)', 3.0, 4, 1, CURRENT_TIMESTAMP),
                (5, '10301', 'M. Sc.', 2.0, 7, 1, CURRENT_TIMESTAMP),
                (6, '10401', 'B. Tech.', 4.0, 4, 1, CURRENT_TIMESTAMP),
                (7, '10501', 'Diploma in Engineering (Polytechnic)', 3.0, 2, 1, CURRENT_TIMESTAMP);");
        }

        // 10. Seed Course Branches if not exists
        int cBranchCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM course_branches;");
        if (cBranchCount == 0)
        {
            await conn.ExecuteAsync(@"
                INSERT INTO course_branches (BranchId, CourseId, BranchCode, BranchName, IsActive, CreatedAt) VALUES
                (1, 1, 'CA', 'Computer Application (Course Duration- 1 Year)', 1, CURRENT_TIMESTAMP),
                (2, 2, 'PCM', 'Physics, Chemistry, Mathematics', 1, CURRENT_TIMESTAMP),
                (3, 2, 'CBZ', 'Chemistry, Botany, Zoology', 1, CURRENT_TIMESTAMP),
                (4, 4, 'CS', 'Computer Science & Software Systems', 1, CURRENT_TIMESTAMP),
                (5, 6, 'CSE', 'Computer Science and Engineering', 1, CURRENT_TIMESTAMP),
                (6, 6, 'MECH', 'Mechanical Engineering', 1, CURRENT_TIMESTAMP);");
        }

        // 11. Seed Institutes if not exists
        int instCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM institutes;");
        if (instCount == 0)
        {
            await conn.ExecuteAsync(@"
                INSERT INTO institutes (InstituteId, InstituteCode, InstituteName, InstituteTypeId, StateId, DistrictId, Address, IsGovernment, IsActive, CreatedAt) VALUES
                (1, '13870012', 'GOVT. NAGARJUNA P.G. COLLEGE OF SCIENCE, RAIPUR (C.G.)', 1, 1, 1, 'G.E. ROAD, RAIPUR', 1, 1, CURRENT_TIMESTAMP),
                (2, '13870022', 'GOVERNMENT ENGINEERING COLLEGE RAIPUR', 1, 1, 1, 'SEJBAHAR, OLD DHAMTARI ROAD, RAIPUR', 1, 1, CURRENT_TIMESTAMP),
                (3, '13870047', 'PT. RAVISHANKAR SHUKLA UNIVERSITY, RAIPUR', 3, 1, 1, 'AMANAKA, G.E. ROAD, RAIPUR', 1, 1, CURRENT_TIMESTAMP),
                (4, '23870087', 'BHILAI INSTITUTE OF TECHNOLOGY KENDRI, RAIPUR', 2, 1, 1, 'KENDRI, ABHANPUR, RAIPUR', 0, 1, CURRENT_TIMESTAMP);");
        }

        // 12. Seed Institute Courses if not exists
        int instCourseCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM institute_courses;");
        if (instCourseCount == 0)
        {
            await conn.ExecuteAsync(@"
                INSERT INTO institute_courses (InstituteCourseId, InstituteId, CourseId, BranchId, CourseCode, DurationYears, IsActive, CreatedAt) VALUES
                (1, 1, 1, 1, '10356', 1.0, 1, CURRENT_TIMESTAMP),
                (2, 1, 2, 2, '10101', 3.0, 1, CURRENT_TIMESTAMP),
                (3, 2, 6, 5, '10401', 4.0, 1, CURRENT_TIMESTAMP),
                (4, 3, 5, 2, '10301', 2.0, 1, CURRENT_TIMESTAMP);");
        }

        // 13. Seed Scheme Academic Years linkage if not exists
        int schemeAyCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM scheme_academic_years;");
        if (schemeAyCount == 0)
        {
            await conn.ExecuteAsync(@"
                INSERT INTO scheme_academic_years (SchemeAcademicYearId, SchemeId, AcademicYearId, StartDate, EndDate, ApplicationStartDate, ApplicationEndDate, IsActive, CreatedAt) VALUES
                (1, 1, 1, '2024-07-01', '2025-06-30', '2024-07-01', '2025-03-31', 1, CURRENT_TIMESTAMP),
                (2, 2, 1, '2024-07-01', '2025-06-30', '2024-07-01', '2025-03-31', 1, CURRENT_TIMESTAMP),
                (3, 3, 1, '2024-07-01', '2025-06-30', '2024-07-01', '2025-03-31', 1, CURRENT_TIMESTAMP),
                (4, 1, 2, '2025-07-01', '2026-06-30', '2025-07-01', '2026-03-31', 1, CURRENT_TIMESTAMP),
                (5, 2, 2, '2025-07-01', '2026-06-30', '2025-07-01', '2026-03-31', 1, CURRENT_TIMESTAMP),
                (6, 3, 2, '2025-07-01', '2026-06-30', '2025-07-01', '2026-03-31', 1, CURRENT_TIMESTAMP);");
        }

        // 14. Seed Official Users (Argon2id hashed with password: 'System@123')
        int userCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM users;");
        if (userCount == 0)
        {
            byte[] pwdHash = _hasher.HashPassword("System@123");

            // Institute Admin
            ulong instUserId = await conn.ExecuteScalarAsync<ulong>(@"
                INSERT INTO users (UserName, PasswordHash, UserTypeId, IsActive, CreatedAt)
                VALUES ('inst_raipur', @PasswordHash, 1, 1, CURRENT_TIMESTAMP);
                SELECT LAST_INSERT_ID();", new { PasswordHash = pwdHash });

            await conn.ExecuteAsync(@"
                INSERT INTO user_roles (UserId, RoleId, AssignedAt) VALUES (@UserId, 4, CURRENT_TIMESTAMP);
                INSERT INTO institute_users (UserId, InstituteId, IsActive, CreatedAt) VALUES (@UserId, 1, 1, CURRENT_TIMESTAMP);",
                new { UserId = instUserId });

            // District Admin
            ulong distUserId = await conn.ExecuteScalarAsync<ulong>(@"
                INSERT INTO users (UserName, PasswordHash, UserTypeId, IsActive, CreatedAt)
                VALUES ('dist_raipur', @PasswordHash, 1, 1, CURRENT_TIMESTAMP);
                SELECT LAST_INSERT_ID();", new { PasswordHash = pwdHash });

            await conn.ExecuteAsync(@"
                INSERT INTO user_roles (UserId, RoleId, AssignedAt) VALUES (@UserId, 3, CURRENT_TIMESTAMP);",
                new { UserId = distUserId });

            // Super Admin
            ulong adminUserId = await conn.ExecuteScalarAsync<ulong>(@"
                INSERT INTO users (UserName, PasswordHash, UserTypeId, IsActive, CreatedAt)
                VALUES ('admin', @PasswordHash, 1, 1, CURRENT_TIMESTAMP);
                SELECT LAST_INSERT_ID();", new { PasswordHash = pwdHash });

            await conn.ExecuteAsync(@"
                INSERT INTO user_roles (UserId, RoleId, AssignedAt) VALUES (@UserId, 1, CURRENT_TIMESTAMP);",
                new { UserId = adminUserId });
        }
    }
}
