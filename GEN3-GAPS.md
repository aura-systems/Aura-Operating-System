# Cosmos gen3 gaps found while porting AuraOS

These are the upstream-issue candidates found while porting AuraOS from gen2
(IL2CPU) to Cosmos gen3 (NativeAOT).

- Checked against **Cosmos HEAD `6c857b567`** (2026-10-02), with the local
  package feed **3.0.89.20261001** built by `make setup`. The `v3.0.89` tag
  packages were not used.
- Each entry is written to be liftable into a GitHub issue.
- This list replaces the one from the previous port attempt (origin/gen3 on
  3.0.77). The section at the end says which of those items HEAD has fixed.

**Finding the workarounds in the code.** Every Aura workaround site carries a
`// GEN3-GAP(<tag>): ...` comment. The tag is the one in each heading below.

```bash
grep -rn "GEN3-GAP(" SRC/            # every workaround site
grep -rn "GEN3-GAP(dhcp)" SRC/       # the sites of one gap
```

When a gap is fixed upstream, remove its workaround and its comments, then
delete the entry here.

**Severity** is the impact on Aura with the workaround applied:
- **major**: a visible feature loss or a real crash risk.
- **minor**: cosmetic, performance or edge cases.
- **(phase 2)**: it becomes major once Aura runs work on several threads. Phase
  1 is single-threaded except for the FTP server.

**Source** names the section of the porting notes that documents the gap:
- 01 build/boot/plugs, 02 core system, 03 filesystem/storage, 04 graphics
- 05 input, 06 network, 07 BCL/packages/Lua
- 08 lessons from the 3.0.77 port (origin/gen3)

The "rule Cn" references in the workarounds are the porting conventions from
the same notes. Examples: C6 explicit null checks, C7 no `finally` on the
exception path, C8 dispose everything.

## Summary

| Area | Major | Minor |
|---|---|---|
| Build / tooling | `iso-files` | `resources`, `resources-perf`, `update-pins`, `prop-trap`, `defines`, `compile-only`, `iso-path`, `dev-stamp`, `bytes-only` |
| Core runtime | `cpu-exception`, `null-deref`, `unhandled`, `finally`, `eh-global`, `finalizers`, `gc-trigger`, `oom`, `gc-conservative`, `idle-thread` | `run-spam`, `log-sink`, `meminfo`, `cpuinfo`, `pci`, `tz-rtc`, `env`, `pc-speaker`, `encoding` |
| Console / graphics | `console-input` | `kernelconsole`, `console-global`, `present`, `display-mode`, `blit`, `psf`, `hw-cursor`, `bmp`, `canvas3d` |
| Input | `keyboard-altgr`, `ps2-sync` | `key-release`, `key-repeat`, `e0`, `sessions`, `mouse`, `key-docs` |
| Filesystem / storage | `ide`, `fat-names`, `vfs-threads` (phase 2), `gpt-crc`, `ext2-format`, `ext2-1k` | `driveinfo`, `fat-label`, `fat-time`, `fat-resize`, `mounts`, `tmp`, `cwd`, `mbr`, `gpt-type`, `ext2-features`, `ext2-dtime` |
| Network | `tcp-receive`, `ipaddress`, `net-threads` (phase 2), `nic-drivers`, `tcp-robust` | `http-tls`, `socket-misc`, `dhcp`, `tcp-primary`, `nic-names`, `net-misc` |
| BCL / packages | | `deflate`, `crypto`, `lua-host` |

---

## Build / tooling

### `resources`: no supported embedded-resource API (minor)
- **Gap:** `Cosmos.Kernel.Core.Runtime.ResourceManager` has been internal since
  `a1835bac0` ("Internalize Cosmos.Kernel.Core outside the ring"), and
  `PCScreenFont` still reads through it.
  - `Assembly.GetManifestResourceStream` / `GetManifestResourceNames` **do
    work** at HEAD. This was verified on 3.0.89.20261001 by booting a test
    kernel: the LogicalNames are listed, and a 300 KB resource streamed
    correctly.
  - But that path is untested and undocumented upstream, and
    `accessing-internals.md` says the kernel cannot use reflection.
- **Aura workaround:** `Files.cs` alone reads assets, through
  `typeof(Aura_OS.Kernel).Assembly.GetManifestResourceStream(name)`. It reads in
  a loop until the buffer is full, disposes explicitly (no `finally`, see
  `finally`) and caches the bytes.
- **Upstream ask:** add a test and a doc for `GetManifestResourceStream` under
  NativeAOT. Alternatively, add a ring facade
  `Cosmos.Kernel.System.Resources.TryGet(string, out ReadOnlySpan<byte>)` /
  `Open(string) -> Stream` / `Names`.
- **Source:** 01, 08, orchestrator amendment A1.

### `resources-perf`: ResourceManager re-parses and logs on every lookup (minor)
- **Gap:** the internal `ResourceManager` re-parses the whole resource index on
  each lookup and serial-logs every entry (`Found resource: ...`).
  - The `s_resourceInfos` cache is commented out.
  - Names are decoded as ASCII only.
  - This hits every `PCScreenFont` default-font load. Aura sets
    `CosmosDefaultFont` to its own `zap-ext-light16.psf`.
- **Aura workaround:**
  - Read each asset once: lazy `Files.*` properties and a single
    `LoadFiles`/`LoadImages` pass.
  - Use ASCII LogicalNames only.
  - Aura's own reads no longer go through ResourceManager (see `resources`).
- **Upstream ask:** cache the index and drop the per-entry log.
- **Source:** 01, 08.

### `iso-files`: the ISO cannot carry or expose extra files (major)
- **Gap:** the kernel has no way to read files from the boot medium.
  - `_BuildIsoCore` wipes `iso_root/` and stages only the ELF, `limine.conf`
    and the Limine binaries. There is no `CosmosIsoFile` item, and a
    pre-fill target is wiped by the same target.
  - Even with extra files, the kernel cannot read the CD: there is no ISO9660
    filesystem, and AHCI refuses SATAPI.
  - `Cosmos.Kernel.Boot.Limine` implements no module request. Limine 10.8.5
    supports `module_path:`.
- **Aura workaround:** embed every asset (`SRC/Aura_OS/Resources/**`, about
  20 MiB, read through `Files.Get`). gen2's "files on the CD" feature is gone,
  and so is `HelloWorld.gb`.
- **Upstream ask:** either
  - an `@(CosmosIsoFile)` item plus a read-only ISO9660 driver (and SATAPI),
    or
  - a Limine module request on the ring (an initrd/tar).
- **Source:** 01, 03, 08.

