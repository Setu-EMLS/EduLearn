using Microsoft.EntityFrameworkCore;
using EduLearn.Areas.Admin.Models;
using EduLearn.Models;
using EduLearn.Models.Lms;
using EduLearn.Services.Interfaces;

namespace EduLearn.Infrastructure
{
	public static class DbSeeder
	{
		public static async Task SeedAsync(IServiceProvider services, IConfiguration config, ILogger logger)
		{
			using var scope = services.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var accounts = scope.ServiceProvider.GetRequiredService<IAccountService>();

			await db.Database.MigrateAsync();

			if (!await db.tbl_roles.AnyAsync())
			{
				// Insert in order so the identity values line up with RoleIds (1 Admin, 2 Teacher, 3 Student)
				foreach (var role in new[] { "Admin", "Teacher", "Student" })
				{
					db.tbl_roles.Add(new Roles { Role = role });
					await db.SaveChangesAsync();
				}
			}

			if (!await db.Tbl_Users.AnyAsync(u => u.RoleId == RoleIds.Admin))
			{
				var email = config["Seed:AdminEmail"] ?? "admin@edulearn.com";
				var password = config["Seed:AdminPassword"] ?? "Admin@123";
				await accounts.CreateAdminAsync("Administrator", email, null, password);
				logger.LogWarning("Created default admin account {Email}. Change its password after first login.", email);
			}

			// Published payment accounts (editable later under Admin > Payment Methods)
			if (!await db.PaymentGetways.AnyAsync())
			{
				db.PaymentGetways.AddRange(
					new PaymentGetway
					{
						Name = "bKash",
						Description = "Send Money / Payment to one of the numbers below, then enter the Transaction ID.",
						GetwayInfo =
						{
							new GetwayInfo { Name = "Merchant", No = "01913168904" },
							new GetwayInfo { Name = "Personal", No = "01771511805" }
						}
					},
					new PaymentGetway
					{
						Name = "Bank Transfer",
						Description = "Deposit or transfer to the account below and enter the deposit slip / reference number.",
						GetwayInfo =
						{
							new GetwayInfo { Name = "Sonali Bank - EduLearn", No = "4439602000928", Branch = "Mirpur-10" }
						}
					});
				await db.SaveChangesAsync();
			}

			if (config.GetValue<bool>("Seed:DemoData"))
			{
				// Demo logins live in appsettings.json (Seed:Teachers / Seed:Students)
				var teachers = config.GetSection("Seed:Teachers").Get<List<SeedUser>>() ?? new();
				var students = config.GetSection("Seed:Students").Get<List<SeedUser>>() ?? new();

				await SeedDemoUsersAsync(accounts, teachers, students, logger);

				if (teachers.Count > 0 && students.Count > 0 && !await db.Courses.AnyAsync())
				{
					await SeedDemoDataAsync(db, teachers[0].Email, students[0].Email);
					logger.LogInformation("Seeded LMS demo data.");
				}
			}
		}

		private sealed class SeedUser
		{
			public string Name { get; set; } = "";
			public string Email { get; set; } = "";
			public string? Phone { get; set; }
			public string Password { get; set; } = "";
			public string? Designation { get; set; }
		}

		// Skipped per user if the email already exists
		private static async Task SeedDemoUsersAsync(IAccountService accounts, List<SeedUser> teachers, List<SeedUser> students, ILogger logger)
		{
			foreach (var t in teachers)
			{
				if (accounts.EmailExists(t.Email)) continue;
				await accounts.CreateTeacherAsync(t.Name, t.Email, t.Phone, t.Password, t.Designation);
				logger.LogInformation("Created demo teacher {Email}.", t.Email);
			}

			foreach (var s in students)
			{
				if (accounts.EmailExists(s.Email)) continue;
				await accounts.CreateStudentAsync(s.Name, s.Email, s.Phone, s.Password);
				logger.LogInformation("Created demo student {Email}.", s.Email);
			}
		}

