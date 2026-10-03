-- Partition table: a new MBR or GPT on the disk, without its partitions.

local ui = require "ui"
local state = require "state"
local segments = require "model.segments"
local operations = require "model.operations"
local status = require "view.status"
local panel = require "view.panel"
local window = require "view.window"

local function newTable()
  local disk = state.disk

  if not disk then
    status.set("There is no disk.", "red")
    return
  end

  for _, partition in ipairs(disk.partitions) do
    if partition.system then
      panel.showError("Partition table", "Aura runs from " .. partition.name .. " on " .. disk.name .. " (" .. partition.mountPoint .. "): its partition table cannot change while Aura uses it.")
      return
    end
  end

  -- MBR addresses 2 TB at most.
  ui.tableBox.selectedIndex = disk.size >= 2 * 1024 * 1024 * segments.MIB and 1 or 0

  panel.open("table", disk, "New partition table on " .. disk.name, { table = true }, "Add",
    "Every partition on " .. disk.name .. " and all their files will be lost. MBR holds 4 primary partitions of up to 2 TB, GPT 128 of any size.")
end

local function tableOk()
  local d = state.panelSegment
  local tableType = ui.tableBox.selectedIndex == 1 and "GPT" or "MBR"
  local op = { kind = "table", disk = d.name, table = tableType,
    text = "Write a new " .. tableType .. " on " .. d.name .. ", without its partitions" }

  local problem = operations.queue(op)

  if problem then
    panel.problem(problem)
    return
  end

  panel.close()
  window.queued(op)
end

return {
  open = newTable,
  ok = tableOk,
}
