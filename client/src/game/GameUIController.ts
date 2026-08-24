/** Runic Field Manual design system: asymmetrical command table, parchment dossiers, and explicit tactical affordances. */

import {
  ABILITIES,
  ACHIEVEMENTS,
  ELEMENT_RULES,
  ENEMIES,
  HEROES,
  MODE_META,
} from "./content";
import { ProgressionService } from "./Progression";
import type {
  AppSettings,
  GameMode,
  GamePhase,
  PlayerProfile,
  ReplayData,
} from "./types";
import type { GameSession } from "./GameSession";

const LOGO_URL = "/manus-storage/runegrid-logo_4ac1c3fe.png";
const REFERENCE_URL = "/manus-storage/runegird-reference_fde55e61.png";
const HERO_ART_URL = "/manus-storage/runegrid-heroes_fffcf0ab.png";
const ENEMY_ART_URL = "/manus-storage/runegrid-enemies_5930cdca.png";

type Screen =
  | "menu"
  | "mode-list"
  | "campaign"
  | "collection"
  | "codex"
  | "achievements"
  | "statistics"
  | "replays"
  | "settings"
  | "about";

interface UiOptions {
  session: () => GameSession | undefined;
  progression: () => ProgressionService;
  settings: () => AppSettings;
  startEncounter: (mode: GameMode, seed?: string) => void;
  restartEncounter: () => void;
  leaveEncounter: () => void;
  persist: () => void;
  exportSave: () => void;
  importSave: (file: File) => void;
  resetSave: () => void;
}

const label = (value: string) =>
  value.replace(/-/g, " ").replace(/\b\w/g, letter => letter.toUpperCase());
const escapeHtml = (value: string) =>
  value.replace(
    /[&<>'"]/g,
    character =>
      ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", "'": "&#39;", '"': "&quot;" })[
        character
      ] ?? character
  );

export class GameUIController {
  private screen: Screen = "menu";
  private notice = "";
  private detachSession?: () => void;

  public constructor(
    private readonly host: HTMLElement,
    private readonly options: UiOptions
  ) {
    this.host.addEventListener("click", this.handleClick);
    this.host.addEventListener("change", this.handleChange);
    this.render();
  }

  public setSession(session?: GameSession): void {
    this.detachSession?.();
    if (session) this.detachSession = session.subscribe(() => this.render());
    this.render();
  }

  public render(): void {
    const settings = this.options.settings();
    const session = this.options.session();
    this.host.dataset.textScale = settings.accessibility.textScale;
    this.host.classList.toggle(
      "is-high-contrast",
      settings.accessibility.highContrast
    );
    this.host.classList.toggle(
      "reduce-motion",
      settings.accessibility.reducedMotion
    );
    this.host.innerHTML =
      session && session.phase !== "briefing"
        ? this.battleShell(session)
        : this.menuShell();
  }

  public dispose(): void {
    this.detachSession?.();
    this.host.removeEventListener("click", this.handleClick);
    this.host.removeEventListener("change", this.handleChange);
    this.host.replaceChildren();
  }

  private menuShell(): string {
    const content = {
      menu: this.mainMenu(),
      "mode-list": this.modeList(),
      campaign: this.campaign(),
      collection: this.collection(),
      codex: this.codex(),
      achievements: this.achievements(),
      statistics: this.statistics(),
      replays: this.replays(),
      settings: this.settingsView(),
      about: this.about(),
    }[this.screen];
    return `<div class="rg-ui rg-menu-shell" aria-live="polite">
      ${this.notice ? `<div class="rg-notice" role="status">${escapeHtml(this.notice)}</div>` : ""}
      <header class="rg-topbar"><button class="rg-brand" data-action="screen" data-screen="menu" aria-label="Open main menu"><img src="${LOGO_URL}" alt="" /><span><b>RUNE</b>GRID <i>TACTICS</i></span></button><nav aria-label="Field library"><button data-action="screen" data-screen="campaign">Campaign</button><button data-action="screen" data-screen="collection">Roster</button><button data-action="screen" data-screen="codex">Codex</button><button data-action="screen" data-screen="settings">Settings</button></nav><button class="rg-profile" data-action="screen" data-screen="statistics" aria-label="View player statistics"><span class="rg-seal">${this.options.progression().profile.playerLevel}</span><span>Field Rank<br /><b>Wayfinder</b></span></button></header>
      <main class="rg-menu-content">${content}</main>
      <footer class="rg-footer"><span>Made by the Sanskar</span><span>Local-first field records · v0.1.0</span><button data-action="screen" data-screen="about">Credits & Support</button></footer>
    </div>`;
  }

