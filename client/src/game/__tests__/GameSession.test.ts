import { afterEach, describe, expect, it, vi } from "vitest";
import { createEncounter } from "../content";
import { GameSession } from "../GameSession";

describe("GameSession", () => {
  afterEach(() => vi.useRealTimers());

  it("resolves a valid selected ability against an enemy", () => {
    const session = new GameSession(createEncounter("DUEL", "training"));
    const vanguard = session.units.find(unit => unit.id === "hero-vanguard")!;
    const enemy = session.livingEnemies[0];
    enemy.x = vanguard.x + 1;
    enemy.y = vanguard.y;
    session.start();
    session.selectUnit(vanguard.id);
    session.selectAbility("shield_bash");
    const before = enemy.hp;
    session.chooseTile(enemy);
    expect(enemy.hp).toBeLessThan(before);
    expect(vanguard.acted).toBe(true);
    expect(session.createReplay().actions.at(-1)?.abilityId).toBe(
      "shield_bash"
    );
  });

  it("returns control to the player after deterministic enemy resolution", () => {
    vi.useFakeTimers();
    const session = new GameSession(createEncounter("ENEMY-LOOP", "training"));
    session.start();
    session.endTurn();
    vi.runAllTimers();
    expect(["player", "victory", "defeat"]).toContain(session.phase);
  });

  it("restores a move when undo is permitted", () => {
    const session = new GameSession(createEncounter("UNDO", "training"));
    const ranger = session.units.find(unit => unit.id === "hero-ranger")!;
    session.start();
    session.selectUnit(ranger.id);
    const target = Array.from(session.getHighlights().reachable)[0];
    session.chooseTile(target);
    expect(session.hasUndo).toBe(true);
    session.undo();
    const restoredRanger = session.units.find(unit => unit.id === ranger.id)!;
    expect(restoredRanger.x).toBe(2);
    expect(restoredRanger.y).toBe(session.encounter.grid.length - 1);
  });
});
