-- The pending operations: Undo, Clear all, taking one out, picking one in their list; and Apply, which
-- does them, one a frame.

local ui = require "ui"
local state = require "state"
local format = require "format"
local operations = require "model.operations"
local list = require "view.list"
local status = require "view.status"
local panel = require "view.panel"
local window = require "view.window"

local app = aura.app

local function undo()
  local done, undone = operations.undo()

  if not done then
    status.set("There is nothing to undo.")
    return
  end

  window.update()
  status.set(undone and "Undone: " .. undone.text .. "." or "Undone.")
end

local function clear()
  local count = #state.pending

  if count == 0 then
    status.set("There is no operation to clear.")
    return
  end

  operations.clear()
  window.update()
  status.set(format.plural(count, "operation") .. " cleared: Undo brings them back.")
end

-- Takes the operation at that index (from 1) out of the list, when the ones after it do not need it.
local function remove(index)
  local removed = state.pending[index]
  local problem = operations.remove(index)

  if problem then
    status.set(problem, "red")
    return
  end

  window.update()
  status.set("Taken out: " .. removed.text .. ".")
end

-- An operation picked in their list: its partition, on its disk, is selected.
local function selectOperation()
  local op = state.pending[ui.operationList.selectedIndex + 1]

  if not op or state.applying then
    return
  end

  if not state.disk or op.disk ~= state.disk.name then
    for i, d in ipairs(state.diskList) do
      if d.name == op.disk then
        ui.diskBox.selectedIndex = i - 1
        window.showDisk(d)
      end
    end
  end

  local index = list.indexOfId(op.target or op.id)

  if index then
    ui.list.selectedIndex = index - 1
    window.selectionChanged()
  end

  status.set(op.text .. ". Delete takes it out of the operations.")
end

-- "The operation before it was applied." or "The 3 operations before it were applied."
local function operationsSentence(count, where, done)
  if count == 1 then
    return "The operation " .. where .. " was " .. done .. "."
  end

  return "The " .. count .. " operations " .. where .. " were " .. done .. "."
end

-- Applies the pending operations, one a frame: the status line and the list of operations show which.
-- The first that fails stops the others.
local function applyAll()
  local all = state.pending
  local total = #all
  local starts = operations.startsNow()
  local name = state.disk and state.disk.name

  state.applying = true

  if state.panelAction then
    panel.close()
  end

  local function finish()
    state.applying = false
    operations.reset()
    state.spaces = {}
    window.reload(name)
  end

  local function step(i)
    if i > total then
      finish()
      status.set(format.plural(total, "operation") .. " applied.", "green")
      return
    end

    local op = all[i]
    ui.operationList.selectedIndex = i - 1
    status.set("Applying " .. i .. " of " .. total .. ": " .. op.text .. "...")

    app:after(0, function()
      local ok, why = operations.execute(op, starts)

      if not ok then
        finish()
        panel.showError("Operation " .. i .. " of " .. total .. " failed", op.text .. ": " .. (why or "unknown error.")
          .. (i > 1 and " " .. operationsSentence(i - 1, "before it", "applied") or "")
          .. (i < total and " " .. operationsSentence(total - i, "after it", "not") or ""))
        return
      end

      step(i + 1)
    end)
  end

  step(1)
end

local function apply()
  local count = #state.pending

  if count == 0 then
    status.set("There is no operation to apply.")
    return
  end

  panel.ask("Apply", (count == 1 and "Apply the operation?" or "Apply the " .. count .. " operations?")
    .. " Keep the computer on until done.", applyAll)
end

return {
  undo = undo,
  clear = clear,
  remove = remove,
  select = selectOperation,
  apply = apply,
}