### `update-pins`: `cosmos update` rewrites third-party `Cosmos.*` pins (minor)
- **Gap:** `ProjectPinUpdater.s_packagePin` matches any
  `PackageReference`/`PackageVersion` whose `Include` starts with `Cosmos.`.
  - It would therefore bump `Cosmos.Network.Ftp` (2.0.0) and
    `Cosmos.Executable.Lua` (4.0.1) to the kernel version.
  - Values containing `$(` or `@` are skipped.
- **Aura workaround:** keep those versions in `$(AuraFtpVersion)` /
  `$(AuraLuaVersion)`, defined in `Directory.Build.props`. The Cosmos version
  itself lives only in `global.json`.
- **Upstream ask:** restrict the updater to the package IDs Cosmos publishes.
- **Source:** 01, 08.

### `prop-trap`: `CosmosPackageVersion` adds both-arch references (minor)
- **Gap:** `Cosmos.Architecture.props` adds `PackageReference`s to both the
  X64 and the ARM64 HAL when `$(CosmosPackageVersion)` is set, and to both
  Native packages when `$(CosmosNativePackageVersion)` is set.
  - This happens in a targets file, after restore.
  - A user who centralizes the version under that natural name gets
    unrestored, build-time-only ARM64 references.
- **Aura workaround:** name the property `AuraCosmosVersion`.
- **Upstream ask:** rename the trigger properties, or document them.
- **Source:** 01.

### `defines`: `cosmos build` passes `DefineConstants` globally (minor)
- **Gap:** `cosmos build` and the Cosmos `Makefile` pass
  `-p:DefineConstants=ARCH_X64` as a global property.
  - That discards every `DefineConstants` a kernel sets.
  - A project that replaces instead of appending loses `ARCH_X64`, and with it
    `--root:Cosmos.Kernel.HAL.X64`.
- **Aura workaround:** no custom defines; only ever append
  (`$(DefineConstants);X`).
- **Upstream ask:** pass only `-p:CosmosArch=...` and let
  `Cosmos.ArchitecturePicker.props` append `ARCH_*`.
- **Source:** 01.

### `compile-only`: no compile-only build switch (minor)
- **Gap:** patch, ILC, link and ISO all hang off `AfterTargets="Build"`, with
  no opt-out property.
  - `-p:EnablePatching=false` skips only the patcher.
  - `-t:Compile` alone fails with CS0016 on the source-generator output
    directory.
- **Aura workaround:** the central compile check is
  `dotnet build SRC/Aura_OS/Aura_OS.csproj -t:Compile -p:EmitCompilerGeneratedFiles=false -v:q -nologo`.
- **Upstream ask:** a `CosmosCompileOnly` / `CosmosSkipNative` property, and a
  `-t:Compile` that works without the extra property.
- **Source:** 01, orchestrator amendment A3.

### `iso-path`: the ISO lands relative to the caller's working directory (minor)
- **Gap:** `Cosmos.Build.Common.targets` sets
  `CosmosOutputPath = NormalizePath('$(OutputPath)', 'cosmos')`. `OutputPath`
  is relative (`bin/Debug/net10.0/linux-x64/`), and `NormalizePath` resolves it
  against the process working directory, not `$(MSBuildProjectDirectory)`.
  - So `dotnet build SRC/Aura_OS/Aura_OS.csproj` run from the repo root writes
    `<repo>/bin/Debug/net10.0/linux-x64/cosmos/Aura_OS.iso`.
  - The ELF still lands in the project's own `bin/`.
- **Aura workaround:** build from `SRC/Aura_OS` (or use `cosmos build -p`,
  which runs in the project directory).
- **Upstream ask:** base the path on `$(MSBuildProjectDirectory)`, for example
  `NormalizePath('$(MSBuildProjectDirectory)', '$(OutputPath)', 'cosmos')`.
- **Source:** first full build of the port (2026-10-02).

### `dev-stamp`: dev package versions are date-stamped (minor)
- **Gap:** `make setup` (`.devcontainer/postCreateCommand.sh`) stamps the
  current date (`<base>.<yyyyMMdd>`) and rewrites Cosmos's own `global.json`.
  - It also wipes `artifacts/` and `~/.nuget/packages/cosmos.*`.
  - So after a `make setup` on a later day, Aura's pinned version exists
    neither in the feed nor in the cache.
- **Aura workaround:** after `make setup`, bump the `Cosmos.Sdk` literal in
  Aura's `global.json`. `Directory.Build.props` derives the rest.
  - CI (`.github/workflows/dotnet-core.yml`) does this with `sed`.
  - After a hand-run `dotnet pack`, also run `rm -rf ~/.nuget/packages/cosmos.*`
    and `dotnet tool update -g Cosmos.Patcher`.
- **Upstream ask:** a per-build counter or a fixed dev version, so consumers
  need not re-pin daily.
- **Source:** 01.

### `bytes-only`: `Bitmap`/`PCScreenFont.LoadFont` take only `byte[]` (minor)
- **Gap:** embedded data already in the read-only image must be copied to the
  heap first. That is about 20 MiB of assets for Aura.
- **Aura workaround:** copy once and cache.
- **Upstream ask:** `ReadOnlySpan<byte>` overloads.
- **Source:** 01.

## Core runtime

### `cpu-exception`: no CPU-exception / panic hook (major)
- **Gap:** gen2's `INTs.HandleException` plug has no replacement.
  - `Panic.CpuException`, `IRQContext`, `InterruptManager.SetHandler` and its
    `IrqDelegate` are all internal, so neither a plug nor an `[UnsafeAccessor]`
    can register a handler.
- **Aura workaround:** the Aura crash screen covers managed exceptions only.
  For faults, read the serial panic (registers plus a raw stack trace) and
  symbolicate it with `.claude/skills/crash-debug`.
- **Upstream ask:** a public `OnPanic(in PanicInfo)` seam: allocation-restricted,
  with the vector, RIP and CR2.
- **Source:** 01, 02, 08.

### `null-deref`: hardware faults are not managed exceptions (major)
- **Gap:** #PF, #DE and stack overflow are not translated into
  `NullReferenceException` / `DivideByZeroException`. A null dereference or an
  integer `/0` halts the kernel.
  - gen2 identity-mapped low memory, so ported code is full of latent null
    dereferences.
- **Aura workaround:** init-order fixes, plus explicit null and divisor checks
  on user-driven and optional-data paths (rule C6).
- **Upstream ask:** a null-page #PF raises an NRE, and #DE raises
  `DivideByZeroException`, as in CoreCLR.
- **Source:** 02, 05, 08.

