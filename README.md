# OrdreFlow Frontend

The frontend is the mobile-first user interface for OrdreFlow, an internal time-registration and work-planning system for VS Automatic.

## Responsibilities

The frontend is responsible for:

- Rendering the user interface in the browser
- Providing a simple phone-first time-registration flow
- Showing departments, categories, accessible work items, and time registrations returned by the API
- Displaying loading, validation, and error states
- Calling the backend through HTTP/JSON
- Applying responsive layouts for phones and desktop screens

The frontend is not responsible for:

- Connecting directly to PostgreSQL
- Using Entity Framework Core
- Enforcing security rules by itself
- Implementing business rules that must also be enforced by the API

## Planned Technology

- C# and .NET
- Blazor WebAssembly
- HTTP/REST and JSON
- Responsive, mobile-first UI

The complete stack is documented in the shared [technology stack documentation](https://github.com/ordreflow/ordreflow-docs/blob/main/docs/technology-stack.md).
The shared [development environment documentation](https://github.com/ordreflow/ordreflow-docs/blob/main/docs/development-environment.md)
describes the Flox and WSL conventions.

## Repository Relationships

- [Backend API](https://github.com/ordreflow/ordreflow-backend)
- [Shared documentation](https://github.com/ordreflow/ordreflow-docs)
- [GitHub Project](https://github.com/orgs/ordreflow/projects)

## Current Status

The Blazor WebAssembly frontend uses the current Employees, Departments, Work Items, Time Entries, Exports, and Reports API contracts. The signed-in employee profile and its permissions come from `GET api/employees/me`.

## Project Structure

```text
.
|-- OrdreFlow.Frontend.sln
|-- global.json
|-- .flox/
|   `-- env/
|       |-- manifest.toml
|       `-- manifest.lock
|-- src/
|   `-- OrdreFlow.Frontend/
|       |-- App.razor
|       |-- Program.cs
|       |-- Layout/
|       |-- Models/
|       |-- Pages/
|       |-- Properties/
|       `-- wwwroot/
|-- GIT_BRANCHING.md
`-- README.md
```

- `OrdreFlow.Frontend.sln`: solution file used by the .NET CLI and IDEs.
- `global.json`: pins the repository to .NET SDK `8.0.424`.
- `.flox/env/manifest.toml`: declares the development environment packages.
- `.flox/env/manifest.lock`: locks the resolved Flox package versions.
- `GIT_BRANCHING.md`: repository-specific branch and pull-request rules.
- `src/OrdreFlow.Frontend/`: the standalone Blazor WebAssembly application.
- `src/OrdreFlow.Frontend/App.razor`: configures client-side routing and the default layout.
- `src/OrdreFlow.Frontend/Program.cs`: starts the WebAssembly host, reads `ApiBaseUrl`, and registers HTTP clients and application services.
- `src/OrdreFlow.Frontend/Pages/`: contains the route-level screens.
- `src/OrdreFlow.Frontend/Pages/Home.razor`: displays the authenticated user's weekly entries and total.
- `src/OrdreFlow.Frontend/Pages/RegisterTime.razor`: validates and submits a time registration.
- `src/OrdreFlow.Frontend/Pages/TimeEntries.razor`: displays successfully submitted entries and the current week's total.
- `src/OrdreFlow.Frontend/Pages/ReviewTimeEntries.razor`: lets category reviewers review entries in their permitted scope.
- `src/OrdreFlow.Frontend/Pages/ExportTimeEntries.razor`: lets payroll users and administrators create export batches and download CSV.
- `src/OrdreFlow.Frontend/Layout/`: contains shared application chrome, including the main layout and navigation menu.
- `src/OrdreFlow.Frontend/Models/`: contains domain, form, and API request/response models.
- `src/OrdreFlow.Frontend/Services/`: contains HTTP clients for employees, departments, work planning, work items, time entries, exports, reports, and backend health.
- `src/OrdreFlow.Frontend/wwwroot/`: contains static assets, CSS, Bootstrap, and runtime configuration such as `appsettings.json`.
- `src/OrdreFlow.Frontend/Properties/launchSettings.json`: contains local launch profiles and development URLs.

## Application Flow

## Component tests

Run `dotnet test tests\OrdreFlow.Frontend.Tests\OrdreFlow.Frontend.Tests.csproj`.
The bUnit/xUnit tests simulate HTTP and JavaScript; they do not call the backend or change real data.

### Current API workflows

- **Employee administration** (`/users`) uses `GET/POST/PUT api/employees` and the employee permissions, activate, and deactivate endpoints. Only administrators see this page.
- **Departments** (`/departments`) lists departments and, for administrators, creates them and assigns/removes members and department managers.
- **Work planning** (`/work-planning`) creates categories under permitted departments and work items under permitted categories. Employees browse only work items returned by `GET api/work-items`, which enforces their access scope.
- **Time registration** (`/register-time`) selects an accessible work item and sends its ID as `TaskId` to `POST api/time_entries`. The API controls whether the entry can be created or changed.
- **Review** (`/time-entries/review`) loads `GET api/time_entries/reviewer` and posts decisions to the separate `/accept` and `/return` endpoints. Returning requires a reason; the reviewer permission and category scope are enforced by the API.
- **Exports** (`/time-entries/export`) creates an export batch with `POST api/exports`, then downloads the batch through `GET api/exports/{id}/download`. Export history and employee selection use the exports endpoints; payroll or administrator permission is required.
- **Reports** (`/reports/top-categories`) uses `GET api/reports/top-categories` and displays the top categories and work-item hour breakdown.

The navigation is permission-based and mirrors the API's `Administrator`, `Payroll`, `DepartmentManager`, and `CatagoryReviewer` permissions. It is a usability aid only; the API remains authoritative for access and business rules.

The frontend communicates with the backend through HTTP/JSON only. It does not connect directly to PostgreSQL, use Entity Framework Core, or replace business and security rules that belong in the API. Frontend models mirror the backend's request and response contracts; review-specific requests live in `Models/TimeEntryReview.cs`.

The shared footer checks `GET api/health` on load and every 30 seconds while the app is open. It displays the backend as available only for a successful response containing `{"status":"ok"}`. The check uses the configured `ApiBaseUrl` (and the existing `/api/` proxy in the Docker image).

## Installable app (PWA)

OrdreFlow has a web app manifest and service worker, so browsers can offer **Install app** (or **Add to Home Screen** on iOS) when served over HTTPS or localhost. The installed app opens in its own window. The existing OrdreFlow icon is used for the home-screen icon.

In published builds, Blazor generates `service-worker-assets.js`; the published service worker caches the versioned static app files and supports loading the app shell after a successful online visit. Development builds use a network-only service worker so local changes appear immediately. Updates download in the background and normally become active after all app tabs are closed.

All `/api/` calls remain network-only. Signing in, loading the employee profile, and working with time entries still require the backend. External Google Fonts and the D3 chart script are also not part of the offline cache. Offline data, edits, and synchronization are future work; installing the PWA does not enable them.

To check the published behavior, run `dotnet publish src/OrdreFlow.Frontend/OrdreFlow.Frontend.csproj -c Release`, serve its `wwwroot` output on localhost or HTTPS (with SPA fallback to `index.html`), and check the browser's Application panel for the manifest, registered service worker, and installation option. Visit once online before trying an offline reload. Use a real backend to check API-dependent screens.

## Running Locally

Install [Flox](https://flox.dev/docs/install-flox/install/) before setting up the
repository. The committed Flox environment provides the exact .NET SDK version
required by this project: `8.0.424`. The root `global.json` keeps the .NET CLI
on that SDK version.

From the repository root, activate the environment:

```bash
flox activate
```

Run the remaining commands inside the activated shell:

```bash
dotnet --version
dotnet restore
dotnet run --project src/OrdreFlow.Frontend
```

The version check should print `8.0.424`. The app is served at the URL printed
in the console (see `src/OrdreFlow.Frontend/Properties/launchSettings.json`).

For integrated testing with the Docker image, set the frontend container's
`API_UPSTREAM` to the backend's internal URL (for example, `http://api:8080`).
The default `ApiBaseUrl` of `/` then sends `/api/` requests through Nginx to
the backend. When running only `dotnet run`, the development server does not
proxy `/api/`; run the app behind the Docker proxy to exercise the API without
cross-origin configuration. In Development the backend can use `DEV_USER_ID`
for its current user until login is available.

For a single command without opening an interactive shell:

```bash
flox activate -c 'dotnet restore && dotnet run --project src/OrdreFlow.Frontend'
```

The frontend Flox environment intentionally contains only the .NET SDK. It
does not provide Git, Node.js, PostgreSQL, Docker, or Entity Framework Core
tools. Those dependencies belong to the repository or environment that needs
them.

## Updating the Development Environment

Use Flox from the repository root when changing the development environment:

```bash
flox search <package>
flox install <package>
```

Commit changes to `.flox/env/manifest.toml` and `.flox/env/manifest.lock`
together. Flox runtime, cache, log, and telemetry files are local-only.

## Planned POC Flow

The first proof of concept should allow a test employee to:

1. Open the application on a phone.
2. See seeded orders or cases.
3. Enter a time registration.
4. Submit the registration to the backend API.
5. See saved registrations and a weekly total.

## Development Notes

- The frontend must use the real backend API for integrated testing.
- API URLs must be configured per environment.
- No secrets or database connection strings belong in this repository.
- The application should be tested on a real phone as early as possible.
