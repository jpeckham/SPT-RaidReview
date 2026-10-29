> Current local branch: EFT **1.3.0.0.48484**, SPT 5.0, `spt-5.0-eft-1.3.0.0.48484`. The 48146 branch and installation remain preserved.

## October 29, 2026: 48484 rebuild

Build projects now default to `D:\sptarkov\SPTarkov 5.0.0-BE-1.3.0.0.48484`. Use .NET 10 and generated references from that installation; existing build commands below accept `-p:SPTPath` to override the target. Build with `-c Release -p:SkipDeploy=true`, then install the staged plugin directory. Do not use old 48146 references for this branch.

The menu toggle explicitly converts its UnityAction<bool> callback; the optional SAIN GetComponent fallback converts System.Type with Il2CppType.From. SAIN is not installed or runtime-validated here. Existing local fixes and mod versions are retained. Installation and validation evidence are recorded in the parent SPT-BE README, section N8.

## Historical 48146 port notes

> Locked baseline: EFT **1.2.5.0.48146**, SPT 5.0. Newer EFT work must use a separate branch and installation. The original `spt-5.0-48146` branch remains available as historical provenance.

# SPT Raid Review

## Local SPT 5.0 / EFT 48146 port

Our [spt-5.0-eft-1.2.5.0.48146 branch](https://github.com/jpeckham/SPT-RaidReview/tree/spt-5.0-eft-1.2.5.0.48146) ports Chazut's 4.1.X commit `2567b3729437cf71248bcec607bdfb8f1393828e` (RaidReview 1.9.0) to the local SPT 5.0 installation. Client and server identify this experimental fork as **2.0.0**. No existing 5.0 port was found in the upstream branches/PRs or the two other listed forks checked. This is not an official release or a guarantee for other EFT builds.

### Build this branch

Requires .NET 10 SDK, Node/npm, and an initialized SPT 5.0 installation with generated IL2CPP assemblies for **EFT 1.2.5.0.48146**. From the repository root:

```powershell
npm --prefix Private ci --no-audit --no-fund
npm --prefix Private run build
dotnet build ServerMod/ServerMod.csproj -c Release -p:SkipDeploy=true `
  '-p:SPTPath=C:\sptarkov\SPTarkov 5.0.0-BE-1.2.5.0.48146'
dotnet build Client/RAID-REVIEW.csproj -c Release `
  '-p:SPTPath=C:\sptarkov\SPTarkov 5.0.0-BE-1.2.5.0.48146'
```

The frontend must be built before the server because its assets are embedded in `RaidReview.dll`. The committed npm lockfile has been repaired: upstream's omitted esbuild platform entries initially made `npm ci` fail. Dependency installation uses that repaired lockfile. The server references SPT assemblies from the selected installation instead of the 4.1.2 NuGet packages; the client likewise references its core/interop assemblies. Game DLLs are not included in our changes.

### Install, preserve data, and use

Close the client/server and back up the test profile plus any existing RaidReview folders before updating. Copy the contents of `Client/Build/BepInEx/plugins/RaidReview` to the installation's `BepInEx/plugins/RaidReview`: `RAID_REVIEW.dll`, managed `Newtonsoft.Json.dll`, managed `websocket-sharp.dll`, and `LICENSE`. Do not substitute the game's native interop copies of those JSON/WebSocket libraries. Avoid duplicate older RaidReview plugin DLLs elsewhere.

Deploy the server with the explicit destination:

```powershell
dotnet build ServerMod/ServerMod.csproj -c Release -p:SkipDeploy=false `
  '-p:SPTPath=C:\sptarkov\SPTarkov 5.0.0-BE-1.2.5.0.48146' `
  '-p:SptModDirectory=C:\sptarkov\SPTarkov 5.0.0-BE-1.2.5.0.48146\SPT_Runtime\user\mods\RaidReview'
Copy-Item LICENSE 'C:\sptarkov\SPTarkov 5.0.0-BE-1.2.5.0.48146\SPT_Runtime\user\mods\RaidReview'
```

Normal builds default to `SkipDeploy=true`. The deployment target preserves an existing config. Keep `data/` and `config/` when upgrading. Only the server mod DLL belongs at its root; SQLite managed dependencies stay in `lib/sqlite`, and the native provider stays under `runtimes/win-x64/native`. Do not copy the entire server build directory to the installed mod root.

Start SPT's server and client normally. **F5** opens the review page; you can also browse to **http://127.0.0.1:7829** while the server is running. Recording uses WebSocket port **7828**. Client settings are in `BepInEx/config/ekky.raidreview.cfg`; server settings are in `SPT_Runtime/user/mods/RaidReview/config/config.json`. Recording defaults on. Server usage telemetry defaults off. Upstream defaults automatically keep the latest 30 raids and remove unfinished recordings; adjust the server config if you want different retention.

To disable, close the client/server, remove or move the client RaidReview plugin folder, and move the server RaidReview folder outside `user/mods`. Preserve its data/config for future use. Restoring an older mod requires its matching client/server pair and your backup, not mixing versions.

### 2.0.1 follow-up fixes and diagnostics

- PMC display faction now follows known PMC roles even when SPT 5 reports Savage. Explicit enum string serialization and server response normalization also fix historical raid labels without rewriting database records. Boss guards remain Followers, matching upstream.
- Starting inventory now includes the human player at raid start and other humans on discovery. Historical missing player spawn values cannot be recovered from later saves. Existing price/quantity and slot rules apply.
- Sustained low FPS on Shoreline is **still under investigation**. Added separate performance stages for spawn inventory, loose loot, and participant sampling. Enable `Performance logging` to obtain `[RR-PERF]` reports every ten seconds; these include overlapping main-thread stages, managed allocations, and sender backlog. Native allocations are not included. The local installation has this enabled for diagnosis; no FPS improvement is claimed.
- Regression suites: RecordingPipeline 1182, HeadlessPresentation 56, BehaviorDecisions 68, SwitchRecording 23, RaidTransfer 289, SQLiteIsolation 14 passed. Both builds passed with zero warnings/errors. Installed API verified nine historical PMCs corrected and four guards preserved; database player rows unchanged. Client startup/WebSocket passed. Human spawn capture and FPS require the next in-raid test.

### Port changes and verification

- BepInEx 6 IL2CPP plugin entry point with an injected capture component and wrapped coroutine; updated patch namespace and plugin discovery. Retained optional dependency declarations.
- Native loot/switch/player casts, pointer-based player identity, native owner-property reflection, and the current shot shooter-profile API. Menu traversal uses indexed native children. F5 is checked once per frame to avoid repeated browser tabs from polling a held key.
- Managed JSON/WebSocket dependencies packaged with the client. ORBIT API access is bound at runtime so building the plugin does not require ORBIT's Mono DLL; existing optional integrations are retained but **not validated against 5.0 versions**.
- Server metadata targets SPT 5.0; storage implementation and isolated SQLite layout retained. No game/runtime component changes were required.

Both .NET builds passed with **0 warnings and 0 errors**. The frontend passed TypeScript/Vite build, with the existing large-bundle warning. Six test suites passed: RecordingPipeline **1172**, BehaviorDecisions **68**, HeadlessPresentation **45**, SwitchRecording **23**, RaidTransfer **289**, and SqliteIsolation `alone` **14** checks. Recording/switch test doubles were updated for native wrapper boundaries; these are not substitutes for in-game recording tests.

Run the first five using `dotnet run --project Tests/<suite> -c Release`. For storage:

```powershell
dotnet run --project Tests/SqliteIsolation -c Release -- alone `
  "$PWD/ServerMod/bin/Release/RaidReview" `
  'C:\sptarkov\SPTarkov 5.0.0-BE-1.2.5.0.48146\SPT_Runtime'
```

Live startup confirmed all **17 patches**, the capture component, WebSocket connection, database initialization, frontend HTTP 200, and an empty `/api/raids` response. Tests used LocalBuildTest, with its save backed up; test processes were stopped afterward. Final whitespace-only cleanup was rebuilt after the last startup check.

**Not established:** a completed raid captured/replayed end to end; correct positions, kills, loot, switches, airdrops, or Fika/AI-mod telemetry in gameplay; visual behavior of the optional menu button. Test a short raid before relying on recordings. No completed gameplay recording was claimed from these startup checks. Original 4.1 documentation below remains historical where it differs from this section.

---

**Raid Review 1.9.0**: [Full changelog](CHANGELOG.md)

![Stars](https://img.shields.io/github/stars/Chazut/SPT-RaidReview?style=flat-square&label=STARS&color=007ec6)
![Issues](https://img.shields.io/github/issues/Chazut/SPT-RaidReview?style=flat-square&label=ISSUES&color=44cc11)
![Downloads](https://img.shields.io/github/downloads/Chazut/SPT-RaidReview/total?style=flat-square&label=DOWNLOADS&color=44cc11)

Every raid you play gets recorded: positions, kills, loot, bot decisions.
After extract, replay the whole thing on an interactive map, scrub the
timeline, and find out what actually happened - who hunted you across the
map, what that scav was thinking, where the good loot you missed was
sitting.

[Watch Preview](https://spt-raid-review.pages.dev/m2-res_1080p.mp4)

Once installed, play a few raids, then hit **F5** in game (or browse to
`http://127.0.0.1:7829`).

---

### Installation

Download a compatible version from the **[releases](https://github.com/Chazut/SPT-RaidReview/releases)** tab. Instructions for each version are included in the corresponding release description.

- **SPT 4.1.X**: version 1.5.0 and newer.
- **SPT 4.0.X**: versions up to 1.4.x.

To upgrade, close the server and game, then extract the full ZIP over your installation
and accept file replacements. Keep your `data/` and `config/` folders. The ZIP preserves
your configuration and automatically replaces the old root SQLite dependencies with
harmless upgrade placeholders. Raid Review archives those placeholders on startup;
its actual SQLite dependencies live in `lib/sqlite/`, isolated from other mods.

---

### Features

- Full raid replay on an interactive map (timeline scrubber, follow mode, floors)
- **Selected floor only** filters replay markers, movement trails, loot, objectives, shots, switches and the heatmap using each map's height and building boundaries. Off by default, remembered in the browser. Unsupported maps remain unfiltered; records without a usable height, including historical 2D field zones, are hidden while filtering.
- Switch replay: the **Switches** toggle shows initial states and later changes on the map. Markers follow the timeline, including rewind and automatic shutoff; hover to see the name, state, height and change time. Requires a new recording.
- Main objective markers stay above switch icons. Updated ORBIT extract camps show that they run last and hold until time-based extraction; older recordings keep their recorded timers.
- Airdrop replay: aircraft icons follow the recorded flight path, cyan diamonds mark release points, and yellow crates appear on landing. All follow the replay timeline and the **Airdrops** toggle. Requires a new recording; remote clients distinguish a crate first observed in flight from a confirmed release.
- With the updated ORBIT client, **ORBIT Main Objectives** also shows a temporary **Airdrop** row from the squad's decision: approach, waiting (including the crate's descent), then loot. Select a squad to see its current stage and target. The row disappears when the detour ends and follows replay rewinds; older recordings remain supported.
- Kill lines, death markers, killfeed details
- Post-raid leaderboard
- Charts: kills, looting, bots over time, factions...
- Bot behavior rings and per-bot timeline
- Loose loot and bot inventories on the map
- Grenade trajectories, ballistics, hit flashes
- Mod integrations: SAIN, ORBIT (ghosts included), QuestingBots, LootingBots
- Ghost hearing replay: enable **Ghost hearing** to see a small ear icon beside Ghost squad members for eight seconds after a recorded decision to investigate a sound. Hover or focus an icon to reveal the sound location and a dotted connection. Ignored sounds stay hidden. Requires hearing decisions recorded by compatible ORBIT and RaidReview clients; older raids without those events remain unchanged.
- Faction badges for modded bots: UNTAR, RUAF, Black Division, ISB, Combine Soldiers
- ISB White Tusks have a separate **ISB WT** display group and **WT** map markers,
  including the March squad and original commanders. Recorded faction and bot
  behaviour are unchanged. Older replays are recognized when their saved role or
  ISB display name identifies a White Tusk.
- Fika support, see [REMOTE_HOST_AND_FIKA.md](REMOTE_HOST_AND_FIKA.md)
- Headless recordings show the human player's name followed by `(headless)` and derive PMC/SCAV from the recorded human participants. Scav players retain their human identity and use their main profile nickname when available. Update both the client plugin on the headless host and the server mod; older raids that recorded these players as bots cannot be reliably relabeled.

---

### Share a raid

Open a completed raid and click **Export raid** to download a ZIP. In another Raid Review installation, click **Import raid** on the Recent Raids page and select that ZIP.

The archive contains the recorded events, replay positions, map variant and available ORBIT data. It includes recorded player/bot names, but no SPT profile save, server configuration or credentials. Both installations need a version with raid sharing support.

Imported raids are labelled **Imported** and remain until you delete them from their Settings page. They do not count toward automatic raid cleanup. Importing the same raid again opens the existing recording without replacing it. If the original recording has no positions, RR reports that the movement replay is unavailable.

Transfers support ZIPs up to 512 MB, with up to 2 GB of uncompressed data. Use ZIPs created by **Export raid**; an ORBIT debug ZIP is a different format.

### Project Structure

| Component | Path | Tech | Purpose |
|-----------|------|------|---------|
| **Client** | `/Client` | C# .NET 4.7.2 / BepInEx | Game plugin: patches EFT methods, streams data via WebSocket |
| **Server** | `/ServerMod` | C# .NET 10 / Kestrel | SPT 4.1 server mod: WebSocket + HTTP server, SQLite storage |
| **Frontend** | `/Private` | React 18 + TypeScript + Vite | Web UI for raid review and replay |

---

### Contributions & Development

- [CONTRIBUTING.md](CONTRIBUTING.md): how to contribute.
- [DEVELOPMENT.md](DEVELOPMENT.md): getting started with development.
- [REMOTE_HOST_AND_FIKA.md](REMOTE_HOST_AND_FIKA.md): using the mod on a remote host or with Fika.
- [TELEMETRY.md](TELEMETRY.md): information on statistics collection.

---

### Credits / Thanks

- **Ekky**: original author.
- **Chazut**: SPT 4.0.X and 4.1.X migrations, current maintainer.
- The SPT team for the framework and docs.
- The SPT Discord, especially the `#mod-development` and `#dev-community` folks.
- The team behind [tarkov.dev](https://tarkov.dev) for open-sourcing the interactive map.
- Olli, Stk2008, and others who helped squash bugs.
- You, for downloading and trying the mod.

---

### Support

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/chazut)
