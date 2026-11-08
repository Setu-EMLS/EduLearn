/* =====================================================================
   EduLearn LMS - 05: demo students and course enrollments (OPTIONAL - for testing only)

   Creates 25 demo students (student6 .. student30 @edulearn.com, password Student@123)
   and enrolls them so that every course ends up with 20 - 25 students:
     target per course = 20 + (CourseId * 7) % 6      (always between 20 and 25)
   - students already enrolled in a course count towards its target
   - only batches that are open for admission and not finished are used, and their
     seats are respected (a course with fewer free seats is filled up to its seats)
   - fee = the course price, payment method "Cash (office)" (what "Add students" in the
     admin panel records), so the revenue totals go up as well
   Safe to run again: it only adds what is missing. Do not run on a live site.

   Set @DryRun = 1 to see what would be added without keeping any change.
   To undo: 05_demo_students_and_enrollments_remove.sql
   ===================================================================== */

USE [EduLearn];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

DECLARE @DryRun bit = 0;   -- 1 = show the result, then roll everything back

BEGIN TRANSACTION;

DECLARE @now datetime2 = SYSDATETIME();
DECLARE @today date = CAST(@now AS date);

/* ---- 1. the demo students (login + student profile) ---- */
DECLARE @demo TABLE (
    [N] INT PRIMARY KEY, [Name] NVARCHAR(100), [Phone] NVARCHAR(20), [Email] NVARCHAR(256),
    [Code] NVARCHAR(20), [Password] NVARCHAR(200), [Salt] NVARCHAR(100));

