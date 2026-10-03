-- Mount and Unmount: done at once, not added to the operations, so refused while the disk has some.

local state = require "state"
local format = require "format"
local operations = require "model.operations"
local status = require "view.status"
local panel = require "view.panel"
local window = require "view.window"

local app = aura.app
local disks = aura.disks

-- Why mounting or unmounting cannot wait for the disk's operations, nil when there are none.
local function pendingProblem()
  local disk = state.disk
  local count = operations.pendingOn(disk.name)

  if count > 0 then
    return "Apply or undo the " .. format.plural(count, "operation") .. " pending on " .. disk.name .. " first."
  end

  return nil
end

-- Runs a job once the window shows the text in the status line.
local function run(text, job)
  if state.busy then
    return
  end

  state.busy = true
  status.set(text)

  app:after(0, function()
    state.busy = false
    job()
  end)
end

local function mount()
  local segment = window.selectedPartition("mount")

  if not segment then
    return
  end

  local partition = segment.partition
  local problem = pendingProblem()

  if problem then
    panel.showError("Mount " .. partition.name, problem)
    return
  end

  local name, start = state.disk.name, segment.start

  run("Mounting " .. partition.name .. "...", function()
    local ok, result = disks.mount(name, start)
    window.reload(name, start, "partition")

    if ok then
      status.set(partition.name .. " mounted at " .. result .. ".", "green")
    else
      panel.showError("Could not mount " .. partition.name, result)
    end
  end)
end

local function unmount()
  local segment = window.selectedPartition("unmount")

  if not segment then
    return
  end

  local partition = segment.partition

  if partition.system then
    panel.showError("Unmount " .. partition.name, "Aura runs from this partition (" .. partition.mountPoint .. "): it stays mounted.")
    return
  end

  local problem = pendingProblem()

  if problem then
    panel.showError("Unmount " .. partition.name, problem)
    return
  end

  local name, start = state.disk.name, segment.start

  run("Unmounting " .. partition.name .. "...", function()
    local ok, why = disks.unmount(name, start)
    state.spaces[partition.mountPoint] = nil
    window.reload(name, start, "partition")

    if ok then
      status.set(partition.name .. " unmounted: it can be unplugged.", "green")
    else
      panel.showError("Could not unmount " .. partition.name, why)
    end
  end)
end

return {
  mount = mount,
  unmount = unmount,
}
