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

## Repository Relationships

- [Backend API](https://github.com/ordreflow/ordreflow-backend)
- [Shared documentation](https://github.com/ordreflow/ordreflow-docs)
- [GitHub Project](https://github.com/orgs/ordreflow/projects)

## Current Status

The Blazor WebAssembly project has been scaffolded. API configuration and test commands will be added as the POC flow is implemented.

## Project Structure

- `OrdreFlow.Frontend.sln` — solution file
- `src/OrdreFlow.Frontend/` — Blazor WebAssembly standalone app
- `.flox/env/manifest.toml` — Flox environment definition
- `.flox/env/manifest.lock` — locked Flox package resolution
- `global.json` — required .NET SDK version

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
