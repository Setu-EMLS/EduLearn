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
    VALUES (N'Personal', N'01302481614', NULL, NULL, @bkash),
           (N'Personal', N'01754617283', NULL, NULL, @bkash);

    INSERT INTO [PaymentGetways] ([Name], [Description])
    VALUES (N'Bank Transfer', N'Deposit or transfer to the account below and enter the deposit slip / reference number.');
    SET @bank = SCOPE_IDENTITY();
    INSERT INTO [GetwayInfo] ([Name], [No], [Description], [Branch], [GetwayId])
    VALUES (N'Eastern Bank PLC- EBL
 - EduLearn', N'10412600454978', NULL, N'Gulshan-2', @bank);

    PRINT 'Payment methods added.';
END
GO
