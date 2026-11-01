# Local recovery bundles

Git metadata lives in `artifacts/source-control.git` because the environment protects `.git`. A local bundle can preserve history independently of that artifacts folder:

```sh
bash tools/git.sh bundle create checkpoints/phase0-source.bundle --all
bash tools/git.sh bundle verify checkpoints/phase0-source.bundle
```

Bundle files are intentionally ignored to avoid committing repository history into itself. Keep the bundle if cleaning artifacts. To recover in an unrestricted new directory, use `git clone <absolute-path-to-bundle> <new-directory>`. No remote or external upload is involved.
