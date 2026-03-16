CREATE TABLE [dbo].[tbl_Friends] (
    [ID]             INT            IDENTITY (1, 1) NOT NULL,
    [FriendOne]      NVARCHAR (128) NULL,
    [FriendTwo]      NVARCHAR (128) NULL,
    [Type]           VARCHAR (50)   NULL,
    [Status]         VARCHAR (50)   NULL,
    [FriendshipDate] DATETIME       NULL,
    CONSTRAINT [PK_tbl_Friends] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_tbl_Friends_AspNetUsers] FOREIGN KEY ([FriendOne]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_tbl_Friends_AspNetUsers1] FOREIGN KEY ([FriendTwo]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

