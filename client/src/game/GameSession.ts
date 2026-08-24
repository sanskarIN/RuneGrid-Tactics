/** Runic Field Manual design system: GameSession owns legal tactics; interface layers only request field actions. */

import { ABILITIES } from "./content";
import { GridModel } from "./GridModel";
import { ReplayRecorder } from "./Replay";
import { SeededRng } from "./rng";
import type {
  AbilityDefinition,
  Encounter,
  GameAction,
  GamePhase,
  Point,
  SessionEvent,
  TileHighlights,
  Unit,
} from "./types";

const clone = <T>(value: T): T => JSON.parse(JSON.stringify(value)) as T;

export class GameSession {
  public readonly grid: GridModel;
  public phase: GamePhase = "briefing";
  public selectedUnitId?: string;
  public selectedAbilityId?: string;
  public turn = 1;
  public logs: string[] = [];
  public readonly replay: ReplayRecorder;
  private readonly listeners = new Set<(event: SessionEvent) => void>();
  private readonly rng: SeededRng;
  private undoSnapshot?: Unit[];
  private enemyTimer?: ReturnType<typeof setTimeout>;
  private recordedOutcome = false;

  public constructor(public readonly encounter: Encounter) {
    this.grid = new GridModel(encounter.grid);
    this.rng = new SeededRng(`${encounter.seed}:combat`);
    this.replay = new ReplayRecorder(encounter);
    this.log(`${encounter.title}: ${encounter.objective}`);
  }

  public get units(): Unit[] {
    return this.encounter.units;
  }
  public get selectedUnit(): Unit | undefined {
    return this.units.find(unit => unit.id === this.selectedUnitId);
  }
  public get selectedAbility(): AbilityDefinition | undefined {
    return this.selectedAbilityId
      ? ABILITIES[this.selectedAbilityId]
      : undefined;
  }
  public get livingHeroes(): Unit[] {
    return this.units.filter(unit => unit.faction === "hero" && unit.hp > 0);
  }
  public get livingEnemies(): Unit[] {
    return this.units.filter(unit => unit.faction === "enemy" && unit.hp > 0);
  }
  public get hasUndo(): boolean {
    return Boolean(this.undoSnapshot && this.phase === "player");
  }

  public subscribe(listener: (event: SessionEvent) => void): () => void {
    this.listeners.add(listener);
    return () => this.listeners.delete(listener);
  }

  public start(): void {
    if (this.phase !== "briefing") return;
    this.phase = "player";
    this.log("Field control established. Select a hero to mark a route.");
    this.emit({ type: "state" });
  }

  public getHighlights(): TileHighlights {
    const selected = this.selectedUnit;
    if (!selected)
      return { reachable: [], targets: [], danger: this.dangerZones() };
    if (this.selectedAbility) {
      return {
        reachable: [],
        targets: this.validAbilityTargets(selected, this.selectedAbility),
        danger: this.dangerZones(),
        selected: selected,
      };
    }
    return {
      reachable: Array.from(
        this.grid
          .reachable(selected, selected.movement, this.units, selected.id)
          .values()
      ).map(entry => entry.point),
      targets: [],
      danger: this.dangerZones(),
      selected,
    };
  }

  public selectUnit(id: string): void {
    const unit = this.units.find(candidate => candidate.id === id);
    if (
      !unit ||
      unit.faction !== "hero" ||
      unit.hp <= 0 ||
      this.phase !== "player"
    )
      return;
    this.selectedUnitId = id;
    this.selectedAbilityId = undefined;
    this.log(`${unit.name} ${unit.title} is ready. Mark a route or ability.`);
    this.emit({ type: "state" });
  }

  public selectAbility(id?: string): void {
    const unit = this.selectedUnit;
    if (
      !unit ||
      !id ||
      !unit.abilityIds.includes(id) ||
      this.phase !== "player"
    )
      return;
    const ability = ABILITIES[id];
    if (
      (unit.cooldowns[id] ?? 0) > 0 ||
      unit.energy < ability.energyCost ||
      unit.acted
    ) {
      this.log(`${ability.name} is not ready this turn.`);
      this.emit({ type: "state" });
      return;
    }
    this.selectedAbilityId = id;
    this.log(`${ability.name}: mark a valid target on the field.`);
    this.emit({ type: "state" });
  }

