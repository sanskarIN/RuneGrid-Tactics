/** Runic Field Manual design system: original heroes and threats are authored as extension-friendly field records. */

import { SeededRng } from "./rng";
import type {
  AbilityDefinition,
  Encounter,
  GameMode,
  Tile,
  Unit,
  UnitTemplate,
} from "./types";

export const GAME_VERSION = "0.1.0";

export const ABILITIES: Record<string, AbilityDefinition> = {
  shield_bash: {
    id: "shield_bash",
    name: "Shield Bash",
    short: "Bash",
    description: "Strike and stagger a nearby foe.",
    kind: "damage",
    shape: "single",
    range: 1,
    power: 4,
    cooldown: 1,
    energyCost: 0,
    element: "none",
    status: "stagger",
  },
  bulwark: {
    id: "bulwark",
    name: "Bulwark",
    short: "Ward",
    description: "Raise a temporary shield around an ally.",
    kind: "shield",
    shape: "single",
    range: 2,
    power: 5,
    cooldown: 2,
    energyCost: 1,
    element: "arcane",
  },
  ember_arc: {
    id: "ember_arc",
    name: "Ember Arc",
    short: "Arc",
    description: "Draw a line of controlled fire through a target.",
    kind: "damage",
    shape: "line",
    range: 4,
    power: 5,
    cooldown: 1,
    energyCost: 1,
    element: "fire",
    status: "scorch",
  },
  rune_mend: {
    id: "rune_mend",
    name: "Rune Mend",
    short: "Mend",
    description: "Mend a nearby ally with an arcane stitch.",
    kind: "heal",
    shape: "single",
    range: 3,
    power: 5,
    cooldown: 2,
    energyCost: 1,
    element: "arcane",
  },
  pinning_shot: {
    id: "pinning_shot",
    name: "Pinning Shot",
    short: "Pin",
    description: "A precise shot that slows a distant target.",
    kind: "damage",
    shape: "single",
    range: 5,
    power: 4,
    cooldown: 1,
    energyCost: 0,
    element: "nature",
    status: "snared",
  },
  field_step: {
    id: "field_step",
    name: "Field Step",
    short: "Step",
    description: "Mark a short safe route and reposition.",
    kind: "teleport",
    shape: "single",
    range: 3,
    cooldown: 2,
    energyCost: 1,
    element: "storm",
  },
  frost_lance: {
    id: "frost_lance",
    name: "Frost Lance",
    short: "Lance",
    description: "A cold line attack that hinders a foe.",
    kind: "damage",
    shape: "line",
    range: 4,
    power: 4,
    cooldown: 1,
    energyCost: 0,
    element: "frost",
    status: "chilled",
  },
  void_pulse: {
    id: "void_pulse",
    name: "Void Pulse",
    short: "Pulse",
    description: "A nearby void shock that damages every adjacent foe.",
    kind: "damage",
    shape: "area",
    range: 2,
    radius: 1,
    power: 3,
    cooldown: 2,
    energyCost: 1,
    element: "void",
  },
  root_bind: {
    id: "root_bind",
    name: "Root Bind",
    short: "Bind",
    description: "Nature constrains a unit in place.",
    kind: "damage",
    shape: "single",
    range: 3,
    power: 3,
    cooldown: 2,
    energyCost: 1,
    element: "nature",
    status: "snared",
  },
  stone_slam: {
    id: "stone_slam",
    name: "Stone Slam",
    short: "Slam",
    description: "A brutal close-range impact.",
    kind: "damage",
    shape: "single",
    range: 1,
    power: 6,
    cooldown: 1,
    energyCost: 0,
    element: "none",
  },
};

