/** Runic Field Manual design system: restrained procedural tones acknowledge tactics without requiring licensed media. */

import type { AppSettings } from "./types";

export type AudioCue =
  | "ui"
  | "move"
  | "ability"
  | "enemy"
  | "victory"
  | "defeat";

export class AudioManager {
  private context?: AudioContext;

  public constructor(private readonly getSettings: () => AppSettings) {}

  public unlock(): void {
    if (this.context || this.getSettings().audioMuted) return;
    const Constructor = window.AudioContext;
    if (!Constructor) return;
    this.context = new Constructor();
    if (this.context.state === "suspended") void this.context.resume();
  }

  public play(cue: AudioCue): void {
    this.unlock();
    const context = this.context;
    const settings = this.getSettings();
    if (!context || settings.audioMuted || context.state !== "running") return;
    const effect = settings.accessibility.effectsVolume / 100;
    const music = settings.accessibility.musicVolume / 100;
    const profile: Record<AudioCue, [number, number, number, number]> = {
      ui: [440, 620, 0.035, effect],
      move: [250, 340, 0.06, effect],
      ability: [520, 780, 0.09, effect],
      enemy: [180, 125, 0.08, effect],
      victory: [392, 784, 0.18, music],
      defeat: [196, 104, 0.18, music],
    };
    const [from, to, duration, volume] = profile[cue];
    const oscillator = context.createOscillator();
    const gain = context.createGain();
    oscillator.type = cue === "enemy" || cue === "defeat" ? "triangle" : "sine";
    oscillator.frequency.setValueAtTime(from, context.currentTime);
    oscillator.frequency.exponentialRampToValueAtTime(
      Math.max(40, to),
      context.currentTime + duration
    );
    gain.gain.setValueAtTime(0.0001, context.currentTime);
    gain.gain.exponentialRampToValueAtTime(
      Math.max(0.0001, volume * 0.08),
      context.currentTime + 0.012
    );
    gain.gain.exponentialRampToValueAtTime(
      0.0001,
      context.currentTime + duration
    );
    oscillator.connect(gain).connect(context.destination);
    oscillator.start();
    oscillator.stop(context.currentTime + duration + 0.02);
  }

  public dispose(): void {
    void this.context?.close();
    this.context = undefined;
  }
}
