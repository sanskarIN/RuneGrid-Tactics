import { describe, expect, it } from "vitest";
import { GridModel } from "../GridModel";
import type { Tile } from "../types";

const grid = (kinds: Tile["kind"][][]): Tile[][] =>
  kinds.map((row, y) => row.map((kind, x) => ({ x, y, kind, elevation: 0 })));

describe("GridModel", () => {
  it("finds a route around a blocking wall", () => {
    const model = new GridModel(
      grid([
        ["floor", "wall", "floor"],
        ["floor", "floor", "floor"],
        ["floor", "floor", "floor"],
      ])
    );
    const path = model.pathfind({ x: 0, y: 0 }, { x: 2, y: 0 }, []);
    expect(path).toEqual([
      { x: 0, y: 1 },
      { x: 1, y: 1 },
      { x: 2, y: 1 },
      { x: 2, y: 0 },
    ]);
  });

  it("counts difficult terrain as two movement marks", () => {
    const model = new GridModel(grid([["floor", "difficult", "floor"]]));
    expect(model.pathfind({ x: 0, y: 0 }, { x: 2, y: 0 }, [], 2)).toBeNull();
    expect(model.pathfind({ x: 0, y: 0 }, { x: 2, y: 0 }, [], 3)).toEqual([
      { x: 1, y: 0 },
      { x: 2, y: 0 },
    ]);
  });

  it("rejects line of sight through a wall", () => {
    const model = new GridModel(grid([["floor", "wall", "floor"]]));
    expect(model.hasLineOfSight({ x: 0, y: 0 }, { x: 2, y: 0 })).toBe(false);
  });
});
