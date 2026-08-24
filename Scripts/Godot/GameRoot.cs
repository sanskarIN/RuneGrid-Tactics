using Godot;
using System.Globalization;
using RuneGrid.Tactics.Core;
using GodotFileAccess = Godot.FileAccess;

namespace RuneGrid.Tactics.Godot;

public partial class GameRoot : Node
{
    private readonly Color _basalt = new("0B1516");
    private readonly Color _slate = new("172B2C");
    private readonly Color _parchment = new("E8DEC4");
    private readonly Color _route = new("56D6E6");
    private GameServices _services = null!;
    private Control _screen = null!;
    private ulong _nextEnemyActionMs;
    private ReplayInspector? _replayInspector;
    private ReplayInspectorShortcut? _bindingCapture;
    private string? _keyBindingNotice;
    private bool _showReplayShortcutOverlay;

    public override void _Ready()
    {
        _services = GetNode<GameServices>("/root/GameServices");
        _screen = new Control { GrowHorizontal = Control.GrowDirection.Both, GrowVertical = Control.GrowDirection.Both };
        _screen.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(_screen);
        ShowMainMenu();
    }

    public override void _Process(double delta)
    {
        var session = _services.CurrentSession;
        if (session?.Phase != GamePhase.Enemy || Time.GetTicksMsec() < _nextEnemyActionMs) return;
        _nextEnemyActionMs = Time.GetTicksMsec() + (ulong)(_services.SaveData.Accessibility.ReducedMotion ? 1 : 430);
        session.ResolveNextEnemy();
        if (session.Phase is GamePhase.Victory or GamePhase.Defeat) _services.CompleteCurrentSession();
    }

    public override void _Input(InputEvent @event)
    {
        if (_bindingCapture is { } captured && @event is InputEventKey bindingKey && bindingKey.Pressed && !bindingKey.Echo)
        {
            if (bindingKey.Keycode == Key.Escape)
            {
                _bindingCapture = null;
                _keyBindingNotice = "Key binding capture cancelled.";
            }
            else if (bindingKey.CtrlPressed || bindingKey.ShiftPressed || bindingKey.AltPressed || bindingKey.MetaPressed)
            {
                _keyBindingNotice = "Use one unmodified supported key.";
            }
            else if (_services.SaveData.Accessibility.ReplayKeyBindings.TryAssign(captured, bindingKey.Keycode.ToString(), out var error))
            {
                _bindingCapture = null;
                _keyBindingNotice = $"{ReplayInspectorKeyBindings.LabelFor(captured)} now uses {_services.SaveData.Accessibility.ReplayKeyBindings.Get(captured)}.";
                _services.Persist();
            }
            else _keyBindingNotice = error;
            GetViewport().SetInputAsHandled();
            ShowSettings();
            return;
        }
        if (_replayInspector is null || @event is not InputEventKey key || !key.Pressed || key.Echo) return;
        if (_showReplayShortcutOverlay)
        {
            if (!key.CtrlPressed && !key.ShiftPressed && !key.AltPressed && !key.MetaPressed && key.Keycode is Key.F1 or Key.Escape)
            {
                _showReplayShortcutOverlay = false;
                ShowReplayInspector();
            }
            GetViewport().SetInputAsHandled();
            return;
        }
        var focused = GetViewport().GuiGetFocusOwner();
        if (focused is LineEdit or TextEdit) return;
        if (!key.CtrlPressed && !key.ShiftPressed && !key.AltPressed && !key.MetaPressed && key.Keycode == Key.F1)
        {
            _showReplayShortcutOverlay = true;
            ShowReplayInspector();
            GetViewport().SetInputAsHandled();
            return;
        }
        if (!_services.SaveData.Accessibility.ReplayKeyBindings.TryResolve(key.Keycode.ToString(), key.CtrlPressed || key.ShiftPressed || key.AltPressed || key.MetaPressed, out var shortcut)) return;

        switch (shortcut)
        {
            case ReplayInspectorShortcut.Previous: _replayInspector.StepBackward(); break;
            case ReplayInspectorShortcut.Next: _replayInspector.StepForward(); break;
            case ReplayInspectorShortcut.Start: _replayInspector.Seek(0); break;
            case ReplayInspectorShortcut.End: _replayInspector.Seek(_replayInspector.Player.ActionCount); break;
            case ReplayInspectorShortcut.PlayToEnd: _replayInspector.StepToEnd(); break;
        }
        GetViewport().SetInputAsHandled();
    }

