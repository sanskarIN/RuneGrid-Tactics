import { readFile, stat } from "node:fs/promises";
import { join, resolve } from "node:path";

const root = resolve(process.argv[2] ?? new URL("..", import.meta.url).pathname);
const jsonFiles = ["heroes.json", "enemies.json", "abilities.json", "items.json", "levels.json", "balance.json"];
const required = ["project.godot", "RuneGrid.Tactics.csproj", "Scenes/Main.tscn", "Scripts/Godot/GameRoot.cs", "Scripts/Core/GameSession.cs", "export_presets.cfg"];

for (const relative of required) {
  await stat(join(root, relative));
}

for (const file of jsonFiles) {
  const parsed = JSON.parse(await readFile(join(root, "Data", file), "utf8"));
  if (!parsed || (Array.isArray(parsed) && parsed.length === 0)) throw new Error(`Invalid or empty JSON content: ${file}`);
}

const heroes = JSON.parse(await readFile(join(root, "Data", "heroes.json"), "utf8"));
const enemies = JSON.parse(await readFile(join(root, "Data", "enemies.json"), "utf8"));
const abilities = JSON.parse(await readFile(join(root, "Data", "abilities.json"), "utf8"));
const abilityIds = new Set(abilities.map((ability) => ability.id));
const unitClasses = new Set(["Vanguard", "Channeler", "Pathfinder", "Warden", "Duelist", "Runesmith", "Seer", "Skywarden", "Sapper", "Sentinel", "Harrier", "Stalker", "Artillery", "Support"]);

for (const unit of [...heroes, ...enemies]) {
  if (!unit.mobility || !unit.tacticalClass || !unitClasses.has(unit.tacticalClass)) throw new Error(`Missing or invalid tactical metadata: ${unit.id}`);
  for (const abilityId of unit.abilityIds ?? []) {
    if (!abilityIds.has(abilityId)) throw new Error(`Unknown ability ${abilityId} referenced by ${unit.id}`);
  }
}

for (const id of ["duelist", "runesmith", "seer", "skywarden", "iron_sentinel", "gale_harrier", "cinder_artillery", "shade_stalker"]) {
  if (![...heroes, ...enemies].some((unit) => unit.id === id)) throw new Error(`Missing expanded unit class: ${id}`);
}

const gridSource = await readFile(join(root, "Scripts", "Core", "TacticalGrid.cs"), "utf8");
const sessionSource = await readFile(join(root, "Scripts", "Core", "GameSession.cs"), "utf8");
for (const feature of ["FindTacticalRoute", "FindBestApproach", "FindFlankAnchors", "BuildReservations"]) {
  if (!gridSource.includes(feature)) throw new Error(`Missing advanced grid feature: ${feature}`);
}
if (!sessionSource.includes("ReserveSuggestedRoute")) throw new Error("Native session does not expose tactical route reservations.");

const presets = await readFile(join(root, "export_presets.cfg"), "utf8");
for (const target of ["Windows Desktop", "Linux/X11", "Android"]) {
  if (!presets.includes(`name="${target}"`)) throw new Error(`Missing export preset: ${target}`);
}

console.log(`Godot project structure, ${jsonFiles.length} JSON content files, expanded unit classes, and advanced tactical routing validated.`);
