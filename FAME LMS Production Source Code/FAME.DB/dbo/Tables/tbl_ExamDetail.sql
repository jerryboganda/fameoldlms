CREATE TABLE [dbo].[tbl_ExamDetail] (
    [ID]      INT IDENTITY (1, 1) NOT NULL,
    [PaperID] INT NULL,
    [ExamID]  INT NULL,
    CONSTRAINT [PK_tbl_ExamDetail] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_tbl_ExamDetail_tbl_Exam] FOREIGN KEY ([ExamID]) REFERENCES [dbo].[tbl_Exam] ([ExamID]) ON DELETE CASCADE,
    CONSTRAINT [FK_tbl_ExamDetail_tbl_QuestionPaper] FOREIGN KEY ([PaperID]) REFERENCES [dbo].[tbl_QuestionPaper] ([PaperID])
);

