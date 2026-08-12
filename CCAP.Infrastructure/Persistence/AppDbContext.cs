using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Discipline> Disciplines => Set<Discipline>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Referral> Referrals => Set<Referral>();
    public DbSet<CallNote> CallNotes => Set<CallNote>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<ComplianceRecord> ComplianceRecords => Set<ComplianceRecord>();
    public DbSet<PatientTask> PatientTasks => Set<PatientTask>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();
    public DbSet<PatientServiceOrder> PatientServiceOrders => Set<PatientServiceOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

        modelBuilder.Entity<Patient>(e =>
        {
            e.ToTable("Patients");
            e.HasKey(x => x.PatientId);
            e.Property(x => x.MRN).HasMaxLength(50).IsRequired();
            e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            e.Property(x => x.MiddleName).HasMaxLength(100);
            e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
            e.HasIndex(x => x.MRN).IsUnique();
            e.HasOne(x => x.Coordinator).WithMany().HasForeignKey(x => x.CoordinatorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Clinician).WithMany().HasForeignKey(x => x.ClinicianId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Referral>(e =>
        {
            e.ToTable("Referrals");
            e.HasKey(x => x.ReferralId);
            e.Property(x => x.ReferralNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
            e.HasIndex(x => x.ReferralNumber).IsUnique();
            e.HasOne(x => x.Patient).WithMany(x => x.Referrals).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.AssignedUser).WithMany().HasForeignKey(x => x.AssignedUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CallNote>(e =>
        {
            e.ToTable("CallNotes");
            e.HasKey(x => x.CallNoteId);
            e.Property(x => x.Subject).HasMaxLength(200).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(5000).IsRequired();
            e.HasOne(x => x.Patient).WithMany(x => x.CallNotes).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.RecordedBy).WithMany().HasForeignKey(x => x.RecordedByUserId).OnDelete(DeleteBehavior.Restrict);
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
            e.HasOne(x => x.Patient).WithMany(x => x.Visits).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Clinician).WithMany().HasForeignKey(x => x.ClinicianId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
