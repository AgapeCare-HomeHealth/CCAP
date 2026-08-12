using CCAP.Web.Features.Admin.Roles.Models;
using CCAP.Web.Features.Admin.Users.Models;
using CCAP.Web.Features.Dashboard.Models;
using CCAP.Web.Features.Patients.Models;
using CCAP.Web.Features.Tracker.PatientWorkflow.Model;
using CCAP.Web.Features.Tracker.PatientWorkflow.Models;

namespace CCAP.Web.Features.MockData;

public sealed class MockDataStore
{
    public Guid JohnId { get; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public Guid MariaId { get; } = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public Guid RobertId { get; } = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public Guid PatriciaId { get; } = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public Guid AdministratorRoleId { get; } = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public Guid CareCoordinatorRoleId { get; } = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public Guid SchedulerRoleId { get; } = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    public Guid ClinicianRoleId { get; } = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    public List<PatientListItem> Patients { get; } = [];
    public List<UserDto> Users { get; } = [];
    public List<RoleApiDto> Roles { get; } = [];
    public List<PermissionDto> Permissions { get; } = [];
    public Dictionary<Guid, HashSet<Guid>> RolePermissions { get; } = [];
    public List<LookupDto> Disciplines { get; } = [];
    public Dictionary<Guid, PatientCareProfileDto> PatientCare { get; } = [];
    public HashSet<Guid> ReadNotificationIds { get; } = [];

    public MockDataStore()
    {
        BuildPermissions();
        BuildRoles();
        BuildDisciplines();
        BuildUsers();
        BuildPatients();
        BuildPatientCare();
    }

    private void BuildPermissions()
    {
        AddPermission("dashboard.view", "View Dashboard", "Dashboard");
        AddPermission("patients.view", "View Patients", "Patients");
        AddPermission("patients.manage", "Edit Patients", "Patients");
        AddPermission("referrals.view", "View Referrals", "Referrals");
        AddPermission("referrals.manage", "Create Referrals", "Referrals");
        AddPermission("users.view", "View Users", "Administration");
        AddPermission("users.manage", "Manage Users", "Administration");
        AddPermission("roles.view", "View Roles", "Administration");
        AddPermission("roles.manage", "Manage Roles", "Administration");
        AddPermission("permissions.manage", "Manage Permissions", "Administration");
        AddPermission("workflow.view", "View Patient Workflow", "Patient Workflow");
        AddPermission("workflow.edit", "Manage Patient Workflow", "Patient Workflow");
        AddPermission("fax.view", "View Referring Fax Information", "Patient Profile");
        AddPermission("fax.manage", "Manage Referring Fax Information", "Patient Profile");
        AddPermission("notifications.view", "View Patient Notifications", "Patient Profile");
        AddPermission("notifications.manage", "Manage Patient Notifications", "Patient Profile");
        AddPermission("notes.view", "View Patient Notes", "Patient Profile");
        AddPermission("notes.manage", "Manage Patient Notes", "Patient Profile");
        AddPermission("labs.view", "View Lab Orders", "Clinical");
        AddPermission("labs.manage", "Manage Lab Orders", "Clinical");
        AddPermission("supplies.view", "View Wound Supplies", "Clinical");
        AddPermission("supplies.manage", "Manage Wound Supplies", "Clinical");
        AddPermission("foley.view", "View Foley Changes", "Clinical");
        AddPermission("foley.manage", "Manage Foley Changes", "Clinical");
        AddPermission("orders.view", "View Order Alerts", "Orders");
        AddPermission("orders.manage", "Manage Order Alerts", "Orders");
    }

    private void AddPermission(string code, string name, string module)
    {
        Permissions.Add(new PermissionDto
        {
            PermissionId = Guid.NewGuid(),
            PermissionCode = code,
            PermissionName = name,
            Module = module,
            Description = name
        });
    }

    private void BuildRoles()
    {
        Roles.Add(new RoleApiDto
        {
            RoleId = AdministratorRoleId,
            RoleName = "Administrator",
            Description = "Full system administration access.",
            UserCount = 1,
            PermissionCount = Permissions.Count,
            IsActive = true
        });
        Roles.Add(new RoleApiDto
        {
            RoleId = CareCoordinatorRoleId,
            RoleName = "Care Coordinator",
            Description = "Manages referrals, patients and care coordination.",
            UserCount = 2,
            PermissionCount = Permissions.Count(x => x.PermissionCode is "dashboard.view" or "patients.view" or "patients.manage" or "referrals.view" or "referrals.manage" or "workflow.view" or "workflow.edit" or "fax.view" or "fax.manage" or "notifications.view" or "notifications.manage" or "notes.view" or "notes.manage" or "labs.view" or "labs.manage" or "supplies.view" or "supplies.manage" or "foley.view" or "foley.manage" or "orders.view" or "orders.manage"),
            IsActive = true
        });
        Roles.Add(new RoleApiDto
        {
            RoleId = ClinicianRoleId,
            RoleName = "Clinician",
            Description = "Clinical access to assigned patient care information.",
            UserCount = 1,
            PermissionCount = Permissions.Count(x => x.PermissionCode is "dashboard.view" or "patients.view" or "patients.manage" or "referrals.view" or "fax.view" or "fax.manage" or "notifications.view" or "notifications.manage" or "notes.view" or "notes.manage" or "labs.view" or "labs.manage" or "supplies.view" or "supplies.manage" or "foley.view" or "foley.manage" or "orders.view" or "orders.manage"),
            IsActive = true
        });
        Roles.Add(new RoleApiDto
        {
            RoleId = SchedulerRoleId,
            RoleName = "Scheduler",
            Description = "Manages scheduling and assigned visits.",
            UserCount = 1,
            PermissionCount = 4,
            IsActive = true
        });

        var all = Permissions.Select(x => x.PermissionId).ToHashSet();
        RolePermissions[AdministratorRoleId] = all;
        RolePermissions[CareCoordinatorRoleId] = Permissions
            .Where(x => x.PermissionCode is "dashboard.view" or "patients.view" or "patients.manage" or "referrals.view" or "referrals.manage" or "workflow.view" or "workflow.edit" or "fax.view" or "fax.manage" or "notifications.view" or "notifications.manage" or "notes.view" or "notes.manage" or "labs.view" or "labs.manage" or "supplies.view" or "supplies.manage" or "foley.view" or "foley.manage" or "orders.view" or "orders.manage")
            .Select(x => x.PermissionId).ToHashSet();
        RolePermissions[SchedulerRoleId] = Permissions
            .Where(x => x.PermissionCode is "dashboard.view" or "patients.view" or "referrals.view" or "referrals.manage")
            .Select(x => x.PermissionId).ToHashSet();
        RolePermissions[ClinicianRoleId] = Permissions
            .Where(x => x.PermissionCode is "dashboard.view" or "patients.view" or "patients.manage" or "referrals.view" or "fax.view" or "fax.manage" or "notifications.view" or "notifications.manage" or "notes.view" or "notes.manage" or "labs.view" or "labs.manage" or "supplies.view" or "supplies.manage" or "foley.view" or "foley.manage" or "orders.view" or "orders.manage")
            .Select(x => x.PermissionId).ToHashSet();
    }

    private void BuildDisciplines()
    {
        Disciplines.Add(new LookupDto { DisciplineId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), Code = "RN", Name = "Registered Nurse" });
        Disciplines.Add(new LookupDto { DisciplineId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), Code = "LVN", Name = "Licensed Vocational Nurse" });
        Disciplines.Add(new LookupDto { DisciplineId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), Code = "PT", Name = "Physical Therapist" });
    }

    private void BuildUsers()
    {
        Users.Add(new UserDto
        {
            UserId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"), EmployeeNo = "EMP-0001",
            FirstName = "Jennifer", LastName = "Reyes", Email = "admin@ccap.local", MobileNo = "555-0101",
            RoleId = AdministratorRoleId, Role = "Administrator", Discipline = "Registered Nurse",
            DisciplineId = Disciplines[0].DisciplineId, IsActive = true, LastLoginAt = DateTime.Now.AddMinutes(-15)
        });
        Users.Add(new UserDto
        {
            UserId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002"), EmployeeNo = "EMP-0002",
            FirstName = "Maria", LastName = "Santos", Email = "maria.santos@ccap.local", MobileNo = "555-0102",
            RoleId = CareCoordinatorRoleId, Role = "Care Coordinator", Discipline = "Licensed Vocational Nurse",
            DisciplineId = Disciplines[1].DisciplineId, IsActive = true, LastLoginAt = DateTime.Now.AddHours(-2)
        });
        Users.Add(new UserDto
        {
            UserId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003"), EmployeeNo = "EMP-0003",
            FirstName = "Robert", LastName = "Wilson", Email = "robert.wilson@ccap.local",
            RoleId = SchedulerRoleId, Role = "Scheduler", Discipline = "",
            IsActive = true, LastLoginAt = DateTime.Now.AddDays(-1)
        });
        Users.Add(new UserDto
        {
            UserId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000005"), EmployeeNo = "EMP-0005",
            FirstName = "Michael", LastName = "Torres", Email = "michael.torres@ccap.local", MobileNo = "555-0105",
            RoleId = ClinicianRoleId, Role = "Clinician", Discipline = "Registered Nurse",
            DisciplineId = Disciplines[0].DisciplineId, IsActive = true, LastLoginAt = DateTime.Now.AddHours(-4)
        });
        Users.Add(new UserDto
        {
            UserId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000004"), EmployeeNo = "EMP-0004",
            FirstName = "Ana", LastName = "Reyes", Email = "ana.reyes@ccap.local",
            RoleId = CareCoordinatorRoleId, Role = "Care Coordinator", Discipline = "Registered Nurse",
            DisciplineId = Disciplines[0].DisciplineId, IsActive = false, LastLoginAt = DateTime.Now.AddDays(-7)
        });
    }

    private void BuildPatients()
    {
        Patients.Add(new PatientListItem { PatientId = JohnId, Name = "John Michael Smith", MRN = "MRN-100001", Status = "Active", PrimaryDiagnosis = "Congestive Heart Failure", AssignedClinician = "Jennifer RN", NextVisit = DateTime.Today.AddDays(1).AddHours(9).ToString("MM/dd/yyyy h:mm tt") });
        Patients.Add(new PatientListItem { PatientId = MariaId, Name = "Maria Elena Cruz", MRN = "MRN-100002", Status = "Pending", PrimaryDiagnosis = "Type 2 Diabetes", AssignedClinician = "Renaldi LVN", NextVisit = DateTime.Today.AddDays(2).AddHours(10).ToString("MM/dd/yyyy h:mm tt") });
        Patients.Add(new PatientListItem { PatientId = RobertId, Name = "Robert James Brown", MRN = "MRN-100003", Status = "Active", PrimaryDiagnosis = "COPD", AssignedClinician = "Jennifer RN", NextVisit = DateTime.Today.AddDays(3).AddHours(11).ToString("MM/dd/yyyy h:mm tt") });
        Patients.Add(new PatientListItem { PatientId = PatriciaId, Name = "Patricia Ann Johnson", MRN = "MRN-100004", Status = "On Hold", PrimaryDiagnosis = "Post-operative care", AssignedClinician = "Michael PT", NextVisit = "Not scheduled" });
    }

    private void BuildPatientCare()
    {
        var patients = new[] { JohnId, MariaId, RobertId, PatriciaId };
        foreach (var patientId in patients)
        {
            var profile = new PatientCareProfileDto
            {
                PatientId = patientId,
                Fax = new FaxInformationDto
                {
                    FaxId = Guid.NewGuid(), PatientId = patientId,
                    FaxNumber = patientId == MariaId ? "555-0140" : "555-0110",
                    ReferringProvider = patientId == RobertId ? "Dr. Robert Miller" : "Dr. Sarah Thompson",
                    Organization = "Springfield Family Medicine",
                    DocumentType = "Referral / Plan of Care",
                    ReceivedAt = DateTime.Today.AddDays(-5).AddHours(10),
                    Verified = patientId != PatriciaId,
                    Notes = "Referring office fax information for follow-up."
                }
            };

            profile.Notes.Add(new PatientNoteDto
            {
                NoteId = Guid.NewGuid(), PatientId = patientId,
                Subject = "Patient preference", Content = "Patient requested morning visits when possible.",
                Priority = "Normal", CreatedBy = "Jennifer LVN", CreatedAt = DateTime.Now.AddDays(-2)
            });

            profile.LabOrders.Add(new LabOrderDto
            {
                LabOrderId = Guid.NewGuid(), PatientId = patientId,
                TestName = patientId == RobertId ? "CBC / CMP" : "Hemoglobin A1C",
                OrderingProvider = "Dr. Robert Miller", OrderedDate = DateTime.Today.AddDays(-2),
                DueDate = DateTime.Today.AddDays(3), Status = "Ordered", Notes = "Track result and upload report when received."
            });

            profile.WoundSupplies.Add(new WoundSupplyDto
            {
                SupplyId = Guid.NewGuid(), PatientId = patientId,
                SupplyName = "4x4 Gauze Pads", Quantity = 20, Frequency = "Daily",
                Status = patientId == PatriciaId ? "Needs Order" : "Required", NeededBy = DateTime.Today.AddDays(5),
                Notes = "Include in next supply request."
            });

            profile.FoleyChanges.Add(new FoleyChangeDto
            {
                FoleyChangeId = Guid.NewGuid(), PatientId = patientId,
                ChangeDate = DateTime.Today.AddDays(-21), NextDueDate = DateTime.Today.AddDays(7),
                CatheterSize = "16 Fr", BalloonSize = "10 mL", ChangedBy = "Jennifer RN",
                Notes = "Routine catheter change."
            });

            profile.OrderAlerts.Add(new OrderAlertDto
            {
                OrderAlertId = Guid.NewGuid(), PatientId = patientId,
                OrderType = "POC", OrderDate = DateTime.Today.AddDays(patientId == JohnId ? -35 : -15),
                SignatureDue30Date = DateTime.Today.AddDays(patientId == JohnId ? -5 : 15),
                SignatureDue60Date = DateTime.Today.AddDays(patientId == JohnId ? 25 : 45),
                Signed = false, Status = "Pending PCP Signature",
                Notes = "Follow up with PCP for signature."
            });

            profile.OrderAlerts.Add(new OrderAlertDto
            {
                OrderAlertId = Guid.NewGuid(), PatientId = patientId,
                OrderType = "OASIS", OrderDate = DateTime.Today.AddDays(-65),
                SignatureDue30Date = DateTime.Today.AddDays(-35), SignatureDue60Date = DateTime.Today.AddDays(-5),
                Signed = patientId == PatriciaId, Status = patientId == PatriciaId ? "Signed" : "60-Day Follow-up Required",
                Notes = "Escalate when PCP signature remains outstanding after 60 days."
            });

            PatientCare[patientId] = profile;
        }
    }

    public List<CCAP.Web.Features.Notifications.Models.UserNotificationDto> BuildGlobalNotifications()
    {
        var now = DateTime.Now;
        var notifications = new List<CCAP.Web.Features.Notifications.Models.UserNotificationDto>();

        foreach (var patient in Patients)
        {
            var profile = GetPatientCareProfile(patient.PatientId);

            // POC/OASIS signature alerts are calculated from OrderDate.
            foreach (var order in profile.OrderAlerts)
            {
                if (order.Signed)
                    continue;

                var age = order.AgeInDays;
                if (age < 30)
                    continue;

                var severity = age >= 60 ? "Critical" : "Warning";
                var title = age >= 60
                    ? $"{order.OrderType} PCP signature overdue"
                    : $"{order.OrderType} PCP signature follow-up";
                var message = age >= 60
                    ? $"The {order.OrderType} order has been unsigned for {age} days and requires escalation."
                    : $"The {order.OrderType} order has reached the 30-day signature follow-up threshold.";

                var id = StableNotificationId(order.OrderAlertId, $"{order.OrderType}:signature");
                notifications.Add(new CCAP.Web.Features.Notifications.Models.UserNotificationDto
                {
                    NotificationId = id,
                    PatientId = patient.PatientId,
                    PatientName = patient.Name,
                    Type = "Order",
                    Title = title,
                    Message = message,
                    Severity = severity,
                    DueDate = order.SignatureDue60Date ?? order.SignatureDue30Date,
                    CreatedAt = order.OrderDate,
                    IsRead = ReadNotificationIds.Contains(id)
                });
            }

            // Lab order alerts are calculated from the lab due date.
            foreach (var lab in profile.LabOrders.Where(x =>
                         !string.Equals(x.Status, "Completed", StringComparison.OrdinalIgnoreCase)))
            {
                var days = (lab.DueDate.Date - now.Date).Days;
                if (days > 3)
                    continue;

                var severity = days < 0 ? "Critical" : days == 0 ? "Warning" : "Info";
                var message = days < 0
                    ? $"{lab.TestName} is overdue."
                    : days == 0
                        ? $"{lab.TestName} is due today."
                        : $"{lab.TestName} is due in {days} day{(days == 1 ? "" : "s")}.";

                var id = StableNotificationId(lab.LabOrderId, "lab");
                notifications.Add(new CCAP.Web.Features.Notifications.Models.UserNotificationDto
                {
                    NotificationId = id,
                    PatientId = patient.PatientId,
                    PatientName = patient.Name,
                    Type = "Lab",
                    Title = "Lab order follow-up",
                    Message = message,
                    Severity = severity,
                    DueDate = lab.DueDate,
                    CreatedAt = lab.OrderedDate,
                    IsRead = ReadNotificationIds.Contains(id)
                });
            }

            // Wound supplies are alerted from the NeededBy date.
            foreach (var supply in profile.WoundSupplies.Where(x =>
                         x.NeededBy.HasValue &&
                         !string.Equals(x.Status, "Fulfilled", StringComparison.OrdinalIgnoreCase)))
            {
                var due = supply.NeededBy!.Value.Date;
                var days = (due - now.Date).Days;
                if (days > 3)
                    continue;

                var severity = days < 0 ? "Critical" : "Warning";
                var message = days < 0
                    ? $"{supply.SupplyName} is past the needed-by date."
                    : days == 0
                        ? $"{supply.SupplyName} is needed today."
                        : $"{supply.SupplyName} is needed in {days} day{(days == 1 ? "" : "s")}.";

                var id = StableNotificationId(supply.SupplyId, "supply");
                notifications.Add(new CCAP.Web.Features.Notifications.Models.UserNotificationDto
                {
                    NotificationId = id,
                    PatientId = patient.PatientId,
                    PatientName = patient.Name,
                    Type = "Supply",
                    Title = "Wound supply follow-up",
                    Message = message,
                    Severity = severity,
                    DueDate = supply.NeededBy,
                    CreatedAt = now,
                    IsRead = ReadNotificationIds.Contains(id)
                });
            }

            // Foley changes are alerted from the next due date.
            foreach (var foley in profile.FoleyChanges.Where(x => x.NextDueDate.HasValue))
            {
                var due = foley.NextDueDate!.Value.Date;
                var days = (due - now.Date).Days;
                if (days > 3)
                    continue;

                var severity = days < 0 ? "Critical" : "Warning";
                var message = days < 0
                    ? "Foley catheter change is overdue."
                    : days == 0
                        ? "Foley catheter change is due today."
                        : $"Foley catheter change is due in {days} day{(days == 1 ? "" : "s")}.";

                var id = StableNotificationId(foley.FoleyChangeId, "foley");
                notifications.Add(new CCAP.Web.Features.Notifications.Models.UserNotificationDto
                {
                    NotificationId = id,
                    PatientId = patient.PatientId,
                    PatientName = patient.Name,
                    Type = "Foley",
                    Title = "Foley change follow-up",
                    Message = message,
                    Severity = severity,
                    DueDate = foley.NextDueDate,
                    CreatedAt = foley.ChangeDate,
                    IsRead = ReadNotificationIds.Contains(id)
                });
            }

            // Unverified referral fax information is an actionable exception.
            if (profile.Fax.FaxId != Guid.Empty && !profile.Fax.Verified)
            {
                var id = StableNotificationId(profile.Fax.FaxId, "fax");
                notifications.Add(new CCAP.Web.Features.Notifications.Models.UserNotificationDto
                {
                    NotificationId = id,
                    PatientId = patient.PatientId,
                    PatientName = patient.Name,
                    Type = "Fax",
                    Title = "Referral fax needs verification",
                    Message = $"Verify the referring fax information for {patient.Name}.",
                    Severity = "Warning",
                    CreatedAt = profile.Fax.ReceivedAt,
                    IsRead = ReadNotificationIds.Contains(id)
                });
            }
        }

        return notifications
            .OrderBy(x => SeverityRank(x.Severity))
            .ThenBy(x => x.DueDate ?? DateTime.MaxValue)
            .ThenByDescending(x => x.CreatedAt)
            .Take(50)
            .ToList();
    }

    public void MarkGlobalNotificationRead(Guid notificationId)
    {
        ReadNotificationIds.Add(notificationId);
    }

    private static int SeverityRank(string severity) => severity switch
    {
        "Critical" => 0,
        "Warning" => 1,
        _ => 2
    };

    private static Guid StableNotificationId(Guid sourceId, string type)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes($"{sourceId:N}:{type}");
        var hash = System.Security.Cryptography.MD5.HashData(bytes);
        return new Guid(hash);
    }

    public PatientCareProfileDto GetPatientCareProfile(Guid patientId)
    {
        if (!PatientCare.TryGetValue(patientId, out var profile))
        {
            profile = new PatientCareProfileDto { PatientId = patientId };
            PatientCare[patientId] = profile;
        }

        // Notifications are intentionally capped to a maximum 60-day lifetime.
        profile.Notifications.RemoveAll(x => x.ExpiresAt < DateTime.Now);
        return profile;
    }

    public void SaveFax(FaxInformationDto fax)
    {
        var profile = GetPatientCareProfile(fax.PatientId);
        if (fax.FaxId == Guid.Empty) fax.FaxId = Guid.NewGuid();
        profile.Fax = fax;
    }

    public void AddNotification(PatientNotificationDto notification)
    {
        var profile = GetPatientCareProfile(notification.PatientId);
        notification.NotificationId = notification.NotificationId == Guid.Empty ? Guid.NewGuid() : notification.NotificationId;
        notification.CreatedAt = notification.CreatedAt == default ? DateTime.Now : notification.CreatedAt;
        var maxExpiry = notification.CreatedAt.AddDays(60);
        if (notification.ExpiresAt <= notification.CreatedAt || notification.ExpiresAt > maxExpiry)
            notification.ExpiresAt = maxExpiry;
        profile.Notifications.Add(notification);
    }

    public void MarkNotificationRead(Guid patientId, Guid notificationId)
    {
        var item = GetPatientCareProfile(patientId).Notifications.FirstOrDefault(x => x.NotificationId == notificationId);
        if (item is not null) item.IsRead = true;
    }

    public void DeleteNotification(Guid patientId, Guid notificationId)
    {
        GetPatientCareProfile(patientId).Notifications.RemoveAll(x => x.NotificationId == notificationId);
    }

    public void AddPatientNote(PatientNoteDto note)
    {
        var profile = GetPatientCareProfile(note.PatientId);
        note.NoteId = note.NoteId == Guid.Empty ? Guid.NewGuid() : note.NoteId;
        note.CreatedAt = note.CreatedAt == default ? DateTime.Now : note.CreatedAt;
        profile.Notes.Insert(0, note);
    }

    public void TogglePatientNoteResolved(Guid patientId, Guid noteId)
    {
        var note = GetPatientCareProfile(patientId).Notes.FirstOrDefault(x => x.NoteId == noteId);
        if (note is not null) note.Resolved = !note.Resolved;
    }

    public void DeletePatientNote(Guid patientId, Guid noteId)
    {
        GetPatientCareProfile(patientId).Notes.RemoveAll(x => x.NoteId == noteId);
    }

    public void AddLabOrder(LabOrderDto order)
    {
        var profile = GetPatientCareProfile(order.PatientId);
        order.LabOrderId = order.LabOrderId == Guid.Empty ? Guid.NewGuid() : order.LabOrderId;
        profile.LabOrders.Insert(0, order);
    }

    public void UpdateLabOrderStatus(Guid patientId, Guid labOrderId, string status)
    {
        var item = GetPatientCareProfile(patientId).LabOrders.FirstOrDefault(x => x.LabOrderId == labOrderId);
        if (item is not null) item.Status = status;
    }

    public void AddWoundSupply(WoundSupplyDto supply)
    {
        var profile = GetPatientCareProfile(supply.PatientId);
        supply.SupplyId = supply.SupplyId == Guid.Empty ? Guid.NewGuid() : supply.SupplyId;
        profile.WoundSupplies.Insert(0, supply);
    }

    public void UpdateWoundSupplyStatus(Guid patientId, Guid supplyId, string status)
    {
        var item = GetPatientCareProfile(patientId).WoundSupplies.FirstOrDefault(x => x.SupplyId == supplyId);
        if (item is not null) item.Status = status;
    }

    public void AddFoleyChange(FoleyChangeDto change)
    {
        var profile = GetPatientCareProfile(change.PatientId);
        change.FoleyChangeId = change.FoleyChangeId == Guid.Empty ? Guid.NewGuid() : change.FoleyChangeId;
        profile.FoleyChanges.Insert(0, change);
    }

    public void AddOrderAlert(OrderAlertDto alert)
    {
        var profile = GetPatientCareProfile(alert.PatientId);
        alert.OrderAlertId = alert.OrderAlertId == Guid.Empty ? Guid.NewGuid() : alert.OrderAlertId;
        profile.OrderAlerts.Insert(0, alert);
    }

    public void MarkOrderSigned(Guid patientId, Guid orderAlertId)
    {
        var item = GetPatientCareProfile(patientId).OrderAlerts.FirstOrDefault(x => x.OrderAlertId == orderAlertId);
        if (item is not null)
        {
            item.Signed = true;
            item.Status = "Signed";
        }
    }

    public PatientWorkflowDto GetPatientWorkflow(Guid patientId)
    {
        var patient = Patients.FirstOrDefault(x => x.PatientId == patientId) ?? Patients[0];
        var referralId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var first = patient.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var firstName = first.ElementAtOrDefault(0) ?? "Patient";
        var lastName = first.ElementAtOrDefault(^1) ?? "";
        var middle = first.Length > 2 ? string.Join(' ', first.Skip(1).Take(first.Length - 2)) : "";

        return new PatientWorkflowDto
        {
            Header = new PatientHeaderDto
            {
                PatientId = patient.PatientId, ReferralId = referralId,
                FirstName = firstName, MiddleName = middle, LastName = lastName,
                Age = patient.PatientId == MariaId ? 58 : patient.PatientId == RobertId ? 72 : patient.PatientId == PatriciaId ? 66 : 65,
                MRN = patient.MRN, ReferralNumber = $"REF-2026-{patient.PatientId.ToString()[..4].ToUpper()}",
                Status = patient.Status, SocDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-4)),
                Coordinator = patient.PatientId == MariaId ? "Maria Santos" : "Jennifer LVN",
                Branch = "Main Office", EpisodeNumber = 1
            },
            WorkflowStages = BuildStages(patient),
            NextAction = new NextActionDto
            {
                TaskId = Guid.NewGuid(), Title = patient.Status == "Pending" ? "Complete Referral Review" : "Verify Insurance",
                Description = patient.Status == "Pending" ? "Review the referral information before assigning the next workflow step." : "Confirm coverage and authorization details.",
                DueDate = DateTime.Today.AddDays(1).AddHours(10), PageRoute = "/tracker/patient", Icon = "bi bi-check2-circle",
                IsOverdue = false
            },
            KeyInformation = new KeyInformationDto
            {
                Coordinator = patient.PatientId == MariaId ? "Maria Santos" : "Jennifer LVN",
                Clinician = patient.AssignedClinician, Discipline = patient.AssignedClinician.Contains("PT") ? "Physical Therapy" : "Skilled Nursing",
                Episode = 1, Branch = "Main Office", Payor = patient.PatientId == RobertId ? "Aetna" : "Medicare", Priority = patient.Status == "On Hold" ? "High" : "Routine"
            },
            RecentActivities =
            [
                new ActivityDto { ActivityId = Guid.NewGuid(), ActivityDate = DateTime.Now.AddHours(-2), Title = "Patient record reviewed", Description = "Patient information was reviewed for the next workflow action.", PerformedBy = "Jennifer LVN", ActivityType = "Review" },
                new ActivityDto { ActivityId = Guid.NewGuid(), ActivityDate = DateTime.Now.AddDays(-1), Title = "Referral received", Description = "Referral documentation was received and added to the patient record.", PerformedBy = "System", ActivityType = "Referral" }
            ],
            Summary = new PatientSummaryDto
            {
                PrimaryDiagnosis = patient.PrimaryDiagnosis, Insurance = patient.PatientId == RobertId ? "Aetna" : "Medicare",
                SocDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-4)), AuthorizedVisits = patient.PatientId == PatriciaId ? 0 : 12,
                Address = "123 Main Street, Springfield", PhoneNumber = "555-0199"
            }
        };
    }

    private static List<WorkflowStageDto> BuildStages(PatientListItem patient)
    {
        var current = patient.Status == "Pending" ? 1 : patient.Status == "On Hold" ? 2 : 3;
        var names = new[] { ("REFERRAL", "Referral"), ("INSURANCE", "Insurance"), ("SOC", "SOC Scheduled"), ("ADMISSION", "Admission"), ("VISITS", "Visits"), ("RECERT", "Recertification"), ("DISCHARGE", "Discharge") };
        return names.Select((x, i) => new WorkflowStageDto
        {
            Sequence = i + 1, StageCode = x.Item1, StageName = x.Item2,
            Status = i < current ? WorkflowStatus.Completed : i == current ? WorkflowStatus.Current : WorkflowStatus.Pending,
            Description = i < current ? "Completed" : i == current ? "Current Stage" : "Pending",
            CompletedDate = i < current ? DateTime.Today.AddDays(-(current - i)) : null,
            IsClickable = i <= current, Route = $"/tracker/patient/{patient.PatientId}"
        }).ToList();
    }

    public bool UpdatePatient(PatientEditModel updatedPatient)
    {
        var existingPatient = Patients.FirstOrDefault(
            x => x.PatientId == updatedPatient.PatientId);

        if (existingPatient is null)
            return false;

        existingPatient.Name =
            $"{updatedPatient.FirstName} {updatedPatient.MiddleName} {updatedPatient.LastName}"
                .Replace("  ", " ")
                .Trim();

        existingPatient.MRN = updatedPatient.MRN;
        existingPatient.Status = updatedPatient.Status;

        return true;
    }
}