export const HEROES: Record<string, UnitTemplate> = {
  vanguard: {
    id: "vanguard",
    name: "Kael",
    title: "Vanguard",
    faction: "hero",
    archetype: "Linebreaker",
    maxHp: 22,
    attack: 5,
    defense: 3,
    movement: 4,
    energy: 3,
    crit: 10,
    element: "none",
    passive: "Stand Firm: gain 1 shield when ending beside a foe.",
    ultimate: "Aegis Break",
    abilities: ["shield_bash", "bulwark"],
    color: "#c99245",
  },
  runemage: {
    id: "runemage",
    name: "Iria",
    title: "Rune Mage",
    faction: "hero",
    archetype: "Channeler",
    maxHp: 15,
    attack: 4,
    defense: 1,
    movement: 4,
    energy: 4,
    crit: 12,
    element: "arcane",
    passive: "Etched Memory: the first spell each turn gains +1 power.",
    ultimate: "Starfall Script",
    abilities: ["ember_arc", "rune_mend"],
    color: "#56d6e6",
  },
  ranger: {
    id: "ranger",
    name: "Mara",
    title: "Ranger",
    faction: "hero",
    archetype: "Pathfinder",
    maxHp: 17,
    attack: 4,
    defense: 2,
    movement: 5,
    energy: 3,
    crit: 18,
    element: "nature",
    passive: "Trailwise: difficult terrain costs 1 movement.",
    ultimate: "Horizon Volley",
    abilities: ["pinning_shot", "field_step"],
    color: "#7fa65b",
  },
  guardian: {
    id: "guardian",
    name: "Orren",
    title: "Guardian",
    faction: "hero",
    archetype: "Warden",
    maxHp: 25,
    attack: 4,
    defense: 4,
    movement: 3,
    energy: 3,
    crit: 6,
    element: "frost",
    passive: "Anchor: nearby allies gain 1 defense.",
    ultimate: "Winter Bastion",
    abilities: ["frost_lance", "bulwark"],
    color: "#a9dce7",
  },
};

export const ENEMIES: Record<string, UnitTemplate> = {
  ash_raider: {
    id: "ash_raider",
    name: "Ash Raider",
    title: "Skirmisher",
    faction: "enemy",
    archetype: "Aggressor",
    maxHp: 11,
    attack: 4,
    defense: 1,
    movement: 4,
    energy: 2,
    crit: 6,
    element: "fire",
    passive: "Emberstep",
    ultimate: "Cinder Rush",
    abilities: ["shield_bash"],
    ai: "aggressive",
    color: "#b96b48",
  },
  frost_wisp: {
    id: "frost_wisp",
    name: "Frost Wisp",
    title: "Controller",
    faction: "enemy",
    archetype: "Controller",
    maxHp: 9,
    attack: 3,
    defense: 0,
    movement: 5,
    energy: 3,
    crit: 8,
    element: "frost",
    passive: "Cold wake",
    ultimate: "Whiteout",
    abilities: ["frost_lance"],
    ai: "controller",
    color: "#9fd6e5",
  },
  void_scout: {
    id: "void_scout",
    name: "Void Scout",
    title: "Ambusher",
    faction: "enemy",
    archetype: "Ambusher",
    maxHp: 10,
    attack: 4,
    defense: 1,
    movement: 5,
    energy: 3,
    crit: 14,
    element: "void",
    passive: "Unseen route",
    ultimate: "Null Step",
    abilities: ["void_pulse"],
    ai: "ambusher",
    color: "#65748c",
  },
  stone_brute: {
    id: "stone_brute",
    name: "Stone Brute",
    title: "Breaker",
    faction: "enemy",
    archetype: "Boss",
    maxHp: 24,
    attack: 6,
    defense: 3,
    movement: 3,
    energy: 2,
    crit: 4,
    element: "nature",
    passive: "Cracked Core",
    ultimate: "Fault Line",
    abilities: ["stone_slam", "root_bind"],
    ai: "boss",
    color: "#8a7961",
  },
  thorn_caster: {
    id: "thorn_caster",
    name: "Thorn Caster",
    title: "Support",
    faction: "enemy",
    archetype: "Support",
    maxHp: 12,
    attack: 3,
    defense: 1,
    movement: 4,
    energy: 3,
    crit: 5,
    element: "nature",
    passive: "Briar ward",
    ultimate: "Overgrowth",
    abilities: ["root_bind"],
    ai: "support",
    color: "#718d4d",
  },
};

export const MODE_META: Record<
  GameMode,
  { title: string; briefing: string; objective: string; enemyCount: number }
