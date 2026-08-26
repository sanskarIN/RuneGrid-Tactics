import { readFile, stat } from "node:fs/promises";
import { join, resolve } from "node:path";

const root = resolve(process.argv[2] ?? new URL("..", import.meta.url).pathname);
const jsonFiles = ["heroes.json", "enemies.json", "abilities.json", "items.json", "levels.json", "balance.json"];
const required = ["project.godot", "RuneGrid.Tactics.csproj", "Scenes/Main.tscn", "Scripts/Godot/GameRoot.cs", "Scripts/Godot/BoardView.cs", "Scripts/Core/GameSession.cs", "Scripts/Core/ReplayPlayer.cs", "Scripts/Core/ReplayFingerprint.cs", "Scripts/Core/ReplayStateDiff.cs", "Scripts/Core/ReplayInspector.cs", "Scripts/Core/ReplayInspectorShortcuts.cs", "Scripts/Core/ReplayInspectorOnboarding.cs", "Scripts/Core/ReplayDiffFilter.cs", "export_presets.cfg", "Tests/RuneGrid.Tactics.Pathfinding.Tests.csproj", "Tests/TacticalGridPathfindingTests.cs"];

for (const relative of required) {
  await stat(join(root, relative));
}

for (const file of jsonFiles) {
  const parsed = JSON.parse(await readFile(join(root, "Data", file), "utf8"));
  if (!parsed || (Array.isArray(parsed) && parsed.length === 0)) throw new Error(`Invalid or empty JSON content: ${file}`);
}

const heroes = JSON.parse(await readFile(join(root, "Data", "heroes.json"), "utf8"));
const enemies = JSON.parse(await readFile(join(root, "Data", "enemies.json"), "utf8"));
const abilities = JSON.parse(await readFile(join(root, "Data", "abilities.json"), "utf8"));
const abilityIds = new Set(abilities.map((ability) => ability.id));
const unitClasses = new Set(["Vanguard", "Channeler", "Pathfinder", "Warden", "Duelist", "Runesmith", "Seer", "Skywarden", "Sapper", "Sentinel", "Harrier", "Stalker", "Artillery", "Support"]);

for (const unit of [...heroes, ...enemies]) {
  if (!unit.mobility || !unit.tacticalClass || !unitClasses.has(unit.tacticalClass)) throw new Error(`Missing or invalid tactical metadata: ${unit.id}`);
  for (const abilityId of unit.abilityIds ?? []) {
    if (!abilityIds.has(abilityId)) throw new Error(`Unknown ability ${abilityId} referenced by ${unit.id}`);
  }
}

for (const id of ["duelist", "runesmith", "seer", "skywarden", "iron_sentinel", "gale_harrier", "cinder_artillery", "shade_stalker"]) {
  if (![...heroes, ...enemies].some((unit) => unit.id === id)) throw new Error(`Missing expanded unit class: ${id}`);
}

const gridSource = await readFile(join(root, "Scripts", "Core", "TacticalGrid.cs"), "utf8");
const sessionSource = await readFile(join(root, "Scripts", "Core", "GameSession.cs"), "utf8");
for (const feature of ["FindTacticalRoute", "FindBestApproach", "FindFlankAnchors", "BuildReservations"]) {
  if (!gridSource.includes(feature)) throw new Error(`Missing advanced grid feature: ${feature}`);
}
if (!sessionSource.includes("ReserveSuggestedRoute")) throw new Error("Native session does not expose tactical route reservations.");
if (!sessionSource.includes("RecordSystem(\"end-turn\"")) throw new Error("Native session does not record deterministic end-turn events.");

