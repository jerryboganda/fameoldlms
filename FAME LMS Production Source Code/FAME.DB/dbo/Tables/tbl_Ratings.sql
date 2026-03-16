CREATE TABLE [dbo].[tbl_Ratings] (
    [ID]         BIGINT         IDENTITY (1, 1) NOT NULL,
    [UserID]     NVARCHAR (128) NULL,
    [ObjectID]   INT            NULL,
    [ObjectType] VARCHAR (50)   NULL,
    [Stars]      INT            NULL,
    CONSTRAINT [PK_tbl_Ratings] PRIMARY KEY CLUSTERED ([ID] ASC)
);

