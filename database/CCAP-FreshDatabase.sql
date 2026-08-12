/* CCAP fresh database bootstrap. Run on SQL Server. */
IF DB_ID(N'CCAP') IS NULL
BEGIN
    CREATE DATABASE [CCAP];
END;
GO
USE [CCAP];
GO

CREATE TABLE dbo.Roles
(
    RoleId uniqueidentifier NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
    RoleName nvarchar(100) NOT NULL,
    Description nvarchar(500) NULL,
    IsActive bit NOT NULL CONSTRAINT DF_Roles_IsActive DEFAULT (1),
    CreatedAt datetime2 NOT NULL CONSTRAINT DF_Roles_CreatedAt DEFAULT (GETUTCDATE()),
    UpdatedAt datetime2 NULL
);
CREATE UNIQUE INDEX IX_Roles_RoleName ON dbo.Roles(RoleName);

CREATE TABLE dbo.Permissions
(
    PermissionId uniqueidentifier NOT NULL CONSTRAINT PK_Permissions PRIMARY KEY,
    PermissionCode nvarchar(100) NOT NULL,
    PermissionName nvarchar(150) NOT NULL,
    Module nvarchar(100) NOT NULL,
    Description nvarchar(max) NULL
);
CREATE UNIQUE INDEX IX_Permissions_PermissionCode ON dbo.Permissions(PermissionCode);

CREATE TABLE dbo.Disciplines
(
    DisciplineId uniqueidentifier NOT NULL CONSTRAINT PK_Disciplines PRIMARY KEY,
    Code nvarchar(30) NOT NULL,
    Name nvarchar(100) NOT NULL,
    Description nvarchar(max) NULL
);
CREATE UNIQUE INDEX IX_Disciplines_Code ON dbo.Disciplines(Code);

CREATE TABLE dbo.ServiceTypes
(
    ServiceTypeId uniqueidentifier NOT NULL CONSTRAINT PK_ServiceTypes PRIMARY KEY,
    Code nvarchar(50) NOT NULL,
    Name nvarchar(150) NOT NULL,
    Icon nvarchar(100) NOT NULL,
    CssClass nvarchar(100) NOT NULL,
    IsActive bit NOT NULL CONSTRAINT DF_ServiceTypes_IsActive DEFAULT (1)
);
CREATE UNIQUE INDEX IX_ServiceTypes_Code ON dbo.ServiceTypes(Code);

CREATE TABLE dbo.ApplicationUsers
(
    UserId uniqueidentifier NOT NULL CONSTRAINT PK_ApplicationUsers PRIMARY KEY,
    RoleId uniqueidentifier NOT NULL,
    DisciplineId uniqueidentifier NULL,
    EmployeeNo nvarchar(50) NOT NULL,
    FirstName nvarchar(100) NOT NULL,
    LastName nvarchar(100) NOT NULL,
    Email nvarchar(200) NOT NULL,
    PasswordHash nvarchar(500) NOT NULL,
    MobileNo nvarchar(30) NULL,
    IsActive bit NOT NULL,
    CreatedAt datetime2 NOT NULL,
    UpdatedAt datetime2 NULL,
    CONSTRAINT FK_ApplicationUsers_Roles FOREIGN KEY(RoleId) REFERENCES dbo.Roles(RoleId),
    CONSTRAINT FK_ApplicationUsers_Disciplines FOREIGN KEY(DisciplineId) REFERENCES dbo.Disciplines(DisciplineId)
);
CREATE UNIQUE INDEX IX_ApplicationUsers_Email ON dbo.ApplicationUsers(Email);
CREATE UNIQUE INDEX IX_ApplicationUsers_EmployeeNo ON dbo.ApplicationUsers(EmployeeNo);

CREATE TABLE dbo.RolePermissions
(
    RolePermissionId uniqueidentifier NOT NULL CONSTRAINT PK_RolePermissions PRIMARY KEY,
    RoleId uniqueidentifier NOT NULL,
    PermissionId uniqueidentifier NOT NULL,
    CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY(RoleId) REFERENCES dbo.Roles(RoleId) ON DELETE CASCADE,
    CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY(PermissionId) REFERENCES dbo.Permissions(PermissionId) ON DELETE CASCADE
);
CREATE UNIQUE INDEX IX_RolePermissions_RoleId_PermissionId ON dbo.RolePermissions(RoleId, PermissionId);

