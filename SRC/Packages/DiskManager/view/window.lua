-- The disk shown: the disks read, the one picked as it will be once the pending operations are done,
-- its segments in the list and on the bar, the selection; and the volumes' free space, read one a frame.

local ui = require "ui"
local state = require "state"
local format = require "format"
local segments = require "model.segments"
local preview = require "model.preview"
local operations = require "model.operations"
local list = require "view.list"
local bar = require "view.bar"
local status = require "view.status"
local panel = require "view.panel"
local information = require "view.information"

local app = aura.app
local disks, fs = aura.disks, aura.fs

-- The disk's segments in the list and on the bar, the one at that index (from 1) selected.
local function render(index)
  list.render(index)
  bar.draw()
  status.show()
  status.showMountButton()
end

-- Reads the volumes' free space, one a frame once the window shows the disk: each read goes through
-- the volume's whole FAT.
local function readSpaces()
  if not state.disk then
    return
  end

  for _, segment in ipairs(state.segments) do
    local partition = segment.partition
    local mountPoint = partition and partition.mountPoint

    if mountPoint and state.spaces[mountPoint] == nil then
      app:after(0, function()
        if state.spaces[mountPoint] ~= nil then
          return
        end

        local free, total = fs.space(mountPoint)
        state.spaces[mountPoint] = free and { free = free, total = total } or false

        -- The status line keeps what it says: the result of an action, maybe.
        if not state.drag then
          list.render(ui.list.selectedIndex >= 0 and ui.list.selectedIndex + 1 or nil)
          bar.draw()
        end

        if state.panelAction == "information" then
          information.show()
        end

        -- One read a frame: the next one once this one shows.
        readSpaces()
      end)

      return
    end
  end
end

-- Shows that disk (an entry of state.diskList) as it will be, the segment at that sector selected.
local function showDisk(d, selectStart, selectKind)
  state.realDisk = d
  state.disk = d and preview.of(d, state.pending) or nil
  state.tree = state.disk and segments.build(state.disk) or {}
  state.segments = segments.flatten(state.tree)
  render(list.indexAt(selectStart, selectKind))
  readSpaces()
end

-- Shows the disk again after a change to the operations, the partition at that sector selected.
local function showChange(selectStart, selectKind)
  showDisk(state.realDisk, selectStart, selectKind)
  list.renderOperations()
end

-- Shows the disk again after a change to the operations, the segment where the selected one was
-- selected.
local function update()
  local segment = list.selected()
  showChange(segment and segment.start, segment and segment.kind)
end

-- The disk again once an operation joined the others, the partition at that sector selected.
local function queued(op, selectStart, selectKind)
  showChange(selectStart, selectKind or "partition")
  status.set(op.cancels and "Back where it was: the resize is no longer pending." or op.text .. ": pending, Apply does it.")
end

local function diskName(d)
  local tableType = (d.table == "MBR" or d.table == "GPT") and d.table or "no table"
  return d.name .. " (" .. format.size(d.size) .. ", " .. tableType .. ")"
end

-- Reads the disks again; shows the one with that name (else the first), the segment at that sector
-- selected.
local function reload(name, selectStart, selectKind)
  local ok, result = pcall(disks.list)
  state.diskList = ok and result or {}

  -- Each partition known by where it starts: the operations name them so.
  for _, d in ipairs(state.diskList) do
    for _, partition in ipairs(d.partitions) do
      partition.id = "p" .. partition.start
    end
  end

  local dropped = operations.check()
  local items, index = {}, 1

  for i, d in ipairs(state.diskList) do
    items[i] = diskName(d)

    if d.name == name then
      index = i
    end
  end

  if #items == 0 then
    items[1] = "No disk"
  end

  ui.diskBox.items = items
  ui.diskBox.selectedIndex = index - 1
  showDisk(state.diskList[index], selectStart, selectKind)
  list.renderOperations()

  if dropped > 0 then
    status.set("The disks changed: " .. format.plural(dropped, "pending operation") .. " no longer fit and went.", "red")
  end
end

-- Reads the disks again, the same disk and segment selected.
local function reloadShown()
  local segment = list.selected()
  reload(state.disk and state.disk.name, segment and segment.start, segment and segment.kind)
end

-- Reads the disks and the volumes' free space again.
local function refresh()
  state.spaces = {}
  reloadShown()
end

-- Another disk picked in the drop-down: shows it.
local function chooseDisk()
  local d = state.diskList[ui.diskBox.selectedIndex + 1]

  if d and d ~= state.realDisk and not state.applying then
    if state.panelAction then
      panel.close()
    end

    showDisk(d)
  end
end

local function selectionChanged()
  bar.draw()
  status.show()
  status.showMountButton()

  -- The panel tells about the selection, or asks about the one it was opened for.
  if panel.tells() then
    information.show()
  end
end

-- The selected segment, when it is a partition; else nil, and why in the status line.
local function selectedPartition(action)
  local segment = list.selected()

  if not state.disk then
    status.set("There is no disk.", "red")
  elseif not segment then
    status.set("Select a partition to " .. action .. " first.", "red")
  elseif segment.kind ~= "partition" then
    status.set("Select a partition to " .. action .. ": this is " .. segments.nameOf(segment) .. ".", "red")
  else
    return segment
  end

  return nil
end

return {
  showDisk = showDisk,
  update = update,
  queued = queued,
  reload = reload,
  reloadShown = reloadShown,
  refresh = refresh,
  chooseDisk = chooseDisk,
  selectionChanged = selectionChanged,
  selectedPartition = selectedPartition,
}
