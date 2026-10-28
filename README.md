# AlgoFlow

ASP.NET Core MVC learning platform for data structures and algorithms, using ASP.NET Core Identity, Entity Framework Core, and MySQL.

> **Current checkpoint:** Student registration/login, an admin-only dashboard, a MySQL-backed course catalog, one seeded free course with a YouTube lesson, free enrollment, and an enrolled-student dashboard are implemented. Quizzes, lesson completion/progress, course editing, user management, payment, and automated business-rule tests are not implemented yet.

## Requirements

- .NET 10 SDK
- MySQL 8.0 or a compatible MySQL server
- A connection string provided in the environment

## Create the database and runtime user

In MySQL Workbench, connect to MySQL with a database administrator account and run this SQL. Replace both example passwords with unique strong passwords:

```sql
CREATE DATABASE IF NOT EXISTS algoflow_db CHARACTER SET utf8mb4;
CREATE USER IF NOT EXISTS 'algoflow_app'@'localhost' IDENTIFIED BY 'REPLACE_WITH_APP_PASSWORD';
CREATE USER IF NOT EXISTS 'algoflow_migrator'@'localhost' IDENTIFIED BY 'REPLACE_WITH_MIGRATION_PASSWORD';
GRANT SELECT, INSERT, UPDATE, DELETE
  ON algoflow_db.* TO 'algoflow_app'@'localhost';
GRANT ALL PRIVILEGES
  ON algoflow_db.* TO 'algoflow_migrator'@'localhost';
```

In VS Code, open **Terminal → New Terminal**. Confirm that the prompt is in the project directory containing `AlgoFlow.csproj`. Restore packages and build:

```powershell
dotnet restore
dotnet build
```

## Apply the database migration

The initial migration in `Data/Migrations` creates Identity and course tables, then seeds the starter course and lesson. Database updates require schema-change permissions; use the migration account rather than granting those permissions to the runtime account. From the project directory:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.12
$env:ConnectionStrings__DefaultConnection = "Server=127.0.0.1;Port=3306;Database=algoflow_db;User=algoflow_migrator;Password=YOUR_MIGRATION_PASSWORD;"
dotnet ef database update
```

If `dotnet-ef` is already installed, skip its installation command. Back up any database that contains data before applying schema changes.

## Start the application

Use the restricted runtime account:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=127.0.0.1;Port=3306;Database=algoflow_db;User=algoflow_app;Password=YOUR_APP_PASSWORD;"
dotnet run
```

Replace `YOUR_APP_PASSWORD` with the password you chose for the MySQL account `algoflow_app`. Do not send it in chat or commit it to source control. The environment value lasts only for the current PowerShell session.

Open the `Now listening on` URL printed by `dotnet run`. Go to **Courses**, create a student account, enroll in the free course, and open the lesson. The lesson video is hosted by YouTube; AlgoFlow embeds it and does not download or rehost it.

## Create the first admin

Public registration always creates a `Student` account; it never accepts a requested role. To bootstrap the first admin, set these values in PowerShell after applying the migration:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=127.0.0.1;Port=3306;Database=algoflow_db;User=algoflow_app;Password=YOUR_APP_PASSWORD;"
$env:ALGOFLOW_BOOTSTRAP_ADMIN_EMAIL = "admin@example.com"
$env:ALGOFLOW_BOOTSTRAP_ADMIN_PASSWORD = "REPLACE_WITH_A_STRONG_ADMIN_PASSWORD"
dotnet run -- --bootstrap-admin
Remove-Item Env:ALGOFLOW_BOOTSTRAP_ADMIN_EMAIL
Remove-Item Env:ALGOFLOW_BOOTSTRAP_ADMIN_PASSWORD
```

Choose a unique, strong password. The command creates the `Student` and `Admin` roles and the initial admin account. If the email already belongs to a non-admin user, the command stops without promoting it. Admins and students share the sign-in form; only the Admin role can access `/admin`.

## Security and current limitations

- ASP.NET Core Identity hashes passwords and applies password and lockout policies.
- Public registration cannot choose an administrator role.
- Student enrollment records are unique per user/course and scoped to the authenticated user.
- Lesson embeds use validated YouTube video IDs and a fixed privacy-enhanced embed host.
- Premium enrollment is not offered; no payment provider or premium entitlement exists.
- Email confirmation is not configured. Configure an email sender and confirmation policy before production use.
- Payment, certificates, quizzes, lesson progress, content editing, and automated business-rule tests remain future work.
- HTTPS redirection is enabled; HSTS and secure production cookies are enabled outside Development.
