# Repository Guidelines

## Project Structure & Module Organization
This repository is a small .NET console app. Source files live at the repo root (`Program.cs`, `GameSettings.cs`, `PathConfig.cs`, `ProcessResult.cs`, `ToolSettings.cs`). Configuration is stored in `appsettings.json`. Build artifacts are emitted to `bin/` and `obj/`.

Key paths used at runtime:
- `appsettings.json` holds `GameSettings` and `ToolSettings` paths (game data paths and `GBFRDataTools.exe`).
- Output copies extracted files under `original/{gbfrelink.*}` with a `GBFR/data` prefix.

## Build, Test, and Development Commands
- `dotnet build` builds the project using `GBFRDataExtrator.csproj`.
- `dotnet run --project GBFRDataExtrator.csproj` runs the console app.
- `dotnet clean` removes build outputs in `bin/` and `obj/`.

The app prompts for the full path to `modded_filelist.txt`, then extracts and copies listed files using `GBFRDataTools.exe`.

## Coding Style & Naming Conventions
- C# with nullable reference types enabled (`<Nullable>enable</Nullable>`) and implicit usings.
- Indentation: 4 spaces.
- Naming: `PascalCase` for types/methods/properties, `_camelCase` for private fields, file names match primary types (e.g., `GameSettings.cs`).
- Prefer simple, single-responsibility methods like `ValidatePaths` and `CopyExtractedFile`.
- XML doc comments are used on public types and members; keep them short.

## Testing Guidelines
There is no test project in this repository yet. If you add tests, place them in a sibling `tests/` directory with a dedicated test project (e.g., `GBFRDataExtrator.Tests`) and use `dotnet test` to run them. Prefer naming tests as `ClassName_MethodName_Condition`.

## Commit & Pull Request Guidelines
This repository has no git commits yet, so no established commit message convention exists. If starting history, use a simple Conventional Commits style (e.g., `feat: add file skip logic`, `fix: handle missing data.i`).

Pull requests should include:
- A clear summary of behavior changes.
- The command used to run the app (`dotnet run --project GBFRDataExtrator.csproj`).
- Any updates to `appsettings.json` or expected directory paths.

## Configuration & Safety Notes
- Avoid committing personal paths in `appsettings.json` when sharing publicly.
- The app launches external tools (`GBFRDataTools.exe`) and assumes the configured paths are valid.
