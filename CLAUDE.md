# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

OrientalApplication is an ASP.NET MVC 5 (.NET Framework 4.7.2) web app for Oriental's internal
purchase/procurement workflow: Purchase Requisitions → Purchase Orders → Outgoing Challans →
Bill Payments, plus Vendor, Item, and Project master data and reporting. It's a Web Forms-era
project (System.Web, `.csproj`/`packages.config`, no `.sln`-level test project), not .NET Core/5+.

## Build & run

There is no CLI test/lint/build tooling configured (no test project, no npm scripts). Development
happens through Visual Studio / MSBuild on Windows:

- Open `OrientalApplication.sln` in Visual Studio and build (restores NuGet packages from
  `packages.config` automatically), or from a Developer Command Prompt:
  ```
  nuget restore OrientalApplication.sln
  msbuild OrientalApplication.sln /p:Configuration=Debug
  ```
- Run via IIS Express (configured project port 44310 / dev server port 58087) — F5 in Visual Studio.
- There are no automated tests in this repo.

## Data layer

- Database is **SQLite**, not SQL Server, at `App_Data/OrientalDB.db` (checked into the repo — it's
  effectively the shared dev/prod-mirror database file, so treat changes to it carefully).
- Despite EntityFramework and `EntityFramework.SqlServer` being referenced NuGet packages, the app
  does **not** use EF for data access. Data access talks to SQLite directly via `System.Data.SQLite`
  (`SQLiteConnection`), through Dapper (`conn.Query<T>`/`conn.Execute`/`conn.QueryFirstOrDefault<T>`)
  with parameterized queries — not raw `SQLiteCommand`/manual `reader.Read()` loops, and not
  string-concatenated SQL. Follow this same Dapper-over-`SQLiteConnection` pattern when adding data
  access; don't introduce EF or a different data-access style for new code without discussing it
  first. Some older queries built with `string.Format`/concatenation may still exist in git history
  but the current code is parameterized.
- **Repository pattern, not DAL classes**: all data access now lives in `Repositories/*.cs` as
  `I<Feature>Repository` interfaces + `<Feature>Repository` implementations (e.g.
  `IVendorRepository`/`VendorRepository`, `IPurchaseOrderRepository`/`PurchaseOrderRepository`, one
  pair per feature area: Company, Item, OutgoingChallan, OutgoingChallanItem, Payment,
  ProjectMaster, PurchaseOrder, PurchaseOrderItem, PurchaseRequisition, User, Vendor). The old
  `DAL/*DAL.cs` static classes (`ItemDAL`, `VendorDAL`, `PurchaseOrderDAL`, etc.) no longer exist —
  don't recreate that pattern for new code; add a new `I<Feature>Repository`/`<Feature>Repository`
  pair in `Repositories/` instead, following the existing ones as a template.
  - `DAL/Database.cs` is the one survivor in `DAL/` — an unused lazy-singleton `SQLiteConnection`
    wrapper that nothing calls (verified: no references anywhere in the codebase). It predates the
    repository migration and was left alone as out of scope; don't build new code on it.
