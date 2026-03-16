CREATE TABLE [dbo].[tbl_ConversationReply] (
    [ID]        BIGINT         IDENTITY (1, 1) NOT NULL,
    [ConvID]    INT            NULL,
    [UserID]    NVARCHAR (128) NULL,
    [ReplyBody] NVARCHAR (MAX) NULL,
    [SentAt]    DATETIME       NULL,
    [ReadAt]    DATETIME       NULL,
    [Status]    VARCHAR (50)   NULL,
    CONSTRAINT [PK_tbl_ConversationReply] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_tbl_ConversationReply_tbl_Conversation] FOREIGN KEY ([ConvID]) REFERENCES [dbo].[tbl_Conversation] ([ConvID]) ON DELETE CASCADE
);