  private mainMenu(): string {
    const profile = this.options.progression().profile;
    const saved = profile.statistics.battlesPlayed > 0;
    return `<section class="rg-main-menu">
      <div class="rg-hero-copy"><p class="rg-eyebrow">FIELD ATLAS · SECTOR 03</p><h1>Mark a route.<br /><em>Own the turn.</em></h1><p class="rg-lead">An original tactical roguelite where every movement mark, elemental choice, and damaged bridge changes the field.</p><div class="rg-primary-actions"><button class="rg-button rg-button-primary" data-action="start" data-mode="campaign">${saved ? "Continue Causeway" : "Begin Campaign"}<span>→</span></button><button class="rg-button rg-button-quiet" data-action="start" data-mode="expedition">Seeded Expedition <span>↗</span></button></div><div class="rg-route-stats"><span><b>${profile.statistics.victories}</b> secured fields</span><span><b>${profile.relics.length}</b> discovered relics</span><span><b>${profile.shards}</b> route shards</span></div></div>
      <div class="rg-atlas-card" style="background-image:linear-gradient(90deg,rgba(14,24,25,.12),rgba(14,24,25,.72)),url('${REFERENCE_URL}')"><div class="rg-atlas-pin"><span class="rg-pulse"></span>LIVE FIELD PREVIEW</div><div class="rg-atlas-bottom"><p>THE SUNKEN CAUSEWAY</p><span>Campaign chapter 01 · 3 heroes deployed</span></div></div>
      <aside class="rg-dispatch"><p class="rg-section-label">DISPATCH BOARD</p><button data-action="start" data-mode="daily"><span class="rg-symbol">◈</span><span><b>Daily Cartography</b><small>Shared coordinates · same field for every team</small></span><i>→</i></button><button data-action="start" data-mode="training"><span class="rg-symbol">⊹</span><span><b>Training Grounds</b><small>Practice movement, range, and timing</small></span><i>→</i></button><button data-action="screen" data-screen="mode-list"><span class="rg-symbol">⌘</span><span><b>All Field Modes</b><small>Puzzles, survival, custom routes, and more</small></span><i>→</i></button></aside>
    </section>`;
  }

  private modeList(): string {
    const entries = Object.entries(MODE_META) as [
      GameMode,
      (typeof MODE_META)[GameMode],
    ][];
    return `<section class="rg-library-view"><div class="rg-view-heading"><div><p class="rg-eyebrow">OPERATION LIBRARY</p><h2>Choose a field condition.</h2></div><button class="rg-text-button" data-action="screen" data-screen="menu">Return to command table</button></div><div class="rg-mode-grid">${entries.map(([mode, meta], index) => `<article class="rg-mode-card ${index === 0 ? "featured" : ""}"><p>${String(index + 1).padStart(2, "0")}</p><h3>${escapeHtml(label(mode))}</h3><span>${escapeHtml(meta.briefing)}</span><button data-action="start" data-mode="${mode}">Deploy <i>→</i></button></article>`).join("")}</div></section>`;
  }

  private campaign(): string {
    const cleared = this.options.progression().profile.campaignCleared;
    return `<section class="rg-campaign-view"><div class="rg-view-heading"><div><p class="rg-eyebrow">THE FRACTURED MERIDIAN</p><h2>Campaign field map</h2><p>Trace the old route network through a sky-scarred region where every recovered marker broadens the atlas.</p></div><button class="rg-text-button" data-action="screen" data-screen="menu">Return to command table</button></div><div class="rg-world-map"><div class="rg-route-line"></div><article class="rg-region is-current"><span>01</span><div><p>THE SUNKEN CAUSEWAY</p><small>${cleared.length ? "Field secured · replayable" : "Active mission · unlock Guardian"}</small><button data-action="start" data-mode="campaign">Open Field</button></div></article><article class="rg-region is-locked"><span>02</span><div><p>THE LANTERN FEN</p><small>Unlock after the causeway report.</small><button data-action="start" data-mode="expedition">Scout as Expedition</button></div></article><article class="rg-region is-locked"><span>03</span><div><p>THE PALE ENGINE</p><small>Boss route · release roadmap field.</small><button data-action="start" data-mode="boss-rush">Enter Boss Field</button></div></article></div><div class="rg-field-note"><b>Field note</b><p>Campaign data is local by default. A cleared field unlocks the Guardian record and preserves a replayable seed.</p></div></section>`;
  }

