-- File Explorer: the folders and files of the volumes. Opens them (a folder here, a file in its app),
-- creates, copies, moves, renames and deletes them. "/" is the computer: its entries are the volumes.
-- The first argument, a folder (or a file, selected in its folder), is where it opens.

local app = aura.app
local fs, desktop = aura.fs, aura.desktop

local pathBox = app:find("path")
local places = app:find("places")
local files = app:find("files")
local status = app:find("status")
local nameBar = app:find("nameBar")
local nameLabel = app:find("nameLabel")
local nameBox = app:find("name")
local confirm = app:find("confirm")
local errorDialog = app:find("error")

local COMPUTER = "/"

-- The list's columns, in characters, after the name.
local SIZE_WIDTH = 10
local TYPE_WIDTH = 12

-- The folder shown, ending with "/".
local current

-- The folders shown before it, for Back.
local history = {}

-- The folder's entries, as shown: { name = , directory = , size = , path = , volume = }. path has no
-- "/" at its end; volume is the aura.fs.volumes() entry of a volume (at the computer only).
local entries = {}

-- The places on the left: { text = , icon = , path = }.
local placeList = {}

-- The path Cut put in the clipboard: pasting it moves it.
local cutPath

-- What the name bar asks for: "folder", "file" or "rename", nil while hidden.
local naming
local renaming

-- The entry the delete dialog asks about.
local deleting

-- A copy or a delete waits for the window to be drawn: the buttons do nothing until it ran.
local busy = false

-- Each volume's { free = , total = } bytes by its path, read once: reading it goes through the whole FAT.
local spaces = {}

local function setStatus(text, color)
  status.text = text
  status.color = color or "black"
end

local function showError(title, message)
  errorDialog.title = title
  errorDialog.message = message or "Unknown error."
  errorDialog.visible = true
end

local function exists(path)
  return fs.fileExists(path) or fs.directoryExists(path)
end

local function asDirectory(path)
  return path:sub(-1) == "/" and path or path .. "/"
end

-- "/0/Users/" -> "/0/", "/0/" -> "/", "/" -> nil.
local function parentOf(path)
  if path == COMPUTER then
    return nil
  end

  return (path:gsub("/+$", "")):match("^(.*/)")
end

-- "/0/Users/" and "/0/Users" -> "Users".
local function nameOf(path)
  return (path:gsub("/+$", "")):match("[^/]*$")
end

-- "/0/Users/" -> "/0/", nil for the computer.
local function volumeOf(path)
  return path:match("^/[^/]+/")
end

