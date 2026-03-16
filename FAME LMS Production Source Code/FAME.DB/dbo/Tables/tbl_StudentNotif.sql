CREATE TABLE [dbo].[tbl_StudentNotif] (
    [ID]            INT            IDENTITY (1, 1) NOT NULL,
    [UniversityIds] VARCHAR (MAX)  NULL,
    [PackageIds]    VARCHAR (MAX)  NULL,
    [Description]   VARCHAR (MAX)  NULL,
    [MessageTitle]  VARCHAR (MAX)  NULL,
    [MaessageBody]  VARCHAR (MAX)  NULL,
    [Banner]        VARCHAR (MAX)  NULL,
    [Status]        VARCHAR (MAX)  NULL,
    [PicturePath]   VARCHAR (MAX)  NULL,
    [IsPush]        BIT            NULL,
    [IsPopup]       BIT            NULL,
    [IsActive]      BIT            NULL,
    [CreatedBy]     NVARCHAR (128) NULL,
    [SendAt]        DATETIME       NULL,
    [CreatedDT]     DATETIME       NULL,
    CONSTRAINT [PK_tbl_StudentNotif] PRIMARY KEY CLUSTERED ([ID] ASC)
);
GO

