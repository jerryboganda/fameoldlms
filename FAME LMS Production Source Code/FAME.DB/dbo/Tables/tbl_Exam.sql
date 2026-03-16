CREATE TABLE [dbo].[tbl_Exam] (
    [ExamID]          INT            IDENTITY (1, 1) NOT NULL,
    [ExamTitle]       VARCHAR (200)  NULL,
    [Time]            TIME (7)       NULL,
    [Thumbnail]       VARCHAR (MAX)  NULL,
    [TopicsCovered]   VARCHAR (MAX)  NULL,
    [PackageIDs]      VARCHAR (MAX)  NULL,
    [CourseIDs]       VARCHAR (MAX)  NULL,
    [Tags]            VARCHAR (MAX)  NULL,
    [Price]           INT            NULL,
    [DifficultyLevel] INT            NULL,
    [Description]     VARCHAR (MAX)  NULL,
    [CreatedBy]       NVARCHAR (128) NULL,
    CONSTRAINT [PK_tbl_Exam] PRIMARY KEY CLUSTERED ([ExamID] ASC)
);

