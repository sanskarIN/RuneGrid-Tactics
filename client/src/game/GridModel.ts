/** Runic Field Manual design system: all route markings derive from deterministic tactical rules. */

import type { Point, Tile, TileKind, Unit } from "./types";

interface QueueNode {
  point: Point;
  cost: number;
  score: number;
}

const key = (point: Point) => `${point.x}:${point.y}`;

export class GridModel {
  public readonly width: number;
  public readonly height: number;

  public constructor(public readonly tiles: Tile[][]) {
    this.height = tiles.length;
    this.width = tiles[0]?.length ?? 0;
  }

  public inBounds(point: Point): boolean {
    return (
      point.x >= 0 &&
      point.y >= 0 &&
      point.x < this.width &&
      point.y < this.height
    );
  }

  public get(point: Point): Tile | undefined {
    return this.inBounds(point) ? this.tiles[point.y][point.x] : undefined;
  }

  public neighbors(point: Point): Point[] {
    return [
      { x: point.x + 1, y: point.y },
      { x: point.x - 1, y: point.y },
      { x: point.x, y: point.y + 1 },
      { x: point.x, y: point.y - 1 },
    ].filter(candidate => this.inBounds(candidate));
  }

  public static distance(a: Point, b: Point): number {
    return Math.abs(a.x - b.x) + Math.abs(a.y - b.y);
  }

  public isWalkable(point: Point): boolean {
    const tile = this.get(point);
    return Boolean(tile && !["wall", "gate"].includes(tile.kind));
  }

  public movementCost(point: Point): number {
    const kind = this.get(point)?.kind;
    if (kind === "difficult") return 2;
    if (kind === "wall" || kind === "gate") return Number.POSITIVE_INFINITY;
    return 1;
  }

  public occupied(point: Point, units: Unit[], excludeId?: string): boolean {
    return units.some(
      unit =>
        unit.hp > 0 &&
        unit.id !== excludeId &&
        unit.x === point.x &&
        unit.y === point.y
    );
  }

  public reachable(
    start: Point,
    allowance: number,
    units: Unit[],
    excludeId?: string
  ): Map<string, { point: Point; cost: number }> {
    const frontier: QueueNode[] = [{ point: start, cost: 0, score: 0 }];
    const best = new Map<string, { point: Point; cost: number }>([
      [key(start), { point: start, cost: 0 }],
    ]);
    while (frontier.length) {
      frontier.sort((a, b) => a.cost - b.cost);
      const current = frontier.shift()!;
      for (const next of this.neighbors(current.point)) {
        if (!this.isWalkable(next) || this.occupied(next, units, excludeId))
          continue;
        const cost = current.cost + this.movementCost(next);
        if (cost > allowance) continue;
        const known = best.get(key(next));
        if (!known || cost < known.cost) {
          best.set(key(next), { point: next, cost });
          frontier.push({ point: next, cost, score: cost });
        }
      }
    }
    best.delete(key(start));
    return best;
  }

  public pathfind(
    start: Point,
    goal: Point,
    units: Unit[],
    allowance = Number.POSITIVE_INFINITY,
    excludeId?: string
  ): Point[] | null {
    if (
      !this.inBounds(goal) ||
      !this.isWalkable(goal) ||
      this.occupied(goal, units, excludeId)
    )
      return null;
    const open: QueueNode[] = [
      { point: start, cost: 0, score: GridModel.distance(start, goal) },
    ];
    const cameFrom = new Map<string, Point>();
    const costs = new Map<string, number>([[key(start), 0]]);
    while (open.length) {
      open.sort((a, b) => a.score - b.score);
      const current = open.shift()!;
      if (current.point.x === goal.x && current.point.y === goal.y) {
        const path: Point[] = [];
        let cursor = goal;
        while (key(cursor) !== key(start)) {
          path.unshift(cursor);
          cursor = cameFrom.get(key(cursor))!;
        }
        return current.cost <= allowance ? path : null;
      }
      for (const next of this.neighbors(current.point)) {
        if (!this.isWalkable(next) || this.occupied(next, units, excludeId))
          continue;
        const nextCost = current.cost + this.movementCost(next);
        if (nextCost > allowance) continue;
        if (nextCost < (costs.get(key(next)) ?? Number.POSITIVE_INFINITY)) {
          costs.set(key(next), nextCost);
          cameFrom.set(key(next), current.point);
          open.push({
            point: next,
            cost: nextCost,
            score: nextCost + GridModel.distance(next, goal),
          });
        }
      }
    }
    return null;
  }

  public hasLineOfSight(start: Point, end: Point): boolean {
    const dx = Math.abs(end.x - start.x);
    const dy = Math.abs(end.y - start.y);
    const sx = start.x < end.x ? 1 : -1;
    const sy = start.y < end.y ? 1 : -1;
    let error = dx - dy;
    let x = start.x;
    let y = start.y;
    while (!(x === end.x && y === end.y)) {
      if (!(x === start.x && y === start.y)) {
        const tile = this.get({ x, y });
        if (tile && ["wall", "gate"].includes(tile.kind)) return false;
      }
      const doubleError = error * 2;
      if (doubleError > -dy) {
        error -= dy;
        x += sx;
      }
      if (doubleError < dx) {
        error += dx;
        y += sy;
      }
    }
    return true;
  }

  public pointsInRange(origin: Point, range: number, kind?: TileKind): Point[] {
    const result: Point[] = [];
    for (let y = 0; y < this.height; y += 1) {
      for (let x = 0; x < this.width; x += 1) {
        const point = { x, y };
        if (
          GridModel.distance(origin, point) <= range &&
          (!kind || this.get(point)?.kind === kind)
        )
          result.push(point);
      }
    }
    return result;
  }
}
