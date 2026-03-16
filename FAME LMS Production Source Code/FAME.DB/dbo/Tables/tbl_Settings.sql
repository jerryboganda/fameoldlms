CREATE TABLE [dbo].[tbl_Settings] (
    [ID]    INT           IDENTITY (1, 1) NOT NULL,
    [Name]  VARCHAR (MAX) NULL,
    [Value] VARCHAR (MAX) NULL,
    CONSTRAINT [PK_tbl_Settings] PRIMARY KEY CLUSTERED ([ID] ASC)
);

