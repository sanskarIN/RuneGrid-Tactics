# RuneGrid Tactics — Design Directions

## Three Candidate Approaches

### A. Runic Field Manual

**Very Brief Intro:** A weathered expedition folio becomes the game interface: graphite terrain, aged parchment information cards, precise celestial-blue route lines, and measured rune marks. It should feel like a commander consulting a living tactical atlas rather than a fantasy storefront.

**Probability:** 0.06

### B. Prism Sanctuary

**Very Brief Intro:** A bright, crystalline strategy chamber with ivory stone, glassy elemental tokens, and calm scientific diagrams. The intent is contemplative clarity and an approachable tactical experience.

**Probability:** 0.03

### C. Aurora Circuit

**Very Brief Intro:** A dark sci-fantasy command deck uses luminous vector contour lines, metallic panels, and restrained elemental glows. The emotional intent is tense precision under an unfamiliar sky.

**Probability:** 0.08

## Chosen Direction — Runic Field Manual

### Design Movement

**Contemporary cartography and expedition ephemera**, translated into a responsive tactical tabletop. This is not a medieval imitation; it pairs coarse mineral textures and imperfect ink marks with sharp, contemporary wayfinding and data typography.

### Core Principles

1. **Spatial thinking stays visible.** The board, movement contours, threat paths, and tile states are given more visual authority than menus or decoration.
2. **Every cue earns its place.** Color is never the sole state indicator; shape, texture, symbols, and wording reinforce it.
3. **Material contrast guides the eye.** Cool slate board surfaces carry the game state, warm parchment panels carry explanation and planning, and one bright rune color marks decisive actions.
4. **Tactical feedback is deliberate.** Interactions feel physical and measured: tiles rise, paths draw, targets pulse once, and combat results land cleanly without noise.

### Color Philosophy

The battlefield is **basalt charcoal and moss-grey**, which makes a field atlas feel grounded and lets hazards read clearly. Information surfaces are **smoked parchment**, rather than sterile white, to make dense systems feel legible and human. **Rune blue (#56D6E6)** is the distinctive signal color: reserved for the player’s selected route, primary actions, and the idea of an active rune. Fire, frost, storm, nature, arcane, and void use disciplined accent colors that also have symbols and labels.

### Layout Paradigm

The game is built as an **off-centre command table**. The board takes the largest contiguous surface, while a low-slung left command rail and a narrower right field dossier create an asymmetric, grounded frame. On phones, the tactical table remains dominant and the dossiers fold into horizontal trays rather than becoming a generic stacked dashboard.

### Signature Elements

1. **Contour-line frames** subtly trace around active cards, campaign regions, and the battle edge.
2. **Wax-seal runes** identify hero classes, ability schools, and completed objectives in a physical, repeatable visual vocabulary.
3. **Field notes** are small serif annotations that explain a tile, turn, or relic without interrupting the grid.

### Interaction Philosophy

Players should always understand what a tap will do before committing. Selecting a hero opens its reachable field; selecting an ability changes the map into a clear targeting instrument; confirming an action uses a single unmistakable button. Hover and focus reveal tactical detail, while touch controls preserve large targets, visible labels, and no hover-only content.

### Animation

Animations use 120–260 ms custom ease-out transitions. Grid paths draw from origin to destination; selected tiles lift by a few pixels with a rune-blue edge; attacks produce one compact line or arc and a short impact bloom. Panels slide along the battlefield edge instead of floating into the center. Reduced-motion mode removes route-drawing and uses instant state changes with high-contrast outlines. No continuous glow, flashing, or camera shake is required for understanding.

### Typography System

**DM Serif Display** provides the measured, archival voice for campaign names, mission titles, and field-note headings. **Manrope** handles tactical data, buttons, labels, and body copy with high x-height and compact numeric clarity. Mission titles use sentence case, never oversized all caps. Combat numbers use tabular figures. The baseline hierarchy is 12/14 for micro-data, 14/20 for operational labels, 18/24 for section headers, and 30/34 for screen titles.

### Brand Essence

**RuneGrid Tactics is a local-first tactical roguelite for careful planners who want every move to leave a mark.**

**Personality:** measured, elemental, resolute.

### Brand Voice

Headlines are direct and field-oriented; CTAs describe the actual tactical intent; microcopy recognizes uncertainty without becoming theatrical. Avoid generic welcome language or artificial urgency.

Example lines:

> “The ridge is unstable. Build the advantage before it breaks.”

> “Mark a route, read the field, then commit the turn.”

### Wordmark & Logo

The wordmark uses a custom-looking split word composition: **RUNE** in a high-contrast serif, **GRID** in a compact geometric sans, and a small four-point route marker replacing the central separator. The mark is a bold transparent PNG: a squared compass-rune made from four stepped paths that meet around an open center, implying both a tactical grid and an unclaimed route.

### Signature Brand Color

**Routeglass Blue — #56D6E6.** It is the only luminous color used across primary controls, active paths, and the rune-mark brand system.

## Style Decisions

- The battlefield is a tactile cartographic surface, not a generic dark dashboard.
- Rune blue is reserved for player agency and primary actions; it must not become a decorative wash.
- Rounded rectangles are used sparingly: operational controls have clipped or bracketed corners, while panels use subtle paper-like radius.
- No purple gradients, glassmorphism, or generic centered hero layout are permitted.
- Every first screen must show the tactical surface, a smoked-parchment planning layer, and one clear Routeglass Blue route or action mark at the same time.
- Basalt atmosphere must always be paired with visible cartographic structure: contour lines, ink ticks, grid paths, rune seals, or field-note annotations.
- Product introduction always uses the custom split RUNE/GRID wordmark treatment with the compass-rune route marker.