### `unhandled`: unhandled exceptions are unobservable (major)
- **Gap:** `AppDomain.UnhandledException` never fires. An escaped exception
  ends in `ExceptionHelper.FailFast`, which prints `[FAILFAST]` on serial and
  spins.
- **Aura workaround:** try/catch in `BeforeRun`, `Run` and every thread body,
  routed to `Crash.StopKernel`.
- **Upstream ask:** wire `OnUnhandledException` / `AppDomain.UnhandledException`.
- **Source:** 02.

### `finally`: `finally` / `using` / `lock` release do not run during unwinding (major)
- **Gap:** fault and finally funclets are skipped when an exception unwinds
  through them. A `using` stream stays open, and a `lock` stays held.
- **Aura workaround:** catch inside the scope, then `Dispose()` / close
  explicitly after the `try/catch` (rule C7). Writers installed with
  `Console.SetOut` never throw.
- **Upstream ask:** run fault/finally funclets during the second pass.
- **Source:** 07.

### `eh-global`: the exception-dispatch guard is a single static (major)
- **Gap:** `ExceptionHelper.s_isHandlingException` is global, not per thread.
  - Two threads throwing concurrently end in a "Recursive exception" FailFast.
  - After an unhandled exception the flag stays set, so any later throw also
    FailFasts.
- **Aura workaround:** single-threaded phase 1. Only the FTP server runs
  concurrently.
- **Upstream ask:** a per-thread guard.
- **Source:** 02.

### `finalizers`: no finalizers; fixed descriptor tables (major)
- **Gap:** finalizers never run (`RhSuppressFinalize` is a no-op). The fd table
  is fixed at 64 files and 32 directory streams.
  - Cosmos.Kernel.Core does not export `RhWaitForPendingFinalizers`. The stub
    is left commented out in `src/Cosmos.Kernel.Core/Runtime/Stdllib.cs`, and
    without the `int allowReentrantWait` parameter that NativeAOT's
    `RuntimeImports` p/invoke declares.
  - So any reachable `GC.WaitForPendingFinalizers` or `GC.GetTotalMemory(bool)`
    fails at ld.lld with `undefined symbol: RhWaitForPendingFinalizers`
    (referenced from `GC.NativeAot.cs:732`, in `GC.GetTotalMemory`).
  - The `false` overload fails too, because the method body still references
    the symbol.
- **Aura workaround:**
  - Dispose every `FileStream`, `ZipStorer` and `LuaInterpreter` (rule C8).
    `File.Create(p)` used as a statement becomes `File.Create(p).Dispose()`.
  - The kernel-assembly plug `SRC/Aura_OS/Plugs/GCImpl.cs`
    (`[Plug(typeof(GC))]`) turns `GC.WaitForPendingFinalizers()` into an
    immediate return. That is correct because there is no finalizer queue, so
    there is never anything to wait for.
- **Upstream ask:**
  - an fd-exhaustion diagnostic and/or a bigger or growable table;
  - export `RhWaitForPendingFinalizers(int allowReentrantWait)` as a no-op from
    Cosmos.Kernel.Core, for example in `Runtime/GC.cs` next to
    `RhGetGcTotalMemory`. Aura's `GCImpl` plug can then be deleted.
- **Source:** 03, 07, 08, and the first full build (link error).

### `run-spam`: `Kernel.Start()` logs on every `Run()` iteration (minor)
- **Gap:** two serial lines are written per `Run()` call.
- **Aura workaround:** loop inside `Aura_Boot.Kernel.Run()`, so it is called
  once.
- **Upstream ask:** log once, or behind a verbose flag.
- **Source:** 02.

### `log-sink`: no kernel log sink or history (minor)
- **Gap:** `Log.WriteString` goes straight to the internal serial port. Nothing
  can subscribe or read it back. gen2's `Debugger` plug fed `logs /k`.
- **Aura workaround:** Aura logs only its own messages (`Logs.DoOSLog` /
  `DoKernelLog`, mirrored to serial). Cosmos-internal messages stay serial-only.
- **Upstream ask:** `Log.Subscribe(Action<string>)` or a ring-buffer
  `Log.History`.
- **Source:** 01, 02.

### `gc-trigger`: GC collects only on page-allocator exhaustion (major)
- **Gap:** OrionGC collects only when the page allocator is exhausted. The
  collection is stop-the-world with interrupts off.
- **Aura workaround:** a rate-limited `MemoryInfo.Collect()` from
  `Aura_OS.Kernel.Run`: 1 Hz, or 10 Hz under memory pressure.
- **Upstream ask:** an allocation-budget trigger.
- **Source:** 02, 08.

### `oom`: a failed allocation returns null instead of throwing (major)
- **Gap:** when the heap is exhausted, `AllocObjectSlow` collects once, then
  returns null. `RhpNewArray` / `RhpNewFast` hand that null to managed code: no
  `OutOfMemoryException`. The first access is a page fault at a small offset
  (`CR2=0x8`), far from the allocation, and nothing can catch it.
  - Seen in VMware (256 MB, 203 MB heap) when switching to 3200x2400: each
    screen-sized buffer is 31 MB.
- **Aura workaround:** `Explorer.ChangeResolution` checks the free pages (after
  `MemoryInfo.Collect()`) against the screen-sized buffers a mode needs, and
  refuses the mode with a message. Screen-sized buffers are kept to a minimum:
  the component cache buffer is allocated on first use, and the hidden login
  screen frees its buffers.
- **Upstream ask:** throw `OutOfMemoryException` from the allocation helpers,
  and a `MemoryInfo.TryReserve(bytes)` or largest-free-block query.
- **Source:** found while adding runtime resolution changes (not in the notes).

### `meminfo`: memory figures describe one region / the last GC (minor)
- **Gap:** `MemoryInfo.RamSizeBytes` / `TotalPages` cover only the largest
  Limine region. `GCMemoryInfo` is a snapshot from the last GC.
- **Aura workaround:** label the figure "memory pool". Use
  `GC.GetTotalMemory(false)` for the live heap. That call links only because of
  the `GCImpl` plug (see `finalizers`).
- **Upstream ask:** `InstalledRamBytes`, a live heap figure, and a fix to the
  doc.
- **Source:** 02.

### `cpuinfo`: no CPU information API (minor)
- **Gap:** there is no CPU info API. Also, `g_cpuFeatures = 0` (`kmain.c`), so
  opportunistic ISA checks (`Avx2.IsSupported`, ...) are false at runtime.
- **Aura workaround:** `X86Base.CpuId` and `Stopwatch.Frequency`; scalar or
  `Vector128` blend loops.
- **Upstream ask:** a `CpuInfo` on the ring, and cpuid feature detection at
  boot.
