CREATE TABLE [dbo].[tbl_FriendRequest] (
    [ReqID]          INT            IDENTITY (1, 1) NOT NULL,
    [FutureFriendID] NVARCHAR (128) NULL,
    [Status]         NVARCHAR (50)  NULL,
    [ExpiresDate]    DATETIME       NULL,
    [UserID]         NVARCHAR (128) NULL,
    CONSTRAINT [PK_tbl_FriendRequest] PRIMARY KEY CLUSTERED ([ReqID] ASC)
);

