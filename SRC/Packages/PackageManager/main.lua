-- Package Manager: the packages Aura has and those of the online repository. Installs, updates and
-- removes them, and changes the repository. A download runs on the UI thread: the status line tells
-- what is going on first (app:after), then the desktop waits for it.

local app = aura.app
local packages, system = aura.packages, aura.system

local repository = app:find("repository")
local status = app:find("status")
local list = app:find("packages")
local name = app:find("name")
local description = app:find("description")
local state = app:find("state")
local install = app:find("install")
local remove = app:find("remove")
local confirm = app:find("confirm")
local errorDialog = app:find("error")

-- The list's columns, in characters.
local NAME_WIDTH = 18
local VERSION_WIDTH = 9

-- One per row of the list, by name: { name = , installed = (an entry of packages.list()),
-- available = (an entry of packages.available()) }, either of them nil.
local rows = {}

-- A download waits for the window to be drawn: the buttons do nothing until it ran.
local busy = false

-- The package the remove dialog asks about.
local removing

local function setStatus(text, color)
  status.text = text
  status.color = color or "black"
end

local function showError(title, message)
  errorDialog.title = title
  errorDialog.message = message or "Unknown error."
  errorDialog.visible = true
end

-- The words of the text in lines of at most that many characters; a longer word is cut.
local function wrap(text, columns)
  local lines, line = {}, ""

  for word in text:gmatch("%S+") do
    if line == "" then
      line = word
    elseif #line + 1 + #word <= columns then
      line = line .. " " .. word
    else
      lines[#lines + 1] = line
      line = word
    end

    while #line > columns do
      lines[#lines + 1] = line:sub(1, columns)
      line = line:sub(columns + 1)
    end
  end

  if line ~= "" then
    lines[#lines + 1] = line
  end

  return table.concat(lines, "\n")
end

-- The installed package, or else the repository's.
local function packageOf(row)
  return row.installed or row.available
end

-- True when the repository has a newer version than the one Aura has.
local function isOutdated(row)
  return row.installed ~= nil and row.available ~= nil
    and system.compareVersions(row.installed.version, row.available.version) < 0
end

-- The installed packages and the repository's, together by name and sorted.
local function collect()
  local byName, result = {}, {}

  local function rowOf(packageName)
    local key = packageName:lower()
    local row = byName[key]

    if not row then
      row = { name = packageName }
      byName[key] = row
      result[#result + 1] = row
    end

    return row
  end

  for _, package in ipairs(packages.list()) do
    rowOf(package.name).installed = package
  end

  for _, package in ipairs(packages.available()) do
    rowOf(package.name).available = package
  end

  table.sort(result, function(a, b) return a.name:lower() < b.name:lower() end)
  return result
end

local function rowText(row)
  local package = packageOf(row)
  local what

  if isOutdated(row) then
    what = "Update: " .. row.available.version
  elseif row.installed and row.installed.builtIn then
    what = "Built in"
  elseif row.installed then
    what = "Installed"
  else
    what = "Not installed"
  end

  return string.format("%-" .. NAME_WIDTH .. "s %-" .. VERSION_WIDTH .. "s %s",
    package.displayName:sub(1, NAME_WIDTH), package.version:sub(1, VERSION_WIDTH), what)
end

local function selectedRow()
  return rows[list.selectedIndex + 1]
end

local function selectedName()
  local row = selectedRow()
  return row and row.name
end

-- The selected package under the list, and the buttons that apply to it.
local function showDetails()
  local row = selectedRow()

  if not row then
    name.text = ""
    description.text = ""
    state.text = #rows > 0 and "Select a package." or ""
    install.visible = false
    remove.visible = false
    return
  end

  local package = packageOf(row)
  local installed, available = row.installed, row.available

  name.text = package.displayName .. (package.author ~= "" and ", by " .. package.author or "")
  description.text = wrap(package.description, list.width // 8 - 1)

  if not installed then
    state.text = "Not installed. The repository has version " .. available.version .. "."
  elseif isOutdated(row) then
    state.text = "Version " .. installed.version .. (installed.builtIn and " is built in" or " is installed")
      .. ", the repository has " .. available.version .. "."
  elseif installed.builtIn then
    state.text = "Version " .. installed.version .. " is built into Aura."
  else
    state.text = "Version " .. installed.version .. " is installed."
  end

  install.text = installed and "Update" or "Install"
  install.visible = not installed or isOutdated(row)
  remove.visible = installed ~= nil and not installed.builtIn
end

-- Lists the packages again, the one with that name (any case) selected if it is still there.
local function fill(selected)
  rows = collect()

  local items, index = {}, -1

  for i, row in ipairs(rows) do
    items[i] = rowText(row)

    if selected and row.name:lower() == selected:lower() then
      index = i - 1
    end
  end

  list.items = items
  list.selectedIndex = index
  showDetails()
end

-- Shows the text in the status line, and runs the job once the window shows it.
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

local function availableCount()
  local count = #packages.available()
  return count .. (count == 1 and " package" or " packages") .. " in the repository."
end

local function refresh()
  if not aura.network.isConfigured() then
    setStatus("No network: the repository cannot be read.", "red")
    return
  end

  run("Reading the package list...", function()
    local ok, why = packages.update()

    if ok then
      setStatus(availableCount(), "green")
    else
      setStatus("The package list could not be read.", "red")
      showError("Refresh", why)
    end

    fill(selectedName())
  end)
end

-- Switches to the repository in the text box, and reads its package list.
local function useRepository()
  if busy then
    return
  end

  local ok, why = packages.setRepository(repository.text)

  if not ok then
    showError("Repository", why)
    return
  end

  repository.text = packages.repository

  -- The packages of the previous repository are gone from the list.
  fill(selectedName())
  refresh()
end

app:on("refresh", refresh)

app:on("useRepository", useRepository)

app:on("defaultRepository", function()
  repository.text = packages.defaultRepository
  useRepository()
end)

app:on("select", showDetails)

app:on("install", function()
  local row = selectedRow()

  if not row or not row.available then
    return
  end

  local updating = row.installed ~= nil

  run((updating and "Updating " or "Installing ") .. row.name .. "...", function()
    local ok, why = packages.add(row.name)

    if ok then
      setStatus(row.name .. (updating and " updated" or " installed")
        .. (system.installed and "." or " until the next boot."), "green")
    else
      setStatus(row.name .. (updating and " could not be updated." or " could not be installed."), "red")
      showError(updating and "Update" or "Install", why)
    end

    fill(row.name)
  end)
end)

app:on("remove", function()
  local row = selectedRow()

  if busy or not row or not row.installed or row.installed.builtIn then
    return
  end

  removing = row.name
  confirm.message = "Remove " .. row.name .. "?"
  confirm.visible = true
end)

app:on("confirmRemove", function()
  confirm.visible = false

  if not removing then
    return
  end

  local ok, why = packages.remove(removing)

  if ok then
    setStatus(removing .. " removed.", "green")
  else
    setStatus(removing .. " could not be removed.", "red")
    showError("Remove", why)
  end

  fill(removing)
  removing = nil
end)

app:on("closeConfirm", function()
  confirm.visible = false
end)

app:on("closeError", function()
  errorDialog.visible = false
end)

-- The description is wrapped to the list's width.
app:onResize(showDetails)

repository.text = packages.repository
fill(nil)

-- The list read since the boot is kept until the repository changes: Refresh reads it again.
if #packages.available() > 0 then
  setStatus(availableCount())
else
  refresh()
end
