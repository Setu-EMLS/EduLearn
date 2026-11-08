/* =====================================================================
   EduLearn LMS - 04: demo data (OPTIONAL - for testing only)
   Adds 3 demo teachers and 5 demo students (each skipped if its email
   already exists), then - only when there are no courses yet - sample
   categories, two published courses with lessons, an assignment, a quiz,
   a live class and enrollments. Do not run on a live site.

   Demo logins (same as the DbSeeder demo users):
     teacher@edulearn.com .. teacher3@edulearn.com  /  Teacher@123
     student@edulearn.com .. student5@edulearn.com  /  Student@123
   ===================================================================== */

USE [EduLearn];
GO
SET NOCOUNT ON;
GO

-- Demo users (login + teacher/student profile)
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @users DATETIME2 = SYSDATETIME();
DECLARE @demo TABLE (
    [RoleId] INT, [Code] NVARCHAR(20), [Name] NVARCHAR(100), [Designation] NVARCHAR(100),
    [Email] NVARCHAR(256), [Phone] NVARCHAR(20), [Password] NVARCHAR(200), [Salt] NVARCHAR(100));

INSERT INTO @demo ([RoleId], [Code], [Name], [Designation], [Email], [Phone], [Password], [Salt]) VALUES
 (2, N'T260001',    N'Demo Teacher',  N'Senior Instructor',         N'teacher@edulearn.com',  N'01700000000', N'PBKDF2$oAmG3CcClHRa/gXx1VrOxl9ESMadvpgnYB9e6bNTZ6I=', N'p91y6Di6KXr7PbNX+FNQVg=='),
 (2, N'T260002',    N'Rahim Uddin',   N'Web Development Instructor', N'teacher2@edulearn.com', N'01700000002', N'PBKDF2$vIKttWC7VW8hgnJz/ZWHOhXiawc1M7QganNgqrsxDng=', N'ARZ826l/zP1vivHo1Ve/TQ=='),
 (2, N'T260003',    N'Nusrat Jahan',  N'Database Instructor',        N'teacher3@edulearn.com', N'01700000003', N'PBKDF2$49tcq08GUhwCHpe6+gltwDaO8cvf4cqxI2ttYk7Ticc=', N'1YX4LTrrD2J2apm4T1zRwA=='),
 (3, N'ST26100001', N'Demo Student',   NULL, N'student@edulearn.com',  N'01800000000', N'PBKDF2$uQPvbjYF49xUaktJ54GfOL25zmlkL+jMBoThSBJ3r9Q=', N'3f32AdRfrKLaQCxBCyg/4g=='),
 (3, N'ST26100002', N'Karim Hasan',    NULL, N'student2@edulearn.com', N'01800000002', N'PBKDF2$ykniO7QYu9LlUj8UchV/4q7Z1xJp1i+5NsBOa/Sl1Es=', N'GXupcTDxc5mcSuyshHymcg=='),
 (3, N'ST26100003', N'Ayesha Siddika', NULL, N'student3@edulearn.com', N'01800000003', N'PBKDF2$48jR5EmBtiJoLFUlNZE1fhczPm0IwAd11jsl+k5oNIY=', N'necb16WX6q0QQc38EA52CA=='),
 (3, N'ST26100004', N'Tanvir Ahmed',   NULL, N'student4@edulearn.com', N'01800000004', N'PBKDF2$68tthz8AGiME9nCRntrSbT3U78UBUVBYxR/8325BwRs=', N'LVfXolj3v9EazA+aFc0UMQ=='),
 (3, N'ST26100005', N'Mim Akter',      NULL, N'student5@edulearn.com', N'01800000005', N'PBKDF2$KCtkZqG/1ryOr41Bxez7fspQUClbJnhZE5IEpdnPzd8=', N'u89KyPDeHDBZqSfNgOkHmA==');

INSERT INTO [Tbl_Users] ([RoleId], [TenantId], [FullName], [MobileNo], [Email], [Password], [Salt], [IsActive], [CreatedAt])
SELECT d.[RoleId], 0, d.[Name], d.[Phone], d.[Email], d.[Password], d.[Salt], 1, @users
FROM @demo d
WHERE NOT EXISTS (SELECT 1 FROM [Tbl_Users] u WHERE u.[Email] = d.[Email]);

