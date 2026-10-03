-- Information: the panel tells about the selected segment, or the disk.

local state = require "state"
local format = require "format"
local filesystems = require "model.filesystems"
local segments = require "model.segments"
local operations = require "model.operations"
local list = require "view.list"
local status = require "view.status"
local panel = require "view.panel"

local function show()
  local disk = state.disk

  if not disk then
    status.set("There is no disk.", "red")
    return
  end

  local segment = list.selected()
  local lines = {}

  local function add(name, value)
    lines[#lines + 1] = name .. ": " .. tostring(value)
  end

  if not segment then
    add("Size", format.size(disk.size))
    add("Partition table", status.tableName(disk))
    add("Partitions", #disk.partitions)
    add("Sectors", disk.sectors)
    add("Sector size", disk.sectorSize .. " bytes")

    if operations.pendingOn(disk.name) > 0 then
      add("Pending", format.plural(operations.pendingOn(disk.name), "operation"))
    end

    if disk.error then
      add("Error", disk.error)
    end

    panel.open("information", nil, disk.name, {}, nil, table.concat(lines, "\n"))
    return
  end

  if segment.kind == "partition" then
    local partition = segment.partition
    local space = state.spaceOf(segment)

    add("File system", partition.filesystem)
    add("Label", partition.label ~= "" and partition.label or "none")
    add("Mount point", partition.mountPoint and partition.mountPoint .. (partition.system and " (Aura runs from it)" or "") or "not mounted")
    add("Size", format.size(segments.bytesOf(disk, segment)))

    if space then
      add("Used", format.size(space.total - space.free) .. ", unused: " .. format.size(space.free))
    end

    if (partition.volumeSectors or 0) > 0 and partition.volumeSectors < partition.sectors then
      add("Volume", format.size(partition.volumeSectors * disk.sectorSize) .. ": a format uses the whole partition")
    end

    local partitionType = filesystems.typeOf(partition)

    if partitionType then
      add("Type", partitionType)
    end

    local flags = filesystems.flagsOf(partition)
    add("Flags", flags ~= "" and flags or "none")

    if partition.logical then
      lines[#lines + 1] = "A logical partition, in the extended one."
    end

    if partition.new then
      lines[#lines + 1] = "Created when the operations are applied."
    elseif partition.changed then
      lines[#lines + 1] = "Changed when the operations are applied."
    end
  else
    add("Size", format.size(segments.bytesOf(disk, segment)))
  end

  add("Sectors", segment.start .. " to " .. (segment.start + segment.sectors - 1))
  add("Sector count", segment.sectors)

  if segment.kind == "free" and segment.sectors < segments.perMiB(disk) then
    lines[#lines + 1] = "Less than 1 MB: no partition fits."
  end

  panel.open("information", segment, segments.nameOf(segment), {}, nil, table.concat(lines, "\n"))
end

return {
  show = show,
}
