CREATE TABLE [dbo].[tbl_UserLockout] (
    [ID]        INT            IDENTITY (1, 1) NOT NULL,
    [LockoutDT] DATETIME       NULL,
    [Remarks]   NVARCHAR (MAX) NULL,
    [AspUserID] NVARCHAR (128) NULL,
    CONSTRAINT [PK_tbl_UserLockout] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_tbl_UserLockout_AspNetUsers] FOREIGN KEY ([AspUserID]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

