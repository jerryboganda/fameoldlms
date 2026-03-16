CREATE TABLE [dbo].[tbl_UserDevRemReq] (
    [ID]         INT            IDENTITY (1, 1) NOT NULL,
    [DeviceID]   NVARCHAR (128) NULL,
    [UserID]     NVARCHAR (128) NULL,
    [Notes]      NVARCHAR (MAX) NULL,
    [DateTime]   DATETIME       NULL,
    [IsAccepted] BIT            NULL,
    CONSTRAINT [PK_tbl_UserDevRemReq] PRIMARY KEY CLUSTERED ([ID] ASC)
);