    private void ShowMainMenu()
    {
        ReplaceScreen();
        var root = MakeColumn(24);
        root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect, Control.LayoutPresetMode.Minsize, 30);
        root.AddChild(MakeHeader("RUNEGRID TACTICS", "FIELD ATLAS · LOCAL-FIRST COMMAND TABLE", ShowMainMenu));

        var body = new HBoxContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        body.AddThemeConstantOverride("separation", 22);
        root.AddChild(body);
        var left = MakeColumn(16); left.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        left.AddChild(MakeLabel("FIELD ATLAS · SECTOR 03", 14, _route));
        left.AddChild(MakeLabel("Mark a route.\nOwn the turn.", 44, _parchment));
        left.AddChild(MakeLabel("An original tactical roguelite where every movement mark, elemental choice, and damaged bridge changes the field.", 17, new Color("B9C0B6"), wrap: true));
        left.AddChild(MakeButton("BEGIN CAMPAIGN", () => StartMode(GameMode.Campaign), primary: true));
        left.AddChild(MakeButton("SEE ALL FIELD MODES", ShowModeLibrary));
        left.AddChild(MakeLabel($"{_services.Progression.Profile.Statistics.Victories} secured fields   ·   {_services.Progression.Profile.Relics.Count} discovered relics   ·   {_services.Progression.Profile.Shards} route shards", 13, new Color("AEB6AC"), wrap: true));
        body.AddChild(left);

        var center = MakePanel(); center.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill; center.CustomMinimumSize = new Vector2(420, 360);
        var centerStack = MakeColumn(10); center.AddChild(centerStack);
        centerStack.AddChild(MakeLabel("THE SUNKEN CAUSEWAY", 28, _parchment));
        centerStack.AddChild(MakeLabel("Campaign chapter 01 · Playable seeded tactical field", 14, new Color("B7BBAF")));
        centerStack.AddChild(MakeLabel("A route marker waits beneath the flooded causeway. The patrol knows the terrain; the field team knows the choices.", 16, new Color("D4D5C7"), wrap: true));
        centerStack.AddChild(MakeButton("DEPLOY TO CAUSEWAY", () => StartMode(GameMode.Campaign), primary: true));
        body.AddChild(center);

        var right = MakeColumn(10); right.CustomMinimumSize = new Vector2(260, 0);
        right.AddChild(MakeLabel("FIELD NOTES", 22, new Color("D4BF7E")));
        right.AddChild(MakeButton("DAILY CARTOGRAPHY", () => StartMode(GameMode.Daily)));
        right.AddChild(MakeButton("TRAINING GROUNDS", () => StartMode(GameMode.Training)));
        right.AddChild(MakeButton("HERO COLLECTION", ShowCollection));
        right.AddChild(MakeButton("FIELD CODEX", ShowCodex));
        right.AddChild(MakeButton("STATISTICS", ShowStatistics));
        right.AddChild(MakeButton("REPLAYS", ShowReplays));
        right.AddChild(MakeButton("SETTINGS & ACCESSIBILITY", ShowSettings));
        body.AddChild(right);

