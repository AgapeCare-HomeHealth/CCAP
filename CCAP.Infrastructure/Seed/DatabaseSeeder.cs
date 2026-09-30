using CCAP.Application.Abstractions.Identity;
using CCAP.Domain.Entities;
using CCAP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken = default)
    {

        var permissions = new (string Code, string Name, string Module)[]
        {
            ("dashboard.view", "View Dashboard", "Dashboard"),
            ("users.view", "View Users", "Users"),
            ("users.manage", "Manage Users", "Users"),
            ("roles.view", "View Roles", "Roles"),
            ("roles.manage", "Manage Roles & Permissions", "Roles"),
            ("patients.view", "View Patients", "Patients"),
            ("patients.manage", "Manage Patients", "Patients"),
            ("referrals.view", "View Referrals", "Referrals"),
            ("referrals.manage", "Manage Referrals", "Referrals"),
            ("fax.view", "View Referring Fax Information", "Patient Profile"),
            ("fax.manage", "Manage Referring Fax Information", "Patient Profile"),
            ("notifications.view", "View Patient Notifications", "Patient Profile"),
            ("notifications.manage", "Manage Patient Notifications", "Patient Profile"),
            ("notes.view", "View Patient Notes", "Patient Profile"),
            ("notes.manage", "Manage Patient Notes", "Patient Profile"),
            ("labs.view", "View Lab Orders", "Clinical"),
            ("labs.manage", "Manage Lab Orders", "Clinical"),
            ("supplies.view", "View Wound Supplies", "Clinical"),
            ("supplies.manage", "Manage Wound Supplies", "Clinical"),
            ("foley.view", "View Foley Changes", "Clinical"),
            ("foley.manage", "Manage Foley Changes", "Clinical"),
            ("orders.view", "View Order Alerts", "Orders"),
            ("orders.manage", "Manage Order Alerts", "Orders"),
            ("lookups.view", "View Lookup Options", "Administration"),
            ("lookups.manage", "Manage Lookup Options", "Administration")
        };

        var permissionEntities = new List<Permission>();
        foreach (var item in permissions)
        {
            var existing = await context.Permissions
                .FirstOrDefaultAsync(x => x.PermissionCode == item.Code, cancellationToken);

            if (existing is null)
            {
                existing = new Permission(item.Code, item.Name, item.Module);
                context.Permissions.Add(existing);
            }

            permissionEntities.Add(existing);
        }

        var roleDefinitions = new[]
        {
            ("Administrator", "Full system administration"),
            ("Care Coordinator", "Coordinates referrals and patient care"),
            ("Clinician", "Provides and documents patient care"),
            ("Scheduler", "Manages scheduling and visits")
        };

        var roles = new Dictionary<string, Role>(StringComparer.OrdinalIgnoreCase);
        foreach (var definition in roleDefinitions)
        {
            var role = await context.Roles
                .FirstOrDefaultAsync(x => x.RoleName == definition.Item1, cancellationToken);

            if (role is null)
            {
                role = new Role(definition.Item1, definition.Item2);
                context.Roles.Add(role);
            }

            roles[role.RoleName] = role;
        }

        await context.SaveChangesAsync(cancellationToken);

        // Administrator receives all permissions.
        var adminRole = roles["Administrator"];
        var existingAdminPermissionIds = await context.RolePermissions
            .Where(x => x.RoleId == adminRole.RoleId)
            .Select(x => x.PermissionId)
            .ToListAsync(cancellationToken);

        foreach (var permission in permissionEntities)
        {
            if (!existingAdminPermissionIds.Contains(permission.PermissionId))
                context.RolePermissions.Add(
                    new RolePermission(
                        adminRole.RoleId,
                        permission.PermissionId));
        }

        // Other roles receive only the permissions needed for their initial workflow.
        var rolePermissionCodes = new Dictionary<string, string[]>
        {
            ["Care Coordinator"] =
            ["dashboard.view", "users.view", "roles.view", "patients.view", "patients.manage", "referrals.view", "referrals.manage",
             "fax.view", "fax.manage", "notifications.view", "notifications.manage", "notes.view", "notes.manage",
             "labs.view", "labs.manage", "supplies.view", "supplies.manage", "foley.view", "foley.manage",
             "orders.view", "orders.manage", "lookups.view"],
            ["Clinician"] =
            ["dashboard.view", "patients.view", "patients.manage", "referrals.view", "fax.view", "fax.manage", "notifications.view", "notifications.manage",
             "notes.view", "notes.manage", "labs.view", "labs.manage", "supplies.view", "supplies.manage",
             "foley.view", "foley.manage", "orders.view", "orders.manage"],
            ["Scheduler"] =
            ["dashboard.view", "patients.view", "referrals.view", "referrals.manage"]
        };

        foreach (var pair in rolePermissionCodes)
        {
            var role = roles[pair.Key];

            var existingIds = await context.RolePermissions
                .Where(x => x.RoleId == role.RoleId)
                .Select(x => x.PermissionId)
                .ToListAsync(cancellationToken);

            var selected = permissionEntities
                .Where(p => pair.Value.Contains(
                    p.PermissionCode,
                    StringComparer.OrdinalIgnoreCase));

            foreach (var permission in selected)
            {
                if (!existingIds.Contains(permission.PermissionId))
                    context.RolePermissions.Add(new RolePermission(role.RoleId, permission.PermissionId));
            }
        }

        var disciplines = new[]
        {
            ("RN", "Registered Nurse"),
            ("LVN", "Licensed Vocational Nurse"),
            ("PT", "Physical Therapy"),
            ("OT", "Occupational Therapy"),
            ("ST", "Speech Therapy"),
            ("HHA", "Home Health Aide")
        };

        foreach (var item in disciplines)
        {
            if (!await context.Disciplines.AnyAsync(
                x => x.Code == item.Item1,
                cancellationToken))
            {
                context.Disciplines.Add(
                    new Discipline(item.Item1, item.Item2));
            }
        }

        // Seed active clinical service types used by the workflow UI.
        // Service types remain normalized entities because they are referenced by orders.
        var serviceEntities = new[]
        {
            ("SN", "Skilled Nursing", "bi bi-heart-pulse", "text-danger"),
            ("PT", "Physical Therapy", "bi bi-person-walking", "text-primary"),
            ("OT", "Occupational Therapy", "bi bi-universal-access", "text-success"),
            ("ST", "Speech Therapy", "bi bi-chat-dots", "text-info"),
            ("HHA", "Home Health Aide", "bi bi-person-heart", "text-warning")
        };

        // Remove any accidental empty additions from the first pass and use
        // reflection-free factory support added to ServiceType.
        context.ServiceTypes.RemoveRange(
            context.ServiceTypes.Where(x => string.IsNullOrWhiteSpace(x.Code)));

        foreach (var item in serviceEntities)
        {
            if (!await context.ServiceTypes.AnyAsync(
                x => x.Code == item.Item1,
                cancellationToken))
            {
                context.ServiceTypes.Add(
                    ServiceType.Create(item.Item1, item.Item2, item.Item3, item.Item4));
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        await SeedLookupOptionsAsync(context, cancellationToken);

        var adminEmail = "admin@ccap.local";
        var adminUser = await context.ApplicationUsers
            .FirstOrDefaultAsync(x => x.Email == adminEmail, cancellationToken);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser(
                "CCAP-ADMIN",
                "CCAP",
                "Administrator",
                adminEmail,
                string.Empty,
                adminRole.RoleId,
                null);

            adminUser.SetPasswordHash(
                passwordHasher.HashPassword(adminUser, "Admin123!"));

            context.ApplicationUsers.Add(adminUser);
        }
        else if (!adminUser.IsActive)
        {
            adminUser.Activate();
        }

        if (!await context.Locations.AnyAsync(
        cancellationToken))
        {
            context.Locations.Add(
                new Location(
                    "Main Office",
                    isDefault: true));
        }

        await context.SaveChangesAsync(cancellationToken);
    }
    private static async Task SeedLookupOptionsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var groups = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Gender"] = ["Male", "Female"],
["ReferralSource"] = ["ADVENTIST HEALTH", "American River Center", "Auburn  Oaks Care Center", "Arden Park Post Acute", "Asbury Park Nursing & Rehab Center", "Bruceville Terrace", "Casa Coloma Care Center", "Cedars Sinai", "Cedarwood Post Acute", "Chapa De Indian Health", "CITRUS HEIGHTS POST ACUTE", "CLINIC FARHANGFAR", "CPMC", "Dignity Health", "Doctors Medical Center of Modesto", "DR BISLA  OFFICE", "Dr Gill office", "Fair Oaks Care Center", "Greenhaven Center", "Kaiser South Sac", "LINCOLN MEADOWS", "Lincoln Meadows Care Center", "McKinley Park Care Center", "Mercy General", "Mercy Hospital of Folsom", "Mercy General Hospital Sacramento", "Mercy San Juan Medical Center", "Methodist Hospital", "Mission Carmichael", "Oak Ridge Health Center", "PCP Office", "PINE CREEK CARE CENTER", "Peach Tree Health", "RCC", "RCC/Email", "River Pointe (Email)", "Rocklin Family Practice Sports (Ana)", "ROSEVILLE CARE CENTER", "Roseville Point Health & Wellness Center", "SACRAMENTO", "Sac Medical Center", "Sacramento Rehab Hospital", "Sherwood Healthcare Center", "Sienna Skilled", "Sierra Nevada Memorial Hospital", "STANFORD HEALTH CARE", "Sutter", "Sutter  Roseville/Allscripts", "SUTTER AUBURN", "Sutter Auburn/Allscripts", "SUTTER AUBURNFAITH", "SUTTER DAVIS", "Sutter Faith Auburn", "SUTTER HEALTH", "Sutter Health Davis", "Sutter Medical Foundation; Fax", "SUTTER ROSEVILLE", "Sutter Roseville/Allscripts", "Sutter Sacramento/ Allscripts", "SUTTER SACRMENTO", "Sutter/ Rose Allscripts", "Sutter/allscripts", "Suttter Health", "Sutter Roseville", "UC DAVIS", "UCD Grass Valley", "UCD Sac", "UCD/ Allscripts", "UCDMC", "UCSF", "UCSF / Allscripts", "VA", "VA Referral via FAX", "Vibra Hospital of Sacramento", "washington hospital health", "Whitney Oaks Care Center", "WOODSIDE HEALTH CARE CENTER", "Woodside Healthare Center (email)"],

            ["Priority"] = ["Routine", "Urgent", "STAT"],
            ["CaseStatus"] = ["Active", "Pending"],
            ["PatientStatus"] = ["Active", "Dead Referral", "Discharged", "On Hold", "Pending Referral", "Pending SOC", "Deceased", "TIF", "ROC"],
            ["YesNo"] = ["YES", "NO"],
            ["YesNoNA"] = ["YES", "NO", "N/A"],
            ["OrdersSignedStatus"] = ["YES", "NO", "NOT YET RETURN"],
            ["CaseMixType"] = ["SN EVAL ONLY", "PT EVAL ONLY", "SN ONLY", "PT ONLY", "SN, PT", "SN, HHA", "SN, OT", "SN, MWS", "SN, ST", "SN, PT, HHA", "SN, PT, MWS", "SN, PT, OT", "SN, PT, SP", "SN, PT, HHA, MWS", "SN, PT, OT, HHA", "SN, PT, HHA, MSW", "SN, PT, OT, SP", "SN, PT, OT, HHA, MSW, ST"],
            ["ContactType"] = ["Patient", "Caregiver", "Physician Office", "Insurance", "Other"],
            ["CommunicationMethod"] = ["Note", "Phone", "Email", "SMS", "Fax"],
            ["ConfirmationStatus"] = ["Confirmed", "Left VM", "Left VM and Sent SMS", "Sent SMS", "No Answer", "Reschedule", "TIF", "Cancelled Visit", "Non-Visit Discharge", "Missed Visit"],
            ["VisitType"] = ["SOC", "ROC", "LVN", "PT EVAL", "BATH", "RE-EVAL", "RECERT", "OT", "ST", "MSW", "HHA"],
            ["VisitStatus"] = ["Completed", "Missed", "Rescheduled", "Cancelled"],
            ["FinalDischargeStatus"] = ["Completed", "Discharged", "Transferred", "Patient Declined"],
            ["ReferralTrackerStatus"] = ["Active", "Dead Referral", "Discharged", "On Hold", "Pending Referral", "Pending SOC", "Deceased", "TIF", "ROC"],
        };

        foreach (var group in groups)
        {
            var existingCodes = await context.LookupOptions
                .Where(x => x.LookupType == group.Key)
                .Select(x => x.Code)
                .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, cancellationToken);

            for (var i = 0; i < group.Value.Length; i++)
            {
                var display = group.Value[i];
                var code = display.Trim().ToUpperInvariant();

                // Add the code to the in-memory set immediately so duplicate
                // values in the same seed array are not added twice before
                // SaveChangesAsync runs.
                if (existingCodes.Add(code))
                    context.LookupOptions.Add(new LookupOption(group.Key, code, display, i + 1, true));
            }
        }

        // Import the actual insurance choices from the tracker workbook that are
        // already part of the project's workflow vocabulary.
        var insuranceChoices = new[]
        {
            "1st: Partnership", "2nd: UHC Sr Adv PPO", "AARP Secure Horizon", "AARP UHC Adv PPO", "Aetna", "Aetna Choice PPO", "Aetna Medicare", "Aetna Medicare Open Plan", "Alignment Medicare Advantage", "Anthem Blue Cross", "Anthem Medical–Medicare", "Anthem PPO", "BC Medicare", "Blue Cross", "Blue Cross / Nivano", "Blue Cross Blue Shield – FEP", "Blue Cross CMG / Cal Aim Tar Pending", "Blue Cross Federal Emp", "Blue Cross Imperial", "Blue Cross Medicare", "Blue Cross Medicare Adv", "Blue Cross Medicare Advantage", "Blue Cross PPO", "Blue Cross Sr Medicare", "Blue Cross Sr Medicare Advantage", "Blue Cross Sr Medicare Advantage Other", "Blue Shield PPO", "Blue Shield Select", "Blue Shield Select / Medicare", "Blue Shield Senior", "Blue Shield Sr PPO", "Blue Shield w/ Hills Physician Medical Group", "California Physicians Services PPO", "Care Advantage Sr Plan", "Carelon Medicare", "Cigna PPO", "Dignity Health MA / Comm Sacramento", "Dignity Healthcare UHC Medicare", "Health Net Medicare", "Hills Physician Care Solutions", "Hills Physicians", "Hills UHC", "HMO Sutter", "Humana", "Kaiser", "Medical / Nivano Physicians", "Medicare", "Medicare (Aetna)", "Medicare / Devoted Health Insurance PPO", "Medicare / Kaiser", "Medicare / Tricare", "Medicare / VA", "Medicare A", "Medicare A/B", "Medicare Advantage", "Medicare Advantage HMO / Anthem Medicare Advantage", "Medicare Advantage HMO / IMPERIAL PLAN MEDICARE ADV", "Medicare Advantage PPO", "Medicare Advantage PPO / Molina Healthcare", "Medicare Part A", "Medicare Part A & B", "Medicare PPO CalPERS", "Medicare UHC", "M–Medicare Part A & B", "Molina (Medicaid)", "Nivano Medicare Advantage", "Nivano Physicians / Alignment Health Plan HMO", "Pro Bono", "Secondary Payer (Medicare)", "Sutter Health", "SCAN", "Tricare", "UHC", "UHC / VA", "UHC Blue Cross Sr", "UHC Choice PPO", "UHC Commercial PPO", "UHC Hills Physicians", "UHC HMO", "UHC Medicare", "UHC Medicare Advantage PPO", "UHC Medicare Choice", "UHC Mercy", "UHC PPO", "UHC Sr", "UHC Sr Advantage", "UHC Sr Advantage PPO", "UHC Sr HMO", "UHC Sr PPO", "United Healthcare – UHC Mgd MCR PPO", "VA", "VA / Blue Shield", "VA Insurance", "VA Tricare", "VA / Medicare", "Western Health Advantage; Hills", "UHC SR HMO", "UHC SR PPO", "UNITED HLTHCARE - UHC MGD MCR PPO", "VA /BLUE SHIELD", "VA insurance", "VA/Medicare", "WESTERN, HEALTH ADVANTAGE; HILLS"
        };
        var existingInsuranceCodes = await context.LookupOptions
            .Where(x => x.LookupType == "Insurance")
            .Select(x => x.Code)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, cancellationToken);

        for (var i = 0; i < insuranceChoices.Length; i++)
        {
            var display = insuranceChoices[i];
            var code = display.Trim().ToUpperInvariant();

            // Prevent duplicate normalized insurance codes such as
            // "UHC Sr PPO" and "UHC SR PPO" from being inserted in the
            // same SaveChangesAsync operation.
            if (existingInsuranceCodes.Add(code))
                context.LookupOptions.Add(new LookupOption("Insurance", code, display, i + 1, true));
        }

        await context.SaveChangesAsync(cancellationToken);
    }

}
