# PlatinumSharp

PlatinumSharp is a hand-written C# port of **Pokémon Platinum (Europe)** for the Nintendo DS.

It is not an emulator. The game's ARM9 binary and overlays are translated function by function into C#, and they run on a small custom engine that reimplements the DS hardware the game depends on: 2D/3D rendering, OAM, DMA, timers, sound and the touch screen. Game data such as graphics, maps, scripts, text and music is read directly from your own ROM at runtime.

> **No game data is included.** You need your own dump of *Pokémon - Platinum Version (Europe)*. This project is not affiliated with or endorsed by Nintendo, Game Freak or The Pokémon Company.

## Status

The port is a work in progress. The game boots, plays the opening and new-game intro, and the overworld, battles, menus, Pokétch and many script commands work. Large parts of the game are still unported.

- Roughly 11,000 functions have been ported so far. Each one is marked with the address of the original function (see [Conventions](#conventions)).
- When the game reaches code that hasn't been ported yet, it stops with a `NotImplementedException` that names the missing function's address, for example `Function 0x02040D08 is not ported yet`.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows for the graphical programs (WinForms + NAudio). `Game`, `Nitro` and the command-line `Tool` target plain `net8.0`.
- A Pokémon Platinum (Europe) ROM (`.nds`)

## Building

```sh
dotnet build PlatinumSharp.sln -c Release
```

## Projects

| Project | Output | Purpose |
|---|---|---|
| `Game` | `PlatinumSharp.Game.dll` | The ported game code (ARM9, overlays, field, battle, menus) and the engine it runs on |
| `Nitro` | `PlatinumSharp.Nitro.dll` | ROM and file formats: NDS file system, NARC, LZ compression, graphics and sound data |
| `Runner` | `PlatinumSharp.exe` | Plays the game in a window with sound |
| `Tool` | `PlatinumSharp.Tool.exe` | Command-line helper: headless boots, scripted test runs, data dumps and verification commands |
| `ScriptPlayer` | `PlatinumScriptPlayer.exe` | Pick a map and a field script and watch it run in the game, with pause and step controls |
| `Viewer` | `PlatinumSharp.Viewer.exe` | Browse the ROM's file system and archives, and play its music |
| `Preview` | `PlatinumSharp.Preview.exe` | Preview Pokémon sprites, graphics archives and sound |

## Running

Start the game and pick your ROM in the file dialog, or pass the ROM path on the command line:

```sh
Runner/bin/Release/net8.0-windows/PlatinumSharp.exe "path/to/Pokemon - Platinum Version (Europe).nds"
```

Default controls (keys can be rebound in the window):

| DS | Key |
|---|---|
| D-Pad | Arrow keys |
| A / B | X / Z |
| X / Y | S / A |
| L / R | Q / W |
| Start / Select | Enter / Backspace |
| Touch screen | Mouse on the lower screen |

Runner options for scripted or automated use: `--screenshot <png>`, `--frames <n>`, `--press <frame:keymask> ...`, `--wav <file>`, `--mute`, `--speed <n>`, `--script <file> [--state <name>]`, `--result <file>`.

### Headless runs

`Tool` can boot the game without a window, which is useful for tests and for finding the next unported function:

```sh
PlatinumSharp.Tool.exe boot "<rom>" <frames> nodump [frame:keymask ...] [script=<test script>] [state=<checkpoint>]
```

For example, `boot "<rom>" 3000 nodump 620:8` boots for 3000 frames and presses Start at frame 620 to skip the opening movie. It prints which function stopped the run, or confirms that it ran without reaching unported code.

## Workspace layout and paths

The test tooling (test scripts, save-state checkpoints, screenshots, explorer memory, the progress dashboard) lives next to the repository, not inside it. Paths are worked out at runtime by `Game/Engine/ProjectPaths.cs`:

```
<workspace>/
├── PlatinumSharp/        this repository
├── tools/
│   ├── scriptdata.json   decoded script listings (used by ScriptPlayer and coverage)
│   └── tests/            test scripts, states/, shots/, explorer memory and log
├── dashboard/live/       live frame and status written by headless runs
├── dumps/                Runner debug dumps
└── *.nds                 your ROM (found automatically for ScriptPlayer)
```

The workspace is the parent folder of the repository. Builds copied elsewhere in the workspace also find it, because the resolver looks for `PlatinumSharp/PlatinumSharp.sln`. You can override this with environment variables:

| Variable | Meaning |
|---|---|
| `PLATINUMSHARP_WORKSPACE` | Workspace folder (default: parent of the repository) |
| `PLATINUMSHARP_ROM` | ROM path used when none is given on the command line |
| `EXPLORE_MEMORY` | Explorer memory file (default: `tools/tests/explore_memory.json`) |
| `EXPLORE_LOG` | Explorer log file (default: `tools/tests/task_log.txt`) |

None of these are needed just to play the game. The Runner only needs a ROM.

## Automated exploration

`Game/Engine/TestScript.cs` runs simple text test scripts with steps such as `talk`, `goto x z`, `warp <map>`, `save <name>` and `explore <n>`. In `explore` mode an automatic player walks the overworld and decides where to go next:

- It records every field-script command it executes and which flags and variables each script read, and saves this to disk. It uses that record to find scripts and branches it hasn't seen yet.
- It plans routes around water and down one-way ledges, and avoids triggers it already knows.
- It fights wild battles (picking the strongest move with PP left, fleeing when low on HP) and heals at Pokémon Centers it has found.

Its coverage of the game's 55,000+ script commands is a rough measure of how much of the game is playable.

## Conventions

- Every ported function carries a comment with the address of the original function in the ROM:

  ```csharp
  // OFFSET 0x02040D08
  private static bool ScrCmd_ShowMenuMultiColumn(ScriptContext ctx)
  ```

- Code is ported from the game binary itself. Names for functions, structures and constants follow the community [pret/pokeplatinum](https://github.com/pret/pokeplatinum) decompilation where a name exists. Where none exists, the symbol is given a descriptive name.
- Functions that haven't been ported are reached through stand-ins in `Game/Unported*.cs`. Each stand-in throws with the original address, so you can see exactly what to port next.
- ROM tables are read from the ROM at runtime (`RomData`) and are not copied into the source.

## Legal

Pokémon and all related names are trademarks of Nintendo, Creatures Inc. and Game Freak Inc. This repository contains no ROM data. You must own the game and provide your own dump to use it.