INSERT INTO @demo ([N], [Name], [Phone], [Email], [Code], [Password], [Salt]) VALUES
 (6, N'Rafiq Islam', N'01800000006', N'student6@edulearn.com', N'ST26110006', N'PBKDF2$1YmNP1HS60ZooTaoTg8jKgNovXlKjy33hVULH3y3wLo=', N'c3Z6cTpb8ulGxmDsKR6amw=='),
 (7, N'Sadia Rahman', N'01800000007', N'student7@edulearn.com', N'ST26110007', N'PBKDF2$1m/j/J4WEGf17Ckw/SaBN7fAALx+KTBLy0WGpF0rjMc=', N'GyIMGBj87jV36hjoPhyUcg=='),
 (8, N'Imran Hossain', N'01800000008', N'student8@edulearn.com', N'ST26110008', N'PBKDF2$i0m4npi5fW8lNmPhgapMEAaW0EwV27Z0h65ecqA2Bhg=', N'h8E+16bYMSx547BjA7Xbzg=='),
 (9, N'Farhana Akter', N'01800000009', N'student9@edulearn.com', N'ST26110009', N'PBKDF2$WebNU7cBH8u99TN21gDI7bPi/F0G5JuJOkFs7kAuR9E=', N'Cc3b7T6BfM+nHox2liL+jw=='),
 (10, N'Shakil Ahmed', N'01800000010', N'student10@edulearn.com', N'ST26110010', N'PBKDF2$hCkRH5IiS8PjsTCmTYelj0E5vPBJ6apjp99hqeAQw/Q=', N'Q1DinG5t3+7kOlOvW8L4tQ=='),
 (11, N'Tasnim Sultana', N'01800000011', N'student11@edulearn.com', N'ST26110011', N'PBKDF2$dOfgJT4iL1VpgoMVwGVFr1sHZ9SvSOAHhHKaFS3c1i8=', N'fhuVN0KSNerznATrTmZgKA=='),
 (12, N'Mahmudul Hasan', N'01800000012', N'student12@edulearn.com', N'ST26110012', N'PBKDF2$kNWEES8LvHVqD7b3Y43F0bDlCmgqUoy6RYprBOi+D/I=', N'Zuzkz0xyyxBujHYJpZEAdw=='),
 (13, N'Jannatul Ferdous', N'01800000013', N'student13@edulearn.com', N'ST26110013', N'PBKDF2$VIPkAooJ24wxWBymuXpI3Zc52fj1VS98D5IFIlOhDP0=', N'JIhFaHELNFByhep/kK0xQw=='),
 (14, N'Rakib Hasan', N'01800000014', N'student14@edulearn.com', N'ST26110014', N'PBKDF2$q8mmVFwaoMknWxdp437wmOT90i0N3A3dxhidravuuko=', N'g8y9T9HxWGuFWT57rQfHog=='),
 (15, N'Sumaiya Khatun', N'01800000015', N'student15@edulearn.com', N'ST26110015', N'PBKDF2$kORfkCGz432TzC+1EjfTk2SrMYDt+7Hoo9DO9nPBuss=', N'W5no9goRWy3YqyXOX/mdvQ=='),
 (16, N'Arif Chowdhury', N'01800000016', N'student16@edulearn.com', N'ST26110016', N'PBKDF2$wBVUz5Zk+TYTb25uSw9C/GMHN3KmvEXe5YdCq8uuJzc=', N'dzYSMAGTWbO0+RIwq7bsjg=='),
 (17, N'Lamia Islam', N'01800000017', N'student17@edulearn.com', N'ST26110017', N'PBKDF2$KvBtekCfY5HP74k+2ub4mSL3I0y9dqttIP3uoh4ermM=', N'op8WuA++vAwGzxGRdsrTwQ=='),
 (18, N'Nayeem Uddin', N'01800000018', N'student18@edulearn.com', N'ST26110018', N'PBKDF2$Clv94iq5JZOrlvW/ucYIRy6311OoctgXlWzA/sz5CXs=', N'Dn4zoZID8nHyR1AVScCEKw=='),
 (19, N'Maliha Tabassum', N'01800000019', N'student19@edulearn.com', N'ST26110019', N'PBKDF2$OKRgh8Cm7CXN9h2xE+UhhDHIvUiSnGNUUEREsforaIU=', N'/kNObM4LEJnU/0/CQ4OXVw=='),
 (20, N'Sohel Rana', N'01800000020', N'student20@edulearn.com', N'ST26110020', N'PBKDF2$dnMEnMrQap0tGx0J9lwd883QVjEPM+gn117DbZrXoSE=', N'Bt9MFC/Q1PaUfw0GkXLuLQ=='),
 (21, N'Rumana Parvin', N'01800000021', N'student21@edulearn.com', N'ST26110021', N'PBKDF2$o31j4g6Yh6F0JQk+mM7+xmyst4ZD2STrddvexlGUEYI=', N'j73ul/22HithflfcaSJN4Q=='),
 (22, N'Habibur Rahman', N'01800000022', N'student22@edulearn.com', N'ST26110022', N'PBKDF2$dJXiffnju8b9Nr7A18cWtMVYvmgPAuUqyNJBvTfCw/g=', N'XxKcPcK1jWwjiQJYDEtWmQ=='),
 (23, N'Sharmin Akter', N'01800000023', N'student23@edulearn.com', N'ST26110023', N'PBKDF2$50oH85lOZY2mbW1L7T/eLvu2OmxObRRvf69p4eegTKI=', N'Asfxgu9uHEWmmIKo+sxDxQ=='),
 (24, N'Tareq Aziz', N'01800000024', N'student24@edulearn.com', N'ST26110024', N'PBKDF2$Dsy5+yQH2jYNd7MBWyNpS+34z3jgx1+yBuTUCJ+E9lA=', N'nSTvvOZm9RIlpxWAO8r+pQ=='),
 (25, N'Mehjabin Chowdhury', N'01800000025', N'student25@edulearn.com', N'ST26110025', N'PBKDF2$kGr1KAAP960E411m8b4yWdUW8mmfWe9IVLjIpAWSGqI=', N'Yp4TB+nznRAlbqNCQl9aXQ=='),
 (26, N'Anik Das', N'01800000026', N'student26@edulearn.com', N'ST26110026', N'PBKDF2$tJBkK4O3tfg99pv+IQjehOohWE6hmatHyv6u0wLZDSo=', N'QtvoaL2EcyuPyZz2jT9Msw=='),
 (27, N'Priya Saha', N'01800000027', N'student27@edulearn.com', N'ST26110027', N'PBKDF2$DzfjpYh/w7tIJTqNTGZDQ8MMkldQ+k8GPRYvmjXNR0w=', N'yYFLqoXHqxnbuokriRY1rg=='),
 (28, N'Fahim Reza', N'01800000028', N'student28@edulearn.com', N'ST26110028', N'PBKDF2$dD30Arghb9FzQIZH9P1VkY01Al41SPLkr96IHsIF2rE=', N'2AauuM0EpdeS0eryEY5GkQ=='),
 (29, N'Tanjila Haque', N'01800000029', N'student29@edulearn.com', N'ST26110029', N'PBKDF2$ERFJOTpLVXB4icfZXxQi4gYC+tpGB13hUqIkyapBQEI=', N'B4p/EFuQeL21YUkULgRcgA=='),
 (30, N'Zahid Hasan', N'01800000030', N'student30@edulearn.com', N'ST26110030', N'PBKDF2$VhVWDn7+SKFhGK5AeTMsQeOGsu1dHW0LQauf5q4rIbI=', N'ZbU4WQPRWhNntQxyEtPFgA==');

