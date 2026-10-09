-- A history drawn on a Canvas, as bars a sample wide: the newest on the right, the oldest leaving on the
-- left. Several histories stack up from the bottom. A grid line every tenth of the height, and one
-- every 10 samples, which moves left with them.

local GRID = "#004000"

-- Draws the histories (lists of shares from 0 to 1, oldest first, all as long) in their colors.
-- slots is how many samples the width holds; count how many were ever taken, for the grid to move.
local function draw(canvas, histories, colors, slots, count)
  local width, height = canvas.width, canvas.height
  canvas:clear()

  if width <= 0 or height <= 0 then
    return
  end

  local function left(slot)
    -- slot 0 is the right edge, slots the left one.
    return width - (slot * width) // slots
  end

  for line = 1, 9 do
    canvas:fillRect(0, (line * height) // 10, width, 1, GRID)
  end

  -- The line of every 10th sample, where that sample is.
  for slot = (count % 10), slots, 10 do
    canvas:fillRect(left(slot), 0, 1, height, GRID)
  end

  local samples = #histories[1]

  for i = 1, samples do
    -- The newest is in slot 1, at the right edge.
    local slot = samples - i + 1
    local x = left(slot)
    local barWidth = left(slot - 1) - x
    local bottom = height

    for h, history in ipairs(histories) do
      local barHeight = math.floor(history[i] * height + 0.5)

      if barHeight > bottom then
        barHeight = bottom
      end

      if barHeight > 0 then
        canvas:fillRect(x, bottom - barHeight, barWidth, barHeight, colors[h])
        bottom = bottom - barHeight
      end
    end
  end
end

return {
  draw = draw,
}
