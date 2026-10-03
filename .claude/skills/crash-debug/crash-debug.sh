#!/usr/bin/env bash
# Boot the Aura OS ISO headless under QEMU with a gdb stub, wait for a fatal
# report on serial (CPU EXCEPTION / KERNEL PANIC / [FAILFAST]), dump the
# crashed stack via gdb, and print a symbolicated backtrace against the kernel
# ELF. Can also symbolicate an existing serial log (--uart) or raw addresses
# (--symbolicate) without booting anything.
#
# This script NEVER builds. The user runs the builds: when the ISO/ELF is
# missing or older than the sources, ask him to rebuild.
#
# The disk image is COPIED to a temp workdir, so this never fights an
# already-running QEMU for the write lock on disk.img.
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
PROJECT_DIR="$REPO_ROOT/SRC/Aura_OS"

# `cosmos build -p SRC/Aura_OS` (and `dotnet publish ... -o SRC/Aura_OS/output-x64`)
# put the ISO there; PublishISO copies the ELF next to it.
ISO="$PROJECT_DIR/output-x64/Aura_OS.iso"
ELF="$PROJECT_DIR/output-x64/Aura_OS.elf"
# Same image as SRC/Aura_OS/.cosmos/config.json (VS Code extension) and run-aura.
DISK="$PROJECT_DIR/disk.img"
TIMEOUT=180
PORT_BASE=12377
KEEP=0
MEM=1024
HANG_PATTERN=""
HANG_GRACE=8
UART_IN=""
SYM_ONLY=()

REBUILD_HINT="ask the user to rebuild (cosmos build -p SRC/Aura_OS); this skill never builds"

usage() {
    cat <<EOF
Usage: crash-debug.sh [options]
       crash-debug.sh --uart PATH
       crash-debug.sh --symbolicate 0xADDR [0xADDR...]

Options:
  --iso PATH      boot ISO            (default: $ISO)
  --elf PATH      kernel ELF for nm   (default: $ELF)
  --disk PATH     disk image to copy  (default: $DISK)
  --timeout N     seconds to wait for a crash (default: $TIMEOUT)
  --mem SIZE      QEMU memory, MB or with a suffix (default: $MEM; small sizes force early GC)
  --hang-after P  hang mode: P is a grep pattern; once it appears on serial,
                  wait --hang-grace seconds, then halt via gdb and sample live
                  registers twice (proves a stuck loop even while timer-tick
                  serial spam keeps the log growing)
  --hang-grace N  seconds to wait after the pattern before sampling (default: $HANG_GRACE)
  --keep          leave QEMU + gdb stub running after the report
  --uart PATH     skip QEMU; symbolicate the last crash report in an existing
                  serial log (e.g. the uart.log written by run-aura)
  --symbolicate   skip QEMU entirely; just resolve addresses via nm/addr2line
EOF
    exit 1
}

