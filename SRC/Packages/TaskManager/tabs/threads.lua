-- The Threads tab: the kernel's threads, with their share of the CPU. The main loop is the thread of
-- the desktop and of every app; the others are the servers Aura starts, and the kernel's own.

local ui = require "ui"
local format = require "format"

local COLUMNS = {
  { title = "TID", width = 5, right = true },
  { title = "Name", width = 26 },
  { title = "State", width = 9 },
  { title = "CPU", width = 6, right = true },
  { title = "CPU time", width = 9, right = true },
  { title = "Stack", width = 8, right = true },
  { title = "Priority", width = 8, right = true },
}

local STATES = {
  created = "Created",
  ready = "Ready",
  running = "Running",
  blocked = "Blocked",
  sleeping = "Sleeping",
}

-- The thread ids of the rows, in their order: the selected one stays selected.
local ids = {}

local function nameOf(thread)
  if thread.main then
    return "Main loop (desktop, apps)"
  end

  -- A thread Aura started names itself (the FTP and HTTP servers).
  return thread.name or (thread.managed and "Thread" or "Kernel thread")
end

local function update(sample)
  local list = ui.threadList
  local selected = ids[list.selectedIndex + 1]
  local top = list.top
  local items = {}

  ids = {}

  for i, thread in ipairs(sample.threads) do
    ids[i] = thread.id
    items[i] = format.columns(COLUMNS, { tostring(thread.id), nameOf(thread), STATES[thread.state] or thread.state,
      format.percent(thread.cpu), format.duration(thread.cpuTime), format.size(thread.stack),
      thread.priority and tostring(thread.priority) or "" })
  end

  list.items = items

  for i, id in ipairs(ids) do
    if id == selected then
      list.selectedIndex = i - 1
    end
  end

  list.top = top
end

local function show(sample)
  ui.threadsHeader.text = format.header(COLUMNS)

  if sample then
    update(sample)
  end
end

return {
  show = show,
  update = update,
}
