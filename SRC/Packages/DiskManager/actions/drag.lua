-- Dragging on the bar: a press selects the box under it; on a partition's side it resizes the partition
-- from that side, on its middle it moves it. The partition follows the mouse in whole MB, and the button
-- released adds the resize to the operations. Over the bar, the cursor says what a press there does.

local ui = require "ui"
local state = require "state"
local format = require "format"
local segments = require "model.segments"
local preview = require "model.preview"
local bar = require "view.bar"
local status = require "view.status"
local panel = require "view.panel"
local window = require "view.window"
local resize = require "actions.resize"

local canvas = ui.bar

-- How near a box's side a press resizes it from that side, and how far the mouse goes before a drag
-- changes anything, in pixels.
local DRAG = { edge = 6, deadZone = 3 }

-- The cursor over the bar for what a press there does.
local CURSORS = { left = "resizeHorizontal", right = "resizeHorizontal", move = "grab" }

-- What a press at x on that box does: resize the partition from its "left" or "right" side, "move" it,
-- or nothing (nil).
local function modeAt(box, x)
  local segment = box.segment

  if segment.kind ~= "partition" or resize.problem(segment.partition) then
    return nil
  end

  local fromLeft, fromRight = x - box.x, box.x + box.width - 1 - x

  if fromRight < DRAG.edge and fromRight <= fromLeft then
    return "right"
  elseif segment.logical then
    -- Aura moves no logical partition, so their start stays.
    return nil
  elseif fromLeft < DRAG.edge then
    return "left"
  end

  return "move"
end

-- Where the dragged partition goes for the mouse dx pixels from the press: in whole MB, in its free
-- space, its size within its limits. Where it was while the mouse is near the press.
local function dragged(dx)
  local drag = state.drag
  local start, sectors = drag.start, drag.sectors

  if math.abs(dx) < DRAG.deadZone then
    return start, sectors
  end

  local alignment = segments.perMiB(state.disk)
  local delta = math.floor(dx * drag.scale)
  local first = segments.alignUp(drag.first, alignment)
  local ending = start + sectors

  if drag.mode == "move" then
    if drag.finish - sectors < first then
      return start, sectors
    end

    return math.max(first, math.min(segments.snap(start + delta, alignment), drag.finish - sectors)), sectors
  elseif drag.mode == "right" then
    local newEnd = math.min(segments.snap(ending + delta, alignment), drag.finish, start + drag.maximum)
    newEnd = math.max(newEnd, start + drag.minimum)
    return start, newEnd - start
  end

  local newStart = math.max(segments.snap(start + delta, alignment), first, ending - drag.maximum)
  newStart = math.min(newStart, ending - drag.minimum)
  return newStart, ending - newStart
end

-- The status line while dragging: what the partition becomes.
local function dragText()
  local drag = state.drag
  local size = function(sectors) return format.size(sectors * state.disk.sectorSize) end
  local moved = drag.newStart - drag.start

  if drag.mode == "move" then
    return "Move " .. drag.name .. ": " .. (moved == 0 and "where it is" or size(math.abs(moved)) .. (moved > 0 and " to the right" or " to the left"))
  end

  return "Resize " .. drag.name .. ": " .. size(drag.newSectors) .. ", was " .. size(drag.sectors)
    .. ". Release the button to add it to the operations."
end

-- The bar with the dragged partition where it goes now.
local function drawDrag()
  local drag = state.drag
  local d = preview.copyDisk(state.disk)
  local partition = preview.partitionById(d, drag.id)
  partition.start, partition.sectors = drag.newStart, drag.newSectors
  partition.size = drag.newSectors * d.sectorSize

  state.tree = segments.build(d)
  state.segments = segments.flatten(state.tree)
  bar.draw()
  status.set(dragText())
end

-- A press on the bar selects the innermost box under it, and starts dragging the partition when that
-- can change.
local function press()
  state.drag = nil

  if state.busy or state.applying then
    return
  end

  local x, y = canvas.clickX, canvas.clickY
  local box = bar.boxAt(x, y)

  if not box then
    return
  end

  for i, segment in ipairs(state.segments) do
    if segment == box.segment then
      ui.list.selectedIndex = i - 1
      ui.list:focus()
      window.selectionChanged()
    end
  end

  local mode = modeAt(box, x)

  if not mode then
    return
  end

  -- The panel's form is about the partition as it was.
  if state.panelAction and not panel.tells() then
    panel.close()
  end

  local segment = box.segment
  local partition = segment.partition
  local first, finish = segments.regionOf(state.disk, state.tree, segment)
  local minimum, maximum = resize.limits(partition)

  state.drag = {
    id = partition.id, name = partition.name, mode = mode, x = x,
    start = segment.start, sectors = segment.sectors, first = first, finish = finish,
    minimum = minimum, maximum = maximum, scale = box.scale,
    newStart = segment.start, newSectors = segment.sectors,
  }
end

-- The mouse over the bar: the cursor says what a press there does. While dragging, the partition follows.
local function move()
  local drag = state.drag

  if not drag then
    if not canvas.pressed then
      local x = canvas.mouseX
      local box = not state.busy and not state.applying and bar.boxAt(x, canvas.mouseY)
      local cursor = box and CURSORS[modeAt(box, x)] or "normal"

      if canvas.cursor ~= cursor then
        canvas.cursor = cursor
      end
    end

    return
  end

  local newStart, newSectors = dragged(canvas.mouseX - drag.x)

  if newStart ~= drag.newStart or newSectors ~= drag.newSectors then
    drag.newStart, drag.newSectors = newStart, newSectors
    drawDrag()
  end
end

-- The button released: the resize or move joins the operations.
local function release()
  local done = state.drag
  state.drag = nil

  if not done then
    return
  end

  if done.newStart == done.start and done.newSectors == done.sectors then
    -- A click: the bar as it was.
    window.showDisk(state.realDisk, done.start, "partition")
    return
  end

  local partition = preview.partitionById(state.disk, done.id)
  local problem = resize.queue(partition, done.newStart, done.newSectors)

  if problem then
    window.showDisk(state.realDisk, done.start, "partition")
    status.set(problem, "red")
  end
end

return {
  press = press,
  move = move,
  release = release,
}
