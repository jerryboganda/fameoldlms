CREATE TABLE [dbo].[tbl_Meeting] (
    [ID]         INT            IDENTITY (1, 1) NOT NULL,
    [MeetingNo]  BIGINT         NULL,
    [Duration]   INT            NULL,
    [PackageIDs] NVARCHAR (500) NULL,
    [CourseIDs]  NVARCHAR (500) NULL,
    [Topic]      NVARCHAR (500) NULL,
    [JoinUrl]    NVARCHAR (MAX) NULL,
    [Status]     NVARCHAR (50)  NULL,
    [Password]   NVARCHAR (50)  NULL,
    [CreatedBy]  NVARCHAR (128) NULL,
    [StartTime]  DATETIME       NULL,
    [CreatedDT]  DATETIME       NULL,
    CONSTRAINT [PK_tbl_Meeting] PRIMARY KEY CLUSTERED ([ID] ASC)
);

