-- Disk Manager: the disks and their partitions, as GParted shows them. A bar draws the partitions in
-- their place on the disk, a list gives their filesystem, mount point, label, size and use. It creates
-- partition tables; creates, deletes, moves, resizes, formats and labels partitions; and mounts and
-- unmounts their volumes. Each action is done once it is confirmed: there is no queue of them.

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
local FILESYSTEMS = { "FAT32", "FAT16", "FAT12", "unformatted" }

-- The smallest and largest partition each FAT takes, in MB, as the formatter needs it.
local MINIMUM_MB = { FAT32 = 33, FAT16 = 3, FAT12 = 1, unformatted = 1 }
local MAXIMUM_MB = { FAT16 = 2047, FAT12 = 127 }

-- What the FATs take, for the panel.
local FAT_SIZES = "FAT32 takes 33 MB or more, FAT16 3 MB to 2 GB, FAT12 up to 127 MB."

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

local UNKNOWN_COLOR = "#000000"
local UNALLOCATED_COLOR = "#A9A9A9"
local UNALLOCATED_INSIDE = "#C8C8C8"
local EXTENDED_COLOR = "#7DFCFE"
local USED_COLOR = "#F8F8BA"
local SELECTION_COLOR = "#316AC5"

-- The bar: the space between two boxes, a box's colored border, the narrowest box.
local GAP = 4
local BORDER = 3
local MIN_WIDTH = 12

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
  ["EBD0A0A2-B9E5-4433-87C0-68B6B72699C7"] = { "Basic data" },
  ["0FC63DAF-8483-4772-8E79-3D69D8477DE4"] = { "Linux filesystem" },
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
  { title = "Label", width = 11 },
  { title = "Size", width = 9, right = true },
  { title = "Used", width = 9, right = true },
  { title = "Unused", width = 9, right = true },
  { title = "Flags", width = 12 },
}

-- aura.disks.list(), and the disk shown (one of them), nil when there is none.
local diskList = {}
local disk

-- The disk's partitions, free spaces and extended partition: { kind = "partition"|"free"|"extended",
-- start = , sectors = , partition = (an aura.disks entry), logical = , tail = , children = }. tree is the
-- disk's top level, where the extended partition holds its own; segments has them all, as the list.
local tree = {}
local segments = {}

-- The bar's boxes: { segment = , x = , y = , width = , height = }, an extended partition's after it.
local boxes = {}

-- Each mounted volume's { free = , total = } bytes by its mount point, read once: it goes through the
-- whole FAT. False for one that could not be read.
local spaces = {}

-- What the panel shows: "new", "resize", "format", "label", "table", "information" or "error", nil
-- while hidden; and the segment (or disk) it is about.
local panelAction
local panelSegment

-- What the confirm dialog asks about: the function Apply calls.
local confirmed

-- An action waits for the window to be drawn: the buttons do nothing until it ran.
local busy = false

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

-- Sectors -------------------------------------------------------------------------------------------