INSERT INTO [Tbl_Users] ([RoleId], [TenantId], [FullName], [MobileNo], [Email], [Password], [Salt], [IsActive], [CreatedAt])
SELECT 3, 0, d.[Name], d.[Phone], d.[Email], d.[Password], d.[Salt], 1, @now
FROM @demo d
WHERE NOT EXISTS (SELECT 1 FROM [Tbl_Users] u WHERE u.[Email] = d.[Email]);
DECLARE @newUsers INT = @@ROWCOUNT;

INSERT INTO [Students] ([StudentID], [UserId], [Name], [Email], [Contact])
SELECT d.[Code], u.[UserId], d.[Name], d.[Email], d.[Phone]
FROM @demo d JOIN [Tbl_Users] u ON u.[Email] = d.[Email]
WHERE NOT EXISTS (SELECT 1 FROM [Students] s WHERE s.[Email] = d.[Email]);

/* ---- 2. enroll them, course by course ---- */
CREATE TABLE #elig (BatchId INT NOT NULL, StartDate DATETIME2 NULL, Free INT NOT NULL, CumFrom INT NOT NULL, CumTo INT NOT NULL);
CREATE TABLE #pick (Seq INT IDENTITY(1,1) PRIMARY KEY, StudentId INT NOT NULL);
DECLARE @report TABLE ([CourseId] INT, [Title] NVARCHAR(200), [Target] INT, [Before] INT, [Added] INT, [After] INT);

DECLARE @CourseId INT, @Title NVARCHAR(200), @Price DECIMAL(18,2), @CourseSeats INT;
DECLARE @Target INT, @Existing INT, @Capacity INT, @Need INT, @Added INT;

