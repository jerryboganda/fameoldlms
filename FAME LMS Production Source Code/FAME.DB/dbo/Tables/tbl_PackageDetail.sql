CREATE TABLE [dbo].[tbl_PackageDetail] (
    [DetailID]  INT          IDENTITY (1, 1) NOT NULL,
    [PackageID] INT          NULL,
    [CourseID]  INT          NULL,
    [Type]      VARCHAR (50) NULL,
    [Paper]     VARCHAR (50) NULL,
    CONSTRAINT [PK_tbl_PackageDetail] PRIMARY KEY CLUSTERED ([DetailID] ASC),
    CONSTRAINT [FK_tbl_PackageDetail_tbl_Courses] FOREIGN KEY ([CourseID]) REFERENCES [dbo].[tbl_Courses] ([Course_Id]),
    CONSTRAINT [FK_tbl_PackageDetail_tbl_Package] FOREIGN KEY ([PackageID]) REFERENCES [dbo].[tbl_Package] ([PackageID]) ON DELETE CASCADE
);

