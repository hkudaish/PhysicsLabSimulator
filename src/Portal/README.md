# Portal scaffold boundary

This is an empty Arabic RTL React + TypeScript scaffold. Teacher/institution workflows belong to Phase 2. No user data or dashboard features are implemented.

Exact pins and npm lockfile are included: React/React DOM/types 19.3.0, TypeScript 7.0.2, Vite 8.3.5. Versions were queried from npm; dependency lifecycle scripts remain disabled. Clean installation and TypeScript/production build now pass on the selected isolated Linux Node 24.22.2/npm 11.19.1. Earlier Windows Node 22.17.0 checks are superseded. Use `python3 ../../tools/dev.py npm ...` from this directory to select the pinned tools.

```sh
npm ci --ignore-scripts
npm run build
npm run dev
```

An empty shell build does not verify browser layout, Arabic shaping, accessibility or Unity text rendering. Native Unity RTL still requires a separate spike.
