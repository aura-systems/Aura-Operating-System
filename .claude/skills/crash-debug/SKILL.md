---
name: crash-debug
description: Symbolicate an Aura OS (Cosmos gen3) kernel crash. Boots the ISO headless in QEMU with a gdb stub, catches the CPU-exception panic / KERNEL PANIC / [FAILFAST] report on serial, dumps the crashed stack via gdb, and resolves the kernel's raw stack trace and every return address against the kernel ELF with nm/addr2line. Also symbolicates an existing serial log (--uart) or pasted addresses (--symbolicate). Use when Aura OS page-faults, panics or fail-fasts in QEMU and you need to know which managed method crashed, or when the user pastes a panic dump that needs symbolication. Never builds.
---

# Aura OS crash debug

Reproduce a boot-time kernel crash and get a fully symbolicated backtrace in
one command:

```bash
.claude/skills/crash-debug/crash-debug.sh
```

The script boots `SRC/Aura_OS/output-x64/Aura_OS.iso` headless (serial to a
temp uart.log, gdb stub on the first free port from 12377, `-m 1024`, a temp
copy of `SRC/Aura_OS/disk.img` on AHCI, e1000e NIC), waits for a fatal report,
and prints:

- the faulting RIP symbol,
- `[RSP]` (the kernel's `sp0:` line): the direct caller when the fault hit a
  frameless leaf runtime helper,
- the kernel's own frame-pointer walk from the panic's
  `Stack trace (raw return addresses, symbolicate with nm):` block
  (NativeAOT keeps frame pointers, so this is reliable),
- with gdb: a conservative scan of all code addresses near RSP (covers broken
  chains), and the RBP chain from the dump when the serial trace has no frames.

Useful flags: `--timeout N` (default 180s; the crash must occur during boot or
be reachable without input), `--keep` (leave QEMU + gdb stub alive for
interactive digging), `--mem SIZE` (default 1024 MB), `--iso/--elf/--disk PATH`
overrides, `--hang-after PATTERN` (no crash, but stuck: samples RIP twice via
gdb once PATTERN shows up on serial).

Symbolicate a crash that already happened, no QEMU run. For example, use the
`uart.log` that run-aura tees at the repo root, or a log the user pasted into a
file:

```bash
.claude/skills/crash-debug/crash-debug.sh --uart uart.log
```

To resolve addresses the user pasted:

```bash
.claude/skills/crash-debug/crash-debug.sh --symbolicate 0xFFFFFFFF8009E2A0 0xFFFFFFFF80088486
```

## Never build

This skill and its scripts never build. The user runs the builds: `cosmos build
-p SRC/Aura_OS`, or `dotnet publish SRC/Aura_OS/Aura_OS.csproj -c Debug -r
linux-x64 -o SRC/Aura_OS/output-x64`.

- The backtrace is only valid against the ELF that produced the crash.
- The script warns when a `.cs`/`.csproj` under `SRC/Aura_OS` is newer than the
  ELF.
- If the ISO or ELF is missing or stale, stop and ask the user to rebuild.

## What the serial reports mean

- **`CPU EXCEPTION: <name>`, then `System halted.`** A hardware fault: #PF,
  #GP, #UD, #DE and the like. gen3 never turns these into managed exceptions:
  a null dereference or an integer `/0` halts the kernel (GEN3-GAP(null-deref)
  in `GEN3-GAPS.md`).
- **`KERNEL PANIC`, then `System halted.`** A kernel-internal `Panic.Halt`,
  printed with the method, file and line.
- **`[FAILFAST] <message>`, then the CPU spins.** A managed exception nobody
  caught ("Unhandled exception"), or one thrown while another was being
  dispatched ("Recursive exception"). The report carries the exception message
  and its managed stack trace.
- **`[CRASH] <exception>: <description>`.** This is Aura's own crash screen
  (`Crash.StopKernel`) for a managed exception it caught. The kernel keeps
  running and waits for a key. The script stops on it, prints the `[CRASH]`
  line and the last serial output, and exits with code 4: there is no CPU
  fault to symbolicate. Read the message and fix the throw site.

## Environment gotchas (already handled by the script)

- QEMU is probed in this order:
  1. `~/.cosmos/tools/qemu/bin/qemu-system-x86_64` (newer bundles)
  2. `~/.cosmos/tools/qemu/qemu-system-x86_64`
  3. the system qemu
- `~/.cosmos/tools/bin/qemu-system-x86_64` is a stale symlink to the bundle
  wrapper. It execs a nonexistent `bin/*.real` and fails `--version`, so it is
  skipped. `cosmos run` is not affected by it.
- `SRC/Aura_OS/disk.img` may be write-locked by the user's own QEMU session
  (run-aura or the VS Code extension). The script always runs against a temp
  copy. A missing image becomes a blank 512 MB disk, and Aura then boots in
  live mode.
- gdb comes from `PATH`, then `~/.cosmos/tools/gdb/bin/gdb`. Without gdb, a CPU
  exception still symbolicates from the kernel's serial stack trace.

## Interpreting the backtrace

- Symbols are ILC-mangled: `Assembly_Namespace_Class__Method`. Map them back
  to the C# source and read the actual method.
- **Page fault with CR2 < 64K = null dereference.** CR2 is the field or element
  offset into the null object. gen2/IL2CPU identity-mapped low memory, so
  ported gen2 code is full of latent null dereferences that only now fault
  (GEN3-GAP(null-deref)).
- **CR2 next to RSP**: the kernel prints `not walking` instead of frames. This
  is most likely a stack overflow (deep or infinite recursion).
- If RIP is in a leaf runtime helper, the real bug is in the `[RSP]` frame:
  - `RhpAssignRef` / `RhpCheckedAssignRef` (GC write barrier): the caller did
    `nullObject.refField = value`; CR2 = the field's offset.
  - `memset/memcpy/RhpCopy*`: caller passed a null/bad buffer.
- Frames like `Aura_OS_Aura_Boot_Kernel__BeforeRun`/`__Run`,
  `Aura_OS_Aura_OS_Kernel__BeforeRun`/`__Run`, `__managed__Main` and `kmain`
  are the normal boot spine. The interesting frames sit above them.

## After symbolication

Read the C# source of frames #1–#3. Find which object can be null on that path
with a ref field at the CR2 offset, and fix it at the source (rule C6: explicit
null and divisor checks).

If the field offsets are ambiguous, count the object-reference fields in class
layout order: base class first, 8 bytes each, after the 8-byte MethodTable
pointer. Or add a targeted `Log.WriteString` and ask the user to rebuild and
rerun.
