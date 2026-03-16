CREATE TABLE [dbo].[tbl_BookMistakes] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [BookEdition] VARCHAR (500) NULL,
    [BookName]    VARCHAR (500) NULL,
    [QuestionNo]  VARCHAR (500) NULL,
    [PageNo]      VARCHAR (500) NULL,
    [Subject]     VARCHAR (500) NULL,
    [Detail]      VARCHAR (MAX) NULL,
    [PicPath]     VARCHAR (MAX) NULL,
    [Correction]  VARCHAR (MAX) NULL,
    [Email]       VARCHAR (MAX) NULL,
    [Phone]       VARCHAR (MAX) NULL,
    [Name]        VARCHAR (MAX) NULL,
    [CreatedDT]   DATETIME      NULL,
    CONSTRAINT [PK_tbl_BookMistakes] PRIMARY KEY CLUSTERED ([Id] ASC)
);

