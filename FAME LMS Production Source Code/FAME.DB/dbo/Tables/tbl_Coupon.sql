CREATE TABLE [dbo].[tbl_Coupon] (
    [Id]           INT             IDENTITY (1, 1) NOT NULL,
    [ExpiryDate]   DATETIME        NULL,
    [CreatedDate]  DATETIME        NULL,
    [CouponSecret] NVARCHAR (MAX)  NULL,
    [SectionID]    INT             NULL,
    [IsActive]     BIT             NOT NULL,
    [Type]         VARCHAR (20)    NULL,
    [Amount]       DECIMAL (18, 2) NULL,
    [DiscountPer]  DECIMAL (18, 2) NULL,
    [NoOfUses]     INT             NULL,
    [CourseFid]    INT             NULL,
    [ForCourse]    BIT             NULL,
    [CreatedBy]    NVARCHAR (128)  NULL,
    [CouponName]   VARCHAR (50)    NULL,
    [RemUses]      INT             NULL,
    CONSTRAINT [PK_tbl_Coupon] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Coupon_AspNetUsers] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_tbl_Coupon_tbl_Courses] FOREIGN KEY ([CourseFid]) REFERENCES [dbo].[tbl_Courses] ([Course_Id]),
    CONSTRAINT [FK_tbl_Coupon_tbl_Section] FOREIGN KEY ([SectionID]) REFERENCES [dbo].[tbl_Section] ([Section_ID])
);

