CREATE TABLE [dbo].[tbl_Progress] (
    [Video_Fid]   INT            NOT NULL,
    [Student_Fid] NVARCHAR (128) NOT NULL,
    [LastOpenDT]  DATETIME       NULL,
    [ScreenTime]  INT            NULL,
    [isCompleted] BIT            NULL,
    CONSTRAINT [PK__tbl_Prog__EF302ECFC72382C6] PRIMARY KEY CLUSTERED ([Video_Fid] ASC, [Student_Fid] ASC),
    CONSTRAINT [FK_tbl_Progress_AspNetUsers] FOREIGN KEY ([Student_Fid]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_tbl_Progress_tbl_Video] FOREIGN KEY ([Video_Fid]) REFERENCES [dbo].[tbl_Video] ([Video_Id]) ON DELETE CASCADE
);




GO
CREATE NONCLUSTERED INDEX [IX_tbl_Progress_StudentFid_LastOpenDT]
    ON [dbo].[tbl_Progress]([Student_Fid] ASC, [LastOpenDT] ASC)
    INCLUDE([ScreenTime]);


GO
CREATE NONCLUSTERED INDEX [IX_tbl_Progress_Student_Fid]
    ON [dbo].[tbl_Progress]([Student_Fid] ASC)
    INCLUDE([LastOpenDT], [ScreenTime], [isCompleted]);

