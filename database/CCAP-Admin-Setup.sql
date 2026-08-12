/*
    CCAP Admin / Authentication setup

    The API also runs DatabaseSeeder on startup for a fresh database.
    Use this script when an existing CCAP database needs the newly introduced
    service tables. Existing Roles/Permissions/ApplicationUsers are preserved.

    Development login:
      Email:    admin@ccap.local
      Password: Admin123!

    Change this password before any non-development deployment.
*/

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

    CREATE UNIQUE INDEX IX_ServiceTypes_Code
        ON dbo.ServiceTypes(Code);
END;

IF OBJECT_ID(N'dbo.PatientServiceOrders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PatientServiceOrders
    (
        PatientServiceOrderId uniqueidentifier NOT NULL
            CONSTRAINT PK_PatientServiceOrders PRIMARY KEY,
        PatientId uniqueidentifier NOT NULL,
        ServiceTypeId uniqueidentifier NOT NULL,
        Status nvarchar(30) NOT NULL,
        Frequency nvarchar(100) NULL,
        Duration nvarchar(100) NULL,
        IsPrimaryDiscipline bit NOT NULL
    );

    ALTER TABLE dbo.PatientServiceOrders
        ADD CONSTRAINT FK_PatientServiceOrders_Patients
        FOREIGN KEY (PatientId) REFERENCES dbo.Patients(PatientId)
        ON DELETE CASCADE;

    ALTER TABLE dbo.PatientServiceOrders
        ADD CONSTRAINT FK_PatientServiceOrders_ServiceTypes
        FOREIGN KEY (ServiceTypeId) REFERENCES dbo.ServiceTypes(ServiceTypeId)
        ON DELETE NO ACTION;

    CREATE UNIQUE INDEX IX_PatientServiceOrders_Patient_Service
        ON dbo.PatientServiceOrders(PatientId, ServiceTypeId);
END;
