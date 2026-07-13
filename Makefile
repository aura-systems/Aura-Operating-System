# AuraOS on Cosmos gen3 (nativeaot-patcher) — dev workflow.
# Prereqs: .NET SDK 10.0.2xx, cosmos.patcher/cosmos global tools, clang, ld.lld, xorriso
# (see nativeaot-patcher `make setup`), and the local package feed referenced by NuGet.Config.

ARCH        ?= x64
RID         := linux-x64
DEFINE      := ARCH_X64
OUTPUT      := ./output-$(ARCH)
KERNEL_PROJ := ./SRC/Aura_OS/Aura_OS.csproj

QEMU := qemu-system-x86_64 -M q35 -cpu max -m 1G

ISO_FLAGS  := -drive file=$(OUTPUT)/Aura_OS.iso,if=none,id=cosmoscd,format=raw,readonly=on \
              -device ide-cd,drive=cosmoscd,bootindex=0 \
              -vga std -device i8042 -serial file:uart.log

DISK_IMG   := disk.img
DISK_FLAGS := -drive file=$(DISK_IMG),if=none,id=ahcidisk,format=raw \
              -device ich9-ahci,id=ahci0 \
              -device ide-hd,drive=ahcidisk,bus=ahci0.0

NET_FLAGS  := -netdev user,id=net0 -device e1000e,netdev=net0

.PHONY: build run run-headless disks clean

# ARCH_X64 define and CosmosArch come from the Cosmos architecture picker (RID-derived);
# passing DefineConstants globally would clobber the csproj's TRACE;NOASYNC.
build:
	dotnet publish -c Debug -r $(RID) -p:CosmosArch=$(ARCH) \
		$(KERNEL_PROJ) -o $(OUTPUT)

$(DISK_IMG):
	truncate -s 256M $@

disks: $(DISK_IMG)

run: build disks
	$(QEMU) $(ISO_FLAGS) $(DISK_FLAGS) $(NET_FLAGS) -no-reboot -no-shutdown

run-headless: build disks
	$(QEMU) $(ISO_FLAGS) $(DISK_FLAGS) $(NET_FLAGS) -display none -no-reboot -no-shutdown

clean:
	rm -rf $(OUTPUT) uart.log
