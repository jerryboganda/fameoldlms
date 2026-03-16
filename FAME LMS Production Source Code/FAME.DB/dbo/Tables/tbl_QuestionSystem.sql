CREATE TABLE [dbo].[tbl_QuestionSystem] (
    [ID]           INT             IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (1000) NULL,
    [Section]      NVARCHAR (1000) NULL,
    [IsActive]     BIT             NULL,
    [IsForStudent] BIT             NULL,
    CONSTRAINT [PK_tbl_QuestionSystem] PRIMARY KEY CLUSTERED ([ID] ASC)
);
GO

CREATE NONCLUSTERED INDEX [IX_tbl_QuestionSystem_IsActive]
    ON [dbo].[tbl_QuestionSystem]([IsActive] ASC, [ID] ASC);
GO

