#!/usr/bin/env python3
"""Symbolicate an Aura OS (Cosmos gen3) CPU-exception panic.

Inputs: the kernel ELF (for nm/addr2line), the QEMU serial log containing the
panic register dump, and optionally a gdb stack dump ("x/Ngx <rsp>" output).
Produces a symbolized backtrace: faulting RIP, top-of-stack return address,
RBP-chain walk, plus a conservative scan of code addresses on the stack.
"""

import argparse
import bisect
import re
import subprocess
import sys

KERNEL_TEXT_BASE = 0xFFFFFFFF80000000


def load_symbols(elf):
    out = subprocess.run(["nm", elf], capture_output=True, text=True, check=True).stdout
    by_addr = {}
    for line in out.splitlines():
        parts = line.split()
        if len(parts) != 3 or parts[1] not in "tTwW":
            continue
        addr = int(parts[0], 16)
        name = parts[2]
        # duplicate names at one address (mangled + exported alias): keep shortest
        if addr not in by_addr or len(name) < len(by_addr[addr]):
            by_addr[addr] = name
    addrs = sorted(by_addr)
    return addrs, by_addr


def symbolize(addr, addrs, by_addr):
    if addr < KERNEL_TEXT_BASE or not addrs or addr < addrs[0] or addr > addrs[-1] + 0x10000:
        return None
    i = bisect.bisect_right(addrs, addr) - 1
    base = addrs[i]
    return f"{by_addr[base]}+0x{addr - base:x}"


def addr2line(elf, addr):
    try:
        out = subprocess.run(
            ["addr2line", "-e", elf, "-f", "-C", hex(addr)],
            capture_output=True, text=True, timeout=10,
        ).stdout.splitlines()
        if len(out) >= 2:
            src = out[1].strip()
            if not src.startswith("??") and not src.endswith("?") and not src.endswith(":0"):
                return src
    except Exception:
        pass
    return None


def parse_panic(uart_text):
    """Parse the LAST register dump block in the serial log."""
    blocks = uart_text.split("CPU EXCEPTION:")
    if len(blocks) < 2:
        return None
    block = blocks[-1]
    regs = {}
    for m in re.finditer(r"\b(RIP|RSP|RBP|CR2|RAX|RDI|RSI)\s*:\s*(0x[0-9A-Fa-f]+)", block):
        regs[m.group(1)] = int(m.group(2), 16)
    regs["exception"] = block.splitlines()[0].strip()
    return regs


def parse_stack_dump(text):
    """Parse gdb 'x/Ngx' output into {address: qword}."""
    mem = {}
    for m in re.finditer(
        r"^(0x[0-9a-fA-F]+):\s+(0x[0-9a-fA-F]+)(?:\s+(0x[0-9a-fA-F]+))?",
        text, re.MULTILINE,
    ):
        addr = int(m.group(1), 16)
        mem[addr] = int(m.group(2), 16)
        if m.group(3):
            mem[addr + 8] = int(m.group(3), 16)
    return mem


def describe(addr, elf, addrs, by_addr, want_src=True):
    sym = symbolize(addr, addrs, by_addr)
    if not sym:
        return None
    src = addr2line(elf, addr) if want_src else None
    return f"0x{addr:016x}  {sym}" + (f"   [{src}]" if src else "")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--elf", required=True)
    ap.add_argument("--uart", help="serial log containing the panic dump")
    ap.add_argument("--stack", help="gdb stack dump (x/Ngx output)")
    ap.add_argument("--addrs", nargs="*", help="just symbolicate these addresses")
    ap.add_argument("--rip", help="live-sampled RIP (hang mode, no panic block)")
    ap.add_argument("--rsp", help="live-sampled RSP (hang mode)")
    ap.add_argument("--rbp", help="live-sampled RBP (hang mode)")
    ap.add_argument("--rip2", help="second RIP sample to prove a stuck loop")
    args = ap.parse_args()

    addrs, by_addr = load_symbols(args.elf)

    if args.addrs:
        for a in args.addrs:
            v = int(a, 16)
            print(describe(v, args.elf, addrs, by_addr) or f"0x{v:016x}  <not in kernel text>")
        return 0

    if args.rip:
        regs = {"exception": "LIVE SAMPLE (hang mode)", "RIP": int(args.rip, 16)}
        if args.rsp:
            regs["RSP"] = int(args.rsp, 16)
        if args.rbp:
            regs["RBP"] = int(args.rbp, 16)
        if args.rip2:
            rip2 = int(args.rip2, 16)
            d = describe(rip2, args.elf, addrs, by_addr)
            print(f"Second RIP sample: {d or hex(rip2)}")
    else:
        uart = open(args.uart).read()
        regs = parse_panic(uart)
        if not regs:
            print("No 'CPU EXCEPTION' panic block found in serial log.", file=sys.stderr)
            return 1

    print(f"Exception: {regs.get('exception', '?')}")
    for r in ("RIP", "RSP", "RBP", "CR2"):
        if r in regs:
            print(f"  {r}: 0x{regs[r]:x}")

    cr2 = regs.get("CR2")
    if cr2 is not None and "Page Fault" in regs.get("exception", "") and cr2 < 0x10000:
        print(f"\nHINT: CR2=0x{cr2:x} (< 64K) => null dereference; "
              f"0x{cr2:x} is the field/element offset into a null object.")

    print("\n=== Backtrace ===")
    rip = regs.get("RIP")
    n = 0
    if rip is not None:
        print(f"#{n}  {describe(rip, args.elf, addrs, by_addr) or hex(rip)}   <- faulting RIP")
        n += 1

    if not args.stack:
        print("(no stack dump provided — RIP only)")
        return 0

    mem = parse_stack_dump(open(args.stack).read())
    rsp, rbp = regs.get("RSP"), regs.get("RBP")

    # Top of stack: the return address if the fault hit a leaf helper
    # (e.g. RhpAssignRef) before it set up a frame.
    if rsp in mem:
        d = describe(mem[rsp], args.elf, addrs, by_addr)
        if d:
            print(f"#{n}  {d}   <- [RSP] (caller, if fault was in a leaf helper)")
            n += 1

    # RBP chain walk.
    frame, seen = rbp, set()
    while frame and frame in mem and frame not in seen and n < 64:
        seen.add(frame)
        ret = mem.get(frame + 8)
        if ret is None:
            break
        d = describe(ret, args.elf, addrs, by_addr)
        print(f"#{n}  {d or hex(ret)}")
        n += 1
        frame = mem[frame]
    if frame and frame not in mem and frame not in seen:
        print(f"(frame 0x{frame:x} outside dumped stack range — increase dump size)")

    # Conservative scan: any code-looking qwords near RSP the chain missed.
    print("\n=== Other code addresses on stack (conservative scan) ===")
    for a in sorted(k for k in mem if rsp <= k < rsp + 64 * 8):
        d = describe(mem[a], args.elf, addrs, by_addr, want_src=False)
        if d:
            print(f"  [rsp+0x{a - rsp:03x}] {d}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