> = {
  campaign: {
    title: "The Sunken Causeway",
    briefing:
      "A collapsed rune-road has opened a route for ash scavengers. Secure the marker before the causeway breaks.",
    objective: "Clear the hostile patrol.",
    enemyCount: 3,
  },
  expedition: {
    title: "Uncharted Field",
    briefing:
      "A seed-marked route waits beyond the known atlas. Read the field and bring the party home.",
    objective: "Eliminate all hostiles.",
    enemyCount: 4,
  },
  daily: {
    title: "Daily Cartography",
    briefing:
      "Every field team receives the same marked coordinates today. Complete the route to set a clean record.",
    objective: "Clear the shared daily field.",
    enemyCount: 4,
  },
  weekly: {
    title: "Weekly Survey",
    briefing:
      "The week’s anomaly has gathered resistance around an old gate. Choose every route carefully.",
    objective: "Defeat the anomaly guard.",
    enemyCount: 5,
  },
  puzzle: {
    title: "Signal Puzzle",
    briefing:
      "The board is compact, the enemy positions are known, and no action can be wasted.",
    objective: "Win in six turns or fewer.",
    enemyCount: 3,
  },
  survival: {
    title: "Stormline Stand",
    briefing:
      "Hold the ridge while incoming threats test the line. Your field notes will record the longest stand.",
    objective: "Survive the hostile wave.",
    enemyCount: 5,
  },
  "boss-rush": {
    title: "Faultline Crown",
    briefing:
      "A Stone Brute guards the crown marker. Break the formation without letting it dictate the board.",
    objective: "Defeat the Stone Brute.",
    enemyCount: 3,
  },
  custom: {
    title: "Custom Field",
    briefing: "Set your own seed and make the map your training instrument.",
    objective: "Clear the configured encounter.",
    enemyCount: 4,
  },
  training: {
    title: "Training Grounds",
    briefing:
      "A controlled board for practicing movement, targeting, and elemental effects without pressure.",
    objective: "Defeat the training targets.",
    enemyCount: 2,
  },
  tutorial: {
    title: "First Marks",
    briefing:
      "Move, mark a target, and bring the team through the opening field.",
    objective: "Complete the guided skirmish.",
    enemyCount: 2,
  },
  endless: {
    title: "Endless Meridian",
    briefing:
      "The field shifts after every clean victory. Record your route before the horizon draws away.",
    objective: "Clear this field to continue the expedition.",
    enemyCount: 5,
  },
};

export function makeUnit(
  template: UnitTemplate,
  position: { x: number; y: number },
  instanceId: string
): Unit {
  return {
    id: instanceId,
    templateId: template.id,
    name: template.name,
    title: template.title,
    faction: template.faction,
    archetype: template.archetype,
    maxHp: template.maxHp,
    hp: template.maxHp,
    attack: template.attack,
    defense: template.defense,
    movement: template.movement,
    maxEnergy: template.energy,
    energy: template.energy,
    crit: template.crit,
    element: template.element,
    passive: template.passive,
    ultimate: template.ultimate,
    abilityIds: template.abilities,
    cooldowns: {},
    statuses: {},
    shield: 0,
    moved: false,
    acted: false,
    level: 1,
    mastery: 0,
    color: template.color,
    ai: template.ai,
    x: position.x,
    y: position.y,
  };
}

function buildGrid(rng: SeededRng, width = 9, height = 7): Tile[][] {
  const grid: Tile[][] = Array.from({ length: height }, (_, y): Tile[] =>
    Array.from(
      { length: width },
      (_, x): Tile => ({ x, y, kind: "floor", elevation: 0, discovered: true })
    )
  );
  const protectedTiles = new Set([
    "0:4",
    "0:5",
    "1:5",
    "1:6",
    "2:6",
    "8:0",
    "8:1",
    "7:0",
  ]);
  const picks: Array<Tile["kind"]> = [
    "wall",
    "difficult",
    "healing",
    "hazard",
    "teleport",
    "destructible",
  ];
  for (let i = 0; i < 10; i += 1) {
    const x = rng.int(1, width - 2);
    const y = rng.int(1, height - 2);
    if (protectedTiles.has(`${x}:${y}`) || grid[y][x].kind !== "floor")
      continue;
    const kind = rng.pick(picks);
    grid[y][x] = {
      x,
      y,
      kind,
      elevation: kind === "wall" ? 1 : 0,
      integrity: kind === "destructible" ? 8 : undefined,
      discovered: true,
    };
  }
  const teleports = grid.flat().filter(tile => tile.kind === "teleport");
  if (teleports.length === 1)
    grid[2][width - 2] = {
      x: width - 2,
      y: 2,
      kind: "teleport",
      elevation: 0,
      discovered: true,
    };
  const finalTeleports = grid.flat().filter(tile => tile.kind === "teleport");
  if (finalTeleports.length >= 2) {
    finalTeleports[0].linkedTo = {
      x: finalTeleports[1].x,
      y: finalTeleports[1].y,
    };
    finalTeleports[1].linkedTo = {
      x: finalTeleports[0].x,
      y: finalTeleports[0].y,
    };
  }
  return grid;
}

