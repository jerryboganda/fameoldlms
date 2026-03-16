CREATE TABLE [dbo].[tbl_StudentGroupDetail] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [GroupID]   INT            NULL,
    [StudentID] NVARCHAR (128) NULL,
    [IsLeader]  BIT            NULL,
    CONSTRAINT [PK_tbl_StudentGroupDetail] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_StudentGroupDetail_tbl_StudentGroup] FOREIGN KEY ([GroupID]) REFERENCES [dbo].[tbl_StudentGroup] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_tbl_StudentGroupDetail_tbl_User] FOREIGN KEY ([StudentID]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

