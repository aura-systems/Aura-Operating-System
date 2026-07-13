---
name: crash-debug
description: Symbolicate an Aura OS (Cosmos gen3) CPU-exception panic. Boots the ISO headless in QEMU with a gdb stub, catches the panic on serial, dumps the crashed stack via gdb, walks the RBP chain, and resolves every return address against the kernel ELF with nm/addr2line. Use when Aura OS page-faults or panics in QEMU and you need to know which managed method crashed, or when the user pastes a panic register dump that needs symbolication.
---

# Aura OS crash debug

Reproduce a boot-time kernel panic and get a fully symbolicated backtrace in
one command:

```bash
.claude/skills/crash-debug/crash-debug.sh
```

The script boots `output-x64/Aura_OS.iso` headless (serial → temp uart.log,
gdb stub on the first free port from 12377), waits for `System halted`,
attaches gdb, dumps 8KB of stack at the crashed RSP, and prints:

- the faulting RIP symbol,
- `[RSP]` — the direct caller when the fault hit a leaf runtime helper,
- the RBP-chain backtrace (NativeAOT emits frame pointers, so this is reliable),
- a conservative scan of all code addresses near RSP (covers broken chains).

Useful flags: `--timeout N` (default 180s; the crash must occur during boot or
be reachable without input), `--keep` (leave QEMU + gdb stub alive for
interactive digging), `--iso/--elf/--disk PATH` overrides.

To just resolve addresses from a panic the user pasted (no QEMU run):

```bash
.claude/skills/crash-debug/crash-debug.sh --symbolicate 0xFFFFFFFF8009E2A0 0xFFFFFFFF80088486
```

## Environment gotchas (already handled by the script)

- `~/.cosmos/tools/bin/qemu-system-x86_64` is a broken wrapper; the working
  binary is `~/.cosmos/tools/qemu/qemu-system-x86_64`. The script probes.
- The repo `disk.img` may be write-locked by the user's own QEMU session; the
  script always runs against a temp copy.
- Rebuild first with `make build` if sources changed — the backtrace is only
  valid against the ELF that produced the panic
  (`SRC/Aura_OS/bin/Debug/net10.0/linux-x64/Aura_OS.elf`).

## Interpreting the backtrace

- Symbols are ILC-mangled: `Assembly_Namespace_Class__Method`. Map them back
  to the C# source and read the actual method.
- **Page fault with CR2 < 64K = null dereference.** CR2 is the field/element
  offset into the null object. gen2/IL2CPU identity-mapped low memory, so
  ported gen2 code is full of latent null-derefs that only now fault
  (see GEN3-GAPS.md §2.6).
- If RIP is in a leaf runtime helper, the real bug is in frame `#1`:
  - `RhpAssignRef` / `RhpCheckedAssignRef` (GC write barrier): the caller did
    `nullObject.refField = value`; CR2 = the field's offset.
  - `memset/memcpy/RhpCopy*`: caller passed a null/bad buffer.
- Frames like `Kernel__BeforeRun`, `__managed__Main`, `kmain` are the normal
  boot spine — the interesting frames sit above them.

## After symbolication

Read the C# source of frames #1–#3, find which object with a ref field at the
CR2 offset can be null on that path, and fix at the source. If field offsets
are ambiguous, count object-reference fields in class layout order (base class
first, 8 bytes each after the 8-byte MethodTable pointer) or add a targeted
serial log and rerun.
