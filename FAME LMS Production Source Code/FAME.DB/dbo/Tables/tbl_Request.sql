CREATE TABLE [dbo].[tbl_Request] (
    [RequestID]         INT            IDENTITY (1, 1) NOT NULL,
    [StudentID]         NVARCHAR (128) NOT NULL,
    [CourseID]          INT            NULL,
    [SectionID]         INT            NULL,
    [RequestDate]       DATETIME       NOT NULL,
    [Duration]          INT            NOT NULL,
    [IsAccepted]        BIT            NULL,
    [StudentEmail]      VARCHAR (100)  NULL,
    [PackageID]         INT            NULL,
    [PackageDurationID] INT            NULL,
    [Payment]           VARCHAR (50)   NULL,
    [TeacherFid]        NVARCHAR (128) NULL,
    [RequestFor]        VARCHAR (50)   NULL,
    CONSTRAINT [PK_tbl_Request] PRIMARY KEY CLUSTERED ([RequestID] ASC),
    CONSTRAINT [FK_tbl_Request_AspNetUsers] FOREIGN KEY ([StudentID]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_tbl_Request_tbl_Courses] FOREIGN KEY ([CourseID]) REFERENCES [dbo].[tbl_Courses] ([Course_Id]),
    CONSTRAINT [FK_tbl_Request_tbl_Package] FOREIGN KEY ([PackageID]) REFERENCES [dbo].[tbl_Package] ([PackageID]),
    CONSTRAINT [FK_tbl_Request_tbl_Section] FOREIGN KEY ([SectionID]) REFERENCES [dbo].[tbl_Section] ([Section_ID])
);




GO
CREATE NONCLUSTERED INDEX [IX_tbl_Request_StudentID]
    ON [dbo].[tbl_Request]([StudentID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_tbl_Request_RequestDate]
    ON [dbo].[tbl_Request]([RequestDate] ASC)
    INCLUDE([StudentID], [CourseID], [SectionID], [Duration], [IsAccepted], [StudentEmail], [PackageID], [Payment], [RequestFor]);


GO
CREATE NONCLUSTERED INDEX [IX_tbl_Request_IsAccepted_StudentID]
    ON [dbo].[tbl_Request]([StudentID] ASC, [IsAccepted] ASC);

