CREATE TABLE [dbo].[tbl_SectionSub] (
    [ID]              INT         IDENTITY (1, 1) NOT NULL,
    [SectionSub_Name] NCHAR (100) NOT NULL,
    [Course_Fid]      INT         NOT NULL,
    [Section_Fid]     INT         NOT NULL,
    [SortID]          INT         NULL,
    [IsActive]        BIT         NULL,
    CONSTRAINT [PK_tbl_SectionSub] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_tbl_SectionSub_tbl_Courses] FOREIGN KEY ([Course_Fid]) REFERENCES [dbo].[tbl_Courses] ([Course_Id]),
    CONSTRAINT [FK_tbl_SectionSub_tbl_Section] FOREIGN KEY ([Section_Fid]) REFERENCES [dbo].[tbl_Section] ([Section_ID])
);