  private collection(): string {
    const profile = this.options.progression().profile;
    return `<section class="rg-library-view"><div class="rg-view-heading"><div><p class="rg-eyebrow">HERO COLLECTION</p><h2>Field roster</h2><p>Each record is data-driven: stats, equipment slots, mastery, cosmetic identity, abilities, passive, and ultimate can be expanded without rewriting the tactical rules.</p></div><button class="rg-text-button" data-action="screen" data-screen="menu">Return to command table</button></div><div class="rg-collection-layout"><div class="rg-art-panel" style="background-image:linear-gradient(0deg,rgba(18,25,25,.8),rgba(18,25,25,.05)),url('${HERO_ART_URL}')"><p>ACTIVE TOKEN STUDIES</p></div><div class="rg-hero-list">${Object.values(
      HEROES
    )
      .map(
        hero =>
          `<article class="rg-hero-record ${profile.unlockedHeroes.includes(hero.id) ? "" : "locked"}"><span class="rg-token" style="--token:${hero.color}"></span><div><p>${escapeHtml(hero.title)}</p><h3>${escapeHtml(hero.name)}</h3><small>${escapeHtml(hero.archetype)} · ${hero.maxHp} vitality · ${hero.movement} movement</small></div><div class="rg-hero-ability"><b>${escapeHtml(hero.passive)}</b><span>${profile.unlockedHeroes.includes(hero.id) ? "Equippable field record" : "Locked through campaign"}</span></div></article>`
      )
      .join("")}</div></div></section>`;
  }

  private codex(): string {
    const tiles = [
      ["Floor", "Open route; standard movement."],
      ["Difficult", "Costs two route marks unless a pathfinder negates it."],
      ["Healing", "Restores health when a unit stops there."],
      ["Hazard", "Deals predictable impact on entry."],
      ["Teleport", "Linked markers reposition a clear unit."],
      ["Destructible", "A brittle obstacle reserved for future tile attacks."],
    ];
    return `<section class="rg-library-view"><div class="rg-view-heading"><div><p class="rg-eyebrow">FIELD ENCYCLOPEDIA</p><h2>Elements, terrain, and threats</h2><p>Every combat relationship is stated in ordinary language, and never relies on color alone.</p></div><button class="rg-text-button" data-action="screen" data-screen="menu">Return to command table</button></div><div class="rg-codex-columns"><div><h3>Elemental interactions</h3>${ELEMENT_RULES.map(rule => `<article class="rg-codex-row"><b>${escapeHtml(rule.name)}</b><span>${escapeHtml(rule.effect)}</span></article>`).join("")}</div><div><h3>Terrain ledger</h3>${tiles.map(([title, text]) => `<article class="rg-codex-row"><b>${title}</b><span>${text}</span></article>`).join("")}</div><div class="rg-enemy-sheet" style="background-image:linear-gradient(0deg,rgba(20,29,29,.86),rgba(20,29,29,.08)),url('${ENEMY_ART_URL}')"><h3>Hostile profiles</h3><p>${Object.values(
      ENEMIES
    )
      .map(enemy => `${enemy.name} · ${enemy.ai}`)
      .join("<br />")}</p></div></div></section>`;
  }

