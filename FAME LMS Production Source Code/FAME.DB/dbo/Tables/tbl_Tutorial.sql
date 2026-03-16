CREATE TABLE [dbo].[tbl_Tutorial] (
    [TutorialID] INT            IDENTITY (1, 1) NOT NULL,
    [Title]      NVARCHAR (MAX) NULL,
    [Body]       NVARCHAR (MAX) NULL,
    [VideoLink]  NVARCHAR (MAX) NULL,
    [CategoryID] INT            NULL,
    [CreatedBy]  NVARCHAR (128) NULL,
    [CreatedDT]  DATETIME       NULL,
    [IsPopular]  BIT            NULL,
    CONSTRAINT [PK_tbl_Tutorial] PRIMARY KEY CLUSTERED ([TutorialID] ASC),
    CONSTRAINT [FK_tbl_Tutorial_tbl_Master] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[tbl_Master] ([Master_ID])
);

