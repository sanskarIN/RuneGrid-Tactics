# Releasing RuneGrid Tactics

Create a release only from a clean native-game branch. A release is a tested Godot executable or mobile package.

| Step | Native release action |
| --- | --- |
| 1 | Update `CHANGELOG.md`, `what_changed.md`, version metadata, and player-facing documentation. |
| 2 | Run `node Tools/validate-project.mjs .` and `dotnet build RuneGrid.Tactics.csproj`. |
| 3 | Open `project.godot` in Godot 4 .NET, then complete the desktop and target-device tactical smoke test. |
| 4 | Create the platform package with `Tools/export-release.sh` or the Godot Export dialog. |
| 5 | Inspect the package for local saves, credentials, keystores, and other excluded data before signing and distribution. |
| 6 | Commit a descriptive release record, wait for the Godot .NET GitHub Actions workflow, then create a release tag. |

Follow [BUILD_EXECUTABLES.md](BUILD_EXECUTABLES.md) for platform-specific signing, Android, and command-line details.