-- The sectors in a MB (MiB) of the disk.
local function perMiB()
  return math.max(1, MIB // disk.sectorSize)
end

local function alignUp(sector, alignment)
  return (sector + alignment - 1) // alignment * alignment
end

local function bytesOf(segment)
  return segment.sectors * disk.sectorSize
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
    return UNALLOCATED_COLOR
  elseif segment.kind == "extended" then
    return EXTENDED_COLOR
  end

  return COLORS[segment.partition.filesystem] or UNKNOWN_COLOR
end

local function isFat(partition)
  return partition.filesystem:sub(1, 3) == "FAT"
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
-- the gaps.
local function widths(items, total)
  local count = #items
  local room = math.max(count, total - (count - 1) * GAP)
  local minimum = math.min(MIN_WIDTH, room // count)
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

  return result
end

local function drawLines(lines, x, y, width, height)
  local columns = (width - 2 * BORDER - 2) // 8

  if columns < 1 then
    return
  end

  local count = math.min(#lines, (height - 2 * BORDER) // 16)
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
local function drawBox(segment, x, y, width, height)
  boxes[#boxes + 1] = { segment = segment, x = x, y = y, width = width, height = height }
  bar:fillRect(x, y, width, height, colorOf(segment))

  local innerWidth, innerHeight = width - 2 * BORDER, height - 2 * BORDER

  if innerWidth <= 0 or innerHeight <= 0 then
    return
  end

  bar:fillRect(x + BORDER, y + BORDER, innerWidth, innerHeight, segment.kind == "free" and UNALLOCATED_INSIDE or "white")

  local space = spaceOf(segment)

  if space and space.total > 0 then
    local usedWidth = innerWidth * (space.total - space.free) // space.total
    bar:fillRect(x + BORDER, y + BORDER, usedWidth, innerHeight, USED_COLOR)
  end

  if segment.kind == "extended" then
    local inset = BORDER + 2
    layoutBoxes(segment.children, x + inset, y + inset, width - 2 * inset, height - 2 * inset)
  else
    drawLines({ nameOf(segment), formatSize(bytesOf(segment)) }, x, y, width, height)
  end
end

layoutBoxes = function(items, x, y, width, height)
  if #items == 0 or width <= 0 or height <= 0 then
    return
  end

  local sizes = widths(items, width)

  for i, item in ipairs(items) do
    drawBox(item, x, y, sizes[i], height)
    x = x + sizes[i] + GAP
  end
end

local function selectedSegment()
  return segments[list.selectedIndex + 1]
end

-- The disk's partitions on the bar, the selected one framed.
local function drawBar()
  bar:clear()
  boxes = {}

  if not disk then
    bar:drawText("No disk", 8, (bar.height - 16) // 2, "gray")
    return
  end

  -- Room around the boxes for the frame.
  layoutBoxes(tree, 3, 3, bar.width - 6, bar.height - 6)

  local selected = selectedSegment()

  for _, box in ipairs(boxes) do
    if box.segment == selected then
      local x, y, w, h = box.x - 2, box.y - 2, box.width + 4, box.height + 4
      bar:fillRect(x, y, w, 2, SELECTION_COLOR)
      bar:fillRect(x, y + h - 2, w, 2, SELECTION_COLOR)
      bar:fillRect(x, y, 2, h, SELECTION_COLOR)
      bar:fillRect(x + w - 2, y, 2, h, SELECTION_COLOR)
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

local function diskSummary(d)
  if d.error then
    return d.name .. " cannot be read: " .. d.error
  end

  local count = #d.partitions
  return d.name .. ": " .. formatSize(d.size) .. ", " .. tableName(d) .. ", "
    .. count .. (count == 1 and " partition" or " partitions")
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

    if partition.system then
      text = text .. ", Aura runs from it (" .. partition.mountPoint .. ")"
    elseif partition.mountPoint then
      text = text .. ", mounted at " .. partition.mountPoint
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

local readSpaces, information

-- Shows that disk (an entry of diskList), the segment at that sector selected.
local function showDisk(d, selectStart, selectKind)
  disk = d
  tree = d and buildTree(d) or {}
  segments = flatten(tree)
  render(indexAt(selectStart, selectKind))
  readSpaces()
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
        renderRows(list.selectedIndex >= 0 and list.selectedIndex + 1 or nil)
        drawBar()

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

-- Reads the disks again; shows the one with that name (else the first), the segment at that sector
-- selected.
local function reload(name, selectStart, selectKind)
  local ok, result = pcall(disks.list)
  diskList = ok and result or {}

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

-- The filesystem drop-down's index of the largest FAT that fits in that many MB.
local function defaultFilesystem(megabytes)
  if megabytes >= MINIMUM_MB.FAT32 then
    return 0
  end

  return megabytes >= MINIMUM_MB.FAT16 and 1 or 2
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

-- A whole number of MB in a text box, nil when it is not one.
local function readMiB(box)
  local text = box.text:match("^%s*(%d+)%s*$")
  return text and tonumber(text)
end

-- Actions -------------------------------------------------------------------------------------------

-- The free space a new partition can take: from its first MB boundary. A logical one goes after the
-- last one, its EBR the sector before it.
local function newRoom(segment)
  local alignment = perMiB()
  local finish = segment.start + segment.sectors

  if segment.logical then
    return segment.start, math.max(0, (finish - segment.start - 1) // alignment)
  end

  local first = alignUp(segment.start, alignment)
  return first, first < finish and (finish - first) // alignment or 0
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
    { before = not segment.logical, size = true, filesystem = true, label = true }, "Create",
    "Up to " .. room .. " MB. " .. FAT_SIZES)
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
  end

  local alignment = perMiB()
  local start = first + before * alignment
  local sectors = size * alignment
  local name = disk.name

  closePanel()

  run("Creating a " .. size .. " MB " .. filesystem .. " partition on " .. name .. "...", function()
    local ok, result = disks.create(name, start, sectors, filesystem, label)
    spaces = {}

    if ok then
      reload(name, result, "partition")
      setStatus("Partition created.", "green")
    else
      reload(name, start)
      showError("Could not create the partition", result)
    end
  end)
end

local function deletePartition()
  local segment = selectedPartition("delete")

  if not segment then
    return
  end

  local partition = segment.partition

  if partition.system then
    showError("Delete " .. partition.name, "Aura runs from this partition (" .. partition.mountPoint .. "): it cannot be deleted while Aura uses it.")
    return
  end

  local name, start = disk.name, segment.start

  ask("Delete", "Delete " .. partition.name .. " (" .. formatSize(bytesOf(segment)) .. ")? Its files will be lost.", function()
    run("Deleting " .. partition.name .. "...", function()
      local ok, why = disks.delete(name, start)
      spaces = {}
      reload(name, start)

      if ok then
        setStatus(partition.name .. " deleted.", "green")
      else
        showError("Could not delete " .. partition.name, why)
      end
    end)
  end)
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

local function resizePartition()
  local segment = selectedPartition("resize or move")

  if not segment then
    return
  end

  local partition = segment.partition

  if disk.table ~= "MBR" and disk.table ~= "GPT" then
    showError("Resize/Move", disk.name .. " has no partition table: the filesystem takes the whole disk.")
    return
  elseif partition.system then
    showError("Resize/Move " .. partition.name, "Aura runs from this partition (" .. partition.mountPoint .. "): it cannot change while Aura uses it.")
    return
  end

  local alignment = perMiB()
  local first, finish = regionOf(segment)
  local alignedFirst = alignUp(first, alignment)
  local room = finish > alignedFirst and (finish - alignedFirst) // alignment or 0
  local before = segment.start > alignedFirst and (segment.start - alignedFirst) // alignment or 0
  local size = segment.sectors // alignment
  local after = finish > segment.start + segment.sectors and (finish - segment.start - segment.sectors) // alignment or 0

  beforeBox.text = tostring(before)
  sizeBox.text = tostring(size)

  local text = "Free space: " .. before .. " MB before it, " .. after .. " MB after; it can take up to " .. room .. " MB."

  if segment.logical then
    text = "It can grow up to " .. (finish - segment.start) // alignment .. " MB. Aura does not move logical partitions."
  end

  if isFat(partition) then
    text = text .. " Its FAT volume keeps its size: the partition can " .. (segment.logical and "grow" or "move and grow")
      .. ", not shrink, and a format uses the new room."
  end

  openPanel("resize", segment, "Resize/Move " .. partition.name,
    { before = not segment.logical, size = true }, "Resize/Move", text)
  panelSegment.original = { before = beforeBox.text, size = sizeBox.text, alignedFirst = alignedFirst, finish = finish }
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
  end

  local name, oldStart = disk.name, segment.start
  local moving = start ~= segment.start

  local function apply()
    run((moving and "Moving " or "Resizing ") .. partition.name .. (moving and ": its sectors are copied, this can take a while..." or "..."), function()
      local ok, why = disks.resize(name, oldStart, start, sectors)
      spaces = {}
      reload(name, ok and start or oldStart, "partition")

      if ok then
        setStatus(partition.name .. (moving and " moved." or " resized."), "green")
      else
        showError("Could not resize/move " .. partition.name, why)
      end
    end)
  end

  closePanel()

  if moving then
    ask("Move", "Move " .. partition.name .. "? Keep the computer on until it is done.", apply)
  else
    apply()
  end
end

local function formatPartition()
  local segment = selectedPartition("format")

  if not segment then
    return
  end

  local partition = segment.partition

  if partition.system then
    showError("Format " .. partition.name, "Aura runs from this partition (" .. partition.mountPoint .. "): it cannot be formatted while Aura uses it.")
    return
  end

  filesystemBox.selectedIndex = defaultFilesystem(bytesOf(segment) // MIB)
  labelBox.text = partition.label

  openPanel("format", segment, "Format " .. partition.name, { filesystem = true, label = true }, "Format",
    "Everything on " .. partition.name .. " will be lost. " .. FAT_SIZES)
end

local function formatOk()
  local segment = panelSegment
  local partition = segment.partition
  local filesystem = FILESYSTEMS[filesystemBox.selectedIndex + 1] or "FAT32"
  local label = labelBox.text
  local name, start = disk.name, segment.start
  local problem = sizeProblem(filesystem, bytesOf(segment) // MIB)

  if problem then
    panelProblem(problem)
    return
  end

  closePanel()

  ask("Format", "Format " .. partition.name .. " as " .. filesystem .. "? Its files will be lost.", function()
    run("Formatting " .. partition.name .. " as " .. filesystem .. "...", function()
      local ok, why = disks.format(name, start, filesystem, label)
      spaces = {}
      reload(name, start, "partition")

      if ok then
        setStatus(partition.name .. " formatted as " .. filesystem .. ".", "green")
      else
        showError("Could not format " .. partition.name, why)
      end
    end)
  end)
end

local function labelPartition()
  local segment = selectedPartition("label")

  if not segment then
    return
  end

  local partition = segment.partition

  if not isFat(partition) then
    showError("Label " .. partition.name, "Aura labels FAT volumes only: this one is " .. partition.filesystem .. ".")
    return
  elseif partition.system then
    showError("Label " .. partition.name, "Aura runs from this partition (" .. partition.mountPoint .. "): its label cannot change while Aura uses it.")
    return
  end

  labelBox.text = partition.label
  openPanel("label", segment, "Label " .. partition.name, { label = true }, "Apply",
    "Up to 11 letters, digits, spaces, - and _: FAT writes them in capitals. Empty for no label.")
end

local function labelOk()
  local segment = panelSegment
  local partition = segment.partition
  local label = labelBox.text:match("^%s*(.-)%s*$")
  local name, start = disk.name, segment.start

  if #label > 11 or label:find("[^%w%s_%-]") then
    panelProblem("A FAT label has up to 11 letters, digits, spaces, - and _.")
    return
  end

  closePanel()

  run("Labeling " .. partition.name .. "...", function()
    local ok, why = disks.setLabel(name, start, label)
    spaces = {}
    reload(name, start, "partition")

    if ok then
      setStatus(partition.name .. (label == "" and " has no label now." or " is labeled " .. label:upper() .. "."), "green")
    else
      showError("Could not label " .. partition.name, why)
    end
  end)
end

local function mountPartition()
  local segment = selectedPartition("mount")

  if not segment then
    return
  end

  local partition = segment.partition
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

  openPanel("table", disk, "New partition table on " .. disk.name, { table = true }, "Create",
    "Every partition on " .. disk.name .. " and all their files will be lost. MBR holds 4 primary partitions of up to 2 TB, GPT 128 of any size.")
end

local function tableOk()
  local d = panelSegment
  local tableType = tableBox.selectedIndex == 1 and "GPT" or "MBR"
  local name = d.name

  closePanel()

  ask("Partition table", "Write a new " .. tableType .. " on " .. name .. "? All its partitions will be lost.", function()
    run("Writing a " .. tableType .. " on " .. name .. "...", function()
      local ok, why = disks.createTable(name, tableType)
      spaces = {}
      reload(name)

      if ok then
        setStatus(name .. " has a new, empty " .. tableType .. ".", "green")
      else
        showError("Could not write the partition table", why)
      end
    end)
  end)
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

    local partitionType = typeOf(partition)

    if partitionType then
      add("Type", partitionType)
    end

    local flags = flagsOf(partition)
    add("Flags", flags ~= "" and flags or "none")

    if partition.logical then
      lines[#lines + 1] = "A logical partition, in the extended one."
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

local function select()
  drawBar()
  showStatus()
  showMountButton()

  -- The panel tells about the selection, or asks about the one it was opened for.
  if panelAction == "information" or panelAction == "error" then
    information()
  end
end

-- A click on the bar selects the innermost box under it.
local function barClick()
  local x, y = bar.clickX, bar.clickY
  local found

  for _, box in ipairs(boxes) do
    if x >= box.x and x < box.x + box.width and y >= box.y and y < box.y + box.height then
      found = box.segment
    end
  end

  for i, segment in ipairs(segments) do
    if segment == found then
      list.selectedIndex = i - 1
      list:focus()
      select()
      return
    end
  end
end

-- Handlers ------------------------------------------------------------------------------------------

local function guard(action)
  return function()
    if not busy then
      action()
    end
  end
end

app:on("new", guard(newPartition))
app:on("delete", guard(deletePartition))
app:on("resize", guard(resizePartition))
app:on("format", guard(formatPartition))
app:on("label", guard(labelPartition))
app:on("mount", guard(mountPartition))
app:on("unmount", guard(unmountPartition))
app:on("information", guard(information))
app:on("table", guard(newTable))
app:on("refresh", guard(refresh))
app:on("ok", guard(ok))
app:on("cancel", cancel)
app:on("confirmOk", confirmOk)
app:on("closeConfirm", closeConfirm)
app:on("select", select)
app:on("barClick", barClick)

app:on("disk", function()
  local d = diskList[diskBox.selectedIndex + 1]

  if d and d ~= disk then
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
  elseif busy then
    return
  elseif name == "enter" then
    information()
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
app:onResize(drawBar)

-- Something else (vol, a USB stick) may have changed the disks: read them again each time the window
-- comes to the front.
local wasFocused = true

app:every(500, function()
  local focused = app.focused

  if focused and not wasFocused and not busy and not confirm.visible then
    local segment = selectedSegment()
    reload(disk and disk.name, segment and segment.start, segment and segment.kind)
  end

  wasFocused = focused
end)

header.text = headerText()
headerPlain.text = header.text
reload(...)
list:focus()