  private achievements(): string {
    const profile = this.options.progression().profile;
    return `<section class="rg-library-view"><div class="rg-view-heading"><div><p class="rg-eyebrow">FIELD DISTINCTIONS</p><h2>Achievements</h2><p>Achievements are local, transparent, and never sold. Hidden distinctions remain unnamed until their conditions are met.</p></div><button class="rg-text-button" data-action="screen" data-screen="menu">Return to command table</button></div><div class="rg-achievement-grid">${ACHIEVEMENTS.map(
      achievement => {
        const state = profile.achievements[achievement.id];
        const unlocked = Boolean(state?.unlockedAt);
        return `<article class="rg-achievement ${unlocked ? "is-unlocked" : ""}"><span>${unlocked ? "◆" : "◇"}</span><div><p>${achievement.hidden && !unlocked ? "Uncharted distinction" : escapeHtml(achievement.name)}</p><small>${achievement.hidden && !unlocked ? "Keep exploring the field." : escapeHtml(achievement.description)}</small><em>${Math.min(state?.progress ?? 0, achievement.goal)} / ${achievement.goal}</em></div></article>`;
      }
    ).join("")}</div></section>`;
  }

  private statistics(): string {
    const stats = this.options.progression().profile.statistics;
    const winRate = stats.battlesPlayed
      ? Math.round((stats.victories / stats.battlesPlayed) * 100)
      : 0;
    const favorite =
      Object.entries(stats.mostUsedHeroes).sort(
        (a, b) => b[1] - a[1]
      )[0]?.[0] ?? "No field record";
    return `<section class="rg-library-view"><div class="rg-view-heading"><div><p class="rg-eyebrow">FIELD LEDGER</p><h2>Local statistics</h2><p>These records stay in this browser unless you explicitly export a backup.</p></div><button class="rg-text-button" data-action="screen" data-screen="menu">Return to command table</button></div><div class="rg-stat-strip"><article><b>${stats.battlesPlayed}</b><span>fields entered</span></article><article><b>${stats.victories}</b><span>fields secured</span></article><article><b>${winRate}%</b><span>win percentage</span></article><article><b>${stats.damageDealt}</b><span>impact dealt</span></article><article><b>${stats.healingDone}</b><span>health restored</span></article><article><b>${stats.bossesDefeated}</b><span>bosses broken</span></article></div><div class="rg-ledger-columns"><article><p>Most-used hero</p><h3>${escapeHtml(label(favorite))}</h3><small>Hero usage increments each time a field begins.</small></article><article><p>Longest expedition</p><h3>${stats.longestExpedition} fields</h3><small>Best ongoing local record across expedition and endless conditions.</small></article><article><p>Perfect victories</p><h3>${stats.perfectVictories}</h3><small>Wins where no hero falls below half vitality.</small></article></div></section>`;
  }

  private replays(): string {
    const replays = this.options.progression().profile.replays;
    return `<section class="rg-library-view"><div class="rg-view-heading"><div><p class="rg-eyebrow">ROUTE ARCHIVE</p><h2>Replay records</h2><p>Completed fields save deterministic actions, encounter seed, difficulty, and game version for local inspection.</p></div><button class="rg-text-button" data-action="screen" data-screen="menu">Return to command table</button></div><div class="rg-replay-list">${replays.length ? replays.map((replay, index) => this.replayItem(replay, index)).join("") : `<article class="rg-empty-record"><b>No route has been archived yet.</b><span>Complete a field to store its seed and tactical log.</span><button data-action="start" data-mode="training">Open Training Field</button></article>`}</div></section>`;
  }

  private replayItem(replay: ReplayData, index: number): string {
    return `<article class="rg-replay-record"><span class="rg-seal">${String(index + 1).padStart(2, "0")}</span><div><p>${escapeHtml(label(replay.encounter.mode))} · ${escapeHtml(replay.outcome ?? "in progress")}</p><small>Seed: ${escapeHtml(replay.encounter.seed)} · ${replay.actions.length} recorded actions · v${escapeHtml(replay.encounter.version)}</small></div><button data-action="replay" data-mode="${replay.encounter.mode}" data-seed="${escapeHtml(replay.encounter.seed)}">Rebuild Field</button></article>`;
  }

