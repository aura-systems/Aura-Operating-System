-- The Processes tab: the apps, then the desktop's processes and the system's time, each with its share
-- of the CPU. End task closes an app, once confirmed; Switch to (or Enter, or a double click) brings it
-- to the front; New task opens an app.

local ui = require "ui"
local format = require "format"
local status = require "status"

local processes = aura.processes
local packages = aura.packages
local desktop = aura.desktop

local COLUMNS = {
  { title = "Name", width = 36 },
  { title = "PID", width = 5, right = true },
  { title = "Status", width = 10 },
  { title = "CPU", width = 6, right = true },
  { title = "CPU time", width = 10, right = true },
}

-- The desktop's processes, by the kernel's name: what the list calls them, and their icon.
local DESKTOP = {
  Explorer = { name = "Desktop (Explorer)", icon = "16-desktop.bmp" },
  MouseManager = { name = "Mouse (MouseManager)", icon = "16-mouse.bmp" },
  KeyboardManager = { name = "Keyboard (KeyboardManager)", icon = "16-keyboard.bmp" },
}

local APP_ICON = "16-program.bmp"
local DESKTOP_ICON = "16-settings.bmp"
local SYSTEM_ICON = "16-computer.bmp"

-- The rows shown, in their order: { key = , text = , icon = , process = , name = }. key finds the row
-- again in the next sample; process is nil for a heading and for the system's row.
local rows = {}

-- The last sample shown.
local shown

-- The apps New task opens, in the order of its list: aura.packages.list() entries.
local newTaskApps = {}

-- The process End task asks about.
local confirmed

local function nameOf(process)
  local known = not process.app and DESKTOP[process.name]
  return known and known.name or process.name
end

local function iconOf(process)
  if process.app then
    return process.icon or APP_ICON
  end

  local known = DESKTOP[process.name]
  return known and known.icon or DESKTOP_ICON
end

local function statusOf(process)
  if process.running then
    return "Running"
  end

  -- An app's process stops while its window is minimized.
  return process.app and "Minimized" or "Stopped"
end

local function processRow(process)
  local name = nameOf(process)

  return {
    key = "process " .. process.id,
    process = process,
    name = name,
    icon = iconOf(process),
    text = format.columns(COLUMNS, { name, tostring(process.id), statusOf(process),
      format.percent(process.cpu), format.duration(process.cpuTime) }),
  }
end

local function heading(key, title, count)
  return { key = key, text = title .. " (" .. count .. ")" }
end

-- The list from a sample, without the process of id ended (one End task just closed). The selected
-- row and the view stay.
local function update(sample, ended)
  shown = sample

  local list = ui.processList
  local selected = rows[list.selectedIndex + 1]
  local selectedKey = selected and selected.key
  local top = list.top

  local apps, others = {}, {}

  for _, process in ipairs(sample.processes) do
    if process.id ~= ended then
      local group = process.app and apps or others
      group[#group + 1] = processRow(process)
    end
  end

  rows = { heading("apps", "Apps", #apps) }
  table.move(apps, 1, #apps, #rows + 1, rows)
  rows[#rows + 1] = heading("background", "Desktop and system", #others + 1)
  table.move(others, 1, #others, #rows + 1, rows)
  rows[#rows + 1] = {
    key = "system",
    name = "System",
    icon = SYSTEM_ICON,
    text = format.columns(COLUMNS, { "System (screen, memory)", "", "Running",
      format.percent(sample.system), format.duration(sample.systemTime) }),
  }

  local items = {}

  for i, row in ipairs(rows) do
    items[i] = row.icon and { text = row.text, icon = row.icon } or row.text
  end

  list.items = items

  for i, row in ipairs(rows) do
    if row.key == selectedKey then
      list.selectedIndex = i - 1
    end
  end

  list.top = top
end

-- The apps New task offers: those of the start menu, by name. The one picked stays picked.
local function fillNewTask()
  local picked = newTaskApps[ui.newTask.selectedIndex + 1]
  local apps = {}

  for _, package in ipairs(packages.list()) do
    if package.app and package.menu then
      apps[#apps + 1] = package
    end
  end

  table.sort(apps, function(a, b) return a.displayName:lower() < b.displayName:lower() end)

  local names = {}
  local index = 0

  for i, package in ipairs(apps) do
    names[i] = package.displayName

    if picked and package.name == picked.name then
      index = i - 1
    end
  end

  newTaskApps = apps
  ui.newTask.items = names
  ui.newTask.selectedIndex = #apps > 0 and index or -1
end

local function show(sample)
  ui.processesHeader.text = format.header(COLUMNS)
  fillNewTask()

  if sample then
    update(sample)
  end
end

local function selectedRow()
  return rows[ui.processList.selectedIndex + 1]
end

local function endTask()
  local row = selectedRow()
  local process = row and row.process

  if not process then
    status.tell("Select the app to end in the list.")
  elseif not process.app then
    status.tell(row.name .. " is part of the desktop: it runs as long as Aura.", "red")
  else
    confirmed = process
    ui.confirm.title = "End task"
    ui.confirm.message = "End " .. process.name .. "? What it has not saved is lost."
    ui.confirm.visible = true
  end
end

local function confirmOk()
  ui.confirm.visible = false
  local process = confirmed
  confirmed = nil

  if not process then
    return
  end

  local ok, why = processes.close(process.id)

  if not ok then
    status.tell(why, "red")
    return
  end

  status.tell(process.name .. " ended.", "green")
  update(shown, process.id)
end

local function closeConfirm()
  ui.confirm.visible = false
  confirmed = nil
end

local function switchTo()
  local row = selectedRow()
  local process = row and row.process

  if not process then
    status.tell("Select the app to switch to in the list.")
  elseif not process.app then
    status.tell(row.name .. " has no window.", "red")
  else
    local ok, why = processes.switchTo(process.id)

    if not ok then
      status.tell(why, "red")
    end
  end
end

local function newTask()
  local package = newTaskApps[ui.newTask.selectedIndex + 1]

  if not package then
    status.tell("Pick the app to open in the list on the left.")
    return
  end

  local ok, why = desktop.start(package.name)

  if ok then
    status.tell(package.displayName .. " opened.", "green")
  else
    status.tell(why, "red")
  end
end

return {
  show = show,
  update = update,
  endTask = endTask,
  confirmOk = confirmOk,
  closeConfirm = closeConfirm,
  switchTo = switchTo,
  newTask = newTask,
}
