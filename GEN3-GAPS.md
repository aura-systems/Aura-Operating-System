# Cosmos gen3 gaps found while porting AuraOS

Findings from porting AuraOS (gen2/IL2CPU) to Cosmos gen3 (nativeaot-patcher,
packages 3.0.63.20260714). Each entry is written to be liftable into a GitHub
issue. Grep the Aura source for `GEN3-GAP(` to find every workaround site.

Status legend: **FIXED** = already fixed on nativeaot-patcher branch
`feat/aura-port-gaps` during this port · **OPEN** = needs an upstream issue.

---

## 1. Fixed during the port (branch `feat/aura-port-gaps`)

### 1.1 Kernel libm lacks `fmod`, `cosh`, `sinh`, `tanh` — FIXED
Any kernel using the C# `%` operator on doubles or `Math.Cosh/Sinh/Tanh` failed
to **link** (`ld.lld: undefined symbol: fmod/cosh/sinh/tanh`). Hit by UniLua's
math library. Fix: fdlibm-style shared C# implementations with `[RuntimeExport]`
in `Cosmos.Kernel.Core/Runtime/Math.cs` (both arches), incl. float wrappers.

### 1.2 `RhCpuIdEx` native stub missing — `X86Base.CpuId` unusable — FIXED
`System.Runtime.Intrinsics.X86.X86Base.CpuId` compiles but fails to link
(`undefined symbol: RhCpuIdEx`). Needed for CPU brand string (gen2
`CPU.GetCPUBrandString` parity). Fix: `RhCpuIdEx` added to
`Cosmos.Kernel.Native.X64/CPU/CpuOps.s` (SysV, preserves RBX).

### 1.3 `MemoryOp.MemSet(uint*)` corrupts the unaligned tail — FIXED
`FillWithSimd` filled the last `count % 16` bytes with only the **low byte** of
the 32-bit pattern. Any ARGB fill whose pixel count wasn't a multiple of 4 got
wrong trailing pixels. Fix: phase-aware tail fill in
`Cosmos.Kernel.Core/Memory/MemoryOp.cs`.

### 1.4 `CosmosDefaultFont` MSBuild property never worked — FIXED
`Sdk.props` registered the knob under `...Fonts.CustomFont` while
`PCScreenFont.DefaultFont` reads `...Fonts.DefaultFont` — the override was
silently ignored (and the bogus default value pointed at a nonexistent
resource). Fix: key corrected, default removed.

