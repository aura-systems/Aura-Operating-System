-- The bar: the disk's segments in their place, each a box as wide as its sectors (the narrow ones a bit
-- wider): a border of its filesystem's color around its name and size, its volume's used part in yellow.
-- The extended partition's boxes are inside its own. The selected one is framed, the dragged one while
-- it is.

local ui = require "ui"
local state = require "state"
local format = require "format"
local segments = require "model.segments"
local list = require "view.list"

local canvas = ui.bar

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

-- The other colors: an unknown filesystem's, free space's (its border and inside), the extended
-- partition's, a volume's used part, the selection's frame.
local SHADES = {
  unknown = "#000000",
  unallocated = "#A9A9A9",
  unallocatedInside = "#C8C8C8",
  extended = "#7DFCFE",
  used = "#F8F8BA",
  selection = "#316AC5",
}

-- The space between two boxes, a box's colored border, the narrowest box, in pixels.
local BOX = { gap = 4, border = 3, minWidth = 12 }

local function colorOf(segment)
  if segment.kind == "free" then
    return SHADES.unallocated
  elseif segment.kind == "extended" then
    return SHADES.extended
  end

  return COLORS[segment.partition.filesystem] or SHADES.unknown
end

-- The boxes' widths: as their sectors, each at least the narrowest, all of them the room left by
-- the gaps. Then the sectors a pixel stands for.
local function widths(items, total)
  local count = #items
  local room = math.max(count, total - (count - 1) * BOX.gap)
  local minimum = math.min(BOX.minWidth, room // count)
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
  local columns = (width - 2 * BOX.border - 2) // 8

  if columns < 1 then
    return
  end

  local count = math.min(#lines, (height - 2 * BOX.border) // 16)
  local top = y + (height - count * 16) // 2

  for i = 1, count do
    local line = lines[i]

    if #line > columns then
      line = line:sub(1, columns)
    end

    canvas:drawText(line, x + (width - #line * 8) // 2, top + (i - 1) * 16, "black")
  end
end

local layoutBoxes

-- A box: a border of its filesystem's color, its volume's used part in yellow, its name and size.
local function drawBox(segment, x, y, width, height, scale)
  state.boxes[#state.boxes + 1] = { segment = segment, x = x, y = y, width = width, height = height, scale = scale }
  canvas:fillRect(x, y, width, height, colorOf(segment))

  local innerWidth, innerHeight = width - 2 * BOX.border, height - 2 * BOX.border

  if innerWidth <= 0 or innerHeight <= 0 then
    return
  end

  canvas:fillRect(x + BOX.border, y + BOX.border, innerWidth, innerHeight, segment.kind == "free" and SHADES.unallocatedInside or "white")

  local space = state.spaceOf(segment)

  if space and space.total > 0 then
    local usedWidth = math.min(innerWidth, innerWidth * (space.total - space.free) // space.total)
    canvas:fillRect(x + BOX.border, y + BOX.border, usedWidth, innerHeight, SHADES.used)
  end

  if segment.kind == "extended" then
    local inset = BOX.border + 2
    layoutBoxes(segment.children, x + inset, y + inset, width - 2 * inset, height - 2 * inset)
  else
    drawLines({ segments.nameOf(segment), format.size(segments.bytesOf(state.disk, segment)) }, x, y, width, height)
  end
end

layoutBoxes = function(items, x, y, width, height)
  if #items == 0 or width <= 0 or height <= 0 then
    return
  end

  local sizes, scale = widths(items, width)

  for i, item in ipairs(items) do
    drawBox(item, x, y, sizes[i], height, scale)
    x = x + sizes[i] + BOX.gap
  end
end

local function frame(box)
  local x, y, w, h = box.x - 2, box.y - 2, box.width + 4, box.height + 4
  canvas:fillRect(x, y, w, 2, SHADES.selection)
  canvas:fillRect(x, y + h - 2, w, 2, SHADES.selection)
  canvas:fillRect(x, y, 2, h, SHADES.selection)
  canvas:fillRect(x + w - 2, y, 2, h, SHADES.selection)
end

-- The disk's segments (state.tree) on the bar, the selected one framed (the one dragged, while it is).
local function draw()
  canvas:clear()
  state.boxes = {}

  if not state.disk then
    canvas:drawText("No disk", 8, (canvas.height - 16) // 2, "gray")
    return
  end

  -- Room around the boxes for the frame.
  layoutBoxes(state.tree, 3, 3, canvas.width - 6, canvas.height - 6)

  local drag = state.drag
  local selected = not drag and list.selected()

  for _, box in ipairs(state.boxes) do
    local partition = box.segment.partition

    if box.segment == selected or (drag and partition and partition.id == drag.id) then
      frame(box)
    end
  end
end

-- The innermost box at that point of the bar.
local function boxAt(x, y)
  local found

  for _, box in ipairs(state.boxes) do
    if x >= box.x and x < box.x + box.width and y >= box.y and y < box.y + box.height then
      found = box
    end
  end

  return found
end

return {
  draw = draw,
  boxAt = boxAt,
}