- **Source:** 01, 02.

### `pci`: no typed PCI enumeration nor name tables (minor)
- **Gap:** the PCI view offers only `DeviceNodeInfo` strings.
- **Aura workaround:** parse `Path` / `Description`; Aura keeps its own
  `PciNames` copy.
- **Upstream ask:** a numeric identity (vendor, device, class) plus public name
  tables.
- **Source:** 02.

### `idle-thread`: the UI loop is the scheduler's idle thread (major, phase 2)
- **Gap:** the kernel `Run()` loop is the scheduler's idle thread.
  - It must never block: only `Mutex` and `InterruptEvent` guard idle-thread
    blocking, not `ConditionVariable` (reached by `Thread.Sleep` and contended
    `lock`).
  - It is excluded from `SchedulerInfo.BusyCpuTimeNs`.
  - It gets the same 100 tickets as every worker.
- **Aura workaround:**
  - Never block the UI thread (rule C9); use `TimerManager.Wait` for short
    waits.
  - Phase 2: a ticket boost through the COSMOS0001 scheduler seam.
- **Verified at HEAD (2026-10-02):** starvation is real without any GC.
  - With 2 workers looping on `Thread.Sleep(10)`, the main loop managed fewer than 100 iterations in 60 s.
  - With 1 worker it managed about 4500 in 20 s.
  - So Aura's UI slows down while the FTP server thread runs.
- **Upstream ask:** a dedicated idle thread, so `Run()` becomes an ordinary
  thread, and a ring priority API, or the earlier `MainThreadTickets` boost for
  the idle thread.
- **Source:** 02, 08; kernel investigation 2026-10-02.

### `gc-conservative`: the GC corrupts objects behind interior pointers on other threads' stacks (major)
- **Gap:** found at HEAD, root-caused and patched in this port. It crashes Aura's `ftp` command within seconds.
  - **Primary defect.** `GarbageCollector.Mark.cs` `ScanMemoryRange` and the saved-register scan of switched-out threads pass every word that falls in the heap to `TryMarkRoot` as an object start. `TryMarkRoot` then ORs the mark bit into whatever word sits there.
  - **Effect on Aura.** CoreLib's `ThreadWaitInfo.Wait` keeps `r12 = &_waitMonitor` (an interior byref) across `LowLevelMonitor_TimedWait`. A collection on the UI thread marks `_nativeMonitor`, the `GCHandle<Monitor>` IntPtr, so it becomes `…E059`.
  - **Symptom.** `LowLevelMonitor_Release` reads the handle one byte off and takes a #GP at `+0x27` (FTP thread: `FtpServer.Listen` → `Thread.Sleep`). Depending on the address bits, the GC hangs in `EnumerateReferences` instead.
  - **Second consequence.** Objects reachable only through interior pointers (byrefs, spans) on another thread's stack are not kept alive.
  - **B, same file.** A suspended boot/idle thread, which is Aura's UI thread, is never stack-scanned when another thread collects.
  - **C, `Runtime/Memory.cs`.** The allocation helpers store the MethodTable after `AllocObject` has re-enabled interrupts. A preempting collection sweeps the zero-header object, and the next allocation overlaps it.
- **Aura workaround:** none possible. The `ftp` command stays as designed. It needs the kernel fix.
- **Upstream fix (validated):** `gc-gp.patch` (Cosmos.Kernel.Core, 4 files).
  - A: `TryMarkConservativeRoot` resolves through `GetParentObject`.
  - B: scan the suspended idle thread from its saved SP to `BootStack.Top`.
  - C: stamp the header inside `AllocObject`'s interrupts-off scope.
  - Results:
    - Repro kernels (sleep/lock/alloc/gcworker variants): unpatched faults in 2.5–35 s; patched runs 150 s × 8 variants with 0 faults.
    - Aura FTP soak: 65/65.
    - GarbageCollector suite 50/50; Threading suite 0 failures.
  - Upstream should also add GC-suite cases for a waiter blocked in `Thread.Sleep`/`Monitor.Wait` while another thread collects.
- **Also open:** the `LowLevelMonitor_*` plug serial-logs every op, and `Thread.Yield` is a no-op.
- **Source:** 02, 06, 07, 08; kernel investigation 2026-10-02.

### `tz-rtc`: no time zone, no RTC write (minor)
- **Gap:** there is no time zone and no way to set the RTC. The SDK does not
  set `InvariantTimezone`.
- **Aura workaround:** `<InvariantTimezone>true</InvariantTimezone>`. Times are
  the firmware clock.
- **Upstream ask:** a settable offset, an RTC write, and an SDK default.
- **Source:** 02, 07.

### `env`: environment variables unusable (minor)
- **Gap:** `Environment.Get*` returns null, and `Set*` fails to link.
- **Aura workaround:** `Kernel.EnvironmentVariables`, plus an `os.getenv`
  override in Lua.
- **Upstream ask:** an in-memory environment block.
- **Source:** 02, 07.

### `pc-speaker`: no PC speaker (minor)
- **Gap:** there is no PC speaker driver, and port I/O (`Native.IO`) is
  internal.
- **Aura workaround:** the `beep` command is dropped.
- **Upstream ask:** a PIT channel 2 driver or a `Console.Beep` plug.
- **Source:** 02, 08.

### `encoding`: no CP437; encoding setters swap the console pipeline (minor)
- **Gap:** there is no CP437 provider. Any `Console.*Encoding` assignment swaps
  the keyboard reader for a `StreamReader` over `ConsoleStream`.
- **Aura workaround:** the encoding lines are deleted.
- **Upstream ask:** compare by CodePage before swapping, and document it.
- **Source:** 02, 07.

## Console / graphics

### `kernelconsole`: the KernelConsole cannot be detached or hidden (minor)
- **Gap:** there is no public way to detach or hide the boot `KernelConsole`.
  `Console` APIs paint the shared canvas.
- **Aura workaround:**
  - Right after acquiring the canvas, call `Console.SetOut(Kernel.GuiSink)`
    and `Console.SetError(Kernel.GuiSink)` (a serial writer).
  - GUI code never calls the Console input or `Clear` APIs (rule C10).
  - KernelConsole paints only on writes and cursor operations; it has no blink
    timer.
  - No `[UnsafeAccessor]` hatch is used.
- **Upstream ask:** a public `IsVisible` / `Suspend()`.
- **Source:** 04, 07, orchestrator amendment A2.

### `console-input`: Console input APIs drain the keyboard queue (major)
- **Gap:** `Console.ReadKey` / `ReadLine` / `KeyAvailable` / `Console.In` drain
  `KeyboardManager`'s queue into tty1.
