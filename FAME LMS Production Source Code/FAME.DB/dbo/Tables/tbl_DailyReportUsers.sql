CREATE TABLE [dbo].[tbl_DailyReportUsers] (
    [ID]        INT            IDENTITY (1, 1) NOT NULL,
    [FirstName]      NVARCHAR (MAX) NULL,
    [LastName]      NVARCHAR (MAX) NULL,
    [UserName]      NVARCHAR (MAX) NULL,
    [Password]      NVARCHAR (MAX) NULL,
    [CreatedDT] DATETIME       NULL,
    [ModifyDT] DATETIME       NULL,
    [Role]      VARCHAR (100)  NULL,
    CONSTRAINT [PK_DailyReportUsers] PRIMARY KEY CLUSTERED ([ID] ASC)
);

