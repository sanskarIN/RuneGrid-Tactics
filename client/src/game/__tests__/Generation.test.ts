import { describe, expect, it } from "vitest";
import { createEncounter, GAME_VERSION } from "../content";

describe("seeded encounter generation", () => {
  it("recreates the same tactical field from the same compatible seed", () => {
    const first = createEncounter("FIELD-7A", "expedition", "veteran");
    const second = createEncounter("FIELD-7A", "expedition", "veteran");
    expect(second.version).toBe(GAME_VERSION);
    expect(second.grid).toEqual(first.grid);
    expect(second.units.map(unit => [unit.templateId, unit.x, unit.y])).toEqual(
      first.units.map(unit => [unit.templateId, unit.x, unit.y])
    );
  });

  it("changes the field when the seed changes", () => {
    const first = createEncounter("FIELD-7A", "expedition");
    const second = createEncounter("FIELD-7B", "expedition");
    expect(JSON.stringify(second.grid)).not.toBe(JSON.stringify(first.grid));
  });
});
