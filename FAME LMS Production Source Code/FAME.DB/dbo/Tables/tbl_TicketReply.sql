CREATE TABLE [dbo].[tbl_TicketReply] (
    [ID]        INT            IDENTITY (1, 1) NOT NULL,
    [Ticket_ID] INT            NULL,
    [FilePath]  VARCHAR (MAX)  NULL,
    [IsRead]    BIT            NULL,
    [ReplyBody] NVARCHAR (MAX) NULL,
    [SentTo]    NVARCHAR (128) NULL,
    [SendDT]    DATETIME       NULL,
    [SendBy]    NVARCHAR (128) NOT NULL,
    CONSTRAINT [PK_tbl_TicketReply] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_tbl_TicketReply_AspNetUsers] FOREIGN KEY ([SendBy]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_tbl_TicketReply_AspNetUsers1] FOREIGN KEY ([SentTo]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_tbl_TicketReply_tbl_Ticket1] FOREIGN KEY ([Ticket_ID]) REFERENCES [dbo].[tbl_Ticket] ([Ticket_ID]) ON DELETE CASCADE
);