  private settingsView(): string {
    const settings = this.options.settings();
    const access = settings.accessibility;
    return `<section class="rg-library-view"><div class="rg-view-heading"><div><p class="rg-eyebrow">FIELD CONFIGURATION</p><h2>Settings & accessibility</h2><p>Changes save locally. Tactical state always has a color-independent label, and reduced motion keeps every result understandable.</p></div><button class="rg-text-button" data-action="screen" data-screen="menu">Return to command table</button></div><div class="rg-settings-grid"><article><h3>Readability</h3>${this.switchRow("highContrast", "High contrast", "Makes route states and controls more distinct.", access.highContrast)}${this.switchRow("reducedMotion", "Reduced motion", "Uses immediate feedback instead of route motion.", access.reducedMotion)}${this.switchRow("reducedFlashing", "Reduce flashing", "Preserves muted visual combat feedback.", access.reducedFlashing)}<label class="rg-select-row">Text scale<select data-setting="textScale"><option value="standard" ${access.textScale === "standard" ? "selected" : ""}>Standard</option><option value="large" ${access.textScale === "large" ? "selected" : ""}>Large</option><option value="x-large" ${access.textScale === "x-large" ? "selected" : ""}>Extra large</option></select></label></article><article><h3>Field comfort</h3>${this.switchRow("vibration", "Haptic cue toggle", "Reserved for compatible installed web apps.", access.vibration)}${this.switchRow("confirmActions", "Confirm actions", "Requires a second deliberate ability confirmation.", access.confirmActions)}${this.switchRow("audioMuted", "Mute all sound", "Silences original field tones immediately.", settings.audioMuted)}<label class="rg-range-row">Music volume<input type="range" data-setting="musicVolume" min="0" max="100" value="${access.musicVolume}" /><output>${access.musicVolume}%</output></label><label class="rg-range-row">Effects volume<input type="range" data-setting="effectsVolume" min="0" max="100" value="${access.effectsVolume}" /><output>${access.effectsVolume}%</output></label><label class="rg-select-row">Dominant hand<select data-setting="handedness"><option value="right" ${access.handedness === "right" ? "selected" : ""}>Right-handed controls</option><option value="left" ${access.handedness === "left" ? "selected" : ""}>Left-handed controls</option></select></label><label class="rg-select-row">Difficulty<select data-setting="difficulty"><option value="field" ${settings.difficulty === "field" ? "selected" : ""}>Field · transparent tactics</option><option value="veteran" ${settings.difficulty === "veteran" ? "selected" : ""}>Veteran · denser patrols</option><option value="legend" ${settings.difficulty === "legend" ? "selected" : ""}>Legend · reinforced enemy teams</option></select></label></article><article><h3>Local records</h3><p class="rg-setting-copy">A validated save has a schema version, checksum, rolling backup, export, import, and a safe recovery path.</p><button class="rg-button rg-button-quiet" data-action="export">Export field record</button><label class="rg-import-button">Import field record<input type="file" data-action="import" accept="application/json" /></label><button class="rg-danger-button" data-action="reset-save">Reset local record</button></article></div></section>`;
  }

  private switchRow(
    id: string,
    title: string,
    description: string,
    checked: boolean
  ): string {
    return `<label class="rg-switch-row"><span><b>${title}</b><small>${description}</small></span><input type="checkbox" data-setting="${id}" ${checked ? "checked" : ""} /><i aria-hidden="true"></i></label>`;
  }

  private about(): string {
    return `<section class="rg-library-view"><div class="rg-view-heading"><div><p class="rg-eyebrow">ABOUT THE ATLAS</p><h2>RuneGrid Tactics</h2><p>Original local-first tactical roguelite designed and developed by Sanskar.</p></div><button class="rg-text-button" data-action="screen" data-screen="menu">Return to command table</button></div><div class="rg-about-grid"><article><h3>Credits</h3><p><b>Developer / Creator:</b> Sanskar</p><p>Made by the Sanskar</p><p><b>Open source:</b> MIT License</p><p><b>Project code:</b> github.com/sanskarIN/RuneGrid-Tactics</p></article><article><h3>Support</h3><p>Support development without interrupting a battle, gating progress, or presenting donations as purchases.</p><a href="https://buymeacoffee.com/sanskarIN" target="_blank" rel="noreferrer">Support development on Buy Me a Coffee →</a><p><b>Support email:</b> supportramsandesh@gmail.com</p></article><article><h3>Privacy</h3><p>Core progress, settings, statistics, replays, and challenge history remain in local browser storage by default. Exports are initiated by you.</p><button data-action="screen" data-screen="settings">Review data controls</button></article></div></section>`;
  }