- **Aura workaround:**
  - The GUI uses only `KeyboardManager` (rules C10/C11).
  - Lua runs with `Input = TextReader.Null`.
- **Upstream ask:** an unrouted `TryReadKey` should also drain the tty1 buffer.
- **Source:** 05, 07.

### `console-global`: `Console.SetOut` is process-global (minor)
- **Gap:** the writer is wrapped in one global `SyncTextWriter`.
- **Aura workaround:** the Terminal swaps its writer in on focus and restores
  `Kernel.GuiSink` when it loses focus.
- **Upstream ask:** a headless `ConsoleSession` or a per-thread hook.
- **Source:** 07.

### `present`: no partial present (minor)
- **Gap:** there is no `Canvas.Display(rect)`, so every present copies the full
  8 MB frame.
- **Aura workaround:** present once per frame (`Kernel.Present()`), and throttle
  terminal presents.
- **Upstream ask:** `Display(x, y, w, h)`.
- **Source:** 04.

### `display-mode`: resolution fixed at boot on GOP / virtio-gpu (minor)
- **Gap:** the resolution is fixed at boot on GOP and virtio-gpu, and the depth
  is 32 bpp only.
- **Aura workaround:** `limine.conf` `resolution:`; read `Canvas.Width/Height`;
  scale assets.
- **Upstream ask:** a documented resolution property, and depth conversion.
- **Source:** 04, 08.

### `blit`: no public memory ops or image resize (minor, partly fixed)
- **Gap:**
  - There are no public memory ops.
  - There is no image resize that keeps the result: the stretched
    `DrawImage` overloads rescale on every call.
  - An `Image` cannot wrap an existing buffer.
- **Fixed upstream** (Cosmos `feature/canvas-compositing`): `DrawImageAlpha`
  blends row by row and keeps the destination's alpha, `DrawCanvasAlpha`
  blends an off-screen canvas with an opacity, and the stretched `DrawCanvas`
  and `DrawImage(image, destination, source)` allocate nothing. Aura's
  components draw on off-screen canvases now, and `DirectBitmap` is gone.
- **Aura workaround:** `ImageUtils.ScaleTo` scales the wallpapers once.
- **Upstream ask:** `Image.Resize`, and a no-copy `Bitmap` constructor.
- **Source:** 04, 08.

### `psf`: PSF2 parsing gaps (minor)
- **Gap:**
  - The PSF2 Unicode table is ignored.
  - The 32-byte header is hard-coded.
  - `(byte)charDataSize` overflows.
  - `DefaultFont` is now 16×32.
- **Aura workaround:** zap 6×12 through `Kernel.font`; `DrawChar` handles
  bytesPerRow.
- **Upstream ask:** a correct PSF2 parse and a glyph-index API.
- **Source:** 04.

### `hw-cursor`: hardware cursor experimental and VMware-only (minor)
- **Gap:** `IHardwareCursor` is experimental (COSMOS0003), and
  `TryDefine` returns false on QEMU.
- **Aura workaround:** a software cursor. Phase 2: the facet.
- **Upstream ask:** a virtio-gpu cursor, and promote the facet.
- **Source:** 04, 05, 08.

### `bmp`: BMP loader limits; `Bitmap.Save` corrupt (minor)
- **Gap:** the BMP loader has no top-down images, no bitfield masks and nothing
  below 24 bpp. `Bitmap.Save` writes zero-length copies.
- **Aura workaround:** catch around loads in `pic` and `ApplicationManager`.
- **Upstream ask:** harden the loader and fix `Save`.
- **Source:** 04, 08.

### `canvas3d`: `Canvas3D` is full-screen and VMware SVGA3D only (minor)
- **Aura workaround:** none needed, no Aura app draws in 3D (the software-rendered CubeApp was removed).
- **Upstream ask:** an off-screen or software `Canvas3D`.
- **Source:** 04.

## Input

### `keyboard-altgr`: layout map defects (major)
- **Gap:**
  - The FR/GB maps lack AltGr.
  - FR/DE/ES lack the ISO 0x56 key.
  - DE/ES/TR lose AltGr under Caps Lock (7-character `KeyMapping`).
- **Aura workaround:** `AuraFRStandardLayout`, built with the 8-character
  constructor, 0x60 = AltGr, and 0x56.
- **Upstream ask:** fix the bundled maps.
- **Source:** 05.

### `key-release`: only key presses are queued (minor)
- **Gap:** there is no held-key query.
- **Aura workaround:** none needed since the GameBoy app was removed (it
  released each key after a fixed time).
- **Upstream ask:** opt-in Break events or a pressed-key bitmap.
- **Source:** 05, 08.

### `key-repeat`: no USB typematic repeat; incomplete virtio keymap (minor)
- **Gap:** there is no typematic repeat for USB keyboards. The virtio-keyboard
  keymap misses Del, Home, End, F11, F12, LWin, the keypad and more.
- **Aura workaround:** PS/2 keyboard in VM configs.
- **Upstream ask:** a repeat timer and a complete keymap.
- **Source:** 05.

### `e0`: E0 prefix dropped (minor)
- **Gap:** with NumLock on, the arrows, Home and End arrive as keypad keys.
- **Aura workaround:** leave NumLock off.
- **Upstream ask:** keep an extended bit.
- **Source:** 05.

### `sessions`: a second console session swallows Alt+F1..F12 (minor)
- **Gap:** once a second console session exists (a virtual console or Telnet),
  the key router swallows Alt+F1..F12 and Alt+arrows, including Alt+F4.
- **Aura workaround:** never open sessions or Telnet (rule C11).
- **Upstream ask:** an opt-out, or a GUI owner for tty1.
- **Source:** 05, 08.

### `mouse`: mouse model limits (minor)
- **Gap:**
  - Buttons are level-only.
  - There is no absolute or USB pointer.
  - The wheel sign likely differs between PS/2 and virtio.
  - `Sensitivity` truncates.
- **Aura workaround:** edge detection, PS/2 mouse, `Sensitivity` 1.0.
- **Upstream ask:** press counters, a tablet/USB mouse, and a documented sign.
- **Source:** 05.

### `ps2-sync`: the PS/2 drivers never resynchronise (major on real hardware)
- **Gap:** found at HEAD and patched in this port.
  - `HAL.X64/Devices/Input/PS2Mouse.cs` checks byte-0 bit 3 only after a whole packet has arrived, and keeps the alignment. One extra or lost byte (an ACK, a stray 00, a lost byte) freezes an IntelliMouse until reboot, after one phantom jump or click.
  - Both IRQ handlers read port 0x60 without checking OBF or AUX.
  - The config byte is read back (0x20) while IRQ1 is live. That causes the phantom `F7` key press on every boot.
  - An edge that arrives on a masked I/O APIC pin is lost at unmask, which freezes keyboard and mouse together.
  - The keyboard driver passes ACK/resend bytes, E0 fake shifts and the E1 Pause sequence through as keys.
