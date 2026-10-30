SOFTWARE ENGINEERING - LAB ASSIGNMENT 2
B2B Wholesale Management System (WinForms Back-office + ASP.NET Core MVC Agent Portal)
Instructor: Ky-Trung Pham - Semester 1, 2026-2027

GROUP MEMBERS
  524K0028 - Nguyễn Lê Toàn Thắng (leader)
  524V0018 - Võ Hữu Phước
  523V0007 - Hà Văn Thành Đạt

GITHUB REPOSITORY
  <GitHub repository link>

VIDEO DEMO (optional)
  <video link>

REQUIREMENTS
  - Visual Studio 2022 (17.12 or later) with the .NET 9 SDK
  - SQL Server Express LocalDB (installed with Visual Studio) or any SQL Server 2016+

SOLUTION
  B2BWholesale.sln
    B2B.Core      entities + business rules (totals, password hashing, lockout, order workflow)
    B2B.Data      EF Core Code First DbContext, migrations, seed data, services (ordering transaction, reports)
    B2B.Web       Agent Portal - ASP.NET Core MVC (Option C, Code First)
    B2B.WinForms  Back-office for warehouse administrators
    B2B.Tests     xUnit unit tests (19 tests)
  Database/B2BWholesaleDB_CreateAndSeed.sql   SQL script generated from the migrations (optional)

CONFIGURING THE DATABASE CONNECTION
  The connection string is NOT hard-coded. Edit it in two places (same value):
    1. B2B.Web/appsettings.json      -> "ConnectionStrings": { "B2BDb": "..." }
    2. B2B.WinForms/App.config       -> <connectionStrings><add name="B2BDb" connectionString="..." /></connectionStrings>
  Default (LocalDB):
    Server=(localdb)\MSSQLLocalDB;Database=B2BWholesaleDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True
  For a full SQL Server instance use e.g.  Server=.\SQLEXPRESS;Database=B2BWholesaleDB;Trusted_Connection=True;TrustServerCertificate=True
  or with SQL login:                       Server=.;Database=B2BWholesaleDB;User Id=sa;Password=<your password>;TrustServerCertificate=True

CREATING THE DATABASE (choose one)
  a) Just run B2B.Web or B2B.WinForms: the EF Core migration creates and seeds B2BWholesaleDB automatically.
  b) Package Manager Console:  Update-Database -Project B2B.Data -StartupProject B2B.Web
  c) Run Database/B2BWholesaleDB_CreateAndSeed.sql in SSMS (create an empty database B2BWholesaleDB first).

RUNNING
  - Web portal : set B2B.Web as startup project, F5.   Login: agent01 / Agent@123  (agent01..agent19; agent20 is locked)
  - Back-office: set B2B.WinForms as startup project, F5.   Login: admin / Admin@123
  - Unit tests : Test > Run All Tests  (or: dotnet test)
  Tip: right-click the solution > Configure Startup Projects > Multiple startup projects to run both apps at once.

CONTRIBUTION OF EACH MEMBER
  Student ID   Full name               Main features                                               Contribution
  524K0028     Nguyễn Lê Toàn Thắng    Database/Code First, seed, order transaction, unit tests,    33.33%
                                       configuration, report, GitHub
  524V0018     Võ Hữu Phước            Agent Web Portal (login, catalog & ordering, My Orders,      33.33%
                                       invoice, Bootstrap UI)
  523V0007     Hà Văn Thành Đạt        WinForms Back-office (login, CRUD, order management,         33.33%
                                       shipping manifest, dashboard)
                                                                                       Total:  100%
