CREATE TABLE [dbo].[tbl_Question] (
    [Question_Id]     INT            IDENTITY (1, 1) NOT NULL,
    [Question]        NVARCHAR (MAX) NULL,
    [AnsExplain]      NVARCHAR (MAX) NULL,
    [Answer]          NVARCHAR (MAX) NULL,
    [Tags]            NVARCHAR (MAX) NULL,
    [Time]            DATETIME       NULL,
    [Marks]           INT            NULL,
    [DifficultyLevel] INT            NULL,
    [Type]            INT            NOT NULL,
    [Section_Fid]     INT            NULL,
    [VideoID]         INT            NULL,
    [Course_Fid]      INT            NULL,
    [IsMultiAns]      BIT            NULL,
    [IsPopular]       BIT            NULL,
    [CategoryID]      INT            NULL,
    [CreatedBy]       NVARCHAR (128) NULL,
    [PartitionID]     INT            NULL,
    [SystemID]        INT            NULL,
    CONSTRAINT [PK_Feedback] PRIMARY KEY CLUSTERED ([Question_Id] ASC),
    CONSTRAINT [FK_tbl_Question_tbl_Courses] FOREIGN KEY ([Course_Fid]) REFERENCES [dbo].[tbl_Courses] ([Course_Id]),
    CONSTRAINT [FK_tbl_Question_tbl_Master] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[tbl_Master] ([Master_ID]),
    CONSTRAINT [FK_tbl_Question_tbl_QuestionPartition] FOREIGN KEY ([PartitionID]) REFERENCES [dbo].[tbl_QuestionPartition] ([ID]),
    CONSTRAINT [FK_tbl_Question_tbl_QuestionSystem] FOREIGN KEY ([SystemID]) REFERENCES [dbo].[tbl_QuestionSystem] ([ID]),
    CONSTRAINT [FK_tbl_Question_tbl_Section] FOREIGN KEY ([Section_Fid]) REFERENCES [dbo].[tbl_Section] ([Section_ID])
);






GO
CREATE NONCLUSTERED INDEX [IX_tbl_Question_Type_PartitionID]
    ON [dbo].[tbl_Question]([Type] ASC, [PartitionID] ASC)
    INCLUDE([SystemID], [Question_Id]);


GO
CREATE NONCLUSTERED INDEX [IX_tbl_Question_Type]
    ON [dbo].[tbl_Question]([Type] ASC)
    INCLUDE([Course_Fid]);

