#!/usr/bin/env python3
"""Symbolicate an Aura OS (Cosmos gen3) kernel crash.

Inputs: the kernel ELF (for nm/addr2line), the QEMU serial log, and optionally
a gdb stack dump ("x/Ngx <rsp>" output).

Cosmos HEAD writes three kinds of fatal report to serial:

- CPU EXCEPTION (Cosmos.Kernel/Panic.cs). Registers, then the kernel's own
  frame-pointer walk:
      Stack trace (raw return addresses, symbolicate with nm):
        ip:  0x...
        sp0: 0x... (return address if ip is a frameless leaf)
        [0]  0x...
  then "System halted.". The walk is skipped when CR2 is near RSP (likely
  stack overflow).
- KERNEL PANIC (Cosmos.Kernel.Core/Panic.cs). A message plus the method,
  file and line, then "System halted.".
- [FAILFAST] (ExceptionHelper.FailFast). An unhandled or recursive managed
  exception: the message, the exception message and the managed stack trace.
  The CPU then spins, so no "System halted." follows.

The LAST report in the log is symbolicated. For a CPU exception, the kernel's
raw trace is the primary backtrace. The gdb dump, when given, adds the RBP
chain (used only when the raw trace has no frames) and a conservative scan of
code addresses near RSP.
"""

import argparse
import bisect
import re
import shutil
import subprocess
import sys

# Link base of the x64 kernel (Cosmos.Build.Templates/Linker/linker.x64.ld).
KERNEL_TEXT_BASE = 0xFFFFFFFF80000000

HEX = r"0x[0-9A-Fa-f]+"


def find_tool(*names):
    for n in names:
        path = shutil.which(n)
        if path:
            return path
    return None


NM = find_tool("nm", "llvm-nm")
ADDR2LINE = find_tool("addr2line", "llvm-addr2line")


def load_symbols(elf):
    if not NM:
        print("ERROR: neither nm nor llvm-nm is installed.", file=sys.stderr)
        sys.exit(1)
    out = subprocess.run([NM, elf], capture_output=True, text=True, check=True).stdout
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
    if not ADDR2LINE:
        return None
    try:
        out = subprocess.run(
            [ADDR2LINE, "-e", elf, "-f", "-C", hex(addr)],
            capture_output=True, text=True, timeout=10,
        ).stdout.splitlines()
        if len(out) >= 2:
            src = out[1].strip()
            if not src.startswith("??") and not src.endswith("?") and not src.endswith(":0"):
                return src
    except Exception:
        pass
    return None


def describe(addr, elf, addrs, by_addr, want_src=True):
    sym = symbolize(addr, addrs, by_addr)
    if not sym:
        return None
    src = addr2line(elf, addr) if want_src else None
    return f"0x{addr:016x}  {sym}" + (f"   [{src}]" if src else "")


def last_report(uart_text):
    """Return (kind, block) for the LAST fatal report in the serial log."""
    text = uart_text.replace("\r", "")
    markers = {
        "cpu": "CPU EXCEPTION:",
        "panic": "KERNEL PANIC",
        "failfast": "[FAILFAST]",
    }
    best = None
    for kind, marker in markers.items():
        i = text.rfind(marker)
        if i >= 0 and (best is None or i > best[1]):
            best = (kind, i)
    if best is None:
        return None, None
    kind, i = best
    block = text[i:]
    end = block.find("System halted.")
    if end >= 0:
        block = block[:end + len("System halted.")]
    return kind, block


def parse_cpu_exception(block):
    regs = {}
    for m in re.finditer(r"\b(RIP|RSP|RBP|CR2|RAX|RDI|RSI)\s*:\s*(" + HEX + ")", block):
        regs[m.group(1)] = int(m.group(2), 16)
    regs["exception"] = block.splitlines()[0].replace("CPU EXCEPTION:", "").strip()
    m = re.search(r"Interrupt Vector:\s*(\d+)", block)
    if m:
        regs["vector"] = int(m.group(1))

    trace = {"frames": [], "no_walk": "not walking" in block}
    i = block.find("Stack trace (raw return addresses")
    if i >= 0:
        for line in block[i:].splitlines()[1:]:
            line = line.strip()
            m = re.match(r"(ip|sp0|lr):\s*(" + HEX + ")", line)
            if m:
                trace[m.group(1)] = int(m.group(2), 16)
                continue
            m = re.match(r"\[(\d+)\]\s+(" + HEX + ")", line)
            if m:
                trace["frames"].append(int(m.group(2), 16))
    return regs, trace


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


