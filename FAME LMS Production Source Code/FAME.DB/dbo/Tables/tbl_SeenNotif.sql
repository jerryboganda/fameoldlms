CREATE TABLE [dbo].[tbl_SeenNotif] (
    [ID]        BIGINT         IDENTITY (1, 1) NOT NULL,
    [NotifID]   INT            NULL,
    [StudentID] NVARCHAR (128) NULL,
    CONSTRAINT [PK_tbl_SeenNotif] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_tbl_SeenNotif_tbl_StudentNotif] FOREIGN KEY ([NotifID]) REFERENCES [dbo].[tbl_StudentNotif] ([ID]) ON DELETE CASCADE
);




GO
CREATE NONCLUSTERED INDEX [IX_tbl_SeenNotif_NotifID_StudentID]
    ON [dbo].[tbl_SeenNotif]([NotifID] ASC, [StudentID] ASC);

