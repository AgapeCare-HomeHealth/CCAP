/*
  CCAP Admin/Security upgrade for an existing SQL Server database.
  Run against the existing CCAP database after taking a backup.
  The script is idempotent for the tables it creates.
*/

IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
BEGIN
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
END;

IF OBJECT_ID(N'dbo.Permissions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Permissions
    (
        PermissionId uniqueidentifier NOT NULL CONSTRAINT PK_Permissions PRIMARY KEY,
        PermissionCode nvarchar(100) NOT NULL,
        PermissionName nvarchar(150) NOT NULL,
        Module nvarchar(100) NOT NULL,
        Description nvarchar(max) NULL
    );
    CREATE UNIQUE INDEX IX_Permissions_PermissionCode ON dbo.Permissions(PermissionCode);
END;

IF OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RolePermissions
    (
        RolePermissionId uniqueidentifier NOT NULL CONSTRAINT PK_RolePermissions PRIMARY KEY,
        RoleId uniqueidentifier NOT NULL,
        PermissionId uniqueidentifier NOT NULL,
        CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY(RoleId) REFERENCES dbo.Roles(RoleId) ON DELETE CASCADE,
        CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY(PermissionId) REFERENCES dbo.Permissions(PermissionId) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_RolePermissions_RoleId_PermissionId ON dbo.RolePermissions(RoleId, PermissionId);
END;

IF OBJECT_ID(N'dbo.Disciplines', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Disciplines
    (
        DisciplineId uniqueidentifier NOT NULL CONSTRAINT PK_Disciplines PRIMARY KEY,
        Code nvarchar(30) NOT NULL,
        Name nvarchar(100) NOT NULL,
        Description nvarchar(max) NULL
    );
    CREATE UNIQUE INDEX IX_Disciplines_Code ON dbo.Disciplines(Code);
END;

IF OBJECT_ID(N'dbo.ServiceTypes', N'U') IS NULL
BEGIN
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
END;

IF OBJECT_ID(N'dbo.ApplicationUsers', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ApplicationUsers_Roles')
        ALTER TABLE dbo.ApplicationUsers ADD CONSTRAINT FK_ApplicationUsers_Roles
            FOREIGN KEY(RoleId) REFERENCES dbo.Roles(RoleId);

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ApplicationUsers_Disciplines')
        ALTER TABLE dbo.ApplicationUsers ADD CONSTRAINT FK_ApplicationUsers_Disciplines
            FOREIGN KEY(DisciplineId) REFERENCES dbo.Disciplines(DisciplineId);
END;

IF OBJECT_ID(N'dbo.PatientServiceOrders', N'U') IS NULL
BEGIN
    IF OBJECT_ID(N'dbo.Patients', N'U') IS NOT NULL
    AND OBJECT_ID(N'dbo.ServiceTypes', N'U') IS NOT NULL
    BEGIN
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
    END;
END;
