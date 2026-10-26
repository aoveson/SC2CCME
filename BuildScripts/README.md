# Platform Packaging

Run each script on its matching operating system. The scripts self-contain the .NET app, install the pinned VPK CLI version, and create Velopack assets under `BuildScripts/artifacts/<RID>/releases`.

Prerequisites: .NET 10 SDK and network access to restore NuGet packages and install VPK. Linux packaging also requires `mksquashfs` (`sudo apt install squashfs-tools` on Ubuntu/Debian).

```powershell
./BuildScripts/build-windows.ps1 2.1.0
```

```bash
bash BuildScripts/build-linux.sh 2.1.0
bash BuildScripts/build-macos.sh 2.1.0
```

The macOS script selects `osx-arm64` or `osx-x64` from the host architecture by default. Pass a second argument to override the RID on any script, for example `osx-x64` or `linux-arm64`. For CI, use a native runner for the target OS and architecture because VPK chooses the package format for its host OS.

Each script accepts a 3- or 4-part version, optionally prefixed with `v`, and uses the same Velopack package ID (`SC2CCME`) and VPK CLI version (`1.2.161`). Scripts only build, publish, and pack; a later release job can upload each platform's release directory with `vpk upload github`.

Note: the package version is passed to `dotnet publish`, but the application's displayed version is currently read from `Settings.ApplicationVersion`; that setting still needs its own release-version update strategy.

## Automated Releases

The `release.yml` workflow runs a prerelease build when commits are pushed to `main`. It uses `BASE_VERSION` from the workflow file and appends GitHub's `run_number`, producing versions like `1.0.0.42`. The counter increments per workflow run; a push containing several commits creates one run and increments it once. Manual runs also consume a run number, and changing `BASE_VERSION` does not reset the counter.

Use **Actions > Build and Publish > Run workflow** to select prerelease or stable release and optionally choose `all` or a comma-separated platform subset, such as `windows,linux`. The manual run uses the same version counter.