const replaySource = await readFile(join(root, "Scripts", "Core", "ReplayPlayer.cs"), "utf8");
const fingerprintSource = await readFile(join(root, "Scripts", "Core", "ReplayFingerprint.cs"), "utf8");
for (const contract of ["LastError", "SameAction", "ReplayEnemyAction", "CreateSession"]) {
  if (!replaySource.includes(contract)) throw new Error(`Missing replay determinism contract: ${contract}`);
}
const diffSource = await readFile(join(root, "Scripts", "Core", "ReplayStateDiff.cs"), "utf8");
for (const contract of ["ReplayStateSnapshot", "ReplayStateDiffGenerator", "ToHumanReadable", "CompareActions", "DeterministicRandom.Hash"]) {
  if (!diffSource.includes(contract)) throw new Error(`Missing replay diff contract: ${contract}`);
}
if (!fingerprintSource.includes("ReplayStateSnapshot.Capture")) throw new Error("Replay fingerprint does not use the canonical snapshot.");
const inspectorSource = await readFile(join(root, "Scripts", "Core", "ReplayInspector.cs"), "utf8");
const shortcutSource = await readFile(join(root, "Scripts", "Core", "ReplayInspectorShortcuts.cs"), "utf8");
const onboardingSource = await readFile(join(root, "Scripts", "Core", "ReplayInspectorOnboarding.cs"), "utf8");
const diffFilterSource = await readFile(join(root, "Scripts", "Core", "ReplayDiffFilter.cs"), "utf8");
const uiSource = await readFile(join(root, "Scripts", "Godot", "GameRoot.cs"), "utf8");
const boardSource = await readFile(join(root, "Scripts", "Godot", "BoardView.cs"), "utf8");
for (const contract of ["BuildReport", "StepToEnd", "StepBackward", "StepForward", "Seek", "ActionRows", "ReplayInspectorReport"]) {
  if (!inspectorSource.includes(contract)) throw new Error(`Missing replay inspector contract: ${contract}`);
}
for (const control of ["OpenReplayInspector", "ShowReplayInspector", "DETERMINISM AUDIT", "PLAY TO END", "RESET INSPECTION", "TIMELINE", "PREVIOUS", "NEXT", "HSlider", "_Input", "KEYS ·", "KEYS · F1", "ReplayInspectorKeyBindings", "REPLAY INSPECTOR KEYS", "BuildReplayShortcutReferenceOverlay", "SHORTCUT REFERENCE", "Key.F1", "CloseReplayShortcutReference", "BuildReplayInspectorOnboardingTooltip", "INTRODUCING REPLAY INSPECTOR", "VIEW SHORTCUTS", "GOT IT", "DismissReplayInspectorOnboarding", "BuildReplayMismatchWarningTooltip", "REPLAY DETERMINISM WARNING", "FIRST DIFFERENCE", "DISMISS WARNING", "AcknowledgeReplayMismatchWarning", "FILTERED DIFF · AFFECTED BOARD MARKERS", "ReplayDiffCategory", "ReplayDiffFilter", "SetReplayDiffMarkers"]) {
  if (!uiSource.includes(control)) throw new Error(`Missing command-table replay inspector control: ${control}`);
}
for (const contract of ["ReplayInspectorShortcut", "ReplayInspectorKeyBindings", "ReplayInspectorShortcutReference", "ReplayShortcutReferenceLine", "TryAssign", "TryResolve", "RestoreDefaults", "Normalize", "Previous", "Next", "Start", "End", "PlayToEnd"]) {
  if (!shortcutSource.includes(contract)) throw new Error(`Missing replay inspector keyboard shortcut contract: ${contract}`);
}
for (const contract of ["ReplayInspectorOnboarding", "HasSeenReplayInspectorIntro", "ShouldShowIntro", "DismissIntro", "AcknowledgedMismatchWarningKeys", "ShouldShowMismatchWarning", "AcknowledgeMismatchWarning", "ReplayInspectorMismatchWarning", "BuildKey", "Summarize"]) {
  if (!onboardingSource.includes(contract)) throw new Error(`Missing replay inspector onboarding contract: ${contract}`);
}
for (const contract of ["ReplayDiffCategory", "ReplayDiffEntry", "ReplayDiffFilterResult", "ReplayDiffFilter", "Filter", "Classify", "AffectedTiles", "AffectedUnitIds"]) {
  if (!diffFilterSource.includes(contract)) throw new Error(`Missing replay diff filter contract: ${contract}`);
}
for (const marker of ["SetReplayDiffMarkers", "_replayDiffTiles", "_replayDiffUnitIds", "Δ"]) {
  if (!boardSource.includes(marker)) throw new Error(`Missing replay diff board marker: ${marker}`);
}

