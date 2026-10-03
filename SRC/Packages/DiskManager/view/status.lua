-- The status line, about the selected segment or the disk, else what an action did; and the Mount or
-- Unmount button, as the selected partition is.

local ui = require "ui"
local state = require "state"
local format = require "format"
local segments = require "model.segments"
local operations = require "model.operations"
local list = require "view.list"

local function set(text, color)
  ui.status.text = text
  ui.status.color = color or "black"
end

local function tableName(d)
  if d.table == "MBR" or d.table == "GPT" then
    return d.table
  elseif d.table == "whole" then
    return "no partition table, a filesystem on the whole disk"
  end

  return "no partition table"
end

local function diskSummary(d)
  if d.error then
    return d.name .. " cannot be read: " .. d.error
  end

  local count = #d.partitions
  local waiting = operations.pendingOn(d.name)

  return d.name .. ": " .. format.size(d.size) .. ", " .. tableName(d) .. ", " .. format.plural(count, "partition")
    .. (waiting == 1 and ", once the pending operation is applied" or "")
    .. (waiting > 1 and ", once the " .. waiting .. " pending operations are applied" or "")
end

-- The status line: the selected segment, or the disk.
local function show()
  local disk = state.disk

  if not disk then
    set("No disk: Aura sees SATA (AHCI), NVMe and USB disks.")
    return
  end

  local segment = list.selected()

  if not segment then
    set(diskSummary(disk), disk.error and "red" or "black")
    return
  end

  local size = format.size(segments.bytesOf(disk, segment))
  local text = segments.nameOf(segment) .. ": "

  if segment.kind == "partition" then
    local partition = segment.partition
    text = text .. partition.filesystem .. (partition.label ~= "" and " '" .. partition.label .. "'" or "") .. ", " .. size

    if partition.new then
      text = text .. ", created when the operations are applied"
    elseif partition.system then
      text = text .. ", Aura runs from it (" .. partition.mountPoint .. ")"
    elseif partition.mountPoint then
      text = text .. ", mounted at " .. partition.mountPoint
    end

    if partition.changed then
      text = text .. ", changes pending"
    end

    local space = state.spaceOf(segment)

    if space then
      text = text .. ", " .. format.size(space.free) .. " free"
    end
  elseif segment.kind == "free" then
    text = text .. size .. " of free space" .. (segment.logical and " in the extended partition" or "")
  else
    text = text .. size .. ", holds the logical partitions"
  end

  set(text)
end

-- Mount or Unmount, as the selected partition is.
local function showMountButton()
  local segment = list.selected()
  local mounted = segment and segment.partition and segment.partition.mountPoint ~= nil
  ui.mountButton.visible = not mounted
  ui.unmountButton.visible = mounted or false
end

return {
  set = set,
  show = show,
  showMountButton = showMountButton,
  tableName = tableName,
}
