IF DB_ID(N'ScenaStudioCoursework') IS NULL
    CREATE DATABASE [ScenaStudioCoursework];
GO

USE [ScenaStudioCoursework];
GO

IF OBJECT_ID(N'dbo.Customers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Customers (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Customers PRIMARY KEY,
        Name nvarchar(100) NOT NULL,
        Phone nvarchar(20) NOT NULL,
        Email nvarchar(100) NOT NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.Customers') AND name = N'IX_Customers_Phone'
)
    CREATE INDEX IX_Customers_Phone ON dbo.Customers(Phone);
GO

IF OBJECT_ID(N'dbo.Enrollments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Enrollments (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Enrollments PRIMARY KEY,
        CustomerId int NOT NULL,
        CourseId int NOT NULL,
        GroupNumber int NOT NULL,
        Quantity int NOT NULL,
        Total int NOT NULL,
        PaymentMethod nvarchar(max) NOT NULL,
        Status nvarchar(30) NOT NULL,
        CreatedAt datetime2 NOT NULL,
        CONSTRAINT FK_Enrollments_Customers_CustomerId FOREIGN KEY (CustomerId)
            REFERENCES dbo.Customers(Id) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.Enrollments') AND name = N'IX_Enrollments_CustomerId'
)
    CREATE INDEX IX_Enrollments_CustomerId ON dbo.Enrollments(CustomerId);
GO

IF OBJECT_ID(N'dbo.SupportMessages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SupportMessages (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_SupportMessages PRIMARY KEY,
        CustomerName nvarchar(max) NOT NULL,
        Text nvarchar(2000) NOT NULL,
        Reply nvarchar(2000) NOT NULL,
        CreatedAt datetime2 NOT NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.Reviews', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reviews (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Reviews PRIMARY KEY,
        Name nvarchar(100) NOT NULL,
        Text nvarchar(2000) NOT NULL,
        Rating int NOT NULL,
        CreatedAt datetime2 NOT NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.AppVisits', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AppVisits (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AppVisits PRIMARY KEY,
        SessionKey uniqueidentifier NOT NULL,
        VisitorName nvarchar(120) NOT NULL,
        StartedAt datetime2 NOT NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.AppVisits') AND name = N'IX_AppVisits_StartedAt'
)
    CREATE INDEX IX_AppVisits_StartedAt ON dbo.AppVisits(StartedAt);
GO

