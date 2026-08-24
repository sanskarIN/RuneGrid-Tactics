/** Runic Field Manual design system: tactical rules remain visual-framework independent. */

export type ElementType =
  | "fire"
  | "frost"
  | "storm"
  | "nature"
  | "arcane"
  | "void"
  | "none";
export type Faction = "hero" | "enemy";
export type TileKind =
  | "floor"
  | "wall"
  | "difficult"
  | "healing"
  | "hazard"
  | "teleport"
  | "gate"
  | "destructible";
export type AbilityKind =
  | "damage"
  | "heal"
  | "shield"
  | "push"
  | "teleport"
  | "tile";
export type AbilityShape = "single" | "area" | "line" | "self";
export type GamePhase =
  | "menu"
  | "briefing"
  | "player"
  | "resolving"
  | "enemy"
  | "victory"
  | "defeat";
export type GameMode =
  | "campaign"
  | "expedition"
  | "daily"
  | "weekly"
  | "puzzle"
  | "survival"
  | "boss-rush"
  | "custom"
  | "training"
  | "tutorial"
  | "endless";

export interface Point {
  x: number;
  y: number;
}

export interface Tile extends Point {
  kind: TileKind;
  elevation: number;
  integrity?: number;
  linkedTo?: Point;
  discovered?: boolean;
}

export interface AbilityDefinition {
  id: string;
  name: string;
  short: string;
  description: string;
  kind: AbilityKind;
  shape: AbilityShape;
  range: number;
  radius?: number;
  power?: number;
  cooldown: number;
  energyCost: number;
  element: ElementType;
  status?: string;
}

export interface UnitTemplate {
  id: string;
  name: string;
  title: string;
  faction: Faction;
  archetype: string;
  maxHp: number;
  attack: number;
  defense: number;
  movement: number;
  energy: number;
  crit: number;
  element: ElementType;
  passive: string;
  ultimate: string;
  abilities: string[];
  ai?:
    | "aggressive"
    | "defensive"
    | "ranged"
    | "support"
    | "ambusher"
    | "controller"
    | "boss"
    | "objective";
  color: string;
}

export interface Unit extends Point {
  id: string;
  templateId: string;
  name: string;
  title: string;
  faction: Faction;
  archetype: string;
  maxHp: number;
  hp: number;
  attack: number;
  defense: number;
  movement: number;
  maxEnergy: number;
  energy: number;
  crit: number;
  element: ElementType;
  passive: string;
  ultimate: string;
  abilityIds: string[];
  cooldowns: Record<string, number>;
  statuses: Record<string, number>;
  shield: number;
  moved: boolean;
  acted: boolean;
  level: number;
  mastery: number;
  color: string;
  ai?: UnitTemplate["ai"];
}

export interface Encounter {
  id: string;
  mode: GameMode;
  seed: string;
  version: string;
  title: string;
  briefing: string;
  objective: string;
  turnLimit?: number;
  grid: Tile[][];
  units: Unit[];
  relic?: { id: string; name: string; description: string };
  reward: { shards: number; mastery: number; unlock?: string };
  difficulty: "field" | "veteran" | "legend";
}

export interface GameAction {
  turn: number;
  actorId: string;
  type: "move" | "ability" | "end-turn" | "enemy";
  target?: Point;
  abilityId?: string;
  note: string;
}

export interface ReplayData {
  schemaVersion: number;
  encounter: Pick<Encounter, "id" | "mode" | "seed" | "version" | "difficulty">;
  createdAt: string;
  actions: GameAction[];
  outcome?: "victory" | "defeat";
}

export interface Statistics {
  battlesPlayed: number;
  victories: number;
  defeats: number;
  turnsPlayed: number;
  damageDealt: number;
  healingDone: number;
  bossesDefeated: number;
  challengesCompleted: number;
  longestExpedition: number;
  perfectVictories: number;
  mostUsedHeroes: Record<string, number>;
  mostUsedAbilities: Record<string, number>;
  dailyHistory: string[];
}

export interface AchievementState {
  unlockedAt?: string;
  progress: number;
}

export interface PlayerProfile {
  playerLevel: number;
  shards: number;
  campaignCleared: string[];
  unlockedHeroes: string[];
  cosmetics: string[];
  relics: string[];
  achievements: Record<string, AchievementState>;
  statistics: Statistics;
  challengeHistory: Array<{
    mode: GameMode;
    seed: string;
    outcome: "victory" | "defeat";
    date: string;
  }>;
  replays: ReplayData[];
}

export interface AccessibilitySettings {
  textScale: "standard" | "large" | "x-large";
  highContrast: boolean;
  reducedMotion: boolean;
  reducedFlashing: boolean;
  musicVolume: number;
  effectsVolume: number;
  vibration: boolean;
  confirmActions: boolean;
  handedness: "right" | "left";
}

export interface AppSettings {
  accessibility: AccessibilitySettings;
  difficulty: Encounter["difficulty"];
  audioMuted: boolean;
}

export interface SaveData {
  schemaVersion: number;
  checksum: string;
  updatedAt: string;
  profile: PlayerProfile;
  settings: AppSettings;
  lastEncounter?: Encounter;
}

export interface SessionEvent {
  type: "state" | "log" | "unit" | "outcome";
  message?: string;
}

export interface TileHighlights {
  reachable: Point[];
  targets: Point[];
  danger: Point[];
  selected?: Point;
}
