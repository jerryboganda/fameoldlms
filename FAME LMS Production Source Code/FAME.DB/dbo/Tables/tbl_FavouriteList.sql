CREATE TABLE [dbo].[tbl_FavouriteList] (
    [ID]          BIGINT         IDENTITY (1, 1) NOT NULL,
    [Student_Fid] NVARCHAR (128) NULL,
    [Object_ID]   INT            NULL,
    [ObjectType]  VARCHAR (50)   NULL,
    [MarkAs]      VARCHAR (MAX)  NULL,
    CONSTRAINT [PK_Wishlist] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_tbl_Wishlist_AspNetUsers] FOREIGN KEY ([Student_Fid]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);




GO
CREATE NONCLUSTERED INDEX [IX_tbl_FavouriteList_Student_Object]
    ON [dbo].[tbl_FavouriteList]([Student_Fid] ASC, [ObjectType] ASC, [Object_ID] ASC);

