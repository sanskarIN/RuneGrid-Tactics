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

const presets = await readFile(join(root, "export_presets.cfg"), "utf8");
for (const target of ["Windows Desktop", "Linux/X11", "Android"]) {
  if (!presets.includes(`name="${target}"`)) throw new Error(`Missing export preset: ${target}`);
}

console.log(`Godot project structure and ${jsonFiles.length} JSON content files validated.`);
