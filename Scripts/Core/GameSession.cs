namespace RuneGrid.Tactics.Core;

public sealed class GameSession
{
    private readonly IReadOnlyDictionary<string, AbilityDefinition> _abilities;
    private readonly List<TacticalAction> _actions = [];
    private readonly List<string> _log = [];
    private readonly DeterministicRandom _combatRandom;
    private List<UnitState>? _undoSnapshot;
    private string? _selectedUnitId;
    private string? _selectedAbilityId;
    private bool _enemyPhaseFinished;

    public EncounterState Encounter { get; }
    public GamePhase Phase { get; private set; } = GamePhase.Briefing;
    public int Turn { get; private set; } = 1;
    public IReadOnlyList<string> Log => _log;
    public IReadOnlyList<TacticalAction> Actions => _actions;
    public UnitState? SelectedUnit => Encounter.Units.SingleOrDefault(unit => unit.Id == _selectedUnitId);
    public AbilityDefinition? SelectedAbility => _selectedAbilityId is not null && _abilities.TryGetValue(_selectedAbilityId, out var ability) ? ability : null;
    public IEnumerable<UnitState> LivingHeroes => Encounter.Units.Where(unit => unit.Faction == Faction.Hero && unit.IsAlive);
    public IEnumerable<UnitState> LivingEnemies => Encounter.Units.Where(unit => unit.Faction == Faction.Enemy && unit.IsAlive);
    public bool CanUndo => _undoSnapshot is not null && Phase == GamePhase.Player;

    public event Action? StateChanged;
    public event Action<string>? MessageLogged;

    public GameSession(EncounterState encounter, IReadOnlyDictionary<string, AbilityDefinition> abilities)
    {
        Encounter = encounter;
        _abilities = abilities;
        _combatRandom = new DeterministicRandom($"{encounter.Seed}:combat");
        LogMessage($"{Encounter.Title}: {Encounter.Objective}");
    }

    public void Start()
    {
        if (Phase != GamePhase.Briefing) return;
        Phase = GamePhase.Player;
        LogMessage("Field control established. Select a hero to mark a route.");
        StateChanged?.Invoke();
    }

    public TacticalHighlights GetHighlights()
    {
        var danger = LivingEnemies.SelectMany(enemy => Encounter.Grid.PointsInRange(enemy.Position, 3)).ToHashSet();
        var unit = SelectedUnit;
        if (unit is null) return new TacticalHighlights { Danger = danger };
        var ability = SelectedAbility;
        if (ability is not null)
            return new TacticalHighlights { Targets = ValidAbilityTargets(unit, ability).ToHashSet(), Danger = danger, Selected = unit.Position };

        var options = BuildRouteOptions(unit, RouteIntent.Safe);
        var nearestOpponent = Encounter.Units.Where(candidate => candidate.IsAlive && candidate.Faction != unit.Faction)
            .OrderBy(candidate => unit.Position.ManhattanDistance(candidate.Position)).FirstOrDefault();
        var suggested = nearestOpponent is null
            ? null
            : Encounter.Grid.FindBestApproach(unit.Position, nearestOpponent.Position, Encounter.Units, unit.Template.Movement, unit.Id, options);
        var flanks = nearestOpponent is null
            ? new HashSet<GridPoint>()
            : Encounter.Grid.FindFlankAnchors(unit.Position, nearestOpponent.Position, Encounter.Units, unit.Template.Movement, unit.Id, options);
        return new TacticalHighlights
        {
            Reachable = Encounter.Grid.Reachable(unit.Position, unit.Template.Movement, Encounter.Units, unit.Id, options).Keys.ToHashSet(),
            Danger = danger,
            Cover = Encounter.Grid.PointsInRange(unit.Position, unit.Template.Movement).Where(point => Encounter.Grid.CoverAt(point) > 0).ToHashSet(),
            FlankAnchors = flanks,
            SuggestedRoute = suggested?.Path ?? Array.Empty<GridPoint>(),
            Selected = unit.Position
        };
    }

