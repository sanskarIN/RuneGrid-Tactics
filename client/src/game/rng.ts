/** Runic Field Manual design system: repeatable field conditions come from a visible, shareable seed. */

export class SeededRng {
  private state: number;

  public constructor(seed: string) {
    this.state = SeededRng.hash(seed);
  }

  public next(): number {
    let value = (this.state += 0x6d2b79f5);
    value = Math.imul(value ^ (value >>> 15), value | 1);
    value ^= value + Math.imul(value ^ (value >>> 7), value | 61);
    return ((value ^ (value >>> 14)) >>> 0) / 4294967296;
  }

  public int(min: number, max: number): number {
    return Math.floor(this.next() * (max - min + 1)) + min;
  }

  public pick<T>(items: readonly T[]): T {
    return items[this.int(0, items.length - 1)];
  }

  public chance(probability: number): boolean {
    return this.next() < probability;
  }

  public shuffle<T>(items: readonly T[]): T[] {
    const copy = [...items];
    for (let index = copy.length - 1; index > 0; index -= 1) {
      const target = this.int(0, index);
      [copy[index], copy[target]] = [copy[target], copy[index]];
    }
    return copy;
  }

  public static hash(input: string): number {
    let hash = 2166136261;
    for (let index = 0; index < input.length; index += 1) {
      hash ^= input.charCodeAt(index);
      hash = Math.imul(hash, 16777619);
    }
    return hash >>> 0;
  }
}
