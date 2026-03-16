CREATE TABLE [dbo].[tbl_CourseDetails] (
    [CourseDetail_ID]   INT           IDENTITY (1, 1) NOT NULL,
    [CourseDetail_Body] VARCHAR (200) NULL,
    [Course_Fid]        INT           NULL,
    CONSTRAINT [PK_tbl_CourseDescription] PRIMARY KEY CLUSTERED ([CourseDetail_ID] ASC),
    CONSTRAINT [FK_tbl_CourseDetails_tbl_Courses] FOREIGN KEY ([Course_Fid]) REFERENCES [dbo].[tbl_Courses] ([Course_Id]) ON DELETE CASCADE ON UPDATE CASCADE
);

