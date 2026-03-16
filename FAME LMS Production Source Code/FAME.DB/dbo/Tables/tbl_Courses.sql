CREATE TABLE [dbo].[tbl_Courses] (
    [Course_Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Course_Name]        NVARCHAR (500) NOT NULL,
    [Course_Description] NVARCHAR (MAX) NULL,
    [Course_Price]       DECIMAL (18)   NULL,
    [Course_Pic]         NVARCHAR (MAX) NULL,
    [TeacherFid]         NVARCHAR (128) NULL,
    [SortID]             INT            NULL,
    [IsActive]           BIT            NULL,
    CONSTRAINT [PK_Course] PRIMARY KEY CLUSTERED ([Course_Id] ASC),
    CONSTRAINT [FK_tbl_Courses_AspNetUsers] FOREIGN KEY ([TeacherFid]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

