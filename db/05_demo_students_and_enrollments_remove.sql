/* =====================================================================
   EduLearn LMS - 05 (undo): removes what 05_demo_students_and_enrollments.sql added

   Deletes the demo students student6 .. student30 @edulearn.com (codes ST26110006 .. ST26110030)
   together with their enrollments, lesson progress, assignment submissions and quiz attempts,
   then their logins. Nobody else is touched.
   ===================================================================== */

USE [EduLearn];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

DECLARE @demo TABLE ([StudentId] INT PRIMARY KEY, [UserId] INT);
INSERT INTO @demo ([StudentId], [UserId])
SELECT s.[Id], s.[UserId]
FROM [Students] s
WHERE s.[Email] LIKE N'student%@edulearn.com'
  AND s.[StudentID] LIKE N'ST261100%'
  AND TRY_CAST(SUBSTRING(s.[Email], 8, CHARINDEX(N'@', s.[Email]) - 8) AS INT) BETWEEN 6 AND 30;

DELETE a FROM [QuizAttempts] a WHERE a.[StudentId] IN (SELECT [StudentId] FROM @demo);          -- their answers go with them
DELETE a FROM [AssignmentSubmissions] a WHERE a.[StudentId] IN (SELECT [StudentId] FROM @demo);
DELETE e FROM [Enrollments] e WHERE e.[StudentId] IN (SELECT [StudentId] FROM @demo);          -- lesson progress goes with them
DECLARE @students INT = (SELECT COUNT(*) FROM @demo);
DELETE s FROM [Students] s WHERE s.[Id] IN (SELECT [StudentId] FROM @demo);
DELETE u FROM [Tbl_Users] u WHERE u.[UserId] IN (SELECT [UserId] FROM @demo WHERE [UserId] IS NOT NULL);

COMMIT TRANSACTION;
PRINT CONCAT(@students, ' demo student(s) removed.');
GO
