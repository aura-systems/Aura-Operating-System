-- Disk Manager: the disks and their partitions, as GParted shows them. A bar draws the partitions in
-- their place on the disk, a list gives their filesystem, mount point, label, size and use. It creates
-- partition tables; creates, deletes, moves, resizes, formats and labels partitions; and mounts and
-- unmounts their volumes.
--
-- The changes wait in a list of operations: the bar and the list show the disk as it will be once they
-- are done, Undo takes the last change back, and Apply does them all, in order. Mounting and unmounting
-- are done at once.
--
-- The code, each module requiring only those listed before it:
--   state.lua    what the app keeps while it runs, in one table
--   ui.lua       the window's controls (DiskManager.xml)
--   format.lua   sizes and text
--   model/       the disks and the operations, without the window:
--                filesystems (what each takes and writes), segments (where things are on a disk),
--                preview (the disk once the operations are done), operations (the pending ones)
--   view/        what the window shows: list, bar, status (the status line), panel, information,
--                window (the disk shown and its selection)
--   actions/     what the buttons, keys and mouse do: partition (New, Delete, Format, Label),
--                resize (Resize/Move), drag (on the bar), volume (Mount, Unmount),
--                table (Partition table), pending (Undo, Clear all, Apply)
-- This file gives the layout's events and the keys to the actions, then shows the disks.

local app = aura.app

local ui = require "ui"
local state = require "state"
local list = require "view.list"
local bar = require "view.bar"
local panel = require "view.panel"
local information = require "view.information"
local window = require "view.window"
local partition = require "actions.partition"
local resize = require "actions.resize"
local drag = require "actions.drag"
local volume = require "actions.volume"
local partitionTable = require "actions.table"
local pending = require "actions.pending"

-- What OK does, by what the panel asks.
local OK = {
  new = partition.createOk,
  resize = resize.ok,
  format = partition.formatOk,
  label = partition.labelOk,
  table = partitionTable.ok,
}

local function ok()
  local action = OK[state.panelAction]

  if action then
    action()
  end
end

-- An action of a button: nothing while another waits for the window to be drawn, or while the
-- operations are applied.
local function guard(action)
  return function()
    if not state.busy and not state.applying then
      action()
    end
  end
end

app:on("new", guard(partition.new))
app:on("delete", guard(partition.delete))
app:on("resize", guard(resize.open))
app:on("format", guard(partition.format))
app:on("label", guard(partition.label))
app:on("undo", guard(pending.undo))
app:on("apply", guard(pending.apply))
app:on("clear", guard(pending.clear))
app:on("mount", guard(volume.mount))
app:on("unmount", guard(volume.unmount))
app:on("information", guard(information.show))
app:on("table", guard(partitionTable.open))
app:on("refresh", guard(window.refresh))
app:on("ok", guard(ok))
app:on("cancel", panel.cancel)
app:on("filesystem", partition.filesystemChanged)
app:on("confirmOk", panel.confirmOk)
app:on("closeConfirm", panel.closeConfirm)
app:on("disk", window.chooseDisk)
app:on("select", window.selectionChanged)
app:on("operation", pending.select)
app:on("barPress", drag.press)
app:on("barMove", drag.move)
app:on("barRelease", drag.release)

-- The keys the list and the text boxes do not use.
app:onKey(function(key)
  local name = key.name

  if ui.confirm.visible then
    if name == "enter" then
      panel.confirmOk()
    elseif name == "escape" then
      panel.closeConfirm()
    end
  elseif name == "escape" and state.panelAction then
    panel.cancel()
  elseif state.busy or state.applying then
    return
  elseif key.ctrl and key.char == "z" then
    pending.undo()
  elseif name == "enter" then
    information.show()
  elseif name == "delete" and ui.operationList.focused and ui.operationList.selectedIndex >= 0 then
    pending.remove(ui.operationList.selectedIndex + 1)
  elseif name == "delete" then
    partition.delete()
  elseif name == "insert" then
    partition.new()
  elseif name == "f2" then
    partition.label()
  elseif name == "f5" then
    window.refresh()
  end
end)

-- The bar is as wide as the window.
app:onResize(function()
  if not state.drag then
    bar.draw()
  end
end)

-- Something else (vol, a USB stick) may have changed the disks: read them again each time the window
-- comes to the front.
local wasFocused = true

app:every(500, function()
  local focused = app.focused

  if focused and not wasFocused and not state.busy and not state.applying and not state.drag and not ui.confirm.visible then
    window.reloadShown()
  end

  wasFocused = focused
end)

list.showHeader()
window.reload(...)
ui.list:focus()
