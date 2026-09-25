using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CCAP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initialCCAPschema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Disciplines",
                schema: "dbo",
                columns: table => new
                {
                    DisciplineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disciplines", x => x.DisciplineId);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                schema: "dbo",
                columns: table => new
                {
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.LocationId);
                });

            migrationBuilder.CreateTable(
                name: "LookupOptions",
                schema: "dbo",
                columns: table => new
                {
                    LookupOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LookupType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupOptions", x => x.LookupOptionId);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                schema: "dbo",
                columns: table => new
                {
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PermissionName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Module = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.PermissionId);
                });

            migrationBuilder.CreateTable(
                name: "ReferralDrafts",
                schema: "dbo",
                columns: table => new
                {
                    ReferralDraftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralDrafts", x => x.ReferralDraftId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "dbo",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "ServiceTypes",
                schema: "dbo",
                columns: table => new
                {
                    ServiceTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CssClass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceTypes", x => x.ServiceTypeId);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUsers",
                schema: "dbo",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisciplineId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmployeeNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MobileNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUsers", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_ApplicationUsers_Disciplines_DisciplineId",
                        column: x => x.DisciplineId,
                        principalSchema: "dbo",
                        principalTable: "Disciplines",
                        principalColumn: "DisciplineId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationUsers_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "dbo",
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                schema: "dbo",
                columns: table => new
                {
                    RolePermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.RolePermissionId);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "dbo",
                        principalTable: "Permissions",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "dbo",
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Announcements",
                schema: "dbo",
                columns: table => new
                {
                    AnnouncementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.AnnouncementId);
                    table.ForeignKey(
                        name: "FK_Announcements_ApplicationUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationReadStates",
                schema: "dbo",
                columns: table => new
                {
                    NotificationReadStateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationReadStates", x => x.NotificationReadStateId);
                    table.ForeignKey(
                        name: "FK_NotificationReadStates_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                schema: "dbo",
                columns: table => new
                {
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MRN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PrimaryDiagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecondaryDiagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlternatePhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyContactRelationship = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryInsurance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsuranceMemberId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthorizationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PreAuthDueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NumberOfVisits = table.Column<int>(type: "int", nullable: true),
                    CaseMixType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DmeMedSupplyNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SocFeedbackFromPatient = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TifDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RocDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RecertDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PcpPtNotified = table.Column<bool>(type: "bit", nullable: true),
                    DischargeDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DischargeFeedback = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TransferDestination = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TransferDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TransferReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ApprovedVisits = table.Column<int>(type: "int", nullable: true),
                    AuthorizationRequired = table.Column<bool>(type: "bit", nullable: false),
                    InsuranceVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InsuranceVerifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferringPhysician = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhysicianPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferralNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CoordinatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClinicianId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SocDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CareCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinalizedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FinalStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.PatientId);
                    table.ForeignKey(
                        name: "FK_Patients_ApplicationUsers_ClinicianId",
                        column: x => x.ClinicianId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Patients_ApplicationUsers_CoordinatorId",
                        column: x => x.CoordinatorId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Activities",
                schema: "dbo",
                columns: table => new
                {
                    ActivityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerformedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActivityDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.ActivityId);
                    table.ForeignKey(
                        name: "FK_Activities_ApplicationUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Activities_Patients_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "dbo",
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Assessments",
                schema: "dbo",
                columns: table => new
                {
                    AssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.AssessmentId);
                    table.ForeignKey(
                        name: "FK_Assessments_ApplicationUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assessments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "dbo",
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CallNotes",
                schema: "dbo",
                columns: table => new
                {
                    CallNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CallDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContactType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Outcome = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallNotes", x => x.CallNoteId);
                    table.ForeignKey(
                        name: "FK_CallNotes_ApplicationUsers_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CallNotes_Patients_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "dbo",
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceRecords",
                schema: "dbo",
                columns: table => new
                {
                    ComplianceRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequirementCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceRecords", x => x.ComplianceRecordId);
                    table.ForeignKey(
                        name: "FK_ComplianceRecords_Patients_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "dbo",
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientAuditLogs",
                schema: "dbo",
                columns: table => new
                {
                    PatientAuditLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerformedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientAuditLogs", x => x.PatientAuditLogId);
                    table.ForeignKey(
                        name: "FK_PatientAuditLogs_ApplicationUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientAuditLogs_Patients_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "dbo",
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientCareLogs",
                schema: "dbo",
                columns: table => new
                {
                    PatientCareLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LogType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Item = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientCareLogs", x => x.PatientCareLogId);
                    table.ForeignKey(
                        name: "FK_PatientCareLogs_ApplicationUsers_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientCareLogs_Patients_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "dbo",
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientServiceOrders",
                schema: "dbo",
                columns: table => new
                {
                    PatientServiceOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Duration = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsPrimaryDiscipline = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientServiceOrders", x => x.PatientServiceOrderId);
                    table.ForeignKey(
                        name: "FK_PatientServiceOrders_Patients_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "dbo",
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientServiceOrders_ServiceTypes_ServiceTypeId",
                        column: x => x.ServiceTypeId,
                        principalSchema: "dbo",
                        principalTable: "ServiceTypes",
                        principalColumn: "ServiceTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PatientTasks",
                schema: "dbo",
                columns: table => new
                {
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PageRoute = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientTasks", x => x.TaskId);
                    table.ForeignKey(
                        name: "FK_PatientTasks_ApplicationUsers_AssignedUserId",
                        column: x => x.AssignedUserId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientTasks_Patients_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "dbo",
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Referrals",
                schema: "dbo",
                columns: table => new
                {
                    ReferralId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferralNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferralDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AssignedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisciplineId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VisitPriority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CaseStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PrimaryInsurance = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    InsuranceMemberId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AuthorizationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ApprovedVisits = table.Column<int>(type: "int", nullable: true),
                    AuthorizationRequired = table.Column<bool>(type: "bit", nullable: false),
                    ReferringPhysician = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PhysicianPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SecondaryDiagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferralNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InternalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referrals", x => x.ReferralId);
                    table.ForeignKey(
                        name: "FK_Referrals_ApplicationUsers_AssignedUserId",
                        column: x => x.AssignedUserId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Referrals_Disciplines_DisciplineId",
                        column: x => x.DisciplineId,
                        principalSchema: "dbo",
                        principalTable: "Disciplines",
                        principalColumn: "DisciplineId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Referrals_Locations_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "dbo",
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Referrals_Patients_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "dbo",
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Visits",
                schema: "dbo",
                columns: table => new
                {
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClinicianId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PatientName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ClinicianName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    VisitType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visits", x => x.VisitId);
                    table.ForeignKey(
                        name: "FK_Visits_ApplicationUsers_AssignedUserId",
                        column: x => x.AssignedUserId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visits_ApplicationUsers_ClinicianId",
                        column: x => x.ClinicianId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visits_Patients_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "dbo",
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReferralDocuments",
                schema: "dbo",
                columns: table => new
                {
                    ReferralDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferralId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralDocuments", x => x.ReferralDocumentId);
                    table.ForeignKey(
                        name: "FK_ReferralDocuments_Referrals_ReferralId",
                        column: x => x.ReferralId,
                        principalSchema: "dbo",
                        principalTable: "Referrals",
                        principalColumn: "ReferralId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_PatientId",
                schema: "dbo",
                table: "Activities",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_PerformedByUserId",
                schema: "dbo",
                table: "Activities",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_CreatedByUserId",
                schema: "dbo",
                table: "Announcements",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_DisciplineId",
                schema: "dbo",
                table: "ApplicationUsers",
                column: "DisciplineId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_Email",
                schema: "dbo",
                table: "ApplicationUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_EmployeeNo",
                schema: "dbo",
                table: "ApplicationUsers",
                column: "EmployeeNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_RoleId",
                schema: "dbo",
                table: "ApplicationUsers",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_CompletedByUserId",
                schema: "dbo",
                table: "Assessments",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_PatientId",
                schema: "dbo",
                table: "Assessments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_CallNotes_PatientId",
                schema: "dbo",
                table: "CallNotes",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_CallNotes_RecordedByUserId",
                schema: "dbo",
                table: "CallNotes",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRecords_PatientId",
                schema: "dbo",
                table: "ComplianceRecords",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Name",
                schema: "dbo",
                table: "Locations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LookupOptions_LookupType_Code",
                schema: "dbo",
                table: "LookupOptions",
                columns: new[] { "LookupType", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LookupOptions_LookupType_IsActive_SortOrder",
                schema: "dbo",
                table: "LookupOptions",
                columns: new[] { "LookupType", "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationReadStates_UserId_NotificationId_NotificationType",
                schema: "dbo",
                table: "NotificationReadStates",
                columns: new[] { "UserId", "NotificationId", "NotificationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientAuditLogs_PatientId_OccurredAt",
                schema: "dbo",
                table: "PatientAuditLogs",
                columns: new[] { "PatientId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientAuditLogs_PerformedByUserId",
                schema: "dbo",
                table: "PatientAuditLogs",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientCareLogs_PatientId_LogType_RecordedAt",
                schema: "dbo",
                table: "PatientCareLogs",
                columns: new[] { "PatientId", "LogType", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientCareLogs_RecordedByUserId",
                schema: "dbo",
                table: "PatientCareLogs",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_ClinicianId",
                schema: "dbo",
                table: "Patients",
                column: "ClinicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_CoordinatorId",
                schema: "dbo",
                table: "Patients",
                column: "CoordinatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_MRN",
                schema: "dbo",
                table: "Patients",
                column: "MRN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientServiceOrders_PatientId_ServiceTypeId",
                schema: "dbo",
                table: "PatientServiceOrders",
                columns: new[] { "PatientId", "ServiceTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientServiceOrders_ServiceTypeId",
                schema: "dbo",
                table: "PatientServiceOrders",
                column: "ServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientTasks_AssignedUserId",
                schema: "dbo",
                table: "PatientTasks",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientTasks_PatientId",
                schema: "dbo",
                table: "PatientTasks",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralDocuments_ReferralId",
                schema: "dbo",
                table: "ReferralDocuments",
                column: "ReferralId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralDrafts_CreatedByUserId",
                schema: "dbo",
                table: "ReferralDrafts",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralDrafts_Status",
                schema: "dbo",
                table: "ReferralDrafts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralDrafts_UpdatedAt",
                schema: "dbo",
                table: "ReferralDrafts",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_AssignedUserId",
                schema: "dbo",
                table: "Referrals",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_DisciplineId",
                schema: "dbo",
                table: "Referrals",
                column: "DisciplineId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_LocationId",
                schema: "dbo",
                table: "Referrals",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_PatientId",
                schema: "dbo",
                table: "Referrals",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_ReferralNumber",
                schema: "dbo",
                table: "Referrals",
                column: "ReferralNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                schema: "dbo",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_PermissionId",
                schema: "dbo",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTypes_Code",
                schema: "dbo",
                table: "ServiceTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visits_AssignedUserId",
                schema: "dbo",
                table: "Visits",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_ClinicianId",
                schema: "dbo",
                table: "Visits",
                column: "ClinicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_PatientId",
                schema: "dbo",
                table: "Visits",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activities",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Announcements",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Assessments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CallNotes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ComplianceRecords",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LookupOptions",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NotificationReadStates",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PatientAuditLogs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PatientCareLogs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PatientServiceOrders",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PatientTasks",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ReferralDocuments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ReferralDrafts",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "RolePermissions",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Visits",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ServiceTypes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Referrals",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Permissions",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Locations",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Patients",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ApplicationUsers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Disciplines",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "dbo");
        }
    }
}
