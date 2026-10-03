-- Terminal: the Aura shell in a window. The commands themselves are the kernel's (help lists them).

local app = aura.app
local console = app:find("console")
local shell = aura.shell.open(console)

local line = ""     -- the command being typed
local history = {}
local index = 0     -- the history entry Up shows next, from 0

-- The prompt before the command line: level, user@computer> directory~
local function prompt()
  console.foreground = "blue"
  console:write(aura.user.level)
  console.foreground = "yellow"
  console:write(aura.user.name)
  console.foreground = "darkGray"
  console:write("@")
  console.foreground = "blue"
  console:write(aura.system.computerName)
  console.foreground = "gray"
  console:write("> ")
  console.foreground = "darkGray"
  console:write(aura.fs.currentDirectory .. "~ ")
  console.foreground = "white"
end

local function setLine(text)
  line = text
  console.input = text
end

local function run()
  if line == "" then
    console:writeLine()
    console:writeLine()
  else
    local command = line

    -- The typed text leaves the input line and is written as output, before the command's own.
    setLine("")
    console:writeLine(command)

    console.inputHidden = true
    shell.execute(command)
    console.inputHidden = false

    history[#history + 1] = command
    index = #history - 1
  end

  prompt()
end

app:onKey(function(key)
  local scroll = key.ctrl and (key.name == "up" or key.name == "down")

  -- Any other key brings the view back to the line being typed.
  if not scroll then
    console:scrollToEnd()
  end

  if key.name == "enter" then
    run()
  elseif key.name == "backspace" then
    if line ~= "" then
      -- One character, which is one to four bytes of UTF-8.
      setLine(line:sub(1, utf8.offset(line, -1) - 1))
    end
  elseif key.name == "up" then
    if key.ctrl then
      console:scrollUp()
    elseif index >= 0 and index < #history then
      setLine(history[index + 1])
      index = index - 1
    end
  elseif key.name == "down" then
    if key.ctrl then
      console:scrollDown()
    elseif index < #history - 1 then
      index = index + 1
      setLine(history[index + 1])
    end
  elseif key.char then
    setLine(line .. key.char)
  end
end)

-- A new size starts the console over, empty: write the prompt and the command line again.
local width, height = console.width, console.height

app:onResize(function()
  if console.width ~= width or console.height ~= height then
    width, height = console.width, console.height
    prompt()
    console.input = line
  end
end)

prompt()
