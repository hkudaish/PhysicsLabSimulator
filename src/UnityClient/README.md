# Unity URP scaffold — editor verification pending

Project source comes from Unity's official `com.unity.template.urp-blank` 17.1.0 archive, verified by registry SHA-1 and pinned SHA-256. Supplier Assets, Packages and ProjectSettings are present; supplier caches are excluded. Editor 6000.3.27f2/revision 22188f149405 is pinned from official release metadata. The supplier license is in ThirdParty and provenance is recorded in bootstrap-provenance.json.

No editor import or build occurred. Before the Phase 0 gate: import/resolve with the selected licensed editor, review regenerated package locks, build/run empty Windows x64 scaffold, run native Arabic and SQLite/IL2CPP spikes, and import Scientific.Core. The entry point `VirtualStem.EditorTools.Phase0Build.BuildWindows` is prepared but uncompiled in Unity. See [the handoff](../../docs/phase-0/12-unity-validation.md). No laboratory interaction or solver behavior exists.
