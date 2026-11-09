/* EduLearn LMS - full install in one file: 01_create_database + 02_schema + 03_seed_required.
   Run in SSMS (or sqlcmd) connected to your SQL Server. Demo data (04) is NOT included. */

/* =====================================================================
   EduLearn LMS - 01: create the database
   Run this first, connected to the SQL Server instance (any database).
   The database files go to the server's default data folder.
   ===================================================================== */

IF DB_ID(N'EduLearn') IS NULL
BEGIN
    CREATE DATABASE [EduLearn];
    PRINT 'Database EduLearn created.';
END
ELSE
    PRINT 'Database EduLearn already exists - nothing to do.';
GO

/* =====================================================================
   EduLearn LMS - 02: database schema (all tables)
   Generated from the Entity Framework Core migrations:
     dotnet ef migrations script --idempotent -o ../db/02_schema.sql
   Idempotent: safe to run on an empty database or one that already has
   some migrations - only the missing ones are applied. It also fills
   __EFMigrationsHistory, so the application won't try to re-create tables.
   ===================================================================== */

USE [EduLearn];
GO
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [AboutUs] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Btn] nvarchar(max) NOT NULL,
        [Url] nvarchar(max) NOT NULL,
        [PicPath] nvarchar(max) NULL,
        CONSTRAINT [PK_AboutUs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [AboutUsHero] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [PicPath] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_AboutUsHero] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [BoosterCategories] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Icon] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_BoosterCategories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [Careers] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        CONSTRAINT [PK_Careers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [ContactHero] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [PicPath] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_ContactHero] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [Counters] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Value] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Counters] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [CourseCategorys] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        CONSTRAINT [PK_CourseCategorys] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [FAQs] (
        [Id] int NOT NULL IDENTITY,
        [Summery] nvarchar(max) NOT NULL,
        [Descriptions] nvarchar(max) NULL,
        CONSTRAINT [PK_FAQs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [FeedbackHero] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [PicPath] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_FeedbackHero] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [FooterNavs] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Url] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NULL,
        CONSTRAINT [PK_FooterNavs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [HeaderNavs] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Url] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NULL,
        CONSTRAINT [PK_HeaderNavs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [HomeHero] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Button] nvarchar(max) NOT NULL,
        [Link] nvarchar(max) NOT NULL,
        [PicPath] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_HomeHero] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [Homes] (
        [Id] int NOT NULL IDENTITY,
        [SectionName] nvarchar(max) NULL,
        CONSTRAINT [PK_Homes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [Information] (
        [Id] int NOT NULL IDENTITY,
        [LogoPath] nvarchar(max) NULL,
        [Phone] nvarchar(max) NULL,
        [CustomerCareNumber] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        [Address] nvarchar(max) NULL,
        [Location] nvarchar(max) NULL,
        CONSTRAINT [PK_Information] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [Missions] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [PicPath] nvarchar(max) NULL,
        CONSTRAINT [PK_Missions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [PaymentGetways] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        CONSTRAINT [PK_PaymentGetways] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [Payments] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [Popups] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Popups] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [ProductHero] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [PicPath] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_ProductHero] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [Products] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Version] nvarchar(max) NOT NULL,
        [Price] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Features] nvarchar(max) NOT NULL,
        [MainImage] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [SeminarCategorys] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        CONSTRAINT [PK_SeminarCategorys] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [Seminars] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Picpath] nvarchar(max) NOT NULL,
        [Btn] nvarchar(max) NULL,
        [Url] nvarchar(max) NULL,
        CONSTRAINT [PK_Seminars] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [ServiceHero] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [PicPath] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_ServiceHero] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [Services] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Icon] nvarchar(max) NULL,
        CONSTRAINT [PK_Services] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [Sliders] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        CONSTRAINT [PK_Sliders] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [StudentFeedbacks] (
        [Id] int NOT NULL IDENTITY,
        [St_name] nvarchar(max) NULL,
        [Course_name] nvarchar(max) NULL,
        [Comments] nvarchar(max) NULL,
        [Rating] nvarchar(max) NULL,
        CONSTRAINT [PK_StudentFeedbacks] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [tbl_roles] (
        [Id] int NOT NULL IDENTITY,
        [Role] nvarchar(max) NULL,
        CONSTRAINT [PK_tbl_roles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [Tbl_Users] (
        [UserId] int NOT NULL IDENTITY,
        [RoleId] int NOT NULL,
        [TenantId] int NOT NULL,
        [FullName] nvarchar(max) NULL,
        [MobileNo] nvarchar(max) NULL,
        [Email] nvarchar(max) NOT NULL,
        [Password] nvarchar(max) NOT NULL,
        [Salt] nvarchar(max) NULL,
        CONSTRAINT [PK_Tbl_Users] PRIMARY KEY ([UserId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [Visions] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [PicPath] nvarchar(max) NULL,
        CONSTRAINT [PK_Visions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [WhyChooseOurProduct] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        CONSTRAINT [PK_WhyChooseOurProduct] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [WhyITBest] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Icon] nvarchar(max) NULL,
        CONSTRAINT [PK_WhyITBest] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [Courses] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Price] decimal(18,2) NULL,
        [CategoryId] int NOT NULL,
        CONSTRAINT [PK_Courses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Courses_CourseCategorys_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [CourseCategorys] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [FooterNavChilds] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Url] nvarchar(max) NOT NULL,
        [ParentId] int NOT NULL,
        CONSTRAINT [PK_FooterNavChilds] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FooterNavChilds_FooterNavs_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [FooterNavs] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [HeaderNavChilds] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Url] nvarchar(max) NOT NULL,
        [ParentId] int NOT NULL,
        CONSTRAINT [PK_HeaderNavChilds] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HeaderNavChilds_HeaderNavs_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [HeaderNavs] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [SMLinks] (
        [id] int NOT NULL IDENTITY,
        [Link_Title] nvarchar(max) NULL,
        [Link] nvarchar(max) NULL,
        [infoID] int NOT NULL,
        CONSTRAINT [PK_SMLinks] PRIMARY KEY ([id]),
        CONSTRAINT [FK_SMLinks_Information_infoID] FOREIGN KEY ([infoID]) REFERENCES [Information] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [GetwayInfo] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [No] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [Branch] nvarchar(max) NULL,
        [GetwayId] int NULL,
        CONSTRAINT [PK_GetwayInfo] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GetwayInfo_PaymentGetways_GetwayId] FOREIGN KEY ([GetwayId]) REFERENCES [PaymentGetways] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [SitePopups] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Url] nvarchar(max) NOT NULL,
        [PicPath] nvarchar(max) NOT NULL,
        [PopupId] int NOT NULL,
        CONSTRAINT [PK_SitePopups] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SitePopups_Popups_PopupId] FOREIGN KEY ([PopupId]) REFERENCES [Popups] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [RelatedProductViewModel] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Version] nvarchar(max) NOT NULL,
        [Price] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Features] nvarchar(max) NOT NULL,
        [MainImage] nvarchar(max) NOT NULL,
        [ProductId] int NULL,
        CONSTRAINT [PK_RelatedProductViewModel] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RelatedProductViewModel_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [SliderBtns] (
        [Id] int NOT NULL IDENTITY,
        [BtnText] nvarchar(max) NOT NULL,
        [BtnUrl] nvarchar(max) NULL,
        [SliderId] int NOT NULL,
        CONSTRAINT [PK_SliderBtns] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SliderBtns_Sliders_SliderId] FOREIGN KEY ([SliderId]) REFERENCES [Sliders] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [Students] (
        [Id] int NOT NULL IDENTITY,
        [StudentID] nvarchar(max) NOT NULL,
        [CourseId] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [Contact] nvarchar(max) NOT NULL,
        [MotherName] nvarchar(max) NOT NULL,
        [FatherName] nvarchar(max) NOT NULL,
        [DateOfBirth] nvarchar(max) NULL,
        CONSTRAINT [PK_Students] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Students_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE TABLE [Teachers] (
        [Id] int NOT NULL IDENTITY,
        [TeacherID] nvarchar(max) NOT NULL,
        [CourseId] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [Contact] nvarchar(max) NOT NULL,
        [MotherName] nvarchar(max) NOT NULL,
        [FatherName] nvarchar(max) NOT NULL,
        [DateOfBirth] nvarchar(max) NULL,
        CONSTRAINT [PK_Teachers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Teachers_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE INDEX [IX_Courses_CategoryId] ON [Courses] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE INDEX [IX_FooterNavChilds_ParentId] ON [FooterNavChilds] ([ParentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE INDEX [IX_GetwayInfo_GetwayId] ON [GetwayInfo] ([GetwayId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE INDEX [IX_HeaderNavChilds_ParentId] ON [HeaderNavChilds] ([ParentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE INDEX [IX_RelatedProductViewModel_ProductId] ON [RelatedProductViewModel] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE INDEX [IX_SitePopups_PopupId] ON [SitePopups] ([PopupId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE INDEX [IX_SliderBtns_SliderId] ON [SliderBtns] ([SliderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE INDEX [IX_SMLinks_infoID] ON [SMLinks] ([infoID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE INDEX [IX_Students_CourseId] ON [Students] ([CourseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    CREATE INDEX [IX_Teachers_CourseId] ON [Teachers] ([CourseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251008103017_init'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251008103017_init', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    ALTER TABLE [Students] DROP CONSTRAINT [FK_Students_Courses_CourseId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    ALTER TABLE [Teachers] DROP CONSTRAINT [FK_Teachers_Courses_CourseId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    DROP INDEX [IX_Teachers_CourseId] ON [Teachers];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    DROP INDEX [IX_Students_CourseId] ON [Students];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Teachers]') AND [c].[name] = N'CourseId');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Teachers] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [Teachers] DROP COLUMN [CourseId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Students]') AND [c].[name] = N'CourseId');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Students] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Students] DROP COLUMN [CourseId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Teachers]') AND [c].[name] = N'Name');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Teachers] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Teachers] ALTER COLUMN [Name] nvarchar(150) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Teachers]') AND [c].[name] = N'MotherName');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Teachers] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Teachers] ALTER COLUMN [MotherName] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Teachers]') AND [c].[name] = N'FatherName');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Teachers] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [Teachers] ALTER COLUMN [FatherName] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Teachers]') AND [c].[name] = N'Description');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Teachers] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Teachers] ALTER COLUMN [Description] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Teachers]') AND [c].[name] = N'Contact');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Teachers] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [Teachers] ALTER COLUMN [Contact] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Teachers]') AND [c].[name] = N'Address');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Teachers] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [Teachers] ALTER COLUMN [Address] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    ALTER TABLE [Teachers] ADD [Designation] nvarchar(150) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    ALTER TABLE [Teachers] ADD [PhotoPath] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    ALTER TABLE [Teachers] ADD [UserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tbl_Users]') AND [c].[name] = N'Email');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Tbl_Users] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [Tbl_Users] ALTER COLUMN [Email] nvarchar(450) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    ALTER TABLE [Tbl_Users] ADD [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE());
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    ALTER TABLE [Tbl_Users] ADD [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Students]') AND [c].[name] = N'Name');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Students] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [Students] ALTER COLUMN [Name] nvarchar(150) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Students]') AND [c].[name] = N'MotherName');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Students] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [Students] ALTER COLUMN [MotherName] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Students]') AND [c].[name] = N'FatherName');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Students] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [Students] ALTER COLUMN [FatherName] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Students]') AND [c].[name] = N'Description');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Students] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [Students] ALTER COLUMN [Description] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Students]') AND [c].[name] = N'Contact');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Students] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [Students] ALTER COLUMN [Contact] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Students]') AND [c].[name] = N'Address');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Students] DROP CONSTRAINT [' + @var14 + '];');
    ALTER TABLE [Students] ALTER COLUMN [Address] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    ALTER TABLE [Students] ADD [PhotoPath] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    ALTER TABLE [Students] ADD [UserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    DECLARE @var15 sysname;
    SELECT @var15 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Courses]') AND [c].[name] = N'Title');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Courses] DROP CONSTRAINT [' + @var15 + '];');
    ALTER TABLE [Courses] ALTER COLUMN [Title] nvarchar(200) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    ALTER TABLE [Courses] ADD [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE());
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    ALTER TABLE [Courses] ADD [Duration] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    ALTER TABLE [Courses] ADD [IsPublished] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    ALTER TABLE [Courses] ADD [Level] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    ALTER TABLE [Courses] ADD [Schedule] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    ALTER TABLE [Courses] ADD [Seats] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    ALTER TABLE [Courses] ADD [ShortDescription] nvarchar(300) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    ALTER TABLE [Courses] ADD [TeacherId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    ALTER TABLE [Courses] ADD [ThumbnailPath] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE TABLE [Announcements] (
        [Id] int NOT NULL IDENTITY,
        [CourseId] int NULL,
        [Title] nvarchar(200) NOT NULL,
        [Message] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedByUserId] int NULL,
        [CreatedByName] nvarchar(max) NULL,
        CONSTRAINT [PK_Announcements] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Announcements_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE TABLE [Assignments] (
        [Id] int NOT NULL IDENTITY,
        [CourseId] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Instructions] nvarchar(max) NULL,
        [DueDate] datetime2 NULL,
        [MaxMarks] int NOT NULL,
        [AttachmentPath] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Assignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Assignments_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE TABLE [CourseSections] (
        [Id] int NOT NULL IDENTITY,
        [CourseId] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [SortOrder] int NOT NULL,
        CONSTRAINT [PK_CourseSections] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CourseSections_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE TABLE [Enrollments] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [CourseId] int NOT NULL,
        [Status] int NOT NULL,
        [EnrolledAt] datetime2 NOT NULL,
        [ApprovedAt] datetime2 NULL,
        [CompletedAt] datetime2 NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaymentMethod] nvarchar(100) NULL,
        [SenderNumber] nvarchar(50) NULL,
        [TransactionId] nvarchar(100) NULL,
        [AdminNote] nvarchar(500) NULL,
        [CertificateNo] nvarchar(50) NULL,
        CONSTRAINT [PK_Enrollments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Enrollments_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Enrollments_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE TABLE [LiveClasses] (
        [Id] int NOT NULL IDENTITY,
        [CourseId] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [StartsAt] datetime2 NOT NULL,
        [DurationMinutes] int NOT NULL,
        [MeetingUrl] nvarchar(1000) NULL,
        [Notes] nvarchar(max) NULL,
        CONSTRAINT [PK_LiveClasses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LiveClasses_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE TABLE [Quizzes] (
        [Id] int NOT NULL IDENTITY,
        [CourseId] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NULL,
        [TimeLimitMinutes] int NULL,
        [PassPercentage] int NOT NULL,
        [MaxAttempts] int NULL,
        [IsPublished] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Quizzes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Quizzes_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE TABLE [AssignmentSubmissions] (
        [Id] int NOT NULL IDENTITY,
        [AssignmentId] int NOT NULL,
        [StudentId] int NOT NULL,
        [Answer] nvarchar(max) NULL,
        [FilePath] nvarchar(max) NULL,
        [SubmittedAt] datetime2 NOT NULL,
        [Marks] decimal(8,2) NULL,
        [Feedback] nvarchar(max) NULL,
        [GradedAt] datetime2 NULL,
        CONSTRAINT [PK_AssignmentSubmissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AssignmentSubmissions_Assignments_AssignmentId] FOREIGN KEY ([AssignmentId]) REFERENCES [Assignments] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AssignmentSubmissions_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE TABLE [Lessons] (
        [Id] int NOT NULL IDENTITY,
        [SectionId] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Type] int NOT NULL,
        [Url] nvarchar(1000) NULL,
        [Content] nvarchar(max) NULL,
        [AttachmentPath] nvarchar(max) NULL,
        [DurationMinutes] int NULL,
        [SortOrder] int NOT NULL,
        [IsPreview] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Lessons] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Lessons_CourseSections_SectionId] FOREIGN KEY ([SectionId]) REFERENCES [CourseSections] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE TABLE [QuizAttempts] (
        [Id] int NOT NULL IDENTITY,
        [QuizId] int NOT NULL,
        [StudentId] int NOT NULL,
        [StartedAt] datetime2 NOT NULL,
        [SubmittedAt] datetime2 NULL,
        [Score] decimal(8,2) NOT NULL,
        [TotalMarks] decimal(8,2) NOT NULL,
        [Passed] bit NOT NULL,
        CONSTRAINT [PK_QuizAttempts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_QuizAttempts_Quizzes_QuizId] FOREIGN KEY ([QuizId]) REFERENCES [Quizzes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_QuizAttempts_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE TABLE [QuizQuestions] (
        [Id] int NOT NULL IDENTITY,
        [QuizId] int NOT NULL,
        [Text] nvarchar(max) NOT NULL,
        [Marks] int NOT NULL,
        [SortOrder] int NOT NULL,
        CONSTRAINT [PK_QuizQuestions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_QuizQuestions_Quizzes_QuizId] FOREIGN KEY ([QuizId]) REFERENCES [Quizzes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE TABLE [LessonProgresses] (
        [Id] int NOT NULL IDENTITY,
        [EnrollmentId] int NOT NULL,
        [LessonId] int NOT NULL,
        [CompletedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_LessonProgresses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LessonProgresses_Enrollments_EnrollmentId] FOREIGN KEY ([EnrollmentId]) REFERENCES [Enrollments] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_LessonProgresses_Lessons_LessonId] FOREIGN KEY ([LessonId]) REFERENCES [Lessons] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE TABLE [QuizOptions] (
        [Id] int NOT NULL IDENTITY,
        [QuestionId] int NOT NULL,
        [Text] nvarchar(max) NOT NULL,
        [IsCorrect] bit NOT NULL,
        CONSTRAINT [PK_QuizOptions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_QuizOptions_QuizQuestions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [QuizQuestions] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE TABLE [QuizAnswers] (
        [Id] int NOT NULL IDENTITY,
        [AttemptId] int NOT NULL,
        [QuestionId] int NOT NULL,
        [SelectedOptionId] int NULL,
        [IsCorrect] bit NOT NULL,
        CONSTRAINT [PK_QuizAnswers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_QuizAnswers_QuizAttempts_AttemptId] FOREIGN KEY ([AttemptId]) REFERENCES [QuizAttempts] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_QuizAnswers_QuizOptions_SelectedOptionId] FOREIGN KEY ([SelectedOptionId]) REFERENCES [QuizOptions] ([Id]),
        CONSTRAINT [FK_QuizAnswers_QuizQuestions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [QuizQuestions] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE INDEX [IX_Teachers_UserId] ON [Teachers] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE INDEX [IX_Tbl_Users_Email] ON [Tbl_Users] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE INDEX [IX_Students_UserId] ON [Students] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE INDEX [IX_Courses_TeacherId] ON [Courses] ([TeacherId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE INDEX [IX_Announcements_CourseId] ON [Announcements] ([CourseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE INDEX [IX_Assignments_CourseId] ON [Assignments] ([CourseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AssignmentSubmissions_AssignmentId_StudentId] ON [AssignmentSubmissions] ([AssignmentId], [StudentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE INDEX [IX_AssignmentSubmissions_StudentId] ON [AssignmentSubmissions] ([StudentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE INDEX [IX_CourseSections_CourseId] ON [CourseSections] ([CourseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE INDEX [IX_Enrollments_CourseId] ON [Enrollments] ([CourseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Enrollments_StudentId_CourseId] ON [Enrollments] ([StudentId], [CourseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE UNIQUE INDEX [IX_LessonProgresses_EnrollmentId_LessonId] ON [LessonProgresses] ([EnrollmentId], [LessonId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE INDEX [IX_LessonProgresses_LessonId] ON [LessonProgresses] ([LessonId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE INDEX [IX_Lessons_SectionId] ON [Lessons] ([SectionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE INDEX [IX_LiveClasses_CourseId] ON [LiveClasses] ([CourseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE INDEX [IX_QuizAnswers_AttemptId] ON [QuizAnswers] ([AttemptId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE INDEX [IX_QuizAnswers_QuestionId] ON [QuizAnswers] ([QuestionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE INDEX [IX_QuizAnswers_SelectedOptionId] ON [QuizAnswers] ([SelectedOptionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE INDEX [IX_QuizAttempts_QuizId] ON [QuizAttempts] ([QuizId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE INDEX [IX_QuizAttempts_StudentId] ON [QuizAttempts] ([StudentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE INDEX [IX_QuizOptions_QuestionId] ON [QuizOptions] ([QuestionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE INDEX [IX_QuizQuestions_QuizId] ON [QuizQuestions] ([QuizId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    CREATE INDEX [IX_Quizzes_CourseId] ON [Quizzes] ([CourseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    ALTER TABLE [Courses] ADD CONSTRAINT [FK_Courses_Teachers_TeacherId] FOREIGN KEY ([TeacherId]) REFERENCES [Teachers] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    ALTER TABLE [Students] ADD CONSTRAINT [FK_Students_Tbl_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Tbl_Users] ([UserId]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    ALTER TABLE [Teachers] ADD CONSTRAINT [FK_Teachers_Tbl_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Tbl_Users] ([UserId]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001050728_LmsCore'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261001050728_LmsCore', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001060000_WebsiteInquiries'
)
BEGIN
    CREATE TABLE [Inquiries] (
        [Id] int NOT NULL IDENTITY,
        [Type] int NOT NULL,
        [Name] nvarchar(150) NULL,
        [Email] nvarchar(200) NOT NULL,
        [Phone] nvarchar(30) NULL,
        [Subject] nvarchar(200) NULL,
        [Course] nvarchar(200) NULL,
        [Message] nvarchar(4000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsHandled] bit NOT NULL,
        CONSTRAINT [PK_Inquiries] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001060000_WebsiteInquiries'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261001060000_WebsiteInquiries', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001115056_Batches'
)
BEGIN
    ALTER TABLE [LiveClasses] ADD [BatchId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001115056_Batches'
)
BEGIN
    ALTER TABLE [Enrollments] ADD [BatchId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001115056_Batches'
)
BEGIN
    ALTER TABLE [Assignments] ADD [BatchId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001115056_Batches'
)
BEGIN
    ALTER TABLE [Announcements] ADD [BatchId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001115056_Batches'
)
BEGIN
    CREATE TABLE [Batches] (
        [Id] int NOT NULL IDENTITY,
        [CourseId] int NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [TeacherId] int NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NULL,
        [Schedule] nvarchar(100) NULL,
        [Seats] int NULL,
        [IsAdmissionOpen] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Batches] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Batches_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Batches_Teachers_TeacherId] FOREIGN KEY ([TeacherId]) REFERENCES [Teachers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001115056_Batches'
)
BEGIN
    CREATE INDEX [IX_LiveClasses_BatchId] ON [LiveClasses] ([BatchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001115056_Batches'
)
BEGIN
    CREATE INDEX [IX_Enrollments_BatchId] ON [Enrollments] ([BatchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001115056_Batches'
)
BEGIN
    CREATE INDEX [IX_Assignments_BatchId] ON [Assignments] ([BatchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001115056_Batches'
)
BEGIN
    CREATE INDEX [IX_Announcements_BatchId] ON [Announcements] ([BatchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001115056_Batches'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Batches_CourseId_Name] ON [Batches] ([CourseId], [Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001115056_Batches'
)
BEGIN
    CREATE INDEX [IX_Batches_TeacherId] ON [Batches] ([TeacherId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001115056_Batches'
)
BEGIN
    ALTER TABLE [Announcements] ADD CONSTRAINT [FK_Announcements_Batches_BatchId] FOREIGN KEY ([BatchId]) REFERENCES [Batches] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001115056_Batches'
)
BEGIN
    ALTER TABLE [Assignments] ADD CONSTRAINT [FK_Assignments_Batches_BatchId] FOREIGN KEY ([BatchId]) REFERENCES [Batches] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001115056_Batches'
)
BEGIN
    ALTER TABLE [Enrollments] ADD CONSTRAINT [FK_Enrollments_Batches_BatchId] FOREIGN KEY ([BatchId]) REFERENCES [Batches] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001115056_Batches'
)
BEGIN
    ALTER TABLE [LiveClasses] ADD CONSTRAINT [FK_LiveClasses_Batches_BatchId] FOREIGN KEY ([BatchId]) REFERENCES [Batches] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001115056_Batches'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261001115056_Batches', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108074732_LiveClassRecording'
)
BEGIN
    ALTER TABLE [LiveClasses] ADD [RecordingAddedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108074732_LiveClassRecording'
)
BEGIN
    ALTER TABLE [LiveClasses] ADD [RecordingPath] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108074732_LiveClassRecording'
)
BEGIN
    ALTER TABLE [LiveClasses] ADD [RecordingUrl] nvarchar(1000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108074732_LiveClassRecording'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261108074732_LiveClassRecording', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108083802_CourseMode'
)
BEGIN
    ALTER TABLE [Courses] ADD [Mode] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108083802_CourseMode'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261108083802_CourseMode', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108085517_LiveClassTopics'
)
BEGIN
    CREATE TABLE [LiveClassTopics] (
        [LiveClassId] int NOT NULL,
        [LessonId] int NOT NULL,
        CONSTRAINT [PK_LiveClassTopics] PRIMARY KEY ([LiveClassId], [LessonId]),
        CONSTRAINT [FK_LiveClassTopics_Lessons_LessonId] FOREIGN KEY ([LessonId]) REFERENCES [Lessons] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_LiveClassTopics_LiveClasses_LiveClassId] FOREIGN KEY ([LiveClassId]) REFERENCES [LiveClasses] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108085517_LiveClassTopics'
)
BEGIN
    CREATE INDEX [IX_LiveClassTopics_LessonId] ON [LiveClassTopics] ([LessonId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108085517_LiveClassTopics'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261108085517_LiveClassTopics', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109055241_LiveClassAttendance'
)
BEGIN
    CREATE TABLE [LiveClassAttendances] (
        [Id] int NOT NULL IDENTITY,
        [LiveClassId] int NOT NULL,
        [EnrollmentId] int NOT NULL,
        [Status] int NOT NULL,
        [MarkedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_LiveClassAttendances] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LiveClassAttendances_Enrollments_EnrollmentId] FOREIGN KEY ([EnrollmentId]) REFERENCES [Enrollments] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_LiveClassAttendances_LiveClasses_LiveClassId] FOREIGN KEY ([LiveClassId]) REFERENCES [LiveClasses] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109055241_LiveClassAttendance'
)
BEGIN
    CREATE INDEX [IX_LiveClassAttendances_EnrollmentId] ON [LiveClassAttendances] ([EnrollmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109055241_LiveClassAttendance'
)
BEGIN
    CREATE UNIQUE INDEX [IX_LiveClassAttendances_LiveClassId_EnrollmentId] ON [LiveClassAttendances] ([LiveClassId], [EnrollmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109055241_LiveClassAttendance'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261109055241_LiveClassAttendance', N'9.0.4');
END;

COMMIT;
GO


/* =====================================================================
   EduLearn LMS - 03: required data
   Roles, the first admin login and the payment methods shown to students.
   Safe to run more than once: each block only inserts when its table is empty.

   Default admin:  admin@edulearn.com  /  Admin@123
   Change this password right after the first login (Account > Change Password).
   ===================================================================== */

USE [EduLearn];
GO
SET NOCOUNT ON;
GO

-- Roles. The application relies on these ids: 1 = Admin, 2 = Teacher, 3 = Student.
IF NOT EXISTS (SELECT 1 FROM [tbl_roles])
BEGIN
    SET IDENTITY_INSERT [tbl_roles] ON;
    INSERT INTO [tbl_roles] ([Id], [Role]) VALUES (1, N'Admin'), (2, N'Teacher'), (3, N'Student');
    SET IDENTITY_INSERT [tbl_roles] OFF;
    PRINT 'Roles added.';
END
GO

-- First administrator (password: Admin@123, stored as a salted PBKDF2-SHA256 hash)
IF NOT EXISTS (SELECT 1 FROM [Tbl_Users] WHERE [RoleId] = 1)
BEGIN
    INSERT INTO [Tbl_Users] ([RoleId], [TenantId], [FullName], [MobileNo], [Email], [Password], [Salt], [IsActive], [CreatedAt])
    VALUES (1, 0, N'Administrator', NULL, N'admin@edulearn.com',
            N'PBKDF2$AW+zT2WJMksxaaGwuw9Fq7XWvBNJ5Lxf51pVBwdfTAU=', N'FgLc7+xwM6lnJRPvXskaAA==', 1, GETDATE());
    PRINT 'Default admin admin@edulearn.com added - change its password after logging in.';
END
GO

-- Payment methods students see on the enroll page (editable under Admin > Payment Methods)
IF NOT EXISTS (SELECT 1 FROM [PaymentGetways])
BEGIN
    DECLARE @bkash INT, @bank INT;

    INSERT INTO [PaymentGetways] ([Name], [Description])
    VALUES (N'bKash', N'Send Money / Payment to one of the numbers below, then enter the Transaction ID.');
    SET @bkash = SCOPE_IDENTITY();
    INSERT INTO [GetwayInfo] ([Name], [No], [Description], [Branch], [GetwayId])
    VALUES (N'Merchant', N'01913168904', NULL, NULL, @bkash),
           (N'Personal', N'01771511805', NULL, NULL, @bkash);

    INSERT INTO [PaymentGetways] ([Name], [Description])
    VALUES (N'Bank Transfer', N'Deposit or transfer to the account below and enter the deposit slip / reference number.');
    SET @bank = SCOPE_IDENTITY();
    INSERT INTO [GetwayInfo] ([Name], [No], [Description], [Branch], [GetwayId])
    VALUES (N'Sonali Bank - EduLearn', N'4439602000928', NULL, N'Mirpur-10', @bank);

    PRINT 'Payment methods added.';
END
GO
