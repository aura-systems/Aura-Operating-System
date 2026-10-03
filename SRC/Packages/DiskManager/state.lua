-- What the Disk Manager keeps while it runs, in one table the other modules read and change. Most of it
-- is replaced rather than changed (a disk, a list of operations), so read it through this table each
-- time: never keep it in a local.

local state = {
  -- aura.disks.list(): the disks as they are, each partition given an id (model/preview.lua); the one
  -- shown, as it is (realDisk) and as it will be once the pending operations are done (disk). nil when
  -- there is none.
  diskList = {},
  realDisk = nil,
  disk = nil,

  -- The operations to apply, in order (model/operations.lua). history holds each list before a change,
  -- for Undo. A list is never changed: a change makes a new one.
  pending = {},
  history = {},

  -- The number of the next partition an operation creates: "new #1".
  nextNew = 1,

  -- The disk's partitions, free spaces and extended partition (model/segments.lua): tree is the disk's
  -- top level, where the extended partition holds its own; segments has them all, as the list.
  tree = {},
  segments = {},

  -- The bar's boxes: { segment = , x = , y = , width = , height = , scale = (sectors a pixel) }, an
  -- extended partition's after it.
  boxes = {},

  -- The partition being dragged on the bar: { id = , name = , mode = "left"|"right"|"move", x = (where
  -- the press was), start = , sectors = , first = , finish = (where it can go), minimum = , maximum = ,
  -- scale = , newStart = , newSectors = }; nil when none is.
  drag = nil,

  -- Each mounted volume's { free = , total = } bytes by its mount point, read once: it goes through the
  -- whole FAT. False for one that could not be read.
  spaces = {},

  -- What the panel shows: "new", "resize", "format", "label", "table", "information" or "error", nil
  -- while hidden; and the segment (or disk) it is about.
  panelAction = nil,
  panelSegment = nil,

  -- What the confirm dialog asks about: the function its OK calls.
  confirmed = nil,

  -- An action waits for the window to be drawn: the buttons do nothing until it ran. applying: the
  -- operations are being applied, one a frame.
  busy = false,
  applying = false,
}

-- The { free = , total = } of a mounted partition's volume, nil until read (or for none).
function state.spaceOf(segment)
  local partition = segment.partition
  return partition and partition.mountPoint and state.spaces[partition.mountPoint] or nil
end

return state
