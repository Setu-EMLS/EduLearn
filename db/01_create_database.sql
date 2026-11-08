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
