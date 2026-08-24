/** Runic Field Manual design system: every field result becomes a local, inspectable record. */

import { ACHIEVEMENTS } from "./content";
import type {
  AchievementState,
  AppSettings,
  Encounter,
  PlayerProfile,
  Statistics,
} from "./types";

export const defaultStatistics = (): Statistics => ({
  battlesPlayed: 0,
  victories: 0,
  defeats: 0,
  turnsPlayed: 0,
  damageDealt: 0,
  healingDone: 0,
  bossesDefeated: 0,
  challengesCompleted: 0,
  longestExpedition: 0,
  perfectVictories: 0,
  mostUsedHeroes: {},
  mostUsedAbilities: {},
  dailyHistory: [],
});

export const defaultProfile = (): PlayerProfile => ({
  playerLevel: 1,
  shards: 0,
  campaignCleared: [],
  unlockedHeroes: ["vanguard", "runemage", "ranger"],
  cosmetics: ["field-standard"],
  relics: [],
  achievements: {},
  statistics: defaultStatistics(),
  challengeHistory: [],
  replays: [],
});

export const defaultSettings = (): AppSettings => ({
  accessibility: {
    textScale: "standard",
    highContrast: false,
    reducedMotion: false,
    reducedFlashing: true,
    musicVolume: 45,
    effectsVolume: 55,
    vibration: false,
    confirmActions: false,
    handedness: "right",
  },
  difficulty: "field",
  audioMuted: false,
});

export class ProgressionService {
  public constructor(public profile: PlayerProfile = defaultProfile()) {}

  public recordHeroUse(heroId: string): void {
    this.profile.statistics.mostUsedHeroes[heroId] =
      (this.profile.statistics.mostUsedHeroes[heroId] ?? 0) + 1;
  }

  public recordAbilityUse(abilityId: string): void {
    this.profile.statistics.mostUsedAbilities[abilityId] =
      (this.profile.statistics.mostUsedAbilities[abilityId] ?? 0) + 1;
  }

  public recordDamage(amount: number): void {
    this.profile.statistics.damageDealt += Math.max(0, amount);
  }

  public recordHealing(amount: number): void {
    this.profile.statistics.healingDone += Math.max(0, amount);
  }

  public recordBattle(
    encounter: Encounter,
    outcome: "victory" | "defeat",
    turns: number,
    pristine: boolean
  ): string[] {
    const stats = this.profile.statistics;
    stats.battlesPlayed += 1;
    stats.turnsPlayed += turns;
    if (outcome === "victory") {
      stats.victories += 1;
      this.profile.shards += encounter.reward.shards;
      this.profile.playerLevel = Math.max(
        1,
        Math.floor(stats.victories / 3) + 1
      );
      if (encounter.mode === "expedition" || encounter.mode === "endless")
        stats.longestExpedition = Math.max(
          stats.longestExpedition,
          stats.victories
        );
      if (encounter.mode === "daily")
        stats.dailyHistory = Array.from(
          new Set([...stats.dailyHistory, encounter.seed])
        ).slice(-30);
      if (["daily", "weekly", "puzzle"].includes(encounter.mode))
        stats.challengesCompleted += 1;
      if (
        encounter.mode === "boss-rush" ||
        encounter.units.some(
          unit => unit.templateId === "stone_brute" && unit.hp <= 0
        )
      )
        stats.bossesDefeated += 1;
      if (pristine) stats.perfectVictories += 1;
      if (
        encounter.mode === "campaign" &&
        !this.profile.campaignCleared.includes(encounter.id)
      )
        this.profile.campaignCleared.push(encounter.id);
      if (
        encounter.reward.unlock &&
        !this.profile.unlockedHeroes.includes(encounter.reward.unlock)
      )
        this.profile.unlockedHeroes.push(encounter.reward.unlock);
      if (encounter.relic && !this.profile.relics.includes(encounter.relic.id))
        this.profile.relics.push(encounter.relic.id);
    } else {
      stats.defeats += 1;
    }
    this.profile.challengeHistory.unshift({
      mode: encounter.mode,
      seed: encounter.seed,
      outcome,
      date: new Date().toISOString(),
    });
    this.profile.challengeHistory = this.profile.challengeHistory.slice(0, 50);
    return this.evaluateAchievements(encounter, outcome, pristine);
  }

  public evaluateAchievements(
    encounter: Encounter,
    outcome: "victory" | "defeat",
    pristine: boolean
  ): string[] {
    const newlyUnlocked: string[] = [];
    const values: Record<string, number> = {
      "first-victory": this.profile.statistics.victories,
      untouchable: pristine && outcome === "victory" ? 1 : 0,
      "expedition-master": this.profile.challengeHistory.filter(
        item => item.mode === "expedition" && item.outcome === "victory"
      ).length,
      "boss-breaker": this.profile.statistics.bossesDefeated,
      "rune-collector": this.profile.relics.length,
      "tactical-genius":
        encounter.mode === "puzzle" && outcome === "victory" ? 1 : 0,
      "the-long-route": this.profile.challengeHistory.filter(
        item =>
          ["expedition", "endless"].includes(item.mode) &&
          item.outcome === "victory"
      ).length,
    };
    for (const achievement of ACHIEVEMENTS) {
      const state: AchievementState = this.profile.achievements[
        achievement.id
      ] ?? { progress: 0 };
      state.progress = Math.max(state.progress, values[achievement.id] ?? 0);
      if (!state.unlockedAt && state.progress >= achievement.goal) {
        state.unlockedAt = new Date().toISOString();
        newlyUnlocked.push(achievement.name);
      }
      this.profile.achievements[achievement.id] = state;
    }
    return newlyUnlocked;
  }
}
