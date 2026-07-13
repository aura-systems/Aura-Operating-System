---
name: run-aura
description: Build and boot Aura OS in a visible QEMU window (interactive GUI session, serial log to uart.log). Use when the user asks to run/start/launch Aura OS, boot the ISO, or open the QEMU window. For headless crash symbolication use crash-debug instead.
---

# Run Aura OS (QEMU window)

Build the kernel and boot it with a visible QEMU display:

```bash
.claude/skills/run-aura/run-aura.sh
```

QEMU stays in the foreground until the window is closed — launch it with
`run_in_background: true` so the session isn't blocked, then watch boot
progress in `uart.log` at the repo root (fresh file each boot).

Flags:

- `--no-build` — boot the existing `output-x64/Aura_OS.iso` without rebuilding
- `--temp-disk` — boot a throwaway copy of `disk.img` (keeps the real one
  untouched and avoids the write-lock conflict when another QEMU is running)
- `--gdb PORT` — also expose a gdb stub (combine with the crash-debug skill's
  `--symbolicate` mode for manual digging)
- `--iso/--disk/--uart PATH` — overrides

Notes:

- The script mirrors the Makefile `run` target (q35, 1G, AHCI disk, e1000e
  NIC, serial → `uart.log`) but probes for a working QEMU binary — the
  `~/.cosmos/tools/bin` wrapper is broken; the real one is in
  `~/.cosmos/tools/qemu/`.
- If the boot panics (`CPU EXCEPTION` ... `System halted.` in `uart.log`),
  switch to the crash-debug skill for a symbolicated backtrace.
- To stop it from the CLI: `pkill -f 'qemu-system-x86_64.real'` (killing the
  plain name also matches your own shell wrapper — use the `.real` pattern).
