CREATE TABLE [dbo].[tbl_BlogPost] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [Title]         NVARCHAR (255) NULL,
    [Description]   NVARCHAR (500) NULL,
    [Slug]          NVARCHAR (255) NOT NULL,
    [BlogContent]   NVARCHAR (MAX) NULL,
    [Author]        NVARCHAR (255) NULL,
    [PublishedDate] DATETIME       CONSTRAINT [DF__tbl_BlogP__Publi__5634BA94] DEFAULT (getdate()) NULL,
    [IsPublished]   BIT            CONSTRAINT [DF__tbl_BlogP__IsPub__5728DECD] DEFAULT ((0)) NULL,
    [Category]      NVARCHAR (500) NULL,
    [Tags]          NVARCHAR (500) NULL,
    [FeaturedImage] NVARCHAR (500) NULL,
    [SortID]        INT            CONSTRAINT [DF_tbl_BlogPost_ViewCount1] DEFAULT ((0)) NULL,
    [ViewCount]     INT            CONSTRAINT [DF__tbl_BlogP__ViewC__581D0306] DEFAULT ((0)) NULL,
    [CreatedBy]     NVARCHAR (128) NULL,
    [CreatedAt]     DATETIME       CONSTRAINT [DF__tbl_BlogP__Creat__5911273F] DEFAULT (getdate()) NULL,
    [UpdatedBy]     NVARCHAR (128) NULL,
    [UpdatedAt]     DATETIME       CONSTRAINT [DF__tbl_BlogP__Updat__5A054B78] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK__tbl_Blog__3214EC07D3A30696] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ__tbl_Blog__BC7B5FB64E30EAEC] UNIQUE NONCLUSTERED ([Slug] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [slug_tbl_BlogPost]
    ON [dbo].[tbl_BlogPost]([Slug] ASC);