local function formatSize(bytes)
  if bytes < 1024 then
    return bytes .. (bytes == 1 and " byte" or " bytes")
  end

  local unit, divisor = "KB", 1024

  if bytes >= 1024 * 1024 * 1024 then
    unit, divisor = "GB", 1024 * 1024 * 1024
  elseif bytes >= 1024 * 1024 then
    unit, divisor = "MB", 1024 * 1024
  end

  -- In integers: one decimal under 10.
  local tenths = bytes * 10 // divisor

  if tenths < 100 then
    return (tenths // 10) .. "." .. (tenths % 10) .. " " .. unit
  end

  return (tenths // 10) .. " " .. unit
end

local function volumeName(volume)
  local mountPoint = volume.path:gsub("/$", "")
  return volume.label ~= "" and volume.label .. " (" .. mountPoint .. ")" or "Volume " .. mountPoint
end

-- A file's type and icon by its extension.
local KINDS = {
  bmp = { "Image", "16-image.bmp" },
  lua = { "Lua script", "16-script.bmp" },
  pkg = { "Package", "16-package.bmp" },
  zip = { "Zip archive", "16-package.bmp" },
}

for _, extension in ipairs({ "txt", "md", "log", "ini", "cfg", "conf", "xml", "json", "csv" }) do
  KINDS[extension] = { "Text", "16-file.bmp" }
end

local function extensionOf(name)
  local extension = name:match("%.([^.]+)$")
  return extension and extension:lower()
end

local function typeOf(entry)
  if entry.volume then
    return entry.volume.filesystem .. " volume"
  elseif entry.directory then
    return "Folder"
  end

  local extension = extensionOf(entry.name)
  local kind = extension and KINDS[extension]

  if kind then
    return kind[1]
  end

  return extension and extension:upper() .. " file" or "File"
end

local function iconOf(entry)
  if entry.volume then
    return "16-drive.bmp"
  elseif entry.directory then
    return "16-folder.bmp"
  end

  local extension = extensionOf(entry.name)
  local kind = extension and KINDS[extension]
  return kind and kind[2] or "16-file.bmp"
end

-- The entry's row: its name, size and type in columns as wide as the list.
local function rowText(entry)
  -- The list's border and padding, an icon, and a scroll bar.
  local columns = math.max(36, (files.width - 41) // 8)
  local nameWidth = columns - SIZE_WIDTH - TYPE_WIDTH - 3
  local name = entry.volume and volumeName(entry.volume) or entry.name

  if #name > nameWidth then
    name = name:sub(1, nameWidth - 1) .. "~"
  end

  local size = entry.directory and "" or formatSize(entry.size)

  -- By hand: string.format takes widths up to 99, and a wide window has longer names.
  return name .. string.rep(" ", nameWidth - #name + 1) .. string.rep(" ", SIZE_WIDTH - #size) .. size
    .. "  " .. typeOf(entry):sub(1, TYPE_WIDTH)
end

-- The entries of a folder, its folders first, each part by name; or nil and why.
local function load(path)
  local result = {}

  if path == COMPUTER then
    for _, volume in ipairs(fs.volumes()) do
      result[#result + 1] = { name = nameOf(volume.path), directory = true, size = 0,
        path = (volume.path:gsub("/$", "")), volume = volume }
    end

    return result
  end

  local list, why = fs.list(path)

  if not list then
    return nil, why
  end

  for _, entry in ipairs(list) do
    -- Hidden, as dir hides them.
    if entry.name:sub(1, 1) ~= "." then
      entry.path = path .. entry.name
      result[#result + 1] = entry
    end
  end

  table.sort(result, function(a, b)
    if a.directory ~= b.directory then
      return a.directory
    end

    return a.name:lower() < b.name:lower()
  end)

  return result
end

local function selectedEntry()
  return entries[files.selectedIndex + 1]
end

-- The status line: the selected entry, or what the folder holds, and its volume's free space.
local function showStatus()
  local entry = selectedEntry()
  local text

  if entry then
    text = (entry.volume and volumeName(entry.volume) or entry.name) .. ": " .. typeOf(entry)
      .. (entry.directory and "" or ", " .. formatSize(entry.size))
  elseif current == COMPUTER and #entries == 0 then
    text = "No volume: Aura runs in live mode."
  else
    text = #entries .. (#entries == 1 and " item" or " items")
  end

  local space = current ~= COMPUTER and spaces[volumeOf(current)]

  if space then
    text = text .. "    " .. formatSize(space.free) .. " free of " .. formatSize(space.total)
  end

  setStatus(text)
end

-- Reads the free space of the folder's volume once the window shows the folder.
local function readSpace()
  local volume = current ~= COMPUTER and volumeOf(current)

  if not volume or spaces[volume] then
    return
  end

  app:after(0, function()
    if spaces[volume] then
      return
    end

    local free, total = fs.space(volume)

    if free then
      spaces[volume] = { free = free, total = total }

      if volumeOf(current) == volume then
        showStatus()
      end
    end
  end)
end

-- Lists the entries, the one with that name (any case) selected, else the one at that index (from 1).
local function render(selectName, selectIndex)
  local items, index = {}, -1

  for i, entry in ipairs(entries) do
    items[i] = { text = rowText(entry), icon = iconOf(entry) }

    if selectName and entry.name:lower() == selectName:lower() then
      index = i - 1
    end
  end

  if index < 0 and selectIndex and #entries > 0 then
    index = math.min(selectIndex, #entries) - 1
  end

  files.items = items
  files.selectedIndex = index
end

-- The place holding the folder: the longest path it starts with (the computer only for itself).
local function selectPlace()
  local best, bestLength = -1, -1

  for i, place in ipairs(placeList) do
    local matches = place.path == current or (place.path ~= COMPUTER and current:sub(1, #place.path) == place.path)

    if matches and #place.path > bestLength then
      best, bestLength = i - 1, #place.path
    end
  end

  places.selectedIndex = best
end

-- The computer, the user's folder and the volumes.
local function fillPlaces()
  placeList = { { text = "Computer", icon = "16-computer.bmp", path = COMPUTER } }

  local home = aura.user.directory

  if home and fs.directoryExists(home) then
    placeList[#placeList + 1] = { text = "Home", icon = "16-home.bmp", path = asDirectory(home) }
  end

  for _, volume in ipairs(fs.volumes()) do
    placeList[#placeList + 1] = { text = volumeName(volume), icon = "16-drive.bmp", path = volume.path }
  end

  local items = {}

  for i, place in ipairs(placeList) do
    items[i] = { text = place.text, icon = place.icon }
  end

  places.items = items

  if current then
    selectPlace()
  end
end

-- Shows that folder (ending with "/"), the entry with that name selected. Back comes back to the
-- folder shown before, unless it is going back. False when the folder cannot be read.
local function navigate(path, selectName, goingBack)
  local list, why = load(path)

  if not list then
    showError("Open", why)
    return false
  end

  -- A name asked for in the folder left would go to this one.
  if naming then
    naming, renaming = nil, nil
    nameBar.visible = false
  end

  if current and current ~= path and not goingBack then
    history[#history + 1] = current
  end

  current = path
  entries = list
  pathBox.text = path

  render(selectName)
  selectPlace()
  showStatus()
  readSpace()
  return true
end

-- The entries' names and sizes: two listings that give the same have nothing new.
local function signature(list)
  local parts = {}

  for i, entry in ipairs(list) do
    parts[i] = entry.name .. "|" .. entry.size .. "|" .. tostring(entry.directory)
  end

  return table.concat(parts, "/")
end

-- Reads the folder again, the selection kept, or the one at that index. A folder that went away
-- gives its nearest parent that is still there.
local function reload(selectName, selectIndex)
  local list = load(current)

  while not list do
    current = parentOf(current) or COMPUTER
    list = load(current)

    if current == COMPUTER then
      list = list or {}
    end
  end

  if signature(list) == signature(entries) and pathBox.text == current and not selectName and not selectIndex then
    return
  end

  local selected = selectedEntry()

  entries = list
  pathBox.text = current
  render(selectName or (selected and selected.name), selectIndex)
  selectPlace()
  showStatus()
  readSpace()
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

local function open()
  local entry = selectedEntry()

  if not entry then
    return
  end

  if entry.directory then
    navigate(asDirectory(entry.path))
    return
  end

  local ok, why = desktop.open(entry.path)

  if not ok then
    showError("Open", why)
  end
end

local function up()
  local parent = parentOf(current)

  if parent then
    navigate(parent, nameOf(current))
  end
end

local function back()
  local previous = table.remove(history)

  if previous then
    -- The folder left was in it, maybe.
    navigate(previous, nameOf(current), true)
  end
end

-- Goes to the path typed: a folder, or the folder of a file, the file selected. A relative path
-- starts from the folder shown.
local function go()
  local text = pathBox.text:match("^%s*(.-)%s*$")

  if text == "" then
    pathBox.text = current
    return
  end

  if text:sub(1, 1) ~= "/" and not text:match("^%d+:") then
    text = current .. text
  end

  local path = fs.resolve(text)

  if fs.directoryExists(path) then
    navigate(asDirectory(path))
  elseif fs.fileExists(path) then
    navigate(parentOf(path), nameOf(path))
  else
    showError("Go to", "'" .. text .. "' does not exist.")
    return
  end

  files:focus()
end

-- The computer only lists the volumes: nothing is created, pasted or renamed there.
local function inFolder(action)
  if current == COMPUTER then
    setStatus("Open a volume or a folder to " .. action .. " there.", "red")
    return false
  end

  return true
end

-- A name that is not taken in the folder: "New folder", then "New folder (2)"...
local function freeName(stem, extension)
  local name = stem .. extension
  local number = 2

  while exists(current .. name) do
    name = stem .. " (" .. number .. ")" .. extension
    number = number + 1
  end

  return name
end

local function askName(kind, label, text)
  naming = kind
  nameLabel.text = label
  nameBox.text = text
  nameBar.visible = true
  nameBox:focus()
end

local function closeName()
  naming = nil
  renaming = nil
  nameBar.visible = false
  files:focus()
end

-- Why the name cannot be a file's, nil when it can.
local function checkName(name)
  if name == "" then
    return "Type a name."
  elseif name == "." or name == ".." then
    return "'" .. name .. "' is not a name."
  elseif name:find('[/\\:*?"<>|]') then
    return 'A name cannot have any of / \\ : * ? " < > |'
  end

  return nil
end

local function nameOk()
  if not naming then
    return
  end

  local name = nameBox.text:match("^%s*(.-)%s*$")
  local title = naming == "folder" and "New folder" or naming == "file" and "New file" or "Rename"
  local problem = checkName(name)

  if problem then
    showError(title, problem)
    return
  end

  local path = current .. name
  local ok, why

  if naming == "rename" then
    if name == renaming.name then
      closeName()
      return
    end

    -- A change of case only is the same name to FAT.
    if name:lower() ~= renaming.name:lower() and exists(path) then
      showError(title, "'" .. name .. "' already exists.")
      return
    end

    ok, why = fs.move(renaming.path, path)

    if ok and fs.clipboard == renaming.path then
      fs.clipboard = path
      cutPath = cutPath and path
    end
  else
    if exists(path) then
      showError(title, "'" .. name .. "' already exists.")
      return
    end

    if naming == "folder" then
      ok, why = fs.createDirectory(path)
    else
      ok, why = fs.writeText(path, "")
    end
  end

  if not ok then
    showError(title, why)
    return
  end

  closeName()
  reload(name)
end

local function newFolder()
  if inFolder("create a folder") then
    askName("folder", "New folder:", freeName("New folder", ""))
  end
end

local function newFile()
  if inFolder("create a file") then
    askName("file", "New file:", freeName("New file", ".txt"))
  end
end

local function rename()
  local entry = selectedEntry()

  if not inFolder("rename") then
    return
  elseif not entry then
    setStatus("Select what to rename first.", "red")
    return
  end

  renaming = entry
  askName("rename", "Rename to:", entry.name)
end

local function copy(cut)
  local entry = selectedEntry()

  if current == COMPUTER then
    setStatus("A volume cannot be " .. (cut and "cut." or "copied."), "red")
    return
  elseif not entry then
    setStatus("Select what to " .. (cut and "cut" or "copy") .. " first.", "red")
    return
  end

  fs.clipboard = entry.path
  cutPath = cut and entry.path or nil
  setStatus((cut and "Cut " or "Copied ") .. entry.name .. ": paste it in a folder.")
end

-- "Copy of notes.txt", then "Copy (2) of notes.txt"...
local function copyName(name)
  local candidate = "Copy of " .. name
  local number = 2

  while exists(current .. candidate) do
    candidate = "Copy (" .. number .. ") of " .. name
    number = number + 1
  end

  return candidate
end

local function paste()
  local source = fs.clipboard

  if not inFolder("paste") then
    return
  elseif not source then
    setStatus("Nothing to paste: copy or cut a file or a folder first.", "red")
    return
  elseif not exists(source) then
    fs.clipboard, cutPath = nil, nil
    showError("Paste", "'" .. source .. "' is not there anymore.")
    return
  end

  local moving = cutPath == source
  local name = nameOf(source)

  if moving and parentOf(source) == current then
    setStatus(name .. " is in this folder already.")
    return
  end

  local target = current .. name

  if exists(target) then
    if moving then
      showError("Paste", "'" .. name .. "' already exists in this folder.")
      return
    end

    target = current .. copyName(name)
  end

  run((moving and "Moving " or "Copying ") .. name .. "...", function()
    local ok, why

    if moving then
      ok, why = fs.move(source, target)
    else
      ok, why = fs.copy(source, target)
    end

    -- Either volume has less or more room now.
    spaces = {}

    if ok then
      if moving then
        fs.clipboard, cutPath = nil, nil
      end

      reload(nameOf(target))
      setStatus(name .. (moving and " moved." or " copied."), "green")
    else
      reload()
      showError("Paste", why)
    end
  end)
end

local function delete()
  local entry = selectedEntry()

  if current == COMPUTER then
    setStatus("A volume cannot be deleted.", "red")
    return
  elseif not entry then
    setStatus("Select what to delete first.", "red")
    return
  end

  deleting = entry
  confirm.message = entry.directory and "Delete the folder " .. entry.name .. " and everything in it?"
    or "Delete " .. entry.name .. "?"
  confirm.visible = true
end

local function confirmDelete()
  confirm.visible = false

  local entry = deleting
  deleting = nil

  if not entry then
    return
  end

  local index = files.selectedIndex + 1

  run("Deleting " .. entry.name .. "...", function()
    local ok, why = fs.delete(entry.path)

    if fs.clipboard == entry.path then
      fs.clipboard, cutPath = nil, nil
    end

    spaces[volumeOf(current)] = nil

    -- The next entry takes its place.
    reload(nil, index)

    if ok then
      setStatus(entry.name .. " deleted.", "green")
    else
      showError("Delete", why)
    end
  end)
end

local function refresh()
  spaces = {}
  fillPlaces()
  reload()

  -- reload keeps the status of an unchanged folder.
  showStatus()
  readSpace()
end

-- A Terminal in the folder shown.
local function terminal()
  local ok, why = pcall(function() fs.currentDirectory = current end)

  if ok then
    ok, why = desktop.start("Terminal")
  end

  if not ok then
    showError("Terminal", why)
  end
end

local function closeConfirm()
  confirm.visible = false
  deleting = nil
end

local function closeError()
  errorDialog.visible = false
end

app:on("back", back)
app:on("up", up)
app:on("go", go)
app:on("refresh", refresh)
app:on("terminal", terminal)
app:on("newFolder", newFolder)
app:on("newFile", newFile)
app:on("copy", function() copy(false) end)
app:on("cut", function() copy(true) end)
app:on("paste", paste)
app:on("rename", rename)
app:on("delete", delete)
app:on("nameOk", nameOk)
app:on("nameCancel", closeName)
app:on("confirmDelete", confirmDelete)
app:on("closeConfirm", closeConfirm)
app:on("closeError", closeError)
app:on("select", showStatus)
app:on("open", open)

app:on("place", function()
  local place = placeList[places.selectedIndex + 1]

  if place and place.path ~= current then
    navigate(place.path)
  end
end)

-- The keys the lists do not use (they move the selection, Enter opens), or all of them when no
-- control has the keys.
app:onKey(function(key)
  local name, char = key.name, key.char and key.char:lower()

  -- The dialogs take no keys: Enter and Escape answer them.
  if errorDialog.visible then
    if name == "enter" or name == "escape" then
      closeError()
    end
  elseif confirm.visible then
    if name == "enter" then
      confirmDelete()
    elseif name == "escape" then
      closeConfirm()
    end
  elseif name == "escape" and naming then
    closeName()
  elseif name == "enter" then
    open()
  elseif name == "backspace" or (name == "up" and key.alt) then
    up()
  elseif name == "left" and key.alt then
    back()
  elseif name == "delete" then
    delete()
  elseif name == "f2" then
    rename()
  elseif name == "f5" then
    refresh()
  elseif key.ctrl and char == "c" then
    copy(false)
  elseif key.ctrl and char == "x" then
    copy(true)
  elseif key.ctrl and char == "v" then
    paste()
  end
end)

-- The rows are as wide as the list.
app:onResize(function()
  local selected = selectedEntry()
  render(selected and selected.name)
end)

-- Something else (the Terminal, the Editor, the desktop) may have changed the folder: read it again
-- each time the window comes to the front.
local wasFocused = true

app:every(500, function()
  local focused = app.focused

  if focused and not wasFocused and not busy then
    fillPlaces()
    reload()
  end

  wasFocused = focused
end)

-- Where to open: the argument, else the user's folder, else the computer.
local function startPath(argument)
  if argument then
    local path = fs.resolve(argument)

    if fs.directoryExists(path) then
      return asDirectory(path)
    elseif fs.fileExists(path) then
      return parentOf(path), nameOf(path)
    end
  end

  local home = aura.user.directory

  if home and fs.directoryExists(home) then
    return asDirectory(home)
  end

  return COMPUTER
end

fillPlaces()

local path, selectName = startPath(...)

if not navigate(path, selectName) then
  navigate(COMPUTER)
end

files:focus()
