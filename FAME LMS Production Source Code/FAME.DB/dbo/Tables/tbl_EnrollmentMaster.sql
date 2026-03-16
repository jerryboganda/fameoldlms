CREATE TABLE [dbo].[tbl_EnrollmentMaster] (
    [Enrollment_Id]      INT            IDENTITY (1, 1) NOT NULL,
    [Enrollment_Date]    DATETIME       NOT NULL,
    [Enrollment_Status]  VARCHAR (50)   NULL,
    [Enrollment_Price]   NUMERIC (18)   NOT NULL,
    [Enrollment_EndDate] DATE           NULL,
    [TeacherFid]         NVARCHAR (128) NULL,
    [CouponID]           INT            NULL,
    [Enrollment_No]      INT            NULL,
    [IsExpired]          BIT            NULL,
    [PackageId]          INT            NULL,
    [PackageDurationFid] INT            NULL,
    [StudentFid]         NVARCHAR (128) NULL,
    [ByRequest]          BIT            NULL,
    [ByManual]           BIT            NULL,
    [ApprovedBy]         NVARCHAR (128) NULL,
    [IsEmailSent]        BIT            NULL,
    [ByExtension]        BIT            NULL,
    CONSTRAINT [PK_Enrollment] PRIMARY KEY CLUSTERED ([Enrollment_Id] ASC),
    CONSTRAINT [FK_tbl_Enrollment_tbl_Coupon] FOREIGN KEY ([CouponID]) REFERENCES [dbo].[tbl_Coupon] ([Id]),
    CONSTRAINT [FK_tbl_Enrollment_tbl_PackageDetail] FOREIGN KEY ([PackageDurationFid]) REFERENCES [dbo].[tbl_PackageDuration] ([ID]),
    CONSTRAINT [FK_tbl_EnrollmentMaster_AspNetUsersAppr] FOREIGN KEY ([ApprovedBy]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_tbl_EnrollmentMaster_AspNetUsersS] FOREIGN KEY ([StudentFid]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_tbl_EnrollmentMaster_tbl_Package] FOREIGN KEY ([PackageId]) REFERENCES [dbo].[tbl_Package] ([PackageID])
);




GO
CREATE NONCLUSTERED INDEX [IX_tbl_EnrollmentMaster_StudentFid_EnrollmentEndDate_PackageId]
    ON [dbo].[tbl_EnrollmentMaster]([StudentFid] ASC, [Enrollment_EndDate] ASC, [PackageId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_tbl_EnrollmentMaster_StudentFid_EnrollmentEndDate_IsExpired]
    ON [dbo].[tbl_EnrollmentMaster]([StudentFid] ASC, [Enrollment_EndDate] ASC, [IsExpired] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_tbl_EnrollmentMaster_StudentFid_EnrollmentEndDate]
    ON [dbo].[tbl_EnrollmentMaster]([StudentFid] ASC)
    INCLUDE([Enrollment_EndDate]);


GO
CREATE NONCLUSTERED INDEX [IX_tbl_EnrollmentMaster_PackageId_StudentFid]
    ON [dbo].[tbl_EnrollmentMaster]([PackageId] ASC, [StudentFid] ASC)
    INCLUDE([Enrollment_EndDate]);


GO
CREATE NONCLUSTERED INDEX [IX_tbl_EnrollmentMaster_PackageDurationFid]
    ON [dbo].[tbl_EnrollmentMaster]([PackageDurationFid] ASC);

