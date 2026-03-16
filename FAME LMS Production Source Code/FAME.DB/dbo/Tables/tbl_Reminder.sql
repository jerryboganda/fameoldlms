CREATE TABLE [dbo].[tbl_Reminder] (
    [RemID]     INT            IDENTITY (1, 1) NOT NULL,
    [Reminder]  NVARCHAR (MAX) NULL,
    [UserID]    NVARCHAR (128) NULL,
    [DateFrom]  DATETIME       NULL,
    [IsFullDay] BIT            NULL,
    [DateTo]    DATETIME       NULL,
    [CreatedDT] DATETIME       NULL,
    CONSTRAINT [PK_tbl_Reminder] PRIMARY KEY CLUSTERED ([RemID] ASC),
    CONSTRAINT [FK_tbl_Reminder_AspNetUsers] FOREIGN KEY ([UserID]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

