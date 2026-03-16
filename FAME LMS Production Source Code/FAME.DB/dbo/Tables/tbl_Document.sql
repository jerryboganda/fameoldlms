CREATE TABLE [dbo].[tbl_Document] (
    [ID]          INT            IDENTITY (1, 1) NOT NULL,
    [Description] VARCHAR (5000) NULL,
    [Tags]        VARCHAR (500)  NULL,
    [Title]       VARCHAR (500)  NULL,
    [Doc_Form]    VARCHAR (50)   NULL,
    [Doc_Path]    NVARCHAR (MAX) NULL,
    [Date]        DATETIME       NULL,
    [Fid]         INT            NULL,
    CONSTRAINT [PK_tbl_Document] PRIMARY KEY CLUSTERED ([ID] ASC)
);

