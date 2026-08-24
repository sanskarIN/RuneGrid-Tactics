# Contributing to RuneGrid Tactics

Thank you for improving RuneGrid Tactics. Contributions should preserve the project’s original identity, local-first stance, accessibility baseline, and data-driven tactical architecture.

Begin from a focused branch such as `feature/terrain-hooks`, `fix/save-validation`, `test/pathfinding-edges`, or `docs/contribution-notes`. Configure repository-local Git identity rather than changing global configuration:

```bash
git config user.name "Sanskar"
git config user.email "sanskarin@outlook.in"
```

Before a pull request, run the full validation sequence described in [BUILDING.md](BUILDING.md). Explain the player-facing outcome, data-model effect, test evidence, and accessibility implication. Avoid commits that only change timestamps, reformat unrelated files, or split one change into artificial fragments.

New gameplay content should be represented through the records in `content.ts` whenever possible. New rules belong in the domain modules; Babylon materials and DOM markup must not be treated as a source of tactical truth. Do not add copyrighted characters, art, music, or maps from existing games. Do not add fabricated reviews, ratings, testimonials, payments, or personally identifiable data collection.

All participants must follow the [Code of Conduct](CODE_OF_CONDUCT.md). Security-sensitive reports follow [SECURITY.md](SECURITY.md).