CREATE TABLE dbo.Patients
(
    PatientId uniqueidentifier NOT NULL CONSTRAINT PK_Patients PRIMARY KEY,
    MRN nvarchar(50) NOT NULL,
    FirstName nvarchar(100) NOT NULL,
    MiddleName nvarchar(100) NULL,
    LastName nvarchar(100) NOT NULL,
    DateOfBirth date NULL,
    PrimaryDiagnosis nvarchar(max) NULL,
    Address nvarchar(max) NULL,
    PhoneNumber nvarchar(max) NULL,
    Status nvarchar(30) NOT NULL,
    CoordinatorId uniqueidentifier NULL,
    ClinicianId uniqueidentifier NULL,
    SocDate date NULL,
    CareCompletedAt datetime2 NULL,
    FinalizedByUserId uniqueidentifier NULL,
    FinalStatus nvarchar(max) NULL,
    ArchivedAt datetime2 NULL,
    ArchivedByUserId uniqueidentifier NULL,
    CONSTRAINT FK_Patients_Coordinator FOREIGN KEY(CoordinatorId) REFERENCES dbo.ApplicationUsers(UserId),
    CONSTRAINT FK_Patients_Clinician FOREIGN KEY(ClinicianId) REFERENCES dbo.ApplicationUsers(UserId)
);
CREATE UNIQUE INDEX IX_Patients_MRN ON dbo.Patients(MRN);

CREATE TABLE dbo.Referrals
(
    ReferralId uniqueidentifier NOT NULL CONSTRAINT PK_Referrals PRIMARY KEY,
    ReferralNumber nvarchar(50) NOT NULL,
    PatientId uniqueidentifier NULL,
    ReferralDate datetime2 NOT NULL,
    Status nvarchar(40) NOT NULL,
    Source nvarchar(max) NULL,
    Priority nvarchar(max) NULL,
    AssignedUserId uniqueidentifier NULL,
    AssignedAt datetime2 NULL,
    CONSTRAINT FK_Referrals_Patients FOREIGN KEY(PatientId) REFERENCES dbo.Patients(PatientId),
    CONSTRAINT FK_Referrals_AssignedUser FOREIGN KEY(AssignedUserId) REFERENCES dbo.ApplicationUsers(UserId)
);
CREATE UNIQUE INDEX IX_Referrals_ReferralNumber ON dbo.Referrals(ReferralNumber);

CREATE TABLE dbo.CallNotes
(
    CallNoteId uniqueidentifier NOT NULL CONSTRAINT PK_CallNotes PRIMARY KEY,
    PatientId uniqueidentifier NOT NULL,
    RecordedByUserId uniqueidentifier NOT NULL,
    CallDate datetime2 NOT NULL,
    Subject nvarchar(200) NOT NULL,
    Notes nvarchar(5000) NOT NULL,
    Outcome nvarchar(max) NULL,
    CONSTRAINT FK_CallNotes_Patients FOREIGN KEY(PatientId) REFERENCES dbo.Patients(PatientId) ON DELETE CASCADE,
    CONSTRAINT FK_CallNotes_RecordedBy FOREIGN KEY(RecordedByUserId) REFERENCES dbo.ApplicationUsers(UserId)
);

CREATE TABLE dbo.Assessments
(
    AssessmentId uniqueidentifier NOT NULL CONSTRAINT PK_Assessments PRIMARY KEY,
    PatientId uniqueidentifier NOT NULL,
    CompletedByUserId uniqueidentifier NOT NULL,
    CompletedAt datetime2 NULL,
    Status nvarchar(max) NOT NULL,
    Notes nvarchar(max) NULL,
    CONSTRAINT FK_Assessments_Patients FOREIGN KEY(PatientId) REFERENCES dbo.Patients(PatientId) ON DELETE CASCADE,
    CONSTRAINT FK_Assessments_CompletedBy FOREIGN KEY(CompletedByUserId) REFERENCES dbo.ApplicationUsers(UserId)
);

