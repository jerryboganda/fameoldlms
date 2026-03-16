CREATE TABLE [dbo].[tbl_QuestionPaperDetail] (
    [ID]         INT IDENTITY (1, 1) NOT NULL,
    [QuestionID] INT NULL,
    [PaperID]    INT NULL,
    [Type]       INT NULL,
    CONSTRAINT [PK_tbl_QuestionPaperDetail] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_tbl_QuestionPaperDetail_tbl_Question] FOREIGN KEY ([QuestionID]) REFERENCES [dbo].[tbl_Question] ([Question_Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_tbl_QuestionPaperDetail_tbl_QuestionPaper] FOREIGN KEY ([PaperID]) REFERENCES [dbo].[tbl_QuestionPaper] ([PaperID]) ON DELETE CASCADE
);

