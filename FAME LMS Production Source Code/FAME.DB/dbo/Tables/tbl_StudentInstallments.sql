CREATE TABLE [dbo].[tbl_StudentInstallments] (
    [ID]               INT             IDENTITY (1, 1) NOT NULL,
    [EnrollmentID]     INT             NULL,
    [IsPaid]           BIT             NULL,
    [InstallmentDate]  DATETIME        NULL,
    [InstallmentNo]    INT             NULL,
    [StudentID]        NVARCHAR (128)  NULL,
    [InstallmentPrice] DECIMAL (18, 2) NULL,
    [PackageID]        INT             NULL,
    [PkgDuration]      INT             NULL,
    CONSTRAINT [PK_tbl_StudentInstallments] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_tbl_StudentInstallments_AspNetUsers] FOREIGN KEY ([StudentID]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_tbl_StudentInstallments_tbl_Enrollment] FOREIGN KEY ([EnrollmentID]) REFERENCES [dbo].[tbl_EnrollmentMaster] ([Enrollment_Id]),
    CONSTRAINT [FK_tbl_StudentInstallments_tbl_Package] FOREIGN KEY ([PackageID]) REFERENCES [dbo].[tbl_Package] ([PackageID])
);

