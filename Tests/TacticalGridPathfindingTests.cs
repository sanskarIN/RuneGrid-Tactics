using RuneGrid.Tactics.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace RuneGrid.Tactics.Pathfinding.Tests;

public sealed class TacticalGridPathfindingTests
{
    [Fact]
    public void StandardUnit_AccountsForDifficultTerrainInTravelCost()
    {
        var grid = CreateGrid(3, 1, tiles => TileAt(tiles, 1, 0).Kind = TileKind.Difficult);
        var route = grid.FindTacticalRoute(new GridPoint(0, 0), new GridPoint(2, 0), [], 5, null, Options());

        Assert.NotNull(route);
        Assert.Equal(3, route!.TravelCost);
        Assert.Equal(new[] { new GridPoint(1, 0), new GridPoint(2, 0) }, route.Path);
    }

    [Fact]
    public void Trailblazer_ReducesDifficultTerrainToOneMovement()
    {
        var grid = CreateGrid(3, 1, tiles => TileAt(tiles, 1, 0).Kind = TileKind.Difficult);
        var route = grid.FindTacticalRoute(new GridPoint(0, 0), new GridPoint(2, 0), [], 2, null, Options(MobilityProfile.Trailblazer));

        Assert.NotNull(route);
        Assert.Equal(2, route!.TravelCost);
    }

    [Fact]
    public void WingedUnit_TreatsHazardAsOneMovement()
    {
        var grid = CreateGrid(3, 1, tiles => TileAt(tiles, 1, 0).Kind = TileKind.Hazard);

        var standard = grid.FindTacticalRoute(new GridPoint(0, 0), new GridPoint(2, 0), [], 2, null, Options());
        var winged = grid.FindTacticalRoute(new GridPoint(0, 0), new GridPoint(2, 0), [], 2, null, Options(MobilityProfile.Winged));

        Assert.Null(standard);
        Assert.NotNull(winged);
        Assert.Equal(2, winged!.TravelCost);
    }

    [Fact]
    public void Phasewalker_CanCrossWallButCannotFinishInsideWall()
    {
        var grid = CreateGrid(3, 1, tiles => TileAt(tiles, 1, 0).Kind = TileKind.Wall);

        var standard = grid.FindTacticalRoute(new GridPoint(0, 0), new GridPoint(2, 0), [], 4, null, Options());
        var phaseRoute = grid.FindTacticalRoute(new GridPoint(0, 0), new GridPoint(2, 0), [], 4, null, Options(MobilityProfile.Phasewalker));
        var wallGoal = grid.FindTacticalRoute(new GridPoint(0, 0), new GridPoint(1, 0), [], 4, null, Options(MobilityProfile.Phasewalker));
        var reachable = grid.Reachable(new GridPoint(0, 0), 4, [], null, Options(MobilityProfile.Phasewalker));

        Assert.Null(standard);
        Assert.NotNull(phaseRoute);
        Assert.Equal(new[] { new GridPoint(1, 0), new GridPoint(2, 0) }, phaseRoute!.Path);
        Assert.Null(wallGoal);
        Assert.DoesNotContain(new GridPoint(1, 0), reachable.Keys);
        Assert.Contains(new GridPoint(2, 0), reachable.Keys);
    }

    [Fact]
    public void SafeRoute_AvoidsThreatenedDirectCorridorWhenDetourIsCheaperTactically()
    {
        var grid = CreateGrid(5, 3);
        var start = new GridPoint(0, 1);
        var goal = new GridPoint(4, 1);
        var threats = new HashSet<GridPoint> { new(1, 1), new(2, 1), new(3, 1) };

        var route = grid.FindTacticalRoute(start, goal, [], 12, null, Options(intent: RouteIntent.Safe, threats: threats, threatPenalty: 3));

        Assert.NotNull(route);
        Assert.DoesNotContain(route!.Path, threats.Contains);
        Assert.Equal(0, route.ThreatenedSteps);
        Assert.True(route.TravelCost > 4);
    }

    [Fact]
    public void FastestRoute_IgnoresSafetyWeightsAndUsesDirectCorridor()
    {
        var grid = CreateGrid(5, 3);
        var threats = new HashSet<GridPoint> { new(1, 1), new(2, 1), new(3, 1) };

        var route = grid.FindTacticalRoute(new GridPoint(0, 1), new GridPoint(4, 1), [], 8, null, Options(intent: RouteIntent.Fastest, threats: threats, threatPenalty: 8));

        Assert.NotNull(route);
        Assert.Equal(4, route!.TravelCost);
        Assert.Equal(3, route.ThreatenedSteps);
        Assert.All(route.Path, point => Assert.Equal(1, point.Y));
    }

