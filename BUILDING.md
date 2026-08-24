# Building RuneGrid Tactics

RuneGrid Tactics is a static browser application. The release artifact is the Vite build output and does not require a production database, login service, or gameplay network connection.

| Command       | Purpose                                                |
| ------------- | ------------------------------------------------------ |
| `pnpm dev`    | Starts the Vite development server.                    |
| `pnpm check`  | Runs TypeScript without emitting output.               |
| `pnpm test`   | Runs the complete Vitest regression suite once.        |
| `pnpm build`  | Creates the optimized client and static server bundle. |
| `pnpm format` | Applies project formatting rules.                      |

Run the complete release check in this order:

```bash
pnpm test
pnpm check
pnpm build
```

The browser is the authoritative runtime verification path. Open `/?demo` after a successful build to inspect the deterministic training field. The board must remain visible at desktop and mobile widths, the console must remain free of errors, and the menu must retain a working path into a playable field.

The current project is a browser-ready, Android-first responsive application. Native Android packaging is intentionally outside this repository’s static-host scope; a future packaging layer must preserve the local-first save and accessibility contracts documented here.
