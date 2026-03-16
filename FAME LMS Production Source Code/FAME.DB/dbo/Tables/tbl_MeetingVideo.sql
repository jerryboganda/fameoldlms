CREATE TABLE [dbo].[tbl_MeetingVideo] (
    [MeetVideoID] INT            IDENTITY (1, 1) NOT NULL,
    [Tags]        VARCHAR (500)  NULL,
    [Description] VARCHAR (5000) NULL,
    [Title]       VARCHAR (500)  NULL,
    [Path]        VARCHAR (MAX)  NULL,
    [MeetID]      INT            NULL,
    CONSTRAINT [PK_tbl_MeetingVideo] PRIMARY KEY CLUSTERED ([MeetVideoID] ASC),
    CONSTRAINT [FK_tbl_MeetingVideo_tbl_Meeting] FOREIGN KEY ([MeetID]) REFERENCES [dbo].[tbl_Meeting] ([ID]) ON DELETE CASCADE
);