while [ $# -gt 0 ]; do
    case "$1" in
        --iso) ISO="$2"; shift 2 ;;
        --elf) ELF="$2"; shift 2 ;;
        --disk) DISK="$2"; shift 2 ;;
        --timeout) TIMEOUT="$2"; shift 2 ;;
        --mem) MEM="$2"; shift 2 ;;
        --hang-after) HANG_PATTERN="$2"; shift 2 ;;
        --hang-grace) HANG_GRACE="$2"; shift 2 ;;
        --keep) KEEP=1; shift ;;
        --uart) UART_IN="$2"; shift 2 ;;
        --symbolicate) shift; SYM_ONLY=("$@"); [ ${#SYM_ONLY[@]} -gt 0 ] || usage; break ;;
        *) usage ;;
    esac
done

[ -f "$ELF" ] || { echo "ERROR: kernel ELF not found: $ELF ($REBUILD_HINT)"; exit 1; }

# The backtrace is only valid against the ELF that produced the crash.
NEWER_SRC="$(find "$PROJECT_DIR" \( -path "$PROJECT_DIR/obj" -o -path "$PROJECT_DIR/bin" -o -path "$PROJECT_DIR/output-x64" \) -prune \
    -o \( -name '*.cs' -o -name '*.csproj' \) -newer "$ELF" -print 2>/dev/null | head -n 1)"
if [ -n "$NEWER_SRC" ]; then
    echo "WARNING: sources changed after the ELF was built (e.g. ${NEWER_SRC#"$REPO_ROOT"/})."
    echo "         Addresses from a NEW crash need a matching ELF: $REBUILD_HINT."
fi

if [ ${#SYM_ONLY[@]} -gt 0 ]; then
    exec python3 "$SCRIPT_DIR/symbolicate.py" --elf "$ELF" --addrs "${SYM_ONLY[@]}"
fi

if [ -n "$UART_IN" ]; then
    [ -f "$UART_IN" ] || { echo "ERROR: serial log not found: $UART_IN"; exit 1; }
    exec python3 "$SCRIPT_DIR/symbolicate.py" --elf "$ELF" --uart "$UART_IN"
fi

[ -f "$ISO" ] || { echo "ERROR: ISO not found: $ISO ($REBUILD_HINT)"; exit 1; }

# gdb is needed for the stack dump and for hang mode. HEAD's panic prints its
# own frame-pointer walk, so a CPU-exception crash still symbolicates without it.
GDB="$(command -v gdb || true)"
[ -z "$GDB" ] && [ -x "$HOME/.cosmos/tools/gdb/bin/gdb" ] && GDB="$HOME/.cosmos/tools/gdb/bin/gdb"
if [ -z "$GDB" ]; then
    [ -n "$HANG_PATTERN" ] && { echo "ERROR: gdb not installed (needed for --hang-after)"; exit 1; }
    echo "WARNING: gdb not installed; using the kernel's serial stack trace only."
fi

# QEMU: the Cosmos bundle (qemu/bin/ in newer bundles, qemu/ in older ones),
# then the system one. ~/.cosmos/tools/bin/qemu-system-x86_64 (a stale symlink
# to the qemu/ wrapper, first on PATH on the user's machine) fails --version
# and is skipped.
QEMU=""
for q in "$HOME/.cosmos/tools/qemu/bin/qemu-system-x86_64" \
         "$HOME/.cosmos/tools/qemu/qemu-system-x86_64" \
         /usr/bin/qemu-system-x86_64 \
         "$(command -v qemu-system-x86_64 || true)"; do
    [ -n "$q" ] || continue
    if "$q" --version >/dev/null 2>&1; then QEMU="$q"; break; fi
done
[ -n "$QEMU" ] || { echo "ERROR: no working qemu-system-x86_64 found (try: cosmos install)"; exit 1; }

PORT=""
for p in $(seq "$PORT_BASE" $((PORT_BASE + 9))); do
    if ! (echo >"/dev/tcp/127.0.0.1/$p") 2>/dev/null; then PORT=$p; break; fi
done
[ -n "$PORT" ] || { echo "ERROR: no free gdb port in $PORT_BASE-$((PORT_BASE + 9))"; exit 1; }

# KVM when the host exposes it: boots and runs the kernel ~10x faster, and
# the gdb usage here (halt, register reads, memory dumps) works fine under it.
KVM_FLAGS=(-cpu max)
[ -w /dev/kvm ] && KVM_FLAGS=(-enable-kvm -cpu host)

WORK="$(mktemp -d "${TMPDIR:-/tmp}/aura-crash-debug.XXXXXX")"
echo "Workdir: $WORK (uart.log, stack.txt, disk copy)"

if [ -f "$DISK" ]; then
    cp "$DISK" "$WORK/disk.img"
else
    # Blank disk: Aura boots in live mode (no FAT volume).
    truncate -s 512M "$WORK/disk.img"
fi

# Same machine as `cosmos run` (q35, CD bootindex=0, AHCI disk, e1000e NIC, intel-hda;
# the audio backend is silent, but the driver and the boot sound play as usual).
"$QEMU" -M q35 "${KVM_FLAGS[@]}" -m "$MEM" \
    -drive file="$ISO",if=none,id=cosmoscd,format=raw,readonly=on \
    -device ide-cd,drive=cosmoscd,bootindex=0 \
    -boot d -vga std \
    -serial file:"$WORK/uart.log" \
    -device ich9-ahci,id=ahci0 \
    -drive file="$WORK/disk.img",if=none,id=ahcidisk0,format=raw \
    -device ide-hd,drive=ahcidisk0,bus=ahci0.0 \
    -netdev user,id=net0 -device e1000e,netdev=net0 \
    -audiodev none,id=snd0 -device intel-hda -device hda-duplex,audiodev=snd0 \
    -display none -no-reboot -no-shutdown -gdb "tcp::$PORT" \
    >"$WORK/qemu.err" 2>&1 &
QPID=$!

cleanup() {
    if [ "$KEEP" = 1 ] && kill -0 "$QPID" 2>/dev/null; then
        echo ""
        echo "QEMU left running (pid $QPID, gdb stub on :$PORT)."
        echo "  attach: gdb -ex 'target remote :$PORT'"
        echo "  kill:   kill $QPID"
    else
        kill "$QPID" 2>/dev/null
    fi
}
trap cleanup EXIT

echo "Booting with $QEMU (gdb stub on :$PORT), waiting up to ${TIMEOUT}s for a crash..."
CRASHED=0
FAILFAST=0
HUNG=0
AURA_CRASH=0
START=$(date +%s)
while :; do
    grep -q "System halted" "$WORK/uart.log" 2>/dev/null && { CRASHED=1; break; }
    # FailFast spins without "System halted."; give it a moment to finish
    # writing the exception message and stack trace.
    if grep -qF "[FAILFAST]" "$WORK/uart.log" 2>/dev/null; then
        sleep 2; CRASHED=1; FAILFAST=1; break
    fi
    # Aura's own crash screen (Crash.StopKernel): a managed exception Aura
    # caught. The kernel then waits for a key, so stop here instead of timing
    # out as "running fine". Drawing the screen can still fault: re-check.
    if grep -qF "[CRASH] " "$WORK/uart.log" 2>/dev/null; then
        sleep 2
        grep -q "System halted" "$WORK/uart.log" 2>/dev/null && { CRASHED=1; break; }
        grep -qF "[FAILFAST]" "$WORK/uart.log" 2>/dev/null && { CRASHED=1; FAILFAST=1; break; }
        AURA_CRASH=1
        break
    fi
    if [ -n "$HANG_PATTERN" ] && grep -qE "$HANG_PATTERN" "$WORK/uart.log" 2>/dev/null; then
        echo "Hang pattern '$HANG_PATTERN' seen; waiting ${HANG_GRACE}s grace, then sampling..."
        sleep "$HANG_GRACE"
        grep -q "System halted" "$WORK/uart.log" 2>/dev/null && { CRASHED=1; break; }
        HUNG=1
        break
    fi
    kill -0 "$QPID" 2>/dev/null || { echo "ERROR: QEMU exited:"; cat "$WORK/qemu.err"; exit 3; }
    [ $(( $(date +%s) - START )) -ge "$TIMEOUT" ] && break
    sleep 1
done

if [ "$HUNG" = 1 ]; then
    # Live-sample the (halted-by-gdb) CPU twice: two RIPs inside the same
    # function a second apart = stuck loop, not normal progress.
    echo "Sampling live registers via gdb (twice, 1s apart)..."
    "$GDB" -batch -ex "set pagination off" -ex "target remote :$PORT" \
        -ex "info registers rip rsp rbp" >"$WORK/regs1.txt" 2>"$WORK/gdb.err"
    sleep 1
    "$GDB" -batch -ex "set pagination off" -ex "target remote :$PORT" \
        -ex "info registers rip rsp rbp" >"$WORK/regs2.txt" 2>>"$WORK/gdb.err"

    parse_reg() { grep -oE "^$2\s+0x[0-9a-fA-F]+" "$1" | awk '{print $2}'; }
    RIP1=$(parse_reg "$WORK/regs1.txt" rip); RIP2=$(parse_reg "$WORK/regs2.txt" rip)
    RSP1=$(parse_reg "$WORK/regs1.txt" rsp); RBP1=$(parse_reg "$WORK/regs1.txt" rbp)
    [ -n "$RIP1" ] || { echo "ERROR: could not sample registers:"; cat "$WORK/gdb.err"; exit 1; }

    "$GDB" -batch -ex "set pagination off" -ex "target remote :$PORT" \
        -ex "x/512gx $RSP1" \
        -ex "x/512gx $RSP1+4096" \
        >"$WORK/stack.txt" 2>>"$WORK/gdb.err"

    echo ""
    echo "Sample 1: RIP=$RIP1  RSP=$RSP1  RBP=$RBP1"
    echo "Sample 2: RIP=$RIP2"
    python3 "$SCRIPT_DIR/symbolicate.py" --elf "$ELF" --stack "$WORK/stack.txt" \
        --rip "$RIP1" --rsp "$RSP1" --rbp "$RBP1" --rip2 "$RIP2"
    exit 0
fi

if [ "$AURA_CRASH" = 1 ]; then
    echo "Aura crash screen after $(( $(date +%s) - START ))s (Crash.StopKernel: a managed exception Aura caught, no CPU fault):"
    grep -F "[CRASH] " "$WORK/uart.log" | tail -n 5
    echo "Nothing to symbolicate: find the throw site from the message. Last serial output:"
    tail -15 "$WORK/uart.log"
    exit 4
fi

if [ "$CRASHED" = 0 ]; then
    echo "No crash within ${TIMEOUT}s: the kernel appears to be running fine."
    echo "Last serial output:"
    tail -15 "$WORK/uart.log"
    exit 2
fi

echo "Crash caught after $(( $(date +%s) - START ))s."

# Only a CPU exception has a meaningful RSP to dump (KERNEL PANIC and
# FAILFAST print their own report and halt/spin in the reporting code).
RSP=""
if [ "$FAILFAST" = 0 ] && grep -q "CPU EXCEPTION:" "$WORK/uart.log"; then
    RSP=$(grep -oE 'RSP: 0x[0-9A-Fa-f]+' "$WORK/uart.log" | tail -1 | cut -d' ' -f2)
fi

STACK_ARGS=()
if [ -n "$RSP" ] && [ -n "$GDB" ]; then
    echo "Dumping the crashed stack via gdb..."
    # Two chunked dumps so a partially unmapped range doesn't lose everything.
    "$GDB" -batch -ex "set pagination off" -ex "target remote :$PORT" \
        -ex "x/512gx $RSP" \
        -ex "x/512gx $RSP+4096" \
        >"$WORK/stack.txt" 2>"$WORK/gdb.err"
    if grep -q "^0x" "$WORK/stack.txt"; then
        STACK_ARGS=(--stack "$WORK/stack.txt")
    else
        echo "WARNING: gdb stack dump failed; using the serial stack trace only:"
        cat "$WORK/gdb.err"
    fi
fi

echo ""
python3 "$SCRIPT_DIR/symbolicate.py" --elf "$ELF" --uart "$WORK/uart.log" "${STACK_ARGS[@]}"