    [Fact]
    public void ReservationPenalty_ReroutesAroundAnAlliedReservedTile()
    {
        var grid = CreateGrid(5, 3);
        var reservedPoint = new GridPoint(2, 1);
        var reservations = new Dictionary<GridPoint, string> { [reservedPoint] = "ally-a" };

        var route = grid.FindTacticalRoute(new GridPoint(0, 1), new GridPoint(4, 1), [], 12, "hero-b", Options(intent: RouteIntent.Safe, reservations: reservations, reservationOwner: "hero-b", reservationPenalty: 5));

        Assert.NotNull(route);
        Assert.DoesNotContain(reservedPoint, route!.Path);
        Assert.False(route.UsesReservation);
    }

    [Fact]
    public void OwnerReservation_DoesNotPenalizeOwnMarkedDestination()
    {
        var grid = CreateGrid(3, 1);
        var target = new GridPoint(2, 0);
        var reservations = new Dictionary<GridPoint, string> { [target] = "hero-a" };

        var route = grid.FindTacticalRoute(new GridPoint(0, 0), target, [], 2, "hero-a", Options(reservations: reservations, reservationOwner: "hero-a", reservationPenalty: 8));

        Assert.NotNull(route);
        Assert.Equal(2, route!.TacticalCost);
        Assert.False(route.UsesReservation);
    }

    [Fact]
    public void BestApproach_TargetsAnOpenTileAdjacentToOccupiedEnemy()
    {
        var grid = CreateGrid(5, 3);
        var enemy = Unit("enemy", new GridPoint(4, 1));

        var route = grid.FindBestApproach(new GridPoint(0, 1), enemy.Position, [enemy], 8, null, Options());

        Assert.NotNull(route);
        Assert.NotEqual(enemy.Position, route!.Path[^1]);
        Assert.Equal(1, route.Path[^1].ManhattanDistance(enemy.Position));
    }

    [Fact]
    public void FlankAnchors_ReturnReachableOppositeSidePositions()
    {
        var grid = CreateGrid(5, 3);
        var target = new GridPoint(2, 1);
        var enemy = Unit("enemy", target);

        var anchors = grid.FindFlankAnchors(new GridPoint(0, 1), target, [enemy], 8, null, Options(intent: RouteIntent.Flank));

        Assert.Contains(new GridPoint(3, 1), anchors);
        Assert.DoesNotContain(target, anchors);
    }

    [Fact]
    public void RouteAnalysis_ReportsCoverHighGroundAndThreatDiagnostics()
    {
        var grid = CreateGrid(3, 1, tiles =>
        {
            var cover = TileAt(tiles, 1, 0);
            cover.CoverValue = 1;
            var highGround = TileAt(tiles, 2, 0);
            highGround.IsHighGround = true;
            highGround.Elevation = 1;
        });
        var threats = new HashSet<GridPoint> { new(1, 0) };

        var route = grid.FindTacticalRoute(new GridPoint(0, 0), new GridPoint(2, 0), [], 8, null, Options(intent: RouteIntent.Direct, threats: threats));

        Assert.NotNull(route);
        Assert.Equal(1, route!.ThreatenedSteps);
        Assert.Equal(1, route.CoverStops);
        Assert.True(route.ReachesHighGround);
    }

    [Fact]
    public void BuildReservations_UsesFirstLivingUnitForSharedDestination()
    {
        var grid = CreateGrid(3, 1);
        var first = Unit("first", new GridPoint(0, 0), reserved: new GridPoint(2, 0));
        var second = Unit("second", new GridPoint(1, 0), reserved: new GridPoint(2, 0));
        var fallen = Unit("fallen", new GridPoint(1, 0), health: 0, reserved: new GridPoint(0, 0));

        var reservations = grid.BuildReservations([first, second, fallen]);

        Assert.Equal("first", reservations[new GridPoint(2, 0)]);
        Assert.DoesNotContain(new GridPoint(0, 0), reservations.Keys);
    }

    [Fact]
    public void SavedReplay_RoundTripsAndReproducesCanonicalFinalFingerprint()
    {
        var recorded = RecordOneRound("replay-roundtrip");
        var options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        var saved = JsonSerializer.Serialize(recorded, options);
        var restored = JsonSerializer.Deserialize<ReplayRecord>(saved, options);

        Assert.NotNull(restored);
        var first = new ReplayPlayer(restored!, BuildEncounter, NoAbilities);
        var second = new ReplayPlayer(restored!, BuildEncounter, NoAbilities);
        PlayAll(first);
        PlayAll(second);

        Assert.False(first.IsInvalid);
        Assert.False(second.IsInvalid);
        Assert.True(first.IsComplete);
        Assert.Equal(first.Fingerprint, second.Fingerprint);
        var expected = ReplayStateSnapshot.Capture(ExecuteReference(restored!), restored);
        var actual = ReplayStateSnapshot.Capture(first.Session, restored);
        var diff = ReplayStateDiffGenerator.Compare(expected, actual);
        Assert.True(diff.IsMatch, diff.ToHumanReadable());
        Assert.Equal(expected.Fingerprint, actual.Fingerprint);
    }

