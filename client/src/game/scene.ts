/** Runic Field Manual design system: scene composition binds Babylon’s board to a local-first command interface. */

import { Scene } from "@babylonjs/core/scene";
import type { Engine } from "@babylonjs/core/Engines/engine";
import { ArcRotateCamera } from "@babylonjs/core/Cameras/arcRotateCamera";
import { Vector3 } from "@babylonjs/core/Maths/math.vector";
import { createEncounter } from "./content";
import { GameSession } from "./GameSession";
import { GameUIController } from "./GameUIController";
import { ProgressionService } from "./Progression";
import { SaveManager } from "./SaveManager";
import { TacticalRenderer } from "./TacticalRenderer";
import { AudioManager } from "./AudioManager";
import type { AppSettings, GameMode, SaveData } from "./types";

export interface GameHandle {
  scene: Scene;
  dispose: () => void;
}

export async function createGameScene(
  engine: Engine,
  canvas: HTMLCanvasElement,
  uiHost: HTMLElement
): Promise<GameHandle> {
  const scene = new Scene(engine);
  const atlasCamera = new ArcRotateCamera(
    "atlas-idle-camera",
    -Math.PI / 2,
    0.8,
    12,
    Vector3.Zero(),
    scene
  );
  scene.activeCamera = atlasCamera;
  let save: SaveData = SaveManager.load();
  let progression = new ProgressionService(save.profile);
  let currentSession: GameSession | undefined;
  let renderer: TacticalRenderer | undefined;
  let detachRenderer: (() => void) | undefined;
  let outcomeRecorded = false;
  const audio = new AudioManager(() => settings());

  const persist = (): void => {
    save.profile = progression.profile;
    save.settings = settings();
    save.lastEncounter = currentSession?.encounter;
    save = SaveManager.save(save);
  };
  const settings = (): AppSettings => save.settings;
  const clearField = (): void => {
    detachRenderer?.();
    currentSession?.dispose();
    renderer?.dispose();
    detachRenderer = undefined;
    renderer = undefined;
    currentSession = undefined;
  };
  const startEncounter = (mode: GameMode, seed?: string): void => {
    clearField();
    const fieldSeed =
      seed ?? `${mode}-${Math.random().toString(36).slice(2, 8).toUpperCase()}`;
    const encounter = createEncounter(fieldSeed, mode, settings().difficulty);
    currentSession = new GameSession(encounter);
    audio.play("ui");
    outcomeRecorded = false;
    for (const hero of currentSession.livingHeroes)
      progression.recordHeroUse(hero.templateId);
    renderer = new TacticalRenderer(scene, currentSession, point =>
      currentSession?.chooseTile(point)
    );
    detachRenderer = currentSession.subscribe(event => {
      renderer?.sync();
      if (event.type === "unit")
        audio.play(
          currentSession?.selectedAbilityId
            ? "ability"
            : currentSession?.phase === "enemy"
              ? "enemy"
              : "move"
        );
      if (event.type === "outcome")
        audio.play(currentSession?.phase === "victory" ? "victory" : "defeat");
      if (event.type === "outcome" && currentSession && !outcomeRecorded) {
        outcomeRecorded = true;
        const pristine = currentSession.livingHeroes.every(
          hero => hero.hp >= Math.ceil(hero.maxHp / 2)
        );
        progression.recordBattle(
          encounter,
          currentSession.phase === "victory" ? "victory" : "defeat",
          currentSession.turn,
          pristine
        );
        progression.profile.replays.unshift(currentSession.createReplay());
        progression.profile.replays = progression.profile.replays.slice(0, 25);
      }
      persist();
    });
    ui.setSession(currentSession);
    currentSession.start();
    persist();
  };
  const exportSave = (): void => {
    const payload = SaveManager.export({
      ...save,
      profile: progression.profile,
      lastEncounter: currentSession?.encounter,
    });
    const url = URL.createObjectURL(
      new Blob([payload], { type: "application/json" })
    );
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = `runegrid-field-record-${new Date().toISOString().slice(0, 10)}.json`;
    anchor.click();
    window.setTimeout(() => URL.revokeObjectURL(url), 0);
  };
  const importSave = (file: File): void => {
    void file.text().then(raw => {
      try {
        save = SaveManager.import(raw);
        progression = new ProgressionService(save.profile);
        clearField();
        ui.setSession(undefined);
      } catch {
        window.alert(
          "This field record could not be imported. Your existing local record remains unchanged."
        );
      }
    });
  };
  const resetSave = (): void => {
    clearField();
    save = SaveManager.reset();
    progression = new ProgressionService(save.profile);
    ui.setSession(undefined);
  };
  const ui = new GameUIController(uiHost, {
    session: () => currentSession,
    progression: () => progression,
    settings,
    startEncounter,
    restartEncounter: () => {
      if (currentSession)
        startEncounter(
          currentSession.encounter.mode,
          currentSession.encounter.seed
        );
    },
    leaveEncounter: () => {
      clearField();
      ui.setSession(undefined);
      persist();
    },
    persist,
    exportSave,
    importSave,
    resetSave,
  });

  const onKeyboard = (event: KeyboardEvent): void => {
    if (!currentSession || currentSession.phase !== "player") return;
    if (event.key === "Enter") {
      currentSession.endTurn();
      return;
    }
    if (event.key.toLowerCase() === "z") currentSession.undo();
  };
  window.addEventListener("keydown", onKeyboard);

  if (new URLSearchParams(window.location.search).has("demo")) {
    window.setTimeout(() => {
      startEncounter("training", "DEMO-ATLAS");
      window.setTimeout(() => {
        currentSession?.selectUnit("hero-ranger");
        currentSession?.chooseTile({ x: 2, y: 5 });
      }, 500);
    }, 450);
  }

  return {
    scene,
    dispose: () => {
      window.removeEventListener("keydown", onKeyboard);
      clearField();
      ui.dispose();
      audio.dispose();
      scene.dispose();
    },
  };
}