CREATE TABLE dbo.ComplianceRecords
(
    ComplianceRecordId uniqueidentifier NOT NULL CONSTRAINT PK_ComplianceRecords PRIMARY KEY,
    PatientId uniqueidentifier NOT NULL,
    RequirementCode nvarchar(max) NOT NULL,
    IsCompleted bit NOT NULL,
    CompletedAt datetime2 NULL,
    CompletedByUserId uniqueidentifier NULL,
    Notes nvarchar(max) NULL,
    CONSTRAINT FK_ComplianceRecords_Patients FOREIGN KEY(PatientId) REFERENCES dbo.Patients(PatientId) ON DELETE CASCADE
);

CREATE TABLE dbo.PatientTasks
(
    TaskId uniqueidentifier NOT NULL CONSTRAINT PK_PatientTasks PRIMARY KEY,
    PatientId uniqueidentifier NOT NULL,
    AssignedUserId uniqueidentifier NULL,
    Title nvarchar(max) NOT NULL,
    Description nvarchar(max) NOT NULL,
    DueDate datetime2 NOT NULL,
    Status nvarchar(30) NOT NULL,
    PageRoute nvarchar(max) NULL,
    CONSTRAINT FK_PatientTasks_Patients FOREIGN KEY(PatientId) REFERENCES dbo.Patients(PatientId) ON DELETE CASCADE,
    CONSTRAINT FK_PatientTasks_AssignedUser FOREIGN KEY(AssignedUserId) REFERENCES dbo.ApplicationUsers(UserId)
);

CREATE TABLE dbo.Activities
(
    ActivityId uniqueidentifier NOT NULL CONSTRAINT PK_Activities PRIMARY KEY,
    PatientId uniqueidentifier NOT NULL,
    PerformedByUserId uniqueidentifier NULL,
    ActivityDate datetime2 NOT NULL,
    ActivityType nvarchar(max) NOT NULL,
    Title nvarchar(max) NOT NULL,
    Description nvarchar(max) NOT NULL,
    CONSTRAINT FK_Activities_Patients FOREIGN KEY(PatientId) REFERENCES dbo.Patients(PatientId) ON DELETE CASCADE,
    CONSTRAINT FK_Activities_PerformedBy FOREIGN KEY(PerformedByUserId) REFERENCES dbo.ApplicationUsers(UserId)
);

CREATE TABLE dbo.Visits
(
    VisitId uniqueidentifier NOT NULL CONSTRAINT PK_Visits PRIMARY KEY,
    PatientId uniqueidentifier NOT NULL,
    ClinicianId uniqueidentifier NOT NULL,
    ScheduledDate datetime2 NOT NULL,
    CompletedDate datetime2 NULL,
    Status nvarchar(max) NOT NULL,
    Notes nvarchar(max) NULL,
    CONSTRAINT FK_Visits_Patients FOREIGN KEY(PatientId) REFERENCES dbo.Patients(PatientId) ON DELETE CASCADE,
    CONSTRAINT FK_Visits_Clinician FOREIGN KEY(ClinicianId) REFERENCES dbo.ApplicationUsers(UserId)
);

CREATE TABLE dbo.PatientServiceOrders
(
    PatientServiceOrderId uniqueidentifier NOT NULL CONSTRAINT PK_PatientServiceOrders PRIMARY KEY,
    PatientId uniqueidentifier NOT NULL,
    ServiceTypeId uniqueidentifier NOT NULL,
    Status nvarchar(30) NOT NULL,
    Frequency nvarchar(100) NULL,
    Duration nvarchar(100) NULL,
    IsPrimaryDiscipline bit NOT NULL,
    CONSTRAINT FK_PatientServiceOrders_Patients FOREIGN KEY(PatientId) REFERENCES dbo.Patients(PatientId) ON DELETE CASCADE,
    CONSTRAINT FK_PatientServiceOrders_ServiceTypes FOREIGN KEY(ServiceTypeId) REFERENCES dbo.ServiceTypes(ServiceTypeId)
);
CREATE UNIQUE INDEX IX_PatientServiceOrders_PatientId_ServiceTypeId ON dbo.PatientServiceOrders(PatientId, ServiceTypeId);
GO