    [Fact]
    public void ReplayPlayback_IsRepeatableAcrossIndependentSavedEncounterInstances()
    {
        var record = RecordOneRound("replay-repeatable");
        var first = new ReplayPlayer(record, BuildEncounter, NoAbilities);
        var second = new ReplayPlayer(record, BuildEncounter, NoAbilities);

        var firstStates = CapturePlaybackFingerprints(first);
        var secondStates = CapturePlaybackFingerprints(second);

        Assert.Equal(firstStates, secondStates);
        Assert.Equal(record.Actions.Count, first.CurrentActionIndex);
        Assert.Equal(GamePhase.Player, first.Session.Phase);
    }

    [Fact]
    public void ReplayReset_RestoresTheExactSeededInitialFingerprint()
    {
        var record = RecordOneRound("replay-reset");
        var player = new ReplayPlayer(record, BuildEncounter, NoAbilities);
        var initial = player.Fingerprint;

        Assert.True(player.Step());
        Assert.NotEqual(initial, player.Fingerprint);
        player.Reset();

        Assert.Equal(0, player.CurrentActionIndex);
        Assert.False(player.IsInvalid);
        Assert.Equal(initial, player.Fingerprint);
    }

    [Fact]
    public void ReplayRejectsOutOfOrderEnemyActionWithoutAdvancingPlayback()
    {
        var record = new ReplayRecord(1, "saved-invalid", "replay-invalid", GameMode.Training, Difficulty.Field, DateTimeOffset.UnixEpoch,
            [new TacticalAction(1, "enemy-scout", "enemy", new GridPoint(2, 0), null, "invalid early enemy")], null);
        var player = new ReplayPlayer(record, BuildEncounter, NoAbilities);
        var initial = player.Fingerprint;

        Assert.False(player.Step());
        Assert.True(player.IsInvalid);
        Assert.Equal(0, player.CurrentActionIndex);
        Assert.Equal(initial, player.Fingerprint);
        Assert.Contains("expected an enemy phase", player.LastError);
    }

    [Fact]
    public void ReplayFingerprint_ChangesWhenSavedEncounterSeedChanges()
    {
        var first = new ReplayPlayer(RecordOneRound("field-alpha"), BuildEncounter, NoAbilities);
        var second = new ReplayPlayer(RecordOneRound("field-beta"), BuildEncounter, NoAbilities);

        Assert.NotEqual(first.Fingerprint, second.Fingerprint);
    }

    [Fact]
    public void ReplayDiff_ReportsExactMatchForEquivalentCanonicalSnapshots()
    {
        var record = RecordOneRound("diff-match");
        var snapshot = ReplayStateSnapshot.Capture(ExecuteReference(record), record);

        var diff = ReplayStateDiffGenerator.Compare(snapshot, snapshot);

        Assert.True(diff.IsMatch);
        Assert.Equal("Replay states match exactly.", diff.ToHumanReadable());
    }

    [Fact]
    public void ReplayDiff_ReportsPhaseDivergenceInStableHumanReadableText()
    {
        var record = RecordOneRound("diff-phase");
        var expected = ReplayStateSnapshot.Capture(ExecuteReference(record), record);
        var actual = expected with { Phase = GamePhase.Enemy };

        var diff = ReplayStateDiffGenerator.Compare(expected, actual);

        Assert.False(diff.IsMatch);
        Assert.Equal(new[] { "phase: expected Player, actual Enemy" }, diff.Lines);
        Assert.Equal("Replay state mismatch (1 difference):" + Environment.NewLine + " - phase: expected Player, actual Enemy", diff.ToHumanReadable());
    }

    [Fact]
    public void ReplayDiff_ReportsTileUnitAndActionDivergences()
    {
        var record = RecordOneRound("diff-entities");
        var expected = ReplayStateSnapshot.Capture(ExecuteReference(record), record);
        var tileKey = expected.Tiles.Keys.First();
        var unitKey = expected.Units.Keys.First();
        var actualTiles = expected.Tiles.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
        var actualUnits = expected.Units.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
        actualTiles[tileKey] = "kind=Wall; elevation=99; integrity=none; link=none; cover=0; highGround=False";
        actualUnits[unitKey] = "template=altered; faction=Hero; position=(9,9); health=1; energy=0; shield=0; moved=True; acted=True; reservation=none";
        var actual = expected with { Tiles = actualTiles, Units = actualUnits, Actions = expected.Actions.Concat(["turn=9; actor=system; type=unexpected; target=none; ability=none"]).ToList() };

        var diff = ReplayStateDiffGenerator.Compare(expected, actual);
        var message = diff.ToHumanReadable();

        Assert.False(diff.IsMatch);
        Assert.Contains($"tile {tileKey}: expected [", message);
        Assert.Contains($"unit {unitKey}: expected [", message);
        Assert.Contains($"action[{expected.Actions.Count}]: expected [<none>], actual [turn=9; actor=system; type=unexpected; target=none; ability=none]", message);
    }

