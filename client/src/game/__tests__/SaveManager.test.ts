import { beforeEach, describe, expect, it } from "vitest";
import { SaveManager } from "../SaveManager";

class MemoryStorage {
  private values = new Map<string, string>();
  public getItem(key: string): string | null {
    return this.values.get(key) ?? null;
  }
  public setItem(key: string, value: string): void {
    this.values.set(key, value);
  }
  public removeItem(key: string): void {
    this.values.delete(key);
  }
  public clear(): void {
    this.values.clear();
  }
}

describe("SaveManager", () => {
  beforeEach(() => {
    Object.assign(globalThis, { localStorage: new MemoryStorage() });
  });

  it("exports and imports a checksum-validated local record", () => {
    const save = SaveManager.makeDefault();
    save.profile.shards = 42;
    const imported = SaveManager.import(SaveManager.export(save));
    expect(imported.profile.shards).toBe(42);
    expect(SaveManager.load().profile.shards).toBe(42);
  });

  it("rejects malformed records without accepting them as a save", () => {
    expect(() => SaveManager.import("{ invalid-json")).toThrow();
    expect(SaveManager.load().profile.playerLevel).toBe(1);
  });
});
