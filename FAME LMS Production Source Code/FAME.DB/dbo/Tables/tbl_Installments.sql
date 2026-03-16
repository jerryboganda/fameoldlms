CREATE TABLE [dbo].[tbl_Installments] (
    [InstallmentID]     INT             IDENTITY (1, 1) NOT NULL,
    [InstallmentPrice]  DECIMAL (18, 2) NULL,
    [Duration]          INT             NULL,
    [PackageDurationID] INT             NULL,
    CONSTRAINT [PK_tbl_Installments] PRIMARY KEY CLUSTERED ([InstallmentID] ASC),
    CONSTRAINT [FK_tbl_Installments_tbl_PackageDuration] FOREIGN KEY ([PackageDurationID]) REFERENCES [dbo].[tbl_PackageDuration] ([ID]) ON DELETE CASCADE
);