def print_text_report(kind, block, elf, addrs, by_addr):
    """KERNEL PANIC / [FAILFAST]: echo the report, resolve any kernel addresses in it."""
    lines = [l for l in block.splitlines() if l.strip() and not set(l.strip()) <= {"="}]
    title = "KERNEL PANIC" if kind == "panic" else "FAILFAST (unhandled or recursive managed exception)"
    print(f"Report: {title}")
    for line in lines[:80]:
        print(f"  {line.rstrip()}")
    if len(lines) > 80:
        print(f"  ... ({len(lines) - 80} more lines in the serial log)")

    seen = []
    for m in re.finditer(HEX, block):
        v = int(m.group(0), 16)
        if v >= KERNEL_TEXT_BASE and v not in seen:
            seen.append(v)
    resolved = [d for d in (describe(v, elf, addrs, by_addr) for v in seen) if d]
    if resolved:
        print("\n=== Kernel addresses in the report ===")
        for d in resolved:
            print(f"  {d}")
    if kind == "failfast":
        print("\nHINT: no CPU fault happened; a managed exception escaped (gen3 does not run "
              "AppDomain.UnhandledException, GEN3-GAP(unhandled)) or was thrown while another "
              "was being dispatched (GEN3-GAP(eh-global)). Find the throw site from the "
              "exception message and the stack trace above.")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--elf", required=True)
    ap.add_argument("--uart", help="serial log containing the crash report")
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

    trace = {"frames": [], "no_walk": False}
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
        if not args.uart:
            print("ERROR: give --uart, --rip or --addrs.", file=sys.stderr)
            return 1
        with open(args.uart, errors="replace") as f:
            uart_text = f.read()
        kind, block = last_report(uart_text)
        if kind is None:
            print("No 'CPU EXCEPTION', 'KERNEL PANIC' or '[FAILFAST]' report found in the serial log.",
                  file=sys.stderr)
            # Aura's own crash screen (Crash.StopKernel) for a caught managed exception.
            crash = [l.strip() for l in uart_text.replace("\r", "").splitlines() if "[CRASH] " in l]
            if crash:
                print("Aura crash screen (a managed exception Aura caught, no CPU fault):", file=sys.stderr)
                for l in crash[-5:]:
                    print(f"  {l}", file=sys.stderr)
            return 1
        if kind != "cpu":
            print_text_report(kind, block, args.elf, addrs, by_addr)
            return 0
        regs, trace = parse_cpu_exception(block)

    print(f"Exception: {regs.get('exception', '?')}")
    if "vector" in regs:
        print(f"  vector: {regs['vector']}")
    for r in ("RIP", "RSP", "RBP", "CR2"):
        if r in regs:
            print(f"  {r}: 0x{regs[r]:x}")

    cr2 = regs.get("CR2")
    if cr2 is not None and "Page Fault" in regs.get("exception", "") and cr2 < 0x10000:
        print(f"\nHINT: CR2=0x{cr2:x} (< 64K) => null dereference; "
              f"0x{cr2:x} is the field/element offset into a null object.")
    if trace["no_walk"]:
        print("\nHINT: the kernel did not walk the stack because CR2 is next to RSP: "
              "most likely a stack overflow (deep or infinite recursion).")

    print("\n=== Backtrace ===")
    rip = regs.get("RIP", trace.get("ip"))
    n = 0
    if rip is not None:
        print(f"#{n}  {describe(rip, args.elf, addrs, by_addr) or hex(rip)}   <- faulting RIP")
        n += 1

    mem = parse_stack_dump(open(args.stack).read()) if args.stack else {}
    rsp, rbp = regs.get("RSP"), regs.get("RBP")

    # Top of stack: the return address if the fault hit a frameless leaf
    # helper (e.g. RhpAssignRef, memcpy) before it set up a frame.
    top = trace.get("sp0")
    if top is None and rsp in mem:
        top = mem[rsp]
    if top is not None:
        d = describe(top, args.elf, addrs, by_addr)
        if d:
            print(f"#{n}  {d}   <- [RSP] (caller, if RIP is in a frameless leaf helper)")
            n += 1

    if trace["frames"]:
        # The kernel's own frame-pointer walk (NativeAOT keeps frame pointers).
        for ret in trace["frames"]:
            d = describe(ret, args.elf, addrs, by_addr)
            print(f"#{n}  {d or hex(ret)}")
            n += 1
    elif mem:
        # No raw trace on serial: walk the RBP chain in the gdb dump.
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
    else:
        print("(no raw stack trace on serial and no gdb stack dump — RIP only)")

    # Conservative scan: any code-looking qwords near RSP the chain missed.
    if mem and rsp is not None:
        print("\n=== Other code addresses on stack (conservative scan) ===")
        for a in sorted(k for k in mem if rsp <= k < rsp + 64 * 8):
            d = describe(mem[a], args.elf, addrs, by_addr, want_src=False)
            if d:
                print(f"  [rsp+0x{a - rsp:03x}] {d}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
