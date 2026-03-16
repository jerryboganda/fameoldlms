CREATE TABLE [dbo].[tbl_Master] (
    [Master_ID]    INT           IDENTITY (1, 1) NOT NULL,
    [Master_Name]  VARCHAR (50)  NULL,
    [Master_Value] VARCHAR (100) NULL,
    [Master_Group] INT           NULL,
    [IsActive]     BIT           NULL,
    CONSTRAINT [PK_tbl_Master] PRIMARY KEY CLUSTERED ([Master_ID] ASC)
);