    public bool ReserveRoute(string unitId, GridPoint destination)
    {
        if (Phase != GamePhase.Player) return false;
        var unit = LivingHeroes.SingleOrDefault(candidate => candidate.Id == unitId);
        if (unit is null) return false;
        var route = Encounter.Grid.FindTacticalRoute(unit.Position, destination, Encounter.Units, unit.Template.Movement, unit.Id, BuildRouteOptions(unit, RouteIntent.Safe));
        if (route is null) return false;
        unit.ReservedDestination = destination;
        LogMessage($"{unit.Template.Name} reserves {destination} for the squad route plan.");
        StateChanged?.Invoke();
        return true;
    }

    public bool ReserveSuggestedRoute()
    {
        var unit = SelectedUnit;
        var route = GetHighlights().SuggestedRoute;
        return unit is not null && route.Count > 0 && ReserveRoute(unit.Id, route[^1]);
    }

    public bool SelectUnit(string unitId)
    {
        if (Phase != GamePhase.Player) return false;
        var unit = Encounter.Units.SingleOrDefault(candidate => candidate.Id == unitId);
        if (unit is null || !unit.IsAlive || unit.Faction != Faction.Hero) return false;
        _selectedUnitId = unitId;
        _selectedAbilityId = null;
        LogMessage($"{unit.Template.Name} {unit.Template.Title} is ready. Mark a route or ability.");
        StateChanged?.Invoke();
        return true;
    }

    public bool SelectAbility(string abilityId)
    {
        var unit = SelectedUnit;
        if (unit is null || Phase != GamePhase.Player || !unit.Template.AbilityIds.Contains(abilityId) || !_abilities.TryGetValue(abilityId, out var ability)) return false;
        if (unit.Acted || unit.Energy < ability.EnergyCost || unit.Cooldowns.GetValueOrDefault(abilityId) > 0)
        {
            LogMessage($"{ability.Name} is not ready this turn.");
            StateChanged?.Invoke();
            return false;
        }
        _selectedAbilityId = abilityId;
        LogMessage($"{ability.Name}: mark a valid target on the field.");
        StateChanged?.Invoke();
        return true;
    }

    public bool ChooseTile(GridPoint target)
    {
        if (Phase != GamePhase.Player) return false;
        var unit = SelectedUnit;
        if (unit is null)
        {
            var hero = LivingHeroes.SingleOrDefault(candidate => candidate.Position == target);
            return hero is not null && SelectUnit(hero.Id);
        }
        return SelectedAbility is { } ability ? UseAbility(unit, ability, target) : Move(unit, target);
    }

    public bool EndTurn()
    {
        if (Phase != GamePhase.Player) return false;
        _selectedUnitId = null;
        _selectedAbilityId = null;
        _undoSnapshot = null;
        _enemyPhaseFinished = false;
        Phase = GamePhase.Enemy;
        RecordSystem("end-turn", null, "Player field turn closed.");
        LogMessage("Field turn closed. Hostiles read the board.");
        StateChanged?.Invoke();
        return true;
    }

    /// <summary>Runs exactly one alive enemy action. Call from a Godot timer until false.</summary>
    public bool ResolveNextEnemy()
    {
        if (Phase != GamePhase.Enemy || _enemyPhaseFinished) return false;
        var enemy = LivingEnemies.FirstOrDefault(unit => !unit.Acted);
        if (enemy is null)
        {
            BeginPlayerTurn();
            return false;
        }

        var target = LivingHeroes.OrderBy(hero => enemy.Position.ManhattanDistance(hero.Position)).ThenBy(hero => hero.Health).FirstOrDefault();
        if (target is null)
        {
            Finish(GamePhase.Defeat, "The field team has been forced to withdraw.");
            return false;
        }

        var ability = enemy.Template.AbilityIds.Select(id => _abilities[id]).FirstOrDefault(candidate =>
            enemy.Cooldowns.GetValueOrDefault(candidate.Id) == 0 && enemy.Energy >= candidate.EnergyCost && ValidAbilityTargets(enemy, candidate).Contains(target.Position));
        if (ability is not null)
        {
            ResolveAbility(enemy, ability, target.Position, recordAsEnemy: true);
        }
        else
        {
            var intent = enemy.Template.AiProfile switch
            {
                "ambusher" => RouteIntent.Flank,
                "controller" or "support" => RouteIntent.Safe,
                _ => TacticalClassRules.PreferredRoute(enemy.Template)
            };
            var route = Encounter.Grid.FindBestApproach(enemy.Position, target.Position, Encounter.Units, enemy.Template.Movement, enemy.Id, BuildRouteOptions(enemy, intent));
            if (route is { Path.Count: > 0 })
            {
                enemy.Position = route.Path[^1];
                ApplyTileEffect(enemy);
                LogMessage($"{enemy.Template.Name} advances along a {(intent == RouteIntent.Flank ? "flank" : "covered")} route.");
            }
            enemy.Moved = true;
            enemy.Acted = true;
            Record(enemy, "enemy", enemy.Position, null, $"{enemy.Template.Name} advances.");
        }

        CheckOutcome();
        StateChanged?.Invoke();
        return Phase == GamePhase.Enemy;
    }

