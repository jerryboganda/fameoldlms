CREATE TABLE [dbo].[tbl_UserDevices] (
    [ID]         INT            IDENTITY (1, 1) NOT NULL,
    [DeviceID]   NVARCHAR (MAX) NOT NULL,
    [UserID]     NVARCHAR (128) NOT NULL,
    [DeviceName] VARCHAR (200)  NULL,
    [UserAgent]  NVARCHAR (MAX) NULL,
    [IsMobile]   BIT            NULL,
    [IpAddress]  VARCHAR (100)  NULL,
    [IsApp]      BIT            NULL,
    [IsActive]   BIT            NULL,
    [CreatedDT]  DATETIME       NULL,
    CONSTRAINT [PK_tbl_UserDevices] PRIMARY KEY CLUSTERED ([ID] ASC)
);




GO
CREATE NONCLUSTERED INDEX [IX_tbl_UserDevices_UserID_IsActive]
    ON [dbo].[tbl_UserDevices]([UserID] ASC, [IsActive] ASC)
    INCLUDE([DeviceID], [DeviceName], [UserAgent], [IsMobile], [IpAddress], [IsApp], [CreatedDT]);


GO
CREATE NONCLUSTERED INDEX [IX_tbl_UserDevices_UserID]
    ON [dbo].[tbl_UserDevices]([UserID] ASC);

