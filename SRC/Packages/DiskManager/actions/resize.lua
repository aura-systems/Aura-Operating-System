-- Resize/Move: the panel's form; and the limits dragging on the bar keeps to too.

local ui = require "ui"
local state = require "state"
local format = require "format"
local filesystems = require "model.filesystems"
local segments = require "model.segments"
local operations = require "model.operations"
local status = require "view.status"
local panel = require "view.panel"
local window = require "view.window"

-- The smallest and largest sizes a partition can take, in sectors: a partition to be created is made at
-- its new size, between what its filesystem takes; another one's volume keeps its size.
local function sizeLimits(partition)
  local disk = state.disk
  local alignment = segments.perMiB(disk)
  local minimum, maximum

  if partition.new and operations.foldable(disk.name, partition.id) then
    local filesystem = partition.filesystem
    minimum = filesystems.MINIMUM_MB[filesystem] * alignment
    maximum = filesystems.MAXIMUM_MB[filesystem] and filesystems.MAXIMUM_MB[filesystem] * alignment or math.huge
  else
    minimum = math.max(alignment, partition.volumeSectors or 0)
    maximum = math.huge
  end

  return math.min(minimum, partition.sectors), math.max(maximum, partition.sectors)
end

-- Why that partition cannot be resized or moved, nil when it can.
local function resizeProblem(partition)
  local disk = state.disk

  if disk.table ~= "MBR" and disk.table ~= "GPT" then
    return disk.name .. " has no partition table: the filesystem takes the whole disk."
  end

  return operations.systemProblem(partition)
end

-- Adds the resize of a partition to the operations: nil, or why it cannot be done.
local function queueResize(partition, newStart, newSectors)
  local disk = state.disk
  local op = {
    kind = "resize", disk = disk.name, target = partition.id, newStart = newStart, newSectors = newSectors,
    moving = newStart ~= partition.start, text = operations.resizeText(disk, partition, newStart, newSectors),
  }

  local problem = operations.queue(op)

  if problem then
    return problem
  end

  window.queued(op, newStart)
  return nil
end

local function resizePartition()
  local segment = window.selectedPartition("resize or move")

  if not segment then
    return
  end

  local partition = segment.partition
  local problem = resizeProblem(partition)

  if problem then
    panel.showError("Resize/Move " .. partition.name, problem)
    return
  end

  local disk = state.disk
  local alignment = segments.perMiB(disk)
  local first, finish = segments.regionOf(disk, state.tree, segment)
  local alignedFirst = segments.alignUp(first, alignment)
  local room = finish > alignedFirst and (finish - alignedFirst) // alignment or 0
  local before = segment.start > alignedFirst and (segment.start - alignedFirst) // alignment or 0
  local size = segment.sectors // alignment
  local after = finish > segment.start + segment.sectors and (finish - segment.start - segment.sectors) // alignment or 0
  local minimum = sizeLimits(partition)

  ui.beforeBox.text = tostring(before)
  ui.sizeBox.text = tostring(size)

  local text = "Free space: " .. before .. " MB before it, " .. after .. " MB after; it can take up to " .. room .. " MB."

  if segment.logical then
    text = "It can grow up to " .. (finish - segment.start) // alignment .. " MB. Aura does not move logical partitions."
  end

  if partition.new and operations.foldable(disk.name, partition.id) then
    local filesystem = partition.filesystem
    text = text .. " It is created at that size: " .. filesystem .. " takes " .. filesystems.MINIMUM_MB[filesystem] .. " MB"
      .. (filesystems.MAXIMUM_MB[filesystem] and " to " .. filesystems.MAXIMUM_MB[filesystem] .. " MB." or " or more.")
  elseif (partition.volumeSectors or 0) > 0 then
    text = text .. " Its " .. partition.filesystem .. " volume keeps its size, " .. format.size(partition.volumeSectors * disk.sectorSize)
      .. ": the partition cannot get smaller, and a format uses the new room."
  end

  text = text .. " Dragging its sides or its middle on the bar does it too."

  panel.open("resize", segment, "Resize/Move " .. partition.name,
    { before = not segment.logical, size = true }, "Resize/Move", text)
  segment.original = { before = ui.beforeBox.text, size = ui.sizeBox.text, alignedFirst = alignedFirst, finish = finish, minimum = minimum }
end

local function resizeOk()
  local disk = state.disk
  local segment = state.panelSegment
  local partition = segment.partition
  local original = segment.original
  local alignment = segments.perMiB(disk)
  local before = segment.logical and 0 or panel.readMiB(ui.beforeBox)
  local size = panel.readMiB(ui.sizeBox)

  if not before or not size or size < 1 then
    panel.problem("Type the sizes in whole MB, the partition's 1 or more.")
    return
  end

  -- What was not changed stays to the sector: a partition another system made may not be in whole MB.
  local start = (segment.logical or ui.beforeBox.text == original.before) and segment.start or original.alignedFirst + before * alignment
  local sectors = ui.sizeBox.text == original.size and segment.sectors or size * alignment

  if start == segment.start and sectors == segment.sectors then
    panel.close()
    status.set("Nothing to change.")
    return
  elseif start + sectors > original.finish then
    local largest = start < original.finish and (original.finish - start) // alignment or 0
    panel.problem("That does not fit: from there, it can take up to " .. largest .. " MB.")
    return
  elseif sectors < original.minimum then
    panel.problem("It takes " .. format.size(original.minimum * disk.sectorSize) .. " or more.")
    return
  elseif partition.new and operations.foldable(disk.name, partition.id) and filesystems.sizeProblem(partition.filesystem, sectors // alignment) then
    panel.problem(filesystems.sizeProblem(partition.filesystem, sectors // alignment))
    return
  end

  local problem = queueResize(partition, start, sectors)

  if problem then
    panel.problem(problem)
    return
  end

  panel.close()
end

return {
  limits = sizeLimits,
  problem = resizeProblem,
  queue = queueResize,
  open = resizePartition,
  ok = resizeOk,
}