### 1.5 `MouseManager.ScrollDelta` can never be consumed — FIXED
The delta is overwritten on each wheel event and never cleared, so pollers
re-process a stale delta every frame forever. gen2 had `ResetScrollDelta()`.
Fix: `ResetScrollDelta()` added. (Follow-up idea: gen2-parity `MouseState`
flags or an event queue so GUIs don't hand-roll edge detection.)

### 1.6 `KernelConsole` palette: DarkMagenta duplicated DarkYellow — FIXED
Both were `0xFF808000`. DarkMagenta is now `0xFF800080`.

### 1.7 x64 CPU-exception dump had no RIP — FIXED
`IRQContext` (x64) dropped the hardware interrupt frame, so a page-fault panic
printed CR2 and GPRs but **not the faulting instruction pointer** — nearly
impossible to symbolicate. The stub already leaves RIP/CS/RFLAGS/RSP right
after the info block (`ThreadContext.X64` layout); the struct now exposes
`temp_rcx/rip/cs/rflags/rsp/ss` and `Panic` prints RIP/RSP/RFL.
(This immediately localized a real Aura crash to
`RegionListBuilder.createAndPlaceRegions` via `nm`.)

### 1.8 CPU-exception panic now prints a stack trace — FIXED
Follow-up to §1.7: RIP alone doesn't identify the caller when the fault hits a
shared leaf helper (`RhpAssignRef`, memcpy, …). `Panic.CpuException` now walks
the frame-pointer chain (allocation-free, plausibility-gated: higher-half,
aligned, bounded window, capped frames; skipped when CR2 is near RSP to avoid
double-faulting on stack overflow) and prints raw return addresses plus the
`[rsp]` leaf-return slot, x64 and ARM64. Symbolicate with `nm`.
(Immediately localized a second masked Aura null-deref:
`LoginScreen.Hide` storing `Kernel.MouseManager.FocusedComponent` before
`MouseManager` was constructed — see §2.6.)

### 1.9 `Canvas.Mode` lied about the resolution after `GetFullScreenCanvas(Mode)` — FIXED
The boot `KernelConsole` creates the GopCanvas first (adopting the real Limine
framebuffer size). A later `GetFullScreenCanvas(mode)` call hit the
already-created path and ran `videoDriver.Mode = mode`, which stored the
*requested* mode verbatim while `SetMode` is a no-op — so `Canvas.Mode` read
back a resolution the hardware never switched to (Aura laid out its whole UI
for 1920×1080 on a 1280×800 framebuffer: taskbar 247 px below the screen).
Fix: `GopCanvas.Mode`'s setter keeps reporting the real `driver.Width/Height`,
and the real framebuffer resolution is never validated against the legacy
`AvailableModes` VBE list (which lacks e.g. 1280×800). Related: §2.3.

### 1.10 GC hang at first collection: TLAB gap double-stamped into free list — FIXED
`RefillAllocContext` stamps the old TLAB gap into the free list *before*
trying to acquire a new buffer, but left `AllocPtr/AllocLimit` intact; when
every refill attempt failed (heap exhausted — the only situation that triggers
`Collect()`), `ReturnAllAllocContexts()` stamped the SAME gap again. The
second head-insert made the FreeBlock point at itself, and the free-list walk
at the top of `Collect()` (`GetCurrentFragmentation`) then spun forever with
interrupts disabled — the "hang at `[GC] Collection #1` when opening windows"
bug. Fix: `StampUnusedTlab` now nulls the context after donating the gap.
Found with a temporary O(list) duplicate/overlap tripwire in `AddToFreeList`;
what remains permanently: an O(1) duplicate-head guard there, and cycle guards
in the fragmentation walkers (degrade the metric instead of hanging).

### 1.11 GC mark stack: capacity 4096 entries over a 512-entry allocation — FIXED
`Initialize` set `s_markStackCapacity = 4096` but allocated ONE 4K page
(512 nint slots); `PushMarkStack` bounds-checks against the capacity, so any
mark phase with >512 pending objects wrote up to 28KB past the page. Demo
kernels never hit it; Aura's UI object graph does immediately. Fix: capacity
is now computed from the actual page allocation.

---

## 2. Open — missing features (gen2 parity)

### 2.1 ICMP is not implemented at all
No `ICMPClient`, and `IPPacket`'s IPv4 handler drops protocol 1 — **ping is
impossible in both directions** (docs acknowledge it). gen2's
`ICMPPacket`/`ICMPEchoRequest`/`ICMPEchoReply`/`ICMPClient` port almost
verbatim next to the existing UDP/DHCP/DNS classes (which are themselves gen2
ports), plus a `case 1:` dispatch and an echo responder.
*Aura impact: `ping` command stubbed (`GEN3-GAP(icmp)`).*

### 2.2 No ISO9660 driver and no way to ship files on the boot ISO
Two halves of one problem:
- `_BuildIsoCore` wipes `$(IsoRoot)` and stages only the ELF + limine files —
  no `@(CosmosIsoFile)`-style item, and no extension point survives the
  `RemoveDir` (ISO cache hash also ignores foreign files).
- Even if files were added, only FAT is mountable; there is no ISO9660
  read-only `IVfsFilesystemType` and no Limine module request, so a kernel
  cannot read its own boot medium.
*Aura impact: 40 UI assets (icons/wallpapers/fonts/theme) moved to
`<EmbeddedResource>` + `ResourceManager`; gen2's installer "copy from live CD"
flow is gone (`GEN3-GAP(iso9660)`).*

### 2.3 Video mode is fixed at boot and the API pretends otherwise
`FullScreenCanvas.GetFullScreenCanvas(Mode)` accepts a mode and **silently
ignores it** (GopCanvas adopts the Limine framebuffer; `SetMode`'s body is
commented out; `AvailableModes` lists 11 unsettable modes). Either implement
GOP/Limine mode plumbing or make the API honest (throw/return actual).
*Aura impact: resolution read back from `Canvas.Mode`; settings.ini resolution
and the Settings app picker are display-only (`GEN3-GAP(video-mode)`).*

### 2.4 Keyboard: US layout only, no 0xE0 extended scancodes, LEDs TODO
Only `USStandardLayout` ships; arrows/Ins/Del/Home/End rely on numpad-code
coincidence (break with NumLock); `PS2Keyboard.UpdateLeds()` is an empty TODO.
`ScanMapBase` is public, so consumers can add layouts — Aura now carries an
AZERTY `FRStandardLayout` worth upstreaming.
*Aura impact: `setkeyboardmap fr` works via Aura-side layout; other layouts
stubbed.*

### 2.5 No user hook for panics / CPU exceptions
`Panic.CpuException` is hard-coded serial-dump + halt; a custom crash screen
requires re-registering ~19 exception vectors via `InterruptManager.SetHandler`
and mirroring `ExceptionHandler.Initialize`'s private vector list. A supported
`Panic.SetHandler(...)` (or `Kernel` virtual) would fix it.
*Aura impact: crash screen only reachable from managed `catch` in `Kernel.Run`;
BSOD for CPU exceptions not yet wired.*

### 2.6 Null dereference = unrecoverable #PF panic, not `NullReferenceException`
NativeAOT normally converts low-address faults into managed NREs; gen3's
exception handler just panics. Made worse by gen2 habits: IL2CPU kernels
identity-mapped low memory, so gen2 code that dereferenced null *fields* often
"worked" (read garbage) — the same code on gen3 hard-halts (bit Aura in
`RegionListBuilder`, see §1.7). Converting #PF with CR2 < 64K into a managed
NRE on the faulting frame would make ports far more debuggable.

### 2.7 Server sockets are unusable: `SocketPlug.Accept` semantics
`Accept()` busy-spins forever with no cancellation, ignores backlog, and
**returns the listening socket itself** — one concurrent connection per
listener. Blocks any FTP/HTTP server workload.
*Aura impact: gen2's CosmosFtpServer-based `ftp` command stubbed
(`GEN3-GAP(ftp-server)`).*

### 2.8 `Socket.Receive`/`NetworkStream.Read` returns 0 spuriously
`SocketPlug.ReceiveTcp` waits with a **fixed-iteration** counter
(`while (timeout < 100000) timeout++;`) then returns 0 while the connection is
still ESTABLISHED — EOF is indistinguishable from "no data yet", and the
effective wait scales inversely with CPU speed. Also `Socket.Connected` is
false in CLOSE_WAIT even though buffered data remains drainable. Receive should
be time-based (or truly blocking) and distinguish EOF.
*Aura impact: HTTP client reads use a `DateTime` deadline + `DataAvailable`
workaround (`GEN3-GAP(tcp-read)`).*

### 2.9 `System.Net.Dns` not plugged
`TcpClient.Connect("hostname", port)` only accepts IP literals; kernels must
resolve manually with the Cosmos `DnsClient`. Backing `Dns.GetHostAddresses`
with `DnsClient` would make standard BCL networking code portable.

### 2.10 No kernel HTTP client story
CosmosHttp (gen2) targets gen2 kernel TCP and cannot be referenced. With raw
`TcpClient` as the only option, every downloader hand-rolls HTTP/1.1 + chunked
decoding (Aura now does, in `System/Network/Http.cs`). A minimal HTTP/1.1 GET
client (or HTTP-only `HttpClient` handler plug) would serve every OS project.
No TLS/HTTPS either (acknowledged on the roadmap).

### 2.11 No deflate encoder anywhere
The vendored SharpZipLib is **inflate-only** ("the compression side itself is
not vendored") and BCL `System.IO.Compression` P/Invokes a native zlib that
doesn't exist on bare metal. Kernels can decompress but never compress.
*Aura impact: `ZipStorer.Store()` downgraded to STORE method
(`GEN3-GAP(deflate)`).*

### 2.12 `System.Security.Cryptography` unusable
No crypto plugs (BCL hashes P/Invoke OpenSSL shims), and
`GetCryptographicallySecureRandomBytes` forwards to the **non-secure** RNG.
Managed plugs for one-shot hashes (`SHA256.HashData` etc.) would cover the
common password-hashing need.
*Aura impact: kept its hand-written MD5/SHA-256; acryptohashnet (pure managed)
compiles but is untested at runtime (`GEN3-TODO`).*

### 2.13 Finalizers are not implemented
`RhpNewFinalizable` allocates as plain memory (upstream TODO) and
`RhSuppressFinalize`/`RhWaitForPendingFinalizers` are no-ops. Consequence: an
undisposed `FileStream` **permanently leaks one of the 64 fds** and can wedge
later delete/replace of that file (delete-pending). At minimum document it;
Aura had four such `File.Create()` sites, now disposed.

### 2.14 Free space / `DriveInfo` not wired to StatFs
`ISuperblockOperations.StatFs` returns real `Blocks/Bfree/BlockSize` for FAT,
but no `System.IO` surface reaches it — `DriveInfo` is unimplemented. Plugging
the Interop.Sys mount-point/space-info PAL calls onto StatFs would light up
`DriveInfo.AvailableFreeSpace/TotalSize`.
*Aura impact: free-space queries go through
`VfsManager.TryGetMount(...).Superblock.SuperOperations.StatFs` directly.*

### 2.15 FAT timestamps: epoch on read, no-op on write
`FatInodeOperations.GetAttr` hard-codes `Atime/Mtime/Ctime = (0,0)` although
the on-disk dir entries carry WrtTime/WrtDate; `UTimensat`/`FUTimens` succeed
without writing. `File.GetLastWriteTime` returns the epoch for every file.

### 2.16 `MountFlags.ReadOnly` accepted but ignored by FAT
`VfsManager.TryMount` takes the flag; the FAT driver never checks it.

### 2.17 PC speaker / beep missing
gen2 `PCSpeaker.Beep()` has no equivalent (trivial PIT ch2 + port 0x61).
*Aura impact: `beep` command reimplemented with raw port I/O, x64 only.*

### 2.18 CPU identification API missing
No `CPU.GetCPUBrandString` equivalent in the HAL. With §1.2 fixed, consumers
can use `X86Base.CpuId` (Aura does), but a HAL API with an ARM64 story would be
better.

---

## 3. Open — bugs / unfinished code

### 3.1 `Canvas.DrawImageAlpha` is a verbatim copy of `DrawImage`
Blending only happens because the per-pixel `DrawPoint(Color)` path blends —
non-virtual, bypasses the GopCanvas fast paths, ~orders slower than DrawImage;
and `SimdNative` has copy/fill but no bulk alpha-blend primitive.

### 3.2 `Bitmap.Save` produces corrupt BMPs
Two `Array.Copy(..., 0)` zero-count copies (vertical-res / color-count fields
never written) and the 24bpp path writes channel order G,R,A instead of B,G,R.

### 3.3 `ResourceManager` re-parses the resource index on every lookup
The cache field is commented out, each first enumeration serial-logs every
resource, `TryGetResouceBlob` [sic] throws `BadImageFormatException` on modules
without a resource blob, and names must be ASCII (ARM64 `Utf8.ToUtf16` codegen
bug workaround). Painful for asset-heavy kernels (Aura embeds 40+).

### 3.4 Kernel hot paths spam serial unconditionally — no log sink/verbosity
`NetworkStack.HandlePacket` (per packet), `KeyboardManager.ReadKey` (every 100
polls), scheduler/LAPIC tick logs, `PageAllocator` allocations, ResourceManager
lookups… all write COM1 directly with no verbosity switch and no way to
redirect kernel logs into an OS log window (gen2 had the pluggable
`Debugger.DoSend`). uart.log from a 40 s Aura boot is dominated by
`[SCHED]/[LAPIC]/[STRIDE]` lines.

### 3.5 `KernelConsole` has no public scroll / batched-cell API
External cell-grid consumers must move `Rows×Cols` cells one-by-one via
`GetCellAt`/`SetCellAt` (each `SetCellAt` redraws). An exposed `ScrollUp()` or
batched cell write would fix retargeted text UIs (Aura's CUI console).

### 3.6 `Console.ReadKey()` did not block during Aura's boot
Gen2 Aura paused at `Console.ReadKey()` before starting the GUI; on gen3 the
boot sailed straight through it. Worth checking `ConsolePlug`/`KeyboardManager
.ReadKey` blocking semantics before the input subsystem is fully initialized.

### 3.7 Build authoring
- `Cosmos.ArchitecturePicker.props` double-import → MSB4011 warning on every
  build (`Cosmos.Build.Common.props` imports it, then `Cosmos.Architecture.props`
  imports it again).
- **Restore ordering trap:** the arch-conditional `PackageReference`s in
  `Sdk.props` (`Cosmos.Kernel.Native.X64` etc., conditioned on `$(CosmosArch)`)
  are silently dropped from the restore graph when `CosmosArch` is only set by
  `ArchitecturePicker.props`, because package props don't exist at first
  restore evaluation. Result: clean checkout of an out-of-repo kernel builds
  fine through ILC then fails at link with dozens of undefined `_native_*`
  symbols. Aura works around it with a repo `Directory.Build.props` pinning
  `CosmosArch=x64`; consider setting it in `Cosmos.Sdk/Sdk.props` directly or
  making the reference unconditional per-RID.
- `NAOT0007` layer-violation warnings fire even for DevKernel itself (HAL/Core
  usage from a "User" layer project) — either the example violates the layering
  or the analyzer is too strict for legitimate kernel projects.
- `docs/articles/dev/plugs.md` documents `[Expose]` and `[FieldAccess]`, which
  do not exist in `Cosmos.Patcher` (only `Plug`/`PlugMember`/`PlatformSpecific`).
- cosmos-tools: `~/.cosmos/tools/bin/qemu-system-x86_64` wrapper execs
  `$DIR/qemu-system-x86_64.real`, but the `.real` binaries live in
  `~/.cosmos/tools/qemu/` — the `bin/` wrapper always fails.

### 3.8 API ergonomics (minor)
- `Cosmos.Kernel.Core.Memory.GarbageCollector.GarbageCollector` — namespace and
  class share a name; every consumer needs an alias to call `GetStats` etc.
- `Directory.GetFiles/GetDirectories` return **full paths** on gen3 (standard
  BCL) where gen2's plugs returned bare names — silent behavior change for
  ports; worth a line in a migration guide.
- `DateTime.Now` is UTC-only (no timezone) — document for gen2 migrants.

---

## 4. Aura-side notes (not gen3's fault, recorded for the port)

- gen2/IL2CPU identity-mapped low memory hid real null-dereference bugs
  (`RegionListBuilder` region `id`); expect more of these to surface at
  runtime on gen3 — each will now panic with a RIP to symbolicate against
  `Aura_OS.elf` (`nm`/`addr2line`).
- Embedded text resources keep their UTF-8 BOM — `Encoding.UTF8.GetString`
  does not strip it (gen2's `File.ReadAllText` did); use
  `Files.GetUiResourceText`.
- XSharp assembler plugs (SSE alpha blend) were rewritten as scalar C# in
  `DirectBitmap`; revisit with `Vector128`/SSE2 intrinsics if compositing is
  slow at 1080p, or upstream a `SimdNative` blend export.
- `os.clock` in UniLua now returns wall-clock-since-boot (TSC) rather than
  CPU time — closest equivalent on a kernel.
