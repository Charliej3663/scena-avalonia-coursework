CREATE DATABASE ScenaStudio;
GO
USE ScenaStudio;
GO
CREATE TABLE Customers (Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Customers PRIMARY KEY, Name nvarchar(100) NOT NULL, Phone nvarchar(20) NOT NULL, Email nvarchar(100) NOT NULL);
CREATE INDEX IX_Customers_Phone ON Customers(Phone);
CREATE TABLE Enrollments (Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Enrollments PRIMARY KEY, CustomerId int NOT NULL, CourseId int NOT NULL, GroupNumber int NOT NULL, Quantity int NOT NULL, Total int NOT NULL, PaymentMethod nvarchar(max) NOT NULL, Status nvarchar(30) NOT NULL, CreatedAt datetime2 NOT NULL, CONSTRAINT FK_Enrollments_Customers_CustomerId FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE);
CREATE INDEX IX_Enrollments_CustomerId ON Enrollments(CustomerId);
CREATE TABLE SupportMessages (Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_SupportMessages PRIMARY KEY, CustomerName nvarchar(max) NOT NULL, Text nvarchar(2000) NOT NULL, Reply nvarchar(2000) NOT NULL, CreatedAt datetime2 NOT NULL);
CREATE TABLE Reviews (Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Reviews PRIMARY KEY, Name nvarchar(100) NOT NULL, Text nvarchar(2000) NOT NULL, Rating int NOT NULL, CreatedAt datetime2 NOT NULL);
GO
