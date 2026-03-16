CREATE TABLE [dbo].[tbl_EmailLogs] (
    [Id]           BIGINT         IDENTITY (1, 1) NOT NULL,
    [StudentID]    NVARCHAR (128) NULL,
    [InactiveDays] INT            NULL,
    [Type]         INT            NULL,
    [Data]         NVARCHAR (MAX) NULL,
    [SentAt]       DATETIME       NULL,
    CONSTRAINT [PK_tbl_EmailLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
);

