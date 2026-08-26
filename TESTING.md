# Native Testing Guide

RuneGrid Tactics keeps tactical navigation in framework-independent C# contracts, allowing the advanced grid system to be verified without rendering a Godot scene. The `Tests/RuneGrid.Tactics.Pathfinding.Tests.csproj` project links the core routefinding files and uses xUnit under .NET 8.

## Run the suite

Use the .NET SDK that accompanies the Godot 4 .NET development setup. From the repository root, run:

```bash
dotnet restore Tests/RuneGrid.Tactics.Pathfinding.Tests.csproj
dotnet test Tests/RuneGrid.Tactics.Pathfinding.Tests.csproj --no-restore --configuration Release
```

The GitHub Actions Godot .NET workflow restores, compiles, and runs the same test project on every relevant push and pull request. The local implementation sandbox does not include the .NET SDK or Godot .NET executable, so local runtime execution is intentionally deferred to a correctly provisioned native development machine or CI.

## Coverage contract

| Test family | Protected behavior |
| --- | --- |
| Terrain mobility | Standard, Trailblazer, and Winged units receive the expected travel cost across difficult and hazard tiles. |
| Phasewalking | A Phasewalker may route through a wall but can never end a movement or selected route inside a wall. |
| Safe and fast intents | Safe routes avoid threatened corridors when a better tactical detour exists; fastest routes take the short travel path. |
| Reservation awareness | Allied route reservations impose a configurable detour cost, while an owner’s own reservation remains unpenalized. |
| Occupied target approach | An approach route resolves a legal adjacent tile instead of attempting to occupy an enemy’s position. |
| Flank anchors | Opposite-side adjacent positions are discovered only when reachable and legal. |
| Route diagnostics | Route analysis reports threat exposure, cover stops, high-ground arrival, and reservation use. |
| Reservation state | Shared squad reservations retain a living unit’s first declared destination and ignore fallen units. |
| Saved replay round trip | Serialized replay data rebuilds the original seeded encounter and reproduces the same canonical final fingerprint. |
| Playback repeatability | Independent replay players produce the same fingerprint after every action in the same saved match. |
| Replay integrity | Reset restores the seeded initial fingerprint; an out-of-order enemy event is rejected without consuming the saved action. |
| Encounter identity | A changed saved seed produces a distinct canonical fingerprint before playback begins. |
| Snapshot equality | Equivalent canonical snapshots produce no differences and a clear successful comparison message. |
| Human-readable mismatches | Phase, tile, unit, and action divergence report stable expected-versus-actual lines for test failure output. |
| Command-table inspector | Inspector reports expose action progress, expected and current fingerprints, initial-state deltas, deterministic state comparison, reset, end playback, and rejected replay status. |
| Timeline scrubbing | Direct seek, previous, next, and timeline-row selection rebuild the same canonical state that sequential playback produces; out-of-range requests preserve the visible state. |
| Keyboard timeline navigation | Framework-independent mapping accepts unmodified Left, Right, Space, Home, End, and P keys for deterministic previous, next, opening, final, and play-to-end commands; modified and unknown keys are rejected. |
| Configurable replay keys | Local binding assignments serialize with the player record, resolve custom commands deterministically, reject duplicate or unsupported keys, normalize malformed imported assignments, and restore safe defaults. |
| Shortcut reference overlay | A framework-independent reference builder lists the active saved bindings in a stable previous, next, opening, final, and play-to-end order; the native inspector presents it through the F1 command and a dismissible overlay. |
| First-time inspector onboarding | The local onboarding state begins visible, serializes with the player record, and remains dismissed after the player acknowledges the inspector introduction or opens the shortcut reference. |
| Determinism mismatch warnings | A mismatch warning key includes action position and both fingerprints, so acknowledgement persists only for the exact observed divergence while changed action positions or state signatures warn again. |
| Filtered replay diffs | Stable replay diff lines classify as phase/state, tile, unit, or action records. Each filter returns only its category while extracting tile coordinates and unit identifiers for focused native board markers. |
| Diff filter keyboard cycling | Reserved unmodified F2 and F3 keys cycle backward and forward through every diff category with wrapping; modifier combinations and unrelated keys are rejected so configurable timeline bindings remain unaffected. |
| Direct numeric diff filters | Reserved unmodified 1–5 keys select All, Phase/state, Tile, Unit, and Action exactly. Standard, `Key`, keypad, and digit key-code forms resolve consistently while modifiers and unsupported digits are rejected. |
| Focused mismatch export | The active filtered audit serializes as deterministic JSON and CSV, retaining replay metadata, fingerprints, selected filter, affected entities, and only filtered difference lines. CSV escapes embedded quotes and delimiters; generated file names sanitize replay identifiers. |

