CREATE TABLE [dbo].[tbl_StudentGroup] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [GroupTitle]   VARCHAR (500)  NULL,
    [Pic]          VARCHAR (500)  NULL,
    [UniversityID] INT            NULL,
    [Description]  VARCHAR (MAX)  NULL,
    [CreatedBy]    NVARCHAR (128) NULL,
    CONSTRAINT [PK_tbl_StudentGroup] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_StudentGroup_AspNetUsers] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

