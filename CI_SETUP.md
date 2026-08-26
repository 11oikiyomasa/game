# CI Setup

The repository includes Unity EditMode CI at `.github/workflows/unity-tests.yml`.

## Required GitHub Actions secrets

Configure these repository secrets before expecting the Unity job to execute successfully:

- `UNITY_LICENSE` — Unity license contents accepted by GameCI.
- `UNITY_EMAIL` — Unity account email used for activation.
- `UNITY_PASSWORD` — Unity account password used for activation.

The workflow targets the Unity editor version declared by `ProjectSettings/ProjectVersion.txt`.

## Verification gate

Do not treat the project as build-verified until GitHub Actions reports a completed Unity EditMode run. A workflow file existing in the repository is not evidence that Unity compiled or that tests passed.

The gameplay implementation already has its own board, dice, run, combat, state-machine, and Phase 1 integration tests under `Assets/`. Those systems are the canonical gameplay layer; do not introduce a second parallel domain implementation.

After EditMode is green, add a PlayMode/integration job and then an Android build job. Keep those gates separate so a failure identifies the exact stage that broke.