  public chooseTile(point: Point): void {
    if (this.phase !== "player") return;
    const unit = this.selectedUnit;
    if (!unit) {
      const target = this.units.find(
        candidate =>
          candidate.x === point.x &&
          candidate.y === point.y &&
          candidate.faction === "hero"
      );
      if (target) this.selectUnit(target.id);
      return;
    }
    if (this.selectedAbility) {
      this.useAbility(unit, this.selectedAbility, point);
    } else {
      this.move(unit, point);
    }
  }

  public endTurn(): void {
    if (this.phase !== "player") return;
    this.selectedUnitId = undefined;
    this.selectedAbilityId = undefined;
    this.undoSnapshot = undefined;
    this.phase = "enemy";
    this.log("Field turn closed. Hostiles read the board.");
    this.emit({ type: "state" });
    this.enemyTimer = globalThis.setTimeout(() => this.runNextEnemy(), 240);
  }

  public undo(): void {
    if (!this.undoSnapshot || this.phase !== "player") return;
    this.encounter.units = clone(this.undoSnapshot);
    this.selectedAbilityId = undefined;
    this.log("Last field action restored.");
    this.undoSnapshot = undefined;
    this.emit({ type: "state" });
  }

  public dispose(): void {
    if (this.enemyTimer) globalThis.clearTimeout(this.enemyTimer);
    this.listeners.clear();
  }

  public createReplay(): ReturnType<ReplayRecorder["create"]> {
    return this.replay.create(
      this.phase === "victory"
        ? "victory"
        : this.phase === "defeat"
          ? "defeat"
          : undefined
    );
  }

  private move(unit: Unit, target: Point): void {
    if (unit.moved) {
      this.log(`${unit.name} has already moved.`);
      this.emit({ type: "state" });
      return;
    }
    const path = this.grid.pathfind(
      unit,
      target,
      this.units,
      unit.movement,
      unit.id
    );
    if (!path?.length) {
      this.log("That route is blocked or exceeds the movement allowance.");
      this.emit({ type: "state" });
      return;
    }
    this.captureUndo();
    const destination = path[path.length - 1];
    unit.x = destination.x;
    unit.y = destination.y;
    unit.moved = true;
    this.applyTileEffect(unit);
    this.record({
      actorId: unit.id,
      type: "move",
      target: destination,
      note: `${unit.name} marked a route.`,
    });
    this.log(
      `${unit.name} moved ${path.length} tile${path.length === 1 ? "" : "s"}.`
    );
    this.checkOutcome();
    this.emit({ type: "unit" });
  }

  private useAbility(
    unit: Unit,
    ability: AbilityDefinition,
    target: Point
  ): void {
    if (
      !this.validAbilityTargets(unit, ability).some(
        candidate => candidate.x === target.x && candidate.y === target.y
      )
    ) {
      this.log("The field cannot support that target.");
      this.emit({ type: "state" });
      return;
    }
    this.captureUndo();
    unit.energy -= ability.energyCost;
    unit.cooldowns[ability.id] = ability.cooldown;
    unit.acted = true;
    const impacted =
      ability.shape === "area"
        ? this.units.filter(
            candidate =>
              candidate.hp > 0 &&
              GridModel.distance(candidate, target) <= (ability.radius ?? 1) &&
              candidate.faction !== unit.faction
          )
        : this.units.filter(
            candidate =>
              candidate.hp > 0 &&
              candidate.x === target.x &&
              candidate.y === target.y
          );
    if (ability.kind === "damage") {
      const targets =
        ability.shape === "line"
          ? this.units.filter(
              candidate =>
                candidate.hp > 0 &&
                candidate.faction !== unit.faction &&
                this.onLine(unit, target, candidate)
            )
          : impacted;
      if (!targets.length) {
        this.log("The rune finds no hostile target.");
        this.emit({ type: "state" });
        return;
      }
      for (const enemy of targets) this.damage(unit, enemy, ability);
    } else if (ability.kind === "heal") {
      const ally = this.units.find(
        candidate => candidate.x === target.x && candidate.y === target.y
      );
      if (ally) {
        const restored = Math.min(ability.power ?? 0, ally.maxHp - ally.hp);
        ally.hp += restored;
        this.log(`${ally.name} restores ${restored} health.`);
      }
    } else if (ability.kind === "shield") {
      const ally = this.units.find(
        candidate => candidate.x === target.x && candidate.y === target.y
      );
      if (ally) {
        ally.shield += ability.power ?? 0;
        this.log(`${ally.name} gains a ${ability.power} point ward.`);
      }
    } else if (ability.kind === "teleport") {
      unit.x = target.x;
      unit.y = target.y;
      unit.moved = true;
      this.applyTileEffect(unit);
      this.log(`${unit.name} takes a marked field step.`);
    }
    this.record({
      actorId: unit.id,
      type: "ability",
      target,
      abilityId: ability.id,
      note: `${unit.name} used ${ability.name}.`,
    });
    this.selectedAbilityId = undefined;
    this.checkOutcome();
    this.emit({ type: "unit" });
  }

