CREATE TABLE [dbo].[AspNetUsers] (
    [Id]                   NVARCHAR (128) NOT NULL,
    [Email]                NVARCHAR (256) NULL,
    [EmailConfirmed]       BIT            NOT NULL,
    [PasswordHash]         NVARCHAR (MAX) NULL,
    [SecurityStamp]        NVARCHAR (MAX) NULL,
    [PhoneNumber]          NVARCHAR (MAX) NULL,
    [PhoneNumberConfirmed] BIT            NOT NULL,
    [TwoFactorEnabled]     BIT            NOT NULL,
    [LockoutEndDateUtc]    DATETIME       NULL,
    [LockoutEnabled]       BIT            NOT NULL,
    [AccessFailedCount]    INT            NOT NULL,
    [UserName]             NVARCHAR (256) NOT NULL,
    [AssistantTeacher]     NVARCHAR (128) NULL,
    [IsActive]             BIT            NULL,
    [RegisteredFrom]       VARCHAR (50)   NULL,
    [CodeType]             VARCHAR (50)   NULL,
    [VerificationCode]     VARCHAR (50)   NULL,
    [CodeExpiry]           DATETIME       NULL,
    [ShouldChangeInfo]     BIT            NOT NULL,
    [ShouldChangePassword] BIT            NOT NULL,
    CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [Email] UNIQUE NONCLUSTERED ([Email] ASC)
);




GO
CREATE NONCLUSTERED INDEX [IX_AspNetUsers_UserName]
    ON [dbo].[AspNetUsers]([UserName] ASC);

