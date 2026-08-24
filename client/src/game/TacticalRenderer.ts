/** Runic Field Manual design system: tactile basalt grid, contour overlays, and physical field tokens. */

import { ArcRotateCamera } from "@babylonjs/core/Cameras/arcRotateCamera";
import { HemisphericLight } from "@babylonjs/core/Lights/hemisphericLight";
import { Vector3 } from "@babylonjs/core/Maths/math.vector";
import { Color3, Color4 } from "@babylonjs/core/Maths/math.color";
import { StandardMaterial } from "@babylonjs/core/Materials/standardMaterial";
import { Texture } from "@babylonjs/core/Materials/Textures/texture";
import { MeshBuilder } from "@babylonjs/core/Meshes/meshBuilder";
import { PointerEventTypes } from "@babylonjs/core/Events/pointerEvents";
import { Scene } from "@babylonjs/core/scene";
import type { AbstractMesh } from "@babylonjs/core/Meshes/abstractMesh";
import type { Point, Tile, TileHighlights, Unit } from "./types";
import { GameSession } from "./GameSession";

const BOARD_TEXTURE = "/manus-storage/runegrid-board-texture_445ff281.png";
const TILE_SIZE = 1.1;

interface UnitMesh {
  root: AbstractMesh;
  target: Vector3;
  ring: AbstractMesh;
}

export class TacticalRenderer {
  private readonly tileMeshes = new Map<string, AbstractMesh>();
  private readonly unitMeshes = new Map<string, UnitMesh>();
  private readonly staticMeshes: AbstractMesh[] = [];
  private readonly baseMaterials = new Map<string, StandardMaterial>();
  private readonly overlayMaterials = new Map<string, StandardMaterial>();
  private readonly boardTexture: Texture;
  private disposed = false;

  public constructor(
    private readonly scene: Scene,
    private readonly session: GameSession,
    private readonly onTile: (point: Point) => void
  ) {
    this.scene.clearColor = new Color4(0.035, 0.055, 0.055, 1);
    this.boardTexture = new Texture(BOARD_TEXTURE, scene, true, false);
    this.boardTexture.uScale = 0.42;
    this.boardTexture.vScale = 0.42;
    this.createCameraAndLight();
    this.createField();
    this.sync();
    this.scene.onPointerObservable.add(event => {
      if (event.type !== PointerEventTypes.POINTERDOWN || this.disposed) return;
      const mesh = event.pickInfo?.pickedMesh;
      const point = mesh?.metadata?.fieldPoint as Point | undefined;
      if (point) this.onTile(point);
    });
    this.scene.onBeforeRenderObservable.add(() => this.animateUnits());
  }

  public sync(): void {
    const highlights = this.session.getHighlights();
    for (const row of this.session.encounter.grid) {
      for (const tile of row) this.paintTile(tile, highlights);
    }
    for (const unit of this.session.units) this.syncUnit(unit, highlights);
  }

  public dispose(): void {
    this.disposed = true;
    this.tileMeshes.forEach(mesh => mesh.dispose(false, true));
    this.unitMeshes.forEach(item => {
      item.root.dispose(false, true);
      item.ring.dispose(false, true);
    });
    this.staticMeshes.forEach(mesh => mesh.dispose(false, true));
    this.baseMaterials.forEach(material => material.dispose(false, true));
    this.boardTexture.dispose();
    this.tileMeshes.clear();
    this.unitMeshes.clear();
  }

  private createCameraAndLight(): void {
    const width = this.session.grid.width * TILE_SIZE;
    const height = this.session.grid.height * TILE_SIZE;
    const camera = new ArcRotateCamera(
      "field-camera",
      -Math.PI / 2,
      0.82,
      Math.max(width, height) * 1.65,
      new Vector3(0.12, 0, 0),
      this.scene
    );
    camera.lowerBetaLimit = 0.68;
    camera.upperBetaLimit = 1.0;
    camera.lowerRadiusLimit = Math.max(width, height) * 1.45;
    camera.upperRadiusLimit = Math.max(width, height) * 2.15;
    camera.wheelPrecision = 60;
    camera.panningSensibility = 0;
    this.scene.activeCamera = camera;
    const light = new HemisphericLight(
      "field-sky",
      new Vector3(-0.35, 1, -0.4),
      this.scene
    );
    light.intensity = 1.75;
    light.groundColor = new Color3(0.16, 0.19, 0.17);
  }