DECLARE course_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT [Id], [Title], ISNULL([Price], 0), ISNULL([Seats], 0) FROM [Courses] ORDER BY [Id];
OPEN course_cursor;
FETCH NEXT FROM course_cursor INTO @CourseId, @Title, @Price, @CourseSeats;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @Target = 20 + (@CourseId * 7) % 6;
    SELECT @Existing = COUNT(*) FROM [Enrollments] WHERE [CourseId] = @CourseId AND [Status] IN (1, 2);

    TRUNCATE TABLE #elig;
    TRUNCATE TABLE #pick;

    IF EXISTS (SELECT 1 FROM [Batches] WHERE [CourseId] = @CourseId)
    BEGIN
        -- Batches that still take students: admission open and not finished (else any batch of the course)
        ;WITH held AS (
            SELECT [BatchId], COUNT(*) AS n FROM [Enrollments]
            WHERE [BatchId] IS NOT NULL AND [Status] IN (0, 1, 2) GROUP BY [BatchId]
        )
        INSERT INTO #elig (BatchId, StartDate, Free, CumFrom, CumTo)
        SELECT b.[Id], b.[StartDate],
               CASE WHEN ISNULL(b.[Seats], 0) <= 0 THEN 100000
                    WHEN b.[Seats] - ISNULL(h.n, 0) < 0 THEN 0
                    ELSE b.[Seats] - ISNULL(h.n, 0) END,
               0, 0
        FROM [Batches] b LEFT JOIN held h ON h.[BatchId] = b.[Id]
        WHERE b.[CourseId] = @CourseId AND b.[IsAdmissionOpen] = 1 AND (b.[EndDate] IS NULL OR b.[EndDate] >= @today);

        IF NOT EXISTS (SELECT 1 FROM #elig)
        BEGIN
            ;WITH held AS (
                SELECT [BatchId], COUNT(*) AS n FROM [Enrollments]
                WHERE [BatchId] IS NOT NULL AND [Status] IN (0, 1, 2) GROUP BY [BatchId]
            )
            INSERT INTO #elig (BatchId, StartDate, Free, CumFrom, CumTo)
            SELECT b.[Id], b.[StartDate],
                   CASE WHEN ISNULL(b.[Seats], 0) <= 0 THEN 100000
                        WHEN b.[Seats] - ISNULL(h.n, 0) < 0 THEN 0
                        ELSE b.[Seats] - ISNULL(h.n, 0) END,
                   0, 0
            FROM [Batches] b LEFT JOIN held h ON h.[BatchId] = b.[Id]
            WHERE b.[CourseId] = @CourseId;
        END

        -- running totals: pick number 1..Free goes to the first batch, the next ones to the next batch, ...
        ;WITH x AS (
            SELECT BatchId, Free, SUM(Free) OVER (ORDER BY BatchId ROWS UNBOUNDED PRECEDING) AS CumTo FROM #elig
        )
        UPDATE e SET e.CumTo = x.CumTo, e.CumFrom = x.CumTo - x.Free FROM #elig e JOIN x ON x.BatchId = e.BatchId;
        SELECT @Capacity = ISNULL(SUM(Free), 0) FROM #elig;
    END
    ELSE
    BEGIN
        -- a course without batches: its own seat limit (0 = unlimited)
        SELECT @Capacity = CASE WHEN @CourseSeats > 0
            THEN @CourseSeats - (SELECT COUNT(*) FROM [Enrollments] WHERE [CourseId] = @CourseId AND [Status] IN (0, 1, 2))
            ELSE 100000 END;
        IF @Capacity < 0 SET @Capacity = 0;
    END

    SET @Need = @Target - @Existing;
    IF @Need > @Capacity SET @Need = @Capacity;
    IF @Need < 0 SET @Need = 0;
    SET @Added = 0;

    IF @Need > 0
    BEGIN
        -- demo students not in this course yet; the starting point moves from course to course so the classes differ
        INSERT INTO #pick (StudentId)
        SELECT TOP (@Need) s.[Id]
        FROM @demo d JOIN [Students] s ON s.[Email] = d.[Email]
        WHERE NOT EXISTS (SELECT 1 FROM [Enrollments] e WHERE e.[StudentId] = s.[Id] AND e.[CourseId] = @CourseId)
        ORDER BY (d.[N] - 6 + (@CourseId - 1) * 7) % 25, d.[N];

        INSERT INTO [Enrollments] ([StudentId], [CourseId], [BatchId], [Status], [EnrolledAt], [ApprovedAt], [Amount], [PaymentMethod])
        SELECT p.StudentId, @CourseId, b.BatchId, 1, w.EnrolledAt, DATEADD(HOUR, 2, w.EnrolledAt), @Price,
               CASE WHEN @Price > 0 THEN N'Cash (office)' ELSE N'Free' END
        FROM #pick p
        OUTER APPLY (SELECT TOP 1 e.BatchId, e.StartDate FROM #elig e WHERE p.Seq > e.CumFrom AND p.Seq <= e.CumTo ORDER BY e.BatchId) b
        CROSS APPLY (SELECT CASE
                WHEN b.StartDate IS NOT NULL AND b.StartDate <= @now   -- the batch has started: they joined in the 2 weeks before it began
                    THEN DATEADD(HOUR, 9 + (p.Seq * 5) % 10, DATEADD(DAY, -((p.Seq * 3) % 14 + 1), CAST(b.StartDate AS DATETIME2)))
                ELSE DATEADD(HOUR, 9 + (p.Seq * 5) % 10, DATEADD(DAY, -((p.Seq * 5) % 25 + 1), CAST(@today AS DATETIME2)))
            END AS EnrolledAt) w;
        SET @Added = @@ROWCOUNT;
    END

    INSERT INTO @report VALUES (@CourseId, @Title, @Target, @Existing, @Added, @Existing + @Added);
    FETCH NEXT FROM course_cursor INTO @CourseId, @Title, @Price, @CourseSeats;
END
CLOSE course_cursor;
DEALLOCATE course_cursor;
DROP TABLE #elig;
DROP TABLE #pick;

SELECT [CourseId], [Title], [Target], [Before] AS [Enrolled before], [Added], [After] AS [Enrolled now] FROM @report ORDER BY [CourseId];
DECLARE @totalAdded INT = (SELECT ISNULL(SUM([Added]), 0) FROM @report);
PRINT CONCAT(@newUsers, ' new demo student(s) created; ', @totalAdded, ' enrollment(s) added.');

IF @DryRun = 1
BEGIN
    ROLLBACK TRANSACTION;
    PRINT 'Dry run: nothing was saved.';
END
ELSE
BEGIN
    COMMIT TRANSACTION;
    PRINT 'Saved.';
END
GO
