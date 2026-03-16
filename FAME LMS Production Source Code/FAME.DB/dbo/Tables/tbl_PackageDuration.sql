CREATE TABLE [dbo].[tbl_PackageDuration] (
    [ID]        INT          IDENTITY (1, 1) NOT NULL,
    [PackageID] INT          NULL,
    [Duration]  INT          NULL,
    [Price]     DECIMAL (18) NULL,
    CONSTRAINT [PK_tbl_PackageDuration] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_tbl_PackageDuration_tbl_Package] FOREIGN KEY ([PackageID]) REFERENCES [dbo].[tbl_Package] ([PackageID]) ON DELETE CASCADE
);