When introducing a new mobility profile, tile type, route intent, or routing penalty, add a deterministic scenario to `TacticalGridPathfindingTests.cs` before changing encounter balance. This keeps navigation behavior explainable and protects replay determinism.

Replay actions are recorded with the authoritative turn, actor, type, target, and ability identifiers. During playback, `ReplayPlayer` validates that the source action and produced event agree before advancing its index. `ReplayFingerprint` serializes order-stable encounter, tile, unit, status, cooldown, and action state before applying the project’s deterministic hash. This makes a mismatch actionable without coupling test execution to frame timing or a rendered Godot scene.

`ReplayStateDiffGenerator` is the companion diagnostic path. It compares canonical snapshots in a fixed order and prints concise lines such as `phase: expected Player, actual Enemy` or `unit hero-scout: expected [...], actual [...]`. Replay equivalence tests include this text as their assertion message, so a fingerprint mismatch identifies its first concrete state difference directly in native test output.

The command-table inspector uses the same `ReplayInspector` core model. It rebuilds an expected replay state through the current action index, compares it with the visible playback session, and displays the resulting human-readable audit next to the replay board. A deterministic replay rejection remains inspectable: the audit can match while the explicit rejection reason explains why progression stopped.

Timeline scrubbing never mutates a previously rendered replay state. Instead, `Seek(actionIndex)` creates a fresh seeded replay player and replays exactly the requested number of authoritative actions. The slider and timeline rows expose post-action positions, while previous and next controls move one state at a time. This retains the same fingerprint as sequential playback and makes non-linear inspection safe for deterministic debugging.

The Godot inspector routes unmodified keyboard input through the same replay inspector methods as the visible controls before a previously focused command button can react. **Left Arrow** steps backward; **Right Arrow** and **Space** step forward; **Home** and **End** rebuild opening and final states; **P** runs to the end. The timeline slider intentionally does not take keyboard focus so its command-table shortcuts remain available, and text-entry controls retain their normal behavior.

The **REPLAY INSPECTOR KEYS** settings panel stores a local binding per replay command. A capture prompt accepts exactly one unmodified supported key; it rejects duplicates and invalid keys without changing the existing assignment, supports **Escape** cancellation, and can restore all defaults. Binding data is normalized after loading or importing a local record, ensuring malformed or duplicate imported values cannot make a replay control unreachable.

The replay inspector exposes the active-binding reference with **F1** and the **KEYS · F1** command-table button. The overlay consumes keyboard input so viewing the reference cannot accidentally move the replay. It displays the current local binding beside every replay command and closes only through **F1**, **Escape**, or its explicit close control.

On first opening an archived replay, the inspector presents a compact **INTRODUCING REPLAY INSPECTOR** tooltip. It explains exact-state reconstruction, offers a direct **VIEW SHORTCUTS** route to the reference overlay, and a **GOT IT** acknowledgement. Either action records the local onboarding dismissal; existing and imported player records receive a safe default onboarding state when the record is loaded.

When the expected and visible snapshots differ, the inspector displays a non-blocking **REPLAY DETERMINISM WARNING**. It exposes the first human-readable difference, points to the persistent **DETERMINISM AUDIT** panel, and offers shortcut help without preventing playback interaction. Acknowledgement is keyed to the current action index plus expected and current fingerprints, preventing duplicate notices for the same state while preserving warnings for new divergences.

The audit panel exposes **ALL**, **PHASE / STATE**, **TILE**, **UNIT**, and **ACTION** filters. The core filter parses the canonical difference format rather than presentation layout, so test coverage verifies each category and its affected entity extraction. The replay board receives only the selected filter’s markers: tiles gain a warm inner outline and living units gain a delta ring, without altering tactical selection or replay state.

Use **F2** and **F3** to move through the prior and next diff categories during playback. The reserved keys are parsed before configurable replay timeline bindings and do not reuse any key accepted by the settings-based binding capture. They are also listed in the shortcut reference, ensuring the active filter can be changed without moving focus to the command-table buttons.

Use **1**, **2**, **3**, **4**, and **5** to select All, Phase / state, Tile, Unit, and Action filters immediately. Direct selection is parsed after the F2/F3 navigator but before configurable timeline bindings, preserving deterministic input behavior and ensuring numeric selection cannot step or scrub the replay.

The replay audit’s **EXPORT FILTER JSON** and **EXPORT FILTER CSV** actions open a native save dialog. The export builder receives the current inspector report plus selected filter result, so export content does not infer state from rendering. JSON is intended for structured diagnostic tooling, while CSV offers a flat, spreadsheet-friendly row per filtered difference. A filter with no differences still exports its full replay metadata and an empty difference record, making the output unambiguous.
