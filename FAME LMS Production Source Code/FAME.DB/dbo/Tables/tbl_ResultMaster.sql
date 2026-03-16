CREATE TABLE [dbo].[tbl_ResultMaster] (
    [ID]              INT            IDENTITY (1, 1) NOT NULL,
    [Datetime]        DATETIME       NULL,
    [TrialStudent]    NVARCHAR (MAX) NULL,
    [StudentID]       NVARCHAR (128) NULL,
    [QuestionPaperID] INT            NULL,
    [ObtainedMarks]   INT            NULL,
    [TotalMarks]      INT            NULL,
    [Mode]            VARCHAR (50)   NULL,
    [TimeElapsed]     INT            NULL,
    [TimeGiven]       INT            NULL,
    [AllowReopen]     BIT            NULL,
    CONSTRAINT [PK_tbl_ResultMaster] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_tbl_ResultMaster_AspNetUsers] FOREIGN KEY ([StudentID]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_tbl_ResultMaster_tbl_QuestionPaper] FOREIGN KEY ([QuestionPaperID]) REFERENCES [dbo].[tbl_QuestionPaper] ([PaperID])
);




GO
CREATE NONCLUSTERED INDEX [IX_tbl_ResultMaster_StudentID]
    ON [dbo].[tbl_ResultMaster]([StudentID] ASC, [ID] ASC);