    [Fact]
    public void ReplayInspector_ReportsMatchingSeededInitialStateAndReadableTimeline()
    {
        var record = RecordOneRound("inspector-initial");
        var inspector = new ReplayInspector(record, BuildEncounter, NoAbilities);

        var report = inspector.BuildReport();
        var rows = inspector.ActionRows();

        Assert.True(report.DeterminismDifference.IsMatch);
        Assert.True(report.DifferenceFromInitial.IsMatch);
        Assert.Equal(0, report.CurrentActionIndex);
        Assert.Equal(record.Actions.Count, report.ActionCount);
        Assert.NotNull(report.NextAction);
        Assert.True(rows[0].IsCurrent);
        Assert.Contains("hero-scout", rows[0].Label);
    }

    [Fact]
    public void ReplayInspector_StepVisualizesStateDeltaWhilePreservingDeterminism()
    {
        var inspector = new ReplayInspector(RecordOneRound("inspector-step"), BuildEncounter, NoAbilities);

        Assert.True(inspector.Step());
        var report = inspector.BuildReport();
        var rows = inspector.ActionRows();

        Assert.True(report.DeterminismDifference.IsMatch, report.DeterminismDifference.ToHumanReadable());
        Assert.False(report.DifferenceFromInitial.IsMatch);
        Assert.Equal(1, report.CurrentActionIndex);
        Assert.True(rows[0].IsResolved);
        Assert.True(rows[1].IsCurrent);
        Assert.Contains("unit hero-scout", report.DifferenceFromInitial.ToHumanReadable());
    }

    [Fact]
    public void ReplayInspector_PlayToEndAndResetMaintainInspectableDeterministicState()
    {
        var record = RecordOneRound("inspector-end");
        var inspector = new ReplayInspector(record, BuildEncounter, NoAbilities);

        Assert.Equal(record.Actions.Count, inspector.StepToEnd());
        var completed = inspector.BuildReport();
        Assert.True(completed.IsComplete);
        Assert.True(completed.DeterminismDifference.IsMatch, completed.DeterminismDifference.ToHumanReadable());

        inspector.Reset();
        var reset = inspector.BuildReport();
        Assert.Equal(0, reset.CurrentActionIndex);
        Assert.True(reset.DifferenceFromInitial.IsMatch);
        Assert.False(reset.IsInvalid);
    }

    [Fact]
    public void ReplayInspector_ExposesInvalidReplayErrorForCommandTableRendering()
    {
        var record = new ReplayRecord(1, "inspector-invalid", "inspector-invalid", GameMode.Training, Difficulty.Field, DateTimeOffset.UnixEpoch,
            [new TacticalAction(1, "enemy-scout", "enemy", new GridPoint(2, 0), null, "invalid early enemy")], null);
        var inspector = new ReplayInspector(record, BuildEncounter, NoAbilities);

        Assert.False(inspector.Step());
        var report = inspector.BuildReport();

        Assert.True(report.IsInvalid);
        Assert.Contains("expected an enemy phase", report.Error);
        Assert.True(report.DeterminismDifference.IsMatch);
    }

    [Fact]
    public void ReplayInspector_SeekRebuildsTheExactActionIndexState()
    {
        var record = RecordOneRound("inspector-seek");
        var inspector = new ReplayInspector(record, BuildEncounter, NoAbilities);
        var stepped = new ReplayInspector(record, BuildEncounter, NoAbilities);

        Assert.True(inspector.Seek(2));
        Assert.True(stepped.Step());
        Assert.True(stepped.Step());
        var seeked = inspector.BuildReport();
        var expected = stepped.BuildReport();

        Assert.Equal(2, seeked.CurrentActionIndex);
        Assert.True(seeked.DeterminismDifference.IsMatch, seeked.DeterminismDifference.ToHumanReadable());
        Assert.Equal(expected.CurrentFingerprint, seeked.CurrentFingerprint);
        Assert.True(inspector.ActionRows()[1].IsResolved);
        Assert.True(inspector.ActionRows()[2].IsCurrent);
    }

    [Fact]
    public void ReplayInspector_PreviousAndNextNavigateWithoutChangingCanonicalPlayback()
    {
        var record = RecordOneRound("inspector-navigation");
        var inspector = new ReplayInspector(record, BuildEncounter, NoAbilities);

        Assert.True(inspector.Seek(2));
        var atSecond = inspector.BuildReport().CurrentFingerprint;
        Assert.True(inspector.StepBackward());
        Assert.Equal(1, inspector.BuildReport().CurrentActionIndex);
        Assert.True(inspector.StepForward());
        var returned = inspector.BuildReport();

        Assert.Equal(2, returned.CurrentActionIndex);
        Assert.Equal(atSecond, returned.CurrentFingerprint);
        Assert.True(returned.DeterminismDifference.IsMatch, returned.DeterminismDifference.ToHumanReadable());
    }

