CREATE TABLE [dbo].[tbl_Section] (
    [Section_ID]    INT          IDENTITY (1, 1) NOT NULL,
    [Section_Name]  NCHAR (100)  NOT NULL,
    [Section_Price] DECIMAL (18) NOT NULL,
    [Course_Fid]    INT          NOT NULL,
    [SortID]        INT          NULL,
    [IsActive]      BIT          NULL,
    CONSTRAINT [PK_tbl_Section] PRIMARY KEY CLUSTERED ([Section_ID] ASC),
    CONSTRAINT [FK_tbl_Section_tbl_Courses] FOREIGN KEY ([Course_Fid]) REFERENCES [dbo].[tbl_Courses] ([Course_Id])
);

