CREATE TABLE [dbo].[tbl_UserLogins] (
    [ID]             BIGINT         IDENTITY (1, 1) NOT NULL,
    [DeviceID]       NVARCHAR (128) NULL,
    [UserID]         NVARCHAR (128) NULL,
    [IsLocalStorage] BIT            NULL,
    [IsCookie]       BIT            NULL,
    [IpAddress]      VARCHAR (100)  NULL,
    [Status]         VARCHAR (100)  NULL,
    [DateTime]       DATETIME       NULL,
    CONSTRAINT [PK_tbl_UserLogins] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_tbl_UserLogins_AspNetUsers] FOREIGN KEY ([UserID]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);




GO
CREATE NONCLUSTERED INDEX [IX_tbl_UserLogins_UserID_DeviceID_DateTime]
    ON [dbo].[tbl_UserLogins]([UserID] ASC)
    INCLUDE([DeviceID], [DateTime]);


GO
CREATE NONCLUSTERED INDEX [IX_tbl_UserLogins_UserID]
    ON [dbo].[tbl_UserLogins]([UserID] ASC);