        root.AddChild(MakeLabel("Made by the Sanskar  ·  RuneGrid Tactics Godot C# migration", 12, new Color("89948D")));
        _screen.AddChild(root);
    }

    private void ShowModeLibrary()
    {
        ReplaceScreen();
        var root = MakeColumn(18); root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect, Control.LayoutPresetMode.Minsize, 30);
        root.AddChild(MakeHeader("FIELD MODE LIBRARY", "Every entry starts a real deterministic encounter.", ShowMainMenu));
        var grid = new GridContainer { Columns = 3, SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        grid.AddThemeConstantOverride("h_separation", 10);
        grid.AddThemeConstantOverride("v_separation", 10);
        foreach (var mode in Enum.GetValues<GameMode>())
        {
            var card = MakePanel();
            var content = MakeColumn(7); card.AddChild(content);
            content.AddChild(MakeLabel(mode switch { GameMode.BossRush => "Boss Rush", _ => mode.ToString() }, 20, _parchment));
            content.AddChild(MakeLabel(ModeDescription(mode), 13, new Color("B1B8AE"), wrap: true));
            content.AddChild(MakeButton("OPEN FIELD", () => StartMode(mode), primary: mode is GameMode.Campaign or GameMode.Expedition));
            grid.AddChild(card);
        }
        root.AddChild(grid);
        _screen.AddChild(root);
    }

    private void StartMode(GameMode mode, string? exactSeed = null)
    {
        var generatedSeed = $"{mode}-{Guid.NewGuid():N}";
        var seed = exactSeed ?? mode switch
        {
            GameMode.Daily => $"daily-{DateTime.UtcNow:yyyy-MM-dd}",
            GameMode.Weekly => $"weekly-{ISOWeek.GetWeekOfYear(DateTime.UtcNow)}-{DateTime.UtcNow:yyyy}",
            _ => generatedSeed[..Math.Min(18, generatedSeed.Length)].ToUpperInvariant()
        };
        var session = _services.StartEncounter(mode, seed);
        session.StateChanged += ShowBattle;
        ShowBattle();
    }

    private void ShowBattle()
    {
        var session = _services.CurrentSession;
        if (session is null) { ShowMainMenu(); return; }
        ReplaceScreen();
        var root = new VBoxContainer();
        root.AddThemeConstantOverride("separation", 8);
        root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect, Control.LayoutPresetMode.Minsize, 10);
        root.AddChild(MakeHeader(session.Encounter.Title, $"TURN {session.Turn} · {session.Phase} PHASE · {session.Encounter.Objective}", ShowPause));

        var body = new HBoxContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        body.AddThemeConstantOverride("separation", 8);
        var squad = MakeColumn(7); squad.CustomMinimumSize = new Vector2(245, 0); squad.AddChild(MakeLabel("FIELD TEAM", 15, _route));
        foreach (var hero in session.LivingHeroes)
        {
            var heroButton = MakeButton($"{hero.Template.Name} · {hero.Template.Title}\n{hero.Health}/{hero.Template.MaxHealth} vitality · {hero.Energy}/{hero.Template.Energy} energy", () => session.SelectUnit(hero.Id));
            heroButton.Disabled = session.Phase != GamePhase.Player;
            squad.AddChild(heroButton);
        }
        squad.AddChild(MakeButton("OPEN CODEX", ShowCodex));
        body.AddChild(squad);

        var board = new BoardView { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        board.Bind(session);
        board.TileChosen += point => session.ChooseTile(point);
        body.AddChild(board);

        var brief = MakeColumn(7); brief.CustomMinimumSize = new Vector2(245, 0);
        brief.AddChild(MakeLabel("FIELD BRIEF", 15, new Color("D4BF7E")));
        brief.AddChild(MakeLabel(session.Encounter.Objective, 15, _parchment, wrap: true));
        brief.AddChild(MakeLabel($"SEED\n{session.Encounter.Seed}\n\nTHREAT\n{session.LivingEnemies.Count()} hostiles\n\nRELIC\nWayfinder Etching", 13, new Color("B6BDAF"), wrap: true));
        foreach (var line in session.Log.Take(3)) brief.AddChild(MakeLabel(line, 13, new Color("D8D0B8"), wrap: true));
        body.AddChild(brief);
        root.AddChild(body);

        var command = new HBoxContainer();
        command.AddThemeConstantOverride("separation", 8);
        var selected = session.SelectedUnit;
        command.AddChild(MakeLabel(selected is null ? "Select a hero token, then mark a valid route." : $"{selected.Template.Name} · {selected.Template.Passive}", 14, _parchment, wrap: true, expand: true));
        if (selected is not null)
        {
            foreach (var abilityId in selected.Template.AbilityIds)
            {
                var ability = _services.Content.Abilities[abilityId];
                var button = MakeButton($"{ability.ShortName}\n{ability.Name}", () => session.SelectAbility(abilityId));
                button.Disabled = selected.Acted || selected.Energy < ability.EnergyCost || selected.Cooldowns.GetValueOrDefault(abilityId) > 0 || session.Phase != GamePhase.Player;
                command.AddChild(button);
            }
            var reserve = MakeButton("RESERVE ROUTE", () => { session.ReserveSuggestedRoute(); });
            reserve.Disabled = session.Phase != GamePhase.Player || selected.Moved || session.GetHighlights().SuggestedRoute.Count == 0;
            command.AddChild(reserve);
        }
        var undo = MakeButton("UNDO", () => { session.Undo(); }); undo.Disabled = !session.CanUndo; command.AddChild(undo);
        var end = MakeButton("END TURN →", () => { session.EndTurn(); }, primary: true); end.Disabled = session.Phase != GamePhase.Player; command.AddChild(end);
        root.AddChild(command);

        if (session.Phase is GamePhase.Victory or GamePhase.Defeat)
        {
            var report = MakePanel(); var reportContent = MakeColumn(8); report.AddChild(reportContent);
            reportContent.AddChild(MakeLabel(session.Phase == GamePhase.Victory ? "ROUTE SECURED." : "ROUTE INTERRUPTED.", 34, session.Phase == GamePhase.Victory ? _route : new Color("E18A76")));
            reportContent.AddChild(MakeLabel(session.Phase == GamePhase.Victory ? $"The field yields {session.Encounter.RewardShards} route shards and stores a local replay record." : "The team withdraws with its observations intact. Study the seed and return when ready.", 16, _parchment, wrap: true));
            reportContent.AddChild(MakeButton("RETURN TO COMMAND TABLE", ShowMainMenu, primary: true));
            root.AddChild(report);
        }
        _screen.AddChild(root);
    }

    private void ShowPause()
    {
        ReplaceScreen();
        var root = MakeColumn(14); root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center, Control.LayoutPresetMode.Minsize, 0); root.Position = new Vector2(430, 220); root.Size = new Vector2(420, 260);
        root.AddChild(MakeLabel("FIELD PAUSED", 14, _route));
        root.AddChild(MakeLabel("Take a reading.", 36, _parchment));
        root.AddChild(MakeButton("RETURN TO FIELD", ShowBattle, primary: true));
        root.AddChild(MakeButton("EXPORT LOCAL RECORD", ExportLocalRecord));
        root.AddChild(MakeButton("WITHDRAW TO COMMAND TABLE", ShowMainMenu));
        _screen.AddChild(root);
    }

    private void ShowCollection()
    {
        ReplaceScreen();
        var root = MakeColumn(12); root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect, Control.LayoutPresetMode.Minsize, 30); root.AddChild(MakeHeader("HERO COLLECTION", "Data-driven field records and local unlocks.", ShowMainMenu));
        foreach (var hero in _services.Content.Heroes.Values)
        {
            var unlocked = _services.Progression.Profile.UnlockedHeroes.Contains(hero.Id);
            root.AddChild(MakeLabel($"{hero.Title} · {hero.Name} {(unlocked ? "" : "· LOCKED")}\n{hero.Archetype} · {hero.MaxHealth} vitality · {hero.Movement} movement\nPassive: {hero.Passive}\nUltimate: {hero.Ultimate}", 16, unlocked ? _parchment : new Color("87918A"), wrap: true));
        }
        _screen.AddChild(root);
    }

    private void ShowCodex()
    {
        ReplaceScreen();
        var root = MakeColumn(11); root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect, Control.LayoutPresetMode.Minsize, 30); root.AddChild(MakeHeader("FIELD CODEX", "Elemental interactions and terrain are always explained in words.", ShowMainMenu));
        foreach (var entry in new[] { "Fire + Nature: scorching a rooted target deals +2 impact damage.", "Frost + Storm: a chilled target struck by storm loses its next movement.", "Arcane + Void: arcane wards halve the next void impact.", "Difficult terrain costs two route marks unless a pathfinder negates it.", "Healing markers restore health on entry; hazards deal predictable damage; linked teleport markers reposition a clear unit." }) root.AddChild(MakeLabel(entry, 16, _parchment, wrap: true));
        _screen.AddChild(root);
    }

    private void ShowStatistics()
    {
        ReplaceScreen();
        var stats = _services.Progression.Profile.Statistics;
        var winRate = stats.BattlesPlayed == 0 ? 0 : (int)Math.Round(100d * stats.Victories / stats.BattlesPlayed);
        var root = MakeColumn(12); root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect, Control.LayoutPresetMode.Minsize, 30); root.AddChild(MakeHeader("LOCAL FIELD LEDGER", "Statistics stay in the local player record until explicitly exported.", ShowMainMenu));
        root.AddChild(MakeLabel($"{stats.BattlesPlayed} fields entered · {stats.Victories} secured · {winRate}% win percentage\n{stats.TurnsPlayed} turns recorded · {stats.DamageDealt} impact dealt · {stats.HealingDone} healing\n{stats.BossesDefeated} bosses broken · {stats.ChallengesCompleted} challenges completed · {stats.LongestExpedition} longest expedition", 20, _parchment, wrap: true));
        _screen.AddChild(root);
    }

    private void ShowReplays()
    {
        _showReplayShortcutOverlay = false;
        ReplaceScreen();
        var root = MakeColumn(12); root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect, Control.LayoutPresetMode.Minsize, 30); root.AddChild(MakeHeader("ROUTE ARCHIVE", "Replay records retain seed, mode, difficulty, outcome, and compact tactical actions.", ShowMainMenu));
        if (_services.Progression.Profile.Replays.Count == 0) root.AddChild(MakeLabel("No route has been archived yet. Complete a field to retain its deterministic record.", 17, _parchment));
        foreach (var replay in _services.Progression.Profile.Replays)
        {
            var entry = new HBoxContainer(); entry.AddThemeConstantOverride("separation", 8);
            entry.AddChild(MakeLabel($"{replay.Mode} · {replay.Outcome} · {replay.Actions.Count} actions · {replay.Seed}", 15, _parchment, expand: true));
            entry.AddChild(MakeButton("INSPECT", () => OpenReplayInspector(replay), primary: true));
            root.AddChild(entry);
        }
        _screen.AddChild(root);
    }

    private void OpenReplayInspector(ReplayRecord replay)
    {
        _showReplayShortcutOverlay = false;
        _replayInspector = _services.InspectReplay(replay);
        _replayInspector.Changed += ShowReplayInspector;
        ShowReplayInspector();
    }

    private void ShowReplayInspector()
    {
        if (_replayInspector is null) { ShowReplays(); return; }
        ReplaceScreen();
        var inspector = _replayInspector;
        var report = inspector.BuildReport();
        var root = MakeColumn(10); root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect, Control.LayoutPresetMode.Minsize, 20);
        root.AddChild(MakeHeader("REPLAY INSPECTOR", $"{report.Record.Mode} · {report.Record.Seed} · action {report.CurrentActionIndex}/{report.ActionCount}", ShowReplays));

        var command = new HBoxContainer(); command.AddThemeConstantOverride("separation", 8);
        var previous = MakeButton("← PREVIOUS", () => { inspector.StepBackward(); }); previous.Disabled = report.CurrentActionIndex == 0; command.AddChild(previous);
        var step = MakeButton(report.IsComplete ? "PLAYBACK COMPLETE" : "STEP ACTION", () => { inspector.Step(); }, primary: true); step.Disabled = report.IsComplete || report.IsInvalid; command.AddChild(step);
        var next = MakeButton("NEXT →", () => { inspector.StepForward(); }); next.Disabled = report.IsComplete || report.IsInvalid; command.AddChild(next);
        var advance = MakeButton("PLAY TO END", () => { inspector.StepToEnd(); }); advance.Disabled = report.IsComplete || report.IsInvalid; command.AddChild(advance);
        command.AddChild(MakeButton("RESET INSPECTION", inspector.Reset));
        var reference = MakeButton("KEYS · F1", () => { _showReplayShortcutOverlay = true; ShowReplayInspector(); }); reference.TooltipText = "Open the active replay shortcut reference."; command.AddChild(reference);
        command.AddChild(MakeLabel(report.NextAction is null ? "No pending action." : $"NEXT · {FormatReplayAction(report.NextAction)}", 14, _parchment, expand: true));
        root.AddChild(MakeLabel(BuildReplayShortcutLegend(), 12, new Color("AEB6AC")));
        root.AddChild(command);

        var scrubber = new HBoxContainer(); scrubber.AddThemeConstantOverride("separation", 10);
        scrubber.AddChild(MakeLabel($"TIMELINE {report.CurrentActionIndex}/{report.ActionCount}", 13, _route));
        var timeline = new HSlider { MinValue = 0, MaxValue = report.ActionCount, Step = 1, Value = report.CurrentActionIndex, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, FocusMode = Control.FocusModeEnum.None, TooltipText = $"Drag to rebuild a precise replay state. {BuildReplayShortcutLegend()}" };
        timeline.ValueChanged += value => { inspector.Seek((int)Math.Round(value)); };
        scrubber.AddChild(timeline);
        scrubber.AddChild(MakeLabel(report.CurrentActionIndex == 0 ? "OPENING STATE" : report.IsComplete ? "FINAL STATE" : $"AFTER ACTION {report.CurrentActionIndex}", 13, _parchment));
        root.AddChild(scrubber);

        var body = new HBoxContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill }; body.AddThemeConstantOverride("separation", 10);
        var archive = MakeColumn(8); archive.CustomMinimumSize = new Vector2(280, 0);
        archive.AddChild(MakeLabel("ARCHIVE RECORD", 15, _route));
        archive.AddChild(MakeLabel($"SEED\n{report.Record.Seed}\n\nMODE\n{report.Record.Mode}\n\nOUTCOME\n{report.Record.Outcome ?? "unresolved"}\n\nACTIONS\n{report.ActionCount}", 14, _parchment, wrap: true));
        archive.AddChild(MakeLabel($"CURRENT FINGERPRINT\n{report.CurrentFingerprint}\n\nEXPECTED FINGERPRINT\n{report.ExpectedFingerprint}", 13, new Color("D4BF7E"), wrap: true));
        body.AddChild(archive);

        var board = new BoardView { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, SizeFlagsVertical = Control.SizeFlags.ExpandFill }; board.Bind(inspector.Player.Session); body.AddChild(board);

        var audit = MakeColumn(8); audit.CustomMinimumSize = new Vector2(330, 0);
        audit.AddChild(MakeLabel("DETERMINISM AUDIT", 15, _route));
        audit.AddChild(MakeLabel(report.DeterminismDifference.IsMatch ? "MATCH · reconstructed state equals live replay." : report.DeterminismDifference.ToHumanReadable(), 13, report.DeterminismDifference.IsMatch ? new Color("A9D9B3") : new Color("F1A18D"), wrap: true));
        audit.AddChild(MakeLabel("DELTA FROM INITIAL", 15, new Color("D4BF7E")));
        audit.AddChild(MakeLabel(report.DifferenceFromInitial.ToHumanReadable(), 12, _parchment, wrap: true, expand: true));
        if (report.IsInvalid) audit.AddChild(MakeLabel($"REPLAY REJECTED\n{report.Error}", 13, new Color("F1A18D"), wrap: true));
        body.AddChild(audit);
        root.AddChild(body);

        var actions = MakePanel(); var rows = MakeColumn(4); actions.AddChild(rows); rows.AddChild(MakeLabel("ACTION TIMELINE · SELECT AN ACTION TO SCRUB TO ITS RESULT", 15, _route));
        foreach (var row in inspector.ActionRows())
        {
            var label = $"{(row.IsResolved ? "✓" : row.IsCurrent ? "→" : "·")} {row.Index + 1:00}  {row.Label}";
            var actionButton = MakeButton(label, () => { inspector.Seek(row.Index + 1); }, primary: row.IsCurrent);
            actionButton.AddThemeFontSizeOverride("font_size", 12);
            rows.AddChild(actionButton);
        }
        root.AddChild(actions);
        _screen.AddChild(root);
        if (_showReplayShortcutOverlay) _screen.AddChild(BuildReplayShortcutReferenceOverlay());
    }

    private Control BuildReplayShortcutReferenceOverlay()
    {
        var overlay = new ColorRect { Color = new Color(0.02f, 0.06f, 0.07f, 0.9f), MouseFilter = Control.MouseFilterEnum.Stop, TooltipText = "Press F1 or Escape to close this shortcut reference." };
        overlay.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var card = MakePanel(); card.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center, Control.LayoutPresetMode.Minsize, 0); card.Position = new Vector2(300, 105); card.Size = new Vector2(680, 510);
        var content = MakeColumn(10); card.AddChild(content);
        content.AddChild(MakeLabel("SHORTCUT REFERENCE", 28, _route));
        content.AddChild(MakeLabel("ACTIVE REPLAY INSPECTOR BINDINGS", 13, new Color("D4BF7E")));
        foreach (var line in ReplayInspectorShortcutReference.Build(_services.SaveData.Accessibility.ReplayKeyBindings))
        {
            var row = new HBoxContainer(); row.AddThemeConstantOverride("separation", 12);
            row.AddChild(MakeLabel(line.Command, 16, _parchment, expand: true));
            row.AddChild(MakeLabel(line.Description, 12, new Color("AEB6AC"), expand: true));
            row.AddChild(MakeLabel(line.Binding, 15, _route));
            content.AddChild(row);
        }
        content.AddChild(MakeLabel("F1 or Escape closes this reference. Configure bindings in Settings & Accessibility.", 13, _parchment, wrap: true));
        content.AddChild(MakeButton("CLOSE REFERENCE", CloseReplayShortcutReference, primary: true));
        overlay.AddChild(card);
        return overlay;
    }

    private void CloseReplayShortcutReference()
    {
        _showReplayShortcutOverlay = false;
        ShowReplayInspector();
    }

    private void ShowSettings()
    {
        ReplaceScreen();
        var settings = _services.SaveData.Accessibility;
        var root = MakeColumn(10); root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect, Control.LayoutPresetMode.Minsize, 30); root.AddChild(MakeHeader("SETTINGS & ACCESSIBILITY", "All preferences are local and can be exported with a field record.", ExitSettings));
        root.AddChild(MakeToggle("High contrast", settings.HighContrast, value => { settings.HighContrast = value; _services.Persist(); }));
        root.AddChild(MakeToggle("Reduced motion", settings.ReducedMotion, value => { settings.ReducedMotion = value; _services.Persist(); }));
        root.AddChild(MakeToggle("Reduced flashing", settings.ReducedFlashing, value => { settings.ReducedFlashing = value; _services.Persist(); }));
        root.AddChild(MakeToggle("Vibration (on compatible mobile exports)", settings.Vibration, value => { settings.Vibration = value; _services.Persist(); }));
        root.AddChild(MakeToggle("Require tactical action confirmation", settings.ConfirmActions, value => { settings.ConfirmActions = value; _services.Persist(); }));
        root.AddChild(MakeButton($"Text scale: {settings.TextScale}", () => { settings.TextScale = settings.TextScale == "standard" ? "large" : settings.TextScale == "large" ? "x-large" : "standard"; _services.Persist(); ShowSettings(); }));
        root.AddChild(MakeLabel("REPLAY INSPECTOR KEYS", 17, _route));
        root.AddChild(MakeLabel(_bindingCapture is { } awaiting ? $"Press one unmodified key for {ReplayInspectorKeyBindings.LabelFor(awaiting)}. Supported: A–Z, arrows, Space, Home, End, Page Up, Page Down. Escape cancels." : "Select a command, then press one supported key. Every command must keep a distinct key.", 13, _parchment, wrap: true));
        if (!string.IsNullOrWhiteSpace(_keyBindingNotice)) root.AddChild(MakeLabel(_keyBindingNotice, 13, new Color("D4BF7E"), wrap: true));
        foreach (var shortcut in Enum.GetValues<ReplayInspectorShortcut>())
        {
            var bindingButton = MakeButton($"{ReplayInspectorKeyBindings.LabelFor(shortcut)} · {settings.ReplayKeyBindings.Get(shortcut)}", () => { _bindingCapture = shortcut; _keyBindingNotice = null; ShowSettings(); }, primary: _bindingCapture == shortcut);
            bindingButton.TooltipText = "Select this command, then press its new key.";
            root.AddChild(bindingButton);
        }
        root.AddChild(MakeButton("RESTORE DEFAULT REPLAY KEYS", () => { settings.ReplayKeyBindings.RestoreDefaults(); _bindingCapture = null; _keyBindingNotice = "Replay inspector keys restored to defaults."; _services.Persist(); ShowSettings(); }));
        root.AddChild(MakeButton("EXPORT LOCAL RECORD", ExportLocalRecord, primary: true));
        root.AddChild(MakeButton("OPEN IMPORT DIALOG", ImportLocalRecord));
        _screen.AddChild(root);
    }

    private void ExitSettings()
    {
        _bindingCapture = null;
        _keyBindingNotice = null;
        ShowMainMenu();
    }

    private void ExportLocalRecord()
    {
        var path = "user://runegrid-export.json";
        using var file = GodotFileAccess.Open(path, GodotFileAccess.ModeFlags.Write);
        file.StoreString(_services.Saves.Export(_services.SaveData));
        ShowMessage("Local record exported", $"A validated field record was written to {ProjectSettings.GlobalizePath(path)}.", ShowSettings);
    }

    private void ImportLocalRecord()
    {
        var dialog = new FileDialog { FileMode = FileDialog.FileModeEnum.OpenFile, Access = FileDialog.AccessEnum.Filesystem, Filters = new[] { "*.json ; RuneGrid local record" } };
        dialog.FileSelected += path =>
        {
            try
            {
                using var file = GodotFileAccess.Open(path, GodotFileAccess.ModeFlags.Read);
                _services.ReplaceSave(_services.Saves.Import(file.GetAsText()));
                ShowMessage("Local record restored", "The validated field record is now active.", ShowSettings);
            }
            catch (Exception) { ShowMessage("Import rejected", "The selected file was not a valid RuneGrid local record. Existing local data was not overwritten.", ShowSettings); }
        };
        AddChild(dialog); dialog.PopupCentered(new Vector2I(720, 460));
    }

    private void ShowMessage(string title, string message, Action back)
    {
        ReplaceScreen(); var root = MakeColumn(15); root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center, Control.LayoutPresetMode.Minsize, 0); root.Position = new Vector2(350, 230); root.Size = new Vector2(580, 240); root.AddChild(MakeLabel(title, 30, _route)); root.AddChild(MakeLabel(message, 16, _parchment, wrap: true)); root.AddChild(MakeButton("CONTINUE", back, primary: true)); _screen.AddChild(root);
    }

    private void ReplaceScreen()
    {
        foreach (var child in _screen.GetChildren()) child.QueueFree();
    }

    private PanelContainer MakePanel()
    {
        var panel = new PanelContainer();
        var style = new StyleBoxFlat { BgColor = _slate, BorderColor = new Color("3D7578"), ContentMarginLeft = 16, ContentMarginRight = 16, ContentMarginTop = 16, ContentMarginBottom = 16 };
        style.SetBorderWidthAll(1); panel.AddThemeStyleboxOverride("panel", style); return panel;
    }

    private VBoxContainer MakeColumn(int separation)
    {
        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", separation);
        return column;
    }

    private Label MakeLabel(string text, int size, Color color, bool wrap = false, bool expand = false)
    {
        var label = new Label { Text = text, AutowrapMode = wrap ? TextServer.AutowrapMode.WordSmart : TextServer.AutowrapMode.Off, SizeFlagsHorizontal = expand ? Control.SizeFlags.ExpandFill : Control.SizeFlags.ShrinkBegin };
        label.AddThemeColorOverride("font_color", color);
        label.AddThemeFontSizeOverride("font_size", size);
        return label;
    }
    private Button MakeButton(string text, Action action, bool primary = false)
    {
        var button = new Button { Text = text, CustomMinimumSize = new Vector2(0, 44), Alignment = HorizontalAlignment.Left };
        button.AddThemeFontSizeOverride("font_size", 15);
        button.Pressed += action;
        var style = new StyleBoxFlat { BgColor = primary ? _route : new Color("1D3839"), BorderColor = primary ? _route : new Color("5B7979"), ContentMarginLeft = 13, ContentMarginRight = 13, ContentMarginTop = 8, ContentMarginBottom = 8 };
        style.SetBorderWidthAll(1); button.AddThemeStyleboxOverride("normal", style); button.AddThemeColorOverride("font_color", primary ? _basalt : _parchment); return button;
    }
    private CheckButton MakeToggle(string text, bool selected, Action<bool> changed) { var toggle = new CheckButton { Text = text, ButtonPressed = selected }; toggle.Toggled += value => changed(value); return toggle; }
    private Control MakeHeader(string title, string subtitle, Action back) { var row = new HBoxContainer(); row.AddChild(MakeLabel(title, 25, _parchment, expand: true)); row.AddChild(MakeLabel(subtitle, 13, _route)); row.AddChild(MakeButton("COMMAND TABLE", back)); return row; }
    private static string ModeDescription(GameMode mode) => mode switch { GameMode.Campaign => "Story route through the fractured meridian.", GameMode.Expedition => "Seeded procedural encounter with local progression.", GameMode.Daily => "Shared date-marked deterministic field.", GameMode.Weekly => "A denser weekly anomaly patrol.", GameMode.Puzzle => "Win a compact field within six turns.", GameMode.Survival => "Hold the ridge through a hostile wave.", GameMode.BossRush => "Break the Stone Brute formation.", GameMode.Custom => "Generate a fresh personal field seed.", GameMode.Training => "Practice routes, targets, and turn timing.", GameMode.Tutorial => "A guided opening field for new players.", GameMode.Endless => "Continue to a new field after every victory.", _ => "Open a tactical field." };
    private static string FormatReplayAction(TacticalAction action) => $"T{action.Turn} · {action.ActorId} · {action.Type}" + (action.Target is { } target ? $" → {target}" : string.Empty) + (action.AbilityId is { } ability ? $" · {ability}" : string.Empty);
    private string BuildReplayShortcutLegend()
    {
        var bindings = _services.SaveData.Accessibility.ReplayKeyBindings;
        var next = bindings.Get(ReplayInspectorShortcut.Next) == "Right" ? "Right/Space" : bindings.Get(ReplayInspectorShortcut.Next);
        return $"KEYS · {bindings.Get(ReplayInspectorShortcut.Previous)}/{next} step · {bindings.Get(ReplayInspectorShortcut.Start)}/{bindings.Get(ReplayInspectorShortcut.End)} jump · {bindings.Get(ReplayInspectorShortcut.PlayToEnd)} play";
    }
}
