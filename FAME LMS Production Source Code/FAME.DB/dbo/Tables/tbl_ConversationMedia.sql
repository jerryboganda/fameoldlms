CREATE TABLE [dbo].[tbl_ConversationMedia] (
    [ID]      INT            IDENTITY (1, 1) NOT NULL,
    [Path]    NVARCHAR (MAX) NULL,
    [ReplyID] BIGINT         NULL,
    CONSTRAINT [PK_tbl_ConversationMedia] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_tbl_ConversationMedia_tbl_ConversationReply] FOREIGN KEY ([ReplyID]) REFERENCES [dbo].[tbl_ConversationReply] ([ID]) ON DELETE CASCADE
);

