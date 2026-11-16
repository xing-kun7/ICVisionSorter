IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'VisualSortingDB')
BEGIN
    CREATE DATABASE [VisualSortingDB] COLLATE Chinese_PRC_CI_AS;
END
GO

USE [VisualSortingDB];
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Member')
BEGIN
    CREATE TABLE [dbo].[Member](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Name] [varchar](50) NOT NULL,
        [Password] [varchar](50) NOT NULL,
        [Role] [int] NOT NULL,
        [InsertDate] [datetime] NULL,
        CONSTRAINT [PK_Member] PRIMARY KEY CLUSTERED ([Name] ASC)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductInspectionRecord')
BEGIN
    CREATE TABLE [dbo].[ProductInspectionRecord](
        [RecordId] [int] IDENTITY(1,1) NOT NULL,
        [ProductModel] [nvarchar](50) NOT NULL,
        [BatchNo] [nvarchar](50) NULL,
        [InspectionTime] [datetime] NOT NULL,
        [TotalInspectedCount] [int] NOT NULL,
        [QualifiedCount] [int] NOT NULL,
        [DefectiveCount] [int] NOT NULL,
        [DefectReasons] [nvarchar](1000) NULL,
        [OperatorId] [int] NULL,
        [DeviceNo] [nvarchar](50) NULL,
        [Remark] [nvarchar](500) NULL,
        CONSTRAINT [PK_ProductInspectionRecord] PRIMARY KEY CLUSTERED ([RecordId] ASC)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Member] WHERE [Name] = '半岛冰茶')
BEGIN
    INSERT INTO [dbo].[Member] ([Name], [Password], [Role], [InsertDate])
    VALUES ('半岛冰茶', '1816301413', 1, GETDATE());
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Member] WHERE [Name] = 'admin')
BEGIN
    INSERT INTO [dbo].[Member] ([Name], [Password], [Role], [InsertDate])
    VALUES ('admin', '123456', 1, GETDATE());
END
GO
