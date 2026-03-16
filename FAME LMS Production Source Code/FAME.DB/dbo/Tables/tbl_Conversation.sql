CREATE TABLE [dbo].[tbl_Conversation] (
    [ConvID]      INT            IDENTITY (1, 1) NOT NULL,
    [UserID_Two]  NVARCHAR (128) NULL,
    [UserID_One]  NVARCHAR (128) NULL,
    [Status]      VARCHAR (50)   NULL,
    [CourseID]    INT            NULL,
    [PackageID]   INT            NULL,
    [Title]       VARCHAR (500)  NULL,
    [Descriptiom] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_tbl_ChatConversation] PRIMARY KEY CLUSTERED ([ConvID] ASC)
);