    public bool Undo()
    {
        if (!CanUndo || _undoSnapshot is null) return false;
        Encounter.Units.Clear();
        Encounter.Units.AddRange(_undoSnapshot.Select(CloneUnit));
        _undoSnapshot = null;
        _selectedAbilityId = null;
        LogMessage("Last field action restored.");
        StateChanged?.Invoke();
        return true;
    }

    public ReplayRecord CreateReplay() => new(1, Encounter.Id, Encounter.Seed, Encounter.Mode, Encounter.Difficulty, DateTimeOffset.UtcNow, _actions, Phase == GamePhase.Victory ? "victory" : Phase == GamePhase.Defeat ? "defeat" : null);

    private bool Move(UnitState unit, GridPoint target)
    {
        if (unit.Moved)
        {
            LogMessage($"{unit.Template.Name} has already moved.");
            StateChanged?.Invoke();
            return false;
        }
        var route = Encounter.Grid.FindTacticalRoute(unit.Position, target, Encounter.Units, unit.Template.Movement, unit.Id, BuildRouteOptions(unit, RouteIntent.Safe));
        if (route is not { Path.Count: > 0 })
        {
            LogMessage("That route is blocked or exceeds the movement allowance.");
            StateChanged?.Invoke();
            return false;
        }
        CaptureUndo();
        unit.Position = route.Path[^1];
        unit.ReservedDestination = null;
        unit.Moved = true;
        ApplyTileEffect(unit);
        Record(unit, "move", unit.Position, null, $"{unit.Template.Name} marked a route.");
        LogMessage($"{unit.Template.Name} moved {route.Path.Count} tile{(route.Path.Count == 1 ? string.Empty : "s")}; tactical cost {route.TacticalCost}.");
        CheckOutcome();
        StateChanged?.Invoke();
        return true;
    }

    private bool UseAbility(UnitState unit, AbilityDefinition ability, GridPoint target)
    {
        if (!ValidAbilityTargets(unit, ability).Contains(target))
        {
            LogMessage("The field cannot support that target.");
            StateChanged?.Invoke();
            return false;
        }
        CaptureUndo();
        ResolveAbility(unit, ability, target, recordAsEnemy: false);
        CheckOutcome();
        StateChanged?.Invoke();
        return true;
    }

    private void ResolveAbility(UnitState unit, AbilityDefinition ability, GridPoint target, bool recordAsEnemy)
    {
        unit.Energy -= ability.EnergyCost;
        unit.Cooldowns[ability.Id] = ability.Cooldown;
        unit.Acted = true;

        if (ability.Kind == AbilityKind.Damage)
        {
            var targets = ability.Shape switch
            {
                AbilityShape.Area => Encounter.Units.Where(candidate => candidate.IsAlive && candidate.Faction != unit.Faction && candidate.Position.ManhattanDistance(target) <= ability.Radius),
                AbilityShape.Line => Encounter.Units.Where(candidate => candidate.IsAlive && candidate.Faction != unit.Faction && IsOnLine(unit.Position, target, candidate.Position)),
                _ => Encounter.Units.Where(candidate => candidate.IsAlive && candidate.Faction != unit.Faction && candidate.Position == target)
            };
            foreach (var affected in targets.ToList()) Damage(unit, affected, ability);
        }
        else if (ability.Kind == AbilityKind.Heal)
        {
            var ally = Encounter.Units.SingleOrDefault(candidate => candidate.IsAlive && candidate.Position == target && candidate.Faction == unit.Faction);
            if (ally is not null)
            {
                var restored = Math.Min(ability.Power + TacticalClassRules.HealingBonus(unit), ally.Template.MaxHealth - ally.Health);
                ally.Health += restored;
                LogMessage($"{ally.Template.Name} restores {restored} health.");
            }
        }
        else if (ability.Kind == AbilityKind.Shield)
        {
            var ally = Encounter.Units.SingleOrDefault(candidate => candidate.IsAlive && candidate.Position == target && candidate.Faction == unit.Faction);
            if (ally is not null)
            {
                ally.Shield += ability.Power;
                LogMessage($"{ally.Template.Name} gains a {ability.Power}-point ward.");
            }
        }
        else if (ability.Kind == AbilityKind.Teleport)
        {
            unit.Position = target;
            unit.Moved = true;
            ApplyTileEffect(unit);
            LogMessage($"{unit.Template.Name} takes a marked field step.");
        }
        Record(unit, recordAsEnemy ? "enemy" : "ability", target, ability.Id, $"{unit.Template.Name} used {ability.Name}.");
        _selectedAbilityId = null;
    }

