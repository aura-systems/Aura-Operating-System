-- Disk Manager: the disks and their partitions, as GParted shows them. A bar draws the partitions in
-- their place on the disk, a list gives their filesystem, mount point, label, size and use. It creates
-- partition tables; creates, deletes, moves, resizes, formats and labels partitions; and mounts and
-- unmounts their volumes.
--
-- The changes wait in a list of operations: the bar and the list show the disk as it will be once they
-- are done, Undo takes the last change back, and Apply does them all, in order. Mounting and unmounting
-- are done at once. A partition is known by an id: "p" and its first sector for one on the disk, "n"
-- and a number for one an operation creates.

local app = aura.app
local disks, fs = aura.disks, aura.fs

local diskBox = app:find("disks")
local bar = app:find("bar")
local list = app:find("partitions")
local header = app:find("header")
local headerPlain = app:find("headerPlain")
local status = app:find("status")
local mountButton = app:find("mount")
local unmountButton = app:find("unmount")
local panel = app:find("panel")
local panelTitle = app:find("panelTitle")
local panelText = app:find("panelText")
local fields = app:find("fields")
local okButton = app:find("ok")
local cancelButton = app:find("cancel")
local confirm = app:find("confirm")
local operations = app:find("operations")
local operationsTitle = app:find("operationsTitle")
local operationList = app:find("operationList")

local rows = {
  before = app:find("beforeRow"),
  size = app:find("sizeRow"),
  filesystem = app:find("filesystemRow"),
  label = app:find("labelRow"),
  table = app:find("tableRow"),
}

local beforeBox = app:find("before")
local sizeBox = app:find("size")
local filesystemBox = app:find("filesystem")
local labelBox = app:find("label")
local tableBox = app:find("tableType")

local MIB = 1024 * 1024

-- The filesystem drop-down's items, in order.
local FILESYSTEMS = { "FAT32", "FAT16", "FAT12", "ext2", "unformatted" }

-- The smallest and largest partition each filesystem takes, in MB, as its formatter needs it.
local MINIMUM_MB = { FAT32 = 33, FAT16 = 3, FAT12 = 1, ext2 = 1, unformatted = 1 }
local MAXIMUM_MB = { FAT16 = 2047, FAT12 = 127 }

-- What they take, for the panel.
local SIZES = "FAT32 takes 33 MB or more, FAT16 3 MB to 2 GB, FAT12 up to 127 MB, ext2 1 MB or more."

-- What a label holds, for the panel.
local LABEL_RULES = {
  FAT = "Up to 11 letters, digits, spaces, - and _: FAT writes them in capitals. Empty for no label.",
  ext2 = "Up to 16 ASCII letters, digits, spaces and punctuation. Empty for no label.",
}

-- The MBR system ID and the GPT type Aura gives each filesystem.
local MBR_IDS = { FAT32 = 0x0C, FAT16 = 0x0E, FAT12 = 0x01, ext2 = 0x83, unformatted = 0x83 }
local BASIC_DATA = "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7"
local LINUX_FILESYSTEM = "0FC63DAF-8483-4772-8E79-3D69D8477DE4"

-- GParted's colors for the filesystems.
local COLORS = {
  FAT32 = "#18D918",
  FAT16 = "#00FF00",
  FAT12 = "#00FF00",
  FAT = "#18D918",
  ext2 = "#9DB8D2",
  ext3 = "#7590AE",
  ext4 = "#4B6983",
  NTFS = "#42E5AC",
  exFAT = "#2E8B57",
  ["linux-swap"] = "#C1665A",
}

-- The bar's other colors: an unknown filesystem's, free space's (its border and inside), the extended
-- partition's, a volume's used part, the selection's frame.
local SHADES = {
  unknown = "#000000",
  unallocated = "#A9A9A9",
  unallocatedInside = "#C8C8C8",
  extended = "#7DFCFE",
  used = "#F8F8BA",
  selection = "#316AC5",
}

-- The bar: the space between two boxes, a box's colored border, the narrowest box; how near a box's side
-- a press resizes it from that side, and how far the mouse goes before a drag changes anything, in pixels.
local BAR = { gap = 4, border = 3, minWidth = 12, edge = 6, deadZone = 3 }

-- The cursor over the bar for what a press there does.
local CURSORS = { left = "resizeHorizontal", right = "resizeHorizontal", move = "grab" }

-- The panel's text, in characters a line.
local PANEL_COLUMNS = 32

-- MBR system IDs and GPT type GUIDs, by name.
local MBR_TYPES = {
  [0x01] = "FAT12", [0x04] = "FAT16 (< 32 MB)", [0x05] = "Extended", [0x06] = "FAT16",
  [0x07] = "NTFS or exFAT", [0x0B] = "FAT32", [0x0C] = "FAT32 (LBA)", [0x0E] = "FAT16 (LBA)",
  [0x0F] = "Extended (LBA)", [0x82] = "Linux swap", [0x83] = "Linux", [0x85] = "Linux extended",
  [0x8E] = "Linux LVM", [0xEE] = "GPT protective", [0xEF] = "EFI System",
}

local GPT_TYPES = {
  ["C12A7328-F81F-11D2-BA4B-00A0C93EC93B"] = { "EFI System", "esp" },
  [BASIC_DATA] = { "Basic data" },
  [LINUX_FILESYSTEM] = { "Linux filesystem" },
  ["21686148-6449-6E6F-744E-656564454649"] = { "BIOS boot", "bios_grub" },
  ["E3C9E316-0B5C-4DB8-817D-F92DF00215AE"] = { "Microsoft reserved", "msftres" },
  ["DE94BBA4-06D1-4D40-A16A-BFD50179D6AC"] = { "Windows recovery" },
  ["0657FD6D-A4AB-43C4-84E5-0933C84B4F4F"] = { "Linux swap" },
  ["E6D6D379-F507-44C2-A23C-238F2A3DF928"] = { "Linux LVM" },
}

-- The list's columns: a title and a width in characters, numbers on the right.
local COLUMNS = {
  { title = "Partition", width = 14 },
  { title = "File system", width = 12 },
  { title = "Mount", width = 6 },
  { title = "Label", width = 16 },
  { title = "Size", width = 9, right = true },
  { title = "Used", width = 9, right = true },
  { title = "Unused", width = 9, right = true },
  { title = "Flags", width = 10 },
}

-- An operation's icon in the list of operations.
local OPERATION_ICONS = {
  table = "16-partition-table.bmp",
  create = "16-add.bmp",
  delete = "16-delete.bmp",
  resize = "16-resize.bmp",
  format = "16-format.bmp",
  label = "16-label.bmp",
}

-- aura.disks.list(): the disks as they are; the one shown, as it is (realDisk) and as it will be once
-- the pending operations are done (disk). nil when there is none.
local diskList = {}
local realDisk
local disk

-- The operations to apply, in order: { kind = "table"|"create"|"delete"|"resize"|"format"|"label",
-- disk = (its name), text = , ... }; what create makes is id, the others change target. history holds
-- each list before a change, for Undo. A list is never changed: a change makes a new one.
local pending = {}
local history = {}

-- The number of the next partition an operation creates: "new #1".
local nextNew = 1

-- The disk's partitions, free spaces and extended partition: { kind = "partition"|"free"|"extended",
-- start = , sectors = , partition = (an aura.disks entry), logical = , tail = , children = }. tree is the
-- disk's top level, where the extended partition holds its own; segments has them all, as the list.
local tree = {}
local segments = {}

-- The bar's boxes: { segment = , x = , y = , width = , height = , scale = (sectors a pixel) }, an
-- extended partition's after it.
local boxes = {}

-- The partition being dragged on the bar: { id = , name = , mode = "left"|"right"|"move", x = (where the
-- press was), start = , sectors = , first = , finish = (where it can go), minimum = , maximum = , scale = ,
-- newStart = , newSectors = }; nil when none is.
local drag

-- Each mounted volume's { free = , total = } bytes by its mount point, read once: it goes through the
-- whole FAT. False for one that could not be read.
local spaces = {}

-- What the panel shows: "new", "resize", "format", "label", "table", "information" or "error", nil
-- while hidden; and the segment (or disk) it is about.
local panelAction
local panelSegment

-- What the confirm dialog asks about: the function Apply calls.
local confirmed

-- An action waits for the window to be drawn: the buttons do nothing until it ran. applying: the
-- operations are being applied, one a frame.
local busy = false
local applying = false

-- Text ----------------------------------------------------------------------------------------------

