-- The lists: the disk's segments under the bar, in columns with their titles over them; and the pending
-- operations under it, shown while there are some.

local ui = require "ui"
local state = require "state"
local format = require "format"
local filesystems = require "model.filesystems"
local segments = require "model.segments"

-- The columns: a title and a width in characters, numbers on the right.
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

-- A row: the values in their columns, cut with "~" when too long.
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

-- The column titles, over the texts of the rows with icons and of those without.
local function showHeader()
  local titles = {}

  for i, column in ipairs(COLUMNS) do
    titles[i] = column.title
  end

  ui.header.text = columns(titles)
  ui.headerPlain.text = ui.header.text
end

local function rowText(segment)
  local values = { segments.nameOf(segment), segments.filesystemOf(segment), "", "",
    format.size(segments.bytesOf(state.disk, segment)), "", "", "" }

  if segment.logical then
    values[1] = "  " .. values[1]
  end

  local partition = segment.partition

  if partition then
    values[3] = partition.mountPoint and partition.mountPoint:gsub("/$", "") or ""
    values[4] = partition.label
    values[8] = filesystems.flagsOf(partition)

    local space = state.spaceOf(segment)

    if space then
      values[6] = format.size(space.total - space.free)
      values[7] = format.size(space.free)
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

-- The selected segment, nil for none.
local function selected()
  return state.segments[ui.list.selectedIndex + 1]
end

-- The disk's segments in the list, the one at that index (from 1) selected.
local function render(index)
  local items, icons = {}, false

  for i, segment in ipairs(state.segments) do
    local icon = iconOf(segment)
    items[i] = { text = rowText(segment), icon = icon }
    icons = icons or icon ~= nil
  end

  ui.list.items = items
  ui.list.selectedIndex = index and index - 1 or -1

  ui.header.visible = icons
  ui.headerPlain.visible = not icons
end

-- The index (from 1) of the segment of that kind starting at that sector, else of the one holding it.
local function indexAt(start, kind)
  if not start then
    return nil
  end

  for i, segment in ipairs(state.segments) do
    if segment.start == start and (not kind or segment.kind == kind) then
      return i
    end
  end

  for i, segment in ipairs(state.segments) do
    if segment.kind ~= "extended" and start >= segment.start and start < segment.start + segment.sectors then
      return i
    end
  end

  return nil
end

-- The index (from 1) of the partition with that id.
local function indexOfId(id)
  for i, segment in ipairs(state.segments) do
    if segment.partition and segment.partition.id == id then
      return i
    end
  end

  return nil
end

-- The operations under the list: shown while there are some.
local function renderOperations()
  local items = {}

  for i, op in ipairs(state.pending) do
    items[i] = { text = op.text, icon = OPERATION_ICONS[op.kind] }
  end

  ui.operationList.items = items
  ui.operationList.selectedIndex = -1
  ui.operationsTitle.text = format.plural(#state.pending, "operation") .. " pending: Apply does them, in this order; Undo takes the last change back."
  ui.operations.visible = #state.pending > 0
end

return {
  showHeader = showHeader,
  selected = selected,
  render = render,
  indexAt = indexAt,
  indexOfId = indexOfId,
  renderOperations = renderOperations,
}
