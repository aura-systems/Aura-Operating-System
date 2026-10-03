-- New, Delete, Format and Label: each adds an operation, once the panel asked what it needs; or tells why
-- it cannot.

local ui = require "ui"
local state = require "state"
local format = require "format"
local filesystems = require "model.filesystems"
local segments = require "model.segments"
local operations = require "model.operations"
local list = require "view.list"
local status = require "view.status"
local panel = require "view.panel"
local window = require "view.window"

-- The free space a new partition can take: from its first MB boundary. A logical one goes after the
-- last one, its EBR the sector before it.
local function newRoom(segment)
  local alignment = segments.perMiB(state.disk)
  local finish = segment.start + segment.sectors

  if segment.logical then
    return segment.start + 1, math.max(0, (finish - segment.start - 1) // alignment)
  end

  local first = segments.alignUp(segment.start, alignment)
  return first, first < finish and (finish - first) // alignment or 0
end

-- The panel's text for the filesystem chosen: what each takes, and its label.
local function filesystemText(room)
  return (room and "Up to " .. room .. " MB. " or "") .. filesystems.SIZES .. " " .. filesystems.labelRules(panel.filesystem())
end

local function formatText(partition)
  return "Everything on " .. partition.name .. " will be lost. " .. filesystemText(nil)
end

local function newPartition()
  local disk = state.disk
  local segment = list.selected()

  if not disk then
    status.set("There is no disk.", "red")
    return
  elseif disk.table == "none" then
    panel.showError("New partition", disk.name .. " has no partition table: create one first, with Partition table.")
    return
  elseif disk.table == "whole" then
    panel.showError("New partition", disk.name .. " has a filesystem on the whole disk, and no partition table. Create one first, with Partition table: its files will be lost.")
    return
  elseif not segment or segment.kind ~= "free" then
    status.set("Select unallocated space first.", "red")
    return
  elseif segment.logical and not segment.tail then
    panel.showError("New partition", "Aura adds a logical partition after the last one only: select the free space at the end of the extended partition.")
    return
  elseif not segment.logical and disk.table == "MBR" and disk.primaries >= 4 then
    panel.showError("New partition", "An MBR disk holds 4 primary partitions at most: delete one first, or use a GPT.")
    return
  end

  local _, room = newRoom(segment)

  if room < 1 then
    panel.showError("New partition", "There is less than 1 MB of free space here.")
    return
  end

  ui.beforeBox.text = "0"
  ui.sizeBox.text = tostring(room)
  ui.labelBox.text = ""
  ui.filesystemBox.selectedIndex = filesystems.defaultFilesystem(room)

  panel.open("new", segment, segment.logical and "New logical partition" or "New partition",
    { before = not segment.logical, size = true, filesystem = true, label = true }, "Add", filesystemText(room))
  segment.room = room
end

local function createOk()
  local disk = state.disk
  local segment = state.panelSegment
  local first, room = newRoom(segment)
  local before = segment.logical and 0 or panel.readMiB(ui.beforeBox)
  local size = panel.readMiB(ui.sizeBox)
  local filesystem = panel.filesystem()
  local label = ui.labelBox.text

  if not before or not size or size < 1 then
    panel.problem("Type the sizes in whole MB, the partition's 1 or more.")
    return
  elseif before + size > room then
    panel.problem("That is " .. (before + size) .. " MB: there are " .. room .. " MB here.")
    return
  end

  local problem = filesystems.sizeProblem(filesystem, size) or filesystems.labelProblem(filesystem, label)

  if problem then
    panel.problem(problem)
    return
  end

  local alignment = segments.perMiB(disk)
  local start = first + before * alignment
  local op = {
    kind = "create", disk = disk.name, id = "n" .. state.nextNew, name = "new #" .. state.nextNew,
    start = start, sectors = size * alignment, filesystem = filesystem, label = label,
    logical = segment.logical or false, sectorSize = disk.sectorSize,
  }
  op.text = operations.createText(op)

  problem = operations.queue(op)

  if problem then
    panel.problem(problem)
    return
  end

  state.nextNew = state.nextNew + 1
  panel.close()
  window.queued(op, start)
end

local function deletePartition()
  local segment = window.selectedPartition("delete")

  if not segment then
    return
  end

  local disk = state.disk
  local partition = segment.partition
  local problem = operations.systemProblem(partition)

  if problem then
    panel.showError("Delete " .. partition.name, problem)
    return
  end

  local op = { kind = "delete", disk = disk.name, target = partition.id,
    text = "Delete " .. partition.name .. " (" .. format.size(segments.bytesOf(disk, segment)) .. " " .. partition.filesystem .. ")" }

  problem = operations.queue(op)

  if problem then
    panel.showError("Delete " .. partition.name, problem)
    return
  end

  if state.panelAction then
    panel.close()
  end

  window.queued(op, segment.start, "free")
end

local function formatPartition()
  local segment = window.selectedPartition("format")

  if not segment then
    return
  end

  local partition = segment.partition
  local problem = operations.systemProblem(partition)

  if problem then
    panel.showError("Format " .. partition.name, problem)
    return
  end

  ui.filesystemBox.selectedIndex = filesystems.defaultFilesystem(segments.bytesOf(state.disk, segment) // segments.MIB)

  for i, name in ipairs(filesystems.NAMES) do
    if name == partition.filesystem then
      ui.filesystemBox.selectedIndex = i - 1
    end
  end

  ui.labelBox.text = partition.label

  panel.open("format", segment, "Format " .. partition.name, { filesystem = true, label = true }, "Format",
    formatText(partition))
end

local function formatOk()
  local disk = state.disk
  local segment = state.panelSegment
  local partition = segment.partition
  local filesystem = panel.filesystem()
  local label = ui.labelBox.text
  local problem = filesystems.sizeProblem(filesystem, segments.bytesOf(disk, segment) // segments.MIB)
    or filesystems.labelProblem(filesystem, label)

  if problem then
    panel.problem(problem)
    return
  end

  local written = filesystems.labelAs(filesystem, label)
  local op = { kind = "format", disk = disk.name, target = partition.id, filesystem = filesystem, label = label,
    text = "Format " .. partition.name .. " as " .. filesystem .. (written ~= "" and " '" .. written .. "'" or "") }

  problem = operations.queue(op)

  if problem then
    panel.problem(problem)
    return
  end

  panel.close()
  window.queued(op, segment.start)
end

local function labelPartition()
  local segment = window.selectedPartition("label")

  if not segment then
    return
  end

  local partition = segment.partition

  if not filesystems.canLabel(partition.filesystem) then
    panel.showError("Label " .. partition.name, "Aura labels FAT and ext2 volumes only: this one is " .. partition.filesystem .. ".")
    return
  end

  local problem = operations.systemProblem(partition)

  if problem then
    panel.showError("Label " .. partition.name, problem)
    return
  end

  ui.labelBox.text = partition.label
  panel.open("label", segment, "Label " .. partition.name, { label = true }, "Label",
    filesystems.labelRules(partition.filesystem))
end

local function labelOk()
  local segment = state.panelSegment
  local partition = segment.partition
  local label = ui.labelBox.text:match("^%s*(.-)%s*$")
  local problem = filesystems.labelProblem(partition.filesystem, label)

  if problem then
    panel.problem(problem)
    return
  end

  local written = filesystems.labelAs(partition.filesystem, label)
  local op = { kind = "label", disk = state.disk.name, target = partition.id, label = label,
    text = written == "" and "Remove the label of " .. partition.name or "Label " .. partition.name .. " '" .. written .. "'" }

  problem = operations.queue(op)

  if problem then
    panel.problem(problem)
    return
  end

  panel.close()
  window.queued(op, segment.start)
end

-- Another filesystem in the form: what it takes, and its label.
local function filesystemChanged()
  if state.panelAction == "new" then
    panel.tell(filesystemText(state.panelSegment.room))
  elseif state.panelAction == "format" then
    panel.tell(formatText(state.panelSegment.partition))
  end
end

return {
  new = newPartition,
  createOk = createOk,
  delete = deletePartition,
  format = formatPartition,
  formatOk = formatOk,
  label = labelPartition,
  labelOk = labelOk,
  filesystemChanged = filesystemChanged,
}
