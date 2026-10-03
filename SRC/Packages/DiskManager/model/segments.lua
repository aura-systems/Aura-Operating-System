-- Where things are on a disk: its partitions, free spaces and extended partition in disk order, the
-- segments the bar and the list show; and its sectors counted in MB.
--
-- A segment: { kind = "partition"|"free"|"extended", start = , sectors = , partition = (an aura.disks
-- entry), logical = , tail = (the free space after the last one), children = (an extended partition's) }.

local MIB = 1024 * 1024

-- The sectors in a MB (MiB) of the disk.
local function perMiB(d)
  return math.max(1, MIB // d.sectorSize)
end

local function alignUp(sector, alignment)
  return (sector + alignment - 1) // alignment * alignment
end

-- The nearest MB boundary.
local function snap(sector, alignment)
  return (sector + alignment // 2) // alignment * alignment
end

local function bytesOf(d, segment)
  return segment.sectors * d.sectorSize
end

local function freeSegment(start, finish, logical)
  return { kind = "free", start = start, sectors = finish - start, logical = logical }
end

-- The segments in [first, finish), in disk order, with the free space between them: 1 MB or more,
-- what is less is alignment. The free space after the last one is its tail.
local function fill(d, items, first, finish, logical)
  local alignment = perMiB(d)
  local result, cursor = {}, first

  table.sort(items, function(a, b) return a.start < b.start end)

  for _, item in ipairs(items) do
    -- A logical partition's EBR is the sector before it.
    local taken = item.logical and item.start - 1 or item.start

    if taken > cursor and taken - cursor >= alignment then
      result[#result + 1] = freeSegment(cursor, taken, logical)
    end

    result[#result + 1] = item
    cursor = math.max(cursor, item.start + item.sectors)
  end

  if finish > cursor and finish - cursor >= alignment then
    local free = freeSegment(cursor, finish, logical)
    free.tail = true
    result[#result + 1] = free
  end

  return result
end

-- The disk's top level: its partitions, free spaces and extended partition, which holds the logical
-- partitions and its own free spaces. A disk with no table is free space; a filesystem on the whole disk
-- is its one partition.
local function build(d)
  if d.table == "none" or d.error then
    return { freeSegment(0, d.sectors, false) }
  end

  local primaries, logicals = {}, {}

  for _, partition in ipairs(d.partitions) do
    local segment = { kind = "partition", start = partition.start, sectors = partition.sectors,
      partition = partition, logical = partition.logical }

    if partition.logical then
      logicals[#logicals + 1] = segment
    else
      primaries[#primaries + 1] = segment
    end
  end

  if d.table == "whole" then
    return primaries
  end

  if d.extended then
    local first, finish = d.extended.start, d.extended.start + d.extended.sectors
    primaries[#primaries + 1] = { kind = "extended", start = first, sectors = d.extended.sectors,
      children = fill(d, logicals, first, finish, true) }
  end

  return fill(d, primaries, d.usableStart, d.usableEnd, false)
end

-- The tree's segments in a list: the extended partition's after it.
local function flatten(tree)
  local result = {}

  for _, item in ipairs(tree) do
    result[#result + 1] = item

    for _, child in ipairs(item.children or {}) do
      result[#result + 1] = child
    end
  end

  return result
end

-- The free space around a segment of the disk's tree, from the previous one's end to the next one's
-- start: where it can move and grow.
local function regionOf(d, tree, segment)
  local siblings = tree
  local first, finish = d.usableStart, d.usableEnd

  if segment.logical then
    for _, item in ipairs(tree) do
      if item.kind == "extended" then
        siblings = item.children
        first, finish = item.start, item.start + item.sectors
      end
    end
  end

  for i, item in ipairs(siblings) do
    if item == segment then
      local previous, next = siblings[i - 1], siblings[i + 1]

      if previous and previous.kind == "free" then
        previous = siblings[i - 2]
      end

      if next and next.kind == "free" then
        next = siblings[i + 2]
      end

      if previous then
        first = previous.start + previous.sectors
      end

      -- A logical partition's EBR is the sector before it.
      if next then
        finish = next.start - (next.logical and 1 or 0)
      end
    end
  end

  return first, finish
end

local function nameOf(segment)
  if segment.kind == "free" then
    return "unallocated"
  elseif segment.kind == "extended" then
    return "extended"
  end

  return segment.partition.name
end

local function filesystemOf(segment)
  if segment.kind == "partition" then
    return segment.partition.filesystem
  end

  return segment.kind == "free" and "unallocated" or "extended"
end

return {
  MIB = MIB,
  perMiB = perMiB,
  alignUp = alignUp,
  snap = snap,
  bytesOf = bytesOf,
  build = build,
  flatten = flatten,
  regionOf = regionOf,
  nameOf = nameOf,
  filesystemOf = filesystemOf,
}
