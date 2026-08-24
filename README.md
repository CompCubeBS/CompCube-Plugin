# CompCube PCVR plugin

The plugin talks only to the TypeScript CompCube backend. Its Socket.IO contract is handled inside
`Networking/ServerListener.cs`; there is no shared dependency on the retired C# server or the old
`CompCube-Models` repository.

## Automated builds

Every plugin change and pull request runs the `Build PCVR plugin` GitHub Actions workflow. The
workflow builds against stripped Beat Saber references and verified BeatMods dependencies, then
uploads two build artifacts:

- `CompCube-bs1.39.1.dll`
- `CompCube-bs1.40.8.dll`

The workflow uploads Actions artifacts only. It does not create a GitHub release.

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
