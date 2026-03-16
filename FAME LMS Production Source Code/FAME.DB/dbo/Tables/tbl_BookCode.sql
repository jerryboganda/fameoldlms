CREATE TABLE [dbo].[tbl_BookCode] (
    [ID]        INT            IDENTITY (1, 1) NOT NULL,
    [BookCode]  VARCHAR (100)  NOT NULL,
    [Notes]     VARCHAR (MAX)  NULL,
    [IsUsed]    BIT            NULL,
    [UsedAt]    DATETIME       NULL,
    [UsedBy]    NVARCHAR (128) NULL,
    [CreatedBy] NVARCHAR (128) NULL,
    [CreatedDT] DATETIME       NULL,
    CONSTRAINT [PK_tbl_BookCode] PRIMARY KEY CLUSTERED ([BookCode] ASC),
    CONSTRAINT [FK_tbl_BookCode_AspNetUsers] FOREIGN KEY ([UsedBy]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE SET NULL
);




GO
CREATE NONCLUSTERED INDEX [IX_tbl_BookCode_UsedBy_UsedAt]
    ON [dbo].[tbl_BookCode]([UsedBy] ASC) WHERE ([UsedAt] IS NOT NULL);

