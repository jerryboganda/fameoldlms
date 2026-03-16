CREATE TABLE [dbo].[tbl_User] (
    [User_Id]           INT            IDENTITY (1, 1) NOT NULL,
    [User_Name]         NVARCHAR (MAX) NULL,
    [SponserProfession] NVARCHAR (MAX) NULL,
    [FatherEmail]       NVARCHAR (MAX) NULL,
    [FatherProfession]  NVARCHAR (MAX) NULL,
    [User_FatherName]   NVARCHAR (MAX) NULL,
    [Notes]             NVARCHAR (MAX) NULL,
    [User_Pic]          NVARCHAR (MAX) NULL,
    [User_AspUser]      NVARCHAR (128) NULL,
    [Allowed_PC_Dev]    INT            NULL,
    [Allowed_Mob_Dev]   INT            NULL,
    [OccupationID]      INT            NULL,
    [InstituteID]       INT            NULL,
    [CNIC]              NVARCHAR (MAX) NULL,
    [YearOfMBBS]        NVARCHAR (MAX) NULL,
    [ExamType]          NVARCHAR (MAX) NULL,
    [User_Mobile]       NVARCHAR (MAX) NULL,
    [CountryID]         INT            NULL,
    [Institute]         NVARCHAR (MAX) NULL,
    [JobLocation]       NVARCHAR (MAX) NULL,
    [CreateDT]          DATETIME       NULL,
    [City]              NVARCHAR (MAX) NULL,
    [CNICFront]         NVARCHAR (MAX) NULL,
    [CNICBack]          NVARCHAR (MAX) NULL,
    [StuCardFront]      NVARCHAR (MAX) NULL,
    [StuCardBack]       NVARCHAR (MAX) NULL,
    [Type]              INT            NULL,
    [CategoryID]        INT            NULL,
    [MockTestType]      INT            NULL,
    CONSTRAINT [PK_Student] PRIMARY KEY CLUSTERED ([User_Id] ASC),
    CONSTRAINT [FK_tbl_User_AspNetUsers] FOREIGN KEY ([User_AspUser]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_tbl_User_tbl_Master] FOREIGN KEY ([OccupationID]) REFERENCES [dbo].[tbl_Master] ([Master_ID]),
    CONSTRAINT [FK_tbl_User_tbl_Master1] FOREIGN KEY ([InstituteID]) REFERENCES [dbo].[tbl_Master] ([Master_ID])
);




GO
CREATE NONCLUSTERED INDEX [IX_tbl_User_User_AspUser_Name_Pic]
    ON [dbo].[tbl_User]([User_AspUser] ASC)
    INCLUDE([User_Name], [User_Pic]);


GO
CREATE NONCLUSTERED INDEX [IX_tbl_User_User_AspUser_Name_Mobile_CountryID]
    ON [dbo].[tbl_User]([User_AspUser] ASC)
    INCLUDE([User_Name], [User_Mobile], [CountryID]);


GO
CREATE NONCLUSTERED INDEX [IX_tbl_User_User_AspUser_ExamType_Type]
    ON [dbo].[tbl_User]([User_AspUser] ASC)
    INCLUDE([User_Name], [ExamType], [User_Mobile], [CountryID], [Type]);


GO
CREATE NONCLUSTERED INDEX [IX_tbl_User_User_AspUser]
    ON [dbo].[tbl_User]([User_AspUser] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_tbl_User_Type]
    ON [dbo].[tbl_User]([Type] ASC);

