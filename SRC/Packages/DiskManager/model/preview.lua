-- A disk as it will be once the pending operations are done: they are replayed on a copy of its
-- aura.disks entry, each checked as Aura will do it. A partition is known by an id: "p" and its first
-- sector for one on the disk, "n" and a number for one an operation creates.

local format = require "format"
local filesystems = require "model.filesystems"

local function copyTable(source)
  local copy = {}

  for key, value in pairs(source) do
    copy[key] = value
  end

  return copy
end

-- A copy of a disk entry the operations can change: its partitions copied too.
local function copyDisk(d)
  local copy = copyTable(d)
  copy.partitions = {}

  for i, partition in ipairs(d.partitions) do
    copy.partitions[i] = copyTable(partition)
  end

  if d.extended then
    copy.extended = copyTable(d.extended)
  end

  return copy
end

local function partitionById(d, id)
  for i, partition in ipairs(d.partitions) do
    if partition.id == id then
      return partition, i
    end
  end

  return nil
end

-- Why a partition cannot take the sectors [start, start + sectors) of the disk, nil when it can. except
-- is the partition that moves there, which does not overlap itself.
local function placeProblem(d, start, sectors, logical, except)
  if sectors < 1 then
    return "A partition takes 1 sector or more."
  end

  local first, finish

  if logical then
    if not d.extended then
      return "There is no extended partition for a logical one."
    end

    first, finish = d.extended.start + 1, d.extended.start + d.extended.sectors
  else
    first, finish = d.usableStart, d.usableEnd
  end

  if start < first or start + sectors > finish then
    return "It does not fit in " .. (logical and "the extended partition." or "the disk.")
  end

  -- A logical partition's EBR is the sector before it.
  local low, high = start - (logical and 1 or 0), start + sectors

  for _, partition in ipairs(d.partitions) do
    if partition ~= except and partition.logical == (logical or false) then
      local otherLow = partition.start - (partition.logical and 1 or 0)

      if low < partition.start + partition.sectors and otherLow < high then
        return "It overlaps " .. partition.name .. "."
      end
    end
  end

  if not logical and d.extended and start < d.extended.start + d.extended.sectors and d.extended.start < high then
    return "It overlaps the extended partition."
  end

  return nil
end

-- Does an operation to the disk d (a copy): nil, or why it cannot be done there.
local function play(d, op)
  if op.kind == "table" then
    d.table = op.table
    d.partitions = {}
    d.extended = nil
    d.primaries = 0
    d.error = nil

    -- As Cosmos writes them: a GPT's 128 entries and backup take 34 and 33 sectors at the ends.
    if op.table == "GPT" then
      d.usableStart, d.usableEnd = 34, d.sectors - 33
    else
      d.usableStart, d.usableEnd = 1, math.min(d.sectors, 0xFFFFFFFF)
    end

    return nil
  end

  if d.table ~= "MBR" and d.table ~= "GPT" then
    return d.name .. " has no partition table."
  end

  if op.kind == "create" then
    if not op.logical and d.table == "MBR" and d.primaries >= 4 then
      return "An MBR disk holds 4 primary partitions at most."
    end

    local problem = placeProblem(d, op.start, op.sectors, op.logical, nil)

    if problem then
      return problem
    end

    -- Cosmos puts a logical partition after the last one, its EBR right after that one's end.
    if op.logical then
      local expected = d.extended.start + 1

      for _, partition in ipairs(d.partitions) do
        if partition.logical then
          expected = math.max(expected, partition.start + partition.sectors + 1)
        end
      end

      if op.start ~= expected then
        return "Aura adds a logical partition right after the last one only."
      end
    end

    d.partitions[#d.partitions + 1] = {
      id = op.id, name = op.name, start = op.start, sectors = op.sectors, size = op.sectors * d.sectorSize,
      filesystem = op.filesystem, label = filesystems.labelAs(op.filesystem, op.label),
      volumeSectors = op.filesystem == "unformatted" and 0 or op.sectors,
      logical = op.logical or false, boot = false, system = false, new = true,
      mbrType = d.table == "MBR" and filesystems.mbrIdOf(op.filesystem) or nil,
      gptType = d.table == "GPT" and filesystems.gptTypeOf(op.filesystem) or nil,
    }

    if not op.logical and d.table == "MBR" then
      d.primaries = d.primaries + 1
    end

    return nil
  end

  local partition, index = partitionById(d, op.target)

  if not partition then
    return "That partition is not on " .. d.name .. " anymore."
  end

  if op.kind == "delete" then
    table.remove(d.partitions, index)

    if not partition.logical and d.table == "MBR" then
      d.primaries = d.primaries - 1
    end
  elseif op.kind == "resize" then
    if op.newSectors < (partition.volumeSectors or 0) then
      return "Its " .. partition.filesystem .. " volume takes " .. format.size(partition.volumeSectors * d.sectorSize)
        .. ": the partition cannot get smaller."
    end

    local problem = placeProblem(d, op.newStart, op.newSectors, partition.logical, partition)

    if problem then
      return problem
    end

    partition.start, partition.sectors = op.newStart, op.newSectors
    partition.size = op.newSectors * d.sectorSize
    partition.changed = true
  elseif op.kind == "format" then
    partition.filesystem = op.filesystem
    partition.label = filesystems.labelAs(op.filesystem, op.label)
    partition.volumeSectors = op.filesystem == "unformatted" and 0 or partition.sectors
    partition.mountPoint = nil
    partition.changed = true

    if d.table == "MBR" and not partition.logical then
      partition.mbrType = filesystems.mbrIdOf(op.filesystem)
    elseif d.table == "GPT" and op.filesystem ~= "unformatted" then
      partition.gptType = filesystems.gptTypeOf(op.filesystem)
    end
  elseif op.kind == "label" then
    partition.label = filesystems.labelAs(partition.filesystem, op.label)
    partition.changed = true
  end

  return nil
end

-- The disk as it will be once those operations (the ones for it) are done; and when one cannot be done,
-- why and its index.
local function previewOf(d, ops)
  local result = copyDisk(d)

  for i, op in ipairs(ops) do
    if op.disk == d.name then
      local problem = play(result, op)

      if problem then
        return result, problem, i
      end
    end
  end

  return result
end

return {
  copyTable = copyTable,
  copyDisk = copyDisk,
  partitionById = partitionById,
  of = previewOf,
}
