# OrdreFlow Frontend

The frontend is the mobile-first user interface for OrdreFlow, an internal time-registration and order-management system for VS Automatic.

## Responsibilities

The frontend is responsible for:

- Rendering the user interface in the browser
- Providing a simple phone-first time-registration flow
- Showing orders, tasks, and time registrations returned by the API
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

The Blazor WebAssembly frontend loads orders, tasks, user details, and time registrations from the backend API. The backend supplies the current user's identity; during Development it can use its `DEV_USER_ID` setting until login is implemented.

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
- `src/OrdreFlow.Frontend/Layout/`: contains shared application chrome, including the main layout and navigation menu.
- `src/OrdreFlow.Frontend/Models/`: contains domain, form, and API request/response models.
- `src/OrdreFlow.Frontend/Services/`: contains HTTP clients for orders, tasks, time entries, user details, and backend health.
- `src/OrdreFlow.Frontend/wwwroot/`: contains static assets, CSS, Bootstrap, and runtime configuration such as `appsettings.json`.
- `src/OrdreFlow.Frontend/Properties/launchSettings.json`: contains local launch profiles and development URLs.

## Application Flow

## Component tests

Run `dotnet test tests\OrdreFlow.Frontend.Tests\OrdreFlow.Frontend.Tests.csproj`.
The bUnit/xUnit tests cover user search, manager-created employee payloads,
manager-created order payloads, task navigation with the selected order ID,
task create/edit payloads, the order-filtered task list, and employee access
restrictions on the user, order, and task pages. HTTP and JavaScript are simulated;
the tests do not call the backend or change real data.

### User management

Managers and administrators can open **Brugere** (`/users`) from the navigation.
Managers can view the users visible to them through the API and open read-only
details. Only administrators can create users, edit profiles, change roles,
assign managers, or deactivate accounts. New users require a name, email,
password, and role. These operations show separate success/error feedback;
authorization remains enforced by the API.
When creating a user, an administrator may optionally assign an active manager.

### Order administration

Active managers and administrators can open **Ordrer** (`/orders`) from the navigation. The
page lists orders with name search and status filtering. Managers manage their
own orders; administrators retain access according to the API's permissions. They can
create an order, select one for details, rename an open order, and confirm
closing it. Closed orders remain visible but cannot be edited. Writes use the
existing orders API, and saved data is reloaded after successful operations.
The API remains responsible for authorization and business rules.
Order details include a **Gå til opgaver** button that opens `/tasks?orderId={orderId}`
with the selected order's ID.

### Task management

The tasks page (`/tasks`, "Opgaver") lists tasks from `GET api/tasks`. When opened
from an order with `?orderId=`, it shows that order's tasks, and the order filter can
be changed on the page. Selecting a task opens its details (`/tasks/{id}`).
Managers and administrators can create a task (`/tasks/new`, pre-selecting the
current order) through `POST api/tasks` and edit its title and description
(`/tasks/{id}/edit`) through `PUT api/tasks/{id}`; employees only see the list and
details. The task's order cannot be changed after creation. **Markér som færdig**
is shown but disabled, because the API has no way to complete a task yet.

The current time-registration flow is:

1. `Home.razor` loads the user's name from `GET api/users/me` and the selected week's entries from `GET api/time_entries?FromDate=...&ToDate=...`.
2. `RegisterTime.razor` loads orders and their tasks from the API, validates the form, and sends `POST api/time_entries` with the selected task UUID. The backend returns `204 No Content` on success.
3. Editing and deleting saved entries use the backend's `GET`, `PUT`, and `DELETE api/time_entries/{id}` routes. After a successful write the pages fetch the saved entries again.
4. `TimeEntries.razor` loads the selected week's saved entries from the API and calculates the weekly total from that response.

The frontend communicates with the backend through HTTP/JSON only. It does not connect directly to PostgreSQL, use Entity Framework Core, or replace business and security rules that belong in the API. Frontend models mirror the backend's request and response contracts; review-specific requests live in `Models/TimeEntryReview.cs`.

The shared footer checks `GET api/health` on load and every 30 seconds while the app is open. It displays the backend as available only for a successful response containing `{"status":"ok"}`. The check uses the configured `ApiBaseUrl` (and the existing `/api/` proxy in the Docker image).

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