- **Aura workaround:** none. QEMU's natural input does not trigger the desync. QMP automation must send at most ±127 per rel event: QEMU queues up to 4 packets per sync and keeps the rest, which looks like a drifting or frozen cursor.
- **Upstream fix (validated):** `ps2-mouse.patch` (HAL.X64, 3 files).
  - Sync byte 0 on arrival with the 0xC8/0x08 mask, and drop a partial packet after 100 ms of silence (TSC).
  - A single status-checked `HandleOutputBuffer` routes bytes by AUX.
  - Service the buffer after unmasking; cache the config byte.
  - Filter keyboard control bytes.
  - Results: fault-injection repro 14/14 (stock 10/14, then a dead mouse); 60-round soak clean; no phantom F7.
- **Source:** kernel investigation 2026-10-02.

### `key-docs`: `KeyEvent.Key` is layout-dependent (minor)
- **Gap:** the docs say `KeyEvent.Key` is the physical key, but it depends on
  the layout. `Menu` is unmapped in all layouts.
- **Aura workaround:** per-layout bindings where needed.
- **Upstream ask:** fix the docs and the maps.
- **Source:** 05.

## Filesystem / storage

### `ide`: no IDE / PATA / ATAPI driver (major)
- **Gap:** only AHCI, NVMe and USB mass storage are supported.
- **Aura workaround:** VMs must use SATA/AHCI or NVMe disks.
  `.cosmos/config.json`, run-aura and crash-debug all attach `disk.img` on
  AHCI.
- **Upstream ask:** an IDE driver.
- **Source:** 03.

### `fat-names`: the FAT driver accepts any character in long names (major)
- **Gap:** a missed gen2 drive-letter path literal (volume 0, backslash
  separators) silently writes junk directory entries.
- **Aura workaround:** convert every path literal to the `/N/...` form, and
  validate user input with `AuraPath.IsValidName`.
- **Upstream ask:** validate names.
- **Source:** 03.

### `driveinfo`: `DriveInfo` not wired (minor)
- **Aura workaround:** `VfsManager.TryStatFs` through `Volumes.TryGetSpace`.
  The result is cached, because each call is a full FAT sweep.
- **Upstream ask:** plug statfs / mount enumeration, and use the FSInfo free
  count as a shortcut.
- **Source:** 03, 08.

### `fat-label`: no API to read or set a FAT volume label (minor)
- **Gap:** the formatter writes `VolumeLabel` in the boot sector only. Other
  systems read the root folder's volume label entry, so they show no label,
  and `fsck.vfat` removes it.
- **Aura workaround:** `FatVolume` reads and writes the BPB bytes at 0x2B /
  0x47 (and the FAT32 backup boot sector), and the root folder's label entry.
- **Upstream ask:** a label API; the formatter writes the root entry too.
- **Source:** 03, Disk Manager.

### `fat-resize`: no FAT volume resize (minor)
- **Gap:** `PartitionManager.Resize` changes the table only, and nothing grows
  or shrinks a FAT volume.
- **Aura workaround:** the Disk Manager lets a FAT partition grow (its volume
  keeps its size until a format) and refuses to shrink it below its volume.
- **Upstream ask:** a FAT grow and shrink.
- **Source:** Disk Manager.

### `fat-time`: FAT timestamps always 0 (minor)
- **Gap:** timestamps are always 0 (1970), and `SetLastWriteTime` is a no-op.
- **Aura workaround:** clamp in ZipStorer; don't sort by date.
- **Upstream ask:** implement timestamps.
- **Source:** 03, 08.

### `mounts`: mount-table rough edges (minor)
- **Gap:**
  - No parent is synthesized for nested mount points.
  - The format and mount guards compare by reference.
  - `TryMount` allows duplicates.
  - `MountFlags` (ReadOnly) are not enforced.
  - There is no hot-plug event.
- **Aura workaround:**
  - Top-level `/N` mount points only.
  - Host plus StartSector checks (`Volumes.IsMounted`).
  - `Volumes.PollHotplug()` from `Run`.
- **Upstream ask:** fix the guards, enforce the flags, and add a hot-plug
  callback.
- **Source:** 03.

### `vfs-threads`: VFS and PAL fd table not thread-safe (major, phase 2)
- **Aura workaround:** single-threaded phase 1. The FTP server is the only
  concurrent VFS user.
- **Upstream ask:** locking.
- **Source:** 03.

### `tmp`: no temp files (minor)
- **Gap:** `Path.GetTempFileName` is unplugged and there is no `/tmp`, so Lua's
  `os.tmpname` fails.
- **Aura workaround:** `.tmp` files on the same volume.
- **Upstream ask:** a `MksTemps` plug or a tmpfs.
- **Source:** 03.

### `cwd`: the current directory is kernel-global (minor)
- **Gap:** the current directory is kernel-global, and
  `VfsManager.CurrentDirectory` is internal.
- **Aura workaround:** the `Kernel.CurrentDirectory` property setter syncs
  `Directory.SetCurrentDirectory`.
- **Upstream ask:** a per-thread current directory.
- **Source:** 03.

### `mbr`: `Mbr.IsMbr` checks only 0x55AA (minor)
- **Gap:** it is therefore true on superfloppies.
- **Aura workaround:** check GPT first; treat a superfloppy as one partition at
  LBA 0.
- **Upstream ask:** real MBR validation.
- **Source:** 03.

### `gpt-crc`: GPT written without checksums or backup (major)
- **Gap:** `Gpt.Create` writes a CRC of 0 and no backup header or array, and
  `AddPartition`, `RemovePartition`, `ResizePartition` and `MovePartition`
  leave a valid table's CRCs and backup stale. Other systems then take the
  disk for a damaged GPT, or read the stale backup, the old partitions.
- **Aura workaround:** `GptChecksums.Update` after each GPT change: both
  CRC-32s, then the backup array and header at the end of the disk (checked
  with `sgdisk -v`). Writing an MBR also wipes a GPT disk's backup header.
- **Upstream ask:** the writers keep both copies and their CRCs.
- **Source:** Disk Manager.

### `gpt-type`: no API changes a GPT partition's type (minor)
- **Gap:** `Gpt` adds, removes, resizes and moves entries, but a format to
  another filesystem cannot change the entry's type GUID.
