CREATE TABLE [dbo].[tbl_Options] (
    [ID]           INT            IDENTITY (1, 1) NOT NULL,
    [OptionDetail] NVARCHAR (MAX) NULL,
    [OptionText]   NVARCHAR (MAX) NULL,
    [Question_Id]  INT            NULL,
    [IsCorrect]    BIT            NULL,
    CONSTRAINT [PK_tbl_Options] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_tbl_Options_tbl_Question] FOREIGN KEY ([Question_Id]) REFERENCES [dbo].[tbl_Question] ([Question_Id]) ON DELETE CASCADE
);




GO
CREATE NONCLUSTERED INDEX [IX_tbl_Options_Question_Id]
    ON [dbo].[tbl_Options]([Question_Id] ASC)
    INCLUDE([OptionDetail], [OptionText], [IsCorrect]);