- **No DI container** (no Unity/Ninject/Autofac registered in `Global.asax`). Controllers get their
  repositories via "poor man's DI": a parameterless constructor that `new`s up the concrete
  repository type(s) and delegates to a second constructor that takes the interface(s), e.g.:
  ```csharp
  public VendorController() : this(new VendorRepository()) { }
  public VendorController(IVendorRepository vendorRepository) { _vendorRepository = vendorRepository; }
  ```
  For controllers needing several repositories (e.g. `ReportController`, `PurchaseOrderController`),
  every dependency goes through this same pair of constructors — one parameterless overload
  chaining to one all-interfaces overload, not a chain of single-dependency constructors. Follow
  this exact shape for new controllers rather than introducing a DI container, unless that's
  discussed and decided separately (it's a bigger, project-wide change: `Global.asax` wiring, a new
  NuGet package, and touching every controller's constructor).
  - Repositories that depend on other repositories internally (e.g. `PurchaseRequisitionRepository`
    needs `IItemRepository` for its `SaveNewItem` helper, `ProjectMasterRepository` needs
    `IPurchaseRequisitionRepository` for `CloneProject`, `OutgoingChallanRepository` needs
    `IOutgoingChallanItemRepository`, `PurchaseOrderRepository` needs `IPurchaseOrderItemRepository`)
    use the identical two-constructor pattern internally.
  - `Core/CustomAuthorizeAttribute` is the one exception: it's instantiated by the MVC/CLR attribute
    pipeline from a compile-time-constant constructor call (`[CustomAuthorize(Roles = "...")]`), so
    there's no way to constructor-inject a repository into it. It `new`s up `UserRepository()`
    directly inside `AuthorizeCore` instead — this is intentional, not an oversight to "fix" toward
    consistency with the controller pattern.

## Architecture

Standard ASP.NET MVC 5 layout: `Controllers/` → `Models/` (plain POCOs + view models) → `Views/`
(Razor `.cshtml`, one folder per controller) → `Repositories/` (SQLite access via Dapper, see Data
layer above; `DAL/` only still holds the unused `Database.cs`). `App_Start/` holds `RouteConfig.cs`
(single default route, default controller/action is `PurchaseRequisition/Index`, not Home),
`FilterConfig.cs`, and `BundleConfig.cs`.

**Auth**: Forms Authentication (`Web.config`, login URL `Accounts/Login`). `AccountsController`
validates credentials via `IUserRepository.ValidateUser` (plaintext password comparison against the
DB) and sets the auth cookie; there's no ASP.NET Identity/OWIN. Authorization on controllers/actions
uses `Core/CustomAuthorizeAttribute` (`[CustomAuthorize(Roles = "Admin,Engineering")]`), a custom
`AuthorizeAttribute` that checks roles via `UserRepository.GetUserRoles` (`new`'d up directly inside
the attribute, not constructor-injected — see Data layer above) against the DB rather than
`System.Web.Security.Roles` — use this attribute (not the built-in `[Authorize]`) for role checks on
new controllers/actions, matching the existing role names used across controllers (e.g. `Admin`,
`Engineering`, `Accounts`).

**Key feature areas / controllers**: `PurchaseRequisitionController`, `PurchaseOrderController`
(includes Word/DOCX generation via Xceed `DocX`, and `PrintPO`/print view models), `VendorController`,
`ItemController`, `OutgoingChallanController`, `BillPaymentController`, `ProjectMasterController`,
`ReportController`, `AccountsController` (login/logout), `HomeController`.

**Excel/Office integration**: `ClosedXML` and `DocumentFormat.OpenXml` are used for reading/writing
`.xlsx` (e.g. `App_Data/FilesStore/PRListTemplate.xlsx` is a template used for PR export/import
flows), and `Microsoft.Office.Interop.Word` / Xceed `DocX`/`Xceed.Words.NET` for Word document
generation (PO printing). `Google.Apis.Sheets.v4` is referenced for Google Sheets integration
(`app_client_secret.json` holds the OAuth client config — never commit real secrets here beyond
what's already tracked).

**Print views**: Several controllers have dedicated print-oriented actions/views (`PrintPO.cshtml`,
`PrintPR.cshtml`, `PrintChallan.cshtml`, `PrintProjectSummary.cshtml`) that render a formatted,
print-friendly Razor view separate from the main CRUD `Index` view for that entity — follow this
existing split (CRUD view vs. print view) when adding new print/export functionality rather than
overloading the `Index` view.

## Working conventions

- `App_Data/OrientalDB.db` and files under `App_Data/FilesStore/` are tracked binary data files, not
  generated artifacts — don't regenerate or overwrite them casually, and call out when a change
  touches them.
- Stray files sometimes show up at the repo root or in `Controllers`/`Models` (e.g. `*.bak` files,
  loose `.dll`/`.pdb` backups, ad hoc `.xlsx` exports) from manual developer workflow — don't assume
  every untracked/modified file in `git status` is meant to be part of your change; ask before
  cleaning these up unless the task specifically concerns them.