- **Aura workaround:** `GptChecksums.SetPartitionType` rewrites the entry, then
  the checksums and backup: Basic data for FAT, Linux filesystem for ext2.
- **Upstream ask:** `Gpt.SetPartitionType`.
- **Source:** Disk Manager.

The ext2 entries below were checked against Cosmos main `f9103822` (the ext2
driver of PR #480), package 3.0.89.20261003, on disk images, with `e2fsck -fn`.

### `ext2-format`: the ext2 formatter's layout breaks past one group (major)
- **Gap:** `Ext2Formatter` puts each group's block and inode bitmaps at the
  group's first block, where the superblock's backups go. Once the volume has
  more than one group (8 MB), `e2fsck` stops at "Corrupt group descriptor: bad
  block for block bitmap", and asks to relocate every group's bitmaps. On any
  volume it also leaves the bitmaps' padding bits clear, and on 1 KB blocks it
  counts one free block too many.
- **Aura workaround:** `Ext2Volume.Format` writes the volume as mke2fs does:
  4 KB blocks, 32768 blocks a group, sparse superblock backups (groups 0, 1
  and the powers of 3, 5 and 7), the padding set, a lost+found folder. `e2fsck`
  passes it from 1 MB to 17 GB, and Cosmos's driver mounts it and writes files
  that Linux reads back.
- **Upstream ask:** the same layout in `Ext2Formatter`.
- **Source:** Disk Manager.

### `ext2-1k`: the ext2 driver writes 1 KB-block volumes one block off (major)
- **Gap:** `TryAllocateBlock` and `FreeBlock` turn a bitmap bit into the block
  `group * BlocksPerGroup + bit`, without the first data block. That is 1 on a
  volume of 1 KB blocks, so each block written lands one block early, over the
  metadata: after Cosmos writes files on a `mke2fs -b 1024` volume, `e2fsck`
  finds "Inode 7 has illegal block(s)".
- **Aura workaround:** `Ext2Volume.CanMount` refuses volumes of 1 KB blocks;
  Aura formats with 4 KB ones.
- **Upstream ask:** add `FirstDataBlock` in both.
- **Source:** Disk Manager.

### `ext2-features`: the ext2 driver mounts any ext superblock (minor)
- **Gap:** `Ext2Superblock.TryCreate` does not check the feature flags: it
  mounts ext3 (a journal it does not replay) and ext4 (extents it does not
  read) as ext2, and writes them. `MountFlags.ReadOnly` is not enforced either
  (see `mounts`), so an unknown read-only-compatible feature cannot be kept
  safe.
- **Aura workaround:** `Ext2Volume.CanMount`: no journal, no incompatible
  feature but file types, no read-only-compatible one but sparse superblocks
  and large files.
- **Upstream ask:** refuse unknown incompatible features, and mount read-only
  on unknown read-only-compatible ones, as Linux's ext2 does.
- **Source:** Disk Manager.

### `ext2-dtime`: deleted inodes keep a zero dtime (minor)
- **Gap:** an unlink frees the inode with its links at 0 but leaves `i_dtime`
  at 0, so `e2fsck` reports "Deleted inode N has zero dtime" for each file
  Cosmos deleted. `e2fsck -p` fixes them without asking.
- **Aura workaround:** none.
- **Upstream ask:** stamp `i_dtime` on delete.
- **Source:** Disk Manager.

## Network

### `http-tls`: no HttpClient, no SslStream (minor)
- **Gap:** `HttpClient` and `SslStream` are not plugged, and the BCL's TLS
  and cryptography route to OpenSSL.
- **Aura workaround:** the `Cosmos.Network.Http` 3.0 package, .NET
  nanoFramework's `System.Net.Http` (`HttpClient`, `HttpListener`) ported
  over `Socket`. A request waits by polling, so it runs on the UI thread
  (`wget`, the package manager); `HttpListener.GetContext` sleeps between
  rounds, so `httpd` serves on a thread of its own. Its `https` runs
  BouncyCastle's managed TLS 1.3/1.2 against the Mozilla roots it embeds,
  and needs the `RandomNumberGenerator` plug (Cosmos `40c043ab3`) to link.
- **Upstream ask:** `SslStream` and `HttpClient` plugs, or the package as
  the supported way.
- **Source:** 06, 08.

### `tcp-receive`: TCP receive returns 0 while open; span paths broken (major)
- **Gap:**
  - `Receive` / `Read` return 0 while the connection is open (a spin-count
    timeout).
  - `NetworkStream.Read(Span)` is unplugged.
  - `Receive(Span)` dropped data and a read racing the receive path could
    lose or misplace bytes: both fixed in Cosmos `570d927c4`.
- **Aura workaround:** the `Available`/`Poll` idiom with a deadline; receive
  into `byte[]` only (rule C15).
- **Upstream ask:** a blocking receive, and plug `NetworkStream.Read(Span)`.
- **Source:** 06, 08.

### `ipaddress`: `IPAddress` plug defects (major)
- **Gap:**
  - `Parse` returns null.
  - `Equals` is always true.
  - The hash is constant.
  - String interpolation prints 0.0.0.0.
- **Aura workaround:** explicit `.ToString()`, compare `GetAddressBytes()`,
  `TryParse` (rule C15).
- **Upstream ask:** store the address in the BCL field.
- **Source:** 06.

### `socket-misc`: socket API rough edges (minor)
- **Gap:**
  - The host-name overloads don't resolve DNS.
  - Errors are bare `Exception`s.
  - UDP `Available` counts datagrams.
  - `Poll` ignores its timeout.
- **Aura workaround:** resolve first (`NetworkHelper.Resolve`); catch
  `Exception`.
- **Upstream ask:** `SocketException`, and byte counts.
- **Source:** 06.

### `dhcp`: DHCP client defects (minor)
- **Gap:**
  - The ACK is applied to the first device only.
  - There is no renewal.
  - A NAK is applied as success.
  - It throws with two or more NICs.
  - Release throws when unconfigured.
  - A 0.0.0.0 config is left behind.
- **Aura workaround:** `NetworkHelper` checks, `RemoveAllConfigIP`, try/catch.
- **Upstream ask:** fix the client, and add an `IsConfigured` API.
- **Source:** 06.

### `tcp-primary`: TCP always sources from `NetworkManager.Primary` (minor)
- **Aura workaround:** make the configured NIC primary.
- **Upstream ask:** route-based source selection.
- **Source:** 06.

### `nic-names`: no interface names or types (minor)
- **Aura workaround:** `eth{Index}` aliases (`NetworkHelper.AliasOf`).
- **Upstream ask:** stable names.
- **Source:** 06.

### `net-threads`: network stack has no locks (major, phase 2)
- **Gap:** for example, the DNS port 53 registry is shared.
- **Aura workaround:** single-threaded phase 1; avoid DNS while FTP is busy.
- **Upstream ask:** locking.
- **Source:** 06.

### `nic-drivers`: only E1000E and virtio-net (major)
- **Gap:** there are no drivers for e1000 82540/82545, PCnet or RTL.
- **Aura workaround:** VM configs use `e1000e` or `virtio-net`.
- **Upstream ask:** more drivers.
- **Source:** 06.

### `tcp-robust`: minimal TCP (major)
- **Gap:**
  - There is no retransmission.
  - `WaitAck` spins.
  - MSS is 536 and the window is fixed.
  - Close takes 5 s.
- **Aura workaround:** timeouts on the Aura side.
- **Upstream ask:** TCP hardening.
- **Source:** 06.

### `net-misc`: miscellaneous network gaps (minor)
- **Gap:**
  - There is no `System.Net.NetworkInformation`.
  - `IcmpClient` doesn't match the id/seq.
  - `Dns` uses only `Nameservers[0]` and A records.
  - The Cosmos `UdpClient.Send(byte[], Address, int)` doesn't pump.
  - `FtpServer` can't detect a port clash.
  - There is serial spam on every socket call.
- **Aura workaround:** use the Cosmos clients; run a single FTP server.
- **Upstream ask:** various, per item.
- **Source:** 06.

## BCL / packages

### `deflate`: no public DEFLATE (minor)
- **Gap:** BCL compression is native, and the vendored SharpZipLib `Inflater`
  is internal.
- **Aura workaround:** `Zlib.Portable` (Ionic.Zlib).
- **Upstream ask:** plug `System.IO.Compression`, or make the inflater public.
- **Source:** 07, 08.

### `crypto`: BCL hashes route to OpenSSL (minor)
- **Gap:** BCL hashes route to OpenSSL. The secure RNG is fixed in Cosmos
  `40c043ab3`: `RandomNumberGenerator` and `Guid.NewGuid` come from a kernel
  CSPRNG (RDSEED/RDRAND or RNDR, plus timer jitter, into ChaCha20).
- **Aura workaround:** Aura's own Sha256/MD5, plus `acryptohashnet`.
- **Upstream ask:** a managed hash plug.
- **Source:** 07, 08.

### `lua-host`: Cosmos.Executable.Lua 4.0.1 host hooks (minor)
- **Gap:**
  - There is no virtual-file loader.
  - Callbacks can't see host settings.
  - `os.getenv` is hard-wired (it reads `Environment`, always empty on gen3).
  - There is no cancellation hook: `SetHook` is internal, only Lua's
    `debug.sethook` reaches it.
  - On `ILuaState` a Lua string is its UTF-8 bytes, one character each.
  - `io` reads a file one `ReadByte` at a time from a `FileStream` opened with
    `bufferSize: 1`: each byte is a call into the VFS.
- **Aura workaround:** `package.preload`, closures, an `os.getenv` override,
  `LuaText.Encode`/`Decode` wherever a C# function takes or gives text, and
  `aura.fs.readText`/`writeText` (`File.ReadAllText`/`WriteAllText`) for whole
  files.
- **Upstream ask:** a loader hook, a host accessor, an env `Func`, a
  public `SetHook`, and reads buffered like writes.
- **Source:** 07.

---

## Other upstream findings (no Aura workaround site)

- **The Stride scheduler's interactive wake boost is dead code.**
  `ReadyThread` sets `Ready` before `OnThreadReady` tests `wasBlocked`, so no
  thread is ever classified as interactive. Ask: test the previous state,
  captured before the write. (08)

## Items from the 3.0.77 list (origin/gen3) re-checked at HEAD

**Fixed at HEAD.** These can be closed upstream:

| Old § | Item | Status at HEAD |
|---|---|---|
| 1.1 | libm `fmod/cosh/sinh/tanh` missing | fixed (`Runtime/Math.cs`) |
| 1.2 | `RhCpuIdEx` stub missing | fixed (`CpuOps.s`) |
| 1.3 | `MemSet` tail corruption | fixed; `MemoryOp` is internal now |
| 1.4 | `CosmosDefaultFont` key | fixed (`Sdk.props`) |
| 1.5 | `ResetScrollDelta` | fixed (public) |
| 1.6 | DarkMagenta palette | fixed |
| 1.7 / 1.8 | RIP + stack trace in the panic | fixed (`Cosmos.Kernel/Panic.cs`) |
| 1.9 | `Canvas.Mode` reported a wrong resolution | fixed (the canvas re-reads the display) |
| 1.10 / 1.11 | GC hang / mark stack out of bounds | fixed |
| 2.1 | ICMP | fixed (`IcmpClient`) |
| 2.4 | US layout only, no E0, LEDs TODO | fixed: 7 layouts, E0 handled, PS/2 LEDs. Remaining: `keyboard-altgr`, `e0` (NumLock case), `key-release`. |
| 2.7 | `Accept()` returned the listener | fixed (`9d1dd320e`) |
| 2.9 | `System.Net.Dns` not plugged | fixed (`NameResolutionPalPlug`) |
| 3.6 | `Console.ReadKey` didn't block | changed: blocks through the session |
| 3.7 | MSB4011, restore trap, NAOT0007 noise, plugs.md | fixed. The broken `~/.cosmos/tools/bin` qemu wrapper is a stale local install, not an upstream issue. |

**Still open, or partly fixed,** under the tags above:

| Old § | Tag |
|---|---|
| 2.2 | `iso-files` |
| 2.3 | `display-mode` (partly fixed: honest API, switchable on SVGA II) |
| 2.5 | `cpu-exception` |
| 2.6 | `null-deref` |
| 2.8 | `tcp-receive` |
| 2.10 | `http-tls` |
| 2.11 | `deflate` (worse: the inflater is internal now) |
| 2.12 | `crypto` |
| 2.13 | `finalizers` |
| 2.14 | `driveinfo` (partly fixed: `TryStatFs`) |
| 2.15 | `fat-time` |
| 2.16 | `mounts` |
| 2.17 | `pc-speaker` |
| 2.18 | `cpuinfo` |
| 3.1 | `blit` |
| 3.2 | `bmp` |
| 3.3 | `resources-perf` |
| 3.4 | `log-sink`, `gc-conservative` (the old #GP, now root-caused) |
| 3.5 | `kernelconsole` |
| 3.8 | `tz-rtc`. `GetFiles` returning full paths is standard BCL behaviour, not a gap. |
