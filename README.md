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

The Blazor WebAssembly project has been scaffolded. API configuration and test commands will be added as the POC flow is implemented.

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
- `global.json`: pins the repository to .NET SDK `8.0.130`.
- `.flox/env/manifest.toml`: declares the development environment packages.
- `.flox/env/manifest.lock`: locks the resolved Flox package versions.
- `GIT_BRANCHING.md`: repository-specific branch and pull-request rules.
- `src/OrdreFlow.Frontend/`: the standalone Blazor WebAssembly application.
- `src/OrdreFlow.Frontend/App.razor`: configures client-side routing and the default layout.
- `src/OrdreFlow.Frontend/Program.cs`: starts the WebAssembly host, reads `ApiBaseUrl`, and registers HTTP clients and application services.
- `src/OrdreFlow.Frontend/Pages/`: contains the route-level screens.
- `src/OrdreFlow.Frontend/Pages/Home.razor`: displays available orders and stores the selected order.
- `src/OrdreFlow.Frontend/Pages/RegisterTime.razor`: validates and submits a time registration.
- `src/OrdreFlow.Frontend/Pages/TimeEntries.razor`: displays successfully submitted entries and the current week's total.
- `src/OrdreFlow.Frontend/Layout/`: contains shared application chrome, including the main layout and navigation menu.
- `src/OrdreFlow.Frontend/Models/`: contains domain, form, and API request/response models.
- `src/OrdreFlow.Frontend/Services/`: contains API clients, provider abstractions, and scoped client-side state. `DummyOrdersProvider` currently supplies sample orders, while `SavedTimeEntriesState` retains successful entries for the current browser session.
- `src/OrdreFlow.Frontend/wwwroot/`: contains static assets, CSS, Bootstrap, and runtime configuration such as `appsettings.json`.
- `src/OrdreFlow.Frontend/Properties/launchSettings.json`: contains local launch profiles and development URLs.

## Application Flow

The current proof-of-concept flow is intentionally small:

1. `Home.razor` loads sample orders through `IOrdersProvider` and records the user's selection in `SelectedOrderState`.
2. `RegisterTime.razor` reads the selected order, validates the form with data annotations, and sends a `POST` request to `api/time_entries` through `TimeEntriesApiClient`.
3. The API response is converted into a typed result. Successes are added to `SavedTimeEntriesState`; failures are shown in the form.
4. `TimeEntries.razor` reads the entries saved during the current session and calculates the Monday-to-Sunday weekly total.

The frontend communicates with the backend through HTTP/JSON only. It does not connect directly to PostgreSQL, use Entity Framework Core, or replace business and security rules that belong in the API. Orders and saved-entry history will move to backend-backed services when the corresponding API endpoints are available.

## Running Locally

Install [Flox](https://flox.dev/docs/install-flox/install/) before setting up the
repository. The committed Flox environment provides the exact .NET SDK version
required by this project: `8.0.130`. The root `global.json` keeps the .NET CLI
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

The version check should print `8.0.130`. The app is served at the URL printed
in the console (see `src/OrdreFlow.Frontend/Properties/launchSettings.json`).

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
