/** Runic Field Manual design system: replay records are concise, deterministic field logs. */

import type { Encounter, GameAction, ReplayData } from "./types";

export class ReplayRecorder {
  private actions: GameAction[] = [];

  public constructor(private readonly encounter: Encounter) {}

  public add(action: GameAction): void {
    this.actions.push({
      ...action,
      target: action.target ? { ...action.target } : undefined,
    });
  }

  public create(outcome?: "victory" | "defeat"): ReplayData {
    return {
      schemaVersion: 1,
      encounter: {
        id: this.encounter.id,
        mode: this.encounter.mode,
        seed: this.encounter.seed,
        version: this.encounter.version,
        difficulty: this.encounter.difficulty,
      },
      createdAt: new Date().toISOString(),
      actions: this.actions,
      outcome,
    };
  }
}
