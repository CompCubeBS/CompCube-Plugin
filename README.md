# CompCube PCVR plugin

The plugin talks only to the TypeScript CompCube backend. Its Socket.IO contract is handled inside
`Networking/ServerListener.cs`; there is no shared dependency on the retired C# server or the old
`CompCube-Models` repository.

`RoundResultsDurationSeconds` defaults to six seconds in the generated plugin configuration. It must match the backend's `ROUND_RESULTS_SECONDS`; the plugin reports the value during Socket.IO authentication and the backend rejects a mismatch. Round-result packets include the persisted end time so network transit does not eat into the following pick phase.

## Automated builds

Every plugin change and pull request runs the `Build PCVR plugin` GitHub Actions workflow. The
workflow builds against stripped Beat Saber references and verified BeatMods dependencies, then
uploads a DLL for each supported game version to the workflow run's **Artifacts** section. Main-branch artifacts use the release-style names:

- `CompCube-bs1.39.1.dll`
- `CompCube-bs1.40.8.dll`

Non-main pushes, pull requests, and manual runs on non-main refs are instead named
`CompCube-development-bs<game-version>-run-<run-number>-attempt-<attempt>` so developers can download and test them without confusing them with production releases. Actions artifacts are retained for 30 days.

Only a run whose ref is exactly `refs/heads/main` may publish DLLs to the CompCube backend. This applies to both automatic pushes and manual workflow runs; selecting another branch for a manual run never publishes to production. Configure these repository Actions secrets:

- `COMPCUBE_UPLOAD_URL`: the public backend origin, for example `https://api.compcube.net`
- `COMPCUBE_UPLOAD_TOKEN`: the same random value as the backend's `PLUGIN_UPLOAD_SECRET`

Generate the token with `openssl rand -hex 32`. If the secrets are absent, even a main-branch run keeps producing Actions artifacts and safely skips server publishing.

## Building on macOS

The DLL can be cross-compiled on macOS with the .NET 8 SDK and Mono after obtaining the matching
stripped game references and mod dependency DLLs. Set `LocalRefsDir` and `BeatSaberDir` to that
reference directory, then run:

```sh
FrameworkPathOverride=/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.8-api \
dotnet build CompCube.sln --configuration Release \
  -p:GameVersion=1.40.8 \
  -p:LocalRefsDir=/path/to/Refs \
  -p:BeatSaberDir=/path/to/Refs \
  -p:DisableCopyToPlugins=True \
  -p:DisableZipRelease=True
```

Use `1.39.1` and that version's references for the other build. Beat Saber itself is a Windows
PCVR game, so the resulting DLL still needs to be loaded and tested in a Windows installation.