local function setStatus(text, color)
  status.text = text
  status.color = color or "black"
end

local function formatSize(bytes)
  if bytes < 1024 then
    return bytes .. " B"
  end

  local units = { "KB", "MB", "GB", "TB" }
  local unit, divisor = 1, 1024

  while unit < #units and bytes >= divisor * 1024 do
    unit, divisor = unit + 1, divisor * 1024
  end

  -- In integers: one decimal under 10.
  local tenths = bytes * 10 // divisor

  if tenths < 100 then
    return (tenths // 10) .. "." .. (tenths % 10) .. " " .. units[unit]
  end

  return (tenths // 10) .. " " .. units[unit]
end

-- The text in lines of at most that many characters, cut at the spaces.
local function wrap(text, columns)
  local lines = {}

  for paragraph in (text .. "\n"):gmatch("(.-)\n") do
    local line = ""

    for each in paragraph:gmatch("%S+") do
      -- Lua 5.5's loop variables are constants.
      local word = each

      while #word > columns do
        if line ~= "" then
          lines[#lines + 1] = line
          line = ""
        end

        lines[#lines + 1] = word:sub(1, columns)
        word = word:sub(columns + 1)
      end

      if line == "" then
        line = word
      elseif #line + 1 + #word <= columns then
        line = line .. " " .. word
      else
        lines[#lines + 1] = line
        line = word
      end
    end

    lines[#lines + 1] = line
  end

  return table.concat(lines, "\n")
end

-- A row of the list: the values in their columns, cut with "~" when too long.
local function columns(values)
  local parts = {}

  for i, column in ipairs(COLUMNS) do
    local text = values[i] or ""

    if #text > column.width then
      text = text:sub(1, column.width - 1) .. "~"
    end

    local padding = string.rep(" ", column.width - #text)
    parts[i] = column.right and padding .. text or text .. padding
  end

  return (table.concat(parts, " "):gsub("%s+$", ""))
end

local function headerText()
  local titles = {}

  for i, column in ipairs(COLUMNS) do
    titles[i] = column.title
  end

  return columns(titles)
end

local function plural(count, word)
  return count .. " " .. word .. (count == 1 and "" or "s")
end

-- "The operation before it was applied." or "The 3 operations before it were applied."
local function operationsSentence(count, where, done)
  if count == 1 then
    return "The operation " .. where .. " was " .. done .. "."
  end

  return "The " .. count .. " operations " .. where .. " were " .. done .. "."
end

-- Sectors -------------------------------------------------------------------------------------------

-- The sectors in a MB (MiB) of the disk.
local function perMiB()
  return math.max(1, MIB // disk.sectorSize)
end

local function alignUp(sector, alignment)
  return (sector + alignment - 1) // alignment * alignment
end

-- The nearest MB boundary.
local function snap(sector, alignment)
  return (sector + alignment // 2) // alignment * alignment
end

local function bytesOf(segment)
  return segment.sectors * disk.sectorSize
end

-- Filesystems ---------------------------------------------------------------------------------------

local function isFat(filesystem)
  return filesystem:sub(1, 3) == "FAT"
end

-- The label as the filesystem writes it: a FAT one in capitals, none on an unformatted partition.
local function labelAs(filesystem, label)
  label = (label or ""):match("^%s*(.-)%s*$")

  if isFat(filesystem) then
    return label:upper()
  end

  return filesystem == "ext2" and label or ""
end

-- Why that label does not go on a volume of that filesystem, nil when it does.
local function labelProblem(filesystem, label)
  label = label:match("^%s*(.-)%s*$")

  if isFat(filesystem) then
    if #label > 11 or label:find("[^%w _%-]") then
      return "A FAT label has up to 11 letters, digits, spaces, - and _."
    end
  elseif filesystem == "ext2" then
    if #label > 16 or label:find("[^\32-\126]") then
      return "An ext2 label has up to 16 ASCII letters, digits, spaces and punctuation."
    end
  end

  return nil
end

local function gptTypeOf(filesystem)
  return filesystem == "ext2" and LINUX_FILESYSTEM or BASIC_DATA
end

-- Why a partition of that many MB cannot take that filesystem, nil when it can.
local function sizeProblem(filesystem, megabytes)
  if megabytes < MINIMUM_MB[filesystem] then
    return filesystem .. " takes " .. MINIMUM_MB[filesystem] .. " MB or more."
  elseif MAXIMUM_MB[filesystem] and megabytes > MAXIMUM_MB[filesystem] then
    return filesystem .. " takes up to " .. MAXIMUM_MB[filesystem] .. " MB."
  end

  return nil
end

-- The filesystem drop-down's index of the largest FAT that fits in that many MB.
local function defaultFilesystem(megabytes)
  if megabytes >= MINIMUM_MB.FAT32 then
    return 0
  end

  return megabytes >= MINIMUM_MB.FAT16 and 1 or 2
end

-- What the disk will be ------------------------------------------------------------------------------

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
      filesystem = op.filesystem, label = labelAs(op.filesystem, op.label),
      volumeSectors = op.filesystem == "unformatted" and 0 or op.sectors,
      logical = op.logical or false, boot = false, system = false, new = true,
      mbrType = d.table == "MBR" and MBR_IDS[op.filesystem] or nil,
      gptType = d.table == "GPT" and gptTypeOf(op.filesystem) or nil,
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
      return "Its " .. partition.filesystem .. " volume takes " .. formatSize(partition.volumeSectors * d.sectorSize)
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
    partition.label = labelAs(op.filesystem, op.label)
    partition.volumeSectors = op.filesystem == "unformatted" and 0 or partition.sectors
    partition.mountPoint = nil
    partition.changed = true

    if d.table == "MBR" and not partition.logical then
      partition.mbrType = MBR_IDS[op.filesystem]
    elseif d.table == "GPT" and op.filesystem ~= "unformatted" then
      partition.gptType = gptTypeOf(op.filesystem)
    end
  elseif op.kind == "label" then
    partition.label = labelAs(partition.filesystem, op.label)
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

local function realOf(name)
  for _, d in ipairs(diskList) do
    if d.name == name then
      return d
    end
  end

  return nil
end

-- Disk ----------------------------------------------------------------------------------------------

local function freeSegment(start, finish, logical)
  return { kind = "free", start = start, sectors = finish - start, logical = logical }
end

-- The segments in [first, finish), in disk order, with the free space between them: 1 MB or more,
-- what is less is alignment. The free space after the last one is its tail.
local function fill(items, first, finish, logical)
  local alignment = perMiB()
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
local function buildTree(d)
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
      children = fill(logicals, first, finish, true) }
  end

  return fill(primaries, d.usableStart, d.usableEnd, false)
end

local function flatten(items)
  local result = {}

  for _, item in ipairs(items) do
    result[#result + 1] = item

    for _, child in ipairs(item.children or {}) do
      result[#result + 1] = child
    end
  end

  return result
end

-- Partitions ----------------------------------------------------------------------------------------

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

local function colorOf(segment)
  if segment.kind == "free" then
    return SHADES.unallocated
  elseif segment.kind == "extended" then
    return SHADES.extended
  end

  return COLORS[segment.partition.filesystem] or SHADES.unknown
end

-- The { free = , total = } of a mounted partition's volume, nil until read (or for none).
local function spaceOf(segment)
  local partition = segment.partition
  return partition and partition.mountPoint and spaces[partition.mountPoint] or nil
end

local function flagsOf(partition)
  local flags = {}

  if partition.boot then
    flags[#flags + 1] = "boot"
  end

  local gptType = partition.gptType and GPT_TYPES[partition.gptType]

  if gptType and gptType[2] then
    flags[#flags + 1] = gptType[2]
  end

  return table.concat(flags, ", ")
end

local function typeOf(partition)
  if partition.mbrType then
    local name = MBR_TYPES[partition.mbrType]
    return string.format("0x%02X", partition.mbrType) .. (name and " " .. name or "")
  elseif partition.gptType then
    local gptType = GPT_TYPES[partition.gptType]
    return gptType and gptType[1] or partition.gptType
  end

  return nil
end

local function rowText(segment)
  local values = { nameOf(segment), filesystemOf(segment), "", "", formatSize(bytesOf(segment)), "", "", "" }

  if segment.logical then
    values[1] = "  " .. values[1]
  end

  local partition = segment.partition

  if partition then
    values[3] = partition.mountPoint and partition.mountPoint:gsub("/$", "") or ""
    values[4] = partition.label
    values[8] = flagsOf(partition)

    local space = spaceOf(segment)

    if space then
      values[6] = formatSize(space.total - space.free)
      values[7] = formatSize(space.free)
    end
  end

  return columns(values)
end

local function iconOf(segment)
  local partition = segment.partition

  if not partition then
    return nil
  end

  -- As GParted's key: a mounted partition is in use.
  return partition.mountPoint and "16-locked.bmp" or "16-drive.bmp"
end

-- Bar -----------------------------------------------------------------------------------------------

-- The boxes' widths: as their sectors, each at least the narrowest, all of them the room left by
-- the gaps. Then the sectors a pixel stands for.
local function widths(items, total)
  local count = #items
  local room = math.max(count, total - (count - 1) * BAR.gap)
  local minimum = math.min(BAR.minWidth, room // count)
  local sectors = 0

  for _, item in ipairs(items) do
    sectors = sectors + item.sectors
  end

  local result, used = {}, 0

  for i, item in ipairs(items) do
    result[i] = math.max(minimum, sectors > 0 and room * item.sectors // sectors or minimum)
    used = used + result[i]
  end

  local function widest()
    local best = 1

    for i = 2, count do
      if result[i] > result[best] then
        best = i
      end
    end

    return best
  end

  -- The narrowest boxes made the others too wide: the widest give the pixels back.
  while used > room do
    local i = widest()

    if result[i] <= minimum then
      break
    end

    local take = math.min(used - room, result[i] - minimum)
    result[i] = result[i] - take
    used = used - take
  end

  -- Rounding left a few: the widest takes them.
  if used < room then
    local i = widest()
    result[i] = result[i] + room - used
  end

  return result, sectors / room
end

local function drawLines(lines, x, y, width, height)
  local columns = (width - 2 * BAR.border - 2) // 8

  if columns < 1 then
    return
  end

  local count = math.min(#lines, (height - 2 * BAR.border) // 16)
  local top = y + (height - count * 16) // 2

  for i = 1, count do
    local line = lines[i]

    if #line > columns then
      line = line:sub(1, columns)
    end

    bar:drawText(line, x + (width - #line * 8) // 2, top + (i - 1) * 16, "black")
  end
end

local layoutBoxes

-- A box: a border of its filesystem's color, its volume's used part in yellow, its name and size.
local function drawBox(segment, x, y, width, height, scale)
  boxes[#boxes + 1] = { segment = segment, x = x, y = y, width = width, height = height, scale = scale }
  bar:fillRect(x, y, width, height, colorOf(segment))

  local innerWidth, innerHeight = width - 2 * BAR.border, height - 2 * BAR.border

  if innerWidth <= 0 or innerHeight <= 0 then
    return
  end

  bar:fillRect(x + BAR.border, y + BAR.border, innerWidth, innerHeight, segment.kind == "free" and SHADES.unallocatedInside or "white")

  local space = spaceOf(segment)

  if space and space.total > 0 then
    local usedWidth = math.min(innerWidth, innerWidth * (space.total - space.free) // space.total)
    bar:fillRect(x + BAR.border, y + BAR.border, usedWidth, innerHeight, SHADES.used)
  end

  if segment.kind == "extended" then
    local inset = BAR.border + 2
    layoutBoxes(segment.children, x + inset, y + inset, width - 2 * inset, height - 2 * inset)
  else
    drawLines({ nameOf(segment), formatSize(bytesOf(segment)) }, x, y, width, height)
  end
end

layoutBoxes = function(items, x, y, width, height)
  if #items == 0 or width <= 0 or height <= 0 then
    return
  end

  local sizes, scale = widths(items, width)

  for i, item in ipairs(items) do
    drawBox(item, x, y, sizes[i], height, scale)
    x = x + sizes[i] + BAR.gap
  end
end

local function selectedSegment()
  return segments[list.selectedIndex + 1]
end

local function frame(box)
  local x, y, w, h = box.x - 2, box.y - 2, box.width + 4, box.height + 4
  bar:fillRect(x, y, w, 2, SHADES.selection)
  bar:fillRect(x, y + h - 2, w, 2, SHADES.selection)
  bar:fillRect(x, y, 2, h, SHADES.selection)
  bar:fillRect(x + w - 2, y, 2, h, SHADES.selection)
end

-- The disk's partitions on the bar, the selected one framed (the one dragged, while it is).
local function drawBar()
  bar:clear()
  boxes = {}

  if not disk then
    bar:drawText("No disk", 8, (bar.height - 16) // 2, "gray")
    return
  end

  -- Room around the boxes for the frame.
  layoutBoxes(tree, 3, 3, bar.width - 6, bar.height - 6)

  local selected = not drag and selectedSegment()

  for _, box in ipairs(boxes) do
    local partition = box.segment.partition

    if box.segment == selected or (drag and partition and partition.id == drag.id) then
      frame(box)
    end
  end
end

-- Status --------------------------------------------------------------------------------------------

local function tableName(d)
  if d.table == "MBR" or d.table == "GPT" then
    return d.table
  elseif d.table == "whole" then
    return "no partition table, a filesystem on the whole disk"
  end

  return "no partition table"
end

local function pendingOn(name)
  local count = 0

  for _, op in ipairs(pending) do
    if op.disk == name then
      count = count + 1
    end
  end

  return count
end

local function diskSummary(d)
  if d.error then
    return d.name .. " cannot be read: " .. d.error
  end

  local count = #d.partitions
  local waiting = pendingOn(d.name)

  return d.name .. ": " .. formatSize(d.size) .. ", " .. tableName(d) .. ", " .. plural(count, "partition")
    .. (waiting == 1 and ", once the pending operation is applied" or "")
    .. (waiting > 1 and ", once the " .. waiting .. " pending operations are applied" or "")
end

-- The status line: the selected segment, or the disk.
local function showStatus()
  if not disk then
    setStatus("No disk: Aura sees SATA (AHCI), NVMe and USB disks.")
    return
  end

  local segment = selectedSegment()

  if not segment then
    setStatus(diskSummary(disk), disk.error and "red" or "black")
    return
  end

  local text = nameOf(segment) .. ": "

  if segment.kind == "partition" then
    local partition = segment.partition
    text = text .. partition.filesystem .. (partition.label ~= "" and " '" .. partition.label .. "'" or "")
      .. ", " .. formatSize(bytesOf(segment))

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

    local space = spaceOf(segment)

    if space then
      text = text .. ", " .. formatSize(space.free) .. " free"
    end
  elseif segment.kind == "free" then
    text = text .. formatSize(bytesOf(segment)) .. " of free space" .. (segment.logical and " in the extended partition" or "")
  else
    text = text .. formatSize(bytesOf(segment)) .. ", holds the logical partitions"
  end

  setStatus(text)
end

-- Mount or Unmount, as the selected partition is.
local function showMountButton()
  local segment = selectedSegment()
  local mounted = segment and segment.partition and segment.partition.mountPoint ~= nil
  mountButton.visible = not mounted
  unmountButton.visible = mounted or false
end

-- List ----------------------------------------------------------------------------------------------

-- The disk's segments in the list, the one at that index (from 1) selected.
local function renderRows(index)
  local items, icons = {}, false

  for i, segment in ipairs(segments) do
    local icon = iconOf(segment)
    items[i] = { text = rowText(segment), icon = icon }
    icons = icons or icon ~= nil
  end

  list.items = items
  list.selectedIndex = index and index - 1 or -1

  header.visible = icons
  headerPlain.visible = not icons
end

-- The disk's segments in the list and on the bar, the one at that index (from 1) selected.
local function render(index)
  renderRows(index)
  drawBar()
  showStatus()
  showMountButton()
end

-- The index (from 1) of the segment of that kind starting at that sector, else of the one holding it.
local function indexAt(start, kind)
  if not start then
    return nil
  end

  for i, segment in ipairs(segments) do
    if segment.start == start and (not kind or segment.kind == kind) then
      return i
    end
  end

  for i, segment in ipairs(segments) do
    if segment.kind ~= "extended" and start >= segment.start and start < segment.start + segment.sectors then
      return i
    end
  end

  return nil
end

-- The index (from 1) of the partition with that id.
local function indexOfId(id)
  for i, segment in ipairs(segments) do
    if segment.partition and segment.partition.id == id then
      return i
    end
  end

  return nil
end

-- The operations under the list: shown while there are some.
local function renderOperations()
  local items = {}

  for i, op in ipairs(pending) do
    items[i] = { text = op.text, icon = OPERATION_ICONS[op.kind] }
  end

  operationList.items = items
  operationList.selectedIndex = -1
  operationsTitle.text = plural(#pending, "operation") .. " pending: Apply does them, in this order; Undo takes the last change back."
  operations.visible = #pending > 0
end

local readSpaces, information

-- Shows that disk (an entry of diskList) as it will be, the segment at that sector selected.
local function showDisk(d, selectStart, selectKind)
  realDisk = d
  disk = d and previewOf(d, pending) or nil
  tree = disk and buildTree(disk) or {}
  segments = flatten(tree)
  render(indexAt(selectStart, selectKind))
  readSpaces()
end

-- Shows the disk again after a change to the operations, the partition at that sector selected.
local function showChange(selectStart, selectKind)
  showDisk(realDisk, selectStart, selectKind)
  renderOperations()
end

-- Reads the volumes' free space, one a frame once the window shows the disk: each read goes through
-- the volume's whole FAT.
readSpaces = function()
  if not disk then
    return
  end

  for _, segment in ipairs(segments) do
    local partition = segment.partition
    local mountPoint = partition and partition.mountPoint

    if mountPoint and spaces[mountPoint] == nil then
      app:after(0, function()
        if spaces[mountPoint] ~= nil then
          return
        end

        local free, total = fs.space(mountPoint)
        spaces[mountPoint] = free and { free = free, total = total } or false

        -- The status line keeps what it says: the result of an action, maybe.
        if not drag then
          renderRows(list.selectedIndex >= 0 and list.selectedIndex + 1 or nil)
          drawBar()
        end

        if panelAction == "information" then
          information()
        end

        -- One read a frame: the next one once this one shows.
        readSpaces()
      end)

      return
    end
  end
end

local function diskName(d)
  local tableType = (d.table == "MBR" or d.table == "GPT") and d.table or "no table"
  return d.name .. " (" .. formatSize(d.size) .. ", " .. tableType .. ")"
end

-- The pending operations that still apply to the disks as they are now: a disk that went, or changed
-- under them (vol, another system), loses its own. How many went.
local function checkPending()
  local kept, dropped, broken = {}, 0, {}

  for _, op in ipairs(pending) do
    if broken[op.disk] == nil then
      local d = realOf(op.disk)
      broken[op.disk] = not d or select(2, previewOf(d, pending)) ~= nil
    end

    if broken[op.disk] then
      dropped = dropped + 1
    else
      kept[#kept + 1] = op
    end
  end

  if dropped > 0 then
    pending = kept
    history = {}
  end

  return dropped
end

-- Reads the disks again; shows the one with that name (else the first), the segment at that sector
-- selected.
local function reload(name, selectStart, selectKind)
  local ok, result = pcall(disks.list)
  diskList = ok and result or {}

  -- Each partition known by where it starts: the operations name them so.
  for _, d in ipairs(diskList) do
    for _, partition in ipairs(d.partitions) do
      partition.id = "p" .. partition.start
    end
  end

  local dropped = checkPending()
  local items, index = {}, 1

  for i, d in ipairs(diskList) do
    items[i] = diskName(d)

    if d.name == name then
      index = i
    end
  end

  if #items == 0 then
    items[1] = "No disk"
  end

  diskBox.items = items
  diskBox.selectedIndex = index - 1
  showDisk(diskList[index], selectStart, selectKind)
  renderOperations()

  if dropped > 0 then
    setStatus("The disks changed: " .. plural(dropped, "pending operation") .. " no longer fit and went.", "red")
  end
end

-- Reads the disks again, the same disk and segment selected.
local function refresh()
  local segment = selectedSegment()
  spaces = {}
  reload(disk and disk.name, segment and segment.start, segment and segment.kind)
end

-- Panel ---------------------------------------------------------------------------------------------

-- Shows the panel for an action: its title, the fields named (before, size, filesystem, label, table),
-- a text, and OK (named so; no OK for nil).
local function openPanel(action, segment, title, shown, okText, text, color)
  panelAction = action
  panelSegment = segment
  panelTitle.text = title
  panelTitle.color = action == "error" and "red" or "black"
  panelText.text = wrap(text or "", PANEL_COLUMNS)
  panelText.color = color or "black"
  okButton.text = okText or "OK"
  cancelButton.text = okText and "Cancel" or "Close"

  -- Showing a container shows everything in it: what the action does not use is hidden after.
  panel.visible = true

  local any = false

  for name in pairs(rows) do
    any = any or shown[name] or false
  end

  fields.visible = any

  for name, row in pairs(rows) do
    row.visible = shown[name] or false
  end

  okButton.visible = okText ~= nil

  if shown.size then
    sizeBox:focus()
  elseif shown.label then
    labelBox:focus()
  end
end

local function closePanel()
  panelAction = nil
  panelSegment = nil
  panel.visible = false
  list:focus()
end

-- Cancel or Close: the status line is about the selection again.
local function cancel()
  closePanel()
  showStatus()
end

-- A problem with what the panel asks for: in red under its fields.
local function panelProblem(text)
  panelText.text = wrap(text, PANEL_COLUMNS)
  panelText.color = "red"
end

local function showError(title, message)
  message = message or "Unknown error."
  openPanel("error", nil, title, {}, nil, message, "black")
  setStatus(message, "red")
end

-- Runs a job once the window shows the text in the status line.
local function run(text, job)
  if busy then
    return
  end

  busy = true
  setStatus(text)

  app:after(0, function()
    busy = false
    job()
  end)
end

-- The confirm dialog: its message takes 2 lines of 31 characters.
local function ask(title, message, action)
  confirmed = action
  confirm.title = title
  confirm.message = message
  confirm.visible = true
end

-- The selected segment, when it is a partition; else nil, and why in the status line.
local function selectedPartition(action)
  local segment = selectedSegment()

  if not disk then
    setStatus("There is no disk.", "red")
  elseif not segment then
    setStatus("Select a partition to " .. action .. " first.", "red")
  elseif segment.kind ~= "partition" then
    setStatus("Select a partition to " .. action .. ": this is " .. nameOf(segment) .. ".", "red")
  else
    return segment
  end

  return nil
end

-- Why Aura cannot change that partition now, nil when it can.
local function systemProblem(partition)
  if partition.system then
    return "Aura runs from this partition (" .. partition.mountPoint .. "): it cannot change while Aura uses it."
  end

  return nil
end

-- A whole number of MB in a text box, nil when it is not one.
local function readMiB(box)
  local text = box.text:match("^%s*(%d+)%s*$")
  return text and tonumber(text)
end

-- Operations ----------------------------------------------------------------------------------------

local function createText(op)
  return "Create " .. op.name .. ": " .. formatSize(op.sectors * op.sectorSize) .. " " .. op.filesystem
    .. (labelAs(op.filesystem, op.label) ~= "" and " '" .. labelAs(op.filesystem, op.label) .. "'" or "")
    .. (op.logical and ", logical," or "") .. " on " .. op.disk
end

local function resizeText(partition, newStart, newSectors)
  local moved = newStart - partition.start
  local parts = {}

  if moved ~= 0 then
    parts[#parts + 1] = "Move " .. partition.name .. " " .. formatSize(math.abs(moved) * disk.sectorSize)
      .. (moved > 0 and " to the right" or " to the left")
  end

  if newSectors ~= partition.sectors then
    local sizes = "from " .. formatSize(partition.sectors * disk.sectorSize) .. " to " .. formatSize(newSectors * disk.sectorSize)
    parts[#parts + 1] = moved ~= 0 and "resize it " .. sizes or "Resize " .. partition.name .. " " .. sizes
  end

  return table.concat(parts, " and ")
end

-- The operation that creates that partition, and its index; nil when it is on the disk already.
local function creationOf(name, id)
  for i, op in ipairs(pending) do
    if op.kind == "create" and op.disk == name and op.id == id then
      return op, i
    end
  end

  return nil
end

-- Whether a change to that partition can go into the operation creating it: it is to be created, and
-- nothing after that changes it.
local function foldable(name, id)
  local _, index = creationOf(name, id)

  if not index then
    return false
  end

  for i = index + 1, #pending do
    if pending[i].disk == name and pending[i].target == id then
      return false
    end
  end

  return true
end

-- The pending operations with op folded into the creation of the partition it changes, nil when it
-- cannot be: deleting it takes the creation and its changes away; formatting, labeling or resizing it
-- changes how it is created.
local function fold(op)
  if not op.target or not foldable(op.disk, op.target) then
    return nil
  end

  local result = {}

  if op.kind == "delete" then
    for _, other in ipairs(pending) do
      if other.disk ~= op.disk or (other.id ~= op.target and other.target ~= op.target) then
        result[#result + 1] = other
      end
    end

    return result
  end

  local creation, index = creationOf(op.disk, op.target)
  local folded = copyTable(creation)

  if op.kind == "format" then
    folded.filesystem, folded.label = op.filesystem, op.label
  elseif op.kind == "label" then
    folded.label = op.label
  elseif op.kind == "resize" then
    folded.start, folded.sectors = op.newStart, op.newSectors
  else
    return nil
  end

  folded.text = createText(folded)

  for i, other in ipairs(pending) do
    result[i] = i == index and folded or other
  end

  return result
end

-- A resize right after another of the same partition: the pending operations with both made one, from
-- where the partition was before the first (none when it goes back there). nil for another operation.
local function combine(op)
  local last = pending[#pending]

  if op.kind ~= "resize" or not last or last.kind ~= "resize" or last.disk ~= op.disk or last.target ~= op.target then
    return nil
  end

  local result = {}

  for i = 1, #pending - 1 do
    result[i] = pending[i]
  end

  local before = partitionById(previewOf(realOf(op.disk), result), op.target)

  if before and (before.start ~= op.newStart or before.sectors ~= op.newSectors) then
    local combined = copyTable(op)
    combined.moving = op.newStart ~= before.start
    combined.text = resizeText(before, op.newStart, op.newSectors)
    result[#result + 1] = combined
  else
    op.cancels = true
  end

  return result
end

-- Adds an operation after the pending ones, or folds it into the creation of the partition it changes,
-- or into the resize just before it: nil, or why it cannot be done once those are.
local function queue(op)
  local d = realOf(op.disk)
  local candidate = fold(op) or combine(op)

  if not candidate or select(2, previewOf(d, candidate)) then
    candidate = {}

    for i, other in ipairs(pending) do
      candidate[i] = other
    end

    candidate[#candidate + 1] = op

    local _, problem = previewOf(d, candidate)

    if problem then
      return problem
    end
  end

  history[#history + 1] = pending
  pending = candidate
  return nil
end

-- The disk again once an operation joined the others, the partition at that sector selected.
local function queued(op, selectStart, selectKind)
  showChange(selectStart, selectKind or "partition")
  setStatus(op.cancels and "Back where it was: the resize is no longer pending." or op.text .. ": pending, Apply does it.")
end

local function undo()
  if #history == 0 then
    setStatus("There is nothing to undo.")
    return
  end

  local before = pending
  pending = table.remove(history)

  -- What went: an operation of the longer list missing from the other.
  local undone

  for _, op in ipairs(before) do
    local found = false

    for _, other in ipairs(pending) do
      found = found or other == op
    end

    undone = undone or (not found and op)
  end

  local segment = selectedSegment()
  showChange(segment and segment.start, segment and segment.kind)
  setStatus(undone and "Undone: " .. undone.text .. "." or "Undone.")
end

-- Takes one operation out of the list (and, for a creation, the changes to what it creates), when the
-- ones after it do not need it.
local function removeOperation(index)
  local removed = pending[index]
  local candidate = {}

  for i, op in ipairs(pending) do
    local changesCreated = removed.kind == "create" and op.disk == removed.disk and op.target == removed.id

    if i ~= index and not changesCreated then
      candidate[#candidate + 1] = op
    end
  end

  local d = realOf(removed.disk)

  if d and select(2, previewOf(d, candidate)) then
    setStatus("The operations after it need it: take them out first, or Undo.", "red")
    return
  end

  history[#history + 1] = pending
  pending = candidate

  local segment = selectedSegment()
  showChange(segment and segment.start, segment and segment.kind)
  setStatus("Taken out: " .. removed.text .. ".")
end

local function clearAll()
  if #pending == 0 then
    setStatus("There is no operation to clear.")
    return
  end

  local count = #pending
  history[#history + 1] = pending
  pending = {}

  local segment = selectedSegment()
  showChange(segment and segment.start, segment and segment.kind)
  setStatus(plural(count, "operation") .. " cleared: Undo brings them back.")
end

-- Does one operation on the disks. starts holds where each partition starts now, by disk and id, as the
-- operations before moved them: a logical partition may start elsewhere than the preview said.
local function execute(op, starts)
  local function startOf(id)
    return starts[op.disk .. ":" .. id]
  end

  if op.kind == "table" then
    return disks.createTable(op.disk, op.table)
  elseif op.kind == "create" then
    local ok, result = disks.create(op.disk, op.start, op.sectors, op.filesystem, op.label)

    if ok then
      starts[op.disk .. ":" .. op.id] = result
    end

    return ok, result
  end

  local start = startOf(op.target)

  if not start then
    return false, "That partition is not on " .. op.disk .. " anymore."
  end

  if op.kind == "delete" then
    return disks.delete(op.disk, start)
  elseif op.kind == "resize" then
    local newStart = op.moving and op.newStart or start
    local ok, why = disks.resize(op.disk, start, newStart, op.newSectors)

    if ok then
      starts[op.disk .. ":" .. op.target] = newStart
    end

    return ok, why
  elseif op.kind == "format" then
    return disks.format(op.disk, start, op.filesystem, op.label)
  elseif op.kind == "label" then
    return disks.setLabel(op.disk, start, op.label)
  end

  return false, "Unknown operation."
end

-- Applies the pending operations, one a frame: the status line and the list of operations show which.
-- The first that fails stops the others.
local function applyAll()
  local all = pending
  local total = #all
  local starts = {}
  local name = disk and disk.name

  for _, d in ipairs(diskList) do
    for _, partition in ipairs(d.partitions) do
      starts[d.name .. ":" .. partition.id] = partition.start
    end
  end

  applying = true

  if panelAction then
    closePanel()
  end

  local function finish()
    applying = false
    pending = {}
    history = {}
    nextNew = 1
    spaces = {}
    reload(name)
  end

  local step

  step = function(i)
    if i > total then
      finish()
      setStatus(plural(total, "operation") .. " applied.", "green")
      return
    end

    local op = all[i]
    operationList.selectedIndex = i - 1
    setStatus("Applying " .. i .. " of " .. total .. ": " .. op.text .. "...")

    app:after(0, function()
      local ok, why = execute(op, starts)

      if not ok then
        finish()
        showError("Operation " .. i .. " of " .. total .. " failed", op.text .. ": " .. (why or "unknown error.")
          .. (i > 1 and " " .. operationsSentence(i - 1, "before it", "applied") or "")
          .. (i < total and " " .. operationsSentence(total - i, "after it", "not") or ""))
        return
      end

      step(i + 1)
    end)
  end

  step(1)
end

local function apply()
  if #pending == 0 then
    setStatus("There is no operation to apply.")
    return
  end

  ask("Apply", (#pending == 1 and "Apply the operation?" or "Apply the " .. #pending .. " operations?")
    .. " Keep the computer on until done.", applyAll)
end

-- Actions -------------------------------------------------------------------------------------------

-- The free space a new partition can take: from its first MB boundary. A logical one goes after the
-- last one, its EBR the sector before it.
local function newRoom(segment)
  local alignment = perMiB()
  local finish = segment.start + segment.sectors

  if segment.logical then
    return segment.start + 1, math.max(0, (finish - segment.start - 1) // alignment)
  end

  local first = alignUp(segment.start, alignment)
  return first, first < finish and (finish - first) // alignment or 0
end

-- The panel's text for that filesystem: what it takes, and its label.
local function filesystemText(room)
  local filesystem = FILESYSTEMS[filesystemBox.selectedIndex + 1] or "FAT32"
  local rules = isFat(filesystem) and LABEL_RULES.FAT or LABEL_RULES[filesystem] or "An unformatted partition has no label."
  return (room and "Up to " .. room .. " MB. " or "") .. SIZES .. " " .. rules
end

local function newPartition()
  local segment = selectedSegment()

  if not disk then
    setStatus("There is no disk.", "red")
    return
  elseif disk.table == "none" then
    showError("New partition", disk.name .. " has no partition table: create one first, with Partition table.")
    return
  elseif disk.table == "whole" then
    showError("New partition", disk.name .. " has a filesystem on the whole disk, and no partition table. Create one first, with Partition table: its files will be lost.")
    return
  elseif not segment or segment.kind ~= "free" then
    setStatus("Select unallocated space first.", "red")
    return
  elseif segment.logical and not segment.tail then
    showError("New partition", "Aura adds a logical partition after the last one only: select the free space at the end of the extended partition.")
    return
  elseif not segment.logical and disk.table == "MBR" and disk.primaries >= 4 then
    showError("New partition", "An MBR disk holds 4 primary partitions at most: delete one first, or use a GPT.")
    return
  end

  local _, room = newRoom(segment)

  if room < 1 then
    showError("New partition", "There is less than 1 MB of free space here.")
    return
  end

  beforeBox.text = "0"
  sizeBox.text = tostring(room)
  labelBox.text = ""
  filesystemBox.selectedIndex = defaultFilesystem(room)

  openPanel("new", segment, segment.logical and "New logical partition" or "New partition",
    { before = not segment.logical, size = true, filesystem = true, label = true }, "Add", filesystemText(room))
  panelSegment.room = room
end

local function createOk()
  local segment = panelSegment
  local first, room = newRoom(segment)
  local before = segment.logical and 0 or readMiB(beforeBox)
  local size = readMiB(sizeBox)
  local filesystem = FILESYSTEMS[filesystemBox.selectedIndex + 1] or "FAT32"
  local label = labelBox.text

  if not before or not size or size < 1 then
    panelProblem("Type the sizes in whole MB, the partition's 1 or more.")
    return
  elseif before + size > room then
    panelProblem("That is " .. (before + size) .. " MB: there are " .. room .. " MB here.")
    return
  elseif sizeProblem(filesystem, size) then
    panelProblem(sizeProblem(filesystem, size))
    return
  elseif labelProblem(filesystem, label) then
    panelProblem(labelProblem(filesystem, label))
    return
  end

  local alignment = perMiB()
  local start = first + before * alignment
  local op = {
    kind = "create", disk = disk.name, id = "n" .. nextNew, name = "new #" .. nextNew,
    start = start, sectors = size * alignment, filesystem = filesystem, label = label,
    logical = segment.logical or false, sectorSize = disk.sectorSize,
  }
  op.text = createText(op)

  local problem = queue(op)

  if problem then
    panelProblem(problem)
    return
  end

  nextNew = nextNew + 1
  closePanel()
  queued(op, start)
end

local function deletePartition()
  local segment = selectedPartition("delete")

  if not segment then
    return
  end

  local partition = segment.partition
  local problem = systemProblem(partition)

  if problem then
    showError("Delete " .. partition.name, problem)
    return
  end

  local op = { kind = "delete", disk = disk.name, target = partition.id,
    text = "Delete " .. partition.name .. " (" .. formatSize(bytesOf(segment)) .. " " .. partition.filesystem .. ")" }

  problem = queue(op)

  if problem then
    showError("Delete " .. partition.name, problem)
    return
  end

  if panelAction then
    closePanel()
  end

  queued(op, segment.start, "free")
end

-- The free space around a partition, from the previous one's end to the next one's start: where it
-- can move and grow.
local function regionOf(segment)
  local siblings = tree
  local first, finish = disk.usableStart, disk.usableEnd

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

-- The smallest and largest sizes a partition can take, in sectors: a partition to be created is made at
-- its new size, between what its filesystem takes; another one's volume keeps its size.
local function sizeLimits(partition)
  local alignment = perMiB()
  local minimum, maximum

  if partition.new and foldable(disk.name, partition.id) then
    local filesystem = partition.filesystem
    minimum = MINIMUM_MB[filesystem] * alignment
    maximum = MAXIMUM_MB[filesystem] and MAXIMUM_MB[filesystem] * alignment or math.huge
  else
    minimum = math.max(alignment, partition.volumeSectors or 0)
    maximum = math.huge
  end

  return math.min(minimum, partition.sectors), math.max(maximum, partition.sectors)
end

-- Why that partition cannot be resized or moved, nil when it can.
local function resizeProblem(partition)
  if disk.table ~= "MBR" and disk.table ~= "GPT" then
    return disk.name .. " has no partition table: the filesystem takes the whole disk."
  end

  return systemProblem(partition)
end

-- Adds the resize of a partition to the operations: nil, or why it cannot be done.
local function queueResize(partition, newStart, newSectors)
  local op = {
    kind = "resize", disk = disk.name, target = partition.id, newStart = newStart, newSectors = newSectors,
    moving = newStart ~= partition.start, text = resizeText(partition, newStart, newSectors),
  }

  local problem = queue(op)

  if problem then
    return problem
  end

  queued(op, newStart)
  return nil
end

local function resizePartition()
  local segment = selectedPartition("resize or move")

  if not segment then
    return
  end

  local partition = segment.partition
  local problem = resizeProblem(partition)

  if problem then
    showError("Resize/Move " .. partition.name, problem)
    return
  end

  local alignment = perMiB()
  local first, finish = regionOf(segment)
  local alignedFirst = alignUp(first, alignment)
  local room = finish > alignedFirst and (finish - alignedFirst) // alignment or 0
  local before = segment.start > alignedFirst and (segment.start - alignedFirst) // alignment or 0
  local size = segment.sectors // alignment
  local after = finish > segment.start + segment.sectors and (finish - segment.start - segment.sectors) // alignment or 0
  local minimum = sizeLimits(partition)

  beforeBox.text = tostring(before)
  sizeBox.text = tostring(size)

  local text = "Free space: " .. before .. " MB before it, " .. after .. " MB after; it can take up to " .. room .. " MB."

  if segment.logical then
    text = "It can grow up to " .. (finish - segment.start) // alignment .. " MB. Aura does not move logical partitions."
  end

  if partition.new and foldable(disk.name, partition.id) then
    text = text .. " It is created at that size: " .. partition.filesystem .. " takes " .. MINIMUM_MB[partition.filesystem]
      .. " MB" .. (MAXIMUM_MB[partition.filesystem] and " to " .. MAXIMUM_MB[partition.filesystem] .. " MB." or " or more.")
  elseif (partition.volumeSectors or 0) > 0 then
    text = text .. " Its " .. partition.filesystem .. " volume keeps its size, " .. formatSize(partition.volumeSectors * disk.sectorSize)
      .. ": the partition cannot get smaller, and a format uses the new room."
  end

  text = text .. " Dragging its sides or its middle on the bar does it too."

  openPanel("resize", segment, "Resize/Move " .. partition.name,
    { before = not segment.logical, size = true }, "Resize/Move", text)
  panelSegment.original = { before = beforeBox.text, size = sizeBox.text, alignedFirst = alignedFirst, finish = finish, minimum = minimum }
end

local function resizeOk()
  local segment = panelSegment
  local partition = segment.partition
  local original = segment.original
  local alignment = perMiB()
  local before = segment.logical and 0 or readMiB(beforeBox)
  local size = readMiB(sizeBox)

  if not before or not size or size < 1 then
    panelProblem("Type the sizes in whole MB, the partition's 1 or more.")
    return
  end

  -- What was not changed stays to the sector: a partition another system made may not be in whole MB.
  local start = (segment.logical or beforeBox.text == original.before) and segment.start or original.alignedFirst + before * alignment
  local sectors = sizeBox.text == original.size and segment.sectors or size * alignment

  if start == segment.start and sectors == segment.sectors then
    closePanel()
    setStatus("Nothing to change.")
    return
  elseif start + sectors > original.finish then
    local largest = start < original.finish and (original.finish - start) // alignment or 0
    panelProblem("That does not fit: from there, it can take up to " .. largest .. " MB.")
    return
  elseif sectors < original.minimum then
    panelProblem("It takes " .. formatSize(original.minimum * disk.sectorSize) .. " or more.")
    return
  elseif partition.new and foldable(disk.name, partition.id) and sizeProblem(partition.filesystem, sectors // alignment) then
    panelProblem(sizeProblem(partition.filesystem, sectors // alignment))
    return
  end

  local problem = queueResize(partition, start, sectors)

  if problem then
    panelProblem(problem)
    return
  end

  panelAction = nil
  panelSegment = nil
  panel.visible = false
  list:focus()
end

local function formatPartition()
  local segment = selectedPartition("format")

  if not segment then
    return
  end

  local partition = segment.partition
  local problem = systemProblem(partition)

  if problem then
    showError("Format " .. partition.name, problem)
    return
  end

  filesystemBox.selectedIndex = defaultFilesystem(bytesOf(segment) // MIB)

  for i, name in ipairs(FILESYSTEMS) do
    if name == partition.filesystem then
      filesystemBox.selectedIndex = i - 1
    end
  end

  labelBox.text = partition.label

  openPanel("format", segment, "Format " .. partition.name, { filesystem = true, label = true }, "Format",
    "Everything on " .. partition.name .. " will be lost. " .. filesystemText(nil))
end

local function formatOk()
  local segment = panelSegment
  local partition = segment.partition
  local filesystem = FILESYSTEMS[filesystemBox.selectedIndex + 1] or "FAT32"
  local label = labelBox.text
  local problem = sizeProblem(filesystem, bytesOf(segment) // MIB) or labelProblem(filesystem, label)

  if problem then
    panelProblem(problem)
    return
  end

  local op = { kind = "format", disk = disk.name, target = partition.id, filesystem = filesystem, label = label,
    text = "Format " .. partition.name .. " as " .. filesystem
      .. (labelAs(filesystem, label) ~= "" and " '" .. labelAs(filesystem, label) .. "'" or "") }

  problem = queue(op)

  if problem then
    panelProblem(problem)
    return
  end

  closePanel()
  queued(op, segment.start)
end

local function labelPartition()
  local segment = selectedPartition("label")

  if not segment then
    return
  end

  local partition = segment.partition

  if not isFat(partition.filesystem) and partition.filesystem ~= "ext2" then
    showError("Label " .. partition.name, "Aura labels FAT and ext2 volumes only: this one is " .. partition.filesystem .. ".")
    return
  end

  local problem = systemProblem(partition)

  if problem then
    showError("Label " .. partition.name, problem)
    return
  end

  labelBox.text = partition.label
  openPanel("label", segment, "Label " .. partition.name, { label = true }, "Label",
    isFat(partition.filesystem) and LABEL_RULES.FAT or LABEL_RULES.ext2)
end

local function labelOk()
  local segment = panelSegment
  local partition = segment.partition
  local label = labelBox.text:match("^%s*(.-)%s*$")
  local problem = labelProblem(partition.filesystem, label)

  if problem then
    panelProblem(problem)
    return
  end

  local written = labelAs(partition.filesystem, label)
  local op = { kind = "label", disk = disk.name, target = partition.id, label = label,
    text = written == "" and "Remove the label of " .. partition.name or "Label " .. partition.name .. " '" .. written .. "'" }

  problem = queue(op)

  if problem then
    panelProblem(problem)
    return
  end

  closePanel()
  queued(op, segment.start)
end

-- Why mounting or unmounting cannot wait for the disk's operations, nil when there are none.
local function pendingProblem()
  local count = pendingOn(disk.name)

  if count > 0 then
    return "Apply or undo the " .. plural(count, "operation") .. " pending on " .. disk.name .. " first."
  end

  return nil
end

local function mountPartition()
  local segment = selectedPartition("mount")

  if not segment then
    return
  end

  local partition = segment.partition
  local problem = pendingProblem()

  if problem then
    showError("Mount " .. partition.name, problem)
    return
  end

  local name, start = disk.name, segment.start

  run("Mounting " .. partition.name .. "...", function()
    local ok, result = disks.mount(name, start)
    reload(name, start, "partition")

    if ok then
      setStatus(partition.name .. " mounted at " .. result .. ".", "green")
    else
      showError("Could not mount " .. partition.name, result)
    end
  end)
end

local function unmountPartition()
  local segment = selectedPartition("unmount")

  if not segment then
    return
  end

  local partition = segment.partition

  if partition.system then
    showError("Unmount " .. partition.name, "Aura runs from this partition (" .. partition.mountPoint .. "): it stays mounted.")
    return
  end

  local problem = pendingProblem()

  if problem then
    showError("Unmount " .. partition.name, problem)
    return
  end

  local name, start = disk.name, segment.start

  run("Unmounting " .. partition.name .. "...", function()
    local ok, why = disks.unmount(name, start)
    spaces[partition.mountPoint] = nil
    reload(name, start, "partition")

    if ok then
      setStatus(partition.name .. " unmounted: it can be unplugged.", "green")
    else
      showError("Could not unmount " .. partition.name, why)
    end
  end)
end

local function newTable()
  if not disk then
    setStatus("There is no disk.", "red")
    return
  end

  for _, partition in ipairs(disk.partitions) do
    if partition.system then
      showError("Partition table", "Aura runs from " .. partition.name .. " on " .. disk.name .. " (" .. partition.mountPoint .. "): its partition table cannot change while Aura uses it.")
      return
    end
  end

  -- MBR addresses 2 TB at most.
  tableBox.selectedIndex = disk.size >= 2 * 1024 * 1024 * MIB and 1 or 0

  openPanel("table", disk, "New partition table on " .. disk.name, { table = true }, "Add",
    "Every partition on " .. disk.name .. " and all their files will be lost. MBR holds 4 primary partitions of up to 2 TB, GPT 128 of any size.")
end

local function tableOk()
  local d = panelSegment
  local tableType = tableBox.selectedIndex == 1 and "GPT" or "MBR"
  local op = { kind = "table", disk = d.name, table = tableType,
    text = "Write a new " .. tableType .. " on " .. d.name .. ", without its partitions" }

  local problem = queue(op)

  if problem then
    panelProblem(problem)
    return
  end

  closePanel()
  queued(op)
end

-- What the panel tells about the selected segment, or the disk.
information = function()
  if not disk then
    setStatus("There is no disk.", "red")
    return
  end

  local segment = selectedSegment()
  local alignment = perMiB()
  local lines = {}

  local function add(name, value)
    lines[#lines + 1] = name .. ": " .. tostring(value)
  end

  if not segment then
    add("Size", formatSize(disk.size))
    add("Partition table", tableName(disk))
    add("Partitions", #disk.partitions)
    add("Sectors", disk.sectors)
    add("Sector size", disk.sectorSize .. " bytes")

    if pendingOn(disk.name) > 0 then
      add("Pending", plural(pendingOn(disk.name), "operation"))
    end

    if disk.error then
      add("Error", disk.error)
    end

    openPanel("information", nil, disk.name, {}, nil, table.concat(lines, "\n"))
    return
  end

  if segment.kind == "partition" then
    local partition = segment.partition
    local space = spaceOf(segment)

    add("File system", partition.filesystem)
    add("Label", partition.label ~= "" and partition.label or "none")
    add("Mount point", partition.mountPoint and partition.mountPoint .. (partition.system and " (Aura runs from it)" or "") or "not mounted")
    add("Size", formatSize(bytesOf(segment)))

    if space then
      add("Used", formatSize(space.total - space.free) .. ", unused: " .. formatSize(space.free))
    end

    if (partition.volumeSectors or 0) > 0 and partition.volumeSectors < partition.sectors then
      add("Volume", formatSize(partition.volumeSectors * disk.sectorSize) .. ": a format uses the whole partition")
    end

    local partitionType = typeOf(partition)

    if partitionType then
      add("Type", partitionType)
    end

    local flags = flagsOf(partition)
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
    add("Size", formatSize(bytesOf(segment)))
  end

  add("Sectors", segment.start .. " to " .. (segment.start + segment.sectors - 1))
  add("Sector count", segment.sectors)

  if segment.kind == "free" and segment.sectors < alignment then
    lines[#lines + 1] = "Less than 1 MB: no partition fits."
  end

  openPanel("information", segment, nameOf(segment), {}, nil, table.concat(lines, "\n"))
end

local function ok()
  if panelAction == "new" then
    createOk()
  elseif panelAction == "resize" then
    resizeOk()
  elseif panelAction == "format" then
    formatOk()
  elseif panelAction == "label" then
    labelOk()
  elseif panelAction == "table" then
    tableOk()
  end
end

-- Another filesystem in the panel: what it takes, and its label.
local function filesystemChanged()
  if panelAction == "new" then
    panelText.text = wrap(filesystemText(panelSegment.room), PANEL_COLUMNS)
    panelText.color = "black"
  elseif panelAction == "format" then
    panelText.text = wrap("Everything on " .. panelSegment.partition.name .. " will be lost. " .. filesystemText(nil), PANEL_COLUMNS)
    panelText.color = "black"
  end
end

local function confirmOk()
  confirm.visible = false
  local action = confirmed
  confirmed = nil

  if action then
    action()
  end
end

local function closeConfirm()
  confirm.visible = false
  confirmed = nil
end

-- Selection -----------------------------------------------------------------------------------------

local function selectionChanged()
  drawBar()
  showStatus()
  showMountButton()

  -- The panel tells about the selection, or asks about the one it was opened for.
  if panelAction == "information" or panelAction == "error" then
    information()
  end
end

-- An operation picked in their list: its partition, on its disk, is selected.
local function selectOperation()
  local op = pending[operationList.selectedIndex + 1]

  if not op or applying then
    return
  end

  if not disk or op.disk ~= disk.name then
    for i, d in ipairs(diskList) do
      if d.name == op.disk then
        diskBox.selectedIndex = i - 1
        showDisk(d)
      end
    end
  end

  local index = indexOfId(op.target or op.id)

  if index then
    list.selectedIndex = index - 1
    selectionChanged()
  end

  setStatus(op.text .. ". Delete takes it out of the operations.")
end

-- Dragging ------------------------------------------------------------------------------------------

-- The innermost box at that point of the bar.
local function boxAt(x, y)
  local found

  for _, box in ipairs(boxes) do
    if x >= box.x and x < box.x + box.width and y >= box.y and y < box.y + box.height then
      found = box
    end
  end

  return found
end

-- What a press at x on that box does: resize the partition from its "left" or "right" side, "move" it,
-- or nothing (nil).
local function dragModeAt(box, x)
  local segment = box.segment

  if segment.kind ~= "partition" or resizeProblem(segment.partition) then
    return nil
  end

  local fromLeft, fromRight = x - box.x, box.x + box.width - 1 - x

  if fromRight < BAR.edge and fromRight <= fromLeft then
    return "right"
  elseif segment.logical then
    -- Aura moves no logical partition, so their start stays.
    return nil
  elseif fromLeft < BAR.edge then
    return "left"
  end

  return "move"
end

-- Where the dragged partition goes for the mouse dx pixels from the press: in whole MB, in its free
-- space, its size within its limits. Where it was while the mouse is near the press.
local function dragged(dx)
  local start, sectors = drag.start, drag.sectors

  if math.abs(dx) < BAR.deadZone then
    return start, sectors
  end

  local alignment = perMiB()
  local delta = math.floor(dx * drag.scale)
  local first = alignUp(drag.first, alignment)
  local ending = start + sectors

  if drag.mode == "move" then
    if drag.finish - sectors < first then
      return start, sectors
    end

    return math.max(first, math.min(snap(start + delta, alignment), drag.finish - sectors)), sectors
  elseif drag.mode == "right" then
    local newEnd = math.min(snap(ending + delta, alignment), drag.finish, start + drag.maximum)
    newEnd = math.max(newEnd, start + drag.minimum)
    return start, newEnd - start
  end

  local newStart = math.max(snap(start + delta, alignment), first, ending - drag.maximum)
  newStart = math.min(newStart, ending - drag.minimum)
  return newStart, ending - newStart
end

-- The status line while dragging: what the partition becomes.
local function dragText()
  local size = function(sectors) return formatSize(sectors * disk.sectorSize) end
  local moved = drag.newStart - drag.start

  if drag.mode == "move" then
    return "Move " .. drag.name .. ": " .. (moved == 0 and "where it is" or size(math.abs(moved)) .. (moved > 0 and " to the right" or " to the left"))
  end

  return "Resize " .. drag.name .. ": " .. size(drag.newSectors) .. ", was " .. size(drag.sectors)
    .. ". Release the button to add it to the operations."
end

-- The bar with the dragged partition where it goes now.
local function drawDrag()
  local d = copyDisk(disk)
  local partition = partitionById(d, drag.id)
  partition.start, partition.sectors = drag.newStart, drag.newSectors
  partition.size = drag.newSectors * d.sectorSize

  tree = buildTree(d)
  segments = flatten(tree)
  drawBar()
  setStatus(dragText())
end

-- A press on the bar selects the innermost box under it, and starts dragging the partition when that
-- can change.
local function barPress()
  drag = nil

  if busy or applying then
    return
  end

  local x, y = bar.clickX, bar.clickY
  local box = boxAt(x, y)

  if not box then
    return
  end

  for i, segment in ipairs(segments) do
    if segment == box.segment then
      list.selectedIndex = i - 1
      list:focus()
      selectionChanged()
    end
  end

  local mode = dragModeAt(box, x)

  if not mode then
    return
  end

  -- The panel's form is about the partition as it was.
  if panelAction and panelAction ~= "information" and panelAction ~= "error" then
    closePanel()
  end

  local segment = box.segment
  local partition = segment.partition
  local first, finish = regionOf(segment)
  local minimum, maximum = sizeLimits(partition)

  drag = {
    id = partition.id, name = partition.name, mode = mode, x = x,
    start = segment.start, sectors = segment.sectors, first = first, finish = finish,
    minimum = minimum, maximum = maximum, scale = box.scale,
    newStart = segment.start, newSectors = segment.sectors,
  }
end

-- The mouse over the bar: the cursor says what a press there does. While dragging, the partition follows.
local function barMove()
  if not drag then
    if not bar.pressed then
      local x = bar.mouseX
      local box = not busy and not applying and boxAt(x, bar.mouseY)
      local cursor = box and CURSORS[dragModeAt(box, x)] or "normal"

      if bar.cursor ~= cursor then
        bar.cursor = cursor
      end
    end

    return
  end

  local newStart, newSectors = dragged(bar.mouseX - drag.x)

  if newStart ~= drag.newStart or newSectors ~= drag.newSectors then
    drag.newStart, drag.newSectors = newStart, newSectors
    drawDrag()
  end
end

-- The button released: the resize or move joins the operations.
local function barRelease()
  local done = drag
  drag = nil

  if not done then
    return
  end

  if done.newStart == done.start and done.newSectors == done.sectors then
    -- A click: the bar as it was.
    showDisk(realDisk, done.start, "partition")
    return
  end

  local partition = partitionById(disk, done.id)
  local problem = queueResize(partition, done.newStart, done.newSectors)

  if problem then
    showDisk(realDisk, done.start, "partition")
    setStatus(problem, "red")
  end
end

-- Handlers ------------------------------------------------------------------------------------------

local function guard(action)
  return function()
    if not busy and not applying then
      action()
    end
  end
end

app:on("new", guard(newPartition))
app:on("delete", guard(deletePartition))
app:on("resize", guard(resizePartition))
app:on("format", guard(formatPartition))
app:on("label", guard(labelPartition))
app:on("undo", guard(undo))
app:on("apply", guard(apply))
app:on("clear", guard(clearAll))
app:on("mount", guard(mountPartition))
app:on("unmount", guard(unmountPartition))
app:on("information", guard(information))
app:on("table", guard(newTable))
app:on("refresh", guard(refresh))
app:on("ok", guard(ok))
app:on("cancel", cancel)
app:on("filesystem", filesystemChanged)
app:on("confirmOk", confirmOk)
app:on("closeConfirm", closeConfirm)
app:on("select", selectionChanged)
app:on("operation", selectOperation)
app:on("barPress", barPress)
app:on("barMove", barMove)
app:on("barRelease", barRelease)

app:on("disk", function()
  local d = diskList[diskBox.selectedIndex + 1]

  if d and d ~= realDisk and not applying then
    if panelAction then
      closePanel()
    end

    showDisk(d)
  end
end)

-- The keys the list and the text boxes do not use.
app:onKey(function(key)
  local name = key.name

  if confirm.visible then
    if name == "enter" then
      confirmOk()
    elseif name == "escape" then
      closeConfirm()
    end
  elseif name == "escape" and panelAction then
    cancel()
  elseif busy or applying then
    return
  elseif key.ctrl and key.char == "z" then
    undo()
  elseif name == "enter" then
    information()
  elseif name == "delete" and operationList.focused and operationList.selectedIndex >= 0 then
    removeOperation(operationList.selectedIndex + 1)
  elseif name == "delete" then
    deletePartition()
  elseif name == "insert" then
    newPartition()
  elseif name == "f2" then
    labelPartition()
  elseif name == "f5" then
    refresh()
  end
end)

-- The bar is as wide as the window.
app:onResize(function()
  if not drag then
    drawBar()
  end
end)

-- Something else (vol, a USB stick) may have changed the disks: read them again each time the window
-- comes to the front.
local wasFocused = true

app:every(500, function()
  local focused = app.focused

  if focused and not wasFocused and not busy and not applying and not drag and not confirm.visible then
    local segment = selectedSegment()
    reload(disk and disk.name, segment and segment.start, segment and segment.kind)
  end

  wasFocused = focused
end)

header.text = headerText()
headerPlain.text = header.text
reload(...)
list:focus()
