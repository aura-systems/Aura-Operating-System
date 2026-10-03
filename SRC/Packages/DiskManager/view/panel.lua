-- The panel on the right: a form, what an action asks (the rows of fields it needs, its text, OK and
-- Cancel), or what the panel tells (Information, an error). And the confirm dialog.

local ui = require "ui"
local state = require "state"
local format = require "format"
local filesystems = require "model.filesystems"
local status = require "view.status"

-- The panel's text, in characters a line.
local COLUMNS = 32

-- Shows the panel for an action: its title, the rows named (before, size, filesystem, label, table),
-- a text, and OK (named so; no OK for nil).
local function open(action, segment, title, shown, okText, text, color)
  state.panelAction = action
  state.panelSegment = segment
  ui.panelTitle.text = title
  ui.panelTitle.color = action == "error" and "red" or "black"
  ui.panelText.text = format.wrap(text or "", COLUMNS)
  ui.panelText.color = color or "black"
  ui.okButton.text = okText or "OK"
  ui.cancelButton.text = okText and "Cancel" or "Close"

  -- Showing a container shows everything in it: what the action does not use is hidden after.
  ui.panel.visible = true

  local any = false

  for name in pairs(ui.rows) do
    any = any or shown[name] or false
  end

  ui.fields.visible = any

  for name, row in pairs(ui.rows) do
    row.visible = shown[name] or false
  end

  ui.okButton.visible = okText ~= nil

  if shown.size then
    ui.sizeBox:focus()
  elseif shown.label then
    ui.labelBox:focus()
  end
end

local function close()
  state.panelAction = nil
  state.panelSegment = nil
  ui.panel.visible = false
  ui.list:focus()
end

-- Whether the panel tells something (Information, an error), rather than asking.
local function tells()
  return state.panelAction == "information" or state.panelAction == "error"
end

-- Cancel or Close: the status line is about the selection again.
local function cancel()
  close()
  status.show()
end

-- The text under the form.
local function tell(text)
  ui.panelText.text = format.wrap(text, COLUMNS)
  ui.panelText.color = "black"
end

-- A problem with what the form asks for: in red under its fields.
local function problem(text)
  ui.panelText.text = format.wrap(text, COLUMNS)
  ui.panelText.color = "red"
end

local function showError(title, message)
  message = message or "Unknown error."
  open("error", nil, title, {}, nil, message, "black")
  status.set(message, "red")
end

-- A whole number of MB in a text box of the form, nil when it is not one.
local function readMiB(box)
  local text = box.text:match("^%s*(%d+)%s*$")
  return text and tonumber(text)
end

-- The filesystem chosen in the form.
local function filesystem()
  return filesystems.NAMES[ui.filesystemBox.selectedIndex + 1] or "FAT32"
end

-- The confirm dialog: its message takes 2 lines of 31 characters. Its OK calls action.
local function ask(title, message, action)
  state.confirmed = action
  ui.confirm.title = title
  ui.confirm.message = message
  ui.confirm.visible = true
end

local function confirmOk()
  ui.confirm.visible = false
  local action = state.confirmed
  state.confirmed = nil

  if action then
    action()
  end
end

local function closeConfirm()
  ui.confirm.visible = false
  state.confirmed = nil
end

return {
  open = open,
  close = close,
  tells = tells,
  cancel = cancel,
  tell = tell,
  problem = problem,
  showError = showError,
  readMiB = readMiB,
  filesystem = filesystem,
  ask = ask,
  confirmOk = confirmOk,
  closeConfirm = closeConfirm,
}
