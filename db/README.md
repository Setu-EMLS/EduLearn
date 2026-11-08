# Database scripts

SQL Server scripts that set up the EduLearn LMS database without running the app.

| File | What it does |
|---|---|
| `EduLearn_full_install.sql` | **Everything in one file:** files 01 + 02 + 03 below. Use this for a new server. |
| `01_create_database.sql` | Creates the `EduLearn` database if it doesn't exist. |
| `02_schema.sql` | Creates all tables. Generated from the EF Core migrations; safe to re-run (only missing migrations are applied). |
| `03_seed_required.sql` | Roles (1 Admin, 2 Teacher, 3 Student), the first admin login and the payment methods. |
| `04_demo_data.sql` | *Optional, testing only.* Demo teacher/student, two courses with lessons, two batches, an assignment, a quiz, a live class. Skips itself if any course exists. |
| `05_demo_students_and_enrollments.sql` | *Optional, testing only.* Adds 25 demo students and enrolls them so every course has 20 - 25 students (only open batches, seats respected, fee = course price). Run it after `04_demo_data.sql`. Set `@DryRun = 1` inside to preview without saving. |
| `05_demo_students_and_enrollments_remove.sql` | Undo for the file above: removes those 25 students with their enrollments. |

Every script can be run more than once without creating duplicates.

## Install on a new server

1. Open **SQL Server Management Studio**, connect to the server and run `EduLearn_full_install.sql`.
   Or from a terminal:
   ```
   sqlcmd -S YOUR_SERVER -E -C -i EduLearn_full_install.sql
   ```
2. *(Optional)* Run `04_demo_data.sql` to get sample content.
3. Point the app at the database in `EduLearn/appsettings.json` → `ConnectionStrings:DefaultConnection`.
4. Log in as **admin@edulearn.com / Admin@123** and change the password straight away (Account → Change Password).

Demo logins (only after step 2): `teacher@edulearn.com / Teacher@123`, `student@edulearn.com / Student@123`. After `05_demo_students_and_enrollments.sql`: `student6@edulearn.com` … `student30@edulearn.com` / `Student@123`.

## Without the scripts

The app also creates and updates the database by itself on startup (it applies the EF Core migrations and adds the roles, the first admin and the payment methods). The scripts are for servers where the app's login isn't allowed to create databases or tables, or when a DBA prefers to run SQL directly.

## After changing the models

When you add an EF Core migration, regenerate the schema script and the full install file from the `EduLearn` project folder:

```
dotnet ef migrations script --idempotent -o ../db/02_schema.sql
```

Then re-add the `USE [EduLearn];` header at the top of `02_schema.sql` and rebuild `EduLearn_full_install.sql` by joining files 01, 02 and 03.

> The older `EduLearnDb.sql` in the repository root only creates an empty database with paths for one specific machine. The scripts in this folder replace it.