  private battleShell(session: GameSession): string {
    const phase = session.phase;
    const selected = session.selectedUnit;
    const showOutcome = phase === "victory" || phase === "defeat";
    return `<div class="rg-ui rg-battle-shell phase-${phase}" aria-live="polite">
      <header class="rg-battle-top"><button class="rg-brand" data-action="leave" aria-label="Leave current field"><img src="${LOGO_URL}" alt="" /><span><b>RUNE</b>GRID <i>TACTICS</i></span></button><div class="rg-mission-title"><p>${escapeHtml(session.encounter.title)}</p><span>${escapeHtml(session.encounter.objective)}</span></div><div class="rg-turn-readout"><b>TURN ${session.turn}</b><span>${label(phase)} phase</span></div><button class="rg-menu-button" data-action="pause">Field pause</button></header>
      <aside class="rg-squad-dossier"><p class="rg-section-label">FIELD TEAM</p>${session.livingHeroes.map(hero => `<button class="rg-squad-card ${selected?.id === hero.id ? "selected" : ""}" data-action="hero" data-id="${hero.id}" ${phase !== "player" ? "disabled" : ""}><span class="rg-token" style="--token:${hero.color}"></span><span><b>${hero.name}</b><small>${hero.title} · ${hero.hp}/${hero.maxHp} vitality</small><i><em style="width:${(hero.hp / hero.maxHp) * 100}%"></em></i></span><strong>${hero.energy}/${hero.maxEnergy}</strong></button>`).join("")}<button class="rg-dossier-link" data-action="screen" data-screen="codex">Open field codex <i>→</i></button></aside>
      <aside class="rg-field-dossier"><p class="rg-section-label">FIELD BRIEF</p><div class="rg-objective-mark"><span>◇</span><p>${escapeHtml(session.encounter.objective)}</p></div><dl><div><dt>SEED</dt><dd>${escapeHtml(session.encounter.seed)}</dd></div><div><dt>THREAT</dt><dd>${session.livingEnemies.length} hostiles</dd></div><div><dt>RELIC</dt><dd>${escapeHtml(session.encounter.relic?.name ?? "None")}</dd></div></dl><div class="rg-turn-log">${session.logs
        .slice(0, 3)
        .map(line => `<p>${escapeHtml(line)}</p>`)
        .join(
          ""
        )}</div><div class="rg-tile-key"><span><i class="route"></i> route</span><span><i class="target"></i> target</span><span><i class="danger"></i> threat range</span></div></aside>
      <div class="rg-command-bar"><div class="rg-selected-unit">${selected ? `<span class="rg-token" style="--token:${selected.color}"></span><span><b>${selected.name} · ${selected.title}</b><small>${selected.passive}</small></span>` : `<span><b>Select a hero token</b><small>Tap a friendly unit on the table, then mark a valid route.</small></span>`}</div><div class="rg-ability-rail">${
        selected
          ? selected.abilityIds
              .map(id => {
                const ability = ABILITIES[id];
                return ability
                  ? `<button class="rg-ability ${session.selectedAbilityId === id ? "active" : ""}" data-action="ability" data-id="${id}" ${selected.acted || selected.energy < ability.energyCost || (selected.cooldowns[id] ?? 0) > 0 || phase !== "player" ? "disabled" : ""}><b>${ability.short}</b><span>${ability.name}</span><small>${ability.energyCost ? `${ability.energyCost} energy` : "free"}</small></button>`
                  : "";
              })
              .join("")
          : ""
      }</div><div class="rg-turn-actions"><button class="rg-icon-button" data-action="undo" ${session.hasUndo ? "" : "disabled"} aria-label="Undo last field action">↶</button><button class="rg-end-turn" data-action="end-turn" ${phase === "player" ? "" : "disabled"}>END TURN <span>→</span></button></div></div>
      ${showOutcome ? this.outcomePanel(session) : ""}${this.pausePanel()}
    </div>`;
  }

