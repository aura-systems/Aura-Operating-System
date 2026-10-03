-- MemoryInfo: memory, page and garbage collector figures, refreshed live.

local app = aura.app
local memory = aura.memory

-- Each refresh builds the value strings: not every frame, the app would grow the heap it measures.
local REFRESH_MS = 250

local labels = {}
for _, id in ipairs({ "memoryPool", "usedMemory", "freeMemory", "totalPages", "freePages", "liveHeap",
                      "gcHeapSize", "gcCommitted", "gcFragmented", "gcPinnedObjects", "collections",
                      "objectsFreed", "gcTime", "freeCount" }) do
  labels[id] = app:find(id)
end

-- The last collection's figures change only with a new collection, and reading them allocates.
local lastCollection = -1
local gc

local function megabytes(pages, pageSize)
  return ((pages * pageSize) >> 20) .. "MB"
end

local function refresh()
  local totalPages, freePages, pageSize = memory.totalPages, memory.freePages, memory.pageSize
  local collections = memory.collections

  if collections ~= lastCollection then
    gc = memory.lastCollection()
    lastCollection = collections
  end

  -- GEN3-GAP(meminfo): the page allocator's pool is the largest usable memory-map region only.
  labels.memoryPool.text = megabytes(totalPages, pageSize)
  labels.usedMemory.text = megabytes(totalPages - freePages, pageSize)
  labels.freeMemory.text = megabytes(freePages, pageSize)
  labels.totalPages.text = totalPages .. " (" .. pageSize .. "B)"
  labels.freePages.text = tostring(freePages)
  labels.liveHeap.text = memory.liveHeap .. "B"
  labels.gcHeapSize.text = gc.heapSize .. "B"
  labels.gcCommitted.text = gc.committed .. "B"
  labels.gcFragmented.text = gc.fragmented .. "B"
  labels.gcPinnedObjects.text = tostring(gc.pinnedObjects)
  labels.collections.text = tostring(collections)
  labels.objectsFreed.text = tostring(memory.objectsFreed)
  labels.gcTime.text = memory.gcTimePercent .. "%"
  labels.freeCount.text = tostring(memory.lastFreed)
end

refresh()
app:every(REFRESH_MS, refresh)