		private static async Task SeedDemoDataAsync(ApplicationDbContext db, string teacherEmail, string studentEmail)
		{
			var web = new CourseCategory { Title = "Web Development", Description = "Build modern websites and web applications." };
			var database = new CourseCategory { Title = "Database", Description = "Design, query and manage databases." };
			var software = new CourseCategory { Title = "Software", Description = "Desktop and enterprise software development." };
			db.CourseCategorys.AddRange(web, database, software);
			await db.SaveChangesAsync();

			var teacher = await db.Teachers.FirstAsync(t => t.Email == teacherEmail);
			var student = await db.Students.FirstAsync(s => s.Email == studentEmail);

			var aspnet = new Course
			{
				Title = "Professional Web Development with ASP.NET Core",
				ShortDescription = "Build real-world web applications with C#, ASP.NET Core MVC and SQL Server.",
				Description = "This course takes you from the fundamentals of C# to building and deploying a complete ASP.NET Core MVC application backed by SQL Server and Entity Framework Core.",
				Price = 6000,
				CategoryId = web.Id,
				TeacherId = teacher.Id,
				Level = "Beginner",
				Duration = "3 Months",
				Schedule = "Sat, Mon, Wed 7.00 pm - 9.00 pm",
				Seats = 30,
				IsPublished = true,
				Sections =
				{
					new CourseSection
					{
						Title = "Getting Started", SortOrder = 1,
						Lessons =
						{
							new Lesson { Title = "Welcome to the course", Type = LessonType.Article, SortOrder = 1, IsPreview = true, DurationMinutes = 5,
								Content = "Welcome! In this course you will build a complete web application step by step.\n\nUse the curriculum on the left to move between lessons and press \"Mark as complete\" when you finish each one." },
							new Lesson { Title = "Installing the tools", Type = LessonType.Link, SortOrder = 2, DurationMinutes = 15,
								Url = "https://learn.microsoft.com/aspnet/core/getting-started/",
								Content = "Install the .NET SDK, Visual Studio (or VS Code) and SQL Server Express before the next lesson." }
						}
					},
					new CourseSection
					{
						Title = "ASP.NET Core MVC Basics", SortOrder = 2,
						Lessons =
						{
							new Lesson { Title = "Controllers and actions", Type = LessonType.Article, SortOrder = 1, DurationMinutes = 20,
								Content = "A controller groups related actions. Each public method returning IActionResult can be reached through routing." },
							new Lesson { Title = "Razor views", Type = LessonType.Article, SortOrder = 2, DurationMinutes = 20,
								Content = "Razor mixes C# and HTML. Use @Model to read the data passed from the controller." }
						}
					}
				}
			};

			var sql = new Course
			{
				Title = "Database Fundamentals with SQL Server",
				ShortDescription = "Learn relational database design and SQL queries from scratch.",
				Description = "A free introductory course covering tables, keys, relationships and everyday SQL queries.",
				Price = 0,
				CategoryId = database.Id,
				TeacherId = teacher.Id,
				Level = "Beginner",
				Duration = "4 Weeks",
				Seats = 50,
				IsPublished = true,
				Sections =
				{
					new CourseSection
					{
						Title = "Relational Databases", SortOrder = 1,
						Lessons =
						{
							new Lesson { Title = "What is a database?", Type = LessonType.Article, SortOrder = 1, IsPreview = true, DurationMinutes = 10,
								Content = "A database stores related data in tables made of rows and columns." },
							new Lesson { Title = "Your first SELECT query", Type = LessonType.Article, SortOrder = 2, DurationMinutes = 15,
								Content = "SELECT Name, Email FROM Students WHERE IsActive = 1 ORDER BY Name;" }
						}
					}
				}
			};

			db.Courses.AddRange(aspnet, sql);
			await db.SaveChangesAsync();

			db.Assignments.Add(new Assignment
			{
				CourseId = aspnet.Id,
				Title = "Build a contact form",
				Instructions = "Create a Contact page with a form (name, email, message) that validates input and shows a thank-you message.",
				DueDate = DateTime.Now.Date.AddDays(7),
				MaxMarks = 20
			});

			db.Quizzes.Add(new Quiz
			{
				CourseId = aspnet.Id,
				Title = "MVC Basics Quiz",
				Description = "Check your understanding of controllers and views.",
				TimeLimitMinutes = 10,
				PassPercentage = 60,
				IsPublished = true,
				Questions =
				{
					new QuizQuestion { Text = "What does the C in MVC stand for?", SortOrder = 1, Options =
						{ new QuizOption { Text = "Controller", IsCorrect = true }, new QuizOption { Text = "Component" }, new QuizOption { Text = "Class" } } },
					new QuizQuestion { Text = "Which file extension do Razor views use?", SortOrder = 2, Options =
						{ new QuizOption { Text = ".aspx" }, new QuizOption { Text = ".cshtml", IsCorrect = true }, new QuizOption { Text = ".razorview" } } },
					new QuizQuestion { Text = "Entity Framework Core is an ORM.", SortOrder = 3, Options =
						{ new QuizOption { Text = "True", IsCorrect = true }, new QuizOption { Text = "False" } } }
				}
			});

			db.Announcements.Add(new Announcement
			{
				Title = "Welcome to EduLearn",
				Message = "Classes for the new batch start next week. Check your course schedule for details.",
				CreatedByName = "Administrator"
			});

			db.LiveClasses.Add(new LiveClass
			{
				CourseId = aspnet.Id,
				Title = "Orientation class",
				StartsAt = DateTime.Now.Date.AddDays(2).AddHours(19),
				DurationMinutes = 90,
				MeetingUrl = "https://meet.google.com/"
			});

			db.Enrollments.Add(new Enrollment
			{
				StudentId = student.Id,
				CourseId = sql.Id,
				Status = EnrollmentStatus.Active,
				ApprovedAt = DateTime.Now,
				Amount = 0
			});
			db.Enrollments.Add(new Enrollment
			{
				StudentId = student.Id,
				CourseId = aspnet.Id,
				Status = EnrollmentStatus.Active,
				ApprovedAt = DateTime.Now,
				Amount = 6000,
				PaymentMethod = "bKash",
				TransactionId = "DEMO123456"
			});

			await db.SaveChangesAsync();
		}
	}
}
