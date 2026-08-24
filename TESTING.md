# Testing Guide

The test suite protects deterministic field behavior and local-record integrity. Tests are located under `client/src/game/__tests__/` and can run without WebGL because tactical calculations are separated from Babylon rendering.

| Test file             | Coverage                                                                                             |
| --------------------- | ---------------------------------------------------------------------------------------------------- |
| `GridModel.test.ts`   | Weighted pathfinding, obstacle routing, difficult terrain cost, and line-of-sight.                   |
| `Generation.test.ts`  | Reproducible seeded grids and roster placement for a compatible game version.                        |
| `GameSession.test.ts` | Target validation, ability resolution, enemy turn return, action replay capture, and permitted undo. |
| `SaveManager.test.ts` | Valid export/import, local record loading, and malformed-record rejection.                           |

Run the suite with `pnpm test`. Before merging gameplay work, also run `pnpm check` and `pnpm build`. A manual browser test should confirm the following sequence: open an encounter, select a hero, move to a highlighted tile, select an ability, resolve an enemy turn, open pause, leave, reopen an archive/settings screen, and confirm the local record persists after a reload.

Regression test additions should target observable player-risk areas: path legality, action validation, turn ownership, save recovery, seeded reproducibility, and UI escape routes.