INSERT INTO [Teachers] ([TeacherID], [UserId], [Name], [Designation], [Email], [Contact])
SELECT d.[Code], u.[UserId], d.[Name], d.[Designation], d.[Email], d.[Phone]
FROM @demo d JOIN [Tbl_Users] u ON u.[Email] = d.[Email]
WHERE d.[RoleId] = 2 AND NOT EXISTS (SELECT 1 FROM [Teachers] t WHERE t.[Email] = d.[Email]);

INSERT INTO [Students] ([StudentID], [UserId], [Name], [Email], [Contact])
SELECT d.[Code], u.[UserId], d.[Name], d.[Email], d.[Phone]
FROM @demo d JOIN [Tbl_Users] u ON u.[Email] = d.[Email]
WHERE d.[RoleId] = 3 AND NOT EXISTS (SELECT 1 FROM [Students] s WHERE s.[Email] = d.[Email]);

COMMIT TRANSACTION;
PRINT 'Demo teachers and students added (existing emails skipped).';
GO

IF EXISTS (SELECT 1 FROM [Courses])
BEGIN
    PRINT 'Courses already exist - demo data skipped.';
    RETURN;
END

BEGIN TRANSACTION;

DECLARE @now DATETIME2 = SYSDATETIME();
DECLARE @catWeb INT, @catDb INT, @catSoft INT;
DECLARE @teacher INT, @student INT;
DECLARE @aspnet INT, @sql INT, @sec INT, @quiz INT, @q INT;

-- Categories
INSERT INTO [CourseCategorys] ([Title], [Description]) VALUES (N'Web Development', N'Build modern websites and web applications.');
SET @catWeb = SCOPE_IDENTITY();
INSERT INTO [CourseCategorys] ([Title], [Description]) VALUES (N'Database', N'Design, query and manage databases.');
SET @catDb = SCOPE_IDENTITY();
INSERT INTO [CourseCategorys] ([Title], [Description]) VALUES (N'Software', N'Desktop and enterprise software development.');
SET @catSoft = SCOPE_IDENTITY();

-- Teacher and student that own the demo courses / enrollments (created above)
SELECT @teacher = [Id] FROM [Teachers] WHERE [Email] = N'teacher@edulearn.com';
SELECT @student = [Id] FROM [Students] WHERE [Email] = N'student@edulearn.com';

-- Course 1: paid web development course
INSERT INTO [Courses] ([Title], [ShortDescription], [Description], [Price], [CategoryId], [TeacherId], [Level], [Duration], [Schedule], [Seats], [IsPublished], [CreatedAt])
VALUES (N'Professional Web Development with ASP.NET Core',
        N'Build real-world web applications with C#, ASP.NET Core MVC and SQL Server.',
        N'This course takes you from the fundamentals of C# to building and deploying a complete ASP.NET Core MVC application backed by SQL Server and Entity Framework Core.',
        6000, @catWeb, @teacher, N'Beginner', N'3 Months', N'Sat, Mon, Wed 7.00 pm - 9.00 pm', 30, 1, @now);
SET @aspnet = SCOPE_IDENTITY();