  private validAbilityTargets(unit: Unit, ability: AbilityDefinition): Point[] {
    const allTiles = this.grid.pointsInRange(unit, ability.range);
    if (ability.kind === "teleport")
      return allTiles.filter(
        point =>
          this.grid.isWalkable(point) &&
          !this.grid.occupied(point, this.units, unit.id)
      );
    const faction =
      ability.kind === "heal" || ability.kind === "shield"
        ? unit.faction
        : unit.faction === "hero"
          ? "enemy"
          : "hero";
    return allTiles.filter(point => {
      const target = this.units.find(
        candidate =>
          candidate.x === point.x &&
          candidate.y === point.y &&
          candidate.hp > 0 &&
          candidate.faction === faction
      );
      return Boolean(
        target &&
          (ability.shape !== "line" || this.grid.hasLineOfSight(unit, point))
      );
    });
  }

  private onLine(unit: Unit, target: Point, candidate: Unit): boolean {
    const dx = target.x - unit.x;
    const dy = target.y - unit.y;
    const aligned = dx === 0 || dy === 0 || Math.abs(dx) === Math.abs(dy);
    if (!aligned || !this.grid.hasLineOfSight(unit, target)) return false;
    const cdx = candidate.x - unit.x;
    const cdy = candidate.y - unit.y;
    const sameAxis =
      (dx === 0 && cdx === 0 && Math.sign(cdy) === Math.sign(dy)) ||
      (dy === 0 && cdy === 0 && Math.sign(cdx) === Math.sign(dx)) ||
      (Math.abs(cdx) === Math.abs(cdy) &&
        Math.sign(cdx) === Math.sign(dx) &&
        Math.sign(cdy) === Math.sign(dy));
    return (
      sameAxis &&
      GridModel.distance(unit, candidate) <= GridModel.distance(unit, target)
    );
  }

  private damage(
    attacker: Unit,
    target: Unit,
    ability: AbilityDefinition
  ): void {
    let amount = Math.max(
      1,
      attacker.attack + (ability.power ?? 0) - target.defense
    );
    if (ability.element === "fire" && target.statuses.rooted) amount += 2;
    if (ability.element === "storm" && target.statuses.chilled)
      target.statuses.stagger = 1;
    const absorbed = Math.min(target.shield, amount);
    target.shield -= absorbed;
    target.hp = Math.max(0, target.hp - (amount - absorbed));
    if (ability.status) target.statuses[ability.status] = 1;
    if (attacker.faction === "hero")
      this.log(
        `${attacker.name} uses ${ability.name}; ${target.name} takes ${amount - absorbed} impact.`
      );
    else this.log(`${target.name} is struck for ${amount - absorbed}.`);
  }

  private applyTileEffect(unit: Unit): void {
    const tile = this.grid.get(unit);
    if (!tile) return;
    if (tile.kind === "hazard") {
      unit.hp = Math.max(0, unit.hp - 2);
      this.log(`${unit.name} crosses a hazard and takes 2 impact.`);
    }
    if (tile.kind === "healing") {
      const restored = Math.min(3, unit.maxHp - unit.hp);
      unit.hp += restored;
      this.log(`${unit.name} recovers ${restored} health at a healing marker.`);
    }
    if (
      tile.kind === "teleport" &&
      tile.linkedTo &&
      !this.grid.occupied(tile.linkedTo, this.units, unit.id)
    ) {
      unit.x = tile.linkedTo.x;
      unit.y = tile.linkedTo.y;
      this.log(`${unit.name} follows a linked teleport marker.`);
    }
  }

