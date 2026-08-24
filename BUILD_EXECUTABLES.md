# Building RuneGrid Tactics Executables

RuneGrid Tactics is a native **Godot 4 .NET** project. C# builds require both the **.NET-enabled Godot editor** and a matching 64-bit .NET SDK. Godot’s C# documentation states that Godot runs compiled C# games but does not bundle the C# build tools; current Godot 4.5 guidance requires .NET 8 or later, and Android export requires .NET 9 or later.[1]

> This workspace did not include the Godot .NET editor or .NET SDK during implementation. The source structure, export presets, scripts, and documentation have been created, but final native executable generation must run on a machine with the toolchain installed.

## 1. Install the required tools

Install **Godot 4.5 .NET** from the official Godot download page, not the standard non-.NET editor. Install the current 64-bit .NET SDK, using .NET 8 or later for desktop and .NET 9 or later for Android. Keep the editor, SDK, and target architecture consistent.[1]

For Windows development, Visual Studio’s **.NET desktop development** workload or VS Code with the Microsoft C# extension are suitable. Godot’s C# guide explains how to point an external debugger at the Godot executable with a `GODOT4` environment variable.[1]

| Target         | Minimum tooling beyond Godot .NET and .NET SDK                                                     | Primary output                                                      |
| -------------- | -------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------- |
| Windows x86_64 | Godot export templates                                                                             | `build/windows/RuneGridTactics.exe`                                 |
| Linux x86_64   | Godot export templates                                                                             | `build/linux/RuneGridTactics.x86_64`                                |
| macOS          | Godot export templates; perform notarization/signing on macOS                                      | `.app` bundle or distribution archive                               |
| Android        | Android Studio, JDK, Android SDK, configured debug/release keystore, Godot Android export template | `build/android/RuneGridTactics.apk` or AAB from the editor workflow |

Godot export templates are required to create playable builds. Install them through **Editor → Manage Export Templates**, then add or review presets in **Project → Export**.[2]

## 2. Open and compile the project

Open the repository-root `project.godot` file in Godot 4 .NET. Let Godot import the project, then use the **Build** button in the editor or run the following from the repository root:

```bash
dotnet build RuneGrid.Tactics.csproj
```

Run the deterministic core-pathfinding suite before exporting a package. The test project links the pure C# grid contracts directly, so these tests do not require launching a Godot scene:

```bash
dotnet test Tests/RuneGrid.Tactics.Pathfinding.Tests.csproj --configuration Release
```

Godot generates or maintains the C# solution and project files as part of its .NET workflow. Commit the `.csproj` and solution when generated; exclude the `.godot` cache and `.godot/mono` cache from source control.[1]

To run the native project from the editor, press **F6** for the current scene or **F5** for the configured main scene. A successful run opens the native command table, where every mode starts a deterministic C# tactical encounter.

## 3. Prepare release configuration

The committed `export_presets.cfg` defines Windows Desktop, Linux/X11, and Android destinations. It includes `*.json` in exported content so data-authored heroes, enemies, abilities, equipment, levels, and balance remain available at runtime. Godot documents `export_presets.cfg` as suitable for source control, while `.godot/export_credentials.cfg` can contain confidential credentials and must not be committed.[2]

Before a signed Android release, open **Project → Export → Android** and configure the local keystore values. Keep passwords and keys only in local credential files or a CI secret store. Do not commit them.

## 4. Create the Windows executable

### Editor path

Open **Project → Export**, choose **Windows Desktop**, set the path to `build/windows/RuneGridTactics.exe`, select **Export Project**, and choose a release build. Zip the entire output directory for distribution because it may contain the executable and PCK data.

### Command-line path

Set `GODOT4` to the path of the Godot .NET executable or ensure the executable is on `PATH`, then run:

```bash
./Tools/export-release.sh windows
```

On PowerShell:

```powershell
./Tools/export-release.ps1 -Target windows -Godot $env:GODOT4
```

The script invokes Godot’s documented `--headless --path <project> --export-release "Windows Desktop" <output>` flow. Command-line export still requires the matching export preset and installed template.[2]

## 5. Create Linux, macOS, and Android artifacts

Linux follows the same command path with `./Tools/export-release.sh linux`. Mark the output executable after copying it to a Linux distribution target:

```bash
chmod +x build/linux/RuneGridTactics.x86_64
```

For macOS, use a macOS host and add a macOS export preset in the editor. Godot’s command-line guide notes that the engine executable lives inside `Godot.app/Contents/MacOS/Godot` when invoked from a macOS terminal.[3] Sign and notarize the final `.app` according to current Apple requirements before distribution.

For Android, first configure the Android SDK, JDK, template, and keystore through the Godot editor, then run `./Tools/export-release.sh android` for a release APK once the preset validates. C# Android export support is documented as experimental in current Godot 4 guidance; test on real devices and preserve a native export validation step in every release.[1]

## 6. Release checklist

| Check              | Required evidence                                                                                                |
| ------------------ | ---------------------------------------------------------------------------------------------------------------- |
| C# compilation     | `dotnet build RuneGrid.Tactics.csproj` exits successfully.                                                       |
| Pathfinding suite  | `dotnet test Tests/RuneGrid.Tactics.Pathfinding.Tests.csproj --configuration Release` exits successfully.       |
| Godot import       | Open `project.godot` in the .NET editor with no parser errors.                                                   |
| Core play          | Start a field, select a hero, move, use an ability, end the turn, complete or lose a field, and reopen the save. |
| Local data         | Export a record, import it back, and verify a malformed file is rejected without replacing the existing record.  |
| Accessibility      | Validate text scale, high contrast, reduced motion, readable non-color state cues, and touch-sized controls.     |
| Platform packaging | Install the final package on a clean target device or VM and test offline.                                       |
| Security           | Confirm the export contains no keystore, passwords, `export_credentials.cfg`, private saves, or CI secrets.      |

## References

[1] [Godot Engine documentation — C# basics](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_basics.html)

[2] [Godot Engine documentation — Exporting projects](https://docs.godotengine.org/en/latest/tutorials/export/exporting_projects.html)

[3] [Godot Engine documentation — Command line tutorial](https://docs.godotengine.org/en/latest/tutorials/editor/command_line_tutorial.html)
