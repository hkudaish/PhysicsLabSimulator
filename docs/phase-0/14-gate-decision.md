# Phase 0 gate decision and Phase 1 authorization

**Decision:** Passed for the discovery, architecture and reproducible-scaffold scope defined in Prompt.txt. **Client engine:** Godot 4.7.2 .NET, Compatibility renderer. **Authorization evidence:** the owner requested a Unity alternative, authorized the Godot evaluation, reviewed the running preview by supplying a screenshot, and instructed “go on” after the diagnostic results were explained. This continuation authorizes the first functional prototype; it does not certify scientific or performance results.

| Prompt.txt Phase 0 criterion | Evidence | Assessment |
|---|---|---|
| Requirements reviewed and categorized | Source audit, conflicts and 58-row RTM | Met |
| Architecture documented | 03-architecture, evaluated Godot revision and server/storage boundaries | Met; Godot selected for implementation |
| Chemistry and electricity models defined | 04-scientific-models: equations, units, bounds, errors and reference cases | Met as design; implementation validation belongs to Phase 1 |
| Database architecture established | ERD, SQLite design and PostgreSQL isolation spike | Met as architecture |
| Major risks and dependencies identified | Risk register and recorded external dependencies | Met |
| MVP clearly scoped | Two initial experiments; institutional features reserved for Phase 2 | Met |
| Reproducible environment and builds | Pinned SDKs, locked Godot builds, actual source-independent Windows export and recovery bundle | Met |
| Phase 1 tasks have measurable tests | SCI-C/E cases, P1-01–11 backlog and benchmark protocol | Met |

Earlier internal checklists treated named scientific-reviewer signoff, school-PC availability and Unity-specific execution as blockers to all further work. Scientific review and actual minimum-hardware benchmarks are required validation dependencies for the functional prototype, not evidence that must be invented during planning. The Godot route replaces Unity/IL2CPP checks; a Unity installation is not a prerequisite for the chosen engine. No external signoff, approved minimum hardware, privacy compliance or completed benchmark is claimed.

The development host observed during Godot rendering is a Windows x64 machine with an NVIDIA RTX 4060 Ti. It is a development reference only. A representative school machine remains unselected; low-spec performance claims and the Phase 1 acceptance decision must wait for that evidence.

Phase 1 scope: isolated scientific calculations; two interactive 3D experiments; real instruments; Arabic/English UI; manipulation/camera/undo; local versioned save/resume; automated scientific/persistence/integration tests; Windows executable and installation/run instructions. Independent scientific review, hardware qualification and the 60-minute stability target remain explicit Phase 1 exit requirements. Accounts, portals, licensing and cloud synchronization are not authorized for this phase.