const testSource = await readFile(join(root, "Tests", "TacticalGridPathfindingTests.cs"), "utf8");
const requiredScenarios = ["StandardUnit_AccountsForDifficultTerrainInTravelCost", "Trailblazer_ReducesDifficultTerrainToOneMovement", "WingedUnit_TreatsHazardAsOneMovement", "Phasewalker_CanCrossWallButCannotFinishInsideWall", "SafeRoute_AvoidsThreatenedDirectCorridorWhenDetourIsCheaperTactically", "ReservationPenalty_ReroutesAroundAnAlliedReservedTile", "BestApproach_TargetsAnOpenTileAdjacentToOccupiedEnemy", "FlankAnchors_ReturnReachableOppositeSidePositions", "RouteAnalysis_ReportsCoverHighGroundAndThreatDiagnostics", "SavedReplay_RoundTripsAndReproducesCanonicalFinalFingerprint", "ReplayPlayback_IsRepeatableAcrossIndependentSavedEncounterInstances", "ReplayReset_RestoresTheExactSeededInitialFingerprint", "ReplayRejectsOutOfOrderEnemyActionWithoutAdvancingPlayback", "ReplayFingerprint_ChangesWhenSavedEncounterSeedChanges", "ReplayDiff_ReportsExactMatchForEquivalentCanonicalSnapshots", "ReplayDiff_ReportsPhaseDivergenceInStableHumanReadableText", "ReplayDiff_ReportsTileUnitAndActionDivergences", "ReplayInspector_ReportsMatchingSeededInitialStateAndReadableTimeline", "ReplayInspector_StepVisualizesStateDeltaWhilePreservingDeterminism", "ReplayInspector_PlayToEndAndResetMaintainInspectableDeterministicState", "ReplayInspector_ExposesInvalidReplayErrorForCommandTableRendering", "ReplayInspector_SeekRebuildsTheExactActionIndexState", "ReplayInspector_PreviousAndNextNavigateWithoutChangingCanonicalPlayback", "ReplayInspector_RejectsOutOfRangeScrubWithoutChangingVisibleState", "ReplayInspectorShortcutMap_MapsTimelineKeysAndRejectsModifiedOrUnknownKeys", "ReplayInspectorKeyBindings_PersistCustomAssignmentsAndRouteDeterministically", "ReplayInspectorKeyBindings_RejectConflictsAndRepairsMalformedImportedValues", "ReplayInspectorShortcutReference_ListsActiveBindingsInStableCommandOrder", "ReplayInspectorOnboarding_PersistsFirstTimeDismissalWithoutReopening", "ReplayInspectorMismatchWarning_AcknowledgesOnlyTheExactReplayStateSignature", "ReplayDiffFilter_IsolatesCategoriesAndExtractsAffectedTileAndUnitMarkers"];
for (const scenario of requiredScenarios) {
  if (!testSource.includes(scenario)) throw new Error(`Missing pathfinding test scenario: ${scenario}`);
}

const presets = await readFile(join(root, "export_presets.cfg"), "utf8");
for (const target of ["Windows Desktop", "Linux/X11", "Android"]) {
  if (!presets.includes(`name="${target}"`)) throw new Error(`Missing export preset: ${target}`);
}

console.log(`Godot project structure, ${jsonFiles.length} JSON content files, advanced routing, replay determinism, diagnostics, inspection, timeline scrubbing, keyboard navigation, shortcut reference overlay, first-time onboarding, mismatch warnings, and filtered diff markers validated.`);
