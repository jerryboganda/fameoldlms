CREATE TABLE [dbo].[tbl_Ticket] (
    [Ticket_ID]  INT            IDENTITY (1, 1) NOT NULL,
    [CategoryID] INT            NULL,
    [Subject]    VARCHAR (MAX)  NULL,
    [Body]       VARCHAR (MAX)  NULL,
    [IssuedTo]   NVARCHAR (128) NULL,
    [IsPopular]  BIT            NULL,
    [Status]     VARCHAR (50)   NULL,
    [CreatedDT]  DATETIME       NULL,
    [CreatedBy]  NVARCHAR (128) NULL,
    CONSTRAINT [PK_tbl_Ticket] PRIMARY KEY CLUSTERED ([Ticket_ID] ASC),
    CONSTRAINT [FK_tbl_Ticket_AspNetUsers] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_tbl_Ticket_AspNetUsers1] FOREIGN KEY ([IssuedTo]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_tbl_Ticket_tbl_Master] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[tbl_Master] ([Master_ID])
);

