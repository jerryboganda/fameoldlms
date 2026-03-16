CREATE TABLE [dbo].[tbl_FileItems] (
    [Id]       INT            IDENTITY (1, 1) NOT NULL,
    [Name]     NVARCHAR (MAX) NULL,
    [MimeType] NVARCHAR (MAX) NULL,
    [Path]     NVARCHAR (MAX) NOT NULL,
    [IsFolder] BIT            NOT NULL,
    [CDate]    DATETIME       NOT NULL,
    [MDate]    DATETIME       NOT NULL,
    [PackID]   INT            NULL,
    [FileId]   INT            NULL,
    CONSTRAINT [PK_dbo.FileItems] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.FileItems_dbo.FileItems_FileId] FOREIGN KEY ([FileId]) REFERENCES [dbo].[tbl_FileItems] ([Id]),
    CONSTRAINT [FK_tbl_FileItems_tbl_Package] FOREIGN KEY ([PackID]) REFERENCES [dbo].[tbl_Package] ([PackageID])
);

