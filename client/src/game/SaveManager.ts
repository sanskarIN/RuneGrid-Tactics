/** Runic Field Manual design system: saved field records are local, validated, versioned, and recoverable. */

import { SeededRng } from "./rng";
import { defaultProfile, defaultSettings } from "./Progression";
import type { SaveData } from "./types";

const SAVE_KEY = "runegrid-tactics-save";
const BACKUP_KEY = "runegrid-tactics-save-backup";
const SCHEMA_VERSION = 2;

export class SaveManager {
  public static makeDefault(): SaveData {
    return SaveManager.seal({
      schemaVersion: SCHEMA_VERSION,
      checksum: "",
      updatedAt: new Date().toISOString(),
      profile: defaultProfile(),
      settings: defaultSettings(),
    });
  }

  public static load(): SaveData {
    const primary = SaveManager.read(SAVE_KEY);
    if (primary) return primary;
    const backup = SaveManager.read(BACKUP_KEY);
    if (backup) {
      localStorage.setItem(SAVE_KEY, JSON.stringify(backup));
      return backup;
    }
    return SaveManager.makeDefault();
  }

  public static save(data: SaveData): SaveData {
    const sealed = SaveManager.seal({
      ...data,
      updatedAt: new Date().toISOString(),
    });
    const previous = localStorage.getItem(SAVE_KEY);
    if (previous) localStorage.setItem(BACKUP_KEY, previous);
    localStorage.setItem(SAVE_KEY, JSON.stringify(sealed));
    return sealed;
  }

  public static export(data: SaveData): string {
    return JSON.stringify(SaveManager.seal(data), null, 2);
  }

  public static import(raw: string): SaveData {
    const parsed: unknown = JSON.parse(raw);
    const candidate = SaveManager.migrate(parsed);
    if (!SaveManager.valid(candidate))
      throw new Error("The imported field record failed integrity validation.");
    return SaveManager.save(candidate);
  }

  public static reset(): SaveData {
    localStorage.removeItem(SAVE_KEY);
    localStorage.removeItem(BACKUP_KEY);
    return SaveManager.makeDefault();
  }

  private static read(key: string): SaveData | null {
    try {
      const raw = localStorage.getItem(key);
      if (!raw) return null;
      const migrated = SaveManager.migrate(JSON.parse(raw));
      return SaveManager.valid(migrated) ? migrated : null;
    } catch {
      return null;
    }
  }

  private static migrate(value: unknown): SaveData {
    const candidate = value as Partial<SaveData>;
    if (candidate?.schemaVersion === 1) {
      const migrated = {
        ...candidate,
        schemaVersion: SCHEMA_VERSION,
        settings: candidate.settings ?? defaultSettings(),
        updatedAt: candidate.updatedAt ?? new Date().toISOString(),
      } as SaveData;
      return SaveManager.seal(migrated);
    }
    return candidate as SaveData;
  }

  private static valid(data: SaveData): boolean {
    if (
      !data ||
      data.schemaVersion !== SCHEMA_VERSION ||
      !data.profile ||
      !data.settings ||
      typeof data.checksum !== "string"
    )
      return false;
    return SaveManager.checksum({ ...data, checksum: "" }) === data.checksum;
  }

  private static seal(data: SaveData): SaveData {
    return {
      ...data,
      checksum: SaveManager.checksum({ ...data, checksum: "" }),
    };
  }

  private static checksum(data: SaveData): string {
    return SeededRng.hash(JSON.stringify(data)).toString(16).padStart(8, "0");
  }
}
