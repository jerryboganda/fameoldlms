CREATE TABLE [dbo].[tbl_Comment] (
    [ID]           INT            IDENTITY (1, 1) NOT NULL,
    [Comment_Body] NVARCHAR (MAX) NULL,
    [Comment_Date] DATETIME       NULL,
    [QuestionID]   INT            NULL,
    [Video_Fid]    INT            NULL,
    [UserFid]      NVARCHAR (128) NULL,
    [IsApproved]   BIT            NULL,
    [ApprovedBy]   NVARCHAR (128) NULL,
    [ApprovedDT]   DATETIME       NULL,
    CONSTRAINT [PK_Comment] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_tbl_Comment_AspNetUsers] FOREIGN KEY ([UserFid]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_tbl_Comment_tbl_Video] FOREIGN KEY ([Video_Fid]) REFERENCES [dbo].[tbl_Video] ([Video_Id])
);

