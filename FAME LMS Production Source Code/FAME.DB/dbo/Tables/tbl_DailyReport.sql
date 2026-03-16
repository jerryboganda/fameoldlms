CREATE TABLE [dbo].[tbl_DailyReport] (
    [ID]        INT IDENTITY (1, 1) NOT NULL,
    [Body]      NVARCHAR (MAX) NULL,
    [Date]      DATETIME NULL,
    [UserFid]   INT NULL,
    [FilePath]  NVARCHAR (MAX) NULL,
    [CreatedDT] DATETIME NULL,
    [ModifyDT]  DATETIME NULL,
    CONSTRAINT [PK_DailyReport] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_DailyReport_User]
        FOREIGN KEY ([UserFid])
        REFERENCES [dbo].[tbl_DailyReportUsers]([ID])
        ON DELETE SET NULL
        ON UPDATE CASCADE
);