-- The pending operations (state.pending): adding one, folded into the creation of the partition it
-- changes or into the resize just before it; taking the last change back, taking one out, clearing them;
-- and doing one on the disks.
--
-- An operation: { kind = , disk = (its name), text = (what the list of operations says) } and, by kind:
--   table   table = "MBR"|"GPT"
--   create  id = , name = , start = , sectors = , filesystem = , label = , logical = , sectorSize =
--   delete  target = (the id of the partition)
--   resize  target = , newStart = , newSectors = , moving = (whether it starts elsewhere)
--   format  target = , filesystem = , label =
--   label   target = , label =

local state = require "state"
local format = require "format"
local filesystems = require "model.filesystems"
local preview = require "model.preview"

local disks = aura.disks

-- The disk of that name as it is, nil when it went.
local function realOf(name)
  for _, d in ipairs(state.diskList) do
    if d.name == name then
      return d
    end
  end

  return nil
end

-- The number of operations pending on that disk.
local function pendingOn(name)
  local count = 0

  for _, op in ipairs(state.pending) do
    if op.disk == name then
      count = count + 1
    end
  end

  return count
end

-- Why Aura cannot change that partition now, nil when it can.
local function systemProblem(partition)
  if partition.system then
    return "Aura runs from this partition (" .. partition.mountPoint .. "): it cannot change while Aura uses it."
  end

  return nil
end

local function createText(op)
  local label = filesystems.labelAs(op.filesystem, op.label)

  return "Create " .. op.name .. ": " .. format.size(op.sectors * op.sectorSize) .. " " .. op.filesystem
    .. (label ~= "" and " '" .. label .. "'" or "")
    .. (op.logical and ", logical," or "") .. " on " .. op.disk
end