  private createField(): void {
    const { width, height } = this.session.grid;
    const frame = MeshBuilder.CreateBox(
      "field-frame",
      {
        width: width * TILE_SIZE + 0.3,
        depth: height * TILE_SIZE + 0.3,
        height: 0.18,
      },
      this.scene
    );
    frame.position.y = -0.17;
    frame.material = this.material("frame", "#28251e", 0.08);
    frame.isPickable = false;
    this.staticMeshes.push(frame);
    this.session.encounter.grid.flat().forEach(tile => {
      const mesh = MeshBuilder.CreateBox(
        `tile-${tile.x}-${tile.y}`,
        { width: TILE_SIZE - 0.035, depth: TILE_SIZE - 0.035, height: 0.13 },
        this.scene
      );
      mesh.position = this.toWorld(tile);
      mesh.metadata = { fieldPoint: { x: tile.x, y: tile.y } };
      this.tileMeshes.set(this.key(tile), mesh);
    });
  }

  private syncUnit(unit: Unit, highlights: TileHighlights): void {
    let item = this.unitMeshes.get(unit.id);
    if (unit.hp <= 0) {
      if (item) {
        item.root.dispose(false, true);
        item.ring.dispose(false, true);
        this.unitMeshes.delete(unit.id);
      }
      return;
    }
    if (!item) {
      item = this.createUnitMesh(unit);
      this.unitMeshes.set(unit.id, item);
    }
    item.target = this.toWorld(unit, 0.28);
    item.ring.material = this.material(
      `ring-${unit.id}`,
      highlights.selected?.x === unit.x && highlights.selected?.y === unit.y
        ? "#56d6e6"
        : unit.faction === "hero"
          ? "#d6c690"
          : "#aa5548",
      0.35
    );
    item.ring.isVisible =
      unit.faction === "hero" ||
      (highlights.selected?.x === unit.x && highlights.selected?.y === unit.y);
  }

  private createUnitMesh(unit: Unit): UnitMesh {
    const target = this.toWorld(unit, 0.28);
    const root = MeshBuilder.CreateCylinder(
      `unit-${unit.id}`,
      {
        height: 0.43,
        diameterTop: 0.72,
        diameterBottom: 0.84,
        tessellation: 12,
      },
      this.scene
    );
    root.position = target.clone();
    root.material = this.material(`unit-${unit.id}`, unit.color, 0.22);
    root.metadata = { fieldPoint: { x: unit.x, y: unit.y } };
    const head = MeshBuilder.CreateSphere(
      `unit-head-${unit.id}`,
      { diameter: 0.27, segments: 12 },
      this.scene
    );
    head.position = new Vector3(target.x, target.y + 0.31, target.z);
    head.parent = root;
    head.position.y = 0.31;
    head.material = this.material(
      `unit-accent-${unit.id}`,
      unit.faction === "hero" ? "#ede2c5" : "#2b2020",
      0.08
    );
    head.isPickable = false;
    if (unit.templateId === "vanguard" || unit.templateId === "stone_brute") {
      const shield = MeshBuilder.CreateBox(
        `unit-marker-${unit.id}`,
        { width: 0.24, height: 0.3, depth: 0.08 },
        this.scene
      );
      shield.parent = root;
      shield.position = new Vector3(0, 0.12, -0.34);
      shield.material = this.material(`unit-marker-${unit.id}`, "#38434b", 0.1);
      shield.isPickable = false;
    } else {
      const marker = MeshBuilder.CreateCylinder(
        `unit-marker-${unit.id}`,
        { height: 0.34, diameter: 0.06, tessellation: 8 },
        this.scene
      );
      marker.parent = root;
      marker.rotation.z = -0.45;
      marker.position = new Vector3(0.18, 0.15, -0.12);
      marker.material = this.material(
        `unit-marker-${unit.id}`,
        unit.element === "arcane" ? "#56d6e6" : "#e0c482",
        0.08
      );
      marker.isPickable = false;
    }
    const ring = MeshBuilder.CreateTorus(
      `unit-ring-${unit.id}`,
      { diameter: 0.92, thickness: 0.045, tessellation: 24 },
      this.scene
    );
    ring.rotation.x = Math.PI / 2;
    ring.position = new Vector3(target.x, 0.08, target.z);
    ring.isPickable = false;
    return { root, target, ring };
  }

