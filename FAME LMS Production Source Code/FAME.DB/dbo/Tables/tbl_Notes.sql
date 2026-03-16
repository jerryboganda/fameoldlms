CREATE TABLE [dbo].[tbl_Notes] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [FID]       INT            NULL,
    [Notes]     NVARCHAR (MAX) NULL,
    [Type]      VARCHAR (50)   NULL,
    [CreatedDT] DATETIME       NULL,
    [StudentID] NVARCHAR (128) NULL,
    CONSTRAINT [PK_tbl_Notes] PRIMARY KEY CLUSTERED ([Id] ASC)
);

