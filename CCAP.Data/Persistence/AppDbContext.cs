using CCAP.Data.Entities;
using Microsoft.EntityFrameworkCore;


namespace CCAP.Data.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();

        public DbSet<Role> Roles => Set<Role>();

        public DbSet<Permission> Permissions => Set<Permission>();

        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

        public DbSet<Discipline> Disciplines => Set<Discipline>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureRole(modelBuilder);

            ConfigurePermission(modelBuilder);

            ConfigureRolePermission(modelBuilder);

            ConfigureDiscipline(modelBuilder);

            ConfigureApplicationUser(modelBuilder);
        }

        #region Configurations

        private static void ConfigureRole(ModelBuilder builder)
        {
            builder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.HasKey(x => x.RoleId);

                entity.Property(x => x.RoleName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(500);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");
            });
        }

        private static void ConfigurePermission(ModelBuilder builder)
        {
            builder.Entity<Permission>(entity =>
            {
                entity.ToTable("Permission");

                entity.HasKey(x => x.PermissionId);

                entity.Property(x => x.PermissionCode)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.PermissionName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Module)
                    .HasMaxLength(100);

                entity.Property(x => x.Description)
                    .HasMaxLength(500);
            });
        }

        private static void ConfigureRolePermission(ModelBuilder builder)
        {
            builder.Entity<RolePermission>(entity =>
            {
                entity.ToTable("RolePermission");

                entity.HasKey(x => x.RolePermissionId);

                entity.HasOne(x => x.Role)
                    .WithMany(x => x.RolePermissions)
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Permission)
                    .WithMany(x => x.RolePermissions)
                    .HasForeignKey(x => x.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureDiscipline(ModelBuilder builder)
        {
            builder.Entity<Discipline>(entity =>
            {
                entity.ToTable("Discipline");

                entity.HasKey(x => x.DisciplineId);

                entity.Property(x => x.Code)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(500);
            });
        }

        private static void ConfigureApplicationUser(ModelBuilder builder)
        {
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("ApplicationUser");

                entity.HasKey(x => x.UserId);

                entity.Property(x => x.EmployeeNo)
                    .HasMaxLength(50);

                entity.Property(x => x.FirstName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.LastName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Email)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.MobileNo)
                    .HasMaxLength(30);

                entity.Property(x => x.PasswordHash)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne(x => x.Role)
                    .WithMany(x => x.Users)
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Discipline)
                    .WithMany(x => x.Users)
                    .HasForeignKey(x => x.DisciplineId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        #endregion
    }
}
