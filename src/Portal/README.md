# Portal scaffold boundary

This is an empty Arabic RTL React + TypeScript scaffold. Teacher/institution workflows belong to Phase 2. No user data or dashboard features are implemented.

Exact pins and npm lockfile are included: React/React DOM/types 19.3.0, TypeScript 7.0.2, Vite 8.3.5. Package versions were queried from npm; dependencies installed with lifecycle scripts disabled. `npm run build` (including TypeScript checking) and a subsequent clean lockfile install passed using installed Windows Node 22.17.0/npm 10.9.2. The selected future Node baseline 24.22.2 is not installed or validated here.

```sh
npm ci --ignore-scripts
npm run build
npm run dev
```

An empty shell build does not verify browser layout, Arabic shaping, accessibility or Unity text rendering. Native Unity RTL still requires a separate spike.