export function createEncounter(
  seed: string,
  mode: GameMode,
  difficulty: Encounter["difficulty"] = "field"
): Encounter {
  const rng = new SeededRng(`${GAME_VERSION}:${mode}:${difficulty}:${seed}`);
  const meta = MODE_META[mode];
  const grid = buildGrid(
    rng,
    mode === "puzzle" ? 7 : 9,
    mode === "puzzle" ? 6 : 7
  );
  const heroes = [
    makeUnit(HEROES.vanguard, { x: 0, y: grid.length - 2 }, "hero-vanguard"),
    makeUnit(HEROES.runemage, { x: 1, y: grid.length - 1 }, "hero-runemage"),
    makeUnit(HEROES.ranger, { x: 2, y: grid.length - 1 }, "hero-ranger"),
  ];
  const enemyPool =
    mode === "boss-rush"
      ? ["stone_brute", "ash_raider", "frost_wisp"]
      : ["ash_raider", "frost_wisp", "void_scout", "thorn_caster"];
  const difficultyBonus =
    difficulty === "legend" ? 2 : difficulty === "veteran" ? 1 : 0;
  const count = meta.enemyCount + difficultyBonus;
  const candidates = rng.shuffle(
    grid
      .flat()
      .filter(
        tile =>
          tile.x >= Math.floor(grid[0].length / 2) &&
          tile.kind !== "wall" &&
          tile.kind !== "gate"
      )
  );
  const enemies = Array.from({ length: count }, (_, index) => {
    const id =
      index === 0 && mode === "boss-rush" ? "stone_brute" : rng.pick(enemyPool);
    const unit = makeUnit(
      ENEMIES[id],
      candidates[index],
      `enemy-${index}-${id}`
    );
    if (difficulty !== "field") unit.hp += difficulty === "legend" ? 3 : 1;
    return unit;
  });
  return {
    id: `${mode}-${seed}`,
    mode,
    seed,
    version: GAME_VERSION,
    title: meta.title,
    briefing: meta.briefing,
    objective: meta.objective,
    turnLimit: mode === "puzzle" ? 6 : undefined,
    grid,
    units: [...heroes, ...enemies],
    relic: {
      id: "wayfinder-etching",
      name: "Wayfinder Etching",
      description: "The first move each turn is marked with clarity.",
    },
    reward: {
      shards: mode === "boss-rush" ? 60 : 25,
      mastery: mode === "training" ? 4 : 10,
      unlock: mode === "campaign" ? "guardian" : undefined,
    },
    difficulty,
  };
}

export const ELEMENT_RULES = [
  {
    name: "Fire + Nature",
    effect: "Scorching a rooted target deals +2 impact damage.",
  },
  {
    name: "Frost + Storm",
    effect: "A chilled target struck by storm loses its next movement.",
  },
  { name: "Arcane + Void", effect: "Arcane wards halve the next void impact." },
  {
    name: "Nature + Healing tile",
    effect: "A rooted ally on a healing tile restores +1 additional health.",
  },
];

export const ACHIEVEMENTS = [
  {
    id: "first-victory",
    name: "First Mark",
    description: "Win a battle.",
    hidden: false,
    goal: 1,
  },
  {
    id: "untouchable",
    name: "Untouchable",
    description: "Win without a hero falling below half health.",
    hidden: false,
    goal: 1,
  },
  {
    id: "expedition-master",
    name: "Expedition Master",
    description: "Win three expeditions.",
    hidden: false,
    goal: 3,
  },
  {
    id: "boss-breaker",
    name: "Boss Breaker",
    description: "Defeat a Stone Brute.",
    hidden: false,
    goal: 1,
  },
  {
    id: "rune-collector",
    name: "Rune Collector",
    description: "Collect five field relics.",
    hidden: false,
    goal: 5,
  },
  {
    id: "tactical-genius",
    name: "Tactical Genius",
    description: "Finish a puzzle inside its turn limit.",
    hidden: false,
    goal: 1,
  },
  {
    id: "the-long-route",
    name: "The Long Route",
    description: "Record five expedition victories.",
    hidden: true,
    goal: 5,
  },
];
