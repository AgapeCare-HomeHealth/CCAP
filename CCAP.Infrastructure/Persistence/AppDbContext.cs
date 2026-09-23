using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<ReferralDraft> ReferralDrafts
    {
        get;
        set;
    }
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Discipline> Disciplines => Set<Discipline>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<LookupOption> LookupOptions => Set<LookupOption>();
    public DbSet<Referral> Referrals => Set<Referral>();
    public DbSet<CallNote> CallNotes => Set<CallNote>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<ComplianceRecord> ComplianceRecords => Set<ComplianceRecord>();
    public DbSet<PatientTask> PatientTasks => Set<PatientTask>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();
    public DbSet<PatientServiceOrder> PatientServiceOrders => Set<PatientServiceOrder>();
    public DbSet<ReferralDocument> ReferralDocuments => Set<ReferralDocument>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<NotificationReadState> NotificationReadStates => Set<NotificationReadState>();
    public DbSet<PatientAuditLog> PatientAuditLogs => Set<PatientAuditLog>();
    public DbSet<PatientCareLog> PatientCareLogs => Set<PatientCareLog>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("dbo");

        modelBuilder.Entity<Announcement>(e =>
        {
            e.ToTable("Announcements");
            e.HasKey(x => x.AnnouncementId);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Message).HasMaxLength(2000).IsRequired();
            e.Property(x => x.PublishedAt).IsRequired();
            e.Property(x => x.IsActive).IsRequired();
            e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(e =>
        {
            e.ToTable("Roles");
            e.HasKey(x => x.RoleId);
            e.Property(x => x.RoleName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Permission>(e =>
        {
            e.ToTable("Permissions");
            e.HasKey(x => x.PermissionId);
            e.Property(x => x.PermissionCode).HasMaxLength(100).IsRequired();
            e.Property(x => x.PermissionName).HasMaxLength(150).IsRequired();
            e.Property(x => x.Module).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<RolePermission>(e =>
        {
            e.ToTable("RolePermissions");
            e.HasKey(x => x.RolePermissionId);
            e.HasOne(x => x.Role).WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Permission).WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
        });

        modelBuilder.Entity<ReferralDraft>(
        entity =>
        {
            entity.HasKey(
                x => x.ReferralDraftId);

            entity.Property(
                x => x.Data)
                .IsRequired();

            entity.Property(
                x => x.Status)
                .IsRequired();

            entity.Property(
                x => x.CreatedAt)
                .IsRequired();

            entity.Property(
                x => x.UpdatedAt)
                .IsRequired();

            entity.HasIndex(
                x => x.CreatedByUserId);

            entity.HasIndex(
                x => x.Status);

            entity.HasIndex(
                x => x.UpdatedAt);
        });

        modelBuilder.Entity<NotificationReadState>(e =>
        {
            e.ToTable("NotificationReadStates");
            e.HasKey(x => x.NotificationReadStateId);
            e.Property(x => x.NotificationType).HasMaxLength(50).IsRequired();
            e.Property(x => x.ReadAt).IsRequired();
            e.HasIndex(x => new { x.UserId, x.NotificationId, x.NotificationType }).IsUnique();
            e.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Discipline>(e =>
        {
            e.ToTable("Disciplines");
            e.HasKey(x => x.DisciplineId);
            e.Property(x => x.Code).HasMaxLength(30).IsRequired();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<ApplicationUser>(e =>
        {
            e.ToTable("ApplicationUsers");
            e.HasKey(x => x.UserId);
            e.Property(x => x.EmployeeNo).HasMaxLength(50).IsRequired();
            e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(x => x.MobileNo).HasMaxLength(30);
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.EmployeeNo).IsUnique();
            e.HasOne(x => x.Role).WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Discipline).WithMany(x => x.Users)
                .HasForeignKey(x => x.DisciplineId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LookupOption>(e =>
        {
            e.ToTable("LookupOptions");
            e.HasKey(x => x.LookupOptionId);
            e.Property(x => x.LookupType).HasMaxLength(100).IsRequired();
            e.Property(x => x.Code).HasMaxLength(200).IsRequired();
            e.Property(x => x.DisplayName).HasMaxLength(300).IsRequired();
            e.Property(x => x.SortOrder).IsRequired();
            e.Property(x => x.IsActive).IsRequired();
            e.HasIndex(x => new { x.LookupType, x.Code }).IsUnique();
            e.HasIndex(x => new { x.LookupType, x.IsActive, x.SortOrder });
        });

        modelBuilder.Entity<Patient>(e =>
        {
            e.ToTable("Patients");

            e.HasKey(x => x.PatientId);

            e.Property(x => x.MRN)
                .HasMaxLength(50)
                .IsRequired();

            e.Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            e.Property(x => x.MiddleName)
                .HasMaxLength(100);

            e.Property(x => x.LastName)
                .HasMaxLength(100)
                .IsRequired();

            e.Property(x => x.Gender)
                .HasMaxLength(30);

            e.Property(x => x.State)
                .HasMaxLength(50);

            e.Property(x => x.ZipCode)
                .HasMaxLength(20);

            e.Property(x => x.AuthorizationDate)
                .HasColumnType("date");

            e.Property(x => x.PreAuthDueDate).HasColumnType("date");
            e.Property(x => x.NumberOfVisits);
            e.Property(x => x.CaseMixType).HasMaxLength(200);
            e.Property(x => x.DmeMedSupplyNotes).HasMaxLength(2000);
            e.Property(x => x.SocFeedbackFromPatient).HasMaxLength(2000);
            e.Property(x => x.TifDate).HasColumnType("date");
            e.Property(x => x.RocDate).HasColumnType("date");
            e.Property(x => x.RecertDate).HasColumnType("date");
            e.Property(x => x.PcpPtNotified);
            e.Property(x => x.DischargeDate).HasColumnType("date");
            e.Property(x => x.DischargeFeedback).HasMaxLength(2000);
            e.Property(x => x.TransferDestination).HasMaxLength(300);
            e.Property(x => x.TransferDate).HasColumnType("date");
            e.Property(x => x.TransferReason).HasMaxLength(2000);

            e.Property(x => x.ApprovedVisits);

            e.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30);

            e.HasIndex(x => x.MRN)
                .IsUnique();

            e.HasOne(x => x.Coordinator)
                .WithMany()
                .HasForeignKey(x => x.CoordinatorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Clinician)
                .WithMany()
                .HasForeignKey(x => x.ClinicianId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Referral>(e =>
        {
            e.ToTable("Referrals");

            e.HasKey(x => x.ReferralId);

            e.Property(x => x.ReferralNumber)
                .HasMaxLength(50)
                .IsRequired();

            e.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(40);

            e.Property(x => x.Source)
                .HasMaxLength(150);

            e.Property(x => x.Priority)
                .HasMaxLength(50);

            e.Property(x => x.VisitPriority)
                .HasMaxLength(50);

            e.Property(x => x.CaseStatus)
                .HasMaxLength(50);

            e.Property(x => x.PrimaryInsurance)
                .HasMaxLength(150);

            e.Property(x => x.InsuranceMemberId)
                .HasMaxLength(100);

            e.Property(x => x.AuthorizationDate)
                .HasColumnType("date");


            e.Property(x => x.ApprovedVisits);

            e.Property(x => x.ReferringPhysician)
                .HasMaxLength(200);

            e.Property(x => x.PhysicianPhone)
                .HasMaxLength(50);

            e.HasIndex(x => x.ReferralNumber)
                .IsUnique();

            e.HasOne(x => x.Patient)
                .WithMany(x => x.Referrals)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.AssignedUser)
                .WithMany()
                .HasForeignKey(x => x.AssignedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Location)
                .WithMany()
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Discipline)
                .WithMany()
                .HasForeignKey(x => x.DisciplineId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CallNote>(e =>
        {
            e.ToTable("CallNotes");
            e.HasKey(x => x.CallNoteId);
            e.Property(x => x.ContactType).HasMaxLength(100).IsRequired();
            e.Property(x => x.Method).HasMaxLength(50).IsRequired();
            e.Property(x => x.Subject).HasMaxLength(200).IsRequired();
            e.Property(x => x.Notes).HasColumnType("nvarchar(max)").IsRequired();
            e.HasOne(x => x.Patient).WithMany(x => x.CallNotes).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.RecordedBy).WithMany().HasForeignKey(x => x.RecordedByUserId).OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<PatientCareLog>(e =>
        {
            e.ToTable("PatientCareLogs"); e.HasKey(x=>x.PatientCareLogId);
            e.Property(x=>x.LogType).HasMaxLength(50).IsRequired(); e.Property(x=>x.Item).HasMaxLength(200).IsRequired();
            e.Property(x=>x.Quantity).HasColumnType("decimal(18,2)"); e.Property(x=>x.Unit).HasMaxLength(50); e.Property(x=>x.Notes).HasMaxLength(2000);
            e.HasOne(x=>x.Patient).WithMany().HasForeignKey(x=>x.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x=>x.RecordedByUser).WithMany().HasForeignKey(x=>x.RecordedByUserId).OnDelete(DeleteBehavior.Restrict); e.HasIndex(x=>new{x.PatientId,x.LogType,x.RecordedAt});
        });

        modelBuilder.Entity<PatientAuditLog>(e =>
        {
            e.ToTable("PatientAuditLogs");
            e.HasKey(x => x.PatientAuditLogId);
            e.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
            e.Property(x => x.EntityId).HasMaxLength(100).IsRequired();
            e.Property(x => x.Action).HasMaxLength(20).IsRequired();
            e.Property(x => x.OldValues).HasColumnType("nvarchar(max)");
            e.Property(x => x.NewValues).HasColumnType("nvarchar(max)");
            e.Property(x => x.Description).HasMaxLength(1000);
            e.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.PerformedByUser).WithMany().HasForeignKey(x => x.PerformedByUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.PatientId, x.OccurredAt });
        });

        modelBuilder.Entity<Assessment>(e =>
        {
            e.ToTable("Assessments");
            e.HasKey(x => x.AssessmentId);
            e.HasOne(x => x.Patient).WithMany(x => x.Assessments).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.CompletedBy).WithMany().HasForeignKey(x => x.CompletedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ComplianceRecord>(e =>
        {
            e.ToTable("ComplianceRecords");
            e.HasKey(x => x.ComplianceRecordId);
            e.HasOne(x => x.Patient).WithMany(x => x.ComplianceRecords).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PatientTask>(e =>
        {
            e.ToTable("PatientTasks");
            e.HasKey(x => x.TaskId);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.CompletedAt).HasColumnType("datetime2");
            e.HasOne(x => x.Patient).WithMany(x => x.Tasks).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.AssignedUser).WithMany().HasForeignKey(x => x.AssignedUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Activity>(e =>
        {
            e.ToTable("Activities");
            e.HasKey(x => x.ActivityId);
            e.HasOne(x => x.Patient).WithMany(x => x.Activities).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.PerformedBy).WithMany().HasForeignKey(x => x.PerformedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ServiceType>(e =>
        {
            e.ToTable("ServiceTypes");
            e.HasKey(x => x.ServiceTypeId);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Icon).HasMaxLength(100).IsRequired();
            e.Property(x => x.CssClass).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<PatientServiceOrder>(e =>
        {
            e.ToTable("PatientServiceOrders");
            e.HasKey(x => x.PatientServiceOrderId);
            e.Property(x => x.Status).HasMaxLength(30).IsRequired();
            e.Property(x => x.Frequency).HasMaxLength(100);
            e.Property(x => x.Duration).HasMaxLength(100);
            e.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.ServiceType).WithMany(x => x.PatientServiceOrders)
                .HasForeignKey(x => x.ServiceTypeId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.PatientId, x.ServiceTypeId }).IsUnique();
        });

        modelBuilder.Entity<Visit>(e =>
        {
            e.ToTable("Visits");
            e.HasKey(x => x.VisitId);
            e.Property(x => x.PatientName).HasMaxLength(300).IsRequired();
            e.HasIndex(x => x.AssignedUserId);
            e.Property(x => x.ClinicianName).HasMaxLength(300);
            e.Property(x => x.VisitType).HasMaxLength(100).IsRequired();
            e.Property(x => x.Location).HasMaxLength(500);
            e.HasOne(x => x.Patient).WithMany(x => x.Visits)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
            e.HasOne<ApplicationUser>().WithMany()
                .HasForeignKey(x => x.AssignedUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
            e.HasOne(x => x.Clinician).WithMany()
                .HasForeignKey(x => x.ClinicianId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });

        modelBuilder.Entity<ReferralDocument>(e =>
        {
            e.ToTable("ReferralDocuments");

            e.HasKey(x => x.ReferralDocumentId);

            e.Property(x => x.StorageKey)
                .HasMaxLength(500);

            e.Property(x => x.OriginalFileName)
                .HasMaxLength(255)
                .IsRequired();

            e.Property(x => x.ContentType)
                .HasMaxLength(100)
                .IsRequired();

            e.Property(x => x.FileSize)
                .IsRequired();

            e.Property(x => x.UploadedAt)
                .IsRequired();

            e.HasOne(x => x.Referral)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.ReferralId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.ReferralId);
        });
        modelBuilder.Entity<Location>(e =>
        {
            e.ToTable("Locations");

            e.HasKey(x => x.LocationId);

            e.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            e.Property(x => x.IsActive)
                .IsRequired();

            e.Property(x => x.IsDefault)
                .IsRequired();

            e.HasIndex(x => x.Name)
                .IsUnique();
        });
    }
}