  private runNextEnemy(): void {
    if (this.phase !== "enemy") return;
    const enemy = this.livingEnemies.find(candidate => !candidate.acted);
    if (!enemy) {
      this.beginPlayerTurn();
      return;
    }
    const heroTargets = this.livingHeroes;
    const target = heroTargets.sort(
      (a, b) =>
        GridModel.distance(enemy, a) - GridModel.distance(enemy, b) ||
        a.hp - b.hp
    )[0];
    const ability = enemy.abilityIds
      .map(id => ABILITIES[id])
      .find(
        candidate =>
          (enemy.cooldowns[candidate.id] ?? 0) === 0 &&
          enemy.energy >= candidate.energyCost &&
          this.validAbilityTargets(enemy, candidate).some(
            point => point.x === target.x && point.y === target.y
          )
      );
    if (ability) {
      const originalFaction = enemy.faction;
      this.useEnemyAbility(enemy, ability, target);
      enemy.faction = originalFaction;
    } else {
      const path = this.grid.pathfind(
        enemy,
        target,
        this.units,
        enemy.movement,
        enemy.id
      );
      if (path?.length) {
        const spot = path[Math.min(path.length, enemy.movement) - 1];
        enemy.x = spot.x;
        enemy.y = spot.y;
        this.applyTileEffect(enemy);
        this.log(`${enemy.name} advances through the field.`);
      }
      enemy.acted = true;
      enemy.moved = true;
      this.record({
        actorId: enemy.id,
        type: "enemy",
        target: { x: enemy.x, y: enemy.y },
        note: `${enemy.name} advances.`,
      });
    }
    this.checkOutcome();
    this.emit({ type: "unit" });
    if (this.phase === "enemy")
      this.enemyTimer = globalThis.setTimeout(() => this.runNextEnemy(), 420);
  }

  private useEnemyAbility(
    enemy: Unit,
    ability: AbilityDefinition,
    target: Unit
  ): void {
    enemy.energy -= ability.energyCost;
    enemy.cooldowns[ability.id] = ability.cooldown;
    enemy.acted = true;
    if (ability.kind === "damage") this.damage(enemy, target, ability);
    else if (ability.kind === "heal" || ability.kind === "shield") {
      enemy.shield += ability.power ?? 0;
      this.log(`${enemy.name} reinforces its field position.`);
    }
    this.record({
      actorId: enemy.id,
      type: "enemy",
      target,
      abilityId: ability.id,
      note: `${enemy.name} used ${ability.name}.`,
    });
  }

  private beginPlayerTurn(): void {
    this.turn += 1;
    for (const unit of this.units.filter(candidate => candidate.hp > 0)) {
      unit.moved = false;
      unit.acted = false;
      unit.energy = Math.min(unit.maxEnergy, unit.energy + 1);
      Object.keys(unit.cooldowns).forEach(id => {
        unit.cooldowns[id] = Math.max(0, unit.cooldowns[id] - 1);
      });
      Object.keys(unit.statuses).forEach(id => {
        unit.statuses[id] = Math.max(0, unit.statuses[id] - 1);
      });
    }
    this.phase = "player";
    this.log(`Turn ${this.turn}: field control returned.`);
    if (this.encounter.turnLimit && this.turn > this.encounter.turnLimit)
      this.finish(
        "defeat",
        "The puzzle field shifted before the objective was complete."
      );
    this.emit({ type: "state" });
  }

  private dangerZones(): Point[] {
    return this.livingEnemies.flatMap(enemy =>
      this.grid.pointsInRange(
        enemy,
        Math.max(1, enemy.abilityIds.length ? 3 : 1)
      )
    );
  }

  private checkOutcome(): void {
    if (!this.livingEnemies.length)
      this.finish("victory", "The field is secure. Record the route.");
    if (!this.livingHeroes.length)
      this.finish("defeat", "The field team has been forced to withdraw.");
  }

  private finish(outcome: "victory" | "defeat", message: string): void {
    if (this.recordedOutcome) return;
    this.recordedOutcome = true;
    if (this.enemyTimer) globalThis.clearTimeout(this.enemyTimer);
    this.phase = outcome;
    this.log(message);
    this.emit({ type: "outcome", message });
  }

  private captureUndo(): void {
    if (!this.undoSnapshot) this.undoSnapshot = clone(this.units);
  }

  private record(action: Omit<GameAction, "turn">): void {
    this.replay.add({ ...action, turn: this.turn });
  }

  private log(message: string): void {
    this.logs.unshift(message);
    this.logs = this.logs.slice(0, 5);
    this.emit({ type: "log", message });
  }

  private emit(event: SessionEvent): void {
    this.listeners.forEach(listener => listener(event));
  }
}
