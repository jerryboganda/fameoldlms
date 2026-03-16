CREATE TABLE [dbo].[tbl_QuestionPartition] (
    [ID]           INT             IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (1000) NULL,
    [IsActive]     BIT             NULL,
    [IsForStudent] BIT             NULL,
    CONSTRAINT [PK_tbl_QuestionPartition] PRIMARY KEY CLUSTERED ([ID] ASC)
);




GO
CREATE NONCLUSTERED INDEX [IX_tbl_QuestionPartition_ID_IsForStudent]
    ON [dbo].[tbl_QuestionPartition]([ID] ASC, [IsForStudent] ASC);