-- "Move new #1 2 MB to the right and resize it from 10 MB to 12 MB", for that partition of the disk d.
local function resizeText(d, partition, newStart, newSectors)
  local moved = newStart - partition.start
  local parts = {}

  if moved ~= 0 then
    parts[#parts + 1] = "Move " .. partition.name .. " " .. format.size(math.abs(moved) * d.sectorSize)
      .. (moved > 0 and " to the right" or " to the left")
  end

  if newSectors ~= partition.sectors then
    local sizes = "from " .. format.size(partition.sectors * d.sectorSize) .. " to " .. format.size(newSectors * d.sectorSize)
    parts[#parts + 1] = moved ~= 0 and "resize it " .. sizes or "Resize " .. partition.name .. " " .. sizes
  end

  return table.concat(parts, " and ")
end

-- The operation that creates that partition, and its index; nil when it is on the disk already.
local function creationOf(name, id)
  for i, op in ipairs(state.pending) do
    if op.kind == "create" and op.disk == name and op.id == id then
      return op, i
    end
  end

  return nil
end

-- Whether a change to that partition can go into the operation creating it: it is to be created, and
-- nothing after that changes it.
local function foldable(name, id)
  local _, index = creationOf(name, id)

  if not index then
    return false
  end

  local pending = state.pending

  for i = index + 1, #pending do
    if pending[i].disk == name and pending[i].target == id then
      return false
    end
  end

  return true
end

-- The pending operations with op folded into the creation of the partition it changes, nil when it
-- cannot be: deleting it takes the creation and its changes away; formatting, labeling or resizing it
-- changes how it is created.
local function fold(op)
  if not op.target or not foldable(op.disk, op.target) then
    return nil
  end

  local result = {}

  if op.kind == "delete" then
    for _, other in ipairs(state.pending) do
      if other.disk ~= op.disk or (other.id ~= op.target and other.target ~= op.target) then
        result[#result + 1] = other
      end
    end

    return result
  end

  local creation, index = creationOf(op.disk, op.target)
  local folded = preview.copyTable(creation)

  if op.kind == "format" then
    folded.filesystem, folded.label = op.filesystem, op.label
  elseif op.kind == "label" then
    folded.label = op.label
  elseif op.kind == "resize" then
    folded.start, folded.sectors = op.newStart, op.newSectors
  else
    return nil
  end

  folded.text = createText(folded)

  for i, other in ipairs(state.pending) do
    result[i] = i == index and folded or other
  end

  return result
end

-- A resize right after another of the same partition: the pending operations with both made one, from
-- where the partition was before the first (none when it goes back there: op.cancels is set). nil for
-- another operation.
local function combine(op)
  local pending = state.pending
  local last = pending[#pending]

  if op.kind ~= "resize" or not last or last.kind ~= "resize" or last.disk ~= op.disk or last.target ~= op.target then
    return nil
  end

  local result = {}

  for i = 1, #pending - 1 do
    result[i] = pending[i]
  end

  local d = realOf(op.disk)
  local before = preview.partitionById(preview.of(d, result), op.target)

  if before and (before.start ~= op.newStart or before.sectors ~= op.newSectors) then
    local combined = preview.copyTable(op)
    combined.moving = op.newStart ~= before.start
    combined.text = resizeText(d, before, op.newStart, op.newSectors)
    result[#result + 1] = combined
  else
    op.cancels = true
  end

  return result
end

-- Adds an operation after the pending ones, or folds it into the creation of the partition it changes,
-- or into the resize just before it: nil, or why it cannot be done once those are.
local function queue(op)
  local d = realOf(op.disk)
  local candidate = fold(op) or combine(op)

  if not candidate or select(2, preview.of(d, candidate)) then
    candidate = {}

    for i, other in ipairs(state.pending) do
      candidate[i] = other
    end

    candidate[#candidate + 1] = op

    local _, problem = preview.of(d, candidate)

    if problem then
      return problem
    end
  end

  state.history[#state.history + 1] = state.pending
  state.pending = candidate
  return nil
end

-- Takes the last change to the operations back: false when there is none; else true, and the operation
-- that went when one did.
local function undo()
  if #state.history == 0 then
    return false
  end

  local before = state.pending
  state.pending = table.remove(state.history)

  -- What went: an operation of the longer list missing from the other.
  local undone

  for _, op in ipairs(before) do
    local found = false

    for _, other in ipairs(state.pending) do
      found = found or other == op
    end

    undone = undone or (not found and op)
  end

  return true, undone
end

-- Takes the operation at that index out (and, for a creation, the changes to what it creates): nil, or
-- why it stays.
local function remove(index)
  local removed = state.pending[index]
  local candidate = {}

  for i, op in ipairs(state.pending) do
    local changesCreated = removed.kind == "create" and op.disk == removed.disk and op.target == removed.id

    if i ~= index and not changesCreated then
      candidate[#candidate + 1] = op
    end
  end

  local d = realOf(removed.disk)

  if d and select(2, preview.of(d, candidate)) then
    return "The operations after it need it: take them out first, or Undo."
  end

  state.history[#state.history + 1] = state.pending
  state.pending = candidate
  return nil
end

-- Takes every operation out: Undo brings them back.
local function clear()
  state.history[#state.history + 1] = state.pending
  state.pending = {}
end

-- No operation pending, nothing to undo, and the next partition created is "new #1" again.
local function reset()
  state.pending = {}
  state.history = {}
  state.nextNew = 1
end

-- Keeps the pending operations that still apply to the disks as they are now: a disk that went, or
-- changed under them (vol, another system), loses its own. How many went.
local function check()
  local kept, dropped, broken = {}, 0, {}

  for _, op in ipairs(state.pending) do
    if broken[op.disk] == nil then
      local d = realOf(op.disk)
      broken[op.disk] = not d or select(2, preview.of(d, state.pending)) ~= nil
    end

    if broken[op.disk] then
      dropped = dropped + 1
    else
      kept[#kept + 1] = op
    end
  end

  if dropped > 0 then
    state.pending = kept
    state.history = {}
  end

  return dropped
end

-- Where each partition on the disks starts now, by disk and id ("sata0:p2048"): what execute starts from.
local function startsNow()
  local result = {}

  for _, d in ipairs(state.diskList) do
    for _, partition in ipairs(d.partitions) do
      result[d.name .. ":" .. partition.id] = partition.start
    end
  end

  return result
end

-- Does one operation on the disks: true, else false and why. starts holds where each partition starts
-- now, as the operations before moved them: a logical partition may start elsewhere than the preview
-- said.
local function execute(op, starts)
  if op.kind == "table" then
    return disks.createTable(op.disk, op.table)
  elseif op.kind == "create" then
    local ok, result = disks.create(op.disk, op.start, op.sectors, op.filesystem, op.label)

    if ok then
      starts[op.disk .. ":" .. op.id] = result
    end

    return ok, result
  end

  local start = starts[op.disk .. ":" .. op.target]

  if not start then
    return false, "That partition is not on " .. op.disk .. " anymore."
  end

  if op.kind == "delete" then
    return disks.delete(op.disk, start)
  elseif op.kind == "resize" then
    local newStart = op.moving and op.newStart or start
    local ok, why = disks.resize(op.disk, start, newStart, op.newSectors)

    if ok then
      starts[op.disk .. ":" .. op.target] = newStart
    end

    return ok, why
  elseif op.kind == "format" then
    return disks.format(op.disk, start, op.filesystem, op.label)
  elseif op.kind == "label" then
    return disks.setLabel(op.disk, start, op.label)
  end

  return false, "Unknown operation."
end

return {
  pendingOn = pendingOn,
  systemProblem = systemProblem,
  createText = createText,
  resizeText = resizeText,
  foldable = foldable,
  queue = queue,
  undo = undo,
  remove = remove,
  clear = clear,
  reset = reset,
  check = check,
  startsNow = startsNow,
  execute = execute,
}