    private IReadOnlySet<GridPoint> ValidAbilityTargets(UnitState unit, AbilityDefinition ability)
    {
        var available = Encounter.Grid.PointsInRange(unit.Position, ability.Range);
        if (ability.Kind == AbilityKind.Teleport)
            return available.Where(point => Encounter.Grid.IsWalkable(point) && !Encounter.Grid.IsOccupied(point, Encounter.Units, unit.Id)).ToHashSet();

        var intendedFaction = ability.Kind is AbilityKind.Heal or AbilityKind.Shield ? unit.Faction : unit.Faction == Faction.Hero ? Faction.Enemy : Faction.Hero;
        return available.Where(point =>
        {
            var target = Encounter.Units.SingleOrDefault(candidate => candidate.IsAlive && candidate.Position == point && candidate.Faction == intendedFaction);
            return target is not null && (ability.Shape != AbilityShape.Line || Encounter.Grid.HasLineOfSight(unit.Position, point));
        }).ToHashSet();
    }

    private bool IsOnLine(GridPoint source, GridPoint destination, GridPoint candidate)
    {
        var deltaX = destination.X - source.X;
        var deltaY = destination.Y - source.Y;
        var aligned = deltaX == 0 || deltaY == 0 || Math.Abs(deltaX) == Math.Abs(deltaY);
        if (!aligned || !Encounter.Grid.HasLineOfSight(source, destination)) return false;
        var candidateX = candidate.X - source.X;
        var candidateY = candidate.Y - source.Y;
        var sameDirection = (deltaX == 0 && candidateX == 0 && Math.Sign(candidateY) == Math.Sign(deltaY)) ||
                            (deltaY == 0 && candidateY == 0 && Math.Sign(candidateX) == Math.Sign(deltaX)) ||
                            (Math.Abs(candidateX) == Math.Abs(candidateY) && Math.Sign(candidateX) == Math.Sign(deltaX) && Math.Sign(candidateY) == Math.Sign(deltaY));
        return sameDirection && source.ManhattanDistance(candidate) <= source.ManhattanDistance(destination);
    }

    private void Damage(UnitState attacker, UnitState target, AbilityDefinition ability)
    {
        var amount = Math.Max(1, attacker.Template.Attack + ability.Power + TacticalClassRules.DamageBonus(attacker, target, ability) - target.Template.Defense);
        if (ability.Element == ElementKind.Fire && target.Statuses.ContainsKey("rooted")) amount += 2;
        if (ability.Element == ElementKind.Storm && target.Statuses.ContainsKey("chilled")) target.Statuses["stagger"] = 1;
        var absorbed = Math.Min(target.Shield, amount);
        target.Shield -= absorbed;
        target.Health = Math.Max(0, target.Health - amount + absorbed);
        if (!string.IsNullOrWhiteSpace(ability.Status)) target.Statuses[ability.Status] = 1;
        LogMessage($"{attacker.Template.Name} uses {ability.Name}; {target.Template.Name} takes {amount - absorbed} impact.");
    }

