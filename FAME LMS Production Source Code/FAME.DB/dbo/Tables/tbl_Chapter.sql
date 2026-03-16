CREATE TABLE [dbo].[tbl_Chapter] (
    [ChapterID]      INT            IDENTITY (1, 1) NOT NULL,
    [Course_Id]      INT            NULL,
    [Title]          NVARCHAR (MAX) NULL,
    [ChapterContent] NVARCHAR (MAX) NULL,
    [SortID]         INT            NULL,
    CONSTRAINT [PK_tbl_Chapter] PRIMARY KEY CLUSTERED ([ChapterID] ASC),
    CONSTRAINT [FK_tbl_Chapter_tbl_Courses] FOREIGN KEY ([Course_Id]) REFERENCES [dbo].[tbl_Courses] ([Course_Id]) ON DELETE CASCADE
);