    [Fact]
    public void ReplayInspector_RejectsOutOfRangeScrubWithoutChangingVisibleState()
    {
        var inspector = new ReplayInspector(RecordOneRound("inspector-bounds"), BuildEncounter, NoAbilities);
        var initial = inspector.BuildReport();

        Assert.False(inspector.Seek(-1));
        Assert.False(inspector.Seek(initial.ActionCount + 1));
        var after = inspector.BuildReport();

        Assert.Equal(initial.CurrentActionIndex, after.CurrentActionIndex);
        Assert.Equal(initial.CurrentFingerprint, after.CurrentFingerprint);
        Assert.True(after.DeterminismDifference.IsMatch);
    }

    [Fact]
    public void ReplayInspectorShortcutMap_MapsTimelineKeysAndRejectsModifiedOrUnknownKeys()
    {
        var expected = new Dictionary<string, ReplayInspectorShortcut>(StringComparer.OrdinalIgnoreCase)
        {
            ["Left"] = ReplayInspectorShortcut.Previous,
            ["Right"] = ReplayInspectorShortcut.Next,
            ["Space"] = ReplayInspectorShortcut.Next,
            ["Home"] = ReplayInspectorShortcut.Start,
            ["End"] = ReplayInspectorShortcut.End,
            ["P"] = ReplayInspectorShortcut.PlayToEnd
        };

        foreach (var entry in expected)
        {
            Assert.True(ReplayInspectorShortcutMap.TryParse(entry.Key, hasModifier: false, out var shortcut));
            Assert.Equal(entry.Value, shortcut);
        }

        Assert.False(ReplayInspectorShortcutMap.TryParse("Left", hasModifier: true, out _));
        Assert.False(ReplayInspectorShortcutMap.TryParse("Escape", hasModifier: false, out _));
    }

    [Fact]
    public void ReplayInspectorKeyBindings_PersistCustomAssignmentsAndRouteDeterministically()
    {
        var bindings = ReplayInspectorKeyBindings.CreateDefault();

        Assert.True(bindings.TryAssign(ReplayInspectorShortcut.Previous, "A", out _));
        Assert.True(bindings.TryAssign(ReplayInspectorShortcut.Next, "D", out _));
        var saved = JsonSerializer.Serialize(bindings);
        var restored = JsonSerializer.Deserialize<ReplayInspectorKeyBindings>(saved);

        Assert.NotNull(restored);
        restored!.Normalize();
        Assert.Equal("A", restored.Get(ReplayInspectorShortcut.Previous));
        Assert.Equal("D", restored.Get(ReplayInspectorShortcut.Next));
        Assert.True(restored.TryResolve("a", hasModifier: false, out var previous));
        Assert.Equal(ReplayInspectorShortcut.Previous, previous);
        Assert.True(restored.TryResolve("D", hasModifier: false, out var next));
        Assert.Equal(ReplayInspectorShortcut.Next, next);
        Assert.False(restored.TryResolve("Space", hasModifier: false, out _));
        Assert.False(restored.TryResolve("A", hasModifier: true, out _));
    }

