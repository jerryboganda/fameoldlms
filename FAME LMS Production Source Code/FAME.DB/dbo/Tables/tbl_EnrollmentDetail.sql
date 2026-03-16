CREATE TABLE [dbo].[tbl_EnrollmentDetail] (
    [ID]           INT IDENTITY (1, 1) NOT NULL,
    [EnrollmentID] INT NULL,
    [Section_Fid]  INT NULL,
    [CourseID]     INT NULL,
    [IsExpired]    BIT NULL,
    CONSTRAINT [PK_tbl_EnrollmentDetail] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_tbl_EnrollmentDetail_tbl_Courses] FOREIGN KEY ([CourseID]) REFERENCES [dbo].[tbl_Courses] ([Course_Id]),
    CONSTRAINT [FK_tbl_EnrollmentDetail_tbl_EnrollmentMaster] FOREIGN KEY ([EnrollmentID]) REFERENCES [dbo].[tbl_EnrollmentMaster] ([Enrollment_Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_tbl_EnrollmentDetail_tbl_Section] FOREIGN KEY ([Section_Fid]) REFERENCES [dbo].[tbl_Section] ([Section_ID])
);




GO
CREATE NONCLUSTERED INDEX [IX_tbl_EnrollmentDetail_Section_Fid]
    ON [dbo].[tbl_EnrollmentDetail]([Section_Fid] ASC)
    INCLUDE([EnrollmentID], [IsExpired]);


GO
CREATE NONCLUSTERED INDEX [IX_tbl_EnrollmentDetail_EnrollmentID_CourseID]
    ON [dbo].[tbl_EnrollmentDetail]([EnrollmentID] ASC, [CourseID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_tbl_EnrollmentDetail_EnrollmentID]
    ON [dbo].[tbl_EnrollmentDetail]([EnrollmentID] ASC)
    INCLUDE([Section_Fid], [CourseID], [IsExpired]);


GO
CREATE NONCLUSTERED INDEX [IX_tbl_EnrollmentDetail_CourseID]
    ON [dbo].[tbl_EnrollmentDetail]([CourseID] ASC);