INSERT INTO [CourseSections] ([CourseId], [Title], [SortOrder]) VALUES (@aspnet, N'Getting Started', 1);
SET @sec = SCOPE_IDENTITY();
INSERT INTO [Lessons] ([SectionId], [Title], [Type], [Url], [Content], [DurationMinutes], [SortOrder], [IsPreview], [CreatedAt])
VALUES (@sec, N'Welcome to the course', 1, NULL,
        N'Welcome! In this course you will build a complete web application step by step.

Use the curriculum on the left to move between lessons and press "Mark as complete" when you finish each one.', 5, 1, 1, @now),
       (@sec, N'Installing the tools', 3, N'https://learn.microsoft.com/aspnet/core/getting-started/',
        N'Install the .NET SDK, Visual Studio (or VS Code) and SQL Server Express before the next lesson.', 15, 2, 0, @now);

INSERT INTO [CourseSections] ([CourseId], [Title], [SortOrder]) VALUES (@aspnet, N'ASP.NET Core MVC Basics', 2);
SET @sec = SCOPE_IDENTITY();
INSERT INTO [Lessons] ([SectionId], [Title], [Type], [Url], [Content], [DurationMinutes], [SortOrder], [IsPreview], [CreatedAt])
VALUES (@sec, N'Controllers and actions', 1, NULL, N'A controller groups related actions. Each public method returning IActionResult can be reached through routing.', 20, 1, 0, @now),
       (@sec, N'Razor views', 1, NULL, N'Razor mixes C# and HTML. Use @Model to read the data passed from the controller.', 20, 2, 0, @now);

-- Course 2: free database course
INSERT INTO [Courses] ([Title], [ShortDescription], [Description], [Price], [CategoryId], [TeacherId], [Level], [Duration], [Schedule], [Seats], [IsPublished], [CreatedAt])
VALUES (N'Database Fundamentals with SQL Server',
        N'Learn relational database design and SQL queries from scratch.',
        N'A free introductory course covering tables, keys, relationships and everyday SQL queries.',
        0, @catDb, @teacher, N'Beginner', N'4 Weeks', NULL, 50, 1, @now);
SET @sql = SCOPE_IDENTITY();

INSERT INTO [CourseSections] ([CourseId], [Title], [SortOrder]) VALUES (@sql, N'Relational Databases', 1);
SET @sec = SCOPE_IDENTITY();
INSERT INTO [Lessons] ([SectionId], [Title], [Type], [Url], [Content], [DurationMinutes], [SortOrder], [IsPreview], [CreatedAt])
VALUES (@sec, N'What is a database?', 1, NULL, N'A database stores related data in tables made of rows and columns.', 10, 1, 1, @now),
       (@sec, N'Your first SELECT query', 1, NULL, N'SELECT Name, Email FROM Students WHERE IsActive = 1 ORDER BY Name;', 15, 2, 0, @now);

-- Assignment
INSERT INTO [Assignments] ([CourseId], [Title], [Instructions], [DueDate], [MaxMarks], [CreatedAt])
VALUES (@aspnet, N'Build a contact form',
        N'Create a Contact page with a form (name, email, message) that validates input and shows a thank-you message.',
        DATEADD(DAY, 7, CAST(CAST(@now AS DATE) AS DATETIME2)), 20, @now);

-- Quiz with three questions
INSERT INTO [Quizzes] ([CourseId], [Title], [Description], [TimeLimitMinutes], [PassPercentage], [MaxAttempts], [IsPublished], [CreatedAt])
VALUES (@aspnet, N'MVC Basics Quiz', N'Check your understanding of controllers and views.', 10, 60, NULL, 1, @now);
SET @quiz = SCOPE_IDENTITY();

INSERT INTO [QuizQuestions] ([QuizId], [Text], [Marks], [SortOrder]) VALUES (@quiz, N'What does the C in MVC stand for?', 1, 1);
SET @q = SCOPE_IDENTITY();
INSERT INTO [QuizOptions] ([QuestionId], [Text], [IsCorrect]) VALUES (@q, N'Controller', 1), (@q, N'Component', 0), (@q, N'Class', 0);

INSERT INTO [QuizQuestions] ([QuizId], [Text], [Marks], [SortOrder]) VALUES (@quiz, N'Which file extension do Razor views use?', 1, 2);
SET @q = SCOPE_IDENTITY();
INSERT INTO [QuizOptions] ([QuestionId], [Text], [IsCorrect]) VALUES (@q, N'.aspx', 0), (@q, N'.cshtml', 1), (@q, N'.razorview', 0);

INSERT INTO [QuizQuestions] ([QuizId], [Text], [Marks], [SortOrder]) VALUES (@quiz, N'Entity Framework Core is an ORM.', 1, 3);
SET @q = SCOPE_IDENTITY();
INSERT INTO [QuizOptions] ([QuestionId], [Text], [IsCorrect]) VALUES (@q, N'True', 1), (@q, N'False', 0);

-- Announcement for everyone and an upcoming live class
INSERT INTO [Announcements] ([CourseId], [Title], [Message], [CreatedAt], [CreatedByUserId], [CreatedByName])
VALUES (NULL, N'Welcome to EduLearn LMS', N'Classes for the new batch start next week. Check your course schedule for details.', @now, NULL, N'Administrator');

INSERT INTO [LiveClasses] ([CourseId], [Title], [StartsAt], [DurationMinutes], [MeetingUrl], [Notes])
VALUES (@aspnet, N'Orientation class', DATEADD(HOUR, 19, DATEADD(DAY, 2, CAST(CAST(@now AS DATE) AS DATETIME2))), 90, N'https://meet.google.com/', NULL);

-- The demo student is enrolled in both courses
INSERT INTO [Enrollments] ([StudentId], [CourseId], [Status], [EnrolledAt], [ApprovedAt], [Amount], [PaymentMethod], [TransactionId])
VALUES (@student, @sql, 1, @now, @now, 0, N'Free', NULL),
       (@student, @aspnet, 1, @now, @now, 6000, N'bKash', N'DEMO123456');

COMMIT TRANSACTION;
PRINT 'Demo data added.';
GO
