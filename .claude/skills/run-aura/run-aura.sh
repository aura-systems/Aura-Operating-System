#!/usr/bin/env bash
# Boot Aura OS in a visible QEMU window through `cosmos run`, teeing the serial
# console to uart.log. Thin wrapper: `cosmos run` builds the QEMU command line
# (q35, KVM + -cpu host when available, CD bootindex=0, AHCI disk, NIC, HD Audio,
# port forwards, -no-reboot -no-shutdown) and finds a working QEMU (bundle or system).
#
# This script NEVER builds. The user runs the builds: when the ISO is missing
# or stale, ask him to rebuild.
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
PROJECT_DIR="$REPO_ROOT/SRC/Aura_OS"

ISO=""                          # default: cosmos run picks SRC/Aura_OS/output-x64/*.iso
DEFAULT_ISO="$PROJECT_DIR/output-x64/Aura_OS.iso"
DISK="$PROJECT_DIR/disk.img"    # same image as SRC/Aura_OS/.cosmos/config.json (VS Code extension)
UART="$REPO_ROOT/uart.log"
MEM=1024
NIC=e1000e
AUDIO=intel-hda                 # + hda-duplex codec, host backend picked by QEMU; 'none' for no sound card
TEMP_DISK=0
HEADLESS=0
GDB_PORT=""

usage() {
    cat <<EOF
Usage: run-aura.sh [options]

Options:
  --temp-disk     boot a throwaway copy of disk.img (no writes to the real one,
                  no write-lock conflict with another QEMU)
  --gdb PORT      also expose a gdb stub on the given port
  --headless      no window (serial only)
  --mem MB        guest memory in MB (default: $MEM)
  --nic MODEL     network card (default: $NIC; gen3 drives e1000e and virtio-net)
  --audio MODEL   HD Audio controller (default: $AUDIO; 'none' for no sound card)
  --iso PATH      boot ISO   (default: $DEFAULT_ISO)
  --disk PATH     disk image (default: $DISK; created blank, 512M, if missing)
  --uart PATH     serial log (default: $UART)
EOF
    exit 1
}

while [ $# -gt 0 ]; do
    case "$1" in
        --temp-disk) TEMP_DISK=1; shift ;;
        --gdb) GDB_PORT="$2"; shift 2 ;;
        --headless) HEADLESS=1; shift ;;
        --mem) MEM="$2"; shift 2 ;;
        --nic) NIC="$2"; shift 2 ;;
        --audio) AUDIO="$2"; shift 2 ;;
        --iso) ISO="$2"; shift 2 ;;
        --disk) DISK="$2"; shift 2 ;;
        --uart) UART="$2"; shift 2 ;;
        *) usage ;;
    esac
done

COSMOS="$(command -v cosmos || true)"
[ -z "$COSMOS" ] && [ -x "$HOME/.dotnet/tools/cosmos" ] && COSMOS="$HOME/.dotnet/tools/cosmos"
[ -n "$COSMOS" ] || {
    echo "ERROR: the 'cosmos' CLI is not installed. In the Cosmos checkout, 'make setup' installs it"
    echo "       (or: dotnet tool install -g Cosmos.Tools --add-source ../Cosmos/artifacts/package/release)."
    exit 1
}

ISO_ARGS=()
if [ -n "$ISO" ]; then
    [ -f "$ISO" ] || { echo "ERROR: ISO not found: $ISO"; exit 1; }
    ISO_ARGS=(--iso "$ISO")
elif [ ! -f "$DEFAULT_ISO" ]; then
    echo "ERROR: ISO not found: $DEFAULT_ISO"
    echo "       Ask the user to rebuild (cosmos build -p SRC/Aura_OS); this skill never builds."
    exit 1
fi

# Need a display for the window.
if [ "$HEADLESS" = 0 ] && [ -z "${DISPLAY:-}" ] && [ -z "${WAYLAND_DISPLAY:-}" ]; then
    echo "ERROR: no DISPLAY/WAYLAND_DISPLAY: cannot open a QEMU window."
    echo "Use --headless, or .claude/skills/crash-debug/crash-debug.sh for headless crash debugging."
    exit 1
fi

# `cosmos run --disk` requires an existing image; a blank one boots Aura in
# live mode until Setup or `vol` formats it.
[ -f "$DISK" ] || truncate -s 512M "$DISK"
if [ "$TEMP_DISK" = 1 ]; then
    TMP="$(mktemp "${TMPDIR:-/tmp}/aura-disk.XXXXXX.img")"
    cp "$DISK" "$TMP"
    DISK="$TMP"
    trap 'rm -f "$TMP"' EXIT
    echo "Using throwaway disk copy: $DISK"
fi

# FTP control port + the passive range (same forwards as .cosmos/config.json).
FWD_ARGS=(--hostfwd "tcp::2121-:21")
for p in $(seq 50000 50009); do
    FWD_ARGS+=(--hostfwd "tcp::$p-:$p")
done

EXTRA_ARGS=()
[ "$HEADLESS" = 1 ] && EXTRA_ARGS+=(--headless)
if [ -n "$GDB_PORT" ]; then
    EXTRA_ARGS+=(-- -gdb "tcp::$GDB_PORT")
    echo "gdb stub on :$GDB_PORT"
fi

echo "Booting Aura OS via cosmos run (serial -> $UART). Close the window or Ctrl+C to stop."
# Serial goes to stdio with `cosmos run`; tee it to uart.log (fresh file each boot).
"$COSMOS" run -p "$PROJECT_DIR" "${ISO_ARGS[@]}" -m "$MEM" \
    --disk "$DISK" --nic "$NIC" --audio "$AUDIO" "${FWD_ARGS[@]}" "${EXTRA_ARGS[@]}" 2>&1 \
    | tee "$UART" | {
        # Surface the classic failure mode with a helpful message.
        while IFS= read -r line; do
            echo "$line"
            case "$line" in
                *'Failed to get "write" lock'*)
                    echo "HINT: $DISK is in use by another QEMU instance (run-aura, crash-debug --keep"
                    echo "      or the VS Code extension). Close it, or rerun with --temp-disk." ;;
            esac
        done
    }