    [Fact]
    public void ReplayInspectorKeyBindings_RejectConflictsAndRepairsMalformedImportedValues()
    {
        var bindings = ReplayInspectorKeyBindings.CreateDefault();

        Assert.True(bindings.TryAssign(ReplayInspectorShortcut.Previous, "A", out _));
        Assert.False(bindings.TryAssign(ReplayInspectorShortcut.End, "A", out var duplicateError));
        Assert.Contains("Previous", duplicateError);
        Assert.False(bindings.TryAssign(ReplayInspectorShortcut.Start, "Space", out var aliasError));
        Assert.Contains("alternate", aliasError);
        Assert.False(bindings.TryAssign(ReplayInspectorShortcut.Start, "Escape", out var unsupportedError));
        Assert.Contains("Choose a letter", unsupportedError);

        var malformed = new ReplayInspectorKeyBindings { Previous = "A", Next = "A", Start = "Escape", End = "", PlayToEnd = "P" };
        malformed.Normalize();

        var normalized = Enum.GetValues<ReplayInspectorShortcut>().Select(malformed.Get).ToList();
        Assert.Equal(normalized.Count, normalized.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(normalized, key => Assert.True(ReplayInspectorKeyBindings.TryNormalizeKey(key, out _)));
        malformed.RestoreDefaults();
        Assert.Equal("Left", malformed.Get(ReplayInspectorShortcut.Previous));
        Assert.True(malformed.TryResolve("Space", hasModifier: false, out var defaultNext));
        Assert.Equal(ReplayInspectorShortcut.Next, defaultNext);
    }

    [Fact]
    public void ReplayInspectorShortcutReference_ListsActiveBindingsInStableCommandOrder()
    {
        var bindings = ReplayInspectorKeyBindings.CreateDefault();
        Assert.True(bindings.TryAssign(ReplayInspectorShortcut.Previous, "A", out _));
        Assert.True(bindings.TryAssign(ReplayInspectorShortcut.Next, "D", out _));
        Assert.True(bindings.TryAssign(ReplayInspectorShortcut.Start, "Page Up", out _));
        Assert.True(bindings.TryAssign(ReplayInspectorShortcut.End, "Page Down", out _));
        Assert.True(bindings.TryAssign(ReplayInspectorShortcut.PlayToEnd, "Q", out _));

        var reference = ReplayInspectorShortcutReference.Build(bindings);

        Assert.Equal(new[] { ReplayInspectorShortcut.Previous, ReplayInspectorShortcut.Next, ReplayInspectorShortcut.Start, ReplayInspectorShortcut.End, ReplayInspectorShortcut.PlayToEnd }, reference.Select(line => line.Shortcut));
        Assert.Equal(new[] { "A", "D", "Page Up", "Page Down", "Q" }, reference.Select(line => line.Binding));
        Assert.Equal("Previous action", reference[0].Command);
        Assert.Equal("Resolve every remaining authoritative action.", reference[^1].Description);
    }

    [Fact]
    public void ReplayInspectorOnboarding_PersistsFirstTimeDismissalWithoutReopening()
    {
        var onboarding = new ReplayInspectorOnboarding();

        Assert.True(onboarding.ShouldShowIntro);
        var imported = JsonSerializer.Deserialize<ReplayInspectorOnboarding>(JsonSerializer.Serialize(onboarding));
        Assert.NotNull(imported);
        Assert.True(imported!.ShouldShowIntro);

        imported.DismissIntro();
        var restored = JsonSerializer.Deserialize<ReplayInspectorOnboarding>(JsonSerializer.Serialize(imported));

        Assert.NotNull(restored);
        Assert.True(restored!.HasSeenReplayInspectorIntro);
        Assert.False(restored.ShouldShowIntro);
    }

    [Fact]
    public void ReplayInspectorMismatchWarning_AcknowledgesOnlyTheExactReplayStateSignature()
    {
        var onboarding = new ReplayInspectorOnboarding();
        var mismatch = ReplayInspectorMismatchWarning.BuildKey(2, "expected-fingerprint", "current-fingerprint");
        var changedAction = ReplayInspectorMismatchWarning.BuildKey(3, "expected-fingerprint", "current-fingerprint");
        var changedState = ReplayInspectorMismatchWarning.BuildKey(2, "expected-fingerprint", "different-current-fingerprint");

        Assert.True(onboarding.ShouldShowMismatchWarning(mismatch));
        onboarding.AcknowledgeMismatchWarning(mismatch);

        Assert.False(onboarding.ShouldShowMismatchWarning(mismatch));
        Assert.True(onboarding.ShouldShowMismatchWarning(changedAction));
        Assert.True(onboarding.ShouldShowMismatchWarning(changedState));

        var restored = JsonSerializer.Deserialize<ReplayInspectorOnboarding>(JsonSerializer.Serialize(onboarding));
        Assert.NotNull(restored);
        Assert.False(restored!.ShouldShowMismatchWarning(mismatch));
        Assert.True(restored.ShouldShowMismatchWarning(changedState));
    }

    [Fact]
    public void ReplayDiffFilter_IsolatesCategoriesAndExtractsAffectedTileAndUnitMarkers()
    {
        var difference = new ReplayStateDiff(new[]
        {
            "phase: expected Player, actual Enemy",
            "tile 2:3: expected [kind=Floor], actual [kind=Hazard]",
            "unit vanguard-1: expected [health=12], actual [health=8]",
            "action[1]: expected [turn=1], actual [turn=2]"
        });

        var all = ReplayDiffFilter.Filter(difference, ReplayDiffCategory.All);
        var phases = ReplayDiffFilter.Filter(difference, ReplayDiffCategory.Phase);
        var tiles = ReplayDiffFilter.Filter(difference, ReplayDiffCategory.Tile);
        var units = ReplayDiffFilter.Filter(difference, ReplayDiffCategory.Unit);
        var actions = ReplayDiffFilter.Filter(difference, ReplayDiffCategory.Action);

        Assert.Equal(4, all.Entries.Count);
        Assert.Single(phases.Entries);
        Assert.Single(tiles.Entries);
        Assert.Single(units.Entries);
        Assert.Single(actions.Entries);
        Assert.Contains(new GridPoint(2, 3), tiles.AffectedTiles);
        Assert.Contains("vanguard-1", units.AffectedUnitIds);
        Assert.Empty(tiles.AffectedUnitIds);
        Assert.Contains("TILE · tile 2:3", tiles.ToHumanReadable());
        Assert.Equal("Phase / state", ReplayDiffFilter.LabelFor(ReplayDiffCategory.Phase));
    }

    [Fact]
    public void ReplayDiffFilterNavigator_CyclesReservedFiltersWithoutTimelineBindingConflicts()
    {
        Assert.Equal(ReplayDiffCategory.Phase, ReplayDiffFilterNavigator.Next(ReplayDiffCategory.All));
        Assert.Equal(ReplayDiffCategory.Tile, ReplayDiffFilterNavigator.Next(ReplayDiffCategory.Phase));
        Assert.Equal(ReplayDiffCategory.Unit, ReplayDiffFilterNavigator.Next(ReplayDiffCategory.Tile));
        Assert.Equal(ReplayDiffCategory.Action, ReplayDiffFilterNavigator.Next(ReplayDiffCategory.Unit));
        Assert.Equal(ReplayDiffCategory.All, ReplayDiffFilterNavigator.Next(ReplayDiffCategory.Action));
        Assert.Equal(ReplayDiffCategory.Action, ReplayDiffFilterNavigator.Previous(ReplayDiffCategory.All));

        Assert.True(ReplayDiffFilterNavigator.TryParseShortcut("F2", hasModifier: false, out var previous));
        Assert.Equal(ReplayDiffFilterShortcut.Previous, previous);
        Assert.True(ReplayDiffFilterNavigator.TryParseShortcut("f3", hasModifier: false, out var next));
        Assert.Equal(ReplayDiffFilterShortcut.Next, next);
        Assert.False(ReplayDiffFilterNavigator.TryParseShortcut("F2", hasModifier: true, out _));
        Assert.False(ReplayDiffFilterNavigator.TryParseShortcut("F1", hasModifier: false, out _));

        var reference = ReplayDiffFilterShortcutReference.Build();
        Assert.Equal(new[] { "F2", "F3" }, reference.Take(2).Select(line => line.Binding));
        Assert.Contains("prior focused mismatch category", reference[0].Description);
    }

    [Fact]
    public void ReplayDiffFilterNavigator_SelectsExactCategoriesFromReservedNumericKeys()
    {
        var expected = new Dictionary<string, ReplayDiffCategory>(StringComparer.OrdinalIgnoreCase)
        {
            ["1"] = ReplayDiffCategory.All,
            ["Key2"] = ReplayDiffCategory.Phase,
            ["D3"] = ReplayDiffCategory.Tile,
            ["Kp4"] = ReplayDiffCategory.Unit,
            ["5"] = ReplayDiffCategory.Action
        };

        foreach (var entry in expected)
        {
            Assert.True(ReplayDiffFilterNavigator.TryParseDirectShortcut(entry.Key, hasModifier: false, out var category));
            Assert.Equal(entry.Value, category);
        }

        Assert.False(ReplayDiffFilterNavigator.TryParseDirectShortcut("1", hasModifier: true, out _));
        Assert.False(ReplayDiffFilterNavigator.TryParseDirectShortcut("0", hasModifier: false, out _));
        Assert.False(ReplayDiffFilterNavigator.TryParseDirectShortcut("F2", hasModifier: false, out _));

        var reference = ReplayDiffFilterShortcutReference.Build();
        Assert.Equal(new[] { "1", "2", "3", "4", "5" }, reference.Skip(2).Select(line => line.Binding));
        Assert.Contains("full audit", reference[2].Description);
    }

    [Fact]
    public void ReplayMismatchExport_ProducesStableJsonCsvAndSafeFocusedFileNames()
    {
        var export = new ReplayMismatchExport(
            SchemaVersion: 1,
            EncounterId: "training-field",
            Seed: "seed/alpha",
            Mode: "Training",
            Difficulty: "Field",
            ReplayCreatedAt: "2026-08-26T00:00:00.0000000Z",
            CurrentActionIndex: 2,
            ActionCount: 4,
            Filter: "Tile",
            ExpectedFingerprint: "ABCD1234",
            CurrentFingerprint: "DEAD5678",
            IsDeterministicMatch: false,
            AffectedTiles: new[] { "2:3" },
            AffectedUnits: new[] { "vanguard-1" },
            Differences: new[] { new ReplayMismatchExportLine("Tile", "tile 2:3: expected [kind=\"Floor\"], actual [kind=Hazard, elevated]") });

        var json = export.ToJson();
        var csv = export.ToCsv();

        using var document = JsonDocument.Parse(json);
        Assert.Equal("Tile", document.RootElement.GetProperty("filter").GetString());
        Assert.Equal("2:3", document.RootElement.GetProperty("affectedTiles")[0].GetString());
        Assert.Contains("\"filtered_replay_mismatch\"", csv);
        Assert.Contains("\"Tile\"", csv);
        Assert.Contains("\"\"Floor\"\"", csv);
        Assert.Equal("runegrid-replay-diff-training-field-seed-alpha-action-2-tile.csv", export.BuildFileName(".CSV"));
    }

    private static TacticalGrid CreateGrid(int width, int height, Action<List<Tile>>? configure = null)
    {
        var tiles = Enumerable.Range(0, height)
            .SelectMany(y => Enumerable.Range(0, width).Select(x => new Tile { Position = new GridPoint(x, y) }))
            .ToList();
        configure?.Invoke(tiles);
        return new TacticalGrid(width, height, tiles);
    }

    private static Tile TileAt(IEnumerable<Tile> tiles, int x, int y) => tiles.Single(tile => tile.Position == new GridPoint(x, y));

    private static UnitState Unit(string id, GridPoint position, MobilityProfile mobility = MobilityProfile.Standard, int health = 10, GridPoint? reserved = null, Faction faction = Faction.Hero) => new()
    {
        Id = id,
        Position = position,
        Health = health,
        Energy = 3,
        ReservedDestination = reserved,
        Template = new UnitTemplate
        {
            Id = $"template-{id}",
            Name = id,
            Title = "Test Unit",
            Faction = faction,
            Archetype = "Test",
            MaxHealth = 10,
            Attack = 3,
            Defense = 1,
            Movement = 4,
            Energy = 3,
            Element = ElementKind.None,
            Passive = "Test",
            Ultimate = "Test",
            Color = "#FFFFFF",
            Mobility = mobility,
            TacticalClass = TacticalClass.Vanguard
        }
    };

    private static RouteOptions Options(
        MobilityProfile mobility = MobilityProfile.Standard,
        RouteIntent intent = RouteIntent.Direct,
        IReadOnlySet<GridPoint>? threats = null,
        IReadOnlyDictionary<GridPoint, string>? reservations = null,
        string? reservationOwner = null,
        int threatPenalty = 2,
        int reservationPenalty = 4) => new()
        {
            Mobility = mobility,
            Intent = intent,
            ThreatenedTiles = threats ?? new HashSet<GridPoint>(),
            Reservations = reservations ?? new Dictionary<GridPoint, string>(),
            ReservationOwnerId = reservationOwner,
            ThreatPenalty = threatPenalty,
            ReservationPenalty = reservationPenalty
        };

    private static readonly IReadOnlyDictionary<string, AbilityDefinition> NoAbilities = new Dictionary<string, AbilityDefinition>();

    private static ReplayRecord RecordOneRound(string seed)
    {
        var session = ReplayReference(new ReplayRecord(1, $"training-{seed}", seed, GameMode.Training, Difficulty.Field, DateTimeOffset.UnixEpoch, [], null));
        Assert.True(session.SelectUnit("hero-scout"));
        Assert.True(session.ChooseTile(new GridPoint(1, 0)));
        Assert.True(session.EndTurn());
        while (session.Phase == GamePhase.Enemy) session.ResolveNextEnemy();
        var record = session.CreateReplay();
        Assert.Equal(new[] { "move", "end-turn", "enemy" }, record.Actions.Select(action => action.Type));
        return record;
    }

    private static GameSession ReplayReference(ReplayRecord record)
    {
        var session = new GameSession(BuildEncounter(record.Seed, record.Mode, record.Difficulty), NoAbilities);
        session.Start();
        return session;
    }

    private static GameSession ExecuteReference(ReplayRecord record)
    {
        var session = ReplayReference(record);
        Assert.True(session.SelectUnit("hero-scout"));
        Assert.True(session.ChooseTile(new GridPoint(1, 0)));
        Assert.True(session.EndTurn());
        while (session.Phase == GamePhase.Enemy) session.ResolveNextEnemy();
        return session;
    }

    private static EncounterState BuildEncounter(string seed, GameMode mode, Difficulty difficulty)
    {
        var random = new DeterministicRandom(seed);
        var grid = CreateGrid(5, 2, tiles =>
        {
            TileAt(tiles, 2, 1).Kind = random.Chance(0.5f) ? TileKind.Difficult : TileKind.Hazard;
            TileAt(tiles, 3, 0).CoverValue = 1;
            TileAt(tiles, 4, 1).IsHighGround = random.Chance(0.5f);
        });
        return new EncounterState
        {
            Id = $"{mode}-{seed}",
            Seed = seed,
            Mode = mode,
            Difficulty = difficulty,
            Title = "Replay Test Field",
            Briefing = "Deterministic replay fixture.",
            Objective = "Complete a recorded turn.",
            Grid = grid,
            Units = [
                Unit("hero-scout", new GridPoint(0, 0), mobility: MobilityProfile.Trailblazer),
                Unit("enemy-scout", new GridPoint(4, 0), mobility: MobilityProfile.Standard, faction: Faction.Enemy)
            ],
            RewardShards = 1,
            RewardMastery = 1
        };
    }

    private static void PlayAll(ReplayPlayer player)
    {
        while (!player.IsComplete && !player.IsInvalid) Assert.True(player.Step());
    }

    private static IReadOnlyList<string> CapturePlaybackFingerprints(ReplayPlayer player)
    {
        var fingerprints = new List<string> { player.Fingerprint };
        while (!player.IsComplete && !player.IsInvalid)
        {
            Assert.True(player.Step());
            fingerprints.Add(player.Fingerprint);
        }
        Assert.False(player.IsInvalid);
        return fingerprints;
    }
}
