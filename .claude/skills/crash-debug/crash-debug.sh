#!/usr/bin/env bash
# Boot the Aura OS ISO headless under QEMU with a gdb stub, wait for a
# CPU-exception panic on serial, dump the crashed stack via gdb, and print a
# symbolicated backtrace against the kernel ELF.
#
# The disk image is COPIED to a temp workdir, so this never fights an
# already-running QEMU for the write lock on disk.img.
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"

ISO="$REPO_ROOT/output-x64/Aura_OS.iso"
ELF="$REPO_ROOT/SRC/Aura_OS/bin/Debug/net10.0/linux-x64/Aura_OS.elf"
DISK="$REPO_ROOT/disk.img"
TIMEOUT=180
PORT_BASE=12377
KEEP=0
MEM=1G
HANG_PATTERN=""
HANG_GRACE=8
SYM_ONLY=()

usage() {
    cat <<EOF
Usage: crash-debug.sh [options]
       crash-debug.sh --symbolicate 0xADDR [0xADDR...]

Options:
  --iso PATH      boot ISO            (default: $ISO)
  --elf PATH      kernel ELF for nm   (default: $ELF)
  --disk PATH     disk image to copy  (default: $DISK)
  --timeout N     seconds to wait for a panic (default: $TIMEOUT)
  --mem SIZE      QEMU memory size (default: $MEM; small sizes force early GC)
  --hang-after P  hang mode: P is a grep pattern; once it appears on serial,
                  wait --hang-grace seconds, then halt via gdb and sample live
                  registers twice (proves a stuck loop even while timer-tick
                  serial spam keeps the log growing)
  --hang-grace N  seconds to wait after the pattern before sampling (default: $HANG_GRACE)
  --keep          leave QEMU + gdb stub running after the report
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
        --symbolicate) shift; SYM_ONLY=("$@"); break ;;
        *) usage ;;
    esac
done

[ -f "$ELF" ] || { echo "ERROR: kernel ELF not found: $ELF (build first: make build)"; exit 1; }

if [ ${#SYM_ONLY[@]} -gt 0 ]; then
    exec python3 "$SCRIPT_DIR/symbolicate.py" --elf "$ELF" --addrs "${SYM_ONLY[@]}"
fi

[ -f "$ISO" ] || { echo "ERROR: ISO not found: $ISO (build first: make build)"; exit 1; }
command -v gdb >/dev/null || { echo "ERROR: gdb not installed"; exit 1; }

# The ~/.cosmos/tools/bin qemu wrapper is broken (execs a nonexistent .real);
# the real binaries live in ~/.cosmos/tools/qemu/. Probe candidates.
QEMU=""
for q in "$HOME/.cosmos/tools/qemu/qemu-system-x86_64" /usr/bin/qemu-system-x86_64 qemu-system-x86_64; do
    if "$q" --version >/dev/null 2>&1; then QEMU="$q"; break; fi
done
[ -n "$QEMU" ] || { echo "ERROR: no working qemu-system-x86_64 found"; exit 1; }

PORT=""
for p in $(seq "$PORT_BASE" $((PORT_BASE + 9))); do
    if ! (echo >"/dev/tcp/127.0.0.1/$p") 2>/dev/null; then PORT=$p; break; fi
done
[ -n "$PORT" ] || { echo "ERROR: no free gdb port in $PORT_BASE-$((PORT_BASE + 9))"; exit 1; }

WORK="$(mktemp -d "${TMPDIR:-/tmp}/aura-crash-debug.XXXXXX")"
echo "Workdir: $WORK (uart.log, stack.txt, disk copy)"

if [ -f "$DISK" ]; then
    cp "$DISK" "$WORK/disk.img"
else
    truncate -s 256M "$WORK/disk.img"
fi

"$QEMU" -M q35 -cpu max -m "$MEM" \
    -drive file="$ISO",if=none,id=cosmoscd,format=raw,readonly=on \
    -device ide-cd,drive=cosmoscd,bootindex=0 \
    -vga std \
    -serial file:"$WORK/uart.log" \
    -drive file="$WORK/disk.img",if=none,id=ahcidisk,format=raw \
    -device ich9-ahci,id=ahci0 -device ide-hd,drive=ahcidisk,bus=ahci0.0 \
    -netdev user,id=net0 -device e1000e,netdev=net0 \
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

echo "Booting (gdb stub on :$PORT), waiting up to ${TIMEOUT}s for a panic..."
CRASHED=0
HUNG=0
START=$(date +%s)
while :; do
    grep -q "System halted" "$WORK/uart.log" 2>/dev/null && { CRASHED=1; break; }
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
    gdb -batch -ex "set pagination off" -ex "target remote :$PORT" \
        -ex "info registers rip rsp rbp" >"$WORK/regs1.txt" 2>"$WORK/gdb.err"
    sleep 1
    gdb -batch -ex "set pagination off" -ex "target remote :$PORT" \
        -ex "info registers rip rsp rbp" >"$WORK/regs2.txt" 2>>"$WORK/gdb.err"

    parse_reg() { grep -oE "^$2\s+0x[0-9a-fA-F]+" "$1" | awk '{print $2}'; }
    RIP1=$(parse_reg "$WORK/regs1.txt" rip); RIP2=$(parse_reg "$WORK/regs2.txt" rip)
    RSP1=$(parse_reg "$WORK/regs1.txt" rsp); RBP1=$(parse_reg "$WORK/regs1.txt" rbp)
    [ -n "$RIP1" ] || { echo "ERROR: could not sample registers:"; cat "$WORK/gdb.err"; exit 1; }

    gdb -batch -ex "set pagination off" -ex "target remote :$PORT" \
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

if [ "$CRASHED" = 0 ]; then
    echo "No panic within ${TIMEOUT}s — the kernel appears to be running fine."
    echo "Last serial output:"
    tail -15 "$WORK/uart.log"
    exit 2
fi

echo "Panic caught after $(( $(date +%s) - START ))s. Dumping stack via gdb..."
RSP=$(grep -oE 'RSP: 0x[0-9A-Fa-f]+' "$WORK/uart.log" | tail -1 | cut -d' ' -f2)
[ -n "$RSP" ] || { echo "ERROR: could not parse RSP from panic dump"; tail -40 "$WORK/uart.log"; exit 1; }

# Two chunked dumps so a partially unmapped range doesn't lose everything.
gdb -batch -ex "set pagination off" -ex "target remote :$PORT" \
    -ex "x/512gx $RSP" \
    -ex "x/512gx $RSP+4096" \
    >"$WORK/stack.txt" 2>"$WORK/gdb.err"

if ! grep -q "^0x" "$WORK/stack.txt"; then
    echo "ERROR: gdb stack dump failed:"; cat "$WORK/gdb.err"; exit 1
fi

echo ""
python3 "$SCRIPT_DIR/symbolicate.py" --elf "$ELF" --uart "$WORK/uart.log" --stack "$WORK/stack.txt"
