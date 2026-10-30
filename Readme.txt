SOFTWARE ENGINEERING - LAB ASSIGNMENT 2
B2B Wholesale Management System (WinForms Back-office + ASP.NET Core MVC Agent Portal)
Instructor: Ky-Trung Pham - Semester 1, 2026-2027

GROUP MEMBERS
  524K0028 - Nguyễn Lê Toàn Thắng (leader)
  524V0018 - Võ Hữu Phước
  523V0007 - Hà Văn Thành Đạt

GITHUB REPOSITORY
  https://github.com/claudeprogrammorpheus-png/LabAssignment-2

VIDEO DEMO (3-5 minutes: source code structure + running demo)
  <video link>

SUBMISSION CONTENT
  SourceCode.zip   complete Visual Studio solution (bin/obj removed) + Database/B2BWholesaleDB.sql
  Readme.txt       this file
  Report.docx      report (division of labor, database diagram, screenshots, unit tests)

=====================================================================
1. REQUIREMENTS
=====================================================================
  - Visual Studio 2022 (17.12 or later) with the ".NET desktop development" and
    "ASP.NET and web development" workloads (.NET 9 SDK)
  - SQL Server Express LocalDB (installed with Visual Studio) or any SQL Server 2016+
  - Internet connection the first time (NuGet packages restore; Bootstrap Icons CDN)

=====================================================================
2. DATABASE CONFIGURATION
=====================================================================
  The connection string is NOT hard-coded. It is stored in two configuration files
  (keep the same value in both):
    1. B2B.Web/appsettings.json   -> "ConnectionStrings": { "B2BDb": "..." }
    2. B2B.WinForms/App.config    -> <connectionStrings><add name="B2BDb" connectionString="..." /></connectionStrings>

  Default value (SQL Server LocalDB, Windows authentication):
    Server=(localdb)\MSSQLLocalDB;Database=B2BWholesaleDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True

  Other servers:
    SQL Express : Server=.\SQLEXPRESS;Database=B2BWholesaleDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True
    SQL login   : Server=.;Database=B2BWholesaleDB;User Id=sa;Password=<your password>;TrustServerCertificate=True;MultipleActiveResultSets=True
  (in appsettings.json write a backslash as \\ , e.g. "Server=.\\SQLEXPRESS;...")

  Create the database - choose ONE way:
    a) SQL script: open Database/B2BWholesaleDB.sql in SSMS and press F5
       (or: sqlcmd -S "(localdb)\MSSQLLocalDB" -i Database\B2BWholesaleDB.sql).
       It creates B2BWholesaleDB with all tables, keys, constraints and the seed data.
    b) Automatic: just start B2B.Web or B2B.WinForms - the EF Core Code First migration
       creates and seeds the database if it does not exist.
    c) Package Manager Console: Update-Database -Project B2B.Data -StartupProject B2B.Web

  Seed data: 25 items, 20 agents, 22 users, 20 orders, 30 order lines.

=====================================================================
3. OPEN AND RUN THE SOURCE CODE
=====================================================================
  1. Unzip SourceCode.zip to a SHORT path, e.g. C:\B2B  (very long Windows paths can make
     the build fail with "Could not copy ... because it was not found").
  2. Open Assignment2_B2B\B2BWholesale.sln in Visual Studio; NuGet packages restore automatically.
  3. Web Agent Portal: right-click B2B.Web > Set as Startup Project > F5.
       Login: agent01 / Agent@123   (agent01..agent19; agent20 is locked on purpose)
  4. WinForms Back-office: right-click B2B.WinForms > Set as Startup Project > F5.
       Login: admin / Admin@123   (also: warehouse / Admin@123)
     Tip: Solution > Configure Startup Projects > Multiple startup projects runs both at once.
  5. Unit tests: Test > Run All Tests (19 tests), or in a terminal: dotnet test

  Solution structure
    B2B.Core      entities + business rules (order totals, PBKDF2 password hashing, lockout, order workflow)
    B2B.Data      EF Core Code First DbContext, migrations, seed data, services
                  (AuthService, OrderService with the stock transaction, ReportService)
    B2B.Web       Agent Portal - ASP.NET Core MVC (Option C, Code First), Bootstrap 5
    B2B.WinForms  Back-office for warehouse administrators (QuestPDF shipping manifest)
    B2B.Tests     xUnit unit tests on SQLite in-memory
    Database      B2BWholesaleDB.sql

=====================================================================
4. NOTES
=====================================================================
  - Only Admin accounts can open the WinForms app; only Agent accounts can use the web portal.
  - An account is locked after 5 wrong passwords; unlock it in WinForms > Agents (untick "Account locked").
  - Placing an order deducts stock in one database transaction; if any line has insufficient
    stock the whole order is rejected and nothing is saved.
  - Cancelling an order in WinForms returns its quantities to stock.
  - Shipping manifests are saved as PDF in "Documents\B2B Manifests" (configurable in App.config).
  - Invoice: My Orders > Invoice > "Print / Save as PDF".

=====================================================================
5. CONTRIBUTION OF EACH MEMBER
=====================================================================
  Student ID   Full name               Main features                                               Contribution
  524K0028     Nguyễn Lê Toàn Thắng    Database/Code First, seed, order transaction, unit tests,    33.33%
                                       configuration, report, GitHub
  524V0018     Võ Hữu Phước            Agent Web Portal (login, catalog & ordering, My Orders,      33.33%
                                       invoice, Bootstrap UI)
  523V0007     Hà Văn Thành Đạt        WinForms Back-office (login, CRUD, order management,         33.33%
                                       shipping manifest, dashboard)
                                                                                       Total:  100%
