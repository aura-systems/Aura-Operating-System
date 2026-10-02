---
name: run-aura
description: Boot an already-built Aura OS ISO in a visible QEMU window through `cosmos run` (1 GB, AHCI disk.img, e1000e, FTP port forwards, serial tee'd to uart.log). Never builds. Use when the user asks to run/start/launch Aura OS, boot the ISO, or open the QEMU window. For headless crash symbolication use crash-debug instead.
---

# Run Aura OS (QEMU window)

Boot the existing ISO with a visible QEMU display:

```bash
.claude/skills/run-aura/run-aura.sh
```

It is a thin wrapper over:

```bash
cosmos run -p SRC/Aura_OS -m 1024 --disk SRC/Aura_OS/disk.img --nic e1000e \
    --hostfwd tcp::2121-:21 --hostfwd tcp::50000-:50000 ... --hostfwd tcp::50009-:50009 \
    | tee uart.log
```

QEMU stays in the foreground until the window is closed. Launch it with
`run_in_background: true` so the session isn't blocked, then watch boot
progress in `uart.log` at the repo root. It is a fresh file each boot, and its
first lines are `cosmos run`'s own banner.

**No build step.** The user runs the builds (`cosmos build -p SRC/Aura_OS`).
If `SRC/Aura_OS/output-x64/Aura_OS.iso` is missing or older than the change
you want to see, stop and ask the user to rebuild.

Flags:

- `--temp-disk`: boot a throwaway copy of `disk.img`. This keeps the real one
  untouched and avoids the write-lock conflict when another QEMU is running.
  The copy is deleted on exit.
- `--gdb PORT`: also expose a gdb stub (passed as `cosmos run -- -gdb
  tcp::PORT`). Combine it with the crash-debug skill's `--symbolicate` mode for
  manual digging.
- `--headless`: no window, serial only.
- `--mem MB`: guest memory, 1024 by default.
- `--nic MODEL`: network card, `e1000e` by default. gen3 drives only e1000e and
  virtio-net.
- `--iso/--disk/--uart PATH`: path overrides.

Notes:

- `cosmos run` picks QEMU itself: the Cosmos bundle, or the system QEMU if its
  version passes. It also adds q35, KVM with `-cpu host` when `/dev/kvm` is
  usable, `-vga std`, the CD at `bootindex=0`, and `-no-reboot -no-shutdown`.
- The disk is `SRC/Aura_OS/disk.img` on AHCI, the same image the VS Code
  extension uses (`SRC/Aura_OS/.cosmos/config.json`).
  - A missing image is created blank at 512M. Aura then boots in live mode
    until Setup or `vol` formats it.
  - gen3 has no IDE driver, so disks must be on AHCI or NVMe.
- FTP from the host: `ftp localhost 2121`. Passive data ports 50000–50009 are
  forwarded.
- If the boot panics (`CPU EXCEPTION` ... `System halted.`, `KERNEL PANIC`, or
  `[FAILFAST]` in `uart.log`), symbolicate that log:
  `.claude/skills/crash-debug/crash-debug.sh --uart uart.log`.
- To stop it from the CLI: `pkill -f '[q]emu-system-x86_64.*Aura_OS.iso'`.
  The `[q]` keeps the pattern from matching your own shell. Stopping the
  `cosmos run` process also takes QEMU down.