  private paintTile(tile: Tile, highlights: TileHighlights): void {
    const mesh = this.tileMeshes.get(this.key(tile));
    if (!mesh) return;
    const reachable = highlights.reachable.some(
      point => point.x === tile.x && point.y === tile.y
    );
    const target = highlights.targets.some(
      point => point.x === tile.x && point.y === tile.y
    );
    const danger = highlights.danger.some(
      point => point.x === tile.x && point.y === tile.y
    );
    const selected =
      highlights.selected?.x === tile.x && highlights.selected?.y === tile.y;
    const color = target
      ? "#b65e52"
      : reachable
        ? "#2c8794"
        : selected
          ? "#56d6e6"
          : danger
            ? "#5e3933"
            : this.tileColor(tile.kind);
    mesh.material = this.material(
      `tile-${tile.x}-${tile.y}-${color}`,
      color,
      target || reachable || selected ? 0.38 : 0.04
    );
    mesh.position.y =
      tile.kind === "wall" ? 0.32 : tile.kind === "gate" ? 0.22 : 0;
    mesh.scaling.y =
      tile.kind === "wall"
        ? 3.5
        : tile.kind === "gate"
          ? 2.4
          : tile.kind === "destructible"
            ? 1.7
            : 1;
  }

  private tileColor(kind: Tile["kind"]): string {
    return {
      floor: "#243236",
      wall: "#425054",
      difficult: "#4d5140",
      healing: "#496957",
      hazard: "#7b5239",
      teleport: "#275d70",
      gate: "#5e4735",
      destructible: "#625045",
    }[kind];
  }

  private material(name: string, hex: string, alpha: number): StandardMaterial {
    const cached = this.baseMaterials.get(name);
    if (cached) return cached;
    const material = new StandardMaterial(name, this.scene);
    material.diffuseColor = Color3.FromHexString(hex);
    material.specularColor = new Color3(0.12, 0.13, 0.12);
    material.alpha = alpha < 0.2 ? 1 : Math.min(1, alpha + 0.65);
    if (
      name.startsWith("tile-") &&
      !name.includes("#2c8794") &&
      !name.includes("#b65e52") &&
      !name.includes("#56d6e6")
    ) {
      material.diffuseTexture = this.boardTexture;
      material.diffuseTexture.hasAlpha = false;
    }
    this.baseMaterials.set(name, material);
    return material;
  }

  private animateUnits(): void {
    const delta = Math.min(this.scene.getEngine().getDeltaTime() / 120, 1);
    for (const item of Array.from(this.unitMeshes.values())) {
      item.root.position = Vector3.Lerp(item.root.position, item.target, delta);
      item.ring.position.x = item.root.position.x;
      item.ring.position.z = item.root.position.z;
    }
  }

  private toWorld(point: Point, y = 0): Vector3 {
    return new Vector3(
      (point.x - (this.session.grid.width - 1) / 2) * TILE_SIZE,
      y,
      ((this.session.grid.height - 1) / 2 - point.y) * TILE_SIZE
    );
  }

  private key(point: Point): string {
    return `${point.x}:${point.y}`;
  }
}
