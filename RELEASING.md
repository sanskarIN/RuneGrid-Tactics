# Releasing RuneGrid Tactics

Create releases from a clean branch only after tests, type checks, production build, and browser visual verification succeed. Do not include signing secrets, private tokens, player records, or unlicensed assets in the repository.

| Step | Release check                                                                                                                                                                         |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1    | Update `CHANGELOG.md`, `what_changed.md`, version metadata, and user-facing documentation.                                                                                            |
| 2    | Run `pnpm test`, `pnpm check`, and `pnpm build`.                                                                                                                                      |
| 3    | Open `/?demo` and a normal menu session at desktop and mobile widths. Check field visibility, readable commands, empty-state handling, screen navigation, and browser console output. |
| 4    | Create a descriptive Git commit and push the release branch.                                                                                                                          |
| 5    | Create a release tag after reviewing the GitHub Actions status.                                                                                                                       |

The current release line is `0.1.x`, representing the browser gameplay foundation. Future mobile packaging should receive a separate release process that verifies interruption recovery, memory behavior, permissions, store assets, and platform policy compliance.
