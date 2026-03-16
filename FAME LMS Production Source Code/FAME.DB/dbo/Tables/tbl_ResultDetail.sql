CREATE TABLE [dbo].[tbl_ResultDetail] (
    [ID]         INT            IDENTITY (1, 1) NOT NULL,
    [ResultID]   INT            NULL,
    [QuestionID] INT            NULL,
    [TimeSpent]  INT            NULL,
    [Answer]     NVARCHAR (MAX) NULL,
    [IsTrue]     BIT            NULL,
    CONSTRAINT [PK_tbl_ResultDetail] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_tbl_ResultDetail_tbl_Question] FOREIGN KEY ([QuestionID]) REFERENCES [dbo].[tbl_Question] ([Question_Id]),
    CONSTRAINT [FK_tbl_ResultDetail_tbl_ResultMaster] FOREIGN KEY ([ResultID]) REFERENCES [dbo].[tbl_ResultMaster] ([ID]) ON DELETE CASCADE
);




GO
CREATE NONCLUSTERED INDEX [IX_tbl_ResultDetail_ResultID_QuestionID]
    ON [dbo].[tbl_ResultDetail]([ResultID] ASC, [QuestionID] ASC)
    INCLUDE([IsTrue]);


GO
CREATE NONCLUSTERED INDEX [IX_tbl_ResultDetail_QuestionID]
    ON [dbo].[tbl_ResultDetail]([QuestionID] ASC);

