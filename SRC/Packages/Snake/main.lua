-- Snake: the arrow keys (or W A S D) steer the snake to the apples, each one makes it longer. Running
-- into a wall or into itself ends the game. P pauses, Enter or Space starts a new game.

local app = aura.app
local board = app:find("board")
local score = app:find("score")

local CELL = 16
local COLUMNS = board.width // CELL
local ROWS = board.height // CELL
local STEP_MS = 120

local BODY = "#3CB043"
local HEAD = "#8BE04E"
local APPLE = "#E0301E"
local TEXT = "white"

local MOVES = {
  up = { x = 0, y = -1 },
  down = { x = 0, y = 1 },
  left = { x = -1, y = 0 },
  right = { x = 1, y = 0 },
}
local OPPOSITE = { up = "down", down = "up", left = "right", right = "left" }
local LETTERS = { w = "up", s = "down", a = "left", d = "right" }

local snake     -- its cells from the head to the tail, { x = , y = }
local occupied  -- index(x, y) -> true for the snake's cells
local heading   -- the move of the last step
local turns     -- the moves asked for, two ahead at most, one taken per step
local apple     -- nil once the snake fills the board
local points
local best = 0
local state     -- "ready" (until the first arrow), "playing", "paused" or "over"

local function index(x, y)
  return y * COLUMNS + x
end

local function placeApple()
  local free = {}

  for y = 0, ROWS - 1 do
    for x = 0, COLUMNS - 1 do
      if not occupied[index(x, y)] then
        free[#free + 1] = { x = x, y = y }
      end
    end
  end

  apple = #free > 0 and free[math.random(#free)] or nil
end

local function showScore()
  score.text = "Score: " .. points .. "    Best: " .. best
end

local function fillCell(cell, color)
  board:fillRect(cell.x * CELL + 1, cell.y * CELL + 1, CELL - 2, CELL - 2, color)
end

-- The system font is 8 pixels a character.
local function centered(text, y)
  board:drawText(text, (board.width - #text * 8) // 2, y, TEXT)
end

local function draw()
  board:clear()

  if apple then
    fillCell(apple, APPLE)
  end

  for i = #snake, 1, -1 do
    fillCell(snake[i], i == 1 and HEAD or BODY)
  end

  local middle = board.height // 2 - 8

  if state == "ready" then
    centered("Press an arrow key to start", middle)
  elseif state == "paused" then
    centered("Paused: P to go on", middle)
  elseif state == "over" then
    centered(apple and "Game over" or "You win!", middle - 10)
    centered("Enter: new game", middle + 10)
  end
end

local function newGame()
  local x, y = COLUMNS // 2, ROWS // 2

  snake = { { x = x, y = y }, { x = x - 1, y = y }, { x = x - 2, y = y } }
  occupied = {}

  for _, cell in ipairs(snake) do
    occupied[index(cell.x, cell.y)] = true
  end

  heading = "right"
  turns = {}
  points = 0
  state = "ready"

  placeApple()
  showScore()
  draw()
end

local function step()
  -- The keys go to the focused window: wait while another one has them.
  if state ~= "playing" or not app.focused then
    return
  end

  if #turns > 0 then
    heading = table.remove(turns, 1)
  end

  local head = snake[1]
  local x, y = head.x + MOVES[heading].x, head.y + MOVES[heading].y
  local eats = apple ~= nil and x == apple.x and y == apple.y

  -- Unless the snake grows, its tail moves on this step: the head may take its cell.
  local tail = snake[#snake]
  local intoTail = not eats and x == tail.x and y == tail.y

  if x < 0 or x >= COLUMNS or y < 0 or y >= ROWS or (occupied[index(x, y)] and not intoTail) then
    state = "over"
    draw()
    return
  end

  if not eats then
    table.remove(snake)
    occupied[index(tail.x, tail.y)] = nil
  end

  table.insert(snake, 1, { x = x, y = y })
  occupied[index(x, y)] = true

  if eats then
    points = points + 1
    best = math.max(best, points)
    showScore()
    placeApple()

    if not apple then
      state = "over"
    end
  end

  draw()
end

app:onKey(function(key)
  local move = key.name or LETTERS[key.char and key.char:lower()]

  if MOVES[move] then
    if state == "ready" then
      state = "playing"
    end

    -- Neither back into the neck nor the same move twice.
    local last = turns[#turns] or heading

    if state == "playing" and #turns < 2 and move ~= last and move ~= OPPOSITE[last] then
      turns[#turns + 1] = move
    end
  elseif key.char == "p" or key.char == "P" then
    if state == "playing" then
      state = "paused"
    elseif state == "paused" then
      state = "playing"
    end

    draw()
  elseif (key.name == "enter" or key.char == " ") and state == "over" then
    newGame()
  end
end)

app:every(STEP_MS, step)

newGame()
app:fit()
