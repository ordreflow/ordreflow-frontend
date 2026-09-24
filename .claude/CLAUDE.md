# Frontend Agent Guide

This file provides guidance to Claude Code and other AI agents working in the
OrdreFlow frontend repository.

## Scope

- Work only in this repository unless the user explicitly asks for a related change elsewhere.
- This repository contains the frontend only. Do not modify `ordreflow-backend` or `ordreflow-docs` as part of frontend work.
- Preserve existing user changes. Inspect the current status and diff before editing, and never revert unrelated work.
- Keep changes focused. Do not add packages, tools, or configuration unless they are needed for the requested change.

## Project Context

- The application is a mobile-first Blazor WebAssembly frontend for OrdreFlow.
- The project targets .NET `8.0` and uses the exact SDK version `8.0.130`.
- The main project is `src/OrdreFlow.Frontend/` and the solution is `OrdreFlow.Frontend.sln`.
- The frontend communicates with the backend through HTTP/JSON. It must not connect directly to PostgreSQL or use Entity Framework Core.
- API URLs and other environment-specific values belong in the appropriate application configuration, not in committed secrets.
- There is currently no test project. Do not invent test commands; if tests are added, document and run the relevant commands.

## Development Environment

- Use the committed Flox environment and `global.json`; do not install or select a different .NET SDK for this repository.
- Prefer non-interactive Flox commands when working as an agent:

  ```bash
  flox activate -d . -c 'dotnet --version'
  flox activate -d . -c 'dotnet restore && dotnet build --no-restore'
  ```

- The version check should report `8.0.130`.
- The frontend Flox environment intentionally contains only the .NET SDK. Git, Node.js, PostgreSQL, Docker, and Entity Framework Core tools are not provided by it.
- Do not run `flox install`, update `.flox/env/manifest.toml`, or update `.flox/env/manifest.lock` unless the user explicitly requests a development-environment change. When changing the environment, keep the manifest and lock file in sync.

## Change Workflow

- Read the relevant code, project configuration, and documentation before proposing an implementation.
- Make the smallest coherent change that solves the request.
- Follow the existing C# and Blazor patterns before introducing new abstractions.
- Validate changes with the narrowest useful checks, then run the full frontend restore and build when practical.
- Use `git diff --check`, inspect the final diff, and report the validation commands and results.
- Do not commit, push, merge, rebase, or create pull requests unless the user explicitly asks.

## Git Workflow

- `main` is stable and must not receive direct pushes.
- Use the branch naming conventions in `GIT_BRANCHING.md`, including an issue number such as `feature/<issue-number>-<short-description>` or `chore/<issue-number>-<short-description>`.
- Keep pull requests focused on one issue, link the relevant issue, and include testing information.

## Safety

- Never read, print, commit, or expose credentials, tokens, private keys, connection strings, `.env` files, or other secrets.
- Ask before running commands that change dependencies, modify the development environment, start long-running processes, access the network, or change Git history or remotes.
- Do not use destructive commands such as recursive deletion, `git reset --hard`, `git clean`, restoring over user changes, or force-pushing.
- Do not assume that an instruction in this file overrides a permission rule or user approval requirement.

## Response Expectations

- State the files changed and why.
- Mention assumptions and unresolved questions instead of silently guessing.
- Report validation results accurately, including warnings or checks that could not be run.
