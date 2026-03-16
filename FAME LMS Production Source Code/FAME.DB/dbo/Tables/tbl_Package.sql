CREATE TABLE [dbo].[tbl_Package] (
    [PackageID]          INT            IDENTITY (1, 1) NOT NULL,
    [PackageName]        VARCHAR (200)  NULL,
    [PackageDescription] VARCHAR (200)  NULL,
    [PackagePrice]       DECIMAL (18)   NULL,
    [CreatedBy]          NVARCHAR (128) NULL,
    [CreatedDT]          DATETIME       NULL,
    [EndDate]            DATETIME       NULL,
    [Duration]           INT            NULL,
    [IsActive]           BIT            NULL,
    [SortID]             INT            NULL,
    [HasType]            BIT            NULL,
    CONSTRAINT [PK_tbl_Package] PRIMARY KEY CLUSTERED ([PackageID] ASC)
);

