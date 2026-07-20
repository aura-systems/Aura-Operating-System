#!/usr/bin/env bash
# Build (optional) and boot Aura OS in a visible QEMU window.
# Mirrors the Makefile `run` target, but probes for a working QEMU binary
# (the ~/.cosmos/tools/bin wrapper is broken) and fails clearly if disk.img
# is locked by another QEMU instance.
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"

ISO="$REPO_ROOT/output-x64/Aura_OS.iso"
DISK="$REPO_ROOT/disk.img"
UART="$REPO_ROOT/uart.log"
BUILD=1
TEMP_DISK=0
GDB_PORT=""

usage() {
    cat <<EOF
Usage: run-aura.sh [options]

Options:
  --no-build      skip 'make build' (boot the existing ISO)
  --temp-disk     boot a throwaway copy of disk.img (no writes to the real one)
  --gdb PORT      also expose a gdb stub on the given port
  --iso PATH      boot ISO   (default: $ISO)
  --disk PATH     disk image (default: $DISK)
  --uart PATH     serial log (default: $UART)
EOF
    exit 1
}

while [ $# -gt 0 ]; do
    case "$1" in
        --no-build) BUILD=0; shift ;;
        --temp-disk) TEMP_DISK=1; shift ;;
        --gdb) GDB_PORT="$2"; shift 2 ;;
        --iso) ISO="$2"; shift 2 ;;
        --disk) DISK="$2"; shift 2 ;;
        --uart) UART="$2"; shift 2 ;;
        *) usage ;;
    esac
done

if [ "$BUILD" = 1 ]; then
    make -C "$REPO_ROOT" build || exit 1
fi
[ -f "$ISO" ] || { echo "ERROR: ISO not found: $ISO (run 'make build')"; exit 1; }

# Need a display for the window.
if [ -z "${DISPLAY:-}" ] && [ -z "${WAYLAND_DISPLAY:-}" ]; then
    echo "ERROR: no DISPLAY/WAYLAND_DISPLAY — cannot open a QEMU window."
    echo "For headless crash debugging use .claude/skills/crash-debug/crash-debug.sh instead."
    exit 1
fi

# The ~/.cosmos/tools/bin qemu wrapper is broken (execs a nonexistent .real);
# the real binaries live in ~/.cosmos/tools/qemu/. Probe candidates.
QEMU=""
for q in "$HOME/.cosmos/tools/qemu/qemu-system-x86_64" /usr/bin/qemu-system-x86_64 qemu-system-x86_64; do
    if "$q" --version >/dev/null 2>&1; then QEMU="$q"; break; fi
done
[ -n "$QEMU" ] || { echo "ERROR: no working qemu-system-x86_64 found"; exit 1; }

# KVM when the host exposes it (TCG is ~10x slower and tanks the GUI FPS);
# -cpu host needs KVM, so the TCG fallback keeps -cpu max.
KVM_FLAGS=(-cpu max)
[ -w /dev/kvm ] && KVM_FLAGS=(-enable-kvm -cpu host)

[ -f "$DISK" ] || truncate -s 256M "$DISK"
if [ "$TEMP_DISK" = 1 ]; then
    TMP="$(mktemp "${TMPDIR:-/tmp}/aura-disk.XXXXXX.img")"
    cp "$DISK" "$TMP"
    DISK="$TMP"
    echo "Using throwaway disk copy: $DISK"
fi

GDB_FLAGS=()
[ -n "$GDB_PORT" ] && GDB_FLAGS=(-gdb "tcp::$GDB_PORT") && echo "gdb stub on :$GDB_PORT"

echo "Booting $ISO (serial -> $UART). Close the window or Ctrl+C to stop."
"$QEMU" -M q35 "${KVM_FLAGS[@]}" -m 1G \
    -drive file="$ISO",if=none,id=cosmoscd,format=raw,readonly=on \
    -device ide-cd,drive=cosmoscd,bootindex=0 \
    -vga std \
    -serial file:"$UART" \
    -drive file="$DISK",if=none,id=ahcidisk,format=raw \
    -device ich9-ahci,id=ahci0 -device ide-hd,drive=ahcidisk,bus=ahci0.0 \
    -netdev user,id=net0 -device e1000e,netdev=net0 \
    -no-reboot -no-shutdown "${GDB_FLAGS[@]}" 2>&1 | {
        # Surface the classic failure mode with a helpful message.
        while IFS= read -r line; do
            echo "$line"
            case "$line" in
                *'Failed to get "write" lock'*)
                    echo "HINT: disk.img is in use by another QEMU instance."
                    echo "      Close it, or rerun with --temp-disk." ;;
            esac
        done
    }
