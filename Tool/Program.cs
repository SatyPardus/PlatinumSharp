using PlatinumSharp.Nitro;
using PlatinumSharp.Nitro.Graphics;
using PlatinumSharp.Nitro.Sound;

if (args.Length < 2)
{
    Console.WriteLine("usage:");
    Console.WriteLine("  list  <rom>");
    Console.WriteLine("  narc  <rom> <path>");
    Console.WriteLine("  tiles <rom> <narc> <ncgr> <nclr> <out.png>");
    Console.WriteLine("  screen <rom> <narc> <nscr> <ncgr> <nclr> <out.png>");
    Console.WriteLine("  seqs  <rom>");
    Console.WriteLine("  wav   <rom> <sequence> <seconds> <out.wav>");
    return 1;
}

using NitroRom rom = new(args[1]);

switch (args[0])
{
    case "list":
        Console.WriteLine($"{rom.Title} {rom.GameCode} rev {rom.Revision}, {rom.Paths.Count} named files");
        foreach (string path in rom.Paths)
        {
            Console.WriteLine($"{rom.GetSize(path),10}  {path}");
        }
        break;

    case "narc":
    {
        Narc narc = Narc.Open(rom, args[2]);
        Console.WriteLine($"{narc.Count} members");
        for (int i = 0; i < narc.Count; i++)
        {
            byte[] head = narc.ReadMemberHead(i, 8);
            string magic = Bin.Magic(head, 0);
            if (magic == "" && head.Length > 4 && (head[0] == 0x10 || head[0] == 0x11))
            {
                magic = "LZ:" + Bin.Magic(Lz.Unpack(narc.ReadMember(i)), 0);
            }
            Console.WriteLine($"{i,5}  {narc.GetSize(i),8}  {magic}");
        }
        break;
    }

    case "tiles":
    {
        Narc narc = Narc.Open(rom, args[2]);
        TileSet tiles = TileSet.Parse(Lz.Unpack(narc.ReadMember(int.Parse(args[3]))));
        Palette palette = Palette.Parse(Lz.Unpack(narc.ReadMember(int.Parse(args[4]))));
        Raster raster = Renderer.RenderTiles(tiles, palette, 0);
        Renderer.SavePng(raster, args[5]);
        Console.WriteLine($"{raster.Width}x{raster.Height} {tiles.Bpp}bpp");
        break;
    }

    case "screen":
    {
        Narc narc = Narc.Open(rom, args[2]);
        TileMap map = TileMap.Parse(Lz.Unpack(narc.ReadMember(int.Parse(args[3]))));
        TileSet tiles = TileSet.Parse(Lz.Unpack(narc.ReadMember(int.Parse(args[4]))));
        Palette palette = Palette.Parse(Lz.Unpack(narc.ReadMember(int.Parse(args[5]))));
        Raster raster = Renderer.RenderScreen(map, tiles, palette);
        Renderer.SavePng(raster, args[6]);
        Console.WriteLine($"{raster.Width}x{raster.Height}");
        break;
    }

    case "seqs":
    {
        Sdat sdat = Sdat.Open(rom, rom.Paths.First(p => p.EndsWith(".sdat", StringComparison.OrdinalIgnoreCase)));
        foreach (SequenceInfo info in sdat.Sequences)
        {
            Console.WriteLine($"{info.Index,5}  bank {info.Bank,4}  vol {info.Volume,3}  {info.Name}");
        }
        break;
    }

    case "wav":
    {
        Sdat sdat = Sdat.Open(rom, rom.Paths.First(p => p.EndsWith(".sdat", StringComparison.OrdinalIgnoreCase)));
        SequenceInfo info = sdat.Sequences.First(s => s.Index.ToString() == args[2] || s.Name == args[2]);
        SequencePlayer player = new(sdat, info);
        int frames = (int)(double.Parse(args[3], System.Globalization.CultureInfo.InvariantCulture) * 44100);
        float[] buffer = new float[frames * 2];
        int done = 0;
        while (done < frames && !player.Finished)
        {
            int start = done;
            done += player.Render(buffer, done * 2, Math.Min(44100 * 5, frames - done));
            double part = 0;
            for (int i = start * 2; i < done * 2; i++)
            {
                part += buffer[i] * buffer[i];
            }
            Console.WriteLine($"{done / 44100.0,6:F1}s rms {Math.Sqrt(part / Math.Max(1, (done - start) * 2)):F4} voices {player.VoiceCount}");
        }
        float peak = 0;
        double energy = 0;
        for (int i = 0; i < done * 2; i++)
        {
            peak = Math.Max(peak, Math.Abs(buffer[i]));
            energy += buffer[i] * buffer[i];
        }
        SequencePlayer.SaveWav(args[4], buffer.AsSpan(0, done * 2).ToArray(), 44100);
        Console.WriteLine($"{info.Name}: {done / 44100.0:F1}s peak {peak:F3} rms {Math.Sqrt(energy / Math.Max(1, done * 2)):F4}");
        break;
    }

    case "verify-narc":
    {
        PlatinumSharp.Game.RomData.Open(rom);
        int failures = 0;
        for (int narcID = 0; narcID < 12; narcID++)
        {
            PlatinumSharp.Game.NARC narc = PlatinumSharp.Game.NARC.NARC_ctor(narcID, 0);
            int count = PlatinumSharp.Game.NARC.NARC_GetFileCount(narc);
            uint size = PlatinumSharp.Game.NARC.NARC_GetMemberSize(narc, 0);
            uint sizeByPair = PlatinumSharp.Game.NARC.NARC_GetMemberSizeByIndexPair(narcID, 0);
            byte[] viaHandle = PlatinumSharp.Game.NARC.NARC_AllocAndReadWholeMember(narc, 0, 0);
            byte[] viaPair = PlatinumSharp.Game.NARC.NARC_AllocAndReadWholeMemberByIndexPair(narcID, 0, 0);
            byte[] partial = new byte[Math.Min(8, (int)size)];
            PlatinumSharp.Game.NARC.NARC_ReadFromMember(narc, 0, 0, (uint)partial.Length, partial);
            PlatinumSharp.Game.NARC.NARC_dtor(narc);
            bool same = size == sizeByPair && viaHandle.AsSpan().SequenceEqual(viaPair) && viaHandle.AsSpan(0, partial.Length).SequenceEqual(partial);
            if (!same)
            {
                failures++;
            }
            string head = Convert.ToHexString(viaHandle, 0, Math.Min(8, viaHandle.Length));
            Console.WriteLine($"narc {narcID,2}: {count,5} members, member 0 is {size,7} bytes, starts {head}, consistent: {same}");
        }
        string path = rom.Paths.First(p => p.EndsWith("pl_personal.narc", StringComparison.OrdinalIgnoreCase));
        Narc reference = Narc.Open(rom, path);
        Console.WriteLine($"reference reader: {path} has {reference.Count} members, member 0 is {reference.GetSize(0)} bytes");
        Console.WriteLine(failures == 0 ? "all consistent" : failures + " inconsistent");
        break;
    }

    case "verify-message":
    {
        PlatinumSharp.Game.RomData.Open(rom);
        static string Decode(PlatinumSharp.Game.GameString text)
        {
            System.Text.StringBuilder builder = new();
            for (int i = 0; i < text.Size; i++)
            {
                ushort c = text.Data[i];
                if (c >= 0x121 && c <= 0x12A)
                {
                    builder.Append((char)('0' + c - 0x121));
                }
                else if (c >= 0x12B && c <= 0x144)
                {
                    builder.Append((char)('A' + c - 0x12B));
                }
                else if (c >= 0x145 && c <= 0x15E)
                {
                    builder.Append((char)('a' + c - 0x145));
                }
                else if (c == 0x1DE)
                {
                    builder.Append(' ');
                }
                else
                {
                    builder.Append($"<{c:X}>");
                }
            }
            return builder.ToString();
        }
        PlatinumSharp.Game.MessageLoader handle = PlatinumSharp.Game.MessageLoader.MessageLoader_Init(1, 0x1A, 0x19C, 0)!;
        PlatinumSharp.Game.MessageLoader preload = PlatinumSharp.Game.MessageLoader.MessageLoader_Init(0, 0x1A, 0x19C, 0)!;
        uint count = PlatinumSharp.Game.MessageLoader.MessageLoader_MessageCount(handle);
        Console.WriteLine($"species bank: {count} entries via handle, {PlatinumSharp.Game.MessageLoader.MessageLoader_MessageCount(preload)} via preload");
        int mismatches = 0;
        foreach (uint species in new uint[] { 1, 25, 150, 387, 493 })
        {
            PlatinumSharp.Game.GameString a = PlatinumSharp.Game.MessageLoader.MessageLoader_GetNewString(handle, species)!;
            PlatinumSharp.Game.GameString b = PlatinumSharp.Game.GameString.String_Init(0x20, 0);
            PlatinumSharp.Game.MessageLoader.MessageLoader_GetString(preload, species, b);
            bool same = PlatinumSharp.Game.GameString.String_Compare(a, b) == 0;
            if (!same)
            {
                mismatches++;
            }
            Console.WriteLine($"species {species,3}: \"{Decode(a)}\" length {a.Size}, both paths agree: {same}");
        }
        PlatinumSharp.Game.GameString move = PlatinumSharp.Game.MessageUtil.MessageUtil_MoveName(1, 0)!;
        Console.WriteLine($"move 1: \"{Decode(move)}\"");
        PlatinumSharp.Game.GameString number = PlatinumSharp.Game.GameString.String_Init(0x10, 0);
        PlatinumSharp.Game.GameString.String_FormatInt(number, -1234, 6, 1, 1);
        Console.WriteLine($"FormatInt(-1234, 6 digits, spaces): \"{Decode(number)}\"");
        PlatinumSharp.Game.GameString.String_FormatInt(number, 42, 4, 2, 1);
        bool parsed = false;
        ulong value = PlatinumSharp.Game.GameString.String_AtoI(number, ref parsed);
        Console.WriteLine($"FormatInt(42, 4 digits, zeroes): \"{Decode(number)}\" parses back to {value} ({parsed})");
        Console.WriteLine(mismatches == 0 ? "all consistent" : mismatches + " inconsistent");
        break;
    }

    case "verify-tasks":
    {
        PlatinumSharp.Game.SysTaskManager manager = PlatinumSharp.Game.SysTaskManager.SysTaskManager_Init(4, new PlatinumSharp.Game.SysTaskManager());
        List<string> log = new();
        PlatinumSharp.Game.SysTask? spawned = null;
        void Record(PlatinumSharp.Game.SysTask task, object? param)
        {
            log.Add((string)param!);
        }
        void RunOnce(PlatinumSharp.Game.SysTask task, object? param)
        {
            log.Add((string)param!);
            PlatinumSharp.Game.SysTaskManager.SysTask_Delete(task);
        }
        void Spawner(PlatinumSharp.Game.SysTask task, object? param)
        {
            log.Add((string)param!);
            spawned ??= PlatinumSharp.Game.SysTaskManager.SysTaskManager_AddTask(manager, Record, "late", 9);
        }
        PlatinumSharp.Game.SysTaskManager.SysTaskManager_AddTask(manager, Record, "p5", 5);
        PlatinumSharp.Game.SysTaskManager.SysTaskManager_AddTask(manager, RunOnce, "p1-once", 1);
        PlatinumSharp.Game.SysTaskManager.SysTaskManager_AddTask(manager, Spawner, "p3-spawner", 3);
        string[] expected =
        {
            "p1-once,p3-spawner,p5",
            "p3-spawner,p5,late",
            "p3-spawner,p5,late",
        };
        bool ok = true;
        for (int frame = 0; frame < expected.Length; frame++)
        {
            log.Clear();
            PlatinumSharp.Game.SysTaskManager.SysTaskManager_ExecuteTasks(manager);
            string actual = string.Join(",", log);
            bool same = actual == expected[frame];
            ok &= same;
            Console.WriteLine($"frame {frame}: {actual} (expected {expected[frame]}) {(same ? "ok" : "MISMATCH")}");
        }
        bool full = PlatinumSharp.Game.SysTaskManager.SysTaskManager_AddTask(manager, Record, "a", 2) != null
            && PlatinumSharp.Game.SysTaskManager.SysTaskManager_AddTask(manager, Record, "b", 2) == null;
        ok &= full;
        Console.WriteLine($"capacity of 4 enforced after one slot was freed and reused: {full}");
        Console.WriteLine(ok ? "all consistent" : "inconsistent");
        break;
    }

    case "verify-fade":
    {
        PlatinumSharp.Game.RomData.Open(rom);
        PlatinumSharp.Game.SystemState system = PlatinumSharp.Game.Globals.gSystem;
        system.MainTaskManager = PlatinumSharp.Game.SysTaskManager.SysTaskManager_Init(32, new PlatinumSharp.Game.SysTaskManager());
        system.VBlankTaskManager = PlatinumSharp.Game.SysTaskManager.SysTaskManager_Init(32, new PlatinumSharp.Game.SysTaskManager());
        system.PostVBlankTaskManager = PlatinumSharp.Game.SysTaskManager.SysTaskManager_Init(32, new PlatinumSharp.Game.SysTaskManager());
        system.PrintTaskManager = PlatinumSharp.Game.SysTaskManager.SysTaskManager_Init(32, new PlatinumSharp.Game.SysTaskManager());
        bool ok = true;
        int RunFade(int type, ushort color, int duration, Func<string> sample, List<string> samples)
        {
            PlatinumSharp.Game.ScreenFadeControl.StartScreenFade(0, type, type, color, duration, 1, 0);
            int frames = 0;
            while (!PlatinumSharp.Game.ScreenFadeControl.IsScreenFadeDone() && frames < 200)
            {
                PlatinumSharp.Game.ScreenFadeControl.ExecScreenFade();
                PlatinumSharp.Game.SysTaskManager.SysTaskManager_ExecuteTasks(system.PostVBlankTaskManager!);
                samples.Add(sample());
                frames++;
            }
            return frames;
        }
        List<string> fadeOut = new();
        int outFrames = RunFade(0, 0x0000, 8, () => PlatinumSharp.Game.Video.GetMasterBrightness(0).ToString(), fadeOut);
        bool outOk = fadeOut.Contains("-16") && outFrames < 200;
        ok &= outOk;
        Console.WriteLine($"brightness fade to black: {outFrames} frames, main brightness {string.Join(" ", fadeOut)} {(outOk ? "ok" : "MISMATCH")}");
        List<string> fadeIn = new();
        int inFrames = RunFade(1, 0x0000, 8, () => PlatinumSharp.Game.Video.GetMasterBrightness(1).ToString(), fadeIn);
        bool inOk = fadeIn.Count > 0 && fadeIn[^1] == "0" && inFrames < 200;
        ok &= inOk;
        Console.WriteLine($"brightness fade from black: {inFrames} frames, sub brightness {string.Join(" ", fadeIn)} {(inOk ? "ok" : "MISMATCH")}");
        List<string> window = new();
        int windowFrames = RunFade(6, 0x0000, 6, () =>
        {
            var w = PlatinumSharp.Game.Video.GetWindow(0, 0);
            return $"[{w.Left},{w.Top}-{w.Right},{w.Bottom}]";
        }, window);
        bool windowOk = windowFrames < 200 && window.Distinct().Count() > 2;
        ok &= windowOk;
        Console.WriteLine($"window fade (table entry 6): {windowFrames} frames, window 0 {string.Join(" ", window)} {(windowOk ? "ok" : "MISMATCH")}");
        Console.WriteLine($"visible windows main {PlatinumSharp.Game.Video.GetVisibleWindows(0)}, inside mask {PlatinumSharp.Game.Video.GetWindowInside(0, 0):X2}, outside mask {PlatinumSharp.Game.Video.GetWindowOutside(0):X2}");
        Console.WriteLine(ok ? "all consistent" : "inconsistent");
        break;
    }

    case "verify-anim":
    {
        PlatinumSharp.Game.RomData.Open(rom);
        PlatinumSharp.Game.Heap.Heap_InitSystem(0x020E5674, 4, 0x7B, 0);
        PlatinumSharp.Game.SystemState system = PlatinumSharp.Game.Globals.gSystem;
        system.MainTaskManager = PlatinumSharp.Game.SysTaskManager.SysTaskManager_Init(32, new PlatinumSharp.Game.SysTaskManager());
        PlatinumSharp.Game.PokemonAnimManager manager = PlatinumSharp.Game.PokemonAnimManager.PokemonAnimManager_New(0, 1, 0);
        int finished = 0;
        int failed = 0;
        int longest = 0;
        int longestAnim = 0;
        int moved = 0;
        int faded = 0;
        for (int animation = 0; animation < 0x8F; animation++)
        {
            PlatinumSharp.Game.PokemonSprite sprite = new();
            sprite.X = 128;
            sprite.Y = 96;
            PlatinumSharp.Game.PokemonAnimTemplate template = new() { Animation = (ushort)animation };
            try
            {
                PlatinumSharp.Game.PokemonAnimManager.PokemonAnimManager_InitAnim(manager, sprite, template, 0);
                int frames = 0;
                bool changed = false;
                bool usedFade = false;
                while (PlatinumSharp.Game.PokemonAnimManager.PokemonAnimManager_HasAnimCompleted(manager, 0) == 0 && frames < 4000)
                {
                    PlatinumSharp.Game.SysTaskManager.SysTaskManager_ExecuteTasks(system.MainTaskManager);
                    if (sprite.FadeActive)
                    {
                        usedFade = true;
                        if (sprite.FadeDelayCounter != 0)
                        {
                            sprite.FadeDelayCounter--;
                        }
                        else
                        {
                            sprite.FadeDelayCounter = sprite.FadeDelayLength;
                            if (sprite.FadeInitAlpha == sprite.FadeTargetAlpha)
                            {
                                sprite.FadeActive = false;
                            }
                            else if (sprite.FadeInitAlpha > sprite.FadeTargetAlpha)
                            {
                                sprite.FadeInitAlpha--;
                            }
                            else
                            {
                                sprite.FadeInitAlpha++;
                            }
                        }
                    }
                    changed |= sprite.X != 128 || sprite.Y != 96
                        || PlatinumSharp.Game.PokemonSprite.PokemonSprite_GetAttribute(sprite, 0xC) != 0x100
                        || PlatinumSharp.Game.PokemonSprite.PokemonSprite_GetAttribute(sprite, 0xD) != 0x100
                        || PlatinumSharp.Game.PokemonSprite.PokemonSprite_GetAttribute(sprite, 9) != 0;
                    frames++;
                }
                bool done = PlatinumSharp.Game.PokemonAnimManager.PokemonAnimManager_HasAnimCompleted(manager, 0) != 0;
                bool restored = sprite.X == 128 && sprite.Y == 96;
                if (done && restored)
                {
                    finished++;
                }
                else
                {
                    failed++;
                    Console.WriteLine($"anim {animation}: done {done}, restored {restored}, frames {frames}");
                    PlatinumSharp.Game.PokemonAnimManager.PokemonAnimManager_DeleteAnim(manager, 0);
                }
                if (changed)
                {
                    moved++;
                }
                if (usedFade)
                {
                    faded++;
                }
                if (frames > longest)
                {
                    longest = frames;
                    longestAnim = animation;
                }
            }
            catch (Exception e)
            {
                failed++;
                Console.WriteLine($"anim {animation}: {e.GetType().Name} {e.Message}");
                PlatinumSharp.Game.PokemonAnimManager.PokemonAnimManager_DeleteAnim(manager, 0);
                manager.Anims[0].Clear();
            }
        }
        Console.WriteLine($"{finished} of 143 scripts ran to completion and restored the sprite; {moved} changed the sprite on the way, {faded} used a palette fade; longest was {longest} frames (anim {longestAnim})");
        Console.WriteLine(failed == 0 ? "all consistent" : failed + " inconsistent");
        break;
    }

    case "verify-heap":
    {
        PlatinumSharp.Game.RomData.Open(rom);
        PlatinumSharp.Game.Heap.Heap_InitSystem(0x020E5674, 4, 0x7B, 0);
        bool ok = true;
        void Check(string what, bool condition)
        {
            ok &= condition;
            Console.WriteLine($"{what}: {(condition ? "ok" : "MISMATCH")}");
        }
        uint systemFree = PlatinumSharp.Game.Heap.HeapExp_FndGetTotalFreeSize(3);
        Console.WriteLine($"free bytes in heaps 0, 1, 3: {PlatinumSharp.Game.Heap.HeapExp_FndGetTotalFreeSize(0):X}, {PlatinumSharp.Game.Heap.HeapExp_FndGetTotalFreeSize(1):X}, {systemFree:X}");
        Check("child heap created from heap 3", PlatinumSharp.Game.Heap.Heap_Create(3, 0x74, 0x28000));
        Check("parent lost the child's size", systemFree - PlatinumSharp.Game.Heap.HeapExp_FndGetTotalFreeSize(3) >= 0x28000);
        uint childFree = PlatinumSharp.Game.Heap.HeapExp_FndGetTotalFreeSize(0x74);
        byte[] first = PlatinumSharp.Game.Heap.Heap_Alloc(0x74, 0x100)!;
        byte[] second = PlatinumSharp.Game.Heap.Heap_AllocAtEnd(0x74, 0x21)!;
        Check("blocks have the requested sizes", first.Length == 0x100 && second.Length == 0x21);
        Check("child free size shrank by payload plus headers", childFree - PlatinumSharp.Game.Heap.HeapExp_FndGetTotalFreeSize(0x74) == 0x100 + 0x24 + 0x40);
        Check("oversized request fails", TryAlloc(0x74, 0x40000) == null);
        PlatinumSharp.Game.Heap.Heap_Free(first);
        PlatinumSharp.Game.Heap.Heap_FreeExplicit(0x74, second);
        Check("child heap is empty again", PlatinumSharp.Game.Heap.HeapExp_FndGetTotalFreeSize(0x74) == childFree);
        PlatinumSharp.Game.Heap.Heap_Destroy(0x74);
        Check("parent got its memory back", PlatinumSharp.Game.Heap.HeapExp_FndGetTotalFreeSize(3) == systemFree);
        Check("heap id can be reused", PlatinumSharp.Game.Heap.Heap_Create(3, 0x74, 0x1000));
        Console.WriteLine(ok ? "all consistent" : "inconsistent");
        break;

        static byte[]? TryAlloc(int heapID, uint size)
        {
            try
            {
                return PlatinumSharp.Game.Heap.Heap_Alloc(heapID, size);
            }
            catch (NotImplementedException)
            {
                return null;
            }
        }
    }

    case "verify-sprite":
    {
        PlatinumSharp.Game.RomData.Open(rom);
        int banks = 0;
        int sequences = 0;
        int ended = 0;
        int looping = 0;
        int broken = 0;
        void Play(byte[] file)
        {
            if (!PlatinumSharp.Game.G2d.GetUnpackedAnimBank(file, out PlatinumSharp.Game.AnimBankData? bank))
            {
                return;
            }
            banks++;
            for (ushort index = 0; ; index++)
            {
                PlatinumSharp.Game.AnimSequence? sequence = PlatinumSharp.Game.NnsG2d.GetAnimSequenceByIdx(bank, index);
                if (sequence == null)
                {
                    break;
                }
                sequences++;
                PlatinumSharp.Game.AnimController controller = new();
                PlatinumSharp.Game.NnsG2d.InitAnimCtrl(controller);
                bool valid = true;
                try
                {
                    PlatinumSharp.Game.NnsG2d.BindAnimCtrl(controller, sequence);
                    for (int step = 0; step < 2000 && controller.Active != 0; step++)
                    {
                        PlatinumSharp.Game.NnsG2d.TickAnimCtrl(controller, 0x2000);
                        int first = sequence.Frames;
                        int last = first + (sequence.NumFrames - 1) * 8;
                        valid &= controller.Current >= first && controller.Current <= last;
                        valid &= controller.ActiveCurrent >= first && controller.ActiveCurrent <= last;
                        valid &= PlatinumSharp.Game.NnsG2d.GetAnimCtrlCurrentElement(controller) + 2 <= file.Length;
                    }
                }
                catch (Exception)
                {
                    valid = false;
                }
                if (!valid)
                {
                    broken++;
                }
                else if (controller.Active == 0)
                {
                    ended++;
                }
                else
                {
                    looping++;
                }
            }
        }
        Play(PlatinumSharp.Game.GameSystem.ReadFileToHeap(0, "data/clact_default.NANR")!);
        foreach (string path in rom.Paths)
        {
            if (!path.EndsWith(".narc", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            Narc narc;
            try
            {
                narc = Narc.Open(rom, path);
            }
            catch (Exception)
            {
                continue;
            }
            for (int i = 0; i < narc.Count; i++)
            {
                if (Bin.Magic(narc.ReadMemberHead(i, 8), 0) == "RNAN")
                {
                    Play(narc.ReadMember(i));
                }
            }
        }
        Console.WriteLine($"{banks} animation banks, {sequences} sequences: {ended} ran to their end, {looping} loop, {broken} left their frame range");
        int pairs = 0;
        int drawnCells = 0;
        int drawnOams = 0;
        int culledCells = 0;
        int affineOams = 0;
        int mismatched = 0;
        List<PlatinumSharp.Game.OamAttributes> registered = new();
        int affineRegistered = 0;
        PlatinumSharp.Game.G2dRenderer renderer = new();
        PlatinumSharp.Game.RenderView.RenderView_InitRenderer(renderer, 0);
        PlatinumSharp.Game.ViewRect viewRect = new() { X = 0, Y = 0, Width = 255 << 12, Height = 192 << 12 };
        PlatinumSharp.Game.RenderView.RenderView_InitSurface(
            renderer.Surfaces[0],
            viewRect,
            (oam, affineIndex) =>
            {
                registered.Add(oam);
                if (affineIndex != PlatinumSharp.Game.NnsG2d.AffineIndexNone)
                {
                    affineOams++;
                }
                return true;
            },
            matrix => (ushort)(affineRegistered++ & 0x1F),
            PlatinumSharp.Game.RenderView.RenderView_IsCellVisible,
            PlatinumSharp.Game.NnsG2d.SurfaceTypeMain2D,
            renderer);
        PlatinumSharp.Game.ImageProxy imageProxy = new();
        PlatinumSharp.Game.ImagePaletteProxy paletteProxy = new();
        void Draw(byte[] cellFile, byte[] animFile)
        {
            if (!PlatinumSharp.Game.G2d.GetUnpackedCellBank(cellFile, out PlatinumSharp.Game.CellDataBank? cellBank))
            {
                return;
            }
            if (!PlatinumSharp.Game.G2d.GetUnpackedAnimBank(animFile, out PlatinumSharp.Game.AnimBankData? animBank))
            {
                return;
            }
            pairs++;
            for (ushort index = 0; ; index++)
            {
                PlatinumSharp.Game.AnimSequence? sequence = PlatinumSharp.Game.NnsG2d.GetAnimSequenceByIdx(animBank, index);
                if (sequence == null)
                {
                    break;
                }
                PlatinumSharp.Game.CellAnimation anim = new();
                try
                {
                    PlatinumSharp.Game.NnsG2d.InitCellAnimation(anim, sequence, cellBank);
                    for (int step = 0; step < 24; step++)
                    {
                        PlatinumSharp.Game.CellData? cell = PlatinumSharp.Game.NnsG2d.GetCellData(cellBank, anim.CurrentCell);
                        registered.Clear();
                        PlatinumSharp.Game.NnsG2d.SetRendererImageProxy(renderer, imageProxy, paletteProxy);
                        PlatinumSharp.Game.NnsG2d.BeginRendering(renderer);
                        PlatinumSharp.Game.NnsG2d.PushMtx();
                        PlatinumSharp.Game.NnsG2d.Translate(128 << 12, 96 << 12, 0);
                        if ((step & 1) != 0)
                        {
                            PlatinumSharp.Game.NnsG2d.RotZ(PlatinumSharp.Game.Render3D.Sin(0x200), PlatinumSharp.Game.Render3D.Cos(0x200));
                        }
                        PlatinumSharp.Game.NnsG2d.DrawCellAnimation(anim);
                        PlatinumSharp.Game.NnsG2d.PopMtx();
                        PlatinumSharp.Game.NnsG2d.EndRendering();
                        if (cell == null)
                        {
                            mismatched += registered.Count == 0 ? 0 : 1;
                        }
                        else if (registered.Count == 0)
                        {
                            culledCells++;
                        }
                        else
                        {
                            drawnCells++;
                            drawnOams += registered.Count;
                            mismatched += registered.Count == cell.NumOamAttrs ? 0 : 1;
                        }
                        PlatinumSharp.Game.NnsG2d.TickCellAnimation(anim, 0x2000);
                    }
                }
                catch (Exception)
                {
                    mismatched++;
                }
            }
        }
        foreach (string path in rom.Paths)
        {
            if (!path.EndsWith(".narc", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            Narc narc;
            try
            {
                narc = Narc.Open(rom, path);
            }
            catch (Exception)
            {
                continue;
            }
            for (int i = 0; i + 1 < narc.Count; i++)
            {
                if (Bin.Magic(narc.ReadMemberHead(i, 8), 0) == "RECN" && Bin.Magic(narc.ReadMemberHead(i + 1, 8), 0) == "RNAN")
                {
                    Draw(narc.ReadMember(i), narc.ReadMember(i + 1));
                }
            }
        }
        Console.WriteLine($"{pairs} cell/animation pairs: {drawnCells} cells drawn as {drawnOams} objects ({affineOams} affine, {affineRegistered} matrices), {culledCells} culled, {mismatched} mismatched");
        Console.WriteLine(broken == 0 && sequences > 0 && mismatched == 0 && drawnCells > 0 ? "all consistent" : "inconsistent");
        break;
    }

    case "verify-vram":
    {
        PlatinumSharp.Game.RomData.Open(rom);
        bool ok = true;
        void Check(string what, bool condition)
        {
            ok &= condition;
            Console.WriteLine($"{what}: {(condition ? "ok" : "MISMATCH")}");
        }
        PlatinumSharp.Game.G3DPipelineBuffers buffers = PlatinumSharp.Game.G3DPipeline.G3DPipeline_InitEx(0, 0, 2, 0, 4, () => { });
        Check("work buffers sized for the block pools", buffers.TexBuffer!.Length == 0x1000 && buffers.PlttBuffer!.Length == 0x4000);
        List<(uint Address, uint Size)> textures = new();
        uint[] textureKeys = new uint[40];
        for (int i = 0; i < textureKeys.Length; i++)
        {
            textureKeys[i] = PlatinumSharp.Game.Gfd.AllocTexVram((uint)(0x400 + i * 0x90), 0, 0);
            textures.Add((PlatinumSharp.Game.Gfd.GetTexKeyAddress(textureKeys[i]), PlatinumSharp.Game.Gfd.GetTexKeySize(textureKeys[i])));
        }
        bool distinct = true;
        for (int i = 0; i < textures.Count; i++)
        {
            distinct &= textureKeys[i] != 0 && textures[i].Address + textures[i].Size <= 0x40000 && textures[i].Size >= 0x400 + i * 0x90;
            for (int j = 0; j < i; j++)
            {
                distinct &= textures[i].Address >= textures[j].Address + textures[j].Size || textures[j].Address >= textures[i].Address + textures[i].Size;
            }
        }
        Check("forty textures placed without overlap inside two slots", distinct);
        Check("request larger than the remaining space fails", PlatinumSharp.Game.Gfd.AllocTexVram(0x3F000, 0, 0) == 0);
        bool freed = true;
        for (int i = 0; i < textureKeys.Length; i += 2)
        {
            freed &= PlatinumSharp.Game.Gfd.FreeTexVram(textureKeys[i]) == 0;
        }
        for (int i = 1; i < textureKeys.Length; i += 2)
        {
            freed &= PlatinumSharp.Game.Gfd.FreeTexVram(textureKeys[i]) == 0;
        }
        Check("all textures freed", freed);
        uint whole = PlatinumSharp.Game.Gfd.AllocTexVram(0x40000 - 0x10, 0, 0);
        Check("free blocks merged back into one region", whole != 0 && PlatinumSharp.Game.Gfd.GetTexKeyAddress(whole) == 0);
        uint palette16 = PlatinumSharp.Game.Gfd.AllocPlttVram(0x20, 0, 0);
        uint palette4 = PlatinumSharp.Game.Gfd.AllocPlttVram(8, 1, 0);
        uint palette256 = PlatinumSharp.Game.Gfd.AllocPlttVram(0x200, 0, 0);
        Check("palettes allocated", palette16 != 0 && palette4 != 0 && palette256 != 0);
        Check("16-colour palettes are aligned to 16 bytes", PlatinumSharp.Game.Gfd.GetPlttKeyAddress(palette256) % 16 == 0);
        Check("palettes freed", PlatinumSharp.Game.Gfd.FreePlttVram(palette16) == 0 && PlatinumSharp.Game.Gfd.FreePlttVram(palette4) == 0 && PlatinumSharp.Game.Gfd.FreePlttVram(palette256) == 0);
        uint wholePalette = PlatinumSharp.Game.Gfd.AllocPlttVram(0x8000, 0, 0);
        Check("palette memory merged back into one region", wholePalette != 0 && PlatinumSharp.Game.Gfd.GetPlttKeyAddress(wholePalette) == 0);
        PlatinumSharp.Game.NnsGfd.InitFrmTexVramManager(1, true);
        uint frameLow = PlatinumSharp.Game.Gfd.AllocTexVram(0x1000, 0, 0);
        uint[] state = new uint[10];
        PlatinumSharp.Game.NnsGfd.GetFrmTexVramState(state);
        uint frameNext = PlatinumSharp.Game.Gfd.AllocTexVram(0x1000, 0, 0);
        PlatinumSharp.Game.NnsGfd.SetFrmTexVramState(state);
        uint frameAgain = PlatinumSharp.Game.Gfd.AllocTexVram(0x1000, 0, 0);
        Check("frame manager allocates from the top of the slot", PlatinumSharp.Game.Gfd.GetTexKeyAddress(frameLow) == 0x1F000);
        Check("restoring the frame state reuses the same address", frameNext == frameAgain && frameNext != frameLow);
        Console.WriteLine(ok ? "all consistent" : "inconsistent");
        break;
    }

    case "verify-overlays":
    {
        PlatinumSharp.Game.RomData.Open(rom);
        int mismatches = 0;
        int checkedCount = 0;
        foreach (string file in Directory.GetFiles(args[2], "overlay_*.bin"))
        {
            int id = int.Parse(Path.GetFileNameWithoutExtension(file).Substring(8));
            if (!PlatinumSharp.Game.OverlaySystem.Image(id).AsSpan().SequenceEqual(File.ReadAllBytes(file)))
            {
                mismatches++;
            }
            checkedCount++;
        }
        Console.WriteLine($"{checkedCount} overlays, {mismatches} mismatches");
        return mismatches == 0 ? 0 : 1;
    }
    case "verify-model":
    {
        PlatinumSharp.Game.RomData.Open(rom);
        int narcID = Convert.ToInt32(args[2], 16);
        uint pathAddress = PlatinumSharp.Game.RomData.Arm9U32(0x02100498 + (uint)narcID * 4, 1)[0];
        string path = Bin.CString(PlatinumSharp.Game.RomData.Arm9(pathAddress, 64), 0);
        Narc narc = Narc.Open(rom, path.StartsWith('/') ? path : "/" + path);
        Console.WriteLine($"{path}: {narc.Count} members");
        uint nextTexture = 0;
        uint nextPalette = 0;
        PlatinumSharp.Game.Gfd.AllocTexVram = (size, is4x4, _) =>
        {
            uint address = nextTexture;
            nextTexture += (size + 7) & ~7u;
            return PlatinumSharp.Game.Gfd.MakeTexKey(address, size, is4x4 != 0);
        };
        PlatinumSharp.Game.Gfd.AllocPlttVram = (size, _, _) =>
        {
            uint address = nextPalette;
            nextPalette += (size + 15) & ~15u;
            return PlatinumSharp.Game.Gfd.MakePlttKey(address, size);
        };
        int textureMember = int.Parse(args[3]);
        PlatinumSharp.Game.G3dTexture? texture = null;
        if (textureMember >= 0)
        {
            byte[] textureFile = narc.ReadMember(textureMember);
            texture = PlatinumSharp.Game.G3d.GetTexture(textureFile);
            PlatinumSharp.Game.G3d.SetupResource(textureFile);
        }
        List<PlatinumSharp.Game.G3dRenderObj> objects = new();
        float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;
        for (int i = 5; i < args.Length; i++)
        {
            byte[] file = narc.ReadMember(int.Parse(args[i]));
            PlatinumSharp.Game.G3dModelSet set = PlatinumSharp.Game.G3d.GetModelSet(file)!;
            if (texture != null)
            {
                PlatinumSharp.Game.G3d.BindModelSet(set, texture);
            }
            else
            {
                PlatinumSharp.Game.G3d.SetupResource(file);
            }
            PlatinumSharp.Game.G3dModel model = PlatinumSharp.Game.G3d.GetModelByIndex(set, 0)!;
            PlatinumSharp.Game.G3dRenderObj renderObj = new();
            PlatinumSharp.Game.G3d.InitRenderObj(renderObj, model);
            objects.Add(renderObj);
            float boxScale = Bin.I32(file, model.Offset + 0x38) / 4096f;
            float x = Bin.S16(file, model.Offset + 0x2C) / 4096f * boxScale;
            float y = Bin.S16(file, model.Offset + 0x2E) / 4096f * boxScale;
            float z = Bin.S16(file, model.Offset + 0x30) / 4096f * boxScale;
            float w = Bin.S16(file, model.Offset + 0x32) / 4096f * boxScale;
            float h = Bin.S16(file, model.Offset + 0x34) / 4096f * boxScale;
            float d = Bin.S16(file, model.Offset + 0x36) / 4096f * boxScale;
            int matsOffset = model.Offset + Bin.I32(file, model.Offset + 8);
            for (int m = 0; m < file[model.Offset + 0x18]; m++)
            {
                int entry = PlatinumSharp.Game.G3dDict.GetEntryByIndex(file, matsOffset + 4, m);
                int mat = matsOffset + Bin.I32(file, entry);
                Console.WriteLine($"  mat {m}: diffAmb {Bin.U32(file, mat + 4):X8} specEmi {Bin.U32(file, mat + 8):X8} polyAttr {Bin.U32(file, mat + 0xC):X8} mask {Bin.U32(file, mat + 0x10):X8} tex {Bin.U32(file, mat + 0x14):X8} pal {Bin.U16(file, mat + 0x1C):X4} flag {Bin.U16(file, mat + 0x1E):X4}");
            }
            Console.WriteLine($"model {args[i]}: nodes {file[model.Offset + 0x17]} materials {file[model.Offset + 0x18]} shapes {file[model.Offset + 0x19]} box ({x:F1},{y:F1},{z:F1}) size ({w:F1},{h:F1},{d:F1})");
            minX = Math.Min(minX, x); minY = Math.Min(minY, y); minZ = Math.Min(minZ, z);
            maxX = Math.Max(maxX, x + w); maxY = Math.Max(maxY, y + h); maxZ = Math.Max(maxZ, z + d);
        }
        float centerX = (minX + maxX) / 2, centerY = (minY + maxY) / 2, centerZ = (minZ + maxZ) / 2;
        float radius = Math.Max(maxX - minX, Math.Max(maxY - minY, maxZ - minZ)) * 0.5f + 0.01f;
        int Fx(float v) => (int)(v * 4096);
        PlatinumSharp.Game.Render3D.Display3DControl = 1 | 8;
        PlatinumSharp.Game.Render3D.SetClearColor(0x2108, 31, 0x7FFF, 63, false);
        PlatinumSharp.Game.Render3D.SetLookAt(
            new PlatinumSharp.Game.VecFx32 { X = Fx(centerX), Y = Fx(centerY + radius * 1.2f), Z = Fx(centerZ + radius * 2.2f) },
            new PlatinumSharp.Game.VecFx32 { Y = 4096 },
            new PlatinumSharp.Game.VecFx32 { X = Fx(centerX), Y = Fx(centerY), Z = Fx(centerZ) });
        PlatinumSharp.Game.Render3D.SetPerspective(Fx(0.3827f), Fx(0.9239f), Fx(4f / 3f), Fx(radius * 0.1f), Fx(radius * 8f), 4096);
        PlatinumSharp.Game.Render3D.SetGlobalLightVector(0, 0, Fx(-0.7f), Fx(-0.7f));
        PlatinumSharp.Game.Render3D.SetGlobalLightColor(0, 0x7FFF);
        PlatinumSharp.Game.Render3D.SetGlobalDiffuseAmbient(0x7FFF, 0x4210, false);
        PlatinumSharp.Game.Render3D.SetGlobalSpecularEmission(0, 0, false);
        PlatinumSharp.Game.Render3D.SetGlobalPolygonAttr(1, 0, 3, 0, 31, 0);
        foreach (PlatinumSharp.Game.G3dRenderObj renderObj in objects)
        {
            PlatinumSharp.Game.Render3D.FlushGlobalState();
            PlatinumSharp.Game.G3d.Draw(renderObj);
        }
        PlatinumSharp.Game.Render3D.SwapBuffers(0, 1);
        Raster modelRaster = new(256, 192);
        int covered = 0;
        for (int i = 0; i < modelRaster.Pixels.Length; i++)
        {
            int value = PlatinumSharp.Game.Render3D.Frame[i];
            modelRaster.Pixels[i] = PlatinumSharp.Game.Rasterizer.ToArgb((ushort)(value & 0x7FFF));
            if ((value & 0x7FFF) != 0x2108)
            {
                covered++;
            }
        }
        Renderer.SavePng(modelRaster, args[4]);
        Console.WriteLine($"covered {covered} pixels");
        break;
    }
    case "boot":
    {
        PlatinumSharp.Game.RomData.Open(rom);
        PlatinumSharp.Game.CardBackup.Open(null);
        int frameLimit = args.Length > 2 ? int.Parse(args[2]) : 600;
        int frames = 0;
        foreach (string arg in args.Skip(3))
        {
            if (arg.StartsWith("redirect="))
            {
                string[] pair = arg.Substring(9).Split('>');
                PlatinumSharp.Game.ApplicationTemplate.Redirects[Convert.ToUInt32(pair[0], 16)] = Convert.ToUInt32(pair[1], 16);
            }
        }
        List<(int Frame, int Keys)> presses = new();
        foreach (string arg in args.Skip(3))
        {
            string[] parts = arg.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[0], out int pressFrame) && int.TryParse(parts[1], out int pressKeys))
            {
                presses.Add((pressFrame, pressKeys));
            }
        }
        float[] audioScratch = new float[735 * 2];
        HashSet<int> shotFrames = new();
        foreach (string arg in args.Skip(3))
        {
            if (arg.StartsWith("shots="))
            {
                foreach (string frame in arg.Substring(6).Split(','))
                {
                    shotFrames.Add(int.Parse(frame));
                }
            }
        }
        PlatinumSharp.Game.TestScript? testScript = null;
        foreach (string arg in args.Skip(3))
        {
            if (arg.StartsWith("script="))
            {
                testScript = new PlatinumSharp.Game.TestScript(arg.Substring(7));
                string? stateName = args.Skip(3).FirstOrDefault(a => a.StartsWith("state="))?.Substring(6);
                PlatinumSharp.Game.CardBackup.Open(PlatinumSharp.Game.TestScript.PrepareBackup(stateName));
                if (stateName != null)
                {
                    testScript.SkipToState(stateName);
                }
            }
        }
        PlatinumSharp.Game.Interrupts.WaitHook = waitMask =>
        {
            if ((waitMask & PlatinumSharp.Game.Interrupts.VBlank) == 0)
            {
                return;
            }
            PlatinumSharp.Game.Audio.Render(audioScratch, 0, 735);
            frames++;
            if (shotFrames.Contains(frames))
            {
                Raster shot = new(256, 384);
                int[] screen = new int[256 * 192];
                bool mainOnTop = (PlatinumSharp.Game.Video.PowerControl & 0x8000) != 0;
                for (int display = 0; display < 2; display++)
                {
                    PlatinumSharp.Game.Rasterizer.LayerMask = 0x1F;
                    PlatinumSharp.Game.Rasterizer.Render(display, screen);
                    PlatinumSharp.Game.Rasterizer.ApplyMasterBrightness(display, screen);
                    int top = (display == 0) == mainOnTop ? 0 : 192;
                    Array.Copy(screen, 0, shot.Pixels, top * 256, screen.Length);
                }
                Directory.CreateDirectory(@"E:\Poketest\tools\tests\shots");
                Renderer.SavePng(shot, $@"E:\Poketest\tools\tests\shots\hl_{frames}.png");
            }
            int held = 0;
            foreach ((int Frame, int Keys) press in presses)
            {
                if (frames >= press.Frame && frames < press.Frame + 4)
                {
                    held |= press.Keys;
                }
            }
            if (testScript != null)
            {
                held |= testScript.Update();
                if (testScript.Finished)
                {
                    throw new OperationCanceledException();
                }
            }
            PlatinumSharp.Game.Input.Keys = held;
            if (frames == 10 && PlatinumSharp.Game.ApplicationTemplate.Redirects.Count > 0)
            {
                PlatinumSharp.Game.TrainerInfo info = PlatinumSharp.Game.PlayerSave.SaveData_GetTrainerInfo(PlatinumSharp.Game.SaveData.SaveData_Ptr());
                if (PlatinumSharp.Game.TrainerInfo.TrainerInfo_HasNoName(info))
                {
                    PlatinumSharp.Game.TrainerInfo.TrainerInfo_SetName(info, new ushort[] { 0x012B, 0x012C, 0x012D, 0xFFFF });
                }
            }
            if (frames >= frameLimit)
            {
                throw new OperationCanceledException();
            }
        };
        try
        {
            PlatinumSharp.Game.Main.NitroMain();
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"ran {frames} frames without hitting a stand-in");
            if (testScript != null)
            {
                Console.WriteLine(testScript.Status);
            }
            Console.Write(PlatinumSharp.Game.DebugInfo.DescribeApplication());
            if (args.Length > 3 && args[3] == "dump")
            {
                for (int display = 0; display < 2; display++)
                {
                    Console.WriteLine($"display {display}: control {PlatinumSharp.Game.Video.GetDisplayControl(display):X8} blend {PlatinumSharp.Game.Video.GetBlendControl(display):X4} alpha {PlatinumSharp.Game.Video.GetBlendAlpha(display):X4} master {PlatinumSharp.Game.Video.GetMasterBrightness(display)}");
                    for (int layer = 0; layer < 4; layer++)
                    {
                        Console.WriteLine($"  bg{layer}: control {PlatinumSharp.Game.Video.GetBgControl(display * 4 + layer):X4} offset {PlatinumSharp.Game.Video.GetBgOffset(display * 4 + layer):X8}");
                    }
                    PlatinumSharp.Game.OamAttributes[] oam = PlatinumSharp.Game.OamManager.GetScreenOam(display);
                    for (int i = 0; i < oam.Length; i++)
                    {
                        if ((oam[i].Attr0 & 0x300) != 0x200)
                        {
                            Console.WriteLine($"  obj {i}: {oam[i].Attr0:X4} {oam[i].Attr1:X4} {oam[i].Attr2:X4}");
                        }
                    }
                    ushort[] objPalette = PlatinumSharp.Game.Video.GetPalette(display == 0 ? PlatinumSharp.Game.Video.MainObject : PlatinumSharp.Game.Video.SubObject);
                    for (int row = 0; row < 16; row++)
                    {
                        Console.WriteLine("  objpal " + row + ": " + string.Join(" ", objPalette.Skip(row * 16).Take(16).Select(c => c.ToString("X4"))));
                    }
                }
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"stopped after {frames} frames: {ex.GetType().Name}: {ex.Message}");
            foreach (string line in (ex.StackTrace ?? "").Split((char)10).Take(12))
            {
                Console.WriteLine(line.TrimEnd());
            }
            return 1;
        }
        return 0;
    }
    case "verify-save":
    {
        PlatinumSharp.Game.RomData.Open(rom);
        bool ok = true;
        void Check(string what, bool condition)
        {
            ok &= condition;
            Console.WriteLine($"{what}: {(condition ? "ok" : "MISMATCH")}");
        }
        foreach (PlatinumSharp.Game.SaveTableEntry entry in PlatinumSharp.Game.SaveDataTable.sSaveTable)
        {
            if (entry.ID == PlatinumSharp.Game.SaveDataTable.EntryMisc)
            {
                continue;
            }
            entry.CreateFunc = () => new object();
            entry.InitFunc = null;
        }
        PlatinumSharp.Game.SaveData Reload()
        {
            try
            {
                return PlatinumSharp.Game.SaveData.SaveData_Init();
            }
            catch (NotImplementedException)
            {
                return PlatinumSharp.Game.SaveData.SaveData_Ptr();
            }
        }
        PlatinumSharp.Game.MathUtil.InitCRC16Table(0);
        PlatinumSharp.Game.CardBackup.Open(null);
        PlatinumSharp.Game.SaveData saveData = Reload();
        PlatinumSharp.Game.SaveBlockInfo normal = saveData.BlockInfo[0];
        PlatinumSharp.Game.SaveBlockInfo boxes = saveData.BlockInfo[1];
        Console.WriteLine($"normal block: offset {normal.Offset:X}, size {normal.Size:X}, {normal.SectorsInUse} sectors; boxes block: offset {boxes.Offset:X}, size {boxes.Size:X}, {boxes.SectorsInUse} sectors");
        Check("blank card reports no data", saveData.BackupExists == 1 && saveData.DataExists == 0 && saveData.IsNewGameData == 1 && saveData.LoadCheckStatus == 0);
        Check("blocks are laid out back to back inside the body", normal.Offset == 0 && boxes.Offset == normal.Size && boxes.Offset + boxes.Size <= 0x20000 && normal.SectorsInUse + boxes.SectorsInUse <= 32);
        Check("page locations follow their sizes", saveData.PageInfo[1].Location == saveData.PageInfo[0].Size && saveData.PageInfo[37].Location == normal.Size);
        Check("first save succeeds", PlatinumSharp.Game.SaveData.SaveData_Save(saveData) == 2 && saveData.DataExists == 1 && saveData.IsNewGameData == 0);
        byte firstMirror = saveData.BlockOffsets[0];
        saveData = Reload();
        Check("saved data is found again", saveData.DataExists == 1 && saveData.IsNewGameData == 0 && saveData.FullSaveRequired == 0 && saveData.BlockOffsets[0] == firstMirror);
        uint counter = saveData.BlockCounters[0];
        Check("second save succeeds", PlatinumSharp.Game.SaveData.SaveData_Save(saveData) == 2);
        Check("second save went to the other mirror", saveData.BlockOffsets[0] != firstMirror && saveData.BlockCounters[0] == counter + 1);
        byte secondMirror = saveData.BlockOffsets[0];
        saveData = Reload();
        Check("newest mirror is chosen on reload", saveData.BlockOffsets[0] == secondMirror && saveData.BlockCounters[0] == counter + 1);
        byte[] damaged = new byte[1];
        uint damagedAddress = (secondMirror == 0 ? 0u : 0x40000u) + 0x100;
        PlatinumSharp.Game.CardBackup.Read(damagedAddress, damaged, 0, 1);
        damaged[0] ^= 0x5A;
        PlatinumSharp.Game.CardBackup.Write(damagedAddress, damaged, 0, 1);
        saveData = Reload();
        Check("damaged mirror falls back to the older one", saveData.DataExists == 1 && saveData.BlockOffsets[0] == firstMirror && saveData.BlockCounters[0] == counter);
        Check("erase clears the card", PlatinumSharp.Game.SaveData.SaveData_Erase(saveData) && Reload().DataExists == 0);
        Console.WriteLine(ok ? "all consistent" : "inconsistent");
        break;
    }

    case "verify-graphics":
    {
        PlatinumSharp.Game.RomData.Open(rom);
        PlatinumSharp.Game.Font.Font_LoadTextPalette(0, 0, 0);
        ushort[] mainBg = PlatinumSharp.Game.Video.GetPalette(PlatinumSharp.Game.Video.MainBackground);
        Console.WriteLine("text palette -> main BG slot 0: " + string.Join(" ", mainBg.Take(16).Select(c => c.ToString("X4"))));
        PlatinumSharp.Game.Font.Font_LoadScreenIndicatorsPalette(4, 0x20, 0);
        ushort[] subBg = PlatinumSharp.Game.Video.GetPalette(PlatinumSharp.Game.Video.SubBackground);
        Console.WriteLine("indicator palette -> sub BG slot 1: " + string.Join(" ", subBg.Skip(16).Take(16).Select(c => c.ToString("X4"))));
        byte[]? file = PlatinumSharp.Game.Graphics.Graphics_GetCharData(4, 0, 0, out PlatinumSharp.Game.CharacterData? character, 0);
        Console.WriteLine($"narc 4 member 0: file {file?.Length} bytes, {character?.Width}x{character?.Height} tiles, pixel format {character?.PixelFormat}, {character?.SizeBytes} bytes of tile data");
        uint loaded = PlatinumSharp.Game.Graphics.Graphics_LoadObjectTiles(4, 0, 0, 0, 0, 0, 0);
        byte[] objectTiles = PlatinumSharp.Game.Video.GetObjectTiles(0);
        bool matches = character != null && objectTiles.AsSpan(0, (int)loaded).SequenceEqual(character.RawData.AsSpan(0, (int)loaded));
        Console.WriteLine($"object tile upload: {loaded} bytes, matches unpacked data: {matches}");
        PlatinumSharp.Game.PaletteData paletteData = PlatinumSharp.Game.PaletteData.PaletteData_New(0);
        PlatinumSharp.Game.PaletteData.PaletteData_AllocBuffer(paletteData, 0, 0x200, 0);
        PlatinumSharp.Game.PaletteData.PaletteData_LoadBufferFromFileStart(paletteData, 0xE, 6, 0, 0, 0x20, 0);
        PlatinumSharp.Game.PaletteData.PaletteData_Blend(paletteData, 0, 0, 16, 8, 0x7FFF);
        ushort[] unfaded = PlatinumSharp.Game.PaletteData.PaletteData_GetUnfadedBuffer(paletteData, 0)!;
        ushort[] faded = PlatinumSharp.Game.PaletteData.PaletteData_GetFadedBuffer(paletteData, 0)!;
        Console.WriteLine("palette buffer unfaded:     " + string.Join(" ", unfaded.Take(8).Select(c => c.ToString("X4"))));
        Console.WriteLine("blended halfway to white:   " + string.Join(" ", faded.Take(8).Select(c => c.ToString("X4"))));
        break;
    }

    default:
        Console.WriteLine("unknown command " + args[0]);
        return 1;
}

return 0;
