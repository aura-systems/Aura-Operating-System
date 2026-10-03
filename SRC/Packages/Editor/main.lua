-- Editor: a text file in a TextBox. Its argument is the file's path (edit {file}, run Editor {file},
-- a file opened from the desktop); a file that does not exist yet opens empty and is created on save.

local app = aura.app
local content = app:find("content")
local dialog = app:find("dialog")

local path = ...

if path == "" then
  path = nil
end

if path then
  path = aura.fs.resolve(path)
  app.title = "Editor - " .. path

  if aura.fs.fileExists(path) then
    local text, why = aura.fs.readText(path)

    if not text then
      error("cannot open '" .. path .. "': " .. why, 0)
    end

    content.text = text
  end
end

local function showDialog(state, message)
  dialog.state = state
  dialog.message = message
  dialog.visible = true
end

app:on("save", function()
  if not path then
    showDialog("error", "This document has no file path, nothing was saved.")
    return
  end

  local saved, why = aura.fs.writeText(path, content.text)

  if saved then
    showDialog("information", "Your file has been saved!")
  else
    showDialog("error", "Save failed: " .. why)
  end
end)

app:on("closeDialog", function()
  dialog.visible = false
end)