    private void ApplyTileEffect(UnitState unit)
    {
        var tile = Encounter.Grid.Get(unit.Position);
        if (tile is null) return;
        if (tile.Kind == TileKind.Hazard)
        {
            unit.Health = Math.Max(0, unit.Health - 2);
            LogMessage($"{unit.Template.Name} crosses a hazard and takes 2 impact.");
        }
        if (tile.Kind == TileKind.Healing)
        {
            var restored = Math.Min(3, unit.Template.MaxHealth - unit.Health);
            unit.Health += restored;
            LogMessage($"{unit.Template.Name} recovers {restored} health at a healing marker.");
        }
        if (tile.Kind == TileKind.Teleport && tile.LinkedTo is { } linked && !Encounter.Grid.IsOccupied(linked, Encounter.Units, unit.Id))
        {
            unit.Position = linked;
            LogMessage($"{unit.Template.Name} follows a linked teleport marker.");
        }
    }

    private void BeginPlayerTurn()
    {
        Turn++;
        foreach (var unit in Encounter.Units.Where(unit => unit.IsAlive))
        {
            unit.Moved = false;
            unit.Acted = false;
            unit.Energy = Math.Min(unit.Template.Energy, unit.Energy + 1);
            foreach (var id in unit.Cooldowns.Keys.ToList()) unit.Cooldowns[id] = Math.Max(0, unit.Cooldowns[id] - 1);
            foreach (var id in unit.Statuses.Keys.ToList()) unit.Statuses[id] = Math.Max(0, unit.Statuses[id] - 1);
            unit.ReservedDestination = null;
        }
        Phase = GamePhase.Player;
        _enemyPhaseFinished = true;
        if (Encounter.TurnLimit is { } limit && Turn > limit)
        {
            Finish(GamePhase.Defeat, "The puzzle field shifted before the objective was complete.");
            return;
        }
        LogMessage($"Turn {Turn}: field control returned.");
        StateChanged?.Invoke();
    }

    private void CheckOutcome()
    {
        if (!LivingEnemies.Any()) Finish(GamePhase.Victory, "The field is secure. Record the route.");
        if (!LivingHeroes.Any()) Finish(GamePhase.Defeat, "The field team has been forced to withdraw.");
    }

    private void Finish(GamePhase result, string message)
    {
        if (Phase is GamePhase.Victory or GamePhase.Defeat) return;
        Phase = result;
        _enemyPhaseFinished = true;
        LogMessage(message);
        StateChanged?.Invoke();
    }

    private void CaptureUndo() => _undoSnapshot ??= Encounter.Units.Select(CloneUnit).ToList();
    private void Record(UnitState unit, string type, GridPoint? target, string? abilityId, string note) => _actions.Add(new TacticalAction(Turn, unit.Id, type, target, abilityId, note));
    private void RecordSystem(string type, GridPoint? target, string note) => _actions.Add(new TacticalAction(Turn, "system", type, target, null, note));
    private void LogMessage(string message) { _log.Insert(0, message); if (_log.Count > 5) _log.RemoveAt(_log.Count - 1); MessageLogged?.Invoke(message); }

    private static UnitState CloneUnit(UnitState source)
    {
        var copy = new UnitState { Id = source.Id, Template = source.Template, Position = source.Position, Health = source.Health, Energy = source.Energy, Shield = source.Shield, Moved = source.Moved, Acted = source.Acted, ReservedDestination = source.ReservedDestination };
        foreach (var (id, value) in source.Cooldowns) copy.Cooldowns[id] = value;
        foreach (var (id, value) in source.Statuses) copy.Statuses[id] = value;
        return copy;
    }

    private RouteOptions BuildRouteOptions(UnitState unit, RouteIntent intent)
    {
        var hostileThreat = Encounter.Units.Where(candidate => candidate.IsAlive && candidate.Faction != unit.Faction)
            .SelectMany(candidate => Encounter.Grid.PointsInRange(candidate.Position, candidate.Template.Movement + 1)).ToHashSet();
        return new RouteOptions
        {
            Mobility = unit.Template.Mobility,
            Intent = intent,
            ThreatenedTiles = hostileThreat,
            Reservations = Encounter.Grid.BuildReservations(Encounter.Units),
            ReservationOwnerId = unit.Id,
            ThreatPenalty = TacticalClassRules.ThreatPenalty(unit.Template),
            CoverReward = TacticalClassRules.CoverReward(unit.Template)
        };
    }
}
