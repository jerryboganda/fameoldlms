CREATE TABLE [dbo].[tbl_Video] (
    [Video_Id]               INT            IDENTITY (1, 1) NOT NULL,
    [Video_Name]             NVARCHAR (500) NOT NULL,
    [Video_ShortDescription] NVARCHAR (MAX) NULL,
    [Video_Length]           NUMERIC (18)   NULL,
    [Video_Path]             NVARCHAR (MAX) NOT NULL,
    [SectionSub_Fid]         INT            NULL,
    [Section_Fid]            INT            NULL,
    [Course_Fid]             INT            NULL,
    [IsActive]               BIT            NULL,
    [Type]                   INT            NULL,
    [SortID]                 INT            NULL,
    [Video_Tags]             NVARCHAR (MAX) NULL,
    [Difficulty]             NVARCHAR (50)  NULL,
    CONSTRAINT [PK_Video] PRIMARY KEY CLUSTERED ([Video_Id] ASC),
    CONSTRAINT [FK_tbl_Video_tbl_Courses] FOREIGN KEY ([Course_Fid]) REFERENCES [dbo].[tbl_Courses] ([Course_Id]),
    CONSTRAINT [FK_tbl_Video_tbl_Section] FOREIGN KEY ([Section_Fid]) REFERENCES [dbo].[tbl_Section] ([Section_ID]) ON DELETE CASCADE,
    CONSTRAINT [FK_tbl_Video_tbl_SectionSub] FOREIGN KEY ([SectionSub_Fid]) REFERENCES [dbo].[tbl_SectionSub] ([ID])
);




GO
CREATE NONCLUSTERED INDEX [IX_tbl_Video_Section_Fid]
    ON [dbo].[tbl_Video]([Section_Fid] ASC)
    INCLUDE([Video_Name], [Video_ShortDescription], [Video_Length], [Video_Path], [SectionSub_Fid], [Course_Fid], [IsActive], [Type], [SortID], [Video_Tags], [Difficulty]);


GO
CREATE NONCLUSTERED INDEX [IX_tbl_Video_Course_Fid]
    ON [dbo].[tbl_Video]([Course_Fid] ASC)
    INCLUDE([Video_Length]);