  private outcomePanel(session: GameSession): string {
    const win = session.phase === "victory";
    return `<div class="rg-outcome" role="dialog" aria-modal="true"><div class="rg-outcome-mark">${win ? "◆" : "◇"}</div><p class="rg-eyebrow">FIELD REPORT COMPLETE</p><h2>${win ? "Route secured." : "Route interrupted."}</h2><p>${win ? `The field yields ${session.encounter.reward.shards} route shards and ${session.encounter.reward.mastery} mastery marks.` : "The team has withdrawn with its observations intact. Study the seed and re-enter when ready."}</p><div><button class="rg-button rg-button-primary" data-action="restart">${win && session.encounter.mode === "endless" ? "Continue Meridian" : "Rebuild Field"}</button><button class="rg-button rg-button-quiet" data-action="leave">Return to command table</button></div></div>`;
  }

  private pausePanel(): string {
    return `<div class="rg-pause-panel" hidden><article><p class="rg-eyebrow">FIELD PAUSED</p><h2>Take a reading.</h2><button data-action="resume">Return to field</button><button data-action="export">Export local record</button><button data-action="leave">Withdraw to command table</button></article></div>`;
  }

  private readonly handleClick = (event: Event): void => {
    const control = (event.target as HTMLElement).closest<HTMLElement>(
      "[data-action]"
    );
    if (!control) return;
    const action = control.dataset.action;
    const session = this.options.session();
    if (action === "screen") {
      this.screen = control.dataset.screen as Screen;
      this.notice = "";
      this.render();
      return;
    }
    if (action === "start") {
      this.options.startEncounter(
        control.dataset.mode as GameMode,
        this.seedFor(control.dataset.mode as GameMode)
      );
      return;
    }
    if (action === "hero" && control.dataset.id) {
      session?.selectUnit(control.dataset.id);
      return;
    }
    if (action === "ability" && control.dataset.id) {
      session?.selectAbility(control.dataset.id);
      return;
    }
    if (action === "end-turn") {
      session?.endTurn();
      return;
    }
    if (action === "undo") {
      session?.undo();
      return;
    }
    if (action === "restart") {
      this.options.restartEncounter();
      return;
    }
    if (action === "leave") {
      this.options.leaveEncounter();
      this.screen = "menu";
      return;
    }
    if (action === "pause") {
      const panel = this.host.querySelector<HTMLElement>(".rg-pause-panel");
      if (panel) panel.hidden = false;
      return;
    }
    if (action === "resume") {
      const panel = this.host.querySelector<HTMLElement>(".rg-pause-panel");
      if (panel) panel.hidden = true;
      return;
    }
    if (action === "export") {
      this.options.exportSave();
      this.notice = "A validated field record has been exported.";
      this.render();
      return;
    }
    if (action === "reset-save") {
      if (
        window.confirm(
          "Reset the local RuneGrid record? This cannot be undone."
        )
      ) {
        this.options.resetSave();
        this.notice = "A fresh local field record is ready.";
        this.render();
      }
      return;
    }
    if (action === "replay") {
      this.options.startEncounter(
        control.dataset.mode as GameMode,
        control.dataset.seed
      );
      this.notice = "The archived seed has been rebuilt for inspection.";
      return;
    }
  };

  private readonly handleChange = (event: Event): void => {
    const input = event.target as HTMLInputElement | HTMLSelectElement;
    if (
      input.dataset.action === "import" &&
      input instanceof HTMLInputElement &&
      input.files?.[0]
    ) {
      this.options.importSave(input.files[0]);
      return;
    }
    const setting = input.dataset.setting;
    if (!setting) return;
    const settings = this.options.settings();
    const value =
      input instanceof HTMLInputElement && input.type === "checkbox"
        ? input.checked
        : input instanceof HTMLInputElement && input.type === "range"
          ? Number(input.value)
          : input.value;
    if (setting in settings.accessibility)
      (
        settings.accessibility as unknown as Record<
          string,
          string | number | boolean
        >
      )[setting] = value;
    else
      (settings as unknown as Record<string, string | number | boolean>)[
        setting
      ] = value;
    this.options.persist();
    this.render();
  };

  private seedFor(mode: GameMode): string {
    if (mode === "daily")
      return `daily-${new Date().toISOString().slice(0, 10)}`;
    if (mode === "weekly") {
      const date = new Date();
      return `weekly-${date.getUTCFullYear()}-${Math.ceil((date.getUTCDate() + 6) / 7)}`;
    }
    return `${mode}-${Math.random().toString(36).slice(2, 8).toUpperCase()}`;
  }
}
