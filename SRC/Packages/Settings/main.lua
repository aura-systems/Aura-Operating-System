-- Settings: the user, theme, desktop and display settings, kept in settings.ini once Aura is installed.

local app = aura.app
local system, user, theme, desktop, display, fs = aura.system, aura.user, aura.theme, aura.desktop, aura.display, aura.fs

local username = app:find("username")
local computerName = app:find("computerName")
local themeBmpPath = app:find("themeBmpPath")
local themeXmlPath = app:find("themeXmlPath")
local windowsAlpha = app:find("windowsAlpha")
local taskbarAlpha = app:find("taskbarAlpha")
local resolution = app:find("resolution")
local scale = app:find("scale")
local wallpaperPath = app:find("wallpaperPath")
local guiDebug = app:find("guiDebug")
local autoLogin = app:find("autoLogin")
local dialog = app:find("dialog")

-- The theme files may be the kernel's own (live mode, missing install files).
local EMBEDDED = "embedded:"

local modes = display.modes()
local scales = display.scales()

-- Position of the first item that matches, nil when none does.
local function indexOf(list, matches)
  for i, item in ipairs(list) do
    if matches(item) then
      return i
    end
  end
end

local function findMode(width, height)
  return indexOf(modes, function(mode) return mode.width == width and mode.height == height end)
end

local function findScale(value)
  return indexOf(scales, function(item) return item == value end)
end

-- Selects the running resolution and scale (drop down indexes start at 0, -1 for none).
local function selectRunning()
  resolution.selectedIndex = (findMode(display.width, display.height) or 0) - 1
  scale.selectedIndex = (findScale(display.scale) or 0) - 1
end

-- The running mode is always selectable, even when the display does not list it.
if not findMode(display.width, display.height) then
  modes[#modes + 1] = { width = display.width, height = display.height }
end

local items = {}
for i, mode in ipairs(modes) do
  items[i] = mode.width .. "x" .. mode.height
end
resolution.items = items

items = {}
for i, value in ipairs(scales) do
  items[i] = value .. "%"
end
scale.items = items

username.text = user.name
computerName.text = system.computerName
themeBmpPath.text = theme.bmpPath or ""
themeXmlPath.text = theme.xmlPath or ""
windowsAlpha.value = desktop.windowsAlpha
taskbarAlpha.value = desktop.taskbarAlpha
selectRunning()
wallpaperPath.text = desktop.wallpaper or ""
guiDebug.checked = desktop.guiDebug

local oldWallpaperPath = wallpaperPath.text

-- Auto log in is kept in settings.ini, which a live system does not have.
if system.installed then
  autoLogin.checked = aura.settings.get("autologin") == "true"
else
  app:find("autoLoginRow").visible = false
end

local function isEmbedded(path)
  return path:sub(1, #EMBEDDED) == EMBEDDED
end

-- A typed path made absolute (gen2 paths such as 0:\Users accepted); the embedded theme files and an
-- empty field stay as they are.
local function normalize(path)
  if path == "" or isEmbedded(path) then
    return path
  end

  return fs.resolve(path)
end

local function isValidTheme(path)
  return isEmbedded(path) or fs.fileExists(path)
end

-- Switches to the selected resolution and scale now. Returns the error to show, or nil; after a
-- refusal the running ones are selected again.
local function applyResolution()
  local mode = modes[resolution.selectedIndex + 1] or { width = display.width, height = display.height }
  local newScale = scales[scale.selectedIndex + 1] or display.scale

  if mode.width == display.width and mode.height == display.height and newScale == display.scale then
    return nil
  end

  local changed, err = display.setMode(mode.width, mode.height, newScale)

  if not changed then
    selectRunning()
  end

  return err
end

-- What settings.ini keeps must be valid: the error to show, or nil.
local function checkPaths()
  if not isValidTheme(normalize(themeBmpPath.text)) then
    return "Theme .bmp path is not valid."
  elseif not isValidTheme(normalize(themeXmlPath.text)) then
    return "Theme .xml path is not valid."
  elseif not fs.fileExists(normalize(wallpaperPath.text)) then
    return "Wallpaper path is not valid."
  end
end

app:on("save", function()
  -- Reset a previous error.
  dialog.state = "information"
  dialog.message = "Settings updated."

  user.name = username.text
  system.computerName = computerName.text
  theme.bmpPath = normalize(themeBmpPath.text)
  theme.xmlPath = normalize(themeXmlPath.text)
  desktop.windowsAlpha = windowsAlpha.value
  desktop.taskbarAlpha = taskbarAlpha.value
  desktop.guiDebug = guiDebug.checked

  local wallpaper = normalize(wallpaperPath.text)
  local err

  if wallpaperPath.text ~= oldWallpaperPath then
    if not fs.fileExists(wallpaper) then
      err = "Wallpaper path is not valid."
    elseif pcall(desktop.setWallpaper, wallpaper) then
      oldWallpaperPath = wallpaperPath.text
    else
      -- GEN3-GAP(bmp): no top-down, bitfield or < 24 bpp BMPs
      err = "Wallpaper could not be loaded."
    end
  end

  err = err or applyResolution()

  if not err and system.installed then
    err = checkPaths()

    if not err then
      aura.settings.save({
        hostname = system.computerName,
        themeBmpPath = theme.bmpPath,
        themeXmlPath = theme.xmlPath,
        windowsTransparency = desktop.windowsAlpha,
        taskbarTransparency = desktop.taskbarAlpha,
        screenWidth = display.width,
        screenHeight = display.height,
        screenScale = display.scale,
        wallpaperPath = wallpaper,
        autologin = autoLogin.checked,
      })
    end
  end

  if err then
    dialog.state = "error"
    dialog.message = err
  end

  dialog.visible = true
end)

app:on("closeDialog", function()
  dialog.visible = false
end)